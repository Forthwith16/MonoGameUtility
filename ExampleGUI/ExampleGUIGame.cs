using GameEngine.Framework;
using GameEngine.GameObjects;
using GameEngine.GUI;
using GameEngine.GUI.Components;
using GameEngine.GUI.Map;
using GameEngine.Input;
using GameEngine.Sprites;
using GameEngine.Utility.ExtensionMethods.ClassExtensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ExampleGUI
{
	/// <summary>
	/// This game demonstrates how to use the GUICore and add GUI elements to the game.
	/// There is a lot to dig into with the GUI system, but the basics are fairly approachable.
	/// </summary>
	public class ExampleGUIGame : RenderTargetFriendlyGame
	{
		public ExampleGUIGame() : base()
		{
			Content.RootDirectory = "Content";

			Graphics.PreferredBackBufferWidth = 500;
			Graphics.PreferredBackBufferHeight = 500;

			return;
		}

		protected override void PreInitialize()
		{
			Components.Add(Input = new InputManager());
			
			Input.AddKeyInput("_E",Keys.Escape);
			Input.AddEdgeTriggeredInput("Back","_E",true);

			Services.AddService(new GUICore(null));
			Components.Add(Services.GetService<GUICore>());

			return;
		}

		protected override void LoadContent()
		{
			// To get a service out of Services, you just call GetService with the type you want out
			Menu = Services.GetService<GUICore>();
			Menu.Renderer = Renderer = new SpriteRenderer(GraphicsDevice);

			// Let's add four buttons
			Button tl = CreateButton("Top Left",new Vector2(100.0f,100.0f),"TL");
			Button tr = CreateButton("Top Right",new Vector2(300.0f,100.0f),"TR");
			Button bl = CreateButton("Bottom Left",new Vector2(100.0f,300.0f),"BL");
			Button br = CreateButton("Bottom Right",new Vector2(300.0f,300.0f),"BR");

			// See the CreateButton documentation for how exactly this works
			Menu.Add(tl);
			Menu.Add(tr);
			Menu.Add(bl);
			Menu.Add(br);

			// We can navigate through GUI menus digitally if we permit it
			// By default, you can use the arrow keys to do this (and the enter key 'clicks' on the button)
			// You can change the bindings in GameEngine's Framework.GlobalConstants class
			Menu.EnableDigital = true; // This value defaults to true, but you can set it to false to disable digital navigation
			Menu.EnableMouse = true; // This value defaults to true, but you can set it to false to disable mouse navigation

			Menu.ConnectComponents(tl,tr,GUIMapDirection.RIGHT,true); // The last optional parameter set to true will mirror the connection in the opposite direction
			Menu.ConnectComponents(bl,br,GUIMapDirection.RIGHT,true);
			Menu.ConnectComponents(tl,bl,GUIMapDirection.DOWN,true);
			Menu.ConnectComponents(tr,br,GUIMapDirection.DOWN,true);

			// We also want some way to navigation from no previous focused component
			// While the mouse could temporarily focus a component and then digital navigation could pick up from there, we want a way to navigate entirely digitally
			// Thus we define the entry point for digital navigation in any (or perhaps all) of the cardinal directions
			Menu.ConnectComponents(null,tl,GUIMapDirection.DOWN); // A null component to another component allows us to navigate from no previous GUI element to this one via the down key

			return;
		}

		/// <summary>
		/// Creates a new button.
		/// </summary>
		/// <param name="name">The name the button should have.</param>
		/// <param name="position">The position of the button.</param>
		/// <param name="text">The text that should be displayed on the button. If this is null, no text will be displayed.</param>
		/// <param name="w">The width of the button.</param>
		/// <param name="h">The height of the button.</param>
		/// <returns>Returns the new button created.</returns>
		protected Button CreateButton(string name, Vector2 position, string? text = null, int w = 100, int h = 100)
		{
			// The button requires a drawable component to display for each of its states
			// These can all be different or all the same, depending on context
			GameObjectLibrary lib = new GameObjectLibrary();

			// The disabled state is when the button is disabled and unable to be interacted with (but still visible)
			lib.Add(Button.DisabledState,new RectangleGameObject(null,w,h,Color.Gray));

			// The normal state is the state the button is in whenever it is not in another state
			lib.Add(Button.NormalState,new RectangleGameObject(null,w,h,Color.Blue));

			// The hover state is when the mouse (or digital input) is hovered over the button but not clicked
			lib.Add(Button.HoverState,new RectangleGameObject(null,w,h,Color.Green));

			// The click state is when the button is begin clicked (by the mouse or digitally)
			lib.Add(Button.ClickState,new RectangleGameObject(null,w,h,Color.Pink));
			
			// We create a button
			// The TextComponent we provide to draw the button's text doesn't need a SpriteRenderer to render it
			// The GUICore will assign that itself
			Button b = new Button(name,lib,text is null ? null : new TextGameObject(null,Content.Load<SpriteFont>("Times New Roman"),text,Color.Black));
			b.Transform = b.Transform.Translate(position);
			
			return b;
		}

		protected override void Update(GameTime delta)
		{
			if(Input!["Back"].CurrentDigitalValue)
				Exit();

			base.Update(delta);
			return;
		}

		protected override void PreDraw(GameTime delta)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);
			Renderer!.Begin(SpriteSortMode.BackToFront,BlendState.NonPremultiplied,SamplerState.LinearClamp,null,null,null,null);

			return;
		}

		protected override void PostDraw(GameTime delta)
		{
			Renderer!.End();
			return;
		}

		protected SpriteRenderer? Renderer;
		protected InputManager? Input;
		protected GUICore? Menu;
	}
}
