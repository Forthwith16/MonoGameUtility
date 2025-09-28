using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ExampleDrawImage
{
	/// <summary>
	/// All games will inherit from Game (or a derived class of Game).
	/// <br/><br/>
	/// This game demonstrates the bare basics of how to draw a texture to the screen.
	/// It also documents the details of how a Game is structured, particularly its core game loop.
	/// </summary>
	public class ExampleDrawImageGame : Game
	{
		/// <summary>
		/// An ordinary constructor.
		/// This is where you perform basic initialization logic such as the lines shown below.
		/// </summary>
		public ExampleDrawImageGame()
		{
			_graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
			IsMouseVisible = true;

			_graphics.PreferredBackBufferWidth = 990;
			_graphics.PreferredBackBufferHeight = 600;
			
			return;
		}

		/// <summary>
		/// This method is called before anything else (after the constructor) once per execution.
		/// It is typically used to load game data and get everything running.
		/// The base Initialize method also performs some necessary graphical settings alterations, some of which must occur before loading graphical content.
		/// In general, it is always better to delay Content.Load calls (and all other graphical calls) until reaching the LoadContent method (here or in DrawableGameComponent objects).
		/// </summary>
		protected override void Initialize()
		{
			base.Initialize(); // The base Initialize call will call the Initialize method on every GameComponent added to Components (these methods in turn call LoadContent on DrawableGameComponents)
			return;
		}

		/// <summary>
		/// This method is called by base.Initialize at its finish on DrawableGameObjects.
		/// This is the ideal place to make queries to Content.Load to load graphical items (e.g. textures).
		/// It is called once per execution.
		/// <br/><br/>
		/// The UnloadContent method is used to dispose of content no longer needed (before the entire execution ends).
		/// </summary>
		protected override void LoadContent()
		{
			// We create a new SpriteBatch using the Game's GraphicsDevice (which is ordinarily initialized between Initialize and LoadContent, i.e. in base.Initialize)
			_spriteBatch = new SpriteBatch(GraphicsDevice);
			
			// We load content relative to Content.RootDirectory and omit file extensions
			// The type of content loaded is given as the generic parameter
			// For this to work, the content in question must be loaded into your content file, in this case Content.mgcb
			_image = Content.Load<Texture2D>("Haruhi");

			return;
		}

		/// <summary>
		/// This method is called every update cycle.
		/// Ordinarily, this happens as quickly as possible with each frame drawn between Update calls.
		/// However, a fixed update cycle can be imposed to fix the draw rate to a certain frequency.
		/// </summary>
		/// <param name="delta">The elapsed time since the last update. This also includes information about the total elapsed time.</param>
		protected override void Update(GameTime delta)
		{
			if(GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
				Exit();
			
			base.Update(delta); // The base update call will call the update method on every GameComponent added to Components
			return;
		}

		/// <summary>
		/// This method is called every update cycle to draw the game scene.
		/// Usually, a Game object will do little to nothing in its Draw method other than clearing the screen (if even that).
		/// The actual drawing work will be distributed to DrawableGameComponents added to the Game.
		/// </summary>
		/// <param name="delta">The elapsed time since the last update. This also includes information about the total elapsed time.</param>
		protected override void Draw(GameTime delta)
		{
			// The following line clears the entire drawing buffer to the color provided
			GraphicsDevice.Clear(Color.CornflowerBlue);

			// To draw a texture in 2D, we use SpriteRendereres
			// A Draw call must occur between a SpriteRenderer Begin and End call
			// In general, Begin and End calls are inefficient, and you do want to batch lots of textures (i.e. sprites) together into one group to make the calls effective
			_spriteBatch.Begin();
			_spriteBatch.Draw(_image,new Rectangle(0,0,990,600),new Rectangle(0,0,2640,1600),Color.White);
			_spriteBatch.End();

			base.Draw(delta); // The base draw call will call the draw method on every DrawableGameComponent added to Components
			return;
		}

		private GraphicsDeviceManager _graphics;
		private SpriteBatch _spriteBatch;
		private Texture2D _image;
	}
}
