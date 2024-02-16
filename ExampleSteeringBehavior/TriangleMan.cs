using GameEngine.Framework;
using GameEngine.GameComponents;
using GameEngine.Input;
using GameEngine.Maths;
using GameEngine.Utility.ExtensionMethods.PrimitiveExtensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace ExampleSteeringBehavior
{
	/// <summary>
	/// A triangle man that can be controlled via steering behaviors.
	/// </summary>
	public class TriangleMan : RectangleComponent
	{
		/// <summary>
		/// Creates a new triangle man. Its head will be pointed in the positive y direction.
		/// </summary>
		/// <param name="game">The game this triangle man will belong to.</param>
		/// <param name="renderer">The way we render a triangle man. This can be changed later.</param>
		/// <param name="w">The width of the traingle man.</param>
		/// <param name="h">The height of the triangle man.</param>
		/// <param name="outline">The outline color of the triangle man.</param>
		/// <param name="line_width">The width of the triangle man outline.</param>
		/// <param name="inside">The internal color of the triangle man.</param>
		public TriangleMan(Game game, SpriteBatch? renderer, int w, int h, Color outline, uint line_width, Color inside) : base(game,renderer,w,h,BuildTriangle(outline,w,h,line_width,inside))
		{
			Position = Vector2.Zero;
			Rotation = 0.0f;
			Center = new Vector2(w,h) / 2.0f;

			Velocity = Vector2.Zero;

			MaxVelocity = 250.0f;
			MaxForce = 350.0f;

			ArrivalRange = 420.0f;
			InnerArrivalRange = 80.0f;
			FleeRange = 500.0f;

			rand = new Random();
			RandomTarget = Vector2.Zero;
			RandomTargetCooldown = 0.0f;

			WanderCircleLead = 100.0f;
			WanderCircleRadius = 200.0f;
			WanderAngle = 0.0f;
			WanderAngleChange = 1.5f;

			EvasionTarget = null;
			PursuitTarget = null;

			SeekFleeTargets = new List<Vector2>();

			Behavior = TriangleManBehavior.UnnaturalSeek;

			LayerDepth = 0.9f;
			return;
		}

		/// <summary>
		/// Determines how to draw a triangle man.
		/// </summary>
		/// <param name="outline">The outline to draw around the triangle man.</param>
		/// <param name="w">The width of the traingle man.</param>
		/// <param name="h">The height of the triangle man.</param>
		/// <param name="line_width">The width of the triangle man outline.</param>
		/// <param name="inside">The internal color of the triangle man.</param>
		/// <returns>Returns a color function describing a traingle man.</returns>
		protected static ColorFunction BuildTriangle(Color outline, int w, int h, uint line_width, Color inside)
		{
			return (x,y) =>
			{
				// If we are near the top, we just draw the outline
				if(y < line_width)
					return outline;

				// If we are within line_width / 2.0f of the line from (0,0) to ((w - 1) / 2.0f,h - 1) or the line from (w - 1,0) to ((w - 1) / 2.0f,h), then we should draw an outline
				float max_dist = line_width / 2.0f;
				max_dist *= max_dist;

				Vector2 p = new Vector2(x,y);

				Vector2 tl = Vector2.Zero;
				Vector2 tr = new Vector2(w - 1.0f,0.0f);
				Vector2 bm = new Vector2((w - 1) / 2.0f,h - 1);
				
				// Project the vector from tl to p onto the vector from tl to bm and then subtract that from the vector from tl to p to get the perpendicular vector
				Vector2 tlbm_dist = (p - tl) - (p - tl).Projection(bm - tl);

				if(tlbm_dist.LengthSquared() < max_dist)
					return outline;

				// Check the other line
				Vector2 trbm_dist = (p - tr) - (p - tr).Projection(bm - tr);

				if(trbm_dist.LengthSquared() < max_dist)
					return outline;

				// Now check if we're outside the triangle man
				if((bm - tl).Cross(tlbm_dist) > 0.0f || (bm - tr).Cross(trbm_dist) < 0.0f)
					return Color.Transparent;

				// Otherwise, we're inside the triangle man
				return inside;
			};
		}

		public override void Update(GameTime delta)
		{
			// We may need to generate a mouse target
			// Since it won't take long, just do it here regardles of our behavior
			InputManager Input = Game.Services.GetService<InputManager>();
			Vector2 mouse_pos = new Vector2(Input["MX"].CurrentAnalogValue,Input["MY"].CurrentAnalogValue);

			// Grab the boundary from our game
			Boundary bounds = Game.Services.GetService<Boundary>();

			// A useful variable for later
			Vector2 steering_force = Vector2.Zero;

			// How we behave changes wildly depending on what our behavior is
			switch(Behavior)
			{
			case TriangleManBehavior.UnnaturalSeek:
				steering_force = UnnaturalSeek(delta,mouse_pos);
				break;
			case TriangleManBehavior.BrokenNaturalSeek:
				steering_force = BrokenNaturalSeek(delta,mouse_pos);
				break;
			case TriangleManBehavior.ReasonablyNaturalSeek:
				steering_force = NaturalSeek(delta,mouse_pos);
				break;
			case TriangleManBehavior.Seek:
				steering_force = Seek(delta,mouse_pos);
				break;
			case TriangleManBehavior.Flee:
				steering_force = Flee(delta,mouse_pos);
				break;
			case TriangleManBehavior.SeekWander:
				if(RandomTargetCooldown <= 0.0f)
				{
					RandomTarget = new Vector2(rand.Next(bounds.Left,bounds.Right + 1),rand.Next(bounds.Bottom,bounds.Top + 1));
					RandomTargetCooldown = RandomTargetJumpDelay;
				}
				else
					RandomTargetCooldown -= (float)delta.ElapsedGameTime.TotalSeconds;

				steering_force = Seek(delta,RandomTarget);
				break;
			case TriangleManBehavior.Wander:
				steering_force = Wander(delta);
				break;
			case TriangleManBehavior.Pursuit:
				if(PursuitTarget is null)
					steering_force = Stop(delta);
				else
					steering_force = Pursue(delta,PursuitTarget);

				break;
			case TriangleManBehavior.Evade:
				if(EvasionTarget is null)
					steering_force = Stop(delta);
				else
					steering_force = Evade(delta,EvasionTarget);

				break;
			case TriangleManBehavior.SeekEvade:
				steering_force = Seek(delta,mouse_pos);

				foreach(Vector2 v in SeekFleeTargets)
					steering_force += Flee(delta,v);

				break;
			}

			#region Common Update Logic
			// Truncate our steering force
			steering_force = TruncateVector(steering_force,MaxForce);

			// Apply our steering force
			Velocity += steering_force * (float)delta.ElapsedGameTime.TotalSeconds;

			// Make sure our velocity isn't out of control
			Velocity = TruncateVector(Velocity,MaxVelocity);
			
			// Make sure small velocities get zeroed
			if(Velocity.LengthSquared() < GlobalConstants.EPSILON_SQUARED)
				Velocity = Vector2.Zero;

			// No matter what our behavior is, we can always update position via velocity
			Position += Velocity * (float)delta.ElapsedGameTime.TotalSeconds; // Euler integral is perfectly fine

			// Make sure our position is in bounds
			while(!bounds.Contains(Position))
				if(Position.X < bounds.Left)
					if(Position.Y < bounds.Bottom)
						Position += new Vector2(bounds.Width,bounds.Height);
					else if(Position.Y > bounds.Top)
						Position += new Vector2(bounds.Width,-bounds.Height);
					else
						Position += new Vector2(bounds.Width,0.0f);
				else if(Position.X > bounds.Right)
					if(Position.Y < bounds.Bottom)
						Position += new Vector2(-bounds.Width,bounds.Height);
					else if(Position.Y > bounds.Top)
						Position += new Vector2(-bounds.Width,-bounds.Height);
					else
						Position += new Vector2(-bounds.Width,0.0f);
				else
					if(Position.Y < bounds.Bottom)
						Position += new Vector2(0.0f,bounds.Height);
					else if(Position.Y > bounds.Top)
						Position += new Vector2(0.0f,-bounds.Height);

			// Calculate rotation
			// We want to rotate to the direction of our velocity
			if(Velocity != Vector2.Zero && Behavior != TriangleManBehavior.UnnaturalSeek)
				Rotation = MathF.Atan2(Velocity.X,Velocity.Y); // The triangle initially points down, so we need a 90 degree rotation to get it to align with the x axis

			// Now assign our transform
			Transform = Matrix2D.FromPositionRotationScale(Position,Rotation,Vector2.One,Center);
			#endregion

			return;
		}

		protected Vector2 Stop(GameTime delta)
		{
			if(Velocity != Vector2.Zero)
				return -Velocity;
			
			return Vector2.Zero;
		}

		protected Vector2 UnnaturalSeek(GameTime delta, Vector2 target)
		{
			// Calculate rotation
			Vector2 rot_dir = target - Position;
			Rotation = MathF.Atan2(rot_dir.X,rot_dir.Y); // The triangle initially points down, so we need a 90 degree rotation to get it to align with the x axis

			// Calculate velocity directly
			Velocity = rot_dir.Normalized() * MaxVelocity;

			return Vector2.Zero;
		}

		protected Vector2 BrokenNaturalSeek(GameTime delta, Vector2 target)
		{
			Vector2 desired_vel = (target - Position).Normalized() * MaxVelocity;

			return desired_vel - Velocity;
		}

		protected Vector2 NaturalSeek(GameTime delta, Vector2 target)
		{
			// Calculate velocity
			Vector2 desired_vel = target - Position;
			
			if(desired_vel.LengthSquared() > ArrivalRangeSquared)
				desired_vel = desired_vel.Normalized() * MaxVelocity; // Gotta go fast!
			else
				desired_vel = desired_vel.Normalized() * MaxVelocity * MathF.Sqrt(desired_vel.LengthSquared() / ArrivalRangeSquared); // Linearly scale down our desired velocity the closer we get to our goal
			
			return desired_vel - Velocity;
		}

		protected Vector2 Seek(GameTime delta, Vector2 target)
		{
			// Calculate velocity
			Vector2 desired_vel = target - Position;
			
			if(desired_vel.LengthSquared() > ArrivalRangeSquared)
				desired_vel = desired_vel.Normalized() * MaxVelocity; // Gotta go fast!
			else if(desired_vel.LengthSquared() < InnerArrivalRangeSquared)
				desired_vel = Vector2.Zero; // We've arrived
			else
				desired_vel = desired_vel.Normalized() * MaxVelocity * (desired_vel.Length() - InnerArrivalRange) / ExcellentArrivalDistance; // Linearly scale down our desired velocity the closer we get to our goal
			
			return desired_vel - Velocity;
		}

		protected Vector2 Flee(GameTime delta, Vector2 target)
		{
			// Calculate velocity
			Vector2 desired_vel = Position - target;
			
			if(desired_vel.LengthSquared() > FleeRangeSquared)
				desired_vel = Vector2.Zero; // We've fled
			else
				desired_vel = desired_vel.Normalized() * MaxVelocity * (FleeRange - desired_vel.Length()) / FleeRange; // Linearly scale down our desired velocity the farther we get from our nightmares
			
			return desired_vel - Velocity;
		}

		protected Vector2 Wander(GameTime delta)
		{
			// We want to cast forward along our velocity vector
			// If our velocity is zero, then we'll just take whichever direction we're facing
			Vector2 circle_center;
			
			if(Velocity == Vector2.Zero)
				circle_center = (Matrix2D.Rotation(Rotation) * new Vector2(0,1)).Normalized() * WanderCircleLead;
			else
				circle_center = Velocity.Normalized() * WanderCircleLead;

			// We generate a wander force by applying a velocity displacement in the direction of WanderAngle
			Vector2 wander_force = (Matrix2D.Rotation(WanderAngle) * new Vector2(0,1)).Normalized() * WanderCircleRadius;

			// Change WanderAngle a little bit for next time
			WanderAngle += (rand.NextSingle() - 0.5f) * WanderAngleChange;

			return circle_center + wander_force;
		}

		protected Vector2 Pursue(GameTime delta, TriangleMan target)
		{
			// We "look ahead" to where our target will be later
			// We determine how many frames we need to look ahead based off of how far we are from them now (which is good enough; no need to solve any equations)
			Vector2 future_target = target.Position + target.Velocity * (Position - target.Position).Length() / MaxVelocity;

			return Seek(delta,future_target);
		}

		protected Vector2 Evade(GameTime delta, TriangleMan target)
		{
			// We "look ahead" to where our target will be later
			// We determine how many frames we need to look ahead based off of how far we are from them now (which is good enough; no need to solve any equations)
			Vector2 future_target = target.Position + target.Velocity * (Position - target.Position).Length() / MaxVelocity;

			return Flee(delta,future_target);
		}

		/// <summary>
		/// Truncates a vector to a maximum length.
		/// </summary>
		protected Vector2 TruncateVector(Vector2 v, float max_len)
		{
			if(v.LengthSquared() > max_len * max_len)
				return v.Normalized() * max_len;

			return v;
		}

		/// <summary>
		/// The position of this triangle man.
		/// </summary>
		/// <remarks>We allow ourselves to warp externally.</remarks>
		public Vector2 Position
		{get; set;}

		/// <summary>
		/// The rotation of this triangle man.
		/// </summary>
		/// <remarks>We allow ourselves to warp externally.</remarks>
		public float Rotation
		{get; set;}

		/// <summary>
		/// The current velocity of this triangle man.
		/// </summary>
		public Vector2 Velocity
		{get; set;}

		/// <summary>
		/// This is the max velocity this triangle man can achieve.
		/// </summary>
		public float MaxVelocity
		{get; set;}

		/// <summary>
		/// The maximum force that this trianle man can exert.
		/// </summary>
		public float MaxForce
		{get; set;}

		/// <summary>
		/// This is the max velocity squared this triangle man can achieve.
		/// </summary>
		public float MaxVelocitySquared => MaxVelocity * MaxVelocity;

		/// <summary>
		/// The range at which this begins to slow down.
		/// </summary>
		public float ArrivalRange
		{get; set;}

		/// <summary>
		/// The square range at which this begins to slow down.
		/// </summary>
		public float ArrivalRangeSquared => ArrivalRange * ArrivalRange;

		/// <summary>
		/// The range at which this is considered to have arrived.
		/// </summary>
		public float InnerArrivalRange
		{get; set;}

		/// <summary>
		/// The square range at which this is considered to have arrived.
		/// </summary>
		public float InnerArrivalRangeSquared => InnerArrivalRange * InnerArrivalRange;

		/// <summary>
		/// The distance to arrival using an inner arrival range.
		/// </summary>
		public float ExcellentArrivalDistance => ArrivalRange - InnerArrivalRange;

		/// <summary>
		/// The square distance to arrival using an inner arrival range.
		/// </summary>
		public float ExcellentArrivalDistanceSquared => ExcellentArrivalDistance * ExcellentArrivalDistance;

		/// <summary>
		/// The distance within which a triangle man may choose to flee.
		/// </summary>
		public float FleeRange
		{get; set;}

		/// <summary>
		/// The square distance within which a triangle man may choose to flee.
		/// </summary>
		public float FleeRangeSquared => FleeRange * FleeRange;

		/// <summary>
		/// The behavior of this triangle man.
		/// </summary>
		public TriangleManBehavior Behavior
		{
			get => _b;

			set
			{
				if(_b == value)
					return;

				_b = value;
				
				switch(_b)
				{
				case TriangleManBehavior.Wander:
					WanderAngle = Rotation;
					break;
				}

				return;
			}
		}

		protected TriangleManBehavior _b;

		/// <summary>
		/// The center of this triangle man.
		/// </summary>
		protected Vector2 Center
		{get; set;}

		/// <summary>
		/// It rolls dice for us.
		/// </summary>
		protected Random rand
		{get;}

		/// <summary>
		/// A random target to head toward.
		/// </summary>
		protected Vector2 RandomTarget
		{get; set;}

		/// <summary>
		/// The time before the random target moves again.
		/// </summary>
		protected float RandomTargetCooldown
		{get; set;}

		/// <summary>
		/// The time before a random target jumps to a new position.
		/// </summary>
		protected const float RandomTargetJumpDelay = 3.0f;

		/// <summary>
		/// The lead that the wander displacements will have along the velocity's direction.
		/// Smaller values will cause more dramatic wandering turns, while larger values will cause wanderers to mostly travel straight.
		/// </summary>
		protected float WanderCircleLead
		{get; set;}

		/// <summary>
		/// The radius of the wander circle.
		/// Larger radii will produce more winding paths while smaller values will produce mostly straight wandering paths.
		/// </summary>
		protected float WanderCircleRadius
		{get; set;}

		/// <summary>
		/// The current wander angle (in radians).
		/// </summary>
		protected float WanderAngle
		{get; set;}

		/// <summary>
		/// The range (in radians) in which the wander angle is allowed to change between frames.
		/// </summary>
		protected float WanderAngleChange
		{get; set;}

		/// <summary>
		/// The target to pursue.
		/// If null, no pursuit will occur.
		/// </summary>
		public TriangleMan? PursuitTarget
		{get; set;}

		/// <summary>
		/// The target to evade.
		/// If null, no evasion will occur.
		/// </summary>
		public TriangleMan? EvasionTarget
		{get; set;}

		/// <summary>
		/// A collectin of targets to avoid when seeking while fleeing.
		/// </summary>
		public IEnumerable<Vector2> SeekFleeTargets
		{get; set;}
	}

	/// <summary>
	/// The behavior of a triangle man.
	/// </summary>
	public enum TriangleManBehavior
	{
		UnnaturalSeek,
		BrokenNaturalSeek,
		ReasonablyNaturalSeek,
		Seek,
		Flee,
		SeekWander,
		Wander,
		Pursuit,
		Evade,
		SeekEvade
	}
}
