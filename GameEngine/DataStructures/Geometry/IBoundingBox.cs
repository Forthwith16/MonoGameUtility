namespace GameEngine.DataStructures.Geometry
{
	/// <summary>
	/// The outline of what it means to be a bounding box.
	/// </summary>
	/// <typeparam name="T">The type of bounding box used.</typeparam>
	public interface IBoundingBox<T> : IEquatable<T> where T : struct, IBoundingBox<T>
	{
		/// <summary>
		/// Unions this bounding box with <paramref name="bb"/>.
		/// </summary>
		/// <returns>Returns the union of this with <paramref name="bb"/> as a new bounding box.</returns>
		public T Union(T bb);

		/// <summary>
		/// Intersects this bounding box with <paramref name="bb"/> and returns the result as a new bounding box.
		/// </summary>
		/// <param name="bb">The bounding box to intersect with.</param>
		/// <returns>Returns the resulting intersection or the empty bounding box if this bounding box and <paramref name="bb"/> do not intersect.</returns>
		public T Intersection(T bb);

		/// <summary>
		/// Determines if this bounding box intersects with <paramref name="bb"/>.
		/// </summary>
		/// <returns>Returns true if the two bounding boxes intersect nontrivially (with nonzero area) and false otherwise.</returns>
		public bool Intersects(T r);

		/// <summary>
		/// Determines if this bounding box contains the bounding box <paramref name="bb"/>.
		/// </summary>
		/// <returns>Returns true if this bounding box contains <paramref name="bb"/> and false otherwise.</returns>
		public bool Contains(T bb);

		/// <summary>
		/// The length/area/space/etc of this bounding box.
		/// </summary>
		public float BoxSpace
		{get;}

		/// <summary>
		/// Determines if this bounding box is empty.
		/// </summary>
		public bool IsEmpty
		{get;}

		/// <summary>
		/// The empty bounding box.
		/// </summary>
		public T EmptyBox
		{get;}
	}
}
