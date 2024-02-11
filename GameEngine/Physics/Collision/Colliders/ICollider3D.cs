using GameEngine.DataStructures.Geometry;

namespace GameEngine.Physics.Collision.Colliders
{
	/// <summary>
	/// The basic requirements to be a 3D collider.
	/// </summary>
	public interface ICollider3D : ICollider<ICollider3D>
	{
		/// <summary>
		/// Determines if this this collider collides with <paramref name="other"/>.
		/// </summary>
		/// <param name="other">The other collider to check collision against. It is assumed that this collider's bounding box and <paramref name="other"/>'s bounding box intersect. If that is not the case, then this method's behavior is undefined.</param>
		/// <returns>Returns true if the colliders truly intersect and false otherwise.</returns>
		public bool CollidesWith(ICollider3D other);

		/// <summary>
		/// Attemps to change the boundary of this collider.
		/// </summary>
		/// <param name="new_boundary">The new boundary to assign to this collider.</param>
		/// <returns>Returns true if the change was made successfully and false otherwise.</returns>
		public bool ChangeBoundary(FPrism new_boundary);

		/// <summary>
		/// The boundary of this collider.
		/// <para/>
		/// This value is equivalent to checking the four individual boundaries.
		/// </summary>
		public FPrism Boundary
		{get;}

		/// <summary>
		/// The lower x bound of this collider's bounding box.
		/// <para/>
		/// This value must always be strictly less than RightBound.
		/// </summary>
		public float LeftBound
		{get;}
		
		/// <summary>
		/// The upper x bound of this collider's bounding box.
		/// <para/>
		/// This value must always be strictly greater than LeftBound.
		/// </summary>
		public float RightBound
		{get;}

		/// <summary>
		/// The lower y bound of this collider's bounding box.
		/// <para/>
		/// This value must always be strictly less than TopBound.
		/// </summary>
		public float BottomBound
		{get;}

		/// <summary>
		/// The upper y bound of this collider's bounding box.
		/// <para/>
		/// This value must always be strictly greater than BottomBound.
		/// </summary>
		public float TopBound
		{get;}

		/// <summary>
		/// The lower z bound of this collider's bounding box.
		/// <para/>
		/// This value must always be strictly less than FarBound.
		/// </summary>
		public float NearBound
		{get;}

		/// <summary>
		/// The upper z bound of this collider's bounding box.
		/// <para/>
		/// This value must always be strictly greater than NearBound.
		/// </summary>
		public float FarBound
		{get;}
	}
}
