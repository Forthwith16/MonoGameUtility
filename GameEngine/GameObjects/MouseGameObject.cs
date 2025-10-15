using GameEngine.Resources.Sprites;
using GameEngine.Framework;
using GameEngine.Maths;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameEngine.GameObjects
{
	/// <summary>
	/// A mouse.
	/// It replaces the normal mouse, whose inner workings are deep magic.
	/// <para/>
	/// In order to not have double vision, a game should hide the system mouse when it has its own custom mouse.
	/// RenderTargetFriendlyGames have a custom mouse component built into its system.
	/// </summary>
	public class MouseGameObject : DrawableAffineObject
	{
		/// <summary>
		/// Creates a mouse game object backed by an ImageGameObject.
		/// </summary>
		/// <param name="src">The texture to pass off to the ImageGameObject.</param>
		/// <param name="renderer">The renderer to use to draw with (this can be changed later).</param>
		public MouseGameObject(Texture2D src, SpriteRenderer? renderer = null) : base(renderer)
		{
			CommonConstruction(new ImageGameObject(renderer,src));
			return;
		}

		/// <summary>
		/// Creates a mouse game object backed by a MultiImageGameObject.
		/// </summary>
		/// <param name="src">The image sources to pass off to the MultiImageGameObject.</param>
		/// <param name="renderer">The renderer to use to draw with (this can be changed later).</param>
		public MouseGameObject(SpriteSheet src, SpriteRenderer? renderer = null) : base(renderer)
		{
			CommonConstruction(new MultiImageGameObject(renderer,src));
			return;
		}

		/// <summary>
		/// Creates a mouse game object backed by an AnimatedGameObject.
		/// </summary>
		/// <param name="src">The animation to pass off to the AnimatedGameObject.</param>
		/// <param name="renderer">The renderer to use to draw with (this can be changed later).</param>
		public MouseGameObject(Animation2D src, SpriteRenderer? renderer = null) : base(renderer)
		{
			CommonConstruction(new AnimatedGameObject(renderer,src));
			return;
		}

		/// <summary>
		/// Creates a mouse game object backed by some DrawbleAffineGameObject.
		/// </summary>
		/// <param name="game">The game this mouse will belong to.</param>
		/// <param name="mouse">
		///	The component that draws the mouse cursor.
		///	This can be anything, but with great power comes great responsibility.
		///	Its parent will be set to this (and should not be changed) but may otherwise be transformed freely (use this power responsibly).
		///	This game object will take responsibility for initializing, updating, and drawing <paramref name="mouse"/>.
		///	It will also take responsibility for disposing of it when this is disposed of.
		///	No other limits are placed upon this.
		/// </param>
		/// <param name="renderer">The renderer to use to draw with (this can be changed later).</param>
		public MouseGameObject(DrawableAffineObject mouse, SpriteRenderer? renderer = null) : base(renderer)
		{
			CommonConstruction(mouse);
			return;
		}

		/// <summary>
		/// Performs the construction logic common to every constructor.
		/// </summary>
		/// <param name="mouse">The game object that will actually draw the mouse.</param>
		private void CommonConstruction(DrawableAffineObject mouse)
		{
			_mc = mouse; // Make sure we assign _mc before we ever use MouseCursor
			MouseCursor.Parent = this;

			return;
		}

		/// <summary>
		/// Initializes this mouse game object.
		/// </summary>
		public override void Initialize()
		{
			if(Initialized)
				return;

			MouseCursor.Initialize();

			base.Initialize();
			return;
		}

		/// <summary>
		/// Updates the position of the mouse to match the system's.
		/// </summary>
		/// <param name="delta">The elapsed time since the last Update call.</param>
		public override void Update(GameTime delta)
		{
			// Force the mouse cursor to be where the mouse cursor should be
			// We won't bother with an InputManager here; instead, directly grab whatever raw mouse position Monogame reports to us from the system at all times
			Transform = Matrix2D.Translation(Mouse.GetState().Position.ToVector2());
			
			MouseCursor.Update(delta);
			return;
		}

		/// <summary>
		/// Draws the mouse game object.
		/// </summary>
		/// <param name="delta">The elapsed time since the last Draw call.</param>
		public override void Draw(GameTime delta)
		{
			MouseCursor.Draw(delta);
			return;
		}

		/// <summary>
		/// Called when a game object is disposed of to unload any resources that must be closed or otherwise disposed of manually.
		/// </summary>
		protected override void UnloadContent()
		{
			MouseCursor.Dispose();
			return;
		}

		/// <summary>
		/// Creates a simple, default mouse cursor.
		/// </summary>
		/// <param name="game">The game to attach the mouse game object to and to initialize the required texture with.</param>
		/// <returns>Returns a new mouse cursor that is standard and boring.</returns>
		public static MouseGameObject GenerateStandardMouse(RenderTargetFriendlyGame game)
		{
			Texture2D src = new Texture2D(game.GraphicsDevice,12,19);
			
			Color t = Color.Transparent;
			Color b = Color.Black;
			Color w = Color.White;

			Color[] c = {b,t,t,t,t,t,t,t,t,t,t,t,
					   b,b,t,t,t,t,t,t,t,t,t,t,
					   b,w,b,t,t,t,t,t,t,t,t,t,
					   b,w,w,b,t,t,t,t,t,t,t,t,
					   b,w,w,w,b,t,t,t,t,t,t,t,
					   b,w,w,w,w,b,t,t,t,t,t,t,
					   b,w,w,w,w,w,b,t,t,t,t,t,
					   b,w,w,w,w,w,w,b,t,t,t,t,
					   b,w,w,w,w,w,w,w,b,t,t,t,
					   b,w,w,w,w,w,w,w,w,b,t,t,
					   b,w,w,w,w,w,w,w,w,w,b,t,
					   b,w,w,w,w,w,w,w,w,w,w,b,
					   b,w,w,w,w,w,w,b,b,b,b,b,
					   b,w,w,w,b,w,w,b,t,t,t,t,
					   b,w,w,b,t,b,w,w,b,t,t,t,
					   b,w,b,t,t,b,w,w,b,t,t,t,
					   b,b,t,t,t,t,b,w,w,b,t,t,
					   t,t,t,t,t,t,b,w,w,b,t,t,
					   t,t,t,t,t,t,t,b,b,t,t,t};

			src.SetData(c);
			return new MouseGameObject(src);
		}

		/// <summary>
		/// Creates a transparent cursor of the given dimensions.
		/// Use this to enable the system cursor but still retain height and width information.
		/// </summary>
		/// <param name="g">The graphics device used to display the generated texture.</param>
		/// <param name="w">The width the mouse to generate. The default value is the normal default width of the Windows 10 arrow cursor.</param>
		/// <param name="h">The height the mouse to generate. The default value is the normal default height of the Windows 10 arrow cursor.</param>
		/// <returns>Returns a transparent mouse cursor used for bookkeeping mouse dimensions.</returns>
		public static MouseGameObject GenerateTransparentMouse(GraphicsDevice g, int w = 12, int h = 19)
		{
			Texture2D src = new Texture2D(g,w,h);
			Color[] c = new Color[w * h];

			for(int i = 0;i < c.Length;i++)
				c[i] = Color.Transparent;

			src.SetData(c);
			return new MouseGameObject(src);
		}

		/// <summary>
		/// The actual mouse cursor that we draw.
		/// </summary>
		public DrawableAffineObject MouseCursor
		{
			get => _mc!;

			protected set
			{
				if(ReferenceEquals(_mc,value) || value == null)
					return;

				_mc = value;

				_mc.Game = Game;
				_mc.Renderer = Renderer;
				_mc.Tint = Tint;
				_mc.LayerDepth = LayerDepth;

				return;
			}
		}

		protected DrawableAffineObject? _mc;

		public override RenderTargetFriendlyGame? Game
		{
			get => base.Game;

			protected internal set
			{
				base.Game = value;

				if(MouseCursor is not null)
					MouseCursor.Game = value;

				return;
			}
		}

		public override SpriteRenderer? Renderer
		{
			get => base.Renderer;

			set
			{
				if(ReferenceEquals(Renderer,value))
					return;

				base.Renderer = value;

				if(MouseCursor is not null)
					MouseCursor.Renderer = value;

				return;
			}
		}

		public override Color Tint
		{
			get => base.Tint;
			
			set
			{
				base.Tint = value;

				if(MouseCursor is not null)
					MouseCursor.Tint = value;

				return;
			}
		}

		public override float LayerDepth
		{
			get => base.LayerDepth;
			
			set
			{
				if(value < 0.0f || value > 1.0f)
					return;
				
				base.LayerDepth = value;

				if(MouseCursor is not null)
					MouseCursor.LayerDepth = value;

				return;
			}
		}

		public override Rectangle Bounds => MouseCursor.Bounds;
		public override int Width => MouseCursor.Width;
		public override int Height => MouseCursor.Height;
	}
}
