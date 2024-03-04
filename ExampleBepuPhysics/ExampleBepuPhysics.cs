using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.Constraints;
using BepuUtilities.Memory;
using ExampleShaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

// Give names to the system types since we'll have to use both them and MonoGame's
using SMatrix = System.Numerics.Matrix4x4;
using SQuaternion = System.Numerics.Quaternion;
using SVector3 = System.Numerics.Vector3;

namespace ExampleBepuPhysics
{
	/// <summary>
	/// Uses Bepu Physics to do some physics.
	/// </summary>
	public class ExampleBepuPhysics : Game
	{
		public ExampleBepuPhysics()
		{
			Graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
			IsMouseVisible = true;

			Graphics.PreferredBackBufferWidth = 1000;
			Graphics.PreferredBackBufferHeight = 1000;
			AspectRatio = (float)Graphics.PreferredBackBufferWidth / Graphics.PreferredBackBufferHeight;

			return;
		}

		protected override void Initialize()
		{
			// Create a new simulation (we'll add stuff to it in LoadContent)
			// We give it more or less default settings
			// Read the Getting Started documentation for what these things mean
			space = Simulation.Create(new BufferPool(),new DemoNarrowPhaseCallbacks(new SpringSettings(30.0f,1.0f)),new DemoPoseIntegratorCallbacks(-10.0f * System.Numerics.Vector3.UnitY),new SolveDescription(6,1));

			base.Initialize();
			return;
		}

		protected override void LoadContent()
		{
			// We will create our objects and add all of them to the physics simulation
			// Make the ground first
			Ground = new SimpleModel(this,Content.Load<Model>("cube"));
			Ground.BasicEffectTint = Color.SaddleBrown;
			Ground.World = Matrix.CreateScale(10.0f,1.0f,10.0f);
			Ground.View = Matrix.CreateLookAt(new Vector3(0.0f,10.0f,-10.0f),Vector3.Zero,Vector3.Up);
			Ground.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f),AspectRatio,1.0f,1000.0f);
			
			// Convex shapes (like boxes and spheres) are centered on their volumetric center (which is where they are rotated about as well)
			// If you know that information about your models, then great!
			// If not...find out
			// I'm educated guessing and checking here, which is perhaps the single worst way to do it short of blindly picking a number and calling it good
			// The meshes in a Model class (so Model m -> m.Meshes) in MonoGame have a BoundingSphere property, which is convenient to use in many cases
			GroundHandle = space!.Statics.Add(new StaticDescription(SVector3.Zero,System.Numerics.Quaternion.Identity,space.Shapes.Add(new Box(20.0f,2.0f,20.0f))));
			
			// Now let's make a sphere to roll around
			Ball = new SimpleModel(this,Content.Load<Model>("sphere"));
			Ball.BasicEffectTint = Color.BlueViolet;
			Ball.View = Matrix.CreateLookAt(new Vector3(0.0f,10.0f,-10.0f),Vector3.Zero,Vector3.Up);
			Ball.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f),AspectRatio,1.0f,1000.0f);

			BallHandle = space.Bodies.Add(BodyDescription.CreateConvexDynamic(new RigidPose(new SVector3(0.0f,5.0f,0.0f)),10.0f,space.Shapes,new Sphere(0.9f)));

			// Now let's make a second sphere to roll around
			Ball2 = new SimpleModel(this,Content.Load<Model>("sphere"));
			Ball2.BasicEffectTint = Color.BlueViolet;
			Ball2.View = Matrix.CreateLookAt(new Vector3(0.0f,10.0f,-10.0f),Vector3.Zero,Vector3.Up);
			Ball2.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f),AspectRatio,1.0f,1000.0f);

			Ball2Handle = space.Bodies.Add(BodyDescription.CreateConvexDynamic(new RigidPose(new SVector3(0.0f,6.0f,0.0f)),1.0f,space.Shapes,new Sphere(0.6f)));

			// We'll give the sphere a ramp to roll along
			Ramp = new SimpleModel(this,Content.Load<Model>("cube"));
			Ramp.BasicEffectTint = Color.PeachPuff;
			//Ramp.World = Matrix.CreateScale(3.0f,1.0f,1.0f) * Matrix.CreateRotationZ(MathHelper.ToRadians(10.0f)) * Matrix.CreateTranslation(-1.0f,1.0f,0.0f);
			Ramp.View = Matrix.CreateLookAt(new Vector3(0.0f,10.0f,-10.0f),Vector3.Zero,Vector3.Up);
			Ramp.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f),AspectRatio,1.0f,1000.0f);
			
			RampHandle = space.Statics.Add(new StaticDescription(new SVector3(-1.0f,1.0f,0.0f),SQuaternion.CreateFromRotationMatrix(SMatrix.CreateRotationZ(MathHelper.ToRadians(10.0f))),space.Shapes.Add(new Box(3.0f,1.0f,1.0f))));

			return;
		}

		protected override void Update(GameTime delta)
		{
			if(GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
				Exit();

			// The first update cycle has a 0 time step, so we need to skip that
			float delta_t = (float)delta.ElapsedGameTime.TotalSeconds;

			if(delta_t == 0.0f)
				return;

			// Step the simulation forward
			space!.Timestep(delta_t);

			// Assign the world matrices appropriately
			space.Bodies[BallHandle].GetDescription(out BodyDescription ball);
			Ball!.World = Matrix.CreateTranslation(ball.Pose.Position);
			
			space.Bodies[Ball2Handle].GetDescription(out BodyDescription ball2);
			Ball2!.World = Matrix.CreateScale(0.5f,0.5f,0.5f) * Matrix.CreateTranslation(ball2.Pose.Position);

			space.Statics[RampHandle].GetDescription(out StaticDescription ramp);
			Ramp!.World = Matrix.CreateScale(3.0f,1.0f,1.0f) * Matrix.CreateFromQuaternion(new Quaternion(ramp.Pose.Orientation.X,ramp.Pose.Orientation.Y,ramp.Pose.Orientation.Z,ramp.Pose.Orientation.W)) * Matrix.CreateTranslation(ramp.Pose.Position + SVector3.UnitX);

			base.Update(delta);
			return;
		}

		protected override void Draw(GameTime delta)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);

			// We don't need to do anything fancy in our draw step
			// It's business as usual
			Ground!.Draw(delta);
			Ball!.Draw(delta);
			Ball2!.Draw(delta);
			Ramp!.Draw(delta);

			base.Draw(delta);
			return;
		}

		// Game variables
		private GraphicsDeviceManager Graphics;
		private float AspectRatio;

		// Systems
		private Simulation? space;

		// Models
		private SimpleModel? Ground;
		private StaticHandle GroundHandle;

		private SimpleModel? Ball;
		private BodyHandle BallHandle;

		private SimpleModel? Ball2;
		private BodyHandle Ball2Handle;

		private SimpleModel? Ramp;
		private StaticHandle RampHandle;
	}
}
