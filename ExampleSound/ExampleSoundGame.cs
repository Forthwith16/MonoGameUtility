using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ExampleSound
{
	public class ExampleSoundGame : Game
	{
		public ExampleSoundGame()
		{
			_graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
			IsMouseVisible = true;

			return;
		}

		protected override void LoadContent()
		{
			// Loading a sound effect is as easy as this
			// Make sure to set the processor in your mgcb file to SoundEffect (it defaults to Song for many audio formats)
			// Adding 3D effects to sound is more complicated, but just playing a sound is easy
			// Playing background songs is also a bit different (see the Song class and its associated classes)
			_sfx = Content.Load<SoundEffect>("clap");
			
			return;
		}

		protected override void Update(GameTime gameTime)
		{
			if(GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
				Exit();

			if(Keyboard.GetState().IsKeyDown(Keys.Space) && !played)
			{
				// There are other parameters you can play with, but this will play your sound effect; easy, no?
				// You can also call CreateInstance to manipulate an instance of this sound effect yourself rather than just firing off a sound effect and then forgetting about it
				// With the instance, you can also track where it is in its lifetime and detect when it finishes
				// To apply 3D effects, you also need to manipulate the instance directly
				_sfx.Play();

				played = true; // We'll only play the sound once upon demand
			}

			base.Update(gameTime);
			return;
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);
			
			base.Draw(gameTime);
			return;
		}

		private GraphicsDeviceManager _graphics;
		private SoundEffect _sfx;
		private bool played = false;
	}
}
