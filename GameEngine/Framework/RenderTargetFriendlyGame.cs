using GameEngine.GameComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngine.Framework
{
	/// <summary>
	/// A game that has utility support for render targets.
	/// </summary>
	public abstract class RenderTargetFriendlyGame : Game
	{
		/// <summary>
		/// Creates a new game that is friendly to classes that use render targets.
		/// </summary>
		protected RenderTargetFriendlyGame() : base()
		{
			// Create the graphics device manager
			Graphics = new GraphicsDeviceManager(this);
			
			// Set up the render targets list
			RenderTargetComponents = new SortedSet<IRenderTargetDrawable>(new OrderComparer());
			
			// We'll need to keep track of components coming in and going out
			Components.ComponentAdded += (a,b) =>
			{
				if(b.GameComponent is IRenderTargetDrawable obj)
				{
					RenderTargetComponents.Add(obj);
					obj.RenderTargetDrawOrderChanged += UpdateDrawOrder;
				}
				
				if(Initialized)
					b.GameComponent.Initialize();

				return;
			};

			Components.ComponentRemoved += (a,b) =>
			{
				if(b.GameComponent is IRenderTargetDrawable obj)
				{
					RenderTargetComponents.Remove(obj);
					obj.RenderTargetDrawOrderChanged -= UpdateDrawOrder;
				}

				return;
			};

			// Initialise the mouse data
			_m = null;
			_mr = null;

			Initialized = false;
			return;
		}

		protected override void Initialize()
		{
			Initialized = true;
			
			// Replace the system mouse with a custom one
			MouseRenderer = new SpriteBatch(GraphicsDevice);
			Mouse = MouseComponent.GenerateStandardMouse(this);
			IsMouseVisible = false;
			IsCustomMouseVisible = true;
			
			base.Initialize();
			return;
		}

		/// <summary>
		/// Updates the position of <paramref name="sender"/> in UpdateChildren.
		/// </summary>
		private void UpdateDrawOrder(object? sender, EventArgs e)
		{
			if(sender is not IRenderTargetDrawable component)
				return;
			
			if(RenderTargetComponents.Remove(component))
				RenderTargetComponents.Add(component);
			
			return;
		}

		/// <summary>
		/// Adds a render target component indepenent of the ordinary Components collection.
		/// </summary>
		/// <param name="component">The component to add.</param>
		/// <returns>Returns true if the component was added and false otherwise.</returns>
		public bool AddRenderTargetComponent(IRenderTargetDrawable component)
		{
			if(!RenderTargetComponents.Add(component))
				return false;

			component.RenderTargetDrawOrderChanged += UpdateDrawOrder;
			return true;
		}

		/// <summary>
		/// Removes a render target component independent of the ordinary Components collection.
		/// </summary>
		/// <param name="component">The component to remove.</param>
		/// <returns>Returns true if the remove was successful and false otherwise.</returns>
		public bool RemoveRenderTargetComponent(IRenderTargetDrawable component)
		{
			if(!RenderTargetComponents.Remove(component))
				return false;

			component.RenderTargetDrawOrderChanged -= UpdateDrawOrder;
			return true;
		}

		/// <summary>
		/// Updates this game.
		/// </summary>
		/// <param name="delta">The elapsed time since the last Update call.</param>
		protected override void Update(GameTime delta)
		{
			if(Mouse is not null)
				Mouse.Update(delta);

			base.Update(delta);
			return;
		}

		/// <summary>
		/// Draws this game.
		/// </summary>
		/// <param name="delta">The elapsed time since the last Draw call.</param>
		protected override void Draw(GameTime delta)
		{
			PreRenderTargetDraw(delta);

			foreach(IRenderTargetDrawable component in RenderTargetComponents)
				component.DrawRenderTarget(delta);

			GraphicsDevice.SetRenderTarget(null);

			PreDraw(delta);
			base.Draw(delta);
			PostDraw(delta);

			// Draw the mouse (there will be some small lag for those with good eyes if drawing below the system cursor's dedicated hardware refresh rate)
			if(Mouse is not null && IsCustomMouseVisible)
			{
				MouseRenderer!.Begin(Mouse.Order,Mouse.Blend,Mouse.Wrap,Mouse.DepthRecord,Mouse.Cull,Mouse.Shader);
				Mouse.Draw(delta);
				MouseRenderer.End();
			}
			
			return;
		}

		/// <summary>
		/// Called before render targets are drawn to.
		/// </summary>
		/// <param name="delta">The elapsed time since the last Draw call.</param>
		protected virtual void PreRenderTargetDraw(GameTime delta)
		{return;}

		/// <summary>
		/// Called after render targets are drawn to but before ordinary Draw calls are made.
		/// </summary>
		/// <param name="delta">The elapsed time since the last Draw call.</param>
		protected virtual void PreDraw(GameTime delta)
		{return;}

		/// <summary>
		/// Called after both render targets are drawn to and ordinary Draw calls are made.
		/// </summary>
		/// <param name="delta">The elapsed time since the last Draw call.</param>
		protected virtual void PostDraw(GameTime delta)
		{return;}

		/// <summary>
		/// The render target components to draw.
		/// </summary>
		protected SortedSet<IRenderTargetDrawable> RenderTargetComponents
		{get; init;}

		/// <summary>
		/// Compares IGUI objects by render draw order.
		/// </summary>
		protected class OrderComparer : IComparer<IRenderTargetDrawable>
		{
			/// <summary>
			/// Creates a new order comparer.
			/// </summary>
			public OrderComparer()
			{return;}

			public int Compare(IRenderTargetDrawable? x, IRenderTargetDrawable? y)
			{
				int val = x!.RenderTargetDrawOrder.CompareTo(y!.RenderTargetDrawOrder);

				if(val == 0)
					return ReferenceEquals(x,y) ? 0 : 1; // We don't care about the order of two distinct objects of the same draw order
				
				return val;
			}
		}

		/// <summary>
		/// If true, then this game has been initialized.
		/// If false, then the game has yet to be initialized.
		/// </summary>
		protected bool Initialized
		{get; private set;}

		/// <summary>
		/// The graphics device manager for the game.
		/// </summary>
		protected GraphicsDeviceManager Graphics
		{get; private set;}

		/// <summary>
		/// The mouse.
		/// <para/>
		/// Attempting to assign null to this does nothing.
		/// </summary>
		public MouseComponent? Mouse
		{
			get => _m;

			set
			{
				if(value is null || ReferenceEquals(_m,value))
					return;

				_m = value;
				_m.Renderer = MouseRenderer;

				return;
			}
		}

		private MouseComponent? _m;

		/// <summary>
		/// The sprite batch that will render the mouse cursor.
		/// </summary>
		private SpriteBatch? MouseRenderer
		{
			get => _mr;
			
			set
			{
				if(value is null || ReferenceEquals(_m,value))
					return;

				_mr = value;
				
				if(Mouse is not null)
					Mouse.Renderer = _mr;

				return;
			}
		}

		private SpriteBatch? _mr;
		
		/// <summary>
		/// Gets or sets the custom mouse's visibility.
		/// <para/>
		/// To show/hide the system mouse, use IsMouseVisible.
		/// </summary>
		public bool IsCustomMouseVisible
		{
			get => Mouse is not null && Mouse.Visible;

			set
			{
				if(Mouse is null || value == Mouse.Visible)
					return;

				Mouse.Visible = value;
				return;
			}
		}
	}
}
