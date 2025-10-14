using GameEngine.Assets;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics.CodeAnalysis;

namespace GameEnginePipeline.Serialization
{
	/// <summary>
	/// Represents the source of an asset in its myriad of forms.
	/// </summary>
	/// <typeparam name="T">
	/// The asset type that this is a source for.
	/// We constrain this to be class types because, for one, they <b>should</b> be, and two, we would like to be able to assign null properly.
	/// </typeparam>
	public class AssetSource<T> where T : class
	{
		/// <summary>
		/// Creates an asset source initialized to have no source.
		/// </summary>
		public AssetSource()
		{
			ConcreteAsset = null;
			RootPath = null;
			RelativePath = null;

			return;
		}

		/// <summary>
		/// Creates an asset source initialized to have <paramref name="asset"/> as its source material.
		/// </summary>
		public AssetSource(T? asset) : this()
		{
			SetConcretAsset(asset);
			return;
		}

		/// <summary>
		/// Creates an asset source initialized to point to the asset file <paramref name="root_path"/>.
		/// If <paramref name="relative_to"/> is not null, it will also set the relative path as well.
		/// </summary>
		/// <param name="root_path">The path from the content root to the asset file (including the filename).</param>
		/// <param name="relative_to">If null, this will have no effect. If not null, this will set <see cref="RelativePath"/> using this as the owning asset's directory.</param>
		public AssetSource(string root_path, string? relative_to = null)
		{
			ConcreteAsset = null;
			RootPath = root_path;

			if(relative_to is not null)
				SetRelativePath(relative_to);

			return;
		}

		/// <summary>
		/// Sets <see cref="ConcreteAsset"/> to <paramref name="asset"/>.
		/// This will also set <see cref="RootPath"/> to the AssetName or Name of <paramref name="asset"/> if it is an IAsset or GraphicsResource respectively, prioritizing the former.
		/// If this would assign the empty string to it (which denotes a missing path in concrete assets), then null is assigned instead.
		/// In the process, this will invalidate <see cref="RelativePath"/> and set it to null.
		/// </summary>
		/// <param name="asset">The concrete asset to set to this. If null, this will invalidate both paths.</param>
		public void SetConcretAsset(T? asset)
		{
			ConcreteAsset = asset;

			if(ConcreteAsset is null)
				RootPath = null;
			else
			{
				if(ConcreteAsset is IAsset iasset)
					RootPath = iasset.AssetName;
				else if(ConcreteAsset is GraphicsResource gr)
					RootPath = gr.Name; // We deprioritize this since Name may be inaccurate in some GraphicsResource assets (such as SpriteRenderer, unless something else has loaded it as a dependency and reassigned its Name)
			
				// We denote missing paths in concrete assets with the empty string and so special case the assignment here
				if(RootPath == "")
					RootPath = null;
			}

			RelativePath = null;
			return;
		}

		/// <summary>
		/// Sets <see cref="RelativePath"/> to be relative to <paramref name="relative_to"/>.
		/// Note that this will do nothing if <see cref="RootPath"/> is not defined.
		/// </summary>
		/// <param name="relative_to">The root path (to a directory) which this asset source should be made relative to.</param>
		public void SetRelativePath(string relative_to)
		{
			if(RootPath is not null)
				RelativePath = Path.GetRelativePath(relative_to,RootPath);
			
			return;
		}

		/// <summary>
		/// Sets <see cref="RelativePath"/> to be relative to <paramref name="relative_to"/>.
		/// </summary>
		/// <param name="root">The root path to this asset which will be assigned to <see cref="RootPath"/>.</param>
		/// <param name="relative_to">The root path (to a directory) which this asset source should be made relative to. This is generally where an owning asset is located.</param>
		public void SetRelativePath(string root, string relative_to)
		{
			RootPath = root;
			RelativePath = Path.GetRelativePath(relative_to,RootPath);
			
			return;
		}

		/// <summary>
		/// Directly assigns <see cref="RelativePath"/> to be <paramref name="path"/>.
		/// </summary>
		/// <remarks>This is best used when <see cref="RootPath"/> is unknown such as during deserialization.</remarks>
		public void AssignRelativePath(string? path)
		{
			RelativePath = path;
			return;
		}

		/// <summary>
		/// Directly assigns <see cref="RootPath"/> to be <paramref name="path"/>.
		/// </summary>
		/// <param name="path">This is best used when <see cref="RelativePath"/> is known but the content root is not.</param>
		public void AssignRootPath(string? path)
		{
			RootPath = path;
			return;
		}

		/// <summary>
		/// Obtains the full path to the asset this describes.
		/// It does so by combining <paramref name="asset_directory"/> and <see cref="RelativePath"/>.
		/// If it cannot, it then attempts to combine <paramref name="content_root"/> and <see cref="RootPath"/> instead.
		/// </summary>
		/// <param name="asset_directory">The full path to the owning asset directory.</param>
		/// <param name="content_root">The full path to the content root directory.</param>
		/// <param name="fullpath">The (simplest) absolute path to the asset file this describes when this returns true and null when this returns false.</param>
		/// <returns>Returns true if the path could be created and false otherwise.</returns>
		public bool GetFullPath(string asset_directory, string content_root, [MaybeNullWhen(false)] out string fullpath)
		{
			if(!GetFullPathFromAssetDirectory(asset_directory,out string? src) && !GetFullPathFromContentRoot(content_root,out src))
			{
				fullpath = null;
				return false;
			}

			fullpath = src;
			return true;
		}

		/// <summary>
		/// Obtains the full path to the asset this describes via the content root.
		/// It does so by combining <paramref name="content_root"/> and <see cref="RootPath"/>.
		/// </summary>
		/// <param name="content_root">The full path to the content root directory.</param>
		/// <param name="fullpath">The (simplest) absolute path to the asset file this describes when this returns true and null when this returns false.</param>
		/// <returns>Returns true if the path could be created and false otherwise.</returns>
		public bool GetFullPathFromContentRoot(string content_root, [MaybeNullWhen(false)] out string fullpath)
		{
			if(RootPath is null)
			{
				fullpath = null;
				return false;
			}

			fullpath = Path.GetFullPath(Path.Combine(content_root,RootPath)); // We must simplfy the path (even if it's probably already simplified)
			return true;
		}

		/// <summary>
		/// Obtains the full path to the asset this describes via the owning asset.
		/// It does so by combining <paramref name="asset_directory"/> and <see cref="RelativePath"/>.
		/// </summary>
		/// <param name="asset_directory">The full path to the owning asset directory.</param>
		/// <param name="fullpath">The (simplest) absolute path to the asset file this describes when this returns true and null when this returns false.</param>
		/// <returns>Returns true if the path could be created and false otherwise.</returns>
		public bool GetFullPathFromAssetDirectory(string asset_directory, [MaybeNullWhen(false)] out string fullpath)
		{
			if(RelativePath is null)
			{
				fullpath = null;
				return false;
			}

			fullpath = Path.GetFullPath(Path.Combine(asset_directory,RelativePath)); // We must simplfy the path
			return true;
		}

		public override string ToString()
		{
			if(RootPath is not null)
				return RootPath;
			else if(RelativePath is not null)
				return RelativePath;
			else if(ConcreteAsset is not null)
				if(ConcreteAsset is IAsset asset)
					return asset.AssetName;
				else if(ConcreteAsset is GraphicsResource gr)
					return gr.Name;

			return "";
		}

		/// <summary>
		/// This is the concrete asset when present and null otherwise.
		/// </summary>
		public T? ConcreteAsset
		{get; protected set;}

		/// <summary>
		/// This is the path from the content root to the asset (including the file name).
		/// This is null when there is no known asset on the disc that this represents.
		/// </summary>
		/// <remarks>This path is not guaranteed to be in its simplest form.</remarks>
		public string? RootPath
		{get; protected set;}

		/// <summary>
		/// This is the directory this asset sits inside of.
		/// This value is null if <see cref="RootPath"/> is not defined.
		/// </summary>
		/// <remarks>This path is not guaranteed to be in its simplest form.</remarks>
		public string? RootDirectory => Path.GetDirectoryName(RootPath);

		/// <summary>
		/// This is the relative path to the asset (including the file name) source from whatever asset owns this.
		/// This is null when there is no known relative path on the disc that this represents.
		/// </summary>
		/// <remarks>This path is not guaranteed to be in its simplest form.</remarks>
		public string? RelativePath
		{get; protected set;}

		/// <summary>
		/// This is the directory this asset sits inside of relative to whatever asset owns this.
		/// This value is null if <see cref="RelativePath"/> is not defined.
		/// </summary>
		/// <remarks>This path is not guaranteed to be in its simplest form.</remarks>
		public string? RelativeDirectory => Path.GetDirectoryName(RelativePath);

		/// <summary>
		/// If true, then this source has no useful information whatsoever.
		/// If false, then this source has at least one piece of information that can be utilized.
		/// </summary>
		public bool Null => Unnamed && ConcreteAsset is null;

		/// <summary>
		/// If true, then this source has no name.
		/// If false, then this source has a name.
		/// </summary>
		public bool Unnamed => RootPath is null && RelativePath is null;
	}
}
