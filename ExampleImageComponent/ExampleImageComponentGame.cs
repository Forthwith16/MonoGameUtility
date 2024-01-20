using GameEngine.Framework;
using GameEngine.GameComponents;
using GameEngine.Maths;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ExampleImageComponent
{
	/// <summary>
	/// A RenderTargetFriendlyGame has a number of additional features over the Game class.
	/// The chief among them is an automated system for drawing to RenderTargets before a proper Draw call to ensure that the proper drawing order is maintained.
	/// It also replaces the mouse with a custom cursor, which may experience some lag on less powerful machines, since it doesn't go through the dedicated hardware for drawing mice.
	/// <br/><br/>
	/// This game demonstrates how to build a RenderTargetFriendlyGame and how to interact with the GameComponents classes of the GameEngine library.
	/// </summary>
	public class ExampleImageComponentGame : RenderTargetFriendlyGame
	{
		public ExampleImageComponentGame()
		{
			// The graphics device and mouse are handled by the base RenderTargetFriendlyGame class, so we need not contend with them
			Content.RootDirectory = "Content";

			return;
		}

		protected override void LoadContent()
		{
			_spriteBatch = new SpriteBatch(GraphicsDevice);

			// Create an ImageComponent
			_image = new ImageComponent(this,_spriteBatch,"Pokeball");

			// The ImageComponent's transform is initialized to the Identity matrix, but you can change it however you like
			// The Matrix2D struct will allow you to create new matrices from old ones to apply more transformations
			// It is, however, an immutable struct, so when you make a new transformation, you must assign it to what you want
			// For example, you could do _image.Transform = _image.Transform.Rotate90(1)
			_image.Transform = Matrix2D.Identity;
			
			// The ImageComponent is a DrawableGameComponent, so we add it to the game and let the game take care of its management
			Components.Add(_image);
			
			return;
		}

		protected override void Update(GameTime delta)
		{
			if(GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
				Exit();

			// Create an adjustable speed parameter for convenience
			float speed = 100.0f * (float)delta.ElapsedGameTime.TotalSeconds; // We currently move 100 pixels per second

			// Let's allow the ImageComponent to move upon demand
			if(GamePad.GetState(PlayerIndex.One).DPad.Left == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Left))
				_image.Transform = _image.Transform.Translate(-speed,0.0f);

			if(GamePad.GetState(PlayerIndex.One).DPad.Right == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Right))
				_image.Transform = _image.Transform.Translate(speed,0.0f);

			if(GamePad.GetState(PlayerIndex.One).DPad.Down == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Down))
				_image.Transform = _image.Transform.Translate(0.0f,speed); // Remember, the TOP-left of the screen is the origin (0,0) and the BOTTOM-right is (width,height), so up is down

			if(GamePad.GetState(PlayerIndex.One).DPad.Up == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Up))
				_image.Transform = _image.Transform.Translate(0.0f,-speed);

			base.Update(delta);
			return;
		}

		/// <summary>
		/// The PreDraw method is called every Draw call before Draw.
		/// It has two primary intended uses.
		/// First, clear the screen.
		/// Second, call Begin on any SpriteBatches used to render game components from the GameEngine library.
		/// </summary>
		/// <param name="delta">The usual elapsed time.</param>
		protected override void PreDraw(GameTime delta)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);

			// A SpriteBatch has many possible global parameters to its later Draw calls
			// These are the normal choices to make for 2D drawing with two notes
			// The first parameter indicates how sprites are sorted when drawn according to a depth parameter ranging within [0,1]
			// The last parameter is the global 'camera' matrix, which can be used to transform ALL draws made with the sprite batch until the next End call
			// You can provide Matrix2D matrices to this parameter for a Matrix2D m via m.Invert().SwapChirality()
			// Note that a camera does opposite transforms as per normal (e.g. if you move the camera left, it moves all objects in the scene right)
			// Also, the SwapChirality call is a necessary component for this particular parameter (and only this one) due to how Monogame handles SpriteBatching and Matrices
			_spriteBatch!.Begin(SpriteSortMode.BackToFront,BlendState.NonPremultiplied,SamplerState.LinearWrap,null,null,null,null);
			
			return;
		}

		/// <summary>
		/// The PostDraw method is called every Draw call after Draw.
		/// Its primary intended use is to call End on any SpriteBatches whose Begin was called in PreDraw.
		/// </summary>
		/// <param name="delta">The usual elapsed time.</param>
		protected override void PostDraw(GameTime delta)
		{
			_spriteBatch!.End();
			return;
		}
		
		private SpriteBatch _spriteBatch;
		private ImageComponent _image;
	}
}
