using GameEngine.Framework;
using GameEngine.GameObjects;
using GameEngine.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MiniGolf
{
	/// <summary>
	/// Represents a golf ball.
	/// </summary>
	public class GolfBall : ImageGameObject
	{
		/// <summary>
		/// Creates a new golf ball.
		/// </summary>
		/// <param name="renderer">The means by which this golf ball will be rendered. This can be changed later.</param>
		/// <param name="texture">The golf ball texture.</param>
		public GolfBall(SpriteRenderer? renderer, Texture2D texture) : base(renderer,texture)
		{
			_v = Vector2.Zero;
			return;
		}

		/// <summary>
		/// Creates a new golf ball.
		/// </summary>
		/// <param name="renderer">The means by which this golf ball will be rendered. This can be changed later.</param>
		/// <param name="texture">The texture resource to load later.</param>
		public GolfBall(SpriteRenderer? renderer, string resource) : base(renderer,resource)
		{
			_v = Vector2.Zero;
			return;
		}

		/// <summary>
		/// Determines if <paramref name="p"/> lies inside this golf ball.
		/// It is assumed that the golf ball is a circle.
		/// </summary>
		/// <param name="p">The point to check for containment.</param>
		/// <returns>Returns true if the point is inside this golf ball (or on the boundary) and false otherwise.</returns>
		public bool ContainsPoint(Vector2 p)
		{return (Position - p).LengthSquared() < Radius * Radius;}

		/// <summary>
		/// The position of this golf ball.
		/// </summary>
		public Vector2 Position => Transform * HalfDim;

		/// <summary>
		/// The velocity of this golf ball.
		/// </summary>
		public Vector2 Velocity
		{
			get => _v;
			
			set
			{
				_v = value;

				if(Still)
					_v = Vector2.Zero;

				return;
			}
		}

		protected Vector2 _v;

		/// <summary>
		/// Determines if the ball is still enough to assume that its velocity IS zero.
		/// </summary>
		public bool Still => Velocity.LengthSquared() < GlobalConstants.EPSILON * GlobalConstants.EPSILON;

		/// <summary>
		/// The radius of the golf ball.
		/// </summary>
		public float Radius => (Width / 4.0f) + (Height / 4.0f);

		/// <summary>
		/// The half dimensions as a vector.
		/// </summary>
		public Vector2 HalfDim => new Vector2(Width / 2.0f,Height / 2.0f);
	}
}
