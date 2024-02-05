using GameEngine.Framework;
using GameEngine.Input;
using GameEngine.Utility.ExtensionMethods.ClassExtensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Example3DGame
{
	public class Example3DGame : RenderTargetFriendlyGame
	{
		public Example3DGame()
		{
			Content.RootDirectory = "Content";

			Graphics.PreferredBackBufferWidth = 1000;
			Graphics.PreferredBackBufferHeight = 1000;

			return;
		}

		protected override void Initialize()
		{
			Components.Add(Input = new InputManager());
			
			Input.AddKeyInput("Exit",Keys.Escape);

			Input.AddKeyInput("U",Keys.Up);
			Input.AddKeyInput("D",Keys.Down);
			Input.AddKeyInput("L",Keys.Left);
			Input.AddKeyInput("R",Keys.Right);

			rotation = 0.0f;
			pos = Vector3.Zero;

			base.Initialize();
			return;
		}

		protected override void LoadContent()
		{
			// Add the model to the game's components
			Components.Add(doughnut = new ExampleModelComponent(this,Content.Load<Model>("doughnut")));

			// This will ensure the mouse is drawn on top of models
			Mouse.DepthRecord = DepthStencilState.Default;
			Mouse.Blend = BlendState.NonPremultiplied;

			return;
		}

		protected override void Update(GameTime delta)
		{
			if(Input["Exit"].CurrentDigitalValue)
				Exit();

			// We'll use this a lot, so just do the cast once and give it a name
			float deltat = (float)delta.ElapsedGameTime.TotalSeconds;

			// Have a common speed
			float speed = 2.0f;

			// Move the doughnut if we desire
			if(Input["L"].CurrentDigitalValue)
				pos += speed * Vector3.UnitX * deltat;

			if(Input["R"].CurrentDigitalValue)
				pos -= speed * Vector3.UnitX * deltat;

			if(Input["U"].CurrentDigitalValue)
				pos += speed * Vector3.UnitZ * deltat;

			if(Input["D"].CurrentDigitalValue)
				pos -= speed * Vector3.UnitZ * deltat;

			// We'll be rotating the doughnut to fully observe its true power and delicious nature
			rotation += 0.2f * deltat;
			
			// We add in a rotation about the X axis and a scale to align the doughtnut to where we actually want it
			// We could change where the camera is instead, but it's generally best to consider the Y axis as the direciton of gravity and align things appropriately
			// We could also set this initial rotation in the importer, but we do so here
			// However, we do in fact scale the doughnut in the importer, increasing its size by a factor of 2
			doughnut.World = Matrix.CreateRotationZ(rotation) * Matrix.CreateRotationX(MathHelper.ToRadians(-90.0f)) * Matrix.CreateTranslation(pos); // The MonoGame Matrix class multiplies points on the left, so matrices on the RIGHT are applied LAST

			base.Update(delta);
			return;
		}

		protected override void PreDraw(GameTime delta)
		{
			// Clear the background as usual
			GraphicsDevice.Clear(Color.CornflowerBlue);
			
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			// The following lines are used before you draw 3D elements after you draw 2D elements                                             //
			// They are very important to have, as 2D drawing will typically change some of your GPU settings that 3D applications depend upon //
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			// This first line is important so that the depth buffer is enabled for 3D drawing
			// 2D drawing with SpriteBatches disables it (unless you use the depth stencil in a SpriteBatch begin) even between drawing cycles (the state is not reset between cycles)
			// You can assign a nondefault value, of course, but the default is the default for a reason
			GraphicsDevice.DepthStencilState = DepthStencilState.Default;

			// The following two settings are also clobbered by SpriteBatch Begin/End calls (though a SpriteBatch may actually set them to values you want to use depending on the Begin mode)
			// These are the default values that you will likely want to use, but you can use any other values or not bother resetting them if you desire
			GraphicsDevice.BlendState = BlendState.Opaque;
			GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
			
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			// Normally, we would keep a single camera object in the games Services property with a transform for models to grab for their view matrix //
			// We would do similarly for the projection matrix                                                                                         //
			// However, we place both here to demonstrate how they work                                                                                //
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			// This is a view matrix for the camera
			// The look at matrix is very commonly used
			// It puts the camera at the first parameter, points it at the second parameter, and the last parameter orients the camera along this line to get its up direction
			// This last parameter doesn't need to be orthogonal to (first - second), as the matrix calculation will ensure that for us
			doughnut.View = Matrix.CreateLookAt(new Vector3(0.0f,5.0f,-10.0f),Vector3.Zero,Vector3.Up);

			// We calculate the aspect ratio to build a projection matrix
			float ar = (float)Graphics.GraphicsDevice.Viewport.Width / Graphics.GraphicsDevice.Viewport.Height;
			
			// The field of view projection is the most common projection matrix for 3D applications
			// It creates a frustum which reasonably mirrors what the human eye would expect to see when looking 'into' or 'through' a computer screen to what lies 'inside'
			doughnut.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f),ar,1.0f,1000.0f);
			
			return;
		}

		protected override void PostDraw(GameTime delta)
		{return;}
		
		protected InputManager Input;

		protected ExampleModelComponent doughnut;
		protected float rotation;
		protected Vector3 pos;
	}

	/// <summary>
	/// A bare bones model game component.
	/// </summary>
	public class ExampleModelComponent : DrawableGameComponent
	{
		public ExampleModelComponent(Game game, Model m) : base(game)
		{
			MyModel = m;
			return;
		}

		public override void Draw(GameTime delta)
		{
			// To draw a model, we must draw all of its meshes
			// A mesh is a collection of vertices into a group for processing
			foreach(ModelMesh mesh in MyModel.Meshes)
			{
				// Effects describe how a model mesh is drawn
				// These are the shaders, the tiny drawing programs that are compiled to talk to the GPU
				// MonoGame provides a BasicEffect (and some others) that perform the most common shader tasks for you, such as texture calculations, basic lighting, etc
				foreach(BasicEffect effect in mesh.Effects)
				{
					// This will tell our effect that we want it to calculate some lighting for us
					// You can set additional lighting data if you desire to fine tune the results of the ambient, diffuse, and specular lighting
					// These settings are done on a per effect basis, so you will have to set them for all models
					effect.EnableDefaultLighting();

					// The following three lines set the model-view-projection matrices
					// The order you set them doesn't matter
					effect.View = View;
					effect.Projection = Projection;
					effect.World = World;
				}

				// This final call will draw the mesh
				mesh.Draw();
			}

			return;
		}

		/// <summary>
		/// The model this game component will draw.
		/// </summary>
		public Model MyModel
		{get; protected set;}

		/// <summary>
		/// The world matrix.
		/// </summary>
		public Matrix World
		{get; set;}

		/// <summary>
		/// The view matrix.
		/// </summary>
		public Matrix View
		{get; set;}

		/// <summary>
		/// The projection matrix.
		/// </summary>
		public Matrix Projection
		{get; set;}
	}
}
