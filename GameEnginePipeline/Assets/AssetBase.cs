using GameEnginePipeline.Serialization;
using System.Diagnostics.CodeAnalysis;

namespace GameEnginePipeline.Assets
{
	/// <summary>
	/// This is the base class for assets.
	/// Primarily, it provides two types of IDs for the asset to allow for proper asset read/write in more complicated cases.
	/// It additionally provides some basic comparison logic utilizing their always unique ContentID values.
	/// </summary>
	public abstract class AssetBase : IComparable<AssetBase>, IEquatable<AssetBase>, IDisposable
	{
		/// <summary>
		/// The basic base asset construction.
		/// This obtains a fresh content ID and assigns NULL to the internal ID.
		/// </summary>
		protected AssetBase()
		{
			ContentID = AssetID.GetFreshID(this);
			InternalID = InternalAssetID.NULL;

			return;
		}

		/// <summary>
		/// The basic base asset construction.
		/// This obtains a fresh content ID and assigns <paramref name="internal_id"/> to the internal ID.
		/// </summary>
		protected AssetBase(InternalAssetID internal_id)
		{
			ContentID = AssetID.GetFreshID(this);
			InternalID = internal_id;
			
			return;
		}

		/// <summary>
		/// The finalizer.
		/// </summary>
		~AssetBase()
		{
			Dispose(false);
			return;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);

			return;
		}

		/// <summary>
		/// Performs the actual disposal logic.
		/// </summary>
		/// <param name="disposing">
		///	If true, we are disposing of this via <see cref="Dispose()"/>.
		///	If false, then this dispose call is from the finalizer.
		/// </param>
		protected virtual void Dispose(bool disposing)
		{
			if(Disposed)
				return;

			ContentID.Dispose();
			
			Disposed = true;
			return;
		}

		public static bool operator ==(AssetBase a, AssetBase b) => a.ContentID == b.ContentID;
		public static bool operator !=(AssetBase a, AssetBase b) => a.ContentID != b.ContentID;

		public bool Equals(AssetBase? other) => other is not null && this == other;
		public override bool Equals([NotNullWhen(true)] object? obj) => obj is AssetBase id && this == id;

		public static bool operator <(AssetBase a, AssetBase b) => a.ContentID < b.ContentID;
		public static bool operator <=(AssetBase a, AssetBase b) => a.ContentID <= b.ContentID;
		public static bool operator >(AssetBase a, AssetBase b) => a.ContentID > b.ContentID;
		public static bool operator >=(AssetBase a, AssetBase b) => a.ContentID >= b.ContentID;

		public int CompareTo(AssetBase? other) => other is null ? -1 : ContentID.CompareTo(other.ContentID); // We make null values bigger than everything so that we shove them to the end of ascended sorted lists and such

		public override int GetHashCode() => ContentID.GetHashCode();
		public override string ToString() => ContentID.ToString();

		/// <summary>
		/// The ID assigned to the asset upon creation.
		/// These values are unique to an asset and may be looked up so long as the asset remains in memory.
		/// </summary>
		public AssetID ContentID
		{get;}

		/// <summary>
		/// An internal ID an asset is assigned by a parent asset which is loading this asset for internal use only, not external exposure.
		/// This is useful for assets to find each other inside of a single asset file.
		/// While these should but unique to such internal reads, they are not required to be.
		/// Moreover, they are unlikely to be unique between different parent asset reads.
		/// <para/>
		/// This value is initialized to Null unless otherwise provided at construction time.
		/// </summary>
		public InternalAssetID InternalID
		{get; set;}

		/// <summary>
		/// If true, then this asset has been disposed.
		/// If false, then it is still valid.
		/// </summary>
		public bool Disposed
		{get; private set;}
	}
}
