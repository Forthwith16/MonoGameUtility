using GameEngine.Maths;
using Microsoft.Xna.Framework;

namespace GameEngine.GameComponents
{
	/// <summary>
	/// The base requirements for a component to be affine.
	/// </summary>
	/// <remarks>Monogame's game component classes have reinitialization checks baked into them, so we don't bother doing the same here.</remarks>
	public abstract class AffineComponent : GameComponent, IAffineComponent2D
	{
		/// <summary>
		/// Initializes this component to have the identity transformation and no parent.
		/// </summary>
		/// <param name="game">The game this component will belong to.</param>
		protected AffineComponent(Game game) : base(game)
		{
			WorldRevision = 2;
			InverseWorldRevision = 2;

			Transform = Matrix2D.Identity;
			Parent = null;

			return;
		}

		public Matrix2D Transform
		{
			get => _t;

			set
			{
				// It's not worth checking if _t == value, so we'll just blindly mark these as stale
				StaleInverse = true;

				// Instead of keeping a stale boolean, we double our value in using the revision number to mark these as stale
				ParentWorldRevision = 0u;
				ParentInverseWorldRevision = 0u;

				_t = value;
				return;
			}
		}

		protected Matrix2D _t;

		public Matrix2D World
		{
			get
			{
				if(StaleWorld)
				{
					// Our behavior differs depending on if we have a parent or not
					if(Parent is null)
					{
						ParentWorldRevision = 1u; // A 1 value means our world is up to date when we have no parent
						_w = Transform;
					}
					else
					{
						ParentWorldRevision = Parent.WorldRevision; // Our parent's revision number means our world is up to date when we have a parent
						_w = Parent.World * Transform;
					}

					++WorldRevision;
				}

				return _w;
			}
		}

		protected Matrix2D _w;

		public uint WorldRevision
		{get; protected set;}
		
		/// <summary>
		/// The currently known revision of Parent.World.
		/// A different (higher) value means that our World matrix is stale.
		/// </summary>
		protected uint ParentWorldRevision
		{get; set;}

		/// <summary>
		/// If true, then the world matrix is stale and needs to be updated.
		/// </summary>
		public bool StaleWorld => Parent is null ? ParentWorldRevision != 1u : ParentWorldRevision != Parent.WorldRevision || Parent.StaleWorld;

		public Matrix2D InverseTransform
		{
			get
			{
				if(StaleInverse)
					InverseTransform = Transform.Invert();

				return _it;
			}
			
			private set
			{
				_it = value;
				StaleInverse = false;

				return;
			}
		}

		protected Matrix2D _it;

		/// <summary>
		/// If true, then our inverse is stale an needs to be recalculated.
		/// If false, then our inverse is up to date.
		/// </summary>
		private bool StaleInverse
		{get; set;}

		public Matrix2D InverseWorld
		{
			get
			{
				if(StaleInverseWorld)
				{
					// Our behavior differs depending on if we have a parent or not
					if(Parent is null)
					{
						ParentInverseWorldRevision = 1u; // A 1 value means our inverse world is up to date when we have no parent
						_iw = InverseTransform;
					}
					else
					{
						ParentInverseWorldRevision = Parent.InverseWorldRevision; // Our parent's revision number means our inverse world is up to date when we have a parent
						_iw = InverseTransform * Parent.InverseWorld;
					}

					++InverseWorldRevision;
				}

				return _iw;
			}
		}

		protected Matrix2D _iw;

		public uint InverseWorldRevision
		{get; protected set;}

		/// <summary>
		/// If true, then the inverse world matrix is stale and needs to be updated.
		/// </summary>
		public bool StaleInverseWorld => StaleInverse || (Parent is null ? ParentInverseWorldRevision != 1u : ParentInverseWorldRevision != Parent.InverseWorldRevision || Parent.StaleInverseWorld);

		/// <summary>
		/// The currently known revision of Parent.InverseWorld.
		/// A different (higher) value means that our World matrix is stale.
		/// </summary>
		protected uint ParentInverseWorldRevision
		{get; set;}

		public IAffineComponent2D? Parent
		{
			get => _p;
			
			set
			{
				ParentWorldRevision = 0u;
				ParentInverseWorldRevision = 0u;

				_p = value;
				return;
			}
		}

		protected IAffineComponent2D? _p;
	}
}
