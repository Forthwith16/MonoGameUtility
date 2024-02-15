using GameEngine.Framework;
using GameEngine.GameComponents;
using GameEngine.Input;
using GameEngine.Maths;
using GameEngine.Utility.ExtensionMethods.PrimitiveExtensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

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
			ArrivalRange = 420.0f;
			InnerArrivalRange = 80.0f;

			Behavior = TriangleManBehavior.UnnaturalSeek;

			LayerDepth = 1.0f;
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
			// How we behave changes wildly depending on what our behavior is
			switch(Behavior)
			{
			case TriangleManBehavior.UnnaturalSeek:
				UnnaturalSeek(delta);
				break;
			case TriangleManBehavior.BrokenNaturalSeek:
				BrokenNaturalSeek(delta);
				break;
			case TriangleManBehavior.NaturalSeek:
				NaturalSeek(delta);
				break;
			case TriangleManBehavior.ExcellentSeek:
				ExcellentSeek(delta);
				break;
			}

			#region Common Update Logic
			// But no matter what our behavior is, we can always update position via velocity
			if(Velocity.LengthSquared() < GlobalConstants.EPSILON_SQUARED)
				Velocity = Vector2.Zero;

			Position += Velocity * (float)delta.ElapsedGameTime.TotalSeconds; // Euler integral is perfectly fine

			// Make sure our position is in bounds
			Boundary bounds = Game.Services.GetService<Boundary>();

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

		protected void UnnaturalSeek(GameTime delta)
		{
			InputManager Input = Game.Services.GetService<InputManager>();
			Vector2 mouse_pos = new Vector2(Input["MX"].CurrentAnalogValue,Input["MY"].CurrentAnalogValue);

			// Calculate rotation
			Vector2 rot_dir = mouse_pos - Position;
			Rotation = MathF.Atan2(rot_dir.X,rot_dir.Y); // The triangle initially points down, so we need a 90 degree rotation to get it to align with the x axis

			// Calculate velocity
			Velocity = rot_dir.Normalized() * MaxVelocity;

			return;
		}

		protected void BrokenNaturalSeek(GameTime delta)
		{
			InputManager Input = Game.Services.GetService<InputManager>();
			Vector2 mouse_pos = new Vector2(Input["MX"].CurrentAnalogValue,Input["MY"].CurrentAnalogValue);

			// Calculate velocity
			Vector2 desired_vel = (mouse_pos - Position).Normalized() * MaxVelocity;
			Vector2 steering_dir = desired_vel - Velocity;

			Velocity += steering_dir * (float)delta.ElapsedGameTime.TotalSeconds;

			// Make sure our velocity isn't out of control
			if(Velocity.LengthSquared() > MaxVelocitySquared)
				Velocity = Velocity.Normalized() * MaxVelocity;

			return;
		}

		protected void NaturalSeek(GameTime delta)
		{
			InputManager Input = Game.Services.GetService<InputManager>();
			Vector2 mouse_pos = new Vector2(Input["MX"].CurrentAnalogValue,Input["MY"].CurrentAnalogValue);

			// Calculate velocity
			Vector2 desired_vel = mouse_pos - Position;
			
			if(desired_vel.LengthSquared() > ArrivalRangeSquared)
				desired_vel = desired_vel.Normalized() * MaxVelocity; // Gotta go fast!
			else
				desired_vel = desired_vel.Normalized() * MaxVelocity * MathF.Sqrt(desired_vel.LengthSquared() / ArrivalRangeSquared); // Linearly scale down our desired velocity the closer we get to our goal
			
			Vector2 steering_dir = desired_vel - Velocity;

			Velocity += steering_dir * (float)delta.ElapsedGameTime.TotalSeconds;

			// Make sure our velocity isn't out of control
			if(Velocity.LengthSquared() > MaxVelocitySquared)
				Velocity = Velocity.Normalized() * MaxVelocity;

			return;
		}

		protected void ExcellentSeek(GameTime delta)
		{
			InputManager Input = Game.Services.GetService<InputManager>();
			Vector2 mouse_pos = new Vector2(Input["MX"].CurrentAnalogValue,Input["MY"].CurrentAnalogValue);

			// Calculate velocity
			Vector2 desired_vel = mouse_pos - Position;
			
			if(desired_vel.LengthSquared() > ArrivalRangeSquared)
				desired_vel = desired_vel.Normalized() * MaxVelocity; // Gotta go fast!
			else if(desired_vel.LengthSquared() < InnerArrivalRangeSquared)
				desired_vel = Vector2.Zero; // We've arrived
			else
				desired_vel = desired_vel.Normalized() * MaxVelocity * (desired_vel.Length() - InnerArrivalRange) / ExcellentArrivalDistance; // Linearly scale down our desired velocity the closer we get to our goal
			
			Vector2 steering_dir = desired_vel - Velocity;

			Velocity += steering_dir * (float)delta.ElapsedGameTime.TotalSeconds;

			// Make sure our velocity isn't out of control
			if(Velocity.LengthSquared() > MaxVelocitySquared)
				Velocity = Velocity.Normalized() * MaxVelocity;

			return;
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
		/// The behavior of this triangle man.
		/// </summary>
		public TriangleManBehavior Behavior
		{get; set;}

		/// <summary>
		/// The center of this triangle man.
		/// </summary>
		protected Vector2 Center
		{get; set;}
	}

	/// <summary>
	/// The behavior of a triangle man.
	/// </summary>
	public enum TriangleManBehavior
	{
		UnnaturalSeek,
		BrokenNaturalSeek,
		NaturalSeek,
		ExcellentSeek
	}
}
