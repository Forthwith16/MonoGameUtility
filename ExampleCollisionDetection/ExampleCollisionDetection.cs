using GameEngine.DataStructures.Geometry;
using GameEngine.Framework;
using GameEngine.GameComponents;
using GameEngine.Physics.Collision;
using GameEngine.Physics.Collision.Colliders;
using GameEngine.Utility.ExtensionMethods.PrimitiveExtensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace ExampleCollisionDetection
{
	/// <summary>
	/// An example of how to use the general purpose collision detection CollisionEngine2D class.
	/// It generates a number of random squares with random velocities.
	/// The squares are tinted green when not colliding with anything and red when colliding with something.
	/// </summary>
	public class ExampleCollisionDetection : RenderTargetFriendlyGame
	{
		public ExampleCollisionDetection()
		{
			Content.RootDirectory = "Content";

			Graphics.PreferredBackBufferWidth = 1600;
			Graphics.PreferredBackBufferHeight = 900;

			return;
		}

		protected override void LoadContent()
		{
			Renderer = new SpriteBatch(GraphicsDevice);

			// Generate a lot of random squares
			Random rand = new Random();

			Squares = new List<SimpleCollider>();
			Detection = new CollisionEngine2D(0.0f,1600.0f,0.0f,900.0f);

			for(int i = 0;i < 200;i++)
			{
				Vector2 offset = new Vector2(rand.Next(1501),rand.Next(801));

				SimpleCollider r = new SimpleCollider(this,Renderer,Color.White,new FRectangle(0.0f,0.0f,100.0f,100.0f) + offset,false);
				r.Translate(offset);
				r.Velocity = new Vector2(0.5f - (float)rand.NextDouble(),0.5f - (float)rand.NextDouble()).Normalized();

				Squares.Add(r);
				Detection.AddCollider(r);

				Components.Add(r);
			}

			return;
		}

		protected override void Update(GameTime delta)
		{
			if(Keyboard.GetState().IsKeyDown(Keys.Escape))
				Exit();
			
			// Update every thing before we update collisions
			// In this case, our game objects don't do anything, but if they did, we would want to move them first before checking collisions
			base.Update(delta);

			// A speed for the squares to move at
			float speed = 100.0f;

			// For each collider, we'll set the tint to green by default and change it to red if experiencing a collision
			foreach(SimpleCollider c in Squares!)
			{
				// This is the delta in this square's position
				Vector2 delta_p = c.Velocity * speed * (float)delta.ElapsedGameTime.TotalSeconds;

				// We need to move the square and update its bounding box
				// In a proper application, the position and bounding box would be tied together in the class's code
				c.ChangeBoundary(c.Boundary + delta_p);
				c.Translate(delta_p);
				c.Tint = Color.Green;
			}

			// Run the collision detection engine
			Detection!.Update();

			// For each pair of items in a collision, we tint them both read (we do not get symmetric collisions)
			foreach((ICollider2D,ICollider2D) collision in Detection.CurrentCollisions)
			{
				((SimpleCollider)collision.Item1).Tint = Color.Red;
				((SimpleCollider)collision.Item2).Tint = Color.Red;
			}
			
			return;
		}

		protected override void PreDraw(GameTime delta)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);
			Renderer!.Begin(SpriteSortMode.BackToFront,BlendState.NonPremultiplied,SamplerState.LinearWrap,null,null,null,null);
			
			return;
		}

		protected override void PostDraw(GameTime delta)
		{
			Renderer!.End();
			return;
		}

		protected SpriteBatch? Renderer;
		protected CollisionEngine2D? Detection;
		protected List<SimpleCollider>? Squares;
	}

	/// <summary>
	/// A very simple collider.
	/// It draws itself as a box via inheriting the RectangleComponent.
	/// It otherwise has an absolute barebones implementation of ICollider2D so that we can put it into our collision engine.
	/// </summary>
	public class SimpleCollider : RectangleComponent, ICollider2D
	{
		public SimpleCollider(Game game, SpriteBatch? renderer, Color c, float l, float r, float b, float t, bool is_static) : this(game,renderer,c,new FRectangle(l,b,r - l,t - b),is_static)
		{return;}

		public SimpleCollider(Game game, SpriteBatch? renderer, Color c, FRectangle bounds, bool is_static) : base(game,renderer,(int)MathF.Ceiling(bounds.Width),(int)MathF.Ceiling(bounds.Height),c)
		{
			Boundary = bounds;

			ID = ICollider<ICollider2D>.NextID;

			_is = is_static;

			OnEnableChanged += (a,b) => {};
			EnabledChanged += (a,b) => OnEnableChanged(this,Enabled);
			OnStaticStateChanged += (a,b) => {};
			OnStaticMovement += (a) => {};

			return;
		}

		public bool ChangeBoundary(FRectangle new_boundary)
		{
			Boundary = new_boundary;
			return true;
		}

		public bool CollidesWith(ICollider2D other) => true;

		public bool Equals(ICollider2D? other) => other is null ? false : ID == other.ID;

		public override string ToString() => Boundary.ToString();

		public FRectangle Boundary
		{get; protected set;}
		
		public float LeftBound => Boundary.Left;
		
		public float RightBound => Boundary.Right;
		
		public float BottomBound => Boundary.Bottom;
		
		public float TopBound => Boundary.Top;
		
		public uint ID
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

		public Vector2 Velocity
		{get; set;}

		public event EnableChanged<ICollider2D> OnEnableChanged;
		public event StaticStateChanged<ICollider2D> OnStaticStateChanged;
		public event StaticMovement<ICollider2D> OnStaticMovement;
	}
}
