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
		/// Saves an asset to <paramref name="path"/>.
		/// </summary>
		/// <param name="path">The desired path to the asset. This must be an absolute path relative to <paramref name="root"/>.</param>
		/// <param name="root">The content root directory.</param>
		public void SaveToDisc(string path, string root)
		{
			// We first need to adjust every filename to be relative to path
			AdjustFilePaths(Path.GetDirectoryName(path) ?? "");

			// Do the actual save logic
			Serialize(Path.Combine(root,path));
			
			return;
		}

		/// <summary>
		/// Adjusts every file in this asset to be relative to <paramref name="path"/>.
		/// It is assumed that 
		/// </summary>
		/// <param name="path">The destination directory of this asset. All relative paths should be made relative to this path.</param>
		protected virtual void AdjustFilePaths(string path)
		{return;}

		/// <summary>
		/// Serializes an asset to <paramref name="path"/>.
		/// </summary>
		/// <param name="path">The desired path to the asset.</param>
		protected abstract void Serialize(string path);

		/// <summary>
		/// The basic base asset construction.
		/// This obtains a fresh content ID and assigns <paramref name="internal_id"/> to NULL.
		/// </summary>
		protected AssetBase()
		{
			ContentID = AssetID.GetFreshID(this);
			InternalID = InternalAssetID.NULL;
			
			Linked = false;
			Disposed = false;
			
			return;
		}

		/// <summary>
		/// The basic base asset construction.
		/// This obtains a fresh content ID and assigns <paramref name="internal_id"/> to the internal ID.
		/// </summary>
		/// <param name="internal_id">The ID to assign to the internal ID.</param>
		protected AssetBase(InternalAssetID internal_id)
		{
			ContentID = AssetID.GetFreshID(this);
			InternalID = internal_id;
			
			Linked = false;
			Disposed = false;
			
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
		private void Dispose(bool disposing)
		{
			if(Disposed)
				return;

			DisposeAsset();
			ContentID.Dispose();
			
			Disposed = true;
			return;
		}

		/// <summary>
		/// Performs disposal logic for the asset (if any).
		/// </summary>
		/// <param name="disposing">
		///	If true, we are disposing of this via <see cref="Dispose()"/>.
		///	If false, then this dispose call is from the finalizer.
		/// </param>
		/// <remarks>This is called before the ContentID is disposed.</remarks>
		protected virtual void DisposeAsset()
		{return;}

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
		/// Links all unbound asset references within this asset.
		/// </summary>
		/// <param name="args">
		/// A list of supporting arguments to enable the linkage to proceed.
		/// What values appear here, if any, are entirely implementation dependent.
		/// </param>
		protected void Link(params object?[] args)
		{
			if(Linked)
				return;

			LinkAssets(args);

			Linked = true;
			return;
		}

		/// <summary>
		/// Links all unbound asset references within this asset.
		/// This method does the actual linkage work, whereas <see cref="Link"/> instead initialize the linkage process (if it has not already occurred).
		/// </summary>
		/// <param name="args">
		/// A list of supporting arguments to enable the linkage to proceed.
		/// What values appear here, if any, are entirely implementation dependent.
		/// </param>
		protected virtual void LinkAssets(params object?[] args)
		{return;}

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
		/// If true, then this asset's links to other assets have been forged.
		/// If false, then this has yet to occur yet.
		/// </summary>
		protected bool Linked
		{get; private set;}

		/// <summary>
		/// If true, then this asset has been disposed.
		/// If false, then it is still valid.
		/// </summary>
		public bool Disposed
		{get; private set;}
	}
}
