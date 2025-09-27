using GameEngine.Events;
using GameEngine.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngine.GUI
{
	/// <summary>
	/// Defines common code to most GUI components.
	/// </summary>
	public abstract class GUIBase : DrawableAffineObject, IGUI
	{
		/// <summary>
		/// Initializes the base information of a GUI component.
		/// </summary>
		/// <param name="name">The name of this component.</param>
		/// <param name="tooltip">
		///	The tooltip for this GUI component (if any).
		///	This will take responsibility for initializing, updating, and drawing <paramref name="tooltip"/>, but it will take no responsibility for disposing of it.
		///	It will also force <paramref name="tooltip"/> to be initially invisible and will null its Parent.
		///	The GUICore will manage its visibility and posiiton thereafter.
		/// </param>
		protected GUIBase(string name, DrawableAffineObject? tooltip = null) : base()
		{
			Name = name;

			_o = null;
			_or = false;

			DrawOrder = 1; // This avoids dividing by 0 for LayerDepth; it will also set _ld
			
			// Give each event a no-op so that we don't ever have to think about them being null
			OnClick += (a,b) => {};
			OnRelease += (a,b) => {};
			OnClicked += (a,b) => {};
			OnHover += (a,b) => {};
			OnMove += (a,b) => {};
			OnExit += (a,b) => {};

			DrawOrderChanged += (a,b) =>
			{
				if(Tooltip is not null)
					Tooltip.LayerDepth = (this as IGUI).LayerDepth;

				return;
			};

			Tooltip = tooltip;
			return;
		}

		protected sealed override void LoadContent()
		{
			LoadAdditionalContent();

			if(Tooltip is not null)
			{
				Tooltip.Initialize();
				Tooltip.Visible = false;
				Tooltip.Parent = null;
				Tooltip.Renderer = Renderer;
				Tooltip.LayerDepth = GlobalConstants.TooltipDrawLayer;
			}

			return;
		}

		/// <summary>
		/// Loads any content for this GUI component.
		/// This is called before Tooltip is initialized/loaded.
		/// </summary>
		protected abstract void LoadAdditionalContent();

		public sealed override void Update(GameTime delta)
		{
			UpdateAddendum(delta);
			Tooltip?.Update(delta);

			return;
		}

		/// <summary>
		/// Peforms custom Update logic.
		/// This occurs before Tooltip is updated.
		/// </summary>
		/// <param name="delta">The elapsed time since the last Update call.</param>
		protected abstract void UpdateAddendum(GameTime delta);

		public sealed override void Draw(GameTime delta)
		{
			DrawAddendum(delta);

			if(Tooltip is not null && Tooltip.Visible)
				Tooltip.Draw(delta);

			return;
		}

		/// <summary>
		/// Peforms custom Draw logic.
		/// This occurs before Tooltip is drawn.
		/// </summary>
		/// <param name="delta">The elapsed time since the last Draw call.</param>
		protected abstract void DrawAddendum(GameTime delta);

		protected override void UnloadContent()
		{
			UnloadContentAddendum();
			
			Owner = null; // This will make sure we unsubscribe to any events and remove ourselves from our owner
			return;
		}

		/// <summary>
		/// Allows for additional resource unloading before the Owner is nulled.
		/// </summary>
		protected virtual void UnloadContentAddendum()
		{return;}

		public bool Contains(Point p, out IGUI? component, bool include_children = true) => Contains(new Vector2(p.X,p.Y),out component,include_children);
		public abstract bool Contains(Vector2 pos, out IGUI? component, bool include_children = true);

		public Vector2 GetAffinePosition() => World * Vector2.Zero;

		public virtual void PerformClick(MouseClickEventArgs args) => OnClick(this,args);
		public virtual void PerformRelease(MouseClickEventArgs args) => OnRelease(this,args);
		public virtual void PerformClicked(MouseClickedEventArgs args) => OnClicked(this,args);
		public virtual void PerformHover(MouseHoverEventArgs args) => OnHover(this,args);
		public virtual void PerformMove(MouseMoveEventArgs args) => OnMove(this,args);
		public virtual void PerformExit(MouseHoverEventArgs args) => OnExit(this,args);

		public override string ToString() => Name;

		public string Name
		{get;}

		public virtual GUICore? Owner
		{
			get => _o;

			set
			{
				// Never practice self-assignment
				if(ReferenceEquals(_o,value))
					return;

				// COUPLING ALERT
				// If we are removing ourselves from _o, then it may ask us to assign to null again, and we should do that so that our value is what it expects to be until later
				if(_or)
				{
					_o = null;
					return;
				}

				if(_o is not null)
				{
					// We need to avoid infinite removals, so we keep a note when we remove ourselves
					_or = true;
					
					// If we are clobbering the owner data, we always want to attempt to remove it from wherever it came from
					// If it succeeds, great; if it fails, no harm done
					_o.Remove(this);

					// Now we unwind the recursion
					_or = false;
				}
				
				// At this point, we can just assign our new owner
				_o = value;
				
				// We do not call Add on this component because we may not want it to be top-level
				// That call is left to the user to do if necessary
				if(_o is not null)
				{
					Game = _o.Game;
					_o.AddToMap(this);
				}

				return;
			}
		}

		private GUICore? _o;
		private bool _or;

		public DrawableAffineObject? Tooltip
		{get; protected set;}

		/// <inheritdoc/>
		/// <remarks>This cannot be set. It always references its <see cref="Owner"/>'s game.</remarks>
		public override RenderTargetFriendlyGame? Game
		{
			get => Owner?.Game;

			protected internal set
			{return;}
		}

		public override SpriteBatch? Renderer
		{
			set
			{
				if(ReferenceEquals(base.Renderer,value))
					return;

				base.Renderer = value;
				
				if(Tooltip is not null)
					Tooltip.Renderer = base.Renderer;

				return;
			}
		}

		public virtual bool SuspendsDigitalNavigation => false;

		/// <summary>
		/// The order that this should be drawn in a GUI system.
		/// <para/>
		/// Unlike with more general drawing, this directly corresponds to what layer a GUI element is drawn on (specifically on layer 1 / <see cref="DrawOrder"/>).
		/// The GUICore always renders BackToFront, so larger values of <see cref="DrawOrder"/> are drawn on top of smaller values of <see cref="DrawOrder"/>.
		/// <para/>
		/// This value is always positive.
		/// </summary>
		public override int DrawOrder
		{
			get => base.DrawOrder;

			set
			{
				if(base.DrawOrder == value || value < 1)
					return;
				
				base.LayerDepth = IGUI.DrawOrderToStandardDrawLayer(value); // Set layer depth first so that it's up to date before we notify observers of the draw order change
				base.DrawOrder = value;

				return;
			}
		}

		/// <summary>
		/// In GUI systems, this value is always equal to 1 / <see cref="DrawOrder"/>.
		/// See <see cref="DrawOrder"/> for more details.
		/// </summary>
		/// <remarks>Setting this does nothing.</remarks>
		public sealed override float LayerDepth
		{
			get => base.LayerDepth; // We keep this value so we don't need to divide a lot for no reason
			
			set
			{return;}
		}

		public event Click OnClick;
		public event Release OnRelease;
		public event Clicked OnClicked;
		public event Hovered OnHover;
		public event Moved OnMove;
		public event Exited OnExit;
	}
}
