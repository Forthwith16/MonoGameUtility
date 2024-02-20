using GameEngine.DataStructures.Geometry;

namespace GameEngine.Physics.Collision.Colliders
{
	/// <summary>
	/// The basic requirements to be a 2D collider.
	/// </summary>
	public interface ICollider2D : ICollider<ICollider2D>
	{
		/// <summary>
		/// Determines if this collider collides with <paramref name="other"/>.
		/// </summary>
		/// <param name="other">The other collider to check collision against. It is assumed that this collider's bounding box and <paramref name="other"/>'s bounding box intersect. If that is not the case, then this method's behavior is undefined.</param>
		/// <returns>Returns true if the colliders truly intersect and false otherwise.</returns>
		public bool CollidesWith(ICollider2D other);

		/// <summary>
		/// The axis-aligned boundary of this collider in world coordinates.
		/// <para/>
		/// This value is equivalent to checking the four individual boundaries.
		/// </summary>
		public FRectangle Boundary
		{get;}

		/// <summary>
		/// The axis-aligned lower x bound in world coordinates of this collider's bounding box.
		/// <para/>
		/// This value must always be strictly less than RightBound.
		/// </summary>
		public float LeftBound
		{get;}
		
		/// <summary>
		/// The axis-aligned upper x bound in world coordinates of this collider's bounding box.
		/// <para/>
		/// This value must always be strictly greater than LeftBound.
		/// </summary>
		public float RightBound
		{get;}

		/// <summary>
		/// The axis-aligned lower y bound in world coordinates of this collider's bounding box.
		/// <para/>
		/// This value must always be strictly less than TopBound.
		/// </summary>
		public float BottomBound
		{get;}

		/// <summary>
		/// The axis-aligned upper y bound in world coordinates of this collider's bounding box.
		/// <para/>
		/// This value must always be strictly greater than BottomBound.
		/// </summary>
		public float TopBound
		{get;}

		/// <summary>
		/// The previous axis-aligned boundary of this collider in world cordinates.
		/// If there was no previous value, then this is FRectangle.Empty.
		/// </summary>
		public FRectangle PreviousBoundary
		{get;}

		/// <summary>
		/// The previous axis-aligned lower x bound in world coordinates of this collider's bounding box.
		/// <para/>
		/// This value must always be strictly less than RightBound.
		/// <para/>
		/// If there is no previous value, then this is 0.0f.
		/// </summary>
		public float PreviousLeftBound
		{get;}
		
		/// <summary>
		/// The previous axis-aligned upper x bound in world coordinates of this collider's bounding box.
		/// <para/>
		/// This value must always be strictly greater than LeftBound.
		/// <para/>
		/// If there is no previous value, then this is 0.0f.
		/// </summary>
		public float PreviousRightBound
		{get;}

		/// <summary>
		/// The previous axis-aligned lower y bound in world coordinates of this collider's bounding box.
		/// <para/>
		/// This value must always be strictly less than TopBound.
		/// <para/>
		/// If there is no previous value, then this is 0.0f.
		/// </summary>
		public float PreviousBottomBound
		{get;}

		/// <summary>
		/// The previous axis-aligned upper y bound in world coordinates of this collider's bounding box.
		/// <para/>
		/// This value must always be strictly greater than BottomBound.
		/// <para/>
		/// If there is no previous value, then this is 0.0f.
		/// </summary>
		public float PreviousTopBound
		{get;}
	}
}
