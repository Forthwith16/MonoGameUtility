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

		/// <summary>
		/// Makes a deep copy of <paramref name="other"/>.
		/// This will not copy events, however, nor will it initialize or dispose of the copy if <paramref name="other"/> is in either state.
		/// The parent will be <b><u>shallow</u></b> copied, whether it is null or otherwise.
		/// </summary>
		protected AffineComponent(AffineComponent other) : base(other.Game)
		{
			Enabled = other.Enabled;
			UpdateOrder = other.UpdateOrder;

			_t = other._t;
			_w = other._w;
			_it = other._it;
			_iw = other._iw;

			WorldRevision = other.WorldRevision;
			ParentWorldRevision = other.ParentWorldRevision;

			StaleInverse = other.StaleInverse;
			InverseWorldRevision = other.InverseWorldRevision;
			ParentInverseWorldRevision = other.InverseWorldRevision;

			_p = other._p;

			Initialized = false;
			return;
		}

		public override void Initialize()
		{
			if(Initialized)
				return;

			Initialized = true;
			base.Initialize();

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

		/// <summary>
		/// If true, then the transform itself is stale and needs to be updated.
		/// </summary>
		/// <remarks>
		///	This value is always false in its initial implementation.
		///	Override this in derived classes if Transform depends on values other than itself.
		///	You should also override Transform's get so that Transform is properly set before any call to its get returns.
		/// </remarks>
		public virtual bool StaleTransform => false;

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
		public bool StaleWorld => StaleTransform || (Parent is null ? ParentWorldRevision != 1u : ParentWorldRevision != Parent.WorldRevision || Parent.StaleWorld);

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
		/// <para/>
		/// Setting this will set _si.
		/// </summary>
		private bool StaleInverse
		{
			get => StaleTransform || _si;
			set => _si = value;
		}

		private bool _si;

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

		/// <summary>
		/// If true, then this has been initialized.
		/// If false, it has not.
		/// </summary>
		public bool Initialized
		{get; protected set;}
	}
}
