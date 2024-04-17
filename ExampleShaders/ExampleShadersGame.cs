using GameEngine.Framework;
using GameEngine.GameComponents;
using GameEngine.GUI;
using GameEngine.GUI.Components;
using GameEngine.Input;
using GameEngine.Texture;
using GameEngine.Utility.ExtensionMethods.ClassExtensions;
using GameEngine.Utility.ExtensionMethods.InterfaceFunctions;
using GameEngine.Utility.ExtensionMethods.PrimitiveExtensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace ExampleShaders
{
	/// <summary>
	/// Shows how to use shaders, which are known as Effects in MonoGame.
	/// Included are several shaders that have various effects.
	/// <para/>
	/// WASD + QE will rotate the model.
	/// <para/>
	/// The arrow keys will move the model.
	/// <para/>
	/// IJKL + UO will move the look at target.
	/// <para/>
	/// Escape closes the program.
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

			Input.AddKeyInput("IT",Keys.I);
			Input.AddKeyInput("KT",Keys.K);
			Input.AddKeyInput("JT",Keys.J);
			Input.AddKeyInput("LT",Keys.L);
			Input.AddKeyInput("OT",Keys.O);
			Input.AddKeyInput("UT",Keys.U);

			// This will ensure the mouse is drawn on top of models
			Mouse!.DepthRecord = DepthStencilState.Default;
			Mouse.Blend = BlendState.NonPremultiplied;

			// Load some models
			doughnut = new SimpleModel(this,Content.Load<Model>("Models/doughnut"));
			doughnut.View = Matrix.CreateLookAt(new Vector3(0.0f,5.0f,-10.0f),target,Vector3.Up);
			doughnut.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f),AspectRatio,1.0f,1000.0f);

			dragon = new SimpleModel(this,Content.Load<Model>("Models/dragon/dragon"));
			dragon.View = Matrix.CreateLookAt(new Vector3(0.0f,5.0f,-10.0f),target,Vector3.Up);
			dragon.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f),AspectRatio,1.0f,1000.0f);
			dragon_texture = Content.Load<Texture2D>("Models/dragon/Difuse Dragao");

			mimikyu = new SimpleModel(this,Content.Load<Model>("Models/mimikyu"));
			mimikyu.View = Matrix.CreateLookAt(new Vector3(0.0f,5.0f,-10.0f),target,Vector3.Up);
			mimikyu.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f),AspectRatio,1.0f,1000.0f);

			Skybox = new SimpleModel(this,Content.Load<Model>("Models/cube"));
			SkyboxTexture = Content.Load<TextureCube>("Skyboxes/Sunset");
			Skybox.World = Matrix.CreateScale(500.0f);
			Skybox.View = Matrix.CreateLookAt(new Vector3(0.0f,5.0f,-10.0f),target,Vector3.Up);
			Skybox.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f),AspectRatio,1.0f,1000.0f);
			Skybox.Shader = SkyboxEffect = Content.Load<Effect>("Shaders/Skybox");
			Skybox.UseBasicEffect = false;
			Skybox.ShaderParameterization = (model,mesh,part,effect) =>
			{
				effect.Parameters["World"].SetValue(model.World);
				effect.Parameters["View"].SetValue(model.View);
				effect.Parameters["Projection"].SetValue(model.Projection);

				effect.Parameters["CameraPosition"].SetValue(new Vector3(0.0f,5.0f,-10.0f));

				effect.Parameters["SkyboxTexture"].SetValue(SkyboxTexture);
				return;
			};

			// Now load some shaders
			AmbientEffect = Content.Load<Effect>("Shaders/Ambient");
			DiffuseEffect = Content.Load<Effect>("Shaders/Diffuse");
			PixelDiffuseEffect = Content.Load<Effect>("Shaders/PixelDiffuse");
			SpecularEffect = Content.Load<Effect>("Shaders/Specular");
			PhongEffect = Content.Load<Effect>("Shaders/Phong");
			TextureEffect = Content.Load<Effect>("Shaders/Texture");
			TextureLightingEffect = Content.Load<Effect>("Shaders/TextureLighting");

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
			Menu.AddRadioButton(CreateMenuOption("Ambient","Uses a shader that applies only ambient light to the model.\nThis light source comes from nowhere and everywhere.\nOmnipresent, it applies lighting equally to all fragments."));
			Menu.AddRadioButton(CreateMenuOption("Diffuse","Uses a shader that applies diffuse and ambient light to the model.\nThe diffuse light comes from a directional light.\n Light comes from 'infinitely far away' in that direction."));
			Menu.AddRadioButton(CreateMenuOption("Pixel Diffuse","Uses a shader that applies diffuse and ambient light to the model in the pixel shader for superior results.\nThe diffuse light comes from a directional light.\n Light comes from 'infinitely far away' in that direction."));
			Menu.AddRadioButton(CreateMenuOption("Specular","Uses a shader that applies specular and ambient light to the model.\nThe light source comes from a directional light."));
			Menu.AddRadioButton(CreateMenuOption("Phong","Uses a shader that applies full phong lighting to the model.\nThis includes ambient light, diffuse light, and specular light.\nThe light is obtained from a single directional light."));
			Menu.AddRadioButton(CreateMenuOption("Texture","A shader that simply copy-pastes a texture onto its model.\nNo lighting is applied."));
			Menu.AddRadioButton(CreateMenuOption("Texture Lighting","A shader that applies texture color data to phong lighting."));

			Menu.SelectionChanged += LoadGameMode;
			Menu.Translate(Bounds.Width + 20.0f,20.0f);
			GUISystem.Add(Menu);

			// Now create the sliders we'll need intermitently
			// The color picker sliders
			#region RBGI Selector
			R = CreateSlider("Red",new Color(0.75f,0.0f,0.0f,1.0f),0,255,"The red value of a custom light color.");
			R.Translate(Bounds.Width + 20.0f,Bounds.Height - 20.0f * 5);
			GUISystem.Add(R);

			G = CreateSlider("Green",new Color(0.0f,0.75f,0.0f,1.0f),0,255,"The green value of a custom light color.");
			G.Translate(Bounds.Width + 20.0f,Bounds.Height - 20.0f * 4);
			GUISystem.Add(G);

			B = CreateSlider("Blue",new Color(0.0f,0.0f,0.75f,1.0f),0,255,"The blue value of a custom light color.");
			B.Translate(Bounds.Width + 20.0f,Bounds.Height - 20.0f * 3);
			GUISystem.Add(B);

			Intensity = CreateSlider("Intensity",new Color(0.75f,0.75f,0.75f,1.0f),0,255,"The intensity of a light source.");
			Intensity.Translate(Bounds.Width + 20.0f,Bounds.Height - 20.0f * 2);
			GUISystem.Add(Intensity);
			#endregion

			// The direction sliders
			#region Direction Selector
			X = CreateSlider("X",new Color(0.75f,0.0f,0.0f,1.0f),-1000,1000,"The x component of the directional light.");
			X.Translate(Bounds.Width + 20.0f,Bounds.Height - 20.0f * 9);
			GUISystem.Add(X);

			Y = CreateSlider("Y",new Color(0.0f,0.75f,0.0f,1.0f),-1000,1000,"The y component of the directional light.");
			Y.Translate(Bounds.Width + 20.0f,Bounds.Height - 20.0f * 8);
			GUISystem.Add(Y);

			Z = CreateSlider("Z",new Color(0.0f,0.0f,0.75f,1.0f),-1000,1000,"The z component of the directional light.");
			Z.Translate(Bounds.Width + 20.0f,Bounds.Height - 20.0f * 7);
			GUISystem.Add(Z);
			#endregion

			// Misc selectors
			Shininess = CreateSlider("Shiny",new Color(0.0f,0.0f,0.75f,1.0f),0,999,"The degree of shininess of the object.");
			Shininess.Translate(Bounds.Width + 20.0f,Bounds.Height - 20.0f * 11);
			GUISystem.Add(Shininess);

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
		protected Slider CreateSlider(string name, Color base_color, int min, int max, string tool_tip)
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

			return new Slider(this,name,bar,knob,min,max,tt);
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
				LoadDefault();
				break;
			case "Ambient":
				LoadAmbient();
				break;
			case "Diffuse":
				LoadDiffuse();
				break;
			case "Pixel Diffuse":
				LoadPixelDiffuse();
				break;
			case "Specular":
				LoadSpecular();
				break;
			case "Phong":
				LoadPhong();
				break;
			case "Texture":
				LoadTexture();
				break;
			case "Texture Lighting":
				LoadTextureLighting();
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

		protected void LoadDefault()
		{
			loaded = dragon!;
			loaded.UseBasicEffect = true;

			R!.Visible = false;
			G!.Visible = false;
			B!.Visible = false;
			Intensity!.Visible = false;

			X!.Visible = false;
			Y!.Visible = false;
			Z!.Visible = false;

			Shininess!.Visible = false;
			return;
		}

		protected void LoadAmbient()
		{
			loaded = dragon!;
			
			R!.Visible = true;
			G!.Visible = true;
			B!.Visible = true;
			Intensity!.Visible = true;

			X!.Visible = false;
			Y!.Visible = false;
			Z!.Visible = false;
			
			Shininess!.Visible = false;

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

			return;
		}

		protected void LoadDiffuse()
		{
			loaded = dragon!;
			
			R!.Visible = true;
			G!.Visible = true;
			B!.Visible = true;
			Intensity!.Visible = true;

			X!.Visible = true;
			Y!.Visible = true;
			Z!.Visible = true;
			
			Shininess!.Visible = false;

			loaded.UseBasicEffect = false;
			loaded.Shader = DiffuseEffect;
			loaded.ShaderParameterization = (model,mesh,part,effect) =>
			{
				// We need to set the uniform values of our shader here
				effect.Parameters["World"].SetValue(model.World);// * mesh.ParentBone.Transform); // We're supposed to use the mesh bone here, but it seems that we don't actually want that here for some reason
				effect.Parameters["View"].SetValue(model.View);
				effect.Parameters["Projection"].SetValue(model.Projection);

				effect.Parameters["NormalMatrix"].SetValue(Matrix.Transpose(Matrix.Invert(model.World))); // We should be using the parent bone's transform here too, but apparently not, I guess
					
				effect.Parameters["AmbientColor"].SetValue(Color.White.ToVector4());
				effect.Parameters["AmbientIntensity"].SetValue(0.1f);

				Vector3 dir = new Vector3(X.SliderValue,Y.SliderValue,Z.SliderValue).Normalized();

				effect.Parameters["LightDirection"].SetValue(dir == Vector3.Zero ? Vector3.Left : dir);
				effect.Parameters["LightColor"].SetValue(new Color(R.SliderValue,G.SliderValue,B.SliderValue,255).ToVector4());
				effect.Parameters["LightIntensity"].SetValue(Intensity.SliderPosition);

				return;
			};

			return;
		}

		protected void LoadPixelDiffuse()
		{
			loaded = dragon!;
			
			R!.Visible = true;
			G!.Visible = true;
			B!.Visible = true;
			Intensity!.Visible = true;

			X!.Visible = true;
			Y!.Visible = true;
			Z!.Visible = true;
			
			Shininess!.Visible = false;

			loaded.UseBasicEffect = false;
			loaded.Shader = PixelDiffuseEffect;
			loaded.ShaderParameterization = (model,mesh,part,effect) =>
			{
				// We need to set the uniform values of our shader here
				effect.Parameters["World"].SetValue(model.World);// * mesh.ParentBone.Transform); // We're supposed to use the mesh bone here, but it seems that we don't actually want that here for some reason
				effect.Parameters["View"].SetValue(model.View);
				effect.Parameters["Projection"].SetValue(model.Projection);

				effect.Parameters["NormalMatrix"].SetValue(Matrix.Transpose(Matrix.Invert(model.World))); // We should be using the parent bone's transform here too, but apparently not, I guess
					
				effect.Parameters["AmbientColor"].SetValue(Color.White.ToVector4());
				effect.Parameters["AmbientIntensity"].SetValue(0.1f);

				Vector3 dir = new Vector3(X.SliderValue,Y.SliderValue,Z.SliderValue).Normalized();

				effect.Parameters["LightDirection"].SetValue(dir == Vector3.Zero ? Vector3.Left : dir);
				effect.Parameters["LightColor"].SetValue(new Color(R.SliderValue,G.SliderValue,B.SliderValue,255).ToVector4());
				effect.Parameters["LightIntensity"].SetValue(Intensity.SliderPosition);

				return;
			};

			return;
		}

		protected void LoadSpecular()
		{
			loaded = dragon!;
			
			R!.Visible = true;
			G!.Visible = true;
			B!.Visible = true;
			Intensity!.Visible = true;

			X!.Visible = true;
			Y!.Visible = true;
			Z!.Visible = true;

			Shininess!.Visible = true;

			loaded.UseBasicEffect = false;
			loaded.Shader = SpecularEffect;
			loaded.ShaderParameterization = (model,mesh,part,effect) =>
			{
				effect.Parameters["World"].SetValue(model.World);
				effect.Parameters["View"].SetValue(model.View);
				effect.Parameters["Projection"].SetValue(model.Projection);

				effect.Parameters["NormalMatrix"].SetValue(Matrix.Transpose(Matrix.Invert(model.World)));
					
				effect.Parameters["AmbientColor"].SetValue(Color.White.ToVector4());
				effect.Parameters["AmbientIntensity"].SetValue(0.1f);

				Vector3 dir = new Vector3(X.SliderValue,Y.SliderValue,Z.SliderValue).Normalized();

				effect.Parameters["LightDirection"].SetValue(dir == Vector3.Zero ? Vector3.Left : dir);

				// The new specular components
				effect.Parameters["Shininess"].SetValue(MathF.Pow(2.0f,1.0f / (1.0f - (Shininess.SliderPosition == 1.0f ? float.Epsilon : Shininess.SliderPosition))) - 2.0f);
				effect.Parameters["SpecularColor"].SetValue(new Color(R.SliderValue,G.SliderValue,B.SliderValue,255).ToVector4());
				effect.Parameters["SpecularIntensity"].SetValue(Intensity.SliderPosition);

				// We asked for the camera position, which doesn't move in our case, so just pass it along
				effect.Parameters["CameraPosition"].SetValue(new Vector3(0.0f,5.0f,-10.0f));
				
				return;
			};

			return;
		}

		protected void LoadPhong()
		{
			loaded = dragon!;
			
			R!.Visible = false;
			G!.Visible = false;
			B!.Visible = false;
			Intensity!.Visible = false;

			X!.Visible = true;
			Y!.Visible = true;
			Z!.Visible = true;

			Shininess!.Visible = true;

			loaded.UseBasicEffect = false;
			loaded.Shader = PhongEffect;
			loaded.ShaderParameterization = (model,mesh,part,effect) =>
			{
				effect.Parameters["World"].SetValue(model.World);
				effect.Parameters["View"].SetValue(model.View);
				effect.Parameters["Projection"].SetValue(model.Projection);

				effect.Parameters["NormalMatrix"].SetValue(Matrix.Transpose(Matrix.Invert(model.World)));
					
				effect.Parameters["AmbientColor"].SetValue(Color.White.ToVector4());
				effect.Parameters["AmbientIntensity"].SetValue(0.1f);

				Vector3 dir = new Vector3(X.SliderValue,Y.SliderValue,Z.SliderValue).Normalized();

				effect.Parameters["LightDirection"].SetValue(dir == Vector3.Zero ? Vector3.Left : dir);
				effect.Parameters["LightColor"].SetValue(Color.White.ToVector4());
				effect.Parameters["LightIntensity"].SetValue(0.5f);

				effect.Parameters["Shininess"].SetValue(MathF.Pow(2.0f,1.0f / (1.0f - (Shininess.SliderPosition == 1.0f ? float.Epsilon : Shininess.SliderPosition))) - 2.0f);
				effect.Parameters["SpecularColor"].SetValue(Color.White.ToVector4());
				effect.Parameters["SpecularIntensity"].SetValue(0.5f);

				effect.Parameters["CameraPosition"].SetValue(new Vector3(0.0f,5.0f,-10.0f));
				
				return;
			};

			return;
		}

		protected void LoadTexture()
		{
			loaded = dragon!;

			R!.Visible = false;
			G!.Visible = false;
			B!.Visible = false;
			Intensity!.Visible = false;

			X!.Visible = false;
			Y!.Visible = false;
			Z!.Visible = false;

			Shininess!.Visible = false;

			loaded.UseBasicEffect = false;
			loaded.Shader = TextureEffect;
			loaded.ShaderParameterization = (model,mesh,part,effect) =>
			{
				effect.Parameters["World"].SetValue(model.World);
				effect.Parameters["View"].SetValue(model.View);
				effect.Parameters["Projection"].SetValue(model.Projection);
				
				effect.Parameters["ModelTexture"].SetValue(dragon_texture);

				return;
			};

			return;
		}

		protected void LoadTextureLighting()
		{
			loaded = dragon!;
			
			R!.Visible = false;
			G!.Visible = false;
			B!.Visible = false;
			Intensity!.Visible = false;

			X!.Visible = true;
			Y!.Visible = true;
			Z!.Visible = true;

			Shininess!.Visible = false;

			loaded.UseBasicEffect = false;
			loaded.Shader = TextureLightingEffect;
			loaded.ShaderParameterization = (model,mesh,part,effect) =>
			{
				effect.Parameters["World"].SetValue(model.World);
				effect.Parameters["View"].SetValue(model.View);
				effect.Parameters["Projection"].SetValue(model.Projection);

				effect.Parameters["NormalMatrix"].SetValue(Matrix.Transpose(Matrix.Invert(model.World)));
					
				effect.Parameters["AmbientColor"].SetValue(Color.White.ToVector4());
				effect.Parameters["AmbientIntensity"].SetValue(0.1f);

				Vector3 dir = new Vector3(X.SliderValue,Y.SliderValue,Z.SliderValue).Normalized();

				effect.Parameters["LightDirection"].SetValue(dir == Vector3.Zero ? Vector3.Left : dir);
				effect.Parameters["LightColor"].SetValue(Color.White.ToVector4());
				effect.Parameters["LightIntensity"].SetValue(1.0f);

				effect.Parameters["Shininess"].SetValue(20.0f);
				effect.Parameters["SpecularColor"].SetValue(Color.White.ToVector4());
				effect.Parameters["SpecularIntensity"].SetValue(0.2f);

				effect.Parameters["CameraPosition"].SetValue(new Vector3(0.0f,5.0f,-10.0f));
				
				effect.Parameters["ModelTexture"].SetValue(dragon_texture);
				
				return;
			};
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

			float tspeed = 2.0f;

			if(Input["IT"].CurrentDigitalValue)
				target += tspeed * deltat * Vector3.Up;

			if(Input["KT"].CurrentDigitalValue)
				target += tspeed * deltat * Vector3.Down;

			if(Input["JT"].CurrentDigitalValue)
				target += tspeed * deltat * Vector3.Right;

			if(Input["LT"].CurrentDigitalValue)
				target += tspeed * deltat * Vector3.Left;

			if(Input["OT"].CurrentDigitalValue)
				target += tspeed * deltat * Vector3.Forward;

			if(Input["UT"].CurrentDigitalValue)
				target += tspeed * deltat * Vector3.Backward;

			if(loaded is not null)
				loaded.View = Matrix.CreateLookAt(new Vector3(0.0f,5.0f,-10.0f),target,Vector3.Up);

			Skybox!.View = Matrix.CreateLookAt(new Vector3(0.0f,5.0f,-10.0f),target,Vector3.Up);

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

			// Draw the skybox
			// Note that the skybox cube has inverted faces so that the 'outside' of the cube is actually inside it
			// This means we will see the cube when inside it and it will be culled when we're outside it
			Skybox!.Draw(delta);

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
		
		protected Slider? X;
		protected Slider? Y;
		protected Slider? Z;

		protected Slider? Shininess;

		protected Boundary Bounds;
		protected RectangleComponent? BoundingBox;

		protected float AspectRatio;

		// Models
		protected SimpleModel? loaded;
		protected Vector3 pos = new Vector3(1.5f,0.0f,0.0f);
		protected Vector3 target = Vector3.Zero;
		protected float x_rot = 0.0f;
		protected float y_rot = 0.0f;
		protected float z_rot = 0.0f;

		protected SimpleModel? doughnut;
		protected SimpleModel? dragon;
		protected Texture2D? dragon_texture;
		protected SimpleModel? mimikyu;

		// Skybox
		protected SimpleModel? Skybox;
		protected TextureCube? SkyboxTexture;

		// Shaders
		protected Effect? AmbientEffect;
		protected Effect? DiffuseEffect;
		protected Effect? PixelDiffuseEffect;
		protected Effect? SpecularEffect;
		protected Effect? PhongEffect;
		protected Effect? TextureEffect;
		protected Effect? TextureLightingEffect;
		
		protected Effect? SkyboxEffect;
	}
}
