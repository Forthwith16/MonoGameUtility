using GameEngine.Framework;
using GameEngine.GameObjects;
using GameEngine.Input;
using GameEngine.Sprites;
using GameEngine.Utility.ExtensionMethods.ClassExtensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ExampleInputManager
{
	/// <summary>
	/// This game demonstrates how to use the basics of the InputManager class for input processing.
	/// It also demonstrates the use (or at least existence) of Services.
	/// </summary>
	public class ExampleInputManagerGame : RenderTargetFriendlyGame
	{
		public ExampleInputManagerGame()
		{
			Content.RootDirectory = "Content";
			digital = true;

			return;
		}

		protected override void PreInitialize()
		{
			// Create a new InputManager and add it to our Components
			// This will automatically call its Update method every frame
			Components.Add(Input = new InputManager());
			
			// We will utilize extension methods found in InputManagerExtensions to add key bindings to the InputManager for us
			// Here, the first default parameter ensures that when we hold Escape down, it only triggers the binding once
			// In other words, it looks for when the binding _E is first satisfied (i.e. it looks for a rising edge) and cannot be satisfied again until _E becomes unsatisfied
			Input.AddKeyInput("_E",Keys.Escape);
			Input.AddEdgeTriggeredInput("Back","_E",true);

			// The following key bindings just check to see if the key is pressed, so we can hold the key and expect the binding to trigger continuously
			Input.AddKeyInput("U",Keys.Up);
			Input.AddKeyInput("D",Keys.Down);
			Input.AddKeyInput("L",Keys.Left);
			Input.AddKeyInput("R",Keys.Right);

			// We can also bind axes (as well as other things) to our game
			// Below, we bind the x-axis of the left joystick of the first controller
			// It's analog value is always ranges within [-1,1] and its digital value is mapped to true iff the magnitude of its analog value is at least 0.5f
			// One could, for example, map player movement to walking if the digital value is false and running if the digital value is true
			Input.AddGamePadAxisInput("X",PlayerIndex.One,true,true,0.5f);

			// Below, we bind the y-axis of the left joystick of the first controller
			// Here, we insist that its analog value is always 0 unless it evaluates digitally to true
			// To continue the example above, this would disable walking in the y direction but permit running in it
			// We could change the last parameter to false instead to do the opposite (permit walking but not running)
			Input.AddGamePadAxisDeadZoneInput("_Y",PlayerIndex.One,true,false,0.5f,true);
			Input.AddInvertedInput("Y","_Y"); // Remember that up is down and down is up in screen coordiantes, so when we invert the input, we actually make up up and down down
			
			// We will swap back and forth between analog and digital input for this example game with the space bar
			Input.AddKeyInput("A",Keys.Space);
			
			// A Game's Services is a useful place to store common utilities (such as an InputManager) that you don't want to have to pass to every GameObject
			// This makes it easy for many GameObjects to reference one location for the current Service
			// For instance, you might want to swap out InputManagers to change how the controller works (think Type A vs Type B controls on a console game)
			// Services can hold exactly one object of every type inside of it
			Services.AddService<InputManager>(Input);

			return;
		}

		protected override void LoadContent()
		{
			_spriteBatch = new SpriteRenderer(this);

			_image = new ImageGameObject(_spriteBatch,"Pokeball");
			Components.Add(_image);

			return;
		}

		protected override void Update(GameTime delta)
		{
			// Checking an input binding's CurrentDigitalValue will tell us if it is currently satisfied
			// It evaluates every input exactly once per frame and stores information about it so that it only needs to be evaluated once
			// When a large number of queries need to be made per frame, this will save time overall
			// When only a small number of queries need to be made per frame, this may be somewhat overkill
			if(Input["Back"].CurrentDigitalValue)
				Exit();

			// Toggle digital/analog controls after pressing the space bar
			if(Input["A"].IsRisingEdge) // We can also check for rising edges directly like this
				digital = !digital;

			// Create an adjustable speed parameter for convenience
			float speed = 100.0f * (float)delta.ElapsedGameTime.TotalSeconds; // We currently move 100 pixels per second

			// The pokeball must dance to our whims
			if(digital)
			{
				if(Input["L"].CurrentDigitalValue)
					_image.Transform = _image.Transform.Translate(-speed,0.0f);

				if(Input["R"].CurrentDigitalValue)
					_image.Transform = _image.Transform.Translate(speed,0.0f);

				if(Input["U"].CurrentDigitalValue)
					_image.Transform = _image.Transform.Translate(0.0f,-speed);

				if(Input["D"].CurrentDigitalValue)
					_image.Transform = _image.Transform.Translate(0.0f,speed);
			}
			else // Analog controls
				_image.Transform = _image.Transform.Translate(speed * Input["X"].CurrentAnalogValue,speed * Input["Y"].CurrentAnalogValue);

			base.Update(delta);
			return;
		}

		protected override void PreDraw(GameTime delta)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);
			_spriteBatch!.Begin(SpriteSortMode.BackToFront,BlendState.NonPremultiplied,SamplerState.LinearWrap,null,null,null,null);
			
			return;
		}

		protected override void PostDraw(GameTime delta)
		{
			_spriteBatch!.End();
			return;
		}
		
		private SpriteRenderer _spriteBatch;
		private ImageGameObject _image;
		
		protected InputManager Input;
		protected bool digital;
	}
}
