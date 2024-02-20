using GameEngine.DataStructures.Geometry;
using GameEngine.GameComponents;
using GameEngine.Physics.Collision.Colliders;
using GameEngine.Utility.ExtensionMethods.InterfaceFunctions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ExampleQueryCollider
{
	/// <summary>
	/// A collision box.
	/// </summary>
	public class CollisionBox : RectangleComponent, ICollider2D
	{
		/// <summary>
		/// Creates a collision box.
		/// </summary>
		/// <param name="game">The game this component will belong to.</param>
		/// <param name="renderer">The image renderer to draw with (can be changed later).</param>
		/// <param name="w">The width of the collision box generated.</param>
		/// <param name="h">The height of the collision box generated.</param>
		/// <param name="c">The color to draw with in debug mode.</param>
		public CollisionBox(Game game, SpriteBatch? renderer, int w, int h, Color c) : base(game,renderer,w,h,ColorFunctions.Wireframe(w,h,3,c))
		{
			_b = new FRectangle(0,0,w,h);
			PreviousBoundary = FRectangle.Empty;

			OnEnableChanged += (a,b) => {};
			EnabledChanged += (a,b) => OnEnableChanged(this,Enabled);
			OnStaticMovement += (a) => {};
			OnStaticStateChanged += (a,b) => {};
			Resolve += (a,b,c) => {};
			
			ID = ICollider2D.NextID;
			IsStatic = false;

			Visible = false;
			return;
		}

		public bool Equals(ICollider2D? other)
		{return other is CollisionBox c && c.Boundary == Boundary;}

		public bool CollidesWith(ICollider2D other) => true; // We assume there are only CollisionBox colliders at play, and they have true colliders equivalent to their bounding boxes

		public bool ChangeBoundary(FRectangle new_boundary)
		{
			PreviousBoundary = Boundary;
			Boundary = new_boundary; // We would usually want to hook this function into all of our transformation functions, but we just assign this here and forget about it
			
			if(IsStatic)
				OnStaticMovement(this);

			return true;
		}

		public override string ToString() => "ID: " + ID + " Boundary: " + Boundary;

		public FRectangle Boundary
		{
			get => this.GetAffinePosition() + _b; // The boundary is allowed to have a relative offset from the position; this WILL NOT allow you to update static colliders properly with OnStaticMovement, so DON'T DO THAT

			protected set
			{
				_b = value; // We assign the boundary to a relative position to this game object
				return;
			}
		}

		protected FRectangle _b;

		public float LeftBound => Boundary.Left;
		public float RightBound => Boundary.Right;
		public float BottomBound => Boundary.Bottom;
		public float TopBound => Boundary.Top;

		public FRectangle PreviousBoundary
		{get; protected set;}

		public float PreviousLeftBound => PreviousBoundary.Left;
		public float PreviousRightBound => PreviousBoundary.Right;
		public float PreviousBottomBound => PreviousBoundary.Bottom;
		public float PreviousTopBound => PreviousBoundary.Top;

		/// <summary>
		/// The velocity of this box.
		/// </summary>
		public Vector2 Velocity
		{get; set;}

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

		/// <summary>
		/// The means by which we resolve 
		/// </summary>
		public ResolveCollision Resolve
		{get; set;}

		public event EnableChanged<ICollider2D> OnEnableChanged;
		public event StaticStateChanged<ICollider2D> OnStaticStateChanged;
		public event StaticMovement<ICollider2D> OnStaticMovement;
	}

	/// <summary>
	/// Resolves a collision.
	/// </summary>
	/// <param name="me">The collider that owns this collision resolution method.</param>
	/// <param name="other">The collider colliding with <paramref name="me"/>.</param>
	/// <param name="delta">The elapsed time.</param>
	public delegate void ResolveCollision(ICollider2D me, ICollider2D other, GameTime delta);
}
