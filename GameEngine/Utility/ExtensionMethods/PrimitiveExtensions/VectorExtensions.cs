﻿using GameEngine.Framework;
using Microsoft.Xna.Framework;

namespace GameEngine.Utility.ExtensionMethods.PrimitiveExtensions
{
	/// <summary>
	/// Provides extensions to Vector classes.
	/// </summary>
	public static class VectorExtensions
	{
		/// <summary>
		/// Returns a new vector with components equal to the magnitude of each original component.
		/// In short, computes the absolute value of each component.
		/// </summary>
		/// <param name="v">The vector to take the absolute value of.</param>
		/// <returns>Returns a new vector whose components are the absolute value of the original vector's.</returns>
		public static Vector2 Abs(this Vector2 v)
		{return new Vector2(MathF.Abs(v.X),MathF.Abs(v.Y));}

		/// <summary>
		/// Computes the cross product of <paramref name="a"/> and <paramref name="b"/> with each vector being extended into 3-space with a z component of 0.
		/// </summary>
		/// <param name="a">The first vector.</param>
		/// <param name="b">The second vector.</param>
		/// <returns>Returns the value of the (only non-zero) z-component of the cross product.</returns>
		public static float Cross(this Vector2 a, Vector2 b)
		{return a.X * b.Y - a.Y * b.X;}

		/// <summary>
		/// Computes the cross product of <paramref name="a"/> and <paramref name="b"/>.
		/// </summary>
		/// <param name="a">The first vector.</param>
		/// <param name="b">The second vector.</param>
		/// <returns>Returns the cross product.</returns>
		public static Vector3 Cross(this Vector3 a, Vector3 b)
		{return new Vector3(a.Y * b.Z - a.Z * b.Y,a.Z * b.X - a.X * b.Z,a.X * b.Y - a.Y * b.X);}

		/// <summary>
		/// Computes the dot product of <paramref name="a"/> and <paramref name="b"/>.
		/// </summary>
		/// <param name="a">The first vector.</param>
		/// <param name="b">The second vector.</param>
		/// <returns>Returns the dot product of the inputs.</returns>
		public static float Dot(this Vector2 a, Vector2 b)
		{return a.X * b.X + a.Y * b.Y;}

		/// <summary>
		/// Returns a new normalized version of this vector.
		/// </summary>
		/// <returns>Returns the normalized vector or the zero vector if this was the zero vector.</returns>
		public static Vector2 Normalized(this Vector2 me)
		{
			if(me.LengthSquared() < GlobalConstants.EPSILON)
				return Vector2.Zero;

			return me / me.Length();
		}

		/// <summary>
		/// Projects <paramref name="me"/> onto the vector <paramref name="onto"/>.
		/// </summary>
		/// <param name="me">The vector to project.</param>
		/// <param name="onto">The vector to project onto.</param>
		/// <returns>Returns the projected vector.</returns>
		public static Vector2 Projection(this Vector2 me, Vector2 onto)
		{return me.Dot(onto) / onto.LengthSquared() * onto;}

		/// <summary>
		/// Projects <paramref name="me"/> onto the vector <paramref name="onto"/> and returns its <i>signed</i> length.
		/// </summary>
		/// <param name="me">The vector to project.</param>
		/// <param name="onto">The vector to project onto.</param>
		/// <returns>Returns the signed length of the projected vector.</returns>
		public static float ProjectionLength(this Vector2 me, Vector2 onto)
		{return me.Dot(onto) / onto.Length();}

		/// <summary>
		/// Reflects this vector across a line with direction specified by <paramref name="line"/>.
		/// For example, reflecting the vector (1,1) across the line in the direction (1,0) produces the vector (1,-1).
		/// </summary>
		/// <param name="me"></param>
		/// <param name="line">The line direction.</param>
		/// <returns>Returns a new vector reflected across the line or the zero vector if <paramref name="line"/> is the zero vector.</returns>
		public static Vector2 Reflect(this Vector2 me, Vector2 line)
		{
			if(line.LengthSquared() < GlobalConstants.EPSILON * GlobalConstants.EPSILON)
				return Vector2.Zero;

			Vector2 projection = me.Projection(line);
			return 2.0f * projection - me;
		}
	}
}