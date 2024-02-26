using GameEngine.Framework;
using GameEngine.GameComponents;
using GameEngine.GUI;
using GameEngine.GUI.Components;
using GameEngine.Input;
using GameEngine.Utility.ExtensionMethods.ClassExtensions;
using GameEngine.Utility.ExtensionMethods.InterfaceFunctions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace ExampleShaders
{
	/// <summary>
	/// Shows how to use shaders, which are known as Effects in MonoGame.
	/// Included are several shaders that have various effects.
	/// </summary>
	public class ExampleShadersGame : RenderTargetFriendlyGame
	{
		public ExampleShadersGame()
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
			AspectRatio = GraphicsDevice.Viewport.AspectRatio;
			
			Input = new InputManager();
			Services.AddService(Input);

			// Add our key bindings
			Input.AddKeyInput("Exit",Keys.Escape);

			Input.AddKeyInput("U",Keys.Up);
			Input.AddKeyInput("D",Keys.Down);
			Input.AddKeyInput("L",Keys.Left);
			Input.AddKeyInput("R",Keys.Right);

			Input.AddKeyInput("RU",Keys.W);
			Input.AddKeyInput("RD",Keys.S);
			Input.AddKeyInput("RL",Keys.A);
			Input.AddKeyInput("RR",Keys.D);
			Input.AddKeyInput("RZL",Keys.Q);
			Input.AddKeyInput("RZR",Keys.E);

			// This will ensure the mouse is drawn on top of models
			Mouse!.DepthRecord = DepthStencilState.Default;
			Mouse.Blend = BlendState.NonPremultiplied;

			// Load some models
			doughnut = new SimpleModel(this,Content.Load<Model>("Models/doughnut"));
			doughnut.View = Matrix.CreateLookAt(new Vector3(0.0f,5.0f,-10.0f),Vector3.Zero,Vector3.Up);
			doughnut.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f),AspectRatio,1.0f,1000.0f);

			dragon = new SimpleModel(this,Content.Load<Model>("Models/dragon/dragon"));
			dragon.View = Matrix.CreateLookAt(new Vector3(0.0f,5.0f,-10.0f),Vector3.Zero,Vector3.Up);
			dragon.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f),AspectRatio,1.0f,1000.0f);

			mimikyu = new SimpleModel(this,Content.Load<Model>("Models/mimikyu"));
			mimikyu.View = Matrix.CreateLookAt(new Vector3(0.0f,5.0f,-10.0f),Vector3.Zero,Vector3.Up);
			mimikyu.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f),AspectRatio,1.0f,1000.0f);

			// Now load some shaders
			AmbientEffect = Content.Load<Effect>("Shaders/Ambient");

			// Create the background for the menu options
			MenuBackground = new RectangleComponent(this,Renderer,300,Bounds.Height,Color.MonoGameOrange);
			MenuBackground.Translate(Bounds.Width,0.0f);
			MenuBackground.LayerDepth = 0.2f;

			// Create the GUI that will allow us to select our game mode
			GUISystem = new GUICore(this,Renderer,false);
			GUISystem.LayerDepth = 0.3f;

			// The menu is what does the actual game mode selection
			Menu = new RadioButtons(this,"menu",null);
			Menu.AddRadioButton(CreateMenuOption("Default","Models in MonoGame have a default shader known as a BasicEffect.\nIt will perform most every basic common task, including lighting, texture mapping, fog, etc...\nYou will want to use custom shaders for specialized tasks and improvements over the basics."));
			Menu.AddRadioButton(CreateMenuOption("Ambient Light Shader","Uses a shader that applies only ambient light to the model.\nThis light source comes from nowhere and everywhere.\nOmnipresent, it applies lighting equally to all fragments."));
			

			Menu.SelectionChanged += LoadGameMode;
			Menu.Translate(Bounds.Width + 20.0f,20.0f);
			GUISystem.Add(Menu);
			
			// Now create the sliders we'll need intermitently
			R = CreateSlider("Red",new Color(0.75f,0.0f,0.0f,1.0f),"The red value of a custom light color.");
			R.Translate(Bounds.Width + 20.0f,Bounds.Height - 20.0f * 5);
			GUISystem.Add(R);

			G = CreateSlider("Green",new Color(0.0f,0.75f,0.0f,1.0f),"The green value of a custom light color.");
			G.Translate(Bounds.Width + 20.0f,Bounds.Height - 20.0f * 4);
			GUISystem.Add(G);

			B = CreateSlider("Blue",new Color(0.0f,0.0f,0.75f,1.0f),"The blue value of a custom light color.");
			B.Translate(Bounds.Width + 20.0f,Bounds.Height - 20.0f * 3);
			GUISystem.Add(B);

			Intensity = CreateSlider("Intensity",new Color(0.75f,0.75f,0.75f,1.0f),"The intensity of a light source.");
			Intensity.Translate(Bounds.Width + 20.0f,Bounds.Height - 20.0f * 2);
			GUISystem.Add(Intensity);

			// Set the initial button to none AFTER assigning the selection change event so that we can finish loading via it
			Menu.SelectedRadioButton = "Default";

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
				TextComponent text = new TextComponent(this,Renderer,Content.Load<SpriteFont>("Fonts/Times New Roman"),tool_tip,Color.Black);
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
			
			return new Checkbox(this,option,bg,new TextComponent(this,Renderer,Content.Load<SpriteFont>("Fonts/Times New Roman"),option,Color.Black),false,5.0f,false,tt);
		}

		/// <summary>
		/// Creates a new slider for the menu.
		/// </summary>
		/// <param name="name">The name of the slider.</param>
		/// <param name="base_color">The base color for knobs.</param>
		/// <param name="tool_tip">The tool tip to display.</param>
		/// <returns>Returns a new slider.</returns>
		protected Slider CreateSlider(string name, Color base_color, string tool_tip)
		{
			const int slider_width = 260;
			
			ComponentLibrary bar = new ComponentLibrary(this);

			bar.Add(Slider.NormalState,new RectangleComponent(this,Renderer,slider_width,10,Color.White));
			bar.Add(Slider.HoverState,new RectangleComponent(this,Renderer,slider_width,10,Color.White));
			bar.Add(Slider.ClickState,new RectangleComponent(this,Renderer,slider_width,10,Color.White));
			bar.Add(Slider.DisabledState,new RectangleComponent(this,Renderer,slider_width,10,Color.LightGray));

			ComponentLibrary knob = new ComponentLibrary(this);

			knob.Add(Slider.NormalState,new RectangleComponent(this,Renderer,15,15,base_color));
			knob.Add(Slider.HoverState,new RectangleComponent(this,Renderer,15,15,base_color * 1.2f));
			knob.Add(Slider.ClickState,new RectangleComponent(this,Renderer,15,15,base_color * 1.4f));
			knob.Add(Slider.DisabledState,new RectangleComponent(this,Renderer,15,15,base_color * 0.5f));

			ComponentGroup? tt;

			if(tool_tip != "")
			{
				TextComponent text = new TextComponent(this,Renderer,Content.Load<SpriteFont>("Fonts/Times New Roman"),tool_tip,Color.Black);
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

			return new Slider(this,name,bar,knob,0,255,tt);
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
			case "Default":
				loaded = dragon!;
				loaded.UseBasicEffect = true;

				R!.Visible = false;
				G!.Visible = false;
				B!.Visible = false;
				Intensity!.Visible = false;

				break;
			case "Ambient Light Shader":
				loaded = dragon!;
				
				R!.Visible = true;
				G!.Visible = true;
				B!.Visible = true;
				Intensity!.Visible = true;

				loaded.UseBasicEffect = false;
				loaded.Shader = AmbientEffect;
				loaded.ShaderParameterization = (model,mesh,part,effect) =>
				{
					// We need to set the uniform values of our shader here
					effect.Parameters["World"].SetValue(model.World);// * mesh.ParentBone.Transform); // We're supposed to use the mesh bone here, but it seems that we don't actually want that here for some reason
					effect.Parameters["View"].SetValue(model.View);
					effect.Parameters["Projection"].SetValue(model.Projection);
					
					effect.Parameters["AmbientColor"].SetValue(new Color(R.SliderValue,G.SliderValue,B.SliderValue,255).ToVector4());
					effect.Parameters["AmbientIntensity"].SetValue(Intensity.SliderPosition);

					return;
				};

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

		protected override void Update(GameTime delta)
		{
			if(Input!["Exit"].CurrentDigitalValue)
				Exit();

			// We'll use this a lot, so just do the cast once and give it a name
			float deltat = (float)delta.ElapsedGameTime.TotalSeconds;

			// Have a common speed
			float speed = 2.0f;

			// Move the model if we desire
			if(Input["L"].CurrentDigitalValue)
				pos += speed * Vector3.UnitX * deltat;

			if(Input["R"].CurrentDigitalValue)
				pos -= speed * Vector3.UnitX * deltat;

			if(Input["U"].CurrentDigitalValue)
				pos += speed * Vector3.UnitZ * deltat;

			if(Input["D"].CurrentDigitalValue)
				pos -= speed * Vector3.UnitZ * deltat;

			// Have a common rotation speed
			float rspeed = 1.0f;

			// Also rotate the model if we desire
			if(Input["RL"].CurrentDigitalValue)
			{
				y_rot = (y_rot - rspeed * deltat);

				if(y_rot < 0.0f)
					y_rot += MathF.Tau;
			}

			if(Input["RR"].CurrentDigitalValue)
				y_rot = (y_rot + rspeed * deltat) % MathF.Tau;

			if(Input["RU"].CurrentDigitalValue)
				x_rot = (x_rot + rspeed * deltat) % MathF.Tau;

			if(Input["RD"].CurrentDigitalValue)
			{
				x_rot = (x_rot - rspeed * deltat);

				if(x_rot < 0.0f)
					x_rot += MathF.Tau;
			}

			if(Input["RZL"].CurrentDigitalValue)
				z_rot = (z_rot + rspeed * deltat) % MathF.Tau;

			if(Input["RZR"].CurrentDigitalValue)
			{
				z_rot = (z_rot - rspeed * deltat);

				if(z_rot < 0.0f)
					z_rot += MathF.Tau;
			}

			base.Update(delta);
			return;
		}

		protected override void PreDraw(GameTime delta)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);
			
			// We do not have a 2D/3D split yet in our base class, so we need to do all 3D here before we draw our 2D stuff so that we have the right rendering properties and so it gets drawn underneath
			GraphicsDevice.DepthStencilState = DepthStencilState.Default;
			GraphicsDevice.BlendState = BlendState.Opaque;
			GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

			// Draw our 3D model, whatever that is
			if(loaded is not null)
			{
				loaded.World = Matrix.CreateRotationZ(z_rot) * Matrix.CreateRotationY(y_rot) * Matrix.CreateRotationX(x_rot) * Matrix.CreateTranslation(pos);
				loaded.Draw(delta);
			}

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

		protected Slider? R;
		protected Slider? G;
		protected Slider? B;
		protected Slider? Intensity;
		
		protected Boundary Bounds;
		protected RectangleComponent? BoundingBox;

		protected float AspectRatio;

		// Models
		protected SimpleModel? loaded;
		protected Vector3 pos = new Vector3(1.5f,0.0f,0.0f);
		protected float x_rot = 0.0f;
		protected float y_rot = 0.0f;
		protected float z_rot = 0.0f;

		protected SimpleModel? doughnut;
		protected SimpleModel? dragon;
		protected SimpleModel? mimikyu;

		// Shaders
		protected Effect? AmbientEffect;

	}
}
