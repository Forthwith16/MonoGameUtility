using GameEngine.Framework;
using GameEngine.GameObjects;
using GameEngine.Input;
using GameEngine.Input.Bindings.MouseBindings;
using GameEngine.Maths;
using GameEngine.Utility.ExtensionMethods.ClassExtensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace ExampleTouchControls
{
	public class ExampleTouchControlsGame : RenderTargetFriendlyGame
	{
		public ExampleTouchControlsGame() : base()
		{
			Content.RootDirectory = "Content";
			return;
		}

		protected override void LoadContent()
		{
			Renderer = new SpriteBatch(GraphicsDevice);
			
			Components.Add(FakeTouch = new InputManager());
			FakeTouch.UpdateOrder = 0;

			FakeTouch.AddMouseButtonInput("ML",MouseButton.Left);
			FakeTouch.AddMouseAxisInput("MX",true,false);
			FakeTouch.AddMouseAxisInput("MY",false,false);
			FakeTouch.AddMouseAxisInput("MXD",true);
			FakeTouch.AddMouseAxisInput("MYD",false);

			Components.Add(Input = new InputManager());
			Input.UpdateOrder = 1;
			
			// If we don't have a touch device available, we can fake it
			if(!TouchPanel.GetCapabilities().IsConnected)
			{
				// Create some local variables for the fetcher to bind
				TouchLocation? l_prev = null;

				Input.TouchPanelFetch = () =>
				{
					InputRecord l = FakeTouch["ML"];
					TouchLocation[] touches = new TouchLocation[l.CurrentDigitalValue || l.IsFallingEdge ? 1 : 0];
					
					if(l.CurrentDigitalValue)
						if(l.IsRisingEdge) // We touched
							touches[0] = new TouchLocation(0,TouchLocationState.Pressed,new Vector2(FakeTouch["MX"].CurrentAnalogValue,FakeTouch["MY"].CurrentAnalogValue));
						else // We are moving during a touch
							touches[0] = new TouchLocation(0,TouchLocationState.Moved,new Vector2(FakeTouch["MX"].CurrentAnalogValue,FakeTouch["MY"].CurrentAnalogValue),l_prev!.Value.State,l_prev.Value.Position);
					else if(l.IsFallingEdge) // We released our touch
						touches[0] = new TouchLocation(0,TouchLocationState.Released,new Vector2(FakeTouch["MX"].CurrentAnalogValue,FakeTouch["MY"].CurrentAnalogValue),l_prev!.Value.State,l_prev.Value.Position);

					l_prev = touches.Length > 0 ? touches[0] : null;
					return new TouchCollection(touches);
				};
			}

			Input.AddKeyInput("Escape",Keys.Escape);
			Input.AddEdgeTriggeredInput("Exit","Escape",true);

			Input.AddTouchPressInput("TouchP",0);
			Input.AddTouchPressInput("TouchR",0,false);
			Input.AddTouchAxisInput("TouchX",0,true,false);
			Input.AddTouchAxisInput("TouchY",0,false,false);

			Components.Add(TouchMe = new RectangleGameObject(Renderer,TouchBoxSize,TouchBoxSize,Color.Red));
			return;
		}

		protected override void Update(GameTime delta)
		{
			base.Update(delta);

			if(Input!["Exit"].CurrentDigitalValue)
				Exit();

			if(Input["TouchR"].CurrentDigitalValue)
				pressed = false;

			if(Input["TouchP"].CurrentDigitalValue || pressed)
			{
				TouchMe!.Transform = Matrix2D.Translation(Input["TouchX"].CurrentAnalogValue - TouchBoxSize / 2,Input["TouchY"].CurrentAnalogValue - TouchBoxSize / 2);
				pressed = true;
			}
			
			return;
		}

		protected override void PreDraw(GameTime delta)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);
			Renderer!.Begin(SpriteSortMode.BackToFront,BlendState.NonPremultiplied,SamplerState.PointClamp);

			return;
		}

		protected override void PostDraw(GameTime delta)
		{
			Renderer!.End();
			return;
		}

		private SpriteBatch? Renderer;
		private InputManager? Input;
		private InputManager? FakeTouch;

		private const int TouchBoxSize = 100;
		private RectangleGameObject? TouchMe;
		private bool pressed = false;
	}
}
