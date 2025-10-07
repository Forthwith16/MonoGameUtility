using Microsoft.Xna.Framework.Content.Pipeline;

namespace GameEnginePipeline
{
	/// <summary>
	/// Defines raw data for a game asset.
	/// </summary>
	public interface IContentItem
	{
		/// <summary>
		/// Builds the specified external asset and adds a reference to it for this content.
		/// </summary>
		/// <typeparam name="TContent">The type of asset data being referenced by this content.</typeparam>
		/// <param name="context">The current content processing context.</param>
		/// <param name="path">
		/// The path to the asset file being referenced including the filename itself.
		/// This is always converted to the simplest absolute path to the file to support proper caching.
		/// If you want to play cache agmes, the best parameter to do so with is <paramref name="asset_name"/>.
		/// </param>
		/// <param name="processor_name">
		/// An optional processor name.
		/// This value is used for external reference caching and serves as an identifier rather than any functional purpose.
		/// This must match a previous value exactly for caching to work on an asset.
		/// </param>
		/// <param name="parameters">
		/// Optional parameters used during the building of the external asset.
		/// This value is also used for external reference caching.
		/// If it does not match exactly, then a cache will fail.
		/// The cache does, however, perform an equality check rather than a reference comparison.
		/// For instance, while null is a superior choice to an empty OpaqueDataDictionary, the latter will not cause caching problems if used instead (but note that null != empty).
		/// </param>
		/// <param name="importer_name">
		/// An optional importer name.
		/// This value is used for external reference caching and serves as an identifier rather than any functional purpose.
		/// This must match a previous value exactly for caching to work on an asset.
		/// </param>
		/// <param name="asset_name">
		/// An optional asset name.
		/// This value is used for external reference caching and serves as an identifier rather than any functional purpose.
		/// This must match a previous value exactly for caching to work on an asset.
		/// </param>
		public void AddExternalReference<TContent>(ContentProcessorContext context, string path, string? processor_name = null, OpaqueDataDictionary? parameters = null, string? importer_name = null, string? asset_name = null) where TContent : notnull;

		/// <summary>
		/// Retrieves a previously referenced external asset from this content.
		/// </summary>
		/// <typeparam name="TContent">The type of externally referenced asset data.</typeparam>
		/// <param name="path">
		/// The path to the externally referenced asset file including the filename itself.
		/// This is always converted to the simplest absolute path to the file to support proper caching.
		/// </param>
		/// <returns>A <see cref="ExternalReference{TContent}"/> instance for the referenced asset build from data found at <paramref name="path"/>.</returns>
		/// <exception cref="ArgumentException">Thrown if <paramref name="path"/> does not point to an asset that was previously built by and added as a reference to this content.</exception>
		public ExternalReference<TContent> GetExternalReference<TContent>(string path) where TContent : notnull;
	}
}