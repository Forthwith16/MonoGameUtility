using GameEngine.GameObjects;
using GameEngine.Texture;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MiniGolf
{
	/// <summary>
	/// A minigolf hole.
	/// </summary>
	public class Hole : RectangleGameObject, ICollider
	{
		/// <summary>
		/// Creates a minigold hole.
		/// </summary>
		/// <param name="renderer">The way to render this hole. This can be changed later.</param>
		/// <param name="d">The diameter of the hole.</param>
		public Hole(Game game, SpriteBatch? renderer, int d) : base(renderer,d,d,ColorFunctions.Ellipse(d,d,Color.Gray))
		{
			Radius = d >> 1;

			Resolve += (a,b,c) => {};
			KineticFriction = 0.0f;
			
			Capture += (a,b) => {};

			return;
		}

		public bool ContainsPoint(Vector2 v)
		{return (InverseWorld * v - new Vector2(Radius,Radius)).LengthSquared() <= Radius * Radius;}

		public bool IntersectsCircle(Vector2 center, float radius)
		{return (center - Position).LengthSquared() <= (radius + Radius) * (radius + Radius);}

		/// <summary>
		/// The position of this hole.
		/// </summary>
		public Vector2 Position => Transform * new Vector2(Radius);

		public float KineticFriction
		{get; set;}

		public ResolveCollision Resolve
		{get; set;}

		/// <summary>
		/// The radius of the hole.
		/// </summary>
		public float Radius
		{get;}

		/// <summary>
		/// An event called when this hole captures a golf ball.
		/// </summary>
		public CaptureBall Capture;
	}

	/// <summary>
	/// An event called when a hole captures a ball.
	/// </summary>
	/// <param name="me">The hole.</param>
	/// <param name="ball">The ball.</param>
	public delegate void CaptureBall(Hole me, GolfBall ball);
}
