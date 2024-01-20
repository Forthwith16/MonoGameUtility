using GameEngine.Framework;
using GameEngine.Maths;
using GameEngine.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameEngine.GameComponents
{
	/// <summary>
	/// A mouse.
	/// It replaces the normal mouse, whose inner workings are deep magic.
	/// <para/>
	/// In order to not have double vision, a game should hide the system mouse when it has its own custom mouse.
	/// RenderTargetFriendlyGames have a custom mouse component built into its system.
	/// </summary>
	public class MouseComponent : DrawableAffineComponent
	{
		/// <summary>
		/// Creates a mouse component backed by an ImageComponent.
		/// </summary>
		/// <param name="game">The game this mouse will belong to.</param>
		/// <param name="src">The texture to pass off to the ImageComponent.</param>
		/// <param name="renderer">The renderer to use to draw with (this can be changed later).</param>
		public MouseComponent(Game game, Texture2D src, SpriteBatch? renderer = null) : base(game,renderer)
		{
			CommonConstruction(new ImageComponent(game,renderer,src));
			return;
		}

		/// <summary>
		/// Creates a mouse component backed by a MultiImageComponent.
		/// </summary>
		/// <param name="game">The game this mouse will belong to.</param>
		/// <param name="src">The image sources to pass off to the MultiImageComponent.</param>
		/// <param name="renderer">The renderer to use to draw with (this can be changed later).</param>
		public MouseComponent(Game game, SpriteSheet src, SpriteBatch? renderer = null) : base(game,renderer)
		{
			CommonConstruction(new MultiImageComponent(game,renderer,src));
			return;
		}

		/// <summary>
		/// Creates a mouse component backed by an AnimatedComponent.
		/// </summary>
		/// <param name="game">The game this mouse will belong to.</param>
		/// <param name="src">The animation to pass off to the AnimatedComponent.</param>
		/// <param name="renderer">The renderer to use to draw with (this can be changed later).</param>
		public MouseComponent(Game game, Animation2D src, SpriteBatch? renderer = null) : base(game,renderer)
		{
			CommonConstruction(new AnimatedComponent(game,renderer,src));
			return;
		}

		/// <summary>
		/// Creates a mouse component backed by some DrawbleAffineComponent.
		/// </summary>
		/// <param name="game">The game this mouse will belong to.</param>
		/// <param name="mouse">
		///	The component that draws the mouse cursor.
		///	This can be anything, but with great power comes great responsibility.
		///	Its parent will be set to this (and should not be changed) but may otherwise be transformed freely (use this power responsibly).
		///	This component will take responsibility for initializing, updating, and drawing <paramref name="mouse"/>.
		///	It will also take responsibility for disposing of it when this is disposed of.
		///	No other limits are placed upon this.
		/// </param>
		/// <param name="renderer">The renderer to use to draw with (this can be changed later).</param>
		public MouseComponent(Game game, DrawableAffineComponent mouse, SpriteBatch? renderer = null) : base(game,renderer)
		{
			CommonConstruction(mouse);
			return;
		}

		/// <summary>
		/// Performs the construction logic common to every constructor.
		/// </summary>
		/// <param name="mouse">The component that will actually draw the mouse.</param>
		private void CommonConstruction(DrawableAffineComponent mouse)
		{
			_mc = mouse; // Make sure we assign _mc before we ever use MouseCursor
			MouseCursor.Parent = this;

			Order = SpriteSortMode.BackToFront;
			Blend = BlendState.NonPremultiplied;
			
			return;
		}

		/// <summary>
		/// Initializes this mouse component.
		/// </summary>
		public override void Initialize()
		{
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
		/// Draws the mouse component.
		/// </summary>
		/// <param name="delta">The elapsed time since the last Draw call.</param>
		public override void Draw(GameTime delta)
		{
			MouseCursor.Draw(delta);
			return;
		}

		/// <summary>
		/// Called when a component is disposed of to unload any resources that must be closed or otherwise disposed of manually.
		/// </summary>
		protected override void UnloadContent()
		{
			MouseCursor.Dispose();
			return;
		}

		/// <summary>
		/// Creates a simple, default mouse cursor.
		/// </summary>
		/// <param name="game">The game to attach the mouse component to and to initialize the required texture with.</param>
		/// <returns>Returns a new mouse cursor that is standard and boring.</returns>
		public static MouseComponent GenerateStandardMouse(RenderTargetFriendlyGame game)
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
			return new MouseComponent(game,src);
		}

		/// <summary>
		/// Creates a transparent cursor of the given dimensions.
		/// Use this to enable the system cursor but still retain height and width information.
		/// </summary>
		/// <param name="game">The game to attach the mouse component to and to initialize the required texture with.</param>
		/// <param name="w">The width the mouse to generate. The default value is the normal default width of the Windows 10 arrow cursor.</param>
		/// <param name="h">The height the mouse to generate. The default value is the normal default height of the Windows 10 arrow cursor.</param>
		/// <returns>Returns a transparent mouse cursor used for bookkeeping mouse dimensions.</returns>
		public static MouseComponent GenerateTransparentMouse(RenderTargetFriendlyGame game, int w = 12, int h = 19)
		{
			Texture2D src = new Texture2D(game.GraphicsDevice,w,h);
			Color[] c = new Color[w * h];

			for(int i = 0;i < c.Length;i++)
				c[i] = Color.Transparent;

			src.SetData(c);
			return new MouseComponent(game,src);
		}

		/// <summary>
		/// The actual mouse cursor that we draw.
		/// </summary>
		public DrawableAffineComponent MouseCursor
		{
			get => _mc!;

			protected set
			{
				if(ReferenceEquals(_mc,value) || value == null)
					return;

				_mc = value;

				_mc.Renderer = Renderer;
				_mc.Tint = Tint;
				_mc.LayerDepth = LayerDepth;

				return;
			}
		}

		protected DrawableAffineComponent? _mc;

		/// <summary>
		/// The order that sprites are sorted when drawn.
		/// If the mouse component has many children to draw the mouse, changing this value may be necessary.
		/// <para/>
		/// This value defaults to BackToFront.
		/// </summary>
		public SpriteSortMode Order
		{get; set;}

		/// <summary>
		/// A blend mode for the mouse to be drawn with.
		/// <para/>
		/// This value defaults to NonPremultiplied (null defaults to NonPremultiplied).
		/// </summary>
		public BlendState? Blend
		{get; set;}

		/// <summary>
		/// A sampler wrap mode for the mouse to be drawn with.
		/// <para/>
		/// This value defaults to null (which in turn defaults to LinearClamp).
		/// </summary>
		public SamplerState? Wrap
		{get; set;}

		/// <summary>
		/// A shader for the mouse to be drawn with.
		/// <para/>
		/// This value defaults to null (which in turn defaults to None).
		/// </summary>
		public DepthStencilState? DepthRecord
		{get; set;}

		/// <summary>
		/// The cull state used when drawing the mouse.
		/// <para/>
		/// This value defaults to null (which in turn deaults to CullCounterClockwise).
		/// </summary>
		public RasterizerState? Cull
		{get; set;}

		/// <summary>
		/// A shader for the mouse to be drawn with.
		/// <para/>
		/// This value defaults to null (which in turn defaults to the default sprite effect).
		/// </summary>
		public Effect? Shader
		{get; set;}

		/// <summary>
		/// This is the SpriteBatch used to draw this component.
		/// If this is unassigned (null), then the draw will be skipped.
		/// </summary>
		public override SpriteBatch? Renderer
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

		/// <summary>
		/// The tint/color to apply to this component.
		/// <para/>
		/// This defaults to Color.White for true color.
		/// </summary>
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

		/// <summary>
		/// This is the drawing layer.
		/// This value must be within [0,1].
		/// With respect to the drawing order, larger values are the 'front' and smaller values are the 'back'.
		/// This value only has significance when the SpriteBatch's SpriteSortMode is BackToFront (in which case smaller values are drawn on top) or FrontToBack (in which case larger values are drawn on top).
		/// <para/>
		/// This value defaults to 0.0f.
		/// </summary>
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
