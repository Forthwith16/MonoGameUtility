namespace GameEngine.Physics.Collision.Colliders
{
	/// <summary>
	/// The basic requirements to be a collider.
	/// </summary>
	/// <remarks>Equality should be checked via ID comparison.</remarks>
	public interface ICollider<T> : IEquatable<T> where T : ICollider<T>
	{
		/// <summary>
		/// The <b><u>unique</u></b> ID of this collider.
		/// <para/>
		/// In general, the collider ID should never change.
		/// However, it must not change during active collision resolution.
		/// </summary>
		public ColliderID<T> ColliderID
		{get;}

		/// <summary>
		/// If true, then this collider is enabled and can be collided with.
		/// If false, then this collider is disabled and cannot be collided with.
		/// </summary>
		public bool Enabled
		{get; set;}

		/// <summary>
		/// If true, then this collider is disabled and cannot be collided with.
		/// If false, then this collider is enabled and can be collided with.
		/// </summary>
		public bool Disabled
		{
			get => !Enabled;
			
			set
			{
				Enabled = !value;
				return;
			}
		}

		/// <summary>
		/// If true, then this collider will not move during physics simluations.
		/// This value is always the opposite of IsKinetic.
		/// </summary>
		public bool IsStatic
		{get; set;}

		/// <summary>
		/// If true, then this collider will move during physics simulations.
		/// This value is always the opposite of IsStatic.
		/// </summary>
		public bool IsKinetic
		{
			get => !IsStatic;

			set
			{
				IsStatic = !value;
				return;
			}
		}

		/// <summary>
		/// This event is called when this collider is enabled or disabled.
		/// </summary>
		public event EnableChanged<T> OnEnableChanged;

		/// <summary>
		/// This event is called whenever this collider's static state changes.
		/// </summary>
		public event StaticStateChanged<T> OnStaticStateChanged;

		/// <summary>
		/// This event is called whenever a static collider is moved.
		/// </summary>
		public event StaticMovement<T> OnStaticMovement;
	}

	/// <summary>
	/// An event called when a collider changes its enabled state.
	/// </summary>
	/// <param name="me">The collider whose enabled state has changed.</param>
	/// <param name="enabled">The current enabled state of the collider.</param>
	public delegate void EnableChanged<T>(T me, bool enabled) where T : ICollider<T>;

	/// <summary>
	/// An event called when the static state of a collider changes.
	/// </summary>
	/// <param name="me">The collider whose state has changed.</param>
	/// <param name="static_state">The current static state of the collider.</param>
	public delegate void StaticStateChanged<T>(T me, bool static_state) where T : ICollider<T>;

	/// <summary>
	/// An event called when a static collider is moved (or when its bounding box is otherwise modified).
	/// </summary>
	/// <param name="me">The collider that moved.</param>
	public delegate void StaticMovement<T>(T me) where T : ICollider<T>;
}
