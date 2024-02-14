using GameEngine.Events;
using GameEngine.Framework;
using GameEngine.GameComponents;
using GameEngine.Maths;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngine.GUI
{
	/// <summary>
	/// Defines common code to most GUI components.
	/// </summary>
	public abstract class GUIBase : IGUI
	{
		/// <summary>
		/// Initializes the base information of a GUI component.
		/// </summary>
		/// <param name="game">The game this component will belong to.</param>
		/// <param name="name">The name of this component.</param>
		/// <param name="tooltip">
		///	The tooltip for this GUI component (if any).
		///	This will take responsibility for initializing, updating, and drawing <paramref name="tooltip"/>, but it will take no responsibility for disposing of it.
		///	It will also force <paramref name="tooltip"/> to be initially invisible and will null its Parent.
		///	The GUICore will manage its visibility and posiiton thereafter.
		/// </param>
		protected GUIBase(RenderTargetFriendlyGame game, string name, DrawableAffineComponent? tooltip = null)
		{
			WorldRevision = 2;
			InverseWorldRevision = 2;

			Game = game;
			Name = name;

			Tint = Color.White;
			_r = null;

			Transform = Matrix2D.Identity;
			Parent = null;

			_o = null;
			_or = false;

			_uo = 0;
			_do = 1; // This avoids dividing by 0 for LayerDepth
			
			_e = true;
			_v = true;

			Initialized = false;
			Disposed = false;

			// Give each event a no-op so that we don't ever have to think about them being null
			OnClick += (a,b) => {};
			OnRelease += (a,b) => {};
			OnClicked += (a,b) => {};
			OnHover += (a,b) => {};
			OnMove += (a,b) => {};
			OnExit += (a,b) => {};

			EnabledChanged += (a,b) => {};
			VisibleChanged += (a,b) => {};
			UpdateOrderChanged += (a,b) => {};
			DrawOrderChanged += (a,b) =>
			{
				if(Tooltip is not null)
					Tooltip.LayerDepth = (this as IGUI).LayerDepth;

				return;
			};

			Tooltip = tooltip;
			return;
		}

		/// <summary>
		/// The finalizer.
		/// </summary>
		~GUIBase()
		{
			Dispose(false);
			return;
		}

		public void Initialize()
		{
			if(!Initialized)
			{
				Initialized = true;
				LoadContent();
			}

			return;
		}

		public void LoadContent()
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
		/// Loads any additional content before LoadContent starts.
		/// </summary>
		protected abstract void LoadAdditionalContent();

		public void Update(GameTime delta)
		{
			UpdateAddendum(delta);

			if(Tooltip is not null)
				Tooltip.Update(delta);

			return;
		}

		/// <summary>
		/// Peforms additional update logic before Update starts.
		/// </summary>
		/// <param name="delta">The elapsed time since the last Update call.</param>
		protected abstract void UpdateAddendum(GameTime delta);

		public void Draw(GameTime delta)
		{
			DrawAddendum(delta);

			if(Tooltip is not null && Tooltip.Visible)
				Tooltip.Draw(delta);

			return;
		}

		/// <summary>
		/// Peforms additional update logic before Draw starts.
		/// </summary>
		/// <param name="delta">The elapsed time since the last Draw call.</param>
		protected abstract void DrawAddendum(GameTime delta);

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);

			return;
		}

		/// <summary>
		/// Disposes of any resources used by this component not otherwise managed elsewhere.
		/// </summary>
		/// <param name="disposing">If true, then this is being called from Dispose. If false, this is being called from the finalizer.</param>
		protected void Dispose(bool disposing)
		{
			if(!Disposed)
			{
				Disposed = true;
				UnloadContent();
			}

			return;
		}

		/// <summary>
		/// Unloads any resources used by this component not otherwise managed elsewhere.
		/// </summary>
		public void UnloadContent()
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

		public bool Contains(Point p, out IGUI? component, bool include_children = true)
		{return Contains(new Vector2(p.X,p.Y),out component,include_children);}

		public abstract bool Contains(Vector2 pos, out IGUI? component, bool include_children = true);

		public virtual void PerformClick(MouseClickEventArgs args)
		{
			OnClick(this,args);
			return;
		}

		public virtual void PerformRelease(MouseClickEventArgs args)
		{
			OnRelease(this,args);
			return;
		}

		public virtual void PerformClicked(MouseClickedEventArgs args)
		{
			OnClicked(this,args);
			return;
		}

		public virtual void PerformHover(MouseHoverEventArgs args)
		{
			OnHover(this,args);
			return;
		}

		public virtual void PerformMove(MouseMoveEventArgs args)
		{
			OnMove(this,args);
			return;
		}

		public virtual void PerformExit(MouseHoverEventArgs args)
		{
			OnExit(this,args);
			return;
		}

		public RenderTargetFriendlyGame Game
		{get; protected set;}

		public string Name
		{get; init;}

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
					_o.AddToMap(this);

				return;
			}
		}

		private GUICore? _o;
		private bool _or;

		public DrawableAffineComponent? Tooltip
		{get; protected set;}

		public virtual SpriteBatch? Renderer
		{
			protected get => _r;

			set
			{
				if(ReferenceEquals(_r,value))
					return;

				_r = value;
				
				if(Tooltip is not null)
					Tooltip.Renderer = _r;

				return;
			}
		}

		protected SpriteBatch? _r;

		public virtual bool SuspendsDigitalNavigation => false;

		public virtual Color Tint
		{get; set;}

		public abstract int Width
		{get;}

		public abstract int Height
		{get;}

		/// <summary>
		/// This is the order that GUI elements are 
		/// </summary>
		public virtual int UpdateOrder
		{
			get => _uo;

			set
			{
				if(_uo == value)
					return;

				_uo = value;
				UpdateOrderChanged(this,EventArgs.Empty);

				return;
			}
		}

		protected int _uo;

		/// <summary>
		/// The order that this should be drawn in a GUI system.
		/// The GUICore always renders BackToFront, so larger values of DrawOrder are drawn on top of smaller values of DrawOrder (assuming this value is n in the scheme proposed in LayerDepth).
		/// <para/>
		/// This value is always positive.
		/// </summary>
		public virtual int DrawOrder
		{
			get => _do;

			set
			{
				if(_do == value || value < 1)
					return;

				_do = value;
				DrawOrderChanged(this,EventArgs.Empty);

				return;
			}
		}

		protected int _do;

		/// <summary>
		/// If true, this component is enabled.
		/// If false, this component is disabled.
		/// <para/>
		/// A disabled GUI component will not have its Update method called or recieve Exit commands from its GUICore.
		/// </summary>
		public virtual bool Enabled
		{
			get => _e;

			set
			{
				if(_e == value)
					return;

				_e = value;
				EnabledChanged(this,EventArgs.Empty);

				return;
			}
		}

		protected bool _e;

		/// <summary>
		/// If true, this component is visible.
		/// If false, this component is not visible.
		/// </summary>
		public virtual bool Visible
		{
			get => _v;

			set
			{
				if(_v == value)
					return;

				_v = value;
				VisibleChanged(this,EventArgs.Empty);

				return;
			}
		}

		protected bool _v;

		public Matrix2D Transform
		{
			get => _t;

			set
			{
				// It's not worth checking if _t == value, so we'll just blindly mark these as stale
				StaleInverse = true;

				// Instead of keeping a stale boolean, we double our value in using the revision number to mark these as stale
				ParentWorldRevision = 0u;
				ParentInverseWorldRevision = 0u;

				_t = value;
				return;
			}
		}

		protected Matrix2D _t;

		public Matrix2D World
		{
			get
			{
				if(StaleWorld)
				{
					// Our behavior differs depending on if we have a parent or not
					if(Parent is null)
					{
						ParentWorldRevision = 1u; // A 1 value means our world is up to date when we have no parent
						_w = Transform;
					}
					else
					{
						ParentWorldRevision = Parent.WorldRevision; // Our parent's revision number means our world is up to date when we have a parent
						_w = Parent.World * Transform;
					}

					++WorldRevision;
				}

				return _w;
			}
		}

		protected Matrix2D _w;

		public uint WorldRevision
		{get; protected set;}
		
		/// <summary>
		/// The currently known revision of Parent.World.
		/// A different (higher) value means that our World matrix is stale.
		/// </summary>
		protected uint ParentWorldRevision
		{get; set;}

		/// <summary>
		/// If true, then the world matrix is stale and needs to be updated.
		/// </summary>
		public bool StaleWorld => Parent is null ? ParentWorldRevision != 1u : ParentWorldRevision != Parent.WorldRevision || Parent.StaleWorld;

		public Matrix2D InverseTransform
		{
			get
			{
				if(StaleInverse)
					InverseTransform = Transform.Invert();

				return _it;
			}
			
			private set
			{
				_it = value;
				StaleInverse = false;

				return;
			}
		}

		protected Matrix2D _it;

		/// <summary>
		/// If true, then our inverse is stale an needs to be recalculated.
		/// If false, then our inverse is up to date.
		/// </summary>
		private bool StaleInverse
		{get; set;}

		public Matrix2D InverseWorld
		{
			get
			{
				if(StaleInverseWorld)
				{
					// Our behavior differs depending on if we have a parent or not
					if(Parent is null)
					{
						ParentInverseWorldRevision = 1u; // A 1 value means our inverse world is up to date when we have no parent
						_iw = InverseTransform;
					}
					else
					{
						ParentInverseWorldRevision = Parent.InverseWorldRevision; // Our parent's revision number means our inverse world is up to date when we have a parent
						_iw = InverseTransform * Parent.InverseWorld;
					}

					++InverseWorldRevision;
				}

				return _iw;
			}
		}

		protected Matrix2D _iw;

		public uint InverseWorldRevision
		{get; protected set;}

		/// <summary>
		/// If true, then the inverse world matrix is stale and needs to be updated.
		/// </summary>
		public bool StaleInverseWorld => StaleInverse || (Parent is null ? ParentInverseWorldRevision != 1u : ParentInverseWorldRevision != Parent.InverseWorldRevision || Parent.StaleInverseWorld);

		/// <summary>
		/// The currently known revision of Parent.InverseWorld.
		/// A different (higher) value means that our World matrix is stale.
		/// </summary>
		protected uint ParentInverseWorldRevision
		{get; set;}

		public IAffineComponent2D? Parent
		{
			get => _p;
			
			set
			{
				ParentWorldRevision = 0u;
				ParentInverseWorldRevision = 0u;

				_p = value;
				return;
			}
		}

		protected IAffineComponent2D? _p;
		
		public abstract Rectangle Bounds
		{get;}

		/// <summary>
		/// If true, this has been initialized.
		/// If false, it has not.
		/// </summary>
		public bool Initialized
		{get; protected set;}

		/// <summary>
		/// If true, this has been disposed.
		/// If false, it has not.
		/// </summary>
		public bool Disposed
		{get; protected set;}

		public event Click OnClick;
		public event Release OnRelease;
		public event Clicked OnClicked;
		public event Hovered OnHover;
		public event Moved OnMove;
		public event Exited OnExit;

		/// <summary>
		/// An event invoked when the enabled state of this component changes.
		/// </summary>
		public event EventHandler<EventArgs> EnabledChanged;

		/// <summary>
		/// An event invoked when the visibility state of this component changes.
		/// </summary>
		public event EventHandler<EventArgs> VisibleChanged;

		/// <summary>
		/// An event invoked when the update order of this component changes.
		/// </summary>
		public event EventHandler<EventArgs> UpdateOrderChanged;

		/// <summary>
		/// An event invoked when the draw order of this component changes.
		/// </summary>
		public event EventHandler<EventArgs> DrawOrderChanged;
	}
}
