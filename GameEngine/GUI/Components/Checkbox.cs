using GameEngine.Framework;
using GameEngine.GameComponents;
using GameEngine.Input.Bindings.MouseBindings;
using GameEngine.Maths;
using GameEngine.Utility.ExtensionMethods.PrimitiveExtensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngine.GUI.Components
{
	/// <summary>
	/// A checkbox.
	/// </summary>
	public class Checkbox : GUIBase
	{
		/// <summary>
		/// Creates a new checkbox.
		/// </summary>
		/// <param name="game">The game this button checkbox to.</param>
		/// <param name="name">The name of this checkbox.</param>
		/// <param name="backgrounds">
		///	The backgrounds to display for the disabled, normal, hovered, and mid-click states.
		///	Each state has a checked and unchecked variation.
		///	The names required for each component are specified in this class's constants.
		///	<para/>
		///	This checkbox will be responsible for initializing <paramref name="backgrounds"/> but will take no responsibility for disposing of it.
		/// </param>
		/// <param name="text">
		///	Text to display over the center of the button.
		///	If no text is required, simply provide null and it will be ignored.
		///	<para/>
		///	This button will be responsible for initializing <paramref name="text"/> but will take no responsibility for disposing of it.
		/// </param>
		/// <param name="left">If true, then the text (if any) is aligned left of the checkbox. If false, then it is aligned right of the checkbox instead.</param>
		/// <param name="margin">The initial (scaling) margin added between the text and the checkbox.</param>
		/// <param name="check">The initial state of the checkbox. If true, then it is checked. If false, then it is unchecked.</param>
		/// <param name="tooltip">
		///	The tooltip for this GUI component (if any).
		///	This will take responsibility for initializing, updating, and drawing <paramref name="tooltip"/>, but it will take no responsibility for disposing of it.
		///	It will also force <paramref name="tooltip"/> to be initially invisible and will null its Parent.
		///	The GUICore will manage its visibility and posiiton thereafter.
		/// </param>
		public Checkbox(RenderTargetFriendlyGame game, string name, ComponentLibrary backgrounds, TextComponent? text = null, bool left = true, float margin = 5.0f, bool check = false, DrawableAffineComponent? tooltip = null) : base(game,name,tooltip)
		{
			Library = backgrounds;
			Text = text;

			StateChanged += (a,b) => {};
			_c = check;

			_lt = left;
			_m = margin;

			return;
		}

		protected override void LoadAdditionalContent()
		{
			Library.Initialize();
			Library.Parent = this;
			Library.Renderer = Renderer;
			Library.LayerDepth = (this as IGUI).LayerDepth;

			// We default to the normal state if we're enabled and the disabled state if not
			if(Enabled)
				Library.ActiveComponentName = Checked ? CheckedNormalState : UncheckedNormalState;
			else
				Library.ActiveComponentName = Checked ? CheckedDisabledState : UncheckedDisabledState;

			// Keep the layer depth up to date
			DrawOrderChanged += (a,b) => Library.LayerDepth = (this as IGUI).LayerDepth;

			if(Text is not null)
			{
				Text.Initialize();
				Text.Parent = Library; // Transform the button (in this class if necessary), not the text (except to align it)
				Text.Renderer = Renderer;
				Text.LayerDepth = (this as IGUI).LayerDepth;

				// Keep the layer depth up to date
				DrawOrderChanged += (a,b) => Text.LayerDepth = (this as IGUI).LayerDepth;

				// Initial text alignment
				AlignText();

				// Subscribe to the text and font change events in case we need to reposition (while trying to preserve other changes made)
				Text.TextChanged += (useless,ntext,otext) => AlignText();
				Text.FontChanged += (useless,nfont,ofont) => AlignText();

				// We'll want to keep the text in the center in case our size changes when we state change, and if that clobbers transformation data, that's not our problem
				Library.StateChange += (useless,active,old) => AlignText();
			}

			// Initialize all of the events that we personally care about
			// The user may have additional interests which they can deal with on their own time
			// If they care about when the background changes, they can provide appropriate delegates to the backgrounds
			OnClick += (a,b) =>
			{
				if(b.Button == MouseButton.Left) // Left click only thanks
					Library.ActiveComponentName = Checked ? CheckedClickState : UncheckedClickState;

				return;
			};

			OnRelease += (a,b) => Library.ActiveComponentName = Checked ? CheckedHoverState : UncheckedHoverState; // We can't not be hovering when we click a checkbox
			OnHover += (a,b) => Library.ActiveComponentName = Checked ? CheckedHoverState : UncheckedHoverState;
			OnExit += (a,b) => Library.ActiveComponentName = Checked ? CheckedNormalState : UncheckedNormalState;
			OnClicked += (a,b) => Checked = !Checked;
			StateChanged += (a,b) => Library.ActiveComponentName = string.Concat(b ? "c" : "u",Library.ActiveComponentName.AsSpan(1));

			EnabledChanged += (a,b) =>
			{
				if(Enabled) // If we're hovering, the next update cycle will catch it and hover, which is fine
					Library.ActiveComponentName = Checked ? CheckedNormalState : UncheckedNormalState;
				else
					Library.ActiveComponentName = Checked ? CheckedDisabledState : UncheckedDisabledState;

				return;
			};

			return;
		}

		/// <summary>
		/// Aligns the text component of this button to the center along both axes.
		/// </summary>
		protected void AlignText()
		{
			if(Text is not null)
				if(LeftAlignText)
					Text.Transform = Matrix2D.Translation(-(Text.Width + Margin),(Library.Height - Text.Height) / 2.0f);
				else
					Text.Transform = Matrix2D.Translation(Margin + Library.Width,(Library.Height - Text.Height) / 2.0f);
			
			return;
		}

		protected override void UpdateAddendum(GameTime delta)
		{
			Library.Update(delta);

			if(Text is not null)
				Text.Update(delta);

			return;
		}

		protected override void DrawAddendum(GameTime delta)
		{
			Library.Draw(delta);

			if(Text is not null)
				Text.Draw(delta);

			return;
		}

		public override bool Contains(Vector2 pos, out IGUI? component, bool include_children = true)
		{
			// Transform pos into the local coordinate system
			pos = World.Invert() * pos;

			// We need to calculate containment within an arbitrarily rotated, translated, and scaled rectangle
			// The obvious choice for doing this is to just make sure we're all on the 'inside' of the lines defined by the four corners of the untransformed boundary
			int W = Library.Width; // Who knows how long these take to calculate
			int H = Library.Height;

			Vector2 TL = new Vector2(0,0); // Screen coorinates (and thus world coordinates) put the origin at the top left
			Vector2 TR = new Vector2(W,0);
			Vector2 BL = new Vector2(0,H);
			Vector2 BR = new Vector2(W,H);

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
				// We need to know where the four corners end up as the most extreme points
				// Then we can pick the min and max values to get a bounding rectangle
				int W = Width; // Who knows how long these take to calculate
				int H = Height;

				float shift_left = Text is not null && LeftAlignText ? -(Text.Width + Margin) : 0.0f;
				float shift_vertical = Text is not null ? Math.Max((Text.Height - Library.Height) / 2.0f,0.0f) : 0.0f;

				Vector2 TL = World * new Vector2(shift_left,-shift_vertical); // Screen coorinates (and thus world coordinates) put the origin at the top left
				Vector2 TR = World * new Vector2(shift_left + W,-shift_vertical);
				Vector2 BL = World * new Vector2(shift_left,H + shift_vertical);
				Vector2 BR = World * new Vector2(shift_left + W,H + shift_vertical);

				float left = MathF.Min(MathF.Min(TL.X,TR.X),MathF.Min(BL.X,BR.X));
				float right = MathF.Max(MathF.Max(TL.X,TR.X),MathF.Max(BL.X,BR.X));
				float top = MathF.Min(MathF.Min(TL.Y,TR.Y),MathF.Min(BL.Y,BR.Y));
				float bottom = MathF.Max(MathF.Max(TL.Y,TR.Y),MathF.Max(BL.Y,BR.Y));

				return new Rectangle((int)left,(int)top,(int)MathF.Ceiling(right - left),(int)MathF.Ceiling(bottom - top));
			}
		}

		/// <summary>
		/// If true, then this check box is checked.
		/// If false, then this check box is unchecked.
		/// </summary>
		public bool Checked
		{
			get => _c;

			set
			{
				if(_c == value)
					return;

				_c = value;
				StateChanged(this,_c);

				return;
			}
		}

		private bool _c;

		/// <summary>
		/// If true, then this check box is unchecked.
		/// If false, then this check box is checked.
		/// </summary>
		public bool Unchecked
		{
			get => !Checked;
			set => Checked = !value;
		}


		/// <summary>
		/// An event called when the state of checkbox changes.
		/// </summary>
		public event CheckStateChanged StateChanged;

		/// <summary>
		/// The text to write on this button (if any).
		/// </summary>
		public TextComponent? Text
		{get; protected set;}

		/// <summary>
		/// If true, then the checkbox text (if any) is aligned left of the checkbox.
		/// If false, then the checkbox text (if any) is aligned right of the checkbox.
		/// <para/>
		/// If the checkbox has no text, this value has no meaning.
		/// </summary>
		public bool LeftAlignText
		{
			get => _lt;

			set
			{
				if(_lt == value)
					return;

				_lt = value;
				AlignText();

				return;
			}
		}

		protected bool _lt;

		/// <summary>
		/// If true, then the checkbox text (if any) is aligned right of the checkbox.
		/// If false, then the checkbox text (if any) is aligned left of the checkbox.
		/// <para/>
		/// If the checkbox has no text, this value has no meaning.
		/// </summary>
		public bool RightAlignText
		{
			get => !_lt;
			set => LeftAlignText = !value;
		}

		/// <summary>
		/// The added margin between the text and the checkbox.
		/// <para/>
		/// The margin can be any real value, including negative values.
		/// <para/>
		/// The margin is not fixed.
		/// It scales with scaling transformations on any of the checkbox's component parts.
		/// </summary>
		public float Margin
		{
			get => _m;

			set
			{
				_m = value;
				AlignText();

				return;
			}
		}

		protected float _m;

		/// <summary>
		/// The component library to use for drawing this checkbox.
		/// </summary>
		protected ComponentLibrary Library
		{get; set;}

		public override SpriteBatch? Renderer
		{
			protected get => base.Renderer;

			set
			{
				if(ReferenceEquals(base.Renderer,value))
					return;

				base.Renderer = value;
				Library.Renderer = value;

				if(Text is not null)
					Text.Renderer  = value;
				
				return;
			}
		}

		/// <summary>
		/// The width of the checkbox (sans transformations).
		/// </summary>
		public override int Width => Library.Width + (int)MathF.Ceiling(Margin) + (Text is null ? 0 : Text.Width);

		/// <summary>
		/// The height of the checkbox (sans transformations).
		/// </summary>
		public override int Height => Math.Max(Library.Height,Text is null ? 0 : Text.Height);

		/// <summary>
		/// The name of the component to use for drawing a checkbox's unchecked normal state.
		/// </summary>
		public const string UncheckedNormalState = "un";

		/// <summary>
		/// The name of the component to use for drawing a checkbox's checked normal state.
		/// </summary>
		public const string CheckedNormalState = "cn";

		/// <summary>
		/// The name of the component to use for drawing a checkbox's unchecked hover state.
		/// </summary>
		public const string UncheckedHoverState = "uh";

		/// <summary>
		/// The name of the component to use for drawing a checkbox's checked hover state.
		/// </summary>
		public const string CheckedHoverState = "ch";

		/// <summary>
		/// The name of the component to use for drawing a checkbox's unchecked mid-click state.
		/// </summary>
		public const string UncheckedClickState = "uc";

		/// <summary>
		/// The name of the component to use for drawing a checkbox's checked mid-click state.
		/// </summary>
		public const string CheckedClickState = "cc";

		/// <summary>
		/// The name of the component to use for drawing a checkbox's unchecked disabled state.
		/// </summary>
		public const string UncheckedDisabledState = "ud";

		/// <summary>
		/// The name of the component to use for drawing a checkbox's checked disabled state.
		/// </summary>
		public const string CheckedDisabledState = "cd";
	}

	/// <summary>
	/// An event called when the state of a checkbox changes.
	/// </summary>
	/// <param name="sender">The checkbox that changed state.</param>
	/// <param name="state">The new state of the checkbox. This is true if it is checked and false if it is unchecked.</param>
	public delegate void CheckStateChanged(Checkbox sender, bool state);
}
