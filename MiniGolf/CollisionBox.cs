using GameEngine.GameObjects;
using GameEngine.Texture;
using GameEngine.Utility.ExtensionMethods.PrimitiveExtensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MiniGolf
{
	/// <summary>
	/// A collision box.
	/// </summary>
	public class CollisionBox : RectangleGameObject, ICollider
	{
		/// <summary>
		/// Creates a collision box.
		/// </summary>
		/// <param name="renderer">The image renderer to draw with (can be changed later).</param>
		/// <param name="w">The width of the collision box generated.</param>
		/// <param name="h">The height of the collision box generated.</param>
		/// <param name="c">The color to draw with in debug mode.</param>
		public CollisionBox(SpriteBatch? renderer, int w, int h, Color c) : base(renderer,w,h,ColorFunctions.Wireframe(w,h,1,c))
		{
			_width = w;
			_hw = _width / 2.0f;
			
			_h = h;
			_hh = h / 2.0f;

			_hd = new Vector2(_hw,_hh);

			Resolve += (a,b,c) => {};
			KineticFriction = 0.0f;

			return;
		}

		/// <summary>
		/// Draws the wireframe for the collision box iff in debug mode.
		/// </summary>
		/// <param name="delta">The elapsed game time since the last draw.</param>
		public override void Draw(GameTime delta)
		{
			#if DEBUG
			base.Draw(delta);
			#endif
			
			return;
		}

		public bool ContainsPoint(Vector2 v)
		{
			Vector2 p = InverseWorld * v;
			return p.X > 0.0f && p.X < Width && p.Y > 0.0f && p.Y < Height; // To be colliding, we require that it is fully inside of the box, not just on a boundary
		}

		public bool IntersectsCircle(Vector2 center, float radius)
		{
			// Reduce this problem to the first quadrant
			Vector2 dist = (HalfDim - InverseWorld * center).Abs();

			// We need these values potentialy more than once, so store them
			Vector2 edge_dist = dist - HalfDim;

			// We can quick and dirty check to see if we're definitely too far away
			if(edge_dist.X >= radius || edge_dist.Y >= radius) // We don't accept boundary touching as an intersection
				return false;

			// The other easy case is when we're definitely close enough
			if(edge_dist.X <= 0.0f || edge_dist.Y <= 0.0f)
				return true;

			// Now we know that we need to check if the top right point it contained within the circle (which will be the case if the box and circle intersect)
			return edge_dist.LengthSquared() < radius * radius;
		}

		/// <summary>
		/// The width of the collision box.
		/// </summary>
		public override int Width => _width;
		protected int _width;

		/// <summary>
		/// Half of the width of the collision box.
		/// </summary>
		public float HalfWidth => _hw;
		protected float _hw;

		/// <summary>
		/// The height of the collision box.
		/// </summary>
		public override int Height => _h;
		protected int _h;

		/// <summary>
		/// Half of the height of the collision box.
		/// </summary>
		public float HalfHeight => _hh;
		protected float _hh;

		/// <summary>
		/// The half dimensions as a vector.
		/// </summary>
		public Vector2 HalfDim => _hd;
		protected Vector2 _hd;

		public float KineticFriction
		{get; set;}

		public ResolveCollision Resolve
		{get; set;}
    }
}
