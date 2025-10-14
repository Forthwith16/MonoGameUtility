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

			DisposeAsset(disposing);
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
		protected virtual void DisposeAsset(bool disposing)
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
		/// Saves an asset to <paramref name="path"/>.
		/// </summary>
		/// <param name="path">The desired path to the asset. This must be an absolute path relative to <paramref name="root"/>.</param>
		/// <param name="root">
		/// The content root directory.
		/// <para/>
		/// This should be the uncompiled content root directory, as this will write uncompiled assets to the disc.
		/// To use them through the content system, this top-level content file (at minimum) must be added to the content file to run through the content builder.
		/// </param>
		/// <param name="overwrite_dependencies">
		/// If true, then this will overwrite all dependencies (regardless of if they have made changes).
		/// <para/>
		/// If false, then this will only serialize itself and missing dependencies.
		/// It will stop serializing at extant dependencies, so if any such dependency depends on an absent dependency in turn, this will not be detected.
		/// </param>
		public void SaveToDisc(string path, string root, bool overwrite_dependencies = false)
		{
			// We first need to adjust every filename to be relative to path
			AdjustFilePaths(Path.GetDirectoryName(path) ?? "",root);

			// Make sure our output directory exists
			string dst = Path.Combine(root,path);
			string? dir = Path.GetDirectoryName(dst);

			if(dir is not null && dir != "")
				Directory.CreateDirectory(dir);

			// Do the actual save logic
			Serialize(dst,root,overwrite_dependencies);
			
			return;
		}

		/// <summary>
		/// Prepares every file path in this asset for writing.
		/// To do so, it will do two things.
		/// <list type="bullet">
		///	<item>
		///	It will adjust every relative path to be relative to <paramref name="path"/>.
		///	If it has an absolute path (relative to the content root), it will utilize that to create the relative path.
		///	If it only has a relative path, then it will leave the relative path alone and utilize that path (even if it results in duplicate asset files when writing to disc).
		///	</item>
		///	<item>
		///	It will assign unique filenames to any assets missing a path.
		///	</item>
		/// </list>
		/// </summary>
		/// <param name="path">
		/// The destination directory of this asset.
		/// This will be an absolute path relative to the content root.
		/// All relative paths should be made relative to this path.
		/// Any absolute paths should be left alone.
		/// </param>
		/// <param name="root">
		/// The content root directory.
		/// <para/>
		/// This is the uncompiled content root directory.
		/// </param>
		protected virtual void AdjustFilePaths(string path, string root)
		{return;}

		/// <summary>
		/// Performs the usual file path adjustment to an asset source to enable writing to disc.
		/// </summary>
		/// <typeparam name="T">The asset source type. Largely irrelevant here.</typeparam>
		/// <param name="source">The asset source whose file paths should be adjusted.</param>
		/// <param name="path">
		/// The destination directory of the asset that owns <paramref name="source"/>.
		/// This should be an absolute path relative to the content root.
		/// </param>
		/// <param name="fallback_filename">
		/// If no filename is available for <paramref name="source"/>, the root path will be assigned to <paramref name="path"/>/<paramref name="fallback_filename"/> and the relative path will be assigned to <paramref name="fallback_filename"/>.
		/// This should include any applicable file extensions.
		/// </param>
		protected void StandardAssetSourcePathAdjustment<T>(AssetSource<T> source, string path, string fallback_filename) where T : class
		{
			if(source.RootPath is null)
			{
				if(source.RelativePath is null)
				{
					// We have nothing to go off of, so we use the fallback file and call that good
					source.AssignRelativePath(fallback_filename);
					source.AssignRootPath(Path.Combine(path,source.RelativePath!));
				}
				// else - we do nothing and hope for the best with the extant relative path
			}
			else // We have a path from the content root to the file, so generating the relative path to the new asset destination is easy
				source.SetRelativePath(path);

			return;
		}

		/// <summary>
		/// Generates a path to a file (including the filename) that does not exist.
		/// </summary>
		/// <param name="destination">The path to the directory to generate a file in. This can be any valid path, but generally, this should be relative to the content root.</param>
		/// <param name="extension">The extension to give the file.</param>
		/// <returns>Returns the filename (and only the filename without <paramref name="destination"/> prepended) generated.</returns>
		protected string GenerateFreeFile(string destination, string extension)
		{
			string[] files;
			
			try
			{files = Directory.GetFiles(destination);}
			catch(DirectoryNotFoundException)
			{return "0" + extension;} // This file for sure doesn't exist already if the directory doesn't
			
			for(uint i = 0;i < uint.MaxValue;i++)
			{
				string ret = i + extension;

				if(!files.Contains(ret))
					return ret;
			}
			
			throw new IOException("You have a stupid amount of files in a single directory. Stop. How does your file system even support this?");
		}

		/// <summary>
		/// Checks if an asset dependency should be serialized.
		/// </summary>
		/// <typeparam name="T">The asset type to consider serializing.</typeparam>
		/// <param name="source">The source asset to consider serializing.</param>
		/// <param name="path">The absolute path (to a directory) in which <paramref name="source"/>'s owning asset will be located.</param>
		/// <param name="overwrite_dependencies">
		/// If true, then this will overwrite all dependencies (regardless of if they have made changes).
		/// <para/>
		/// If false, then this will only serialize itself and missing dependencies.
		/// It will stop serializing at extant dependencies, so if any such dependency depends on an absent dependency in turn, this will not be detected.
		/// </param>
		/// <param name="dst">The desination path (including the filename) of <paramref name="source"/> when this returns true or null when this returns false.</param>
		/// <returns>Returns true if <paramref name="source"/> should be serialized and false otherwise.</returns>
		public bool StandardShouldSerializeCheck<T>(AssetSource<T> source, string path, bool overwrite_dependencies, [MaybeNullWhen(false)] out string dst) where T : class
		{
			if(source.ConcreteAsset is null || !source.GetFullPathFromAssetDirectory(path,out string? source_path) || !overwrite_dependencies && File.Exists(source_path))
			{
				dst = null;
				return false;
			}

			dst = source_path;
			return true;
		}

		/// <summary>
		/// Serializes an asset to <paramref name="path"/>.
		/// </summary>
		/// <param name="path">The desired path to the asset (including the filename).</param>
		/// <param name="root">
		/// The content root directory.
		/// <para/>
		/// This is the uncompiled content root directory.
		/// </param>
		/// <param name="overwrite_dependencies">
		/// If true, then this will overwrite all dependencies (regardless of if they have made changes).
		/// <para/>
		/// If false, then this will only serialize itself and missing dependencies.
		/// It will stop serializing at extant dependencies, so if any such dependency depends on an absent dependency in turn, this will not be detected.
		/// </param>
		protected abstract void Serialize(string path, string root, bool overwrite_dependencies = false);

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
