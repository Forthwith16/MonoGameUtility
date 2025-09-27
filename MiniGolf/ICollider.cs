using Microsoft.Xna.Framework;

namespace MiniGolf
{
	/// <summary>
	/// Defines what it means to be a collider.
	/// </summary>
	public interface ICollider
	{
		/// <summary>
		/// Determines if this collider contains the point <paramref name="v"/>.
		/// </summary>
		/// <param name="v">The point to check for containment.</param>
		/// <returns>Returns true if this collider contains <paramref name="v"/> and false otherwise.</returns>
		public bool ContainsPoint(Vector2 v);

		/// <summary>
		/// Determines if this collider intersects with the circle centered at <paramref name="center"/> with radius <paramref name="radius"/>.
		/// </summary>
		/// <param name="center">The circle center.</param>
		/// <param name="radius">The circle radius.</param>
		/// <returns>Returns true if the circle intersects with this collider and false otherwise.</returns>
		public bool IntersectsCircle(Vector2 center, float radius);

		/// <summary>
		/// The coefficient of kinetic friction.
		/// </summary>
		public float KineticFriction
		{get; set;}

		/// <summary>
		/// The means by which collisions are resolved with golf balls.
		/// </summary>
		public ResolveCollision Resolve
		{get; set;}
	}

	/// <summary>
	/// Resolves a collision with a golf ball.
	/// </summary>
	/// <param name="me">The collider involved.</param>
	/// <param name="g">The golf ball colliding with it.</param>
	/// <param name="delta">The elapsed time.</param>
	public delegate void ResolveCollision(ICollider me, GolfBall g, GameTime delta);
}
