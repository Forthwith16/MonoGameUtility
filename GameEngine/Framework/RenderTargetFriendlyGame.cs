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
			// Initialize initialzation data
			Initialized = false;
			Initializing = false;

			#if VerifyMidInitializationComponentAdds
			MidInitializationAdds = new Queue<IGameComponent>();
			#endif

			// Create the graphics device manager
			Graphics = new GraphicsDeviceManager(this);
			
			// Set up the render targets list
			RenderTargetComponents = new SortedDictionary<int,HashSet<IRenderTargetDrawable>>();
			
			// Do the same for the debug list
			#if DEBUG
			DebugComponents = new SortedDictionary<int,HashSet<IDebugDrawable>>();
			#endif

			// We'll need to keep track of render target components coming in and going out
			// Do the same for debug components
			// We also need to make sure things get initialized
			Components.ComponentAdded += (a,b) =>
			{
				if(b.GameComponent is IRenderTargetDrawable obj)
					AddRenderTargetComponent(obj);

				#if DEBUG
				if(b.GameComponent is IDebugDrawable obj2)
					AddDebugComponent(obj2);
				#endif

				// This will take care of initialization for anything that gets added
				if(Initialized)
					b.GameComponent.Initialize();
				#if VerifyMidInitializationComponentAdds
				else if(Initializing)
					MidInitializationAdds.Enqueue(b.GameComponent);
				#endif
				return;
			};

			Components.ComponentRemoved += (a,b) =>
			{
				if(b.GameComponent is IRenderTargetDrawable obj)
					RemoveRenderTargetComponent(obj);

				#if DEBUG
				if(b.GameComponent is IDebugDrawable obj2)
					RemoveDebugComponent(obj2);
				#endif

				return;
			};

			// Initialise the mouse data
			_m = null;
			_mr = null;
			
			return;
		}

		protected sealed override void Initialize()
		{
			BeforeInitialize();
			
			// Replace the system mouse with a custom one
			MouseRenderer = new SpriteBatch(GraphicsDevice);
			Mouse = MouseComponent.GenerateStandardMouse(this);
			IsMouseVisible = false;
			IsCustomMouseVisible = true;
			
			PreInitialize();

			Initializing = true;
			base.Initialize();
			
			#if VerifyMidInitializationComponentAdds
			// To ensure we actually catch all of the components to initialize, we have to do this (but only when we request it)
			// This can occur when someone inserts something into Components before the current object of initialization during base.Initialize or at any time during LoadContent
				while(MidInitializationAdds.Count > 0)
					MidInitializationAdds.Dequeue().Initialize();
			#endif

			Initializing = false;
			Initialized = true;
			
			PostInitialize();

			return;
		}

		/// <summary>
		/// Called before any initialization occurs (also before Initialized is set to true).
		/// </summary>
		protected virtual void BeforeInitialize()
		{return;}

		/// <summary>
		/// Called after mouse initialization but before the base game Initialize is called (also before Initialized is set to true).
		/// </summary>
		protected virtual void PreInitialize()
		{return;}

		/// <summary>
		/// Called after mouse initialization and after the base game Initialize is finished (and after Initialized is set to true).
		/// </summary>
		protected virtual void PostInitialize()
		{return;}

		/// <summary>
		/// Updates the position of <paramref name="sender"/> in RenderTargetComponents.
		/// </summary>
		private void UpdateRenderDrawOrder(object? sender, RenderTargetDrawOrderEventArgs e)
		{
			// Perform the removal logic first
			if(!RenderTargetComponents.TryGetValue(e.OldOrder,out HashSet<IRenderTargetDrawable>? rs))
				return;

			if(!rs.Remove(e.Sender))
				return;

			if(rs.Count == 0)
				RenderTargetComponents.Remove(e.OldOrder); // Whether this works or not doesn't really matter, so we'll ignore the return value

			// Now perform the addition logic
			if(!RenderTargetComponents.TryGetValue(e.NewOrder,out rs))
				RenderTargetComponents.Add(e.NewOrder,rs = new HashSet<IRenderTargetDrawable>());

			// If this fails, we're in lot of trouble regardless of what we do, so let's just hope it doesn't
			rs.Add(e.Sender);

			return;
		}
		
		#if DEBUG
		/// <summary>
		/// Updates the position of <paramref name="sender"/> in DebugComponents.
		/// </summary>
		private void UpdateDebugDrawOrder(IDebugDrawable sender, int new_order, int old_order)
		{
			// Perform the removal logic first
			if(!DebugComponents.TryGetValue(old_order,out HashSet<IDebugDrawable>? rs))
				return;

			if(!rs.Remove(sender))
				return;

			if(rs.Count == 0)
				DebugComponents.Remove(old_order); // Whether this works or not doesn't really matter, so we'll ignore the return value

			// Now perform the addition logic
			if(!DebugComponents.TryGetValue(new_order,out rs))
				DebugComponents.Add(new_order,rs = new HashSet<IDebugDrawable>());

			// If this fails, we're in lot of trouble regardless of what we do, so let's just hope it doesn't
			rs.Add(sender);
			
			return;
		}
		#endif

		/// <summary>
		/// Adds a render target component indepenent of the ordinary Components collection.
		/// </summary>
		/// <param name="component">The component to add.</param>
		/// <returns>Returns true if the component was added and false otherwise.</returns>
		public bool AddRenderTargetComponent(IRenderTargetDrawable component)
		{
			HashSet<IRenderTargetDrawable>? rs;

			if(!RenderTargetComponents.TryGetValue(component.RenderTargetDrawOrder,out rs))
				RenderTargetComponents.Add(component.RenderTargetDrawOrder,rs = new HashSet<IRenderTargetDrawable>());

			if(!rs.Add(component) && rs.Count == 0)
			{
				RenderTargetComponents.Remove(component.RenderTargetDrawOrder);
				return false;
			}

			component.RenderTargetDrawOrderChanged += UpdateRenderDrawOrder;
			return true;
		}

		/// <summary>
		/// Removes a render target component independent of the ordinary Components collection.
		/// </summary>
		/// <param name="component">The component to remove.</param>
		/// <returns>Returns true if the remove was successful and false otherwise.</returns>
		public bool RemoveRenderTargetComponent(IRenderTargetDrawable component)
		{
			HashSet<IRenderTargetDrawable>? rs;

			if(!RenderTargetComponents.TryGetValue(component.RenderTargetDrawOrder,out rs))
				return false;

			if(!rs.Remove(component))
				return false;

			if(rs.Count == 0)
				RenderTargetComponents.Remove(component.RenderTargetDrawOrder); // Whether this works or not doesn't really matter, so we'll ignore the return value

			component.RenderTargetDrawOrderChanged -= UpdateRenderDrawOrder;
			return true;
		}
		
		#if DEBUG
		/// <summary>
		/// Adds a debug component indepenent of the ordinary Components collection.
		/// </summary>
		/// <param name="component">The component to add.</param>
		/// <returns>Returns true if the component was added and false otherwise.</returns>
		protected bool AddDebugComponent(IDebugDrawable component)
		{
			HashSet<IDebugDrawable>? rs;

			if(!DebugComponents.TryGetValue(component.DrawDebugOrder,out rs))
				DebugComponents.Add(component.DrawDebugOrder,rs = new HashSet<IDebugDrawable>());

			if(!rs.Add(component) && rs.Count == 0)
			{
				DebugComponents.Remove(component.DrawDebugOrder);
				return false;
			}

			component.OnDrawDebugOrderChanged += UpdateDebugDrawOrder;
			return true;
		}
		#endif
		
		#if DEBUG
		/// <summary>
		/// Removes a debug component independent of the ordinary Components collection.
		/// </summary>
		/// <param name="component">The component to remove.</param>
		/// <returns>Returns true if the remove was successful and false otherwise.</returns>
		protected bool RemoveDebugComponent(IDebugDrawable component)
		{
			HashSet<IDebugDrawable>? rs;

			if(!DebugComponents.TryGetValue(component.DrawDebugOrder,out rs))
				return false;

			if(!rs.Remove(component))
				return false;

			if(rs.Count == 0)
				DebugComponents.Remove(component.DrawDebugOrder); // Whether this works or not doesn't really matter, so we'll ignore the return value

			component.OnDrawDebugOrderChanged -= UpdateDebugDrawOrder;
			return true;
		}
		#endif

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
		protected sealed override void Draw(GameTime delta)
		{
			PreRenderTargetDraw(delta);

			foreach(HashSet<IRenderTargetDrawable> rs in RenderTargetComponents.Values)
				foreach(IRenderTargetDrawable component in rs)
					if(component.Visible)
						component.DrawRenderTarget(delta);

			PostRenderTargetDraw(delta);

			GraphicsDevice.SetRenderTarget(null);

			PreDraw(delta);

			#if DEBUG
			// Don't draw debug information if we don't request it
			if(GlobalConstants.DrawDebugInformation)
			{
				DrawDebugInfo(delta);

				foreach(HashSet<IDebugDrawable> ds in DebugComponents.Values)
					foreach(IDebugDrawable draw in ds)
						if(draw.Visible)
							draw.DrawDebugInfo(delta);
			}
			#endif

			base.Draw(delta);
			PostDraw(delta);

			// Draw the mouse (there will be some small lag for those with good eyes if drawing below the system cursor's dedicated hardware refresh rate)
			if(Mouse is not null && IsCustomMouseVisible)
			{
				MouseRenderer!.Begin(Mouse.Order,Mouse.Blend,Mouse.Wrap,Mouse.DepthRecord,Mouse.Cull,Mouse.Shader);
				Mouse.Draw(delta);
				MouseRenderer.End();
			}
			
			AfterDraw(delta);
			return;
		}

		/// <summary>
		/// Called before render targets are drawn to.
		/// </summary>
		/// <param name="delta">The elapsed time since the last Draw call.</param>
		protected virtual void PreRenderTargetDraw(GameTime delta)
		{return;}

		/// <summary>
		/// Called after render targets are drawn to but before the render target is set back to null (the default).
		/// </summary>
		/// <param name="delta">The elapsed time since the last Draw call.</param>
		protected virtual void PostRenderTargetDraw(GameTime delta)
		{return;}

		/// <summary>
		/// Called after render targets are drawn to but before debug and ordinary Draw calls are made.
		/// </summary>
		/// <param name="delta">The elapsed time since the last Draw call.</param>
		protected virtual void PreDraw(GameTime delta)
		{return;}

		/// <summary>
		/// Draws debug information.
		/// </summary>
		/// <param name="delta">The elapsed game time since the last draw.</param>
		protected virtual void DrawDebugInfo(GameTime delta)
		{return;}

		/// <summary>
		/// Called after render targets are drawn to, debug drawing is finished, and ordinary Draw calls are made.
		/// </summary>
		/// <param name="delta">The elapsed time since the last Draw call.</param>
		protected virtual void PostDraw(GameTime delta)
		{return;}

		/// <summary>
		/// Called after all drawing is finished, including the custom mouse.
		/// </summary>
		/// <param name="delta">The elapsed time since the last Draw call.</param>
		protected virtual void AfterDraw(GameTime delta)
		{return;}

		/// <summary>
		/// The render target components to draw.
		/// </summary>
		protected SortedDictionary<int,HashSet<IRenderTargetDrawable>> RenderTargetComponents
		{get; init;}

		#if DEBUG
		/// <summary>
		/// The set of components which draw debug information.
		/// </summary>
		protected SortedDictionary<int,HashSet<IDebugDrawable>> DebugComponents
		{get; init;}
		#endif

		/// <summary>
		/// Compares IGUI objects by some draw order.
		/// </summary>
		protected class OrderComparer : IComparer<IRenderTargetDrawable>, IComparer<IDebugDrawable>
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

			public int Compare(IDebugDrawable? x, IDebugDrawable? y)
			{
				int val = x!.DrawDebugOrder.CompareTo(y!.DrawDebugOrder);

				if(val == 0)
					return ReferenceEquals(x,y) ? 0 : 1; // We don't care about the order of two distinct objects of the same draw order
				
				return val;
			}
		}

		/// <summary>
		/// If true, then this game is being initialized.
		/// If false, then the game either has yet to have initialization started or has been intialized.
		/// </summary>
		protected bool Initializing
		{get; private set;}

		#if VerifyMidInitializationComponentAdds
		/// <summary>
		/// A list of game components added while initializing the game.
		/// They may have already been initialized, but as we have no way of checking (thanks MonoGame), we just have to initialize all of them in case we missed one due to dumb Components insertions and hope for the best.
		/// </summary>
		protected Queue<IGameComponent> MidInitializationAdds
		{get; private set;}
		#endif

		/// <summary>
		/// If true, then this game has been initialized.
		/// If false, then the game has yet to be initialized.
		/// </summary>
		protected bool Initialized
		{get; private set;}

		/// <summary>
		/// The graphics device manager for the game.
		/// </summary>
		public GraphicsDeviceManager Graphics
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
