using GameEngine.Resources;

namespace GameEngine.Assets.Serialization
{

	[AttributeUsage(AttributeTargets.Class,AllowMultiple = true,Inherited = false)]
	public class Asset : Attribute
	{
		/// <summary>
		/// Denotes a type as an asset which can be used to construct a resource type.
		/// </summary>
		/// <param name="resource_type">The resource type this asset can be turned into.</param>
		/// <param name="converter">
		/// This is a method that transforms resources into assets.
		/// This should transform resources of type <paramref name="resource_type"/> (or further derived) into assets of some appropriate asset type that inherits from <see cref="AssetBase"/>.
		/// The actual output type only matters into so far as it can be assigned to whatever generic type is specified by an Asset static method.
		/// </param>
		/// <param name="loader">
		/// This is a method that loads an asset from the disc into memory given a path to it.
		/// </param>
		public Asset(Type resource_type, ResourceToAssetConverter converter, AssetLoadFromDisc loader)
		{
			ResourceType = resource_type;
			RToAConverter = converter;
			Loader = loader;

			return;
		}

		/// <summary>
		/// This is the resource type that this asset can become.
		/// </summary>
		public Type ResourceType
		{get;}

		/// <summary>
		/// Converts resources into assets.
		/// </summary>
		public ResourceToAssetConverter RToAConverter
		{get;}

		/// <summary>
		/// The method that loads an asset from the disc.
		/// </summary>
		public AssetLoadFromDisc Loader
		{get;}
	}

	/// <summary>
	/// Represents a method that transforms a resource into an asset.
	/// </summary>
	/// <param name="r">The resource to transform into an asset.</param>
	/// <returns>Returns an asset derived from <paramref name="r"/> if possible and null otherwise.</returns>
	public delegate AssetBase? ResourceToAssetConverter(IResource r);

	/// <summary>
	/// Represents a method that loads an asset from disc into memory.
	/// </summary>
	/// <param name="path">
	/// A valid path to the asset (including the filename and extension).
	/// This can be relative to the working directory or absolute.
	/// </param>
	/// <returns>Returns the asset laoded or null if something went wrong.</returns>
	public delegate AssetBase? AssetLoadFromDisc(string path);
}
