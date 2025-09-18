using GameEngine.Maths;
using System.Diagnostics.CodeAnalysis;

// This is the backing type for revision values
// Change this if you need more or fewer revision values
using RevisionType = uint;

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
		public RevisionID WorldRevision
		{get;}

		/// <summary>
		/// If true, then the world matrix is stale and needs to be updated.
		/// </summary>
		/// <remarks>This is used purely for matrix caching. The average user need not be concerned with it. It also must be expsed in order to recursively check staleness.</remarks>
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
		public RevisionID InverseWorldRevision
		{get;}

		/// <summary>
		/// If true, then the inverse world matrix is stale and needs to be updated.
		/// </summary>
		/// <remarks>This is used purely for matrix caching. The average user need not be concerned with it. It also must be expsed in order to recursively check staleness.</remarks>
		public bool StaleInverseWorld
		{get;}

		/// <summary>
		/// The parent of this component.
		/// Transforming the parent will transform this object in turn.
		/// </summary>
		public IAffineComponent2D? Parent
		{get; set;}
	}

	/// <summary>
	/// A struct wrapper for revision IDs.
	/// <para/>
	/// Revision number 0 is reserved for stale matrices, and revision number 1 is reserved for up-to-date matrices with no parent.
	/// Revision numbers thus always start from at least 2.
	/// </summary>
	public readonly struct RevisionID : IComparable<RevisionID>, IEquatable<RevisionID>
	{
		/// <summary>
		/// Creates an ID.
		/// </summary>
		/// <param name="id">The raw ID value.</param>
		public RevisionID(RevisionType id)
		{
			ID = id;
			return;
		}

		public static implicit operator RevisionID(RevisionType id) => new RevisionID(id);
		public static explicit operator RevisionType(RevisionID id) => id.ID;

		public static RevisionID operator ++(RevisionID id) => new RevisionID(id.ID + 1);

		public static bool operator ==(RevisionID a, RevisionID b) => a.ID == b.ID;
		public static bool operator !=(RevisionID a, RevisionID b) => a.ID != b.ID;

		public bool Equals(RevisionID other) => this == other;
		public override bool Equals([NotNullWhen(true)] object? obj) => obj is RevisionID id && this == id;

		public static bool operator <(RevisionID a, RevisionID b) => a.ID < b.ID;
		public static bool operator <=(RevisionID a, RevisionID b) => a.ID <= b.ID;
		public static bool operator >(RevisionID a, RevisionID b) => a.ID > b.ID;
		public static bool operator >=(RevisionID a, RevisionID b) => a.ID >= b.ID;

		public int CompareTo(RevisionID other) => ID.CompareTo(other.ID);

		public override int GetHashCode() => ID.GetHashCode();
		public override string ToString() => ID.ToString();

		/// <summary>
		/// The ID itself.
		/// </summary>
		public readonly RevisionType ID;

		/// <summary>
		/// The stale revision ID.
		/// </summary>
		public static readonly RevisionID Stale = new RevisionID(0);

		/// <summary>
		/// A no parent ID.
		/// </summary>
		public static readonly RevisionID NoParentInfo = new RevisionID(1);

		/// <summary>
		/// The initial revision ID.
		/// </summary>
		public static readonly RevisionID Initial = new RevisionID(2);
	}
}
