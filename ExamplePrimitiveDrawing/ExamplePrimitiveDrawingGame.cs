using GameEngine.Texture;
using GameEngine.Utility.ExtensionMethods.ClassExtensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ExamplePrimitiveDrawing
{
	/// <summary>
	/// Shows how to use the primitive drawing extensions to the SpriteBatch class.
	/// </summary>
	public class ExamplePrimitiveDrawingGame : Game
	{
		public ExamplePrimitiveDrawingGame()
		{
			Graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
			IsMouseVisible = true;

			Graphics.PreferredBackBufferWidth = 1000;
			Graphics.PreferredBackBufferHeight = 1000;

			return;
		}

		protected override void LoadContent()
		{
			Renderer = new SpriteBatch(GraphicsDevice);
			DoublePixel = ColorFunctions.GenerateTexture(this,2,1,(x,y) => x == 0 ? Color.Blue : Color.Green);

			return;
		}

		protected override void Update(GameTime delta)
		{
			if(GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
				Exit();

			return;
		}

		protected override void Draw(GameTime delta)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);

			// We're responsible for beginning the batch draw
			Renderer!.Begin(SpriteSortMode.Deferred,null,SamplerState.LinearClamp);
			
			// Draw a solid line from point a to point b; nothing fancy
			Renderer.DrawLine(new Vector2(100.0f,100.0f),new Vector2(900.0f,100.0f),Color.Red,3.0f,1.0f);
			
			// Experiment with the sampler states to get different effects as we draw two different colors
			Renderer.DrawLine(DoublePixel!,new Vector2(100.0f,200.0f),500.0f,MathHelper.ToRadians(-22.0f),Color.White,3.0f,1.0f);

			// Draw polygons!
			Renderer.DrawPolygon(Color.Black,3.0f,1.0f,new Vector2(300.0f,300.0f),new Vector2(500.0f,400.0f),new Vector2(500.0f,500.0f),new Vector2(300.0f,500.0f));

			// We're responsible for ending the batch draw
			Renderer.End();
			
			return;
		}

		private GraphicsDeviceManager Graphics;
		private SpriteBatch? Renderer;
		private Texture2D? DoublePixel;
	}
}
