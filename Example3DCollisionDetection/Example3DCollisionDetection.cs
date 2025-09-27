using GameEngine.DataStructures.Geometry;
using GameEngine.Framework;
using GameEngine.Input;
using GameEngine.Physics.Collision;
using GameEngine.Physics.Collision.Colliders;
using GameEngine.Utility.ExtensionMethods.ClassExtensions;
using GameEngine.Utility.ExtensionMethods.PrimitiveExtensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Example3DCollisionDetection
{
	/// <summary>
	/// An example of how to use the general purpose collision detection CollisionEngine3D class.
	/// It generates a number of random cubes with random velocities.
	/// The cubes are tinted green when not colliding with anything and red when colliding with something.
	/// The camera can be controlled with WASDQE, and the simulation can paused with space.
	/// </summary>
	public class Example3DCollisionDetection : RenderTargetFriendlyGame
	{
		public Example3DCollisionDetection()
		{
			Content.RootDirectory = "Content";

			Graphics.PreferredBackBufferWidth = 1600;
			Graphics.PreferredBackBufferHeight = 900;

			return;
		}

		protected override void LoadContent()
		{
			Components.Add(Input = new InputManager());

			// Escape will allow us to exit
			Input.AddKeyInput("Esc",Keys.Escape);

			// Space will allow us to pause the simulation
			Input.AddKeyInput("Pause",Keys.Space);
			Pause = false;

			// We'll control the camera with WASDQE
			Input.AddKeyInput("A",Keys.A);
			Input.AddKeyInput("D",Keys.D);
			Input.AddKeyInput("W",Keys.W);
			Input.AddKeyInput("S",Keys.S);
			Input.AddKeyInput("Q",Keys.Q);
			Input.AddKeyInput("E",Keys.E);

			// Generate some random cubes
			Random rand = new Random();

			Squares = new List<SimpleCollider>();
			Detection = new CollisionEngine3D(-10.0f,10.0f,-10.0f,10.0f,-10.0f,10.0f);

			// Add a bunch of random kinetic cubes
			for(int i = 0;i < 100;i++)
			{
				Vector3 offset = new Vector3(rand.Next(41) - 20,rand.Next(41) - 20,rand.Next(41) - 20);

				SimpleCollider r = new SimpleCollider(this,Color.White,new FPrism(-1.0f,-1.0f,-1.0f,2.0f,2.0f,2.0f) + offset,false);
				r.Velocity = new Vector3(0.5f - (float)rand.NextDouble(),0.5f - (float)rand.NextDouble(),0.5f - (float)rand.NextDouble()).Normalized();
				
				Squares.Add(r);
				Detection.AddCollider(r);

				Components.Add(r);
			}

			// Add a bunch of random static cubes
			// These generally should not move, though you can move them if your really desire
			for(int i = 0;i < 100;i++)
			{
				Vector3 offset = new Vector3(rand.Next(41) - 20,rand.Next(41) - 20,rand.Next(41) - 20);

				SimpleCollider r = new SimpleCollider(this,Color.White,new FPrism(-1.0f,-1.0f,-1.0f,2.0f,2.0f,2.0f) + offset,true);
				//r.Velocity = new Vector3(0.5f - (float)rand.NextDouble(),0.5f - (float)rand.NextDouble(),0.5f - (float)rand.NextDouble()).Normalized();
				
				Squares.Add(r);
				Detection.AddCollider(r);

				Components.Add(r);
			}

			// Store the camera matrix in the Services (in a real application you would want to wrap this in a useful class)
			Services.AddService(Matrix.CreateLookAt(new Vector3(0.0f,25.0f,-50.0f),Vector3.Zero,Vector3.Up));

			return;
		}

		protected override void Update(GameTime delta)
		{
			if(Input!["Esc"].CurrentDigitalValue)
				Exit();

			if(Input!["Pause"].IsRisingEdge)
				Pause = !Pause;
			
			float camera_speed = 50.0f;

			Matrix m = (Matrix)Services.GetService(typeof(Matrix));
			
			// Control the camera with WASDQE
			if(Input["A"].CurrentDigitalValue)
				m *= Matrix.CreateTranslation(-m.Translation) * Matrix.CreateRotationY(MathHelper.ToRadians(camera_speed * (float)delta.ElapsedGameTime.TotalSeconds)) * Matrix.CreateTranslation(m.Translation);

			if(Input["D"].CurrentDigitalValue)
				m *= Matrix.CreateTranslation(-m.Translation) * Matrix.CreateRotationY(MathHelper.ToRadians(-camera_speed * (float)delta.ElapsedGameTime.TotalSeconds)) * Matrix.CreateTranslation(m.Translation);

			if(Input["W"].CurrentDigitalValue)
				m *= Matrix.CreateTranslation(-m.Translation) * Matrix.CreateRotationX(MathHelper.ToRadians(-camera_speed * (float)delta.ElapsedGameTime.TotalSeconds)) * Matrix.CreateTranslation(m.Translation);
			
			if(Input["S"].CurrentDigitalValue)
				m *= Matrix.CreateTranslation(-m.Translation) * Matrix.CreateRotationX(MathHelper.ToRadians(camera_speed * (float)delta.ElapsedGameTime.TotalSeconds)) * Matrix.CreateTranslation(m.Translation);

			if(Input["Q"].CurrentDigitalValue)
				m *= Matrix.CreateTranslation(-m.Translation) * Matrix.CreateRotationZ(MathHelper.ToRadians(camera_speed * (float)delta.ElapsedGameTime.TotalSeconds)) * Matrix.CreateTranslation(m.Translation);

			if(Input["E"].CurrentDigitalValue)
				m *= Matrix.CreateTranslation(-m.Translation) * Matrix.CreateRotationZ(MathHelper.ToRadians(-camera_speed * (float)delta.ElapsedGameTime.TotalSeconds)) * Matrix.CreateTranslation(m.Translation);

			// Replace the camera matrix with the new one
			Services.RemoveService(typeof(Matrix));
			Services.AddService(m);

			// Run the base update first
			// We'll do velocity updates here manually for demonstration, but ordinarily that would be a bad idea
			base.Update(delta);

			// The speed the cubes move at per second
			float speed = 1.0f;

			// Move the cubes
			foreach(SimpleCollider c in Squares!)
			{
				if(!Pause)
				{
					Vector3 delta_p = c.Velocity * speed * (float)delta.ElapsedGameTime.TotalSeconds;
					
					c.ChangeBoundary(c.Boundary + delta_p);
					c.World *= Matrix.CreateTranslation(delta_p);
				}

				c.Tint = Color.Green;
			}

			// Update the collision engine
			Detection!.Update();

			// Process each collision
			foreach((ICollider3D,ICollider3D) collision in Detection.CurrentCollisions)
			{
				((SimpleCollider)collision.Item1).Tint = Color.Red;
				((SimpleCollider)collision.Item2).Tint = Color.Red;
			}
			
			return;
		}

		protected override void PreDraw(GameTime delta)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);
			
			// The custom mouse is being rendered via a SpriteBatch, so we'll need to reset these values
			GraphicsDevice.DepthStencilState = DepthStencilState.Default;
			GraphicsDevice.BlendState = BlendState.Opaque;
			GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
			
			return;
		}

		protected override void PostDraw(GameTime delta)
		{return;}

		protected CollisionEngine3D? Detection;
		protected List<SimpleCollider>? Squares;
		protected InputManager? Input;
		protected bool Pause;
	}

	/// <summary>
	/// A barebones ICollider3D to demonstrate the 3D collision engine.
	/// </summary>
	public class SimpleCollider : DrawableGameComponent, ICollider3D
	{
		public SimpleCollider(RenderTargetFriendlyGame game, Color c, float l, float r, float b, float t, float n, float f, bool is_static) : this(game,c,new FPrism(l,b,n,r - l,t - b,f - n),is_static)
		{return;}

		public SimpleCollider(RenderTargetFriendlyGame game, Color c, FPrism bounds, bool is_static) : base(game)
		{
			Boundary = bounds;
			PreviousBoundary = FPrism.Empty;
			Tint = c;

			World = Matrix.CreateTranslation(Boundary.Location);

			float ar = (float)FriendlyGame.Graphics.GraphicsDevice.Viewport.Width / FriendlyGame.Graphics.GraphicsDevice.Viewport.Height;
			Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f),ar,1.0f,1000.0f);

			ColliderID = ColliderID<ICollider3D>.GetFreshID(this);

			_is = is_static;

			OnEnableChanged += (a,b) => {};
			EnabledChanged += (a,b) => OnEnableChanged(this,Enabled);
			OnStaticStateChanged += (a,b) => {};
			OnStaticMovement += (a) => {};

			return;
		}

		protected override void LoadContent()
		{
			MyModel = Game.Content.Load<Model>("Models/cube");
			return;
		}

		public override void Draw(GameTime delta)
		{
			foreach(ModelMesh mesh in MyModel!.Meshes)
			{
				foreach(BasicEffect effect in mesh.Effects)
				{
					effect.EnableDefaultLighting();
					effect.AmbientLightColor = Tint.ToVector3(); // This is a crude but effective way to tint the cube
					
					effect.World = World;
					effect.View = (Matrix)Game.Services.GetService(typeof(Matrix));
					effect.Projection = Projection;
				}

				mesh.Draw();
			}
			
			return;
		}

		public bool ChangeBoundary(FPrism new_boundary)
		{
			PreviousBoundary = Boundary;
			Boundary = new_boundary;
			
			if(IsStatic)
				OnStaticMovement(this);

			return true;
		}

		public bool CollidesWith(ICollider3D other) => true;

		public bool Equals(ICollider3D? other) => other is null ? false : ColliderID == other.ColliderID;

		public override string ToString() => Boundary.ToString();

		public FPrism Boundary
		{get; protected set;}
		
		public float LeftBound => Boundary.Left;
		public float RightBound => Boundary.Right;
		public float BottomBound => Boundary.Bottom;
		public float TopBound => Boundary.Top;
		public float NearBound => Boundary.Near;
		public float FarBound => Boundary.Far;

		public FPrism PreviousBoundary
		{get; protected set;}
		
		public float PreviousLeftBound => PreviousBoundary.Left;
		public float PreviousRightBound => PreviousBoundary.Right;
		public float PreviousBottomBound => PreviousBoundary.Bottom;
		public float PreviousTopBound => PreviousBoundary.Top;
		public float PreviousNearBound => PreviousBoundary.Near;
		public float PreviousFarBound => PreviousBoundary.Far;
		
		public ColliderID<ICollider3D> ColliderID
		{get;}

		public bool IsStatic
		{
			get => _is;

			set
			{
				if(_is == value)
					return;

				_is = value;
				OnStaticStateChanged(this,_is);

				return;
			}
		}

		protected bool _is;

		public RenderTargetFriendlyGame FriendlyGame => (Game as RenderTargetFriendlyGame)!;

		public Model? MyModel
		{get; protected set;}

		public Matrix World
		{get; set;}

		public Matrix Projection
		{get; set;}

		public Vector3 Velocity
		{get; set;}

		public Color Tint
		{get; set;}

		public event EnableChanged<ICollider3D> OnEnableChanged;
		public event StaticStateChanged<ICollider3D> OnStaticStateChanged;
		public event StaticMovement<ICollider3D> OnStaticMovement;
	}
}
