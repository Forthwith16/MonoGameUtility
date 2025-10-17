using GameEngine.Resources;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace GameEngine.Assets.Serialization
{
	/// <summary>
	/// This class is of a dual nature.
	/// <para/>
	/// This is the asset attribute tag.
	/// This denotes that a class is an asset version of a resource for a game.
	/// This allows the asset type to work with the static system of this class.
	/// <para/>
	/// In this form, the attribute takes a single Type parameter (call this type <c>T</c>) specifying what the resource version of the asset class is.
	/// It will use type <c>T</c> to construct assets from resources.
	/// <para/>
	/// This is the dual aspect of this class.
	/// This is a static class that allows for asset creation from resources.
	/// <para/>
	/// This attribute can only be applied to assets.
	/// It will silently enforce that the class it's attached to (call this type <c>A</c>) derives from AssetBase.
	/// It will also silently enforce that type <c>A</c> has a constructor available requiring only a single parameter of type <c>T</c> at runtime.
	/// This will allow for serialization from type <c>T</c> to type <c>A</c> without knowledge of either type.
	/// If this constructor is not present, this will fail to generate an entry in this class for such construction.
	/// </summary>
	public static class Assets
	{
		/// <summary>
		/// Constructs the asset double dictionary.
		/// </summary>
		static Assets()
		{
			AssetInfo = new Dictionary<Type,Dictionary<Type,Asset>>();
			return;
		}

		/// <summary>
		/// Loads an asset from disc.
		/// </summary>
		/// <typeparam name="A">The asset type to load.</typeparam>
		/// <param name="path">
		/// A valid path to the asset (including the filename and extension).
		/// This can be relative to the working directory or absolute.
		/// </param>
		/// <returns>Returns the loaded asset or null if something went wrong.</returns>
		public static A? LoadAsset<A>(string path) where A : AssetBase
		{


			return null;
		}

		/// <summary>
		/// Creates an asset from a resource type.
		/// </summary>
		/// <typeparam name="A">The asset type to produce.</typeparam>
		/// <param name="resource">The resource to turn into an asset.</param>
		/// <returns>Returns the asset created or null if no asset could be produced from <paramref name="resource"/>.</returns>
		public static A? CreateAsset<A>(IResource resource) where A : AssetBase
		{
			Type a = typeof(A);

			// The first thing to do is check if type A is a type we know about
			if(!AssetInfo.TryGetValue(a,out Dictionary<Type,Asset>? raw_info))
				if(!LoadType(a,out raw_info))
					return null;

			// At this point, we know at least something about A, but it remains to show if we know something useful about A
			if(!raw_info.TryGetValue(resource.GetType(),out Asset? a_info))
				return null; // We know nothing about turning raw into an A asset

			try
			{return (A?)a_info.RToAConverter(resource);}
			catch
			{return null;}
		}

		/// <summary>
		/// Determines if a concrete object of type <paramref name="concrete_type"/> can be converted into an asset of type <paramref name="asset_type"/>.
		/// </summary>
		/// <param name="concrete_type">The concrete type to turn into an asset type.</param>
		/// <param name="asset_type">The asset type to create from <paramref name="concrete_type"/>.</param>
		/// <returns>Returns true if the conversion is possible and false otherwise.</returns>
		public static bool CanCreateAsset(Type concrete_type, Type asset_type)
		{
			// Get or load and get the information for asset_type
			if(!AssetInfo.TryGetValue(asset_type,out Dictionary<Type,Asset>? raw_info))
				if(!LoadType(asset_type,out raw_info))
					return false;

			return raw_info.ContainsKey(concrete_type);
		}

		/// <summary>
		/// Loads asset info for type <paramref name="asset_type"/>.
		/// </summary>
		/// <param name="asset_type">The asset type to load info for.</param>
		/// <param name="raw_info">The asset information loaded when this returns true or null when this returns false.</param>
		/// <returns>Returns true if <paramref name="asset_type"/> was a valid asset type and something was loaded for it and false otherwise.</returns>
		private static bool LoadType(Type asset_type, [MaybeNullWhen(false)] out Dictionary<Type,Asset> raw_info)
		{
			// We've never heard of asset_type, so let's learn about it
			raw_info = new Dictionary<Type,Asset>();
			
			// asset_type may have multiple Asset attributes, and we need to document each one
			foreach(Asset asset in asset_type.GetCustomAttributes<Asset>())
				raw_info[asset.ResourceType] = asset;

			// If we learned nothing, we're done
			if(raw_info.Count == 0)
			{
				raw_info = null;
				return false;
			}

			// Otherwise, we learned SOMETHING and should remember it for later
			AssetInfo[asset_type] = raw_info;

			return true;
		}

		/// <summary>
		/// The information required to build assets from resources.
		/// The first index is the asset type.
		/// The second index is the resource type.
		/// The value, then, is the actual Asset that specifies how to do things.
		/// </summary>
		private static Dictionary<Type,Dictionary<Type,Asset>> AssetInfo;
	}
}
