using GameEngine.Maths;

namespace GameEngine.GameComponents
{
	/// <summary>
	/// Defines the bare bones of what it means to be an affine component in two dimensions.
	/// Affine components may be translated, rotated, or scaled.
	/// <para/>
	/// The useful functions are extension methods found in GameEngine.Utility.ExtensionMethods.InterfaceFunctions.AffineExtensions.
	/// These provide common code for a number of common transformations.
	/// </summary>
	public interface IAffineComponent2D
	{
		/// <summary>
		/// The transformation matrix of this component absent its parent transform (if it has one).
		/// <para/>
		/// For the transformation matrix containing the parent (if present), use World instead.
		/// </summary>
		public Matrix2D Transform
		{get; set;}

		/// <summary>
		/// The transformation matrix of this component.
		/// If this component has a parent, this includes the parent transform.
		/// The parent transform is applied second when it exists with this component's transform being applied first (i.e. Transform is left multiplied by Parent.Transform).
		/// <para/>
		/// For the raw transformation matrix, use Transform instead.
		/// </summary>
		public Matrix2D World
		{get;}

		/// <summary>
		/// This is the revision number of the World matrix.
		/// Revision number 0 is reserved for stale matrices, and revision number 1 is reserved for up-to-date matrices with no parent.
		/// This number should thus always start from at least 2.
		/// </summary>
		/// <remarks>This is used purely for matrix caching. The average user need not be concerned with it.</remarks>
		public uint WorldRevision
		{get;}

		/// <summary>
		/// If true, then the world matrix is stale and needs to be updated.
		/// </summary>
		/// <remarks>This is used purely for matrix caching. The average user need not be concerned with it.</remarks>
		public bool StaleWorld
		{get;}

		/// <summary>
		/// The inverse transform of this component's Transform absent its parent transform (if it has one).
		/// <para/>
		/// For the inverse transformation matrix containing the parent (if present), use InverseWorld instead.
		/// </summary>
		public Matrix2D InverseTransform
		{get;}

		/// <summary>
		/// The inverse transformation matrix of this component.
		/// If this component has a parent, this includes the parent's inverse transformation.
		/// The parent transform is applied first in the inverse when it exists with this component's transform being applied last (i.e. InverseTransform is right multiplied by Parent.InverseTransform).
		/// <para/>
		/// For the raw inverse transformation matrix, use InverseTransform instead.
		/// </summary>
		public Matrix2D InverseWorld
		{get;}

		/// <summary>
		/// This is the revision number of the InverseWorld matrix.
		/// Revision number 0 is reserved for stale matrices, and revision number 1 is reserved for up-to-date matrices with no parent.
		/// This number should thus always start from at least 2.
		/// </summary>
		/// <remarks>This is used purely for matrix caching. The average user need not be concerned with it.</remarks>
		public uint InverseWorldRevision
		{get;}

		/// <summary>
		/// If true, then the inverse world matrix is stale and needs to be updated.
		/// </summary>
		/// <remarks>This is used purely for matrix caching. The average user need not be concerned with it.</remarks>
		public bool StaleInverseWorld
		{get;}

		/// <summary>
		/// The parent of this component.
		/// Transforming the parent will transform this object in turn.
		/// </summary>
		public IAffineComponent2D? Parent
		{get; set;}
	}
}
