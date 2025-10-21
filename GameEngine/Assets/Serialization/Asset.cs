using GameEngine.Resources;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace GameEngine.Assets.Serialization
{
	/// <summary>
	/// A static class used manage interactions between assets and the disc.
	/// </summary>
	public static class Asset
	{
		/// <summary>
		/// Constructs the asset loader dictionary.
		/// </summary>
		static Asset()
		{
			AssetInfo = new Dictionary<Type,AssetLoader>();
			return;
		}

		/// <summary>
		/// Converts <paramref name="asset"/> into an instance of its resource form.
		/// </summary>
		/// <typeparam name="AssetType">The asset type to instantiate. This is a convenience parameter and has no other effect upon this than to better define the input parameters.</typeparam>
		/// <typeparam name="ResourceType">The target resource type.</typeparam>
		/// <param name="asset">The asset to instantiate.</param>
		/// <param name="g">The GraphicsDevice to instantiate <paramref name="asset"/> with. This is not always necessary to instantiate, but set this to null at your own peril.</param>
		/// <param name="output">The resource instance created when this returns true or null when this returns false.</param>
		/// <returns>Returns true if <paramref name="asset"/> could be transformed into a resource instance of type <typeparamref name="ResourceType"/> (or a derived type assignable to it). Returns false otherwise.</returns>
		public static bool Instantiate<AssetType,ResourceType>(AssetType asset, GraphicsDevice g, [MaybeNullWhen(false)] out ResourceType output) where AssetType : AssetBase where ResourceType : IResource
		{
			return asset.Instantiate(g,out output);
		}

		/// <summary>
		/// Saves an asset to <paramref name="path"/>.
		/// </summary>
		/// <param name="path">
		/// The desired path to the asset.
		/// This must be an absolute path relative to <paramref name="root"/>.
		/// </param>
		/// <param name="root">
		/// The content root directory (or a directory otherwise in which to save the asset).
		/// This can be an absolute path or relative to the current working directory.
		/// <para/>
		/// Ideally, this should be the uncompiled content root directory, as this will write uncompiled assets to the disc.
		/// To use them through the content system, this top-level content file (at minimum) must be added to the content file to run through the content builder.
		/// </param>
		/// <param name="overwrite_dependencies">
		/// If true, then this will overwrite all dependencies (regardless of if they have made changes).
		/// <para/>
		/// If false, then this will only serialize itself and any missing dependencies.
		/// In this case, this asset will stop serialization when it exncounters extant dependencies.
		/// As such, if any such extant dependency further depends on an absent dependency of its own, this will not be detected and that dependency will remain absent.
		/// </param>
		public static void SaveAsset<AssetType>(AssetType asset, string path, string root, bool overwrite_dependencies = false) where AssetType : AssetBase
		{
			asset.SaveToDisc(path,root,overwrite_dependencies);
			return;
		}

		/// <summary>
		/// Loads an asset from the disc.
		/// </summary>
		/// <typeparam name="AssetType">
		/// The asset type to load.
		/// The type must have an <see cref="AssetLoader"/> attribute for this method to work.
		/// If it does not, this method will always return false.
		/// </typeparam>
		/// <param name="path">
		/// A valid path to the asset (including the filename and extension).
		/// This can be relative to the working directory or absolute.
		/// </param>
		/// <param name="output">The asset loaded when this returns true and null when this returns false.</param>
		/// <returns>Returns true if the asset was loaded and false otherwise in every other circumstance.</returns>
		public static bool LoadAsset<AssetType>(string path, [MaybeNullWhen(false)] out AssetType output) where AssetType : AssetBase
		{
			Type a = typeof(AssetType);

			// Get the loader for the asset type
			if(!AssetInfo.TryGetValue(a,out AssetLoader? loader))
				if(!LoadType(a,out loader))
				{
					output = null;
					return false;
				}

			// Perform the actual asset load
			if(!loader.LoadAsset(path,out output))
			{
				output = null;
				return false;
			}

			return true;
		}

		/// <summary>
		/// Creates an asset from a resource type.
		/// </summary>
		/// <typeparam name="AssetType">
		/// The asset type to produce.
		/// The actual type of the returned object is determined by <paramref name="resource"/>, but the caller-facing static type will be this.
		/// </typeparam>
		/// <param name="resource">The resource to turn into an asset.</param>
		/// <param name="output">The asset created when this returns true or null when this returns false.</param>
		/// <returns>Returns true if <paramref name="resource"/> was turned into an asset and false otherwise if anything goes wrong.</returns>
		public static bool CreateAssetFromResource<AssetType>(IResource resource, [MaybeNullWhen(false)] out AssetType output) where AssetType : AssetBase
		{return resource.ToAsset(out output);}

		/// <summary>
		/// Fetches asset loader info for type <paramref name="asset_type"/>.
		/// </summary>
		/// <param name="asset_type">The asset type to load info for.</param>
		/// <param name="output">The asset information loaded when this returns true or null when this returns false.</param>
		/// <returns>Returns true if <paramref name="asset_type"/> was a valid asset type with loading information and something was loaded for it. Returns false otherwise.</returns>
		private static bool LoadType(Type asset_type, [MaybeNullWhen(false)] out AssetLoader output)
		{
			// If we're not ignorant to the asset type, we can finish without going through the attribute system
			if(AssetInfo.TryGetValue(asset_type,out output))
				return true;

			// Try to learn something about the asset type
			output = asset_type.GetCustomAttribute<AssetLoader>();
			
			// If we learned nothing, we're done
			if(output is null)
				return false;
			
			// Otherwise, we learned SOMETHING and should remember it for later
			AssetInfo[asset_type] = output;

			return true;
		}

		/// <summary>
		/// This dictionary stores precomputed asset loader information.
		/// We keep this information around because it is somewhat expensive to compute the attribute, since it must go through the reflection system.
		/// </summary>
		private static Dictionary<Type,AssetLoader> AssetInfo;
	}
}
