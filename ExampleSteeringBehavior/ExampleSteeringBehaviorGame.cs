using GameEngine.Framework;
using GameEngine.GameComponents;
using GameEngine.GUI;
using GameEngine.GUI.Components;
using GameEngine.Input;
using GameEngine.Texture;
using GameEngine.Utility.ExtensionMethods.ClassExtensions;
using GameEngine.Utility.ExtensionMethods.InterfaceFunctions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ExampleSteeringBehavior
{
	/// <summary>
	/// An example of how local decision making can lead to emergent global behavior.
	/// This shows how to steer and 'flock' units as different types of groups.
	/// </summary>
	public class ExampleSteeringBehaviorGame : RenderTargetFriendlyGame
	{
		public ExampleSteeringBehaviorGame()
		{
			Content.RootDirectory = "Content";

			Bounds = new Boundary(0,1000,0,1000);
			Services.AddService(Bounds);

			Graphics.PreferredBackBufferWidth = Bounds.Width + 300;
			Graphics.PreferredBackBufferHeight = Bounds.Height;

			return;
		}

		protected override void LoadContent()
		{
			Renderer = new SpriteBatch(GraphicsDevice);
			
			Input = new InputManager();
			Services.AddService(Input);

			// Escape will allow us to exit
			Input.AddKeyInput("Esc",Keys.Escape);

			// We'll need to know the mouse position for some game modes
			Input.AddMouseAxisInput("MX",true,false);
			Input.AddMouseAxisInput("MY",false,false);

			// Load the environment
			Mountain = new RectangleComponent(this,Renderer,100,100,ColorFunctions.Ellipse(100,100,Color.RosyBrown));
			Mountain.Translate(Bounds.Width / 2.0f - 50.0f,Bounds.Height / 2.0f - 50.0f);
			Mountain.LayerDepth = 1.0f;

			// Load the triangle men
			// The solo triangle man first
			Solo = new TriangleMan(this,Renderer,30,50,Color.Black,3,Color.White);
			Solo.Position = new Vector2(Bounds.Width / 2.0f,Bounds.Height / 2.0f);

			// Load the pursue/evade pair next
			PairFirst = new TriangleMan(this,Renderer,30,50,Color.Black,3,Color.Purple);
			PairFirst.Position = new Vector2(Bounds.Width * 0.75f,Bounds.Height / 2.0f);
			
			PairSecond = new TriangleMan(this,Renderer,30,50,Color.Black,3,Color.Red);
			PairSecond.Position = new Vector2(Bounds.Width * 0.25f,Bounds.Height / 2.0f);
			
			PairFirst.EvasionTarget = PairFirst.PursuitTarget = PairSecond;
			PairSecond.EvasionTarget = PairSecond.PursuitTarget = PairFirst;

			// Create the bounding box for things to have fun in
			BoundingBox = new RectangleComponent(this,Renderer,Bounds.Width + 6,Bounds.Height + 6,ColorFunctions.Wireframe(Bounds.Width + 6,Bounds.Height + 6,3,Color.Black));
			BoundingBox.Translate(-3.0f,-3.0f);
			BoundingBox.LayerDepth = 0.1f;
			
			// Create the background for the menu options
			MenuBackground = new RectangleComponent(this,Renderer,300,Bounds.Height,Color.MonoGameOrange);
			MenuBackground.Translate(Bounds.Width,0.0f);
			MenuBackground.LayerDepth = 0.3f;

			// Create the GUI that will allow us to select our game mode
			GUISystem = new GUICore(this,Renderer,false);
			GUISystem.LayerDepth = 0.2f;

			// The menu is what does the actual game mode selection
			Menu = new RadioButtons(this,"menu",null);
			Menu.AddRadioButton(CreateMenuOption("None","Does nothing."));
			Menu.AddRadioButton(CreateMenuOption("Unnatural Seek","A single triangle will chase after the mouse.\nThis occurs with Aristotelian physics."));
			Menu.AddRadioButton(CreateMenuOption("Broken Natural Seek","A single triangle will chase after the mouse.\nThis occurs with Newtonian physics."));
			Menu.AddRadioButton(CreateMenuOption("Natural Seek","A single triangle will chase after the mouse.\nThis occurs with Newtonian physics.\nWhen it nears the mouse, it will slow down to arrive at its destination."));
			Menu.AddRadioButton(CreateMenuOption("Excellent Seek","A single triangle will chase after the mouse.\nThis occurs with Newtonian physics.\nWhen it nears the mouse, it will slow down to arrive at its destination.\nIt will arrive early and produce a more natural stopping behavior."));
			Menu.AddRadioButton(CreateMenuOption("Flee","A single triangle will flee from the mouse."));
			Menu.AddRadioButton(CreateMenuOption("Seek Wander","A single triangle will wander around the world.\nThe wander method uses a randomly generated point which the triangle man will (excellent) seek."));
			Menu.AddRadioButton(CreateMenuOption("Wander","A single triangle will wander around the world.\nThe wander method uses mooth circle sampling to generate wander forces."));
			Menu.AddRadioButton(CreateMenuOption("Pursue","Two traingles will exist in the game.\nThe first triangle will wander about.\nThe second triangle will pursue the first."));
			Menu.AddRadioButton(CreateMenuOption("Evade","Two traingles will exist in the game.\nThe first triangle will wander about.\nThe second triangle will evade the first."));
			Menu.AddRadioButton(CreateMenuOption("Pursuit & Evasion","Two traingles will exist in the game.\nThe first triangle will pursue the second..\nThe second triangle will evade the first."));
			Menu.AddRadioButton(CreateMenuOption("Seek & Evade","A single triagnle will seek the mouse while avoiding an area."));

			Menu.SelectionChanged += LoadGameMode;
			Menu.Translate(Bounds.Width + 20.0f,20.0f);
			GUISystem.Add(Menu);
			
			// Set the initial button to none AFTER assigning the selection change event so that we can finish loading via it
			// This will also load all of the basic assets into Components to finish loading the game
			Menu.SelectedRadioButton = "None";

			return;
		}

		/// <summary>
		/// Creates a radio button for the menu.
		/// </summary>
		/// <param name="option">The text to display with the radio button.</param>
		/// <param name="tool_tip">The tool tip string to display.</param>
		/// <returns>Returns the new radio button to be added.</returns>
		protected Checkbox CreateMenuOption(string option, string tool_tip)
		{
			const int side_width = 20;
			ComponentLibrary bg = new ComponentLibrary(this);
			
			bg.Add(Checkbox.UncheckedNormalState,new RectangleComponent(this,Renderer,side_width,side_width,ColorFunctions.Wireframe(side_width,side_width,2,new Color(0.5f,0.5f,0.5f))));
			bg.Add(Checkbox.UncheckedHoverState,new RectangleComponent(this,Renderer,side_width,side_width,ColorFunctions.Wireframe(side_width,side_width,2,new Color(0.3f,0.3f,0.3f))));
			bg.Add(Checkbox.UncheckedClickState,new RectangleComponent(this,Renderer,side_width,side_width,ColorFunctions.Wireframe(side_width,side_width,2,new Color(0.2f,0.2f,0.2f))));
			bg.Add(Checkbox.UncheckedDisabledState,new RectangleComponent(this,Renderer,side_width,side_width,ColorFunctions.Wireframe(side_width,side_width,2,new Color(0.75f,0.75f,0.75f))));

			bg.Add(Checkbox.CheckedNormalState,new RectangleComponent(this,Renderer,side_width,side_width,new Color(0.0f,0.5f,0.0f)));
			bg.Add(Checkbox.CheckedHoverState,new RectangleComponent(this,Renderer,side_width,side_width,new Color(0.0f,0.3f,0.0f)));
			bg.Add(Checkbox.CheckedClickState,new RectangleComponent(this,Renderer,side_width,side_width,new Color(0.0f,0.2f,0.0f)));
			bg.Add(Checkbox.CheckedDisabledState,new RectangleComponent(this,Renderer,side_width,side_width,new Color(0.0f,0.75f,0.0f)));

			ComponentGroup? tt;

			if(tool_tip != "")
			{

				TextComponent text = new TextComponent(this,Renderer,Content.Load<SpriteFont>("Times New Roman"),tool_tip,Color.Black);
				text.LayerDepth = 0.01f;
				text.Translate(5.0f,0.0f);

				RectangleComponent text_bg = new RectangleComponent(this,Renderer,text.Width + 10,text.Height,Color.Cornsilk);
				text_bg.LayerDepth = 0.02f;
				
				tt = new ComponentGroup(this,text_bg,text);
				
				text.Parent = text_bg;
				text_bg.Parent = tt;
			}
			else
				tt = null;
			
			return new Checkbox(this,option,bg,new TextComponent(this,Renderer,Content.Load<SpriteFont>("Times New Roman"),option,Color.Black),false,5.0f,false,tt);
		}

		/// <summary>
		/// Loads a game mode based on the current menu selection.
		/// </summary>
		protected void LoadGameMode(RadioButtons sender, string? old_selection, string? new_selection)
		{
			// First empty our components since we don't care about the old game mode anymore
			Components.Clear();

			// Load our basic assets into the game to prepare for the new game mode
			AddBasicAssets();

			// And now figure out what on earth we're doing
			switch(new_selection)
			{
			case "None":
				break;
			case "Unnatural Seek":
				LoadUnnaturalSeeking();
				break;
			case "Broken Natural Seek":
				LoadBrokenNaturalSeeking();
				break;
			case "Natural Seek":
				LoadNaturalSeeking();
				break;
			case "Excellent Seek":
				LoadExcellentSeeking();
				break;
			case "Flee":
				LoadFlee();
				break;
			case "Seek Wander":
				LoadSeekWander();
				break;
			case "Wander":
				LoadWander();
				break;
			case "Pursue":
				LoadPursue();
				break;
			case "Evade":
				LoadEvade();
				break;
			case "Pursuit & Evasion":
				LoadPursuitEvasion();
				break;
			case "Seek & Evade":
				LoadSeekEvade();
				break;
			}
			
			return;
		}

		/// <summary>
		/// Loads the common assets to every game mode.
		/// </summary>
		protected void AddBasicAssets()
		{
			Components.Add(Input); // Input must be added first so everyone can use the most up to date input values
			Components.Add(GUISystem);
			
			Components.Add(BoundingBox);
			Components.Add(MenuBackground);
			
			return;
		}

		protected void LoadUnnaturalSeeking()
		{
			Solo!.Behavior = TriangleManBehavior.UnnaturalSeek;
			Components.Add(Solo);

			return;
		}

		protected void LoadBrokenNaturalSeeking()
		{
			Solo!.Behavior = TriangleManBehavior.BrokenNaturalSeek;
			Components.Add(Solo);

			return;
		}

		protected void LoadNaturalSeeking()
		{
			Solo!.Behavior = TriangleManBehavior.ReasonablyNaturalSeek;
			Components.Add(Solo);

			return;
		}

		protected void LoadExcellentSeeking()
		{
			Solo!.Behavior = TriangleManBehavior.Seek;
			Components.Add(Solo);

			return;
		}

		protected void LoadFlee()
		{
			Solo!.Behavior = TriangleManBehavior.Flee;
			Components.Add(Solo);

			return;
		}

		protected void LoadSeekWander()
		{
			Solo!.Behavior = TriangleManBehavior.SeekWander;
			Components.Add(Solo);

			return;
		}

		protected void LoadWander()
		{
			Solo!.Behavior = TriangleManBehavior.Wander;
			Components.Add(Solo);

			return;
		}

		protected void LoadPursue()
		{
			PairFirst!.Behavior = TriangleManBehavior.Wander;
			Components.Add(PairFirst);

			PairSecond!.Behavior = TriangleManBehavior.Pursuit;
			Components.Add(PairSecond);

			return;
		}

		protected void LoadEvade()
		{
			PairFirst!.Behavior = TriangleManBehavior.Wander;
			Components.Add(PairFirst);

			PairSecond!.Behavior = TriangleManBehavior.Evade;
			Components.Add(PairSecond);

			return;
		}

		protected void LoadPursuitEvasion()
		{
			PairFirst!.Behavior = TriangleManBehavior.Pursuit;
			Components.Add(PairFirst);

			PairSecond!.Behavior = TriangleManBehavior.Evade;
			Components.Add(PairSecond);

			return;
		}

		protected void LoadSeekEvade()
		{
			Solo!.Behavior = TriangleManBehavior.SeekEvade;
			Solo.SeekFleeTargets = new Vector2[] {Mountain!.GetAffinePosition() + Vector2.One * 50.0f};
			Components.Add(Solo);
			
			Components.Add(Mountain);
			return;
		}

		protected override void Update(GameTime delta)
		{
			// If we ever press escape, we're done
			if(Input!["Esc"].CurrentDigitalValue)
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

		// System components
		protected InputManager? Input;
		protected SpriteBatch? Renderer;
		
		protected GUICore? GUISystem;
		protected RadioButtons? Menu;
		protected RectangleComponent? MenuBackground;
		
		protected Boundary Bounds;
		protected RectangleComponent? BoundingBox;

		// Triangle men
		protected TriangleMan? Solo;

		protected TriangleMan? PairFirst;
		protected TriangleMan? PairSecond;

		// Environmental items
		protected RectangleComponent? Mountain;
	}
}
