using GameEngine.Assets.Sprites;
using GameEngine.Framework;
using GameEngine.GameObjects;
using GameEngine.GUI.Map;
using GameEngine.Input.Bindings.MouseBindings;
using GameEngine.Maths;
using GameEngine.Utility.ExtensionMethods.PrimitiveExtensions;
using Microsoft.Xna.Framework;

namespace GameEngine.GUI.Components
{
	/// <summary>
	/// A slider.
	/// <para/>
	/// Slider knobs are always kept within the boundary of the bars.
	/// To achieve an knob offset from the bar or knob spillover past the bars' ends, clever use of transparency of the knobs and bars respectively are suggested.
	/// </summary>
	public class Slider : GUIBase
	{
		/// <summary>
		/// Creates a new slider.
		/// </summary>
		/// <param name="name">The name of this slider.</param>
		/// <param name="bars">
		///	The backgrounds to display for the disabled, normal, hovered, and mid-click states.
		///	This is the slider bar, not the slider knob.
		///	The names required for each component are specified in this class's constants.
		///	<para/>
		///	These backgrounds can also be used to draw stylized text in stylized formats.
		///	<para/>
		///	This slider will be responsible for initializing <paramref name="bars"/> but will take no responsibility for disposing of it.
		///	This slider will also <u><b>force <paramref name="bars"/>' transform to be the identity</b></u> for efficiency's sake.
		/// </param>
		/// <param name="knobs">
		///	The knobs to display for the disabled, normal, hovered, and mid-click states.
		///	This is the slider knob, not the slider bar.
		///	The names required for each component are specified in this class's constants.
		///	<para/>
		///	These backgrounds can also be used to draw stylized text in stylized formats.
		///	<para/>
		///	This slider will be responsible for initializing <paramref name="bars"/> but will take no responsibility for disposing of it.
		///	This slider will also <u><b>force <paramref name="knobs"/>' transform to be the identity</b></u> for efficiency's sake.
		/// </param>
		/// <param name="min">
		///	The minimum discrete slider value.
		///	This occurs when the slider is at its leftmost position.
		///	If this is greater than <paramref name="max"/>, it will swap with <paramref name="max"/>.
		///	If this is equal to <paramref name="max"/>, then <paramref name="max"/> will be assigned to be one greater.
		/// </param>
		/// <param name="max">
		///	The maximum discrete slider value.
		///	This occurs when the slider is at its rightmost position.
		///	If this is less than <paramref name="min"/>, it will swap with <paramref name="min"/>.
		///	If this is equal to <paramref name="min"/>, then this will be assigned to be one greater.
		/// </param>
		/// <param name="tooltip">
		///	The tooltip for this GUI component (if any).
		///	This will take responsibility for initializing, updating, and drawing <paramref name="tooltip"/>, but it will take no responsibility for disposing of it.
		///	It will also force <paramref name="tooltip"/> to be initially invisible and will null its Parent.
		///	The GUICore will manage its visibility and posiiton thereafter.
		/// </param>
		public Slider(string name, GameObjectLibrary bars, GameObjectLibrary knobs, int min = 0, int max = 100, DrawableAffineObject? tooltip = null) : base(name,tooltip)
		{
			Bars = bars;
			Knobs = knobs;

			_sv = 0;
			_sMv = Math.Max(min,max);
			_smv = Math.Min(min,max);
			_ds = 1;

			if(SliderMinValue == SliderMaxValue)
				_sMv++;
			
			StateChanged += (a,b,c) => {};
			SliderValue = SliderMinValue;

			Clicking = false;
			StillClicking = false;
			DigitalPowerActivated = false;

			return;
		}

		protected override void LoadAdditionalContent()
		{
			Bars.Transform = Matrix2D.Identity;
			Bars.Parent = this;
			Bars.Renderer = Renderer;
			Bars.DrawOrder = DrawOrder;
			Bars.LayerDepth = LayerDepth;
			Bars.Initialize();
			
			// We want to leave the knobs' transformation alone so we can manipulate their position independently
			Knobs.Transform = Matrix2D.Identity;
			Knobs.Parent = Bars; // Move the bar, not the knob, if we ever have to (aside from aligning the knob to the bar)
			Knobs.Renderer = Renderer;
			Knobs.DrawOrder = DrawOrder + 1;
			Knobs.LayerDepth = IGUI.DrawOrderToStandardDrawLayer(DrawOrder + 1);
			Knobs.Initialize();
			
			// We default to the normal state if we're enabled and the disabled state if not
			if(Enabled)
			{
				Bars.ActiveGameObjectName = NormalState;
				Knobs.ActiveGameObjectName = NormalState;
			}
			else
			{
				Bars.ActiveGameObjectName = DisabledState;
				Knobs.ActiveGameObjectName = DisabledState;
			}

			// Keep the layer depth up to date
			DrawOrderChanged += (a,b) =>
			{
				Bars.DrawOrder = DrawOrder;
				Bars.LayerDepth = LayerDepth;
				
				Knobs.DrawOrder = DrawOrder + 1;
				Knobs.LayerDepth = IGUI.DrawOrderToStandardDrawLayer(DrawOrder + 1);

				return;
			};
			
			// We want to keep our knobs in the vertical center and in the correct horizontal location in case our size changes when we state change
			StateChanged += (a,b,c) =>
			{
				// Remove the knob's width from the available bar width, since we can't overflow at the right end of the bar
				Knobs.Transform = Matrix2D.Translation(SliderPosition * (Bars.Width - Knobs.Width),(Bars.Height - Knobs.Height) / 2.0f);
				return;
			};

			// The initial knob position assignment since we don't want to fire off a false state change to get it here
			Knobs.Transform = Matrix2D.Translation(0.0f,(Bars.Height - Knobs.Height) / 2.0f);

			// Initialize all of the events that we personally care about
			// The user may have additional interests which they can deal with on their own time
			// If they care about when the background changes, they can provide appropriate delegates to the backgrounds
			OnClick += (a,b) =>
			{
				if(b.Button == MouseButton.Left) // Left click only thanks
				{
					Bars.ActiveGameObjectName = ClickState;
					Knobs.ActiveGameObjectName = ClickState;

					// We only want to (and only meaningfully CAN) trigger additional click logic if we have a true blue mouse click
					if(b.IsMouse)
					{
						// Change the slider value to be whatever is closest to the mouse position
						// To do this, we need to calculate how far along the bar (whatever weird transform it has now) the mouse is
						// Translate half the knob back to center mouse zone
						AlignKnob(b.Position.ToVector2());
						Clicking = true;
					}
					else
						DigitalPowerActivated = !DigitalPowerActivated; // We toggle our ability to navigate GUI items digitally when we hit enter
				}
				
				return;
			};

			OnRelease += (a,b) =>
			{
				// We can't not be hovering when we click a button, so if we trigger the release, then we were hovering
				Bars.ActiveGameObjectName = HoverState;
				Knobs.ActiveGameObjectName = HoverState;

				Clicking = false;
				return;
			};

			OnHover += (a,b) =>
			{
				if(!StillClicking)
				{
					Bars.ActiveGameObjectName = HoverState;
					Knobs.ActiveGameObjectName = HoverState;
				}
				
				Clicking = StillClicking; // If we were still clicking when we enter, we should be clicking now
				StillClicking = false; // We are no longer still clicking

				return;
			};

			OnMove += (a,b) =>
			{
				if(Clicking)
				{
					if(b.IsMouse)
					{
						// Change the slider value to be whatever is closest to the mouse position
						// To do this, we need to calculate how far along the bar (whatever weird transform it has now) the mouse is
						// Translate half the knob back to center mouse zone
						AlignKnob(b.Position.ToVector2());
					}
				}
				else if(!b.IsMouse)
				{
					// We care about the slider's true orientation, so we should include its parents (and the camera if we have one)
					Vector2 right;
					
					if(Owner is null)
						right = World * Vector2.UnitX - World * Vector2.Zero;
					else
						right = Owner.World * World * Vector2.UnitX - Owner.World * World * Vector2.Zero;

					// We only allow digital navigation if we've hit enter or if there's no conflicting navigational commands
					bool no_left_right = Owner is null || !(Owner.HasConnection(this,GUIMapDirection.LEFT) || Owner.HasConnection(this,GUIMapDirection.RIGHT));
					bool no_up_down = Owner is null || !(Owner.HasConnection(this,GUIMapDirection.UP) || Owner.HasConnection(this,GUIMapDirection.DOWN));

					// Figure out which direction should make the slider increase in value
					// Also, remember that up is down and down is up in screen land
					if(right.X >= 0.0f)
						if(right.Y <= 0.0f)
							if(right.X >= -right.Y)
							{
								if(no_left_right || DigitalPowerActivated)
									SliderValue += b.DeltaX * DigitalSensitivity;
							}
							else
							{
								if(no_up_down || DigitalPowerActivated)
									SliderValue -= b.DeltaY * DigitalSensitivity;
							}
						else // Y > 0
							if(right.X >= right.Y)
							{
								if(no_left_right || DigitalPowerActivated)
									SliderValue += b.DeltaX * DigitalSensitivity;
							}
							else
							{
								if(no_up_down || DigitalPowerActivated)
									SliderValue -= b.DeltaY * DigitalSensitivity;
							}
					else // X < 0
						if(right.Y <= 0.0f)
							if(right.X <= right.Y)
							{
								if(no_left_right || DigitalPowerActivated)
									SliderValue += b.DeltaX * DigitalSensitivity;
							}
							else
							{
								if(no_up_down || DigitalPowerActivated)
									SliderValue -= b.DeltaY * DigitalSensitivity;
							}
						else // Y > 0
							if(-right.X >= right.Y)
							{
								if(no_left_right || DigitalPowerActivated)
									SliderValue += b.DeltaX * DigitalSensitivity;
							}
							else
							{
								if(no_up_down || DigitalPowerActivated)
									SliderValue -= b.DeltaY * DigitalSensitivity;
							}

					return;
				}

				return;
			};

			OnExit += (a,b) =>
			{
				StillClicking = Clicking; // If we exit while clicking, we want to keep draging the knob
				Clicking = false; // If we exit, we are no longer clicking

				if(!StillClicking)
				{
					Bars.ActiveGameObjectName = NormalState;
					Knobs.ActiveGameObjectName = NormalState;
				}

				return;
			};

			EnabledChanged += (a,b) =>
			{
				if(Enabled)
				{
					Bars.ActiveGameObjectName = NormalState; // If we're hovering, the next update cycle will catch it and hover, which is fine
					Knobs.ActiveGameObjectName = NormalState;
				}
				else
				{
					Bars.ActiveGameObjectName = DisabledState;
					Knobs.ActiveGameObjectName = DisabledState;
					
					// If we get disabled while clicking, stop
					Clicking = false;
					StillClicking = false;
				}

				return;
			};

			return;
		}

		/// <summary>
		/// Drops the unique line through <paramref name="pos"/> orthogonal to the bar and places the slider knob at that position (or caps at the end points).
		/// </summary>
		/// <param name="pos">The position to project onto the bar.</param>
		protected void AlignKnob(Vector2 pos)
		{
			// Move the alignment position to the center of the knob for smoother shifting
			pos -= Knobs.World * new Vector2(Knobs.Width / 2.0f,0.0f) - Knobs.World * Vector2.Zero;

			// We want to project onto the bar, so grab it's left and right positions
			Vector2 left = Bars.World * new Vector2(0,0);
			Vector2 right = Bars.World * new Vector2(Bars.Width,0);

			float len = (pos - left).ProjectionLength(right - left);
			float max = (right - left).Length() - Knobs.Width;

			SliderPosition = len / max;
			return;
		}

		protected override void UpdateAddendum(GameTime delta)
		{
			Bars.Update(delta);
			Knobs.Update(delta);

			// If we have an owner and it's active component is not this, then we are for sure not attempting to navigate it digitally
			if(Owner is not null && !ReferenceEquals(Owner.ActiveComponent,this))
				DigitalPowerActivated = false;

			if(StillClicking)
				if(Owner is null)
					StillClicking = false;
				else if(Owner.Input[GUICore.MouseClick].CurrentDigitalValue)
				{
					// Change the slider value to be whatever is closest to the mouse position (also translate it out of view coordinates)
					// To do this, we need to calculate how far along the bar (whatever weird transform it has now) the mouse is
					// Translate half the knob back to center mouse zone
					AlignKnob(Owner.World * new Vector2(Owner.Input[GUICore.MouseX].CurrentAnalogValue,Owner.Input[GUICore.MouseY].CurrentAnalogValue));
				}
				else
				{
					StillClicking = false;

					Bars.ActiveGameObjectName = NormalState;
					Knobs.ActiveGameObjectName = NormalState;
				}

			return;
		}

		protected override void DrawAddendum(GameTime delta)
		{
			Bars.Draw(delta);
			Knobs.Draw(delta);
			
			return;
		}

		public override void NotifyGameChange()
		{
			Bars.Game = Game;
			Knobs.Game = Game;

			base.NotifyGameChange();
			return;
		}

		/// <summary>
		/// Determines if this slider contains the position <paramref name="pos"/>.
		/// This is true if the knob <i>could</i> be at any position containing the mouse (with a fine enough granularity on its values).
		/// Whether the knob is actually there or not is another matter entirely.
		/// </summary>
		/// <param name="pos">The position (in world coordinates) to check for containment.</param>
		/// <param name="component">The GUI component where the <i>topmost</i> containment (a child could overlap its parent) occurred if there was one; null otherwise.</param>
		/// <param name="include_children">If true, we include (enabled and visible) children in the potential collision.</param>
		/// <returns>Returns true if this or a child contained <paramref name="pos"/> and false otherwise.</returns>
		public override bool Contains(Vector2 pos, out IGUI? component, bool include_children = true)
		{
			// Transform pos into the local coordinate system
			pos = InverseWorld * pos;

			// We need to know where the four corners end up as the most extreme points
			// Then we can pick the min and max values to get a bounding rectangle
			int W = Width; // Who knows how long these take to calculate
			int H = Height;

			// We need to adjust the height to account for the knob's offset (if it's bigger than the bar)
			int h = Bars.Height;
			float fh;

			if(h == H) // The knob is no taller than the bar
				fh = 0.0f;
			else // h < H (the knob is taller than the bar)
				fh = (H - h) / 2.0f;

			Vector2 TL = new Vector2(0,-fh); // Screen coorinates (and thus world coordinates) put the origin at the top left
			Vector2 TR = new Vector2(W,-fh);
			Vector2 BL = new Vector2(0,H - fh);
			Vector2 BR = new Vector2(W,H - fh);
			
			// To check that we're on the 'inside', we need only check that the cross product points up clockwise (screen coordinates are left-handed)
			if((TR - TL).Cross(pos - TL) >= 0.0f && (BR - TR).Cross(pos - TR) >= 0.0f && (BL - BR).Cross(pos - BR) >= 0.0f && (TL - BL).Cross(pos - BL) >= 0.0f)
			{
				component = this;
				return true;
			}
			
			component = null;
			return false;
		}

		public override Rectangle Bounds
		{
			get
			{
				// We're hacking together a bounding box, so we need to remember this for later
				Matrix2D temp = Knobs.Transform;

				// We need to know the bounds when the knobs are at the extrema
				Knobs.Transform = Matrix2D.Translation(0.0f,(Bars.Height - Knobs.Height) / 2.0f);
				Rectangle left = Knobs.Bounds;

				Knobs.Transform = Matrix2D.Translation(Bars.Width - Knobs.Width,(Bars.Height - Knobs.Height) / 2.0f);
				Rectangle right = Knobs.Bounds;

				// Reset the knobs transform
				Knobs.Transform = temp;

				// The knob and bar bounds are, on their own, correct since they are untransformed
				// However, their union can be incorrect when rotations get involved, since the knob can add spillover width/height when rotated at either extrema
				return Bars.Bounds.Union(left).Union(right);
			}
		}

		/// <summary>
		/// The position of the slider knob between [0,1].
		/// </summary>
		public float SliderPosition
		{
			get => (float)(SliderValue - SliderMinValue) / SliderRange;

			set
			{
				SliderValue = (int)MathF.Round(Math.Clamp(value,0.0f,1.0f) * SliderRange) + SliderMinValue;
				return;
			}
		}

		/// <summary>
		/// The discrete value of the slider.
		/// </summary>
		public int SliderValue
		{
			get => _sv;

			set
			{
				int nv = Math.Clamp(value,SliderMinValue,SliderMaxValue);

				if(nv == _sv)
					return;

				int old = _sv;
				_sv = nv;

				StateChanged(this,old,nv);
				return;
			}
		}

		protected int _sv;

		/// <summary>
		/// The minimum discrete value of the slider.
		/// <para/>
		/// If this value is assigned to something at least SliderMaxValue, then the maximum is set to be one higher.
		/// Similarly, if the SliderValue is less than the the assigned value, it is set to the new SliderMinValue.
		/// </summary>
		public int SliderMinValue
		{
			get => _smv;

			set
			{
				_smv = value;

				if(_smv >= SliderMaxValue)
					SliderMaxValue = _smv + 1;

				if(SliderValue < _smv)
					SliderValue = _smv;

				return;
			}
		}

		protected int _smv;

		/// <summary>
		/// The maximum discrete value of the slider.
		/// <para/>
		/// If this value is assigned to something at most SliderMinValue, then the minimum is set to be one lower.
		/// Similarly, if the SliderValue is greater than the the assigned value, it is set to the new SliderMaxValue.
		/// </summary>
		public int SliderMaxValue
		{
			get => _sMv;

			set
			{
				_sMv = value;

				if(_sMv <= SliderMinValue)
					SliderMinValue = _sMv - 1;

				if(SliderValue > _sMv)
					SliderValue = _sMv;

				return;
			}
		}

		protected int _sMv;

		/// <summary>
		/// The range of the slider's discrete values (i.e. SliderMaxValue - SliderMinValue).
		/// </summary>
		public int SliderRange
		{get => SliderMaxValue - SliderMinValue;}

		/// <summary>
		/// This is the number of units a digital input moves the slider (if applicable).
		/// This value is always positive.
		/// If a nonpositive value is assigned to this, it defaults to one.
		/// </summary>
		public int DigitalSensitivity
		{
			get => _ds;

			set
			{
				if(value < 1)
					_ds = 1;
				else
					_ds = value;

				return;
			}
		}

		protected int _ds;

		/// <summary>
		/// The component library to use for drawing this slider's knobs.
		/// </summary>
		protected GameObjectLibrary Knobs
		{get; set;}

		/// <summary>
		/// The component library to use for drawing this slider's bars.
		/// </summary>
		protected GameObjectLibrary Bars
		{get; set;}

		public override SpriteRenderer? Renderer
		{
			set
			{
				if(ReferenceEquals(base.Renderer,value))
					return;

				base.Renderer = value;
				
				Knobs.Renderer = value;
				Bars.Renderer = value;
				
				return;
			}
		}

		/// <summary>
		/// The width of the slider (sans transformations).
		/// </summary>
		public override int Width => Bars.Width; // We force knobs/bars to be untransformed and keep the knobs in bounds; spillover achieved via bar transparency is ignored
		
		/// <summary>
		/// The height of the slider (sans transformations).
		/// </summary>
		public override int Height => Math.Max(Bars.Height,Knobs.Height); // We go with the taller of the knobs and bars; they are untransformed, so the raw Height is correct

		/// <summary>
		/// An event called when the state of the slider's value changes.
		/// </summary>
		public event SliderStateChanged StateChanged;

		/// <summary>
		/// This is true if the mouse has clicked on this slider but not release or otherwise left yet.
		/// This is independent of if a mouse happens to wander in with its click already down.
		/// </summary>
		protected bool Clicking
		{get; set;}

		/// <summary>
		/// If true, then the mouse button was still held when we exited the slider, and we want to keep aligning the knob.
		/// If false, then we don't need to do anything after exiting.
		/// </summary>
		protected bool StillClicking
		{get; set;}

		public override bool SuspendsDigitalNavigation => DigitalPowerActivated;

		/// <summary>
		/// If true, then we are attempting to navigate this slider with digital navigation.
		/// If false, then we are not.
		/// </summary>
		protected bool DigitalPowerActivated;

		/// <summary>
		/// The name of the component to use for drawing a slider's normal state.
		/// </summary>
		public const string NormalState = "n";

		/// <summary>
		/// The name of the component to use for drawing a slider's hover state.
		/// </summary>
		public const string HoverState = "h";

		/// <summary>
		/// The name of the component to use for drawing a slider's mid-click state.
		/// </summary>
		public const string ClickState = "c";

		/// <summary>
		/// The name of the component to use for drawing a slider's disabled state.
		/// </summary>
		public const string DisabledState = "d";
	}

	/// <summary>
	/// An event called when the state of a slider changes.
	/// </summary>
	/// <param name="sender">The slider sending the event.</param>
	/// <param name="old_value">The old slider value.</param>
	/// <param name="new_value">The new slider value.</param>
	public delegate void SliderStateChanged(Slider sender, int old_value, int new_value);
}
