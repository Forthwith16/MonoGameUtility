#define DEBUG // We're defining the DEBUG token so we can use it for debug info; to disable it, just comment this out

using GameEngine.Framework;
using GameEngine.GameObjects;
using GameEngine.Physics.Collision;
using GameEngine.Physics.Collision.Colliders;
using GameEngine.Utility.ExtensionMethods.ClassExtensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Collections.Generic;
using System.Linq;

namespace ExampleQueryCollider
{
	/// <summary>
	/// Demonstrates how to use collision as a query about game states.
	/// </summary>
	public class ExampleQueryCollider : RenderTargetFriendlyGame
	{
		// Pick one of the below (and only one) to see how each setting behaves
		public const bool NoQueryColliders = false; // Uses no query colliders to walk forward
		public const bool GroundQueryCollider = false; // Uses a ground query collider to walk forward better
		public const bool ForwardQueryCollider = true; // Uses a ground and forward query collider to walk back and forth
		public const bool OopsForwardQueryCollider = false; // Uses a ground and forward query collider to walk back and forth, but the koopa eventually falls off due to the changing position of the forward query

		public ExampleQueryCollider() : base()
		{
			// Sanity check the settings
			if(new bool[] {NoQueryColliders,GroundQueryCollider,ForwardQueryCollider,OopsForwardQueryCollider}.Where(a => a).Count() != 1)
				Exit();

			Content.RootDirectory = "Content";

			Graphics.PreferredBackBufferWidth = 1280;
			Graphics.PreferredBackBufferHeight = 655;

			Engine = new CollisionEngine2D(0.0f,1280.0f,0.0f,655.0f);
			Colliders = new LinkedList<CollisionBox>();

			return;
		}

		protected override void LoadContent()
		{
			Renderer = new SpriteBatch(GraphicsDevice);

			// Load the background
			Components.Add(Background = new ImageGameObject(Renderer,"background"));

			// Load the background music
			Song bgm = Content.Load<Song>("overworld");
			MediaPlayer.Play(bgm);
			MediaPlayer.IsRepeating = true;
			
			// Now we need to load the ground collision box
			CollisionBox temp;
			Colliders.AddLast(temp = new CollisionBox(Renderer,1280,128,Color.Red));
			Components.Add(temp);
			temp.Translate(0.0f,527.0f);
			temp.IsStatic = true;
			
			// Now add the box collider(s)
			Colliders.AddLast(temp = new CollisionBox(Renderer,320,64,Color.Red));
			Components.Add(temp);
			temp.Translate(512.0f,271.0f);
			temp.IsStatic = true;

			// Now add the koopa
			Components.Add(Koopa = new AnimatedGameObject(Renderer,"Animations/koopa"));
			Koopa.Scale(4.0f,4.0f);
			Koopa.Translate(650.0f,-200.0f);
			//Koopa.Animation.Paused = true;

			// Now add the koopa's collider
			Colliders.AddLast(KoopaBox = new CollisionBox(Renderer,18 * 4,27 * 4,Color.Red));
			Components.Add(KoopaBox);
			KoopaBox.Parent = Koopa;
			KoopaBox.Scale(0.25f);

			// Create the ground query collider (this one doesn't need to be in the collision engine, at least not in this example)
			// We'll set the query height to 5 for this example for visibility; in principle, we should use a height of 1 in 2D (where 1 height = 1 pixel) and in 3D the height can be whatever works
			// We change how the collision resolution works here in this 2D exmaple to support a larger ground query, since we don't want to be grounded early and hover in the air
			Components.Add(GroundQuery = new CollisionBox(Renderer,18 * 4,5,Color.Green));
			GroundQuery.Parent = KoopaBox;
			GroundQuery.Translate(0.0f,27.0f * 4.0f);

			// Create the forward query collider (this one also doesn't need to be in the collision engine, at least not in this example)
			// We'll set the query height just as with the ground query, because this is another ground query
			// This time, however, we'll query AHEAD of our koopa to check to see if moving foward would cause him to fall (become ungrounded)
			// We can do something similar, if desired, to avoid walking face first into walls
			Components.Add(ForwardQuery = new CollisionBox(Renderer,5,5,Color.Blue));
			ForwardQuery.Parent = GroundQuery;
			ForwardQuery.Translate(15.0f,0.0f); // More negative x values will allow us to turn around sooner; more positive will turn around later

			// Set up collision resolution
			if(NoQueryColliders)
				KoopaBox.Resolve += (me,other,delta) =>
				{
					// We only ever need to move up
					Koopa.Translate(0.0f,other.BottomBound - me.TopBound);
					KoopaBox.Velocity = Vector2.Zero;
				
					Koopa.Animation.Playing = true;
					Grounded = true;

					return;
				};
			else if(GroundQueryCollider || ForwardQueryCollider || OopsForwardQueryCollider)
				KoopaBox.Resolve += (me,other,delta) =>
				{
					// We only ever need to move up
					Koopa.Translate(0.0f,other.BottomBound - me.TopBound);
					KoopaBox.Velocity = Vector2.Zero;
					
					Grounded = true;
					return;
				};

			// Initialize our koopa states
			Grounded = false;
			Left = true;

			// Add all the colliders to the engine
			Engine.AddAllColliders(Colliders);

			// Add the colliders we don't want in the engine now
			if(GroundQueryCollider || ForwardQueryCollider || OopsForwardQueryCollider)
				Colliders.AddLast(GroundQuery);

			if(ForwardQueryCollider || OopsForwardQueryCollider)
				Colliders.AddLast(ForwardQuery);

			return;
		}

		protected override void Update(GameTime delta)
		{
			if(Keyboard.GetState().IsKeyDown(Keys.Escape))
				Exit();
			
			// Show the colliders with the s key
			if(Keyboard.GetState().IsKeyDown(Keys.S))
				foreach(CollisionBox box in Colliders)
					box.Visible = true;

			// Hide the colliders with the h key
			if(Keyboard.GetState().IsKeyDown(Keys.H))
				foreach(CollisionBox box in Colliders)
					box.Visible = false;

			// Pick which version of the program we should run
			if(NoQueryColliders)
				NoQueryColliderUpdate(delta);
			else if(GroundQueryCollider)
				UseGroundQueryCollider(delta);
			else if(ForwardQueryCollider)
				UseForwardQueryCollider(delta);
			else if(OopsForwardQueryCollider)
				UseOopsForwardQueryCollider(delta);

			base.Update(delta);
			return;
		}

		/// <summary>
		/// The update logic when we are not using query colliders.
		/// </summary>
		protected void NoQueryColliderUpdate(GameTime delta)
		{
			// Apply gravity to the koopa
			if(Airborn)
				KoopaBox!.Velocity += new Vector2(0.0f,1000.0f * (float)delta.ElapsedGameTime.TotalSeconds);

			// Apply walking power to the koopa
			if(Grounded)
				KoopaBox!.Velocity = new Vector2(-100.0f,KoopaBox.Velocity.Y);

			// Apply velocity to the koopa
			Koopa!.Translate(KoopaBox!.Velocity * (float)delta.ElapsedGameTime.TotalSeconds);
			
			// Update the collision detection
			Engine.Update();

			// We assume the koopa is airborn unless proven otherwise
			Grounded = false;

			// Resolve all collisions
			foreach((ICollider2D,ICollider2D) cc in Engine.CurrentCollisions)
			{
				(cc.Item1 as CollisionBox)!.Resolve(cc.Item1,cc.Item2,delta);
				(cc.Item2 as CollisionBox)!.Resolve(cc.Item2,cc.Item1,delta);
			}
			
			// This will cause frame skipping, since we have one frame airborn, one frame grounded, one frame airborn, one frame grounded, etc
			Koopa!.Animation.Paused = Airborn;

			return;
		}

		/// <summary>
		/// The update logic when using a ground query collider.
		/// </summary>
		protected void UseGroundQueryCollider(GameTime delta)
		{
			// Apply gravity to the koopa
			if(Airborn)
				KoopaBox!.Velocity += new Vector2(0.0f,1000.0f * (float)delta.ElapsedGameTime.TotalSeconds);

			// Apply walking power to the koopa
			if(Grounded)
				KoopaBox!.Velocity = new Vector2(-100.0f,KoopaBox.Velocity.Y);

			// Apply velocity to the koopa
			Koopa!.Translate(KoopaBox!.Velocity * (float)delta.ElapsedGameTime.TotalSeconds);
			
			// Update the collision detection
			Engine.Update();

			// Resolve all collisions
			foreach((ICollider2D,ICollider2D) cc in Engine.CurrentCollisions)
			{
				(cc.Item1 as CollisionBox)!.Resolve(cc.Item1,cc.Item2,delta);
				(cc.Item2 as CollisionBox)!.Resolve(cc.Item2,cc.Item1,delta);
			}

			// This time we will query the collision engine to see if we're grounded AFTER resolving our collisions
			// We insist that you must be grounded (by resolving a collision) in order to remain grounded
			// In 2D, this is not neceesary; we could just make our grounded query of height 1 which directly corresponds to 1 pixel height
			// In 2D, "1 unit" can be any number of pixels
			Grounded = Grounded && Engine.Query(GroundQuery!,true,false).GetEnumerator().MoveNext(); // If we have at least one thing, then we're grounded if we were before
			
			// Pause the animation if we're airborn (and thus not walking)
			Koopa!.Animation.Paused = Airborn;

			return;
		}

		/// <summary>
		/// The update logic when using a ground query collider and a forward query collider.
		/// </summary>
		protected void UseForwardQueryCollider(GameTime delta)
		{
			// Apply gravity to the koopa
			if(Airborn)
				KoopaBox!.Velocity += new Vector2(0.0f,1000.0f * (float)delta.ElapsedGameTime.TotalSeconds);

			// Apply walking power to the koopa
			if(Grounded)
				if(Left)
					KoopaBox!.Velocity = new Vector2(-100.0f,KoopaBox.Velocity.Y);
				else
					KoopaBox!.Velocity = new Vector2(100.0f,KoopaBox.Velocity.Y);

			// Apply velocity to the koopa
			Koopa!.Translate(KoopaBox!.Velocity * (float)delta.ElapsedGameTime.TotalSeconds);
			
			// Update the collision detection
			Engine.Update();

			// Resolve all collisions
			foreach((ICollider2D,ICollider2D) cc in Engine.CurrentCollisions)
			{
				(cc.Item1 as CollisionBox)!.Resolve(cc.Item1,cc.Item2,delta);
				(cc.Item2 as CollisionBox)!.Resolve(cc.Item2,cc.Item1,delta);
			}

			// This time we will query the collision engine to see if we're grounded AFTER resolving our collisions
			// We insist that you must be grounded (by resolving a collision) in order to remain grounded
			// In 2D, this is not neceesary; we could just make our grounded query of height 1 which directly corresponds to 1 pixel height
			// In 2D, "1 unit" can be any number of pixels
			Grounded = Grounded && Engine.Query(GroundQuery!,true,false).GetEnumerator().MoveNext(); // If we have at least one thing, then we're grounded if we were before
			
			// Pause the animation if we're airborn (and thus not walking)
			Koopa!.Animation.Paused = Airborn;

			// Lastly, if our forward collider has nothing going on, then we need to turn around (if we aren't airborn already)
			if(Grounded && !Engine.Query(ForwardQuery!,true,false).GetEnumerator().MoveNext())
			{
				Left = !Left;

				if(Left)
				{
					// The koopa faces left by default
					Koopa.Effect = SpriteEffects.None;

					// We need to move the forward query to the left of the koopa
					ForwardQuery!.Translate(-37.0f,0.0f);
				}
				else
				{
					// The koopa faces left by default, so we need to make it face right
					Koopa.Effect = SpriteEffects.FlipHorizontally;

					// We need to move the forward query to the right of the koopa
					ForwardQuery!.Translate(37.0f,0.0f);
				}
			}

			return;
		}

		/// <summary>
		/// The update logic when using a ground query collider and a forward query collider (but badly so that the koopa walks off eventually).
		/// </summary>
		protected void UseOopsForwardQueryCollider(GameTime delta)
		{
			// Apply gravity to the koopa
			if(Airborn)
				KoopaBox!.Velocity += new Vector2(0.0f,1000.0f * (float)delta.ElapsedGameTime.TotalSeconds);

			// Apply walking power to the koopa
			if(Grounded)
				if(Left)
					KoopaBox!.Velocity = new Vector2(-100.0f,KoopaBox.Velocity.Y);
				else
					KoopaBox!.Velocity = new Vector2(100.0f,KoopaBox.Velocity.Y);

			// Apply velocity to the koopa
			Koopa!.Translate(KoopaBox!.Velocity * (float)delta.ElapsedGameTime.TotalSeconds);
			
			// Update the collision detection
			Engine.Update();

			// Resolve all collisions
			foreach((ICollider2D,ICollider2D) cc in Engine.CurrentCollisions)
			{
				(cc.Item1 as CollisionBox)!.Resolve(cc.Item1,cc.Item2,delta);
				(cc.Item2 as CollisionBox)!.Resolve(cc.Item2,cc.Item1,delta);
			}

			// This time we will query the collision engine to see if we're grounded AFTER resolving our collisions
			// We insist that you must be grounded (by resolving a collision) in order to remain grounded
			// In 2D, this is not neceesary; we could just make our grounded query of height 1 which directly corresponds to 1 pixel height
			// In 2D, "1 unit" can be any number of pixels
			Grounded = Grounded && Engine.Query(GroundQuery!,true,false).GetEnumerator().MoveNext(); // If we have at least one thing, then we're grounded if we were before
			
			// Pause the animation if we're airborn (and thus not walking)
			Koopa!.Animation.Paused = Airborn;

			// Lastly, if our forward collider has nothing going on, then we need to turn around (if we aren't airborn already)
			if(Grounded && !Engine.Query(ForwardQuery!,true,false).GetEnumerator().MoveNext())
			{
				Left = !Left;

				if(Left)
				{
					// The koopa faces left by default
					Koopa.Effect = SpriteEffects.None;

					// We need to move the forward query to the left of the koopa
					ForwardQuery!.Translate(-67.0f,0.0f);
				}
				else
				{
					// The koopa faces left by default, so we need to make it face right
					Koopa.Effect = SpriteEffects.FlipHorizontally;

					// We need to move the forward query to the right of the koopa
					ForwardQuery!.Translate(57.0f,0.0f);
				}
			}

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

		private SpriteBatch? Renderer;
		private ImageGameObject? Background;
		
		private AnimatedGameObject? Koopa;
		private CollisionBox? KoopaBox;
		private CollisionBox? GroundQuery;
		private CollisionBox? ForwardQuery;
		private bool Grounded;
		private bool Airborn => !Grounded;
		private bool Left;

		private CollisionEngine2D Engine;
		private LinkedList<CollisionBox> Colliders;
	}
}
