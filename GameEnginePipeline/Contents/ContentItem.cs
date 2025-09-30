using GameEnginePipeline.Assets;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace GameEnginePipeline
{
	/// <summary>
	/// Common functionality to content item classes.
	/// </summary>
	/// <typeparam name="T">The type of asset data.</typeparam>
	public abstract class ContentItem<T> : ContentItem, IContentItem where T : notnull, IAsset
	{
		/// <summary>
		/// Transforms an asset into content.
		/// </summary>
		/// <param name="asset">The asset we are loading.</param>
		/// <param name="filename">The filename of the asset.</param>
		protected ContentItem(T asset, string filename)
		{
			ExternalReferences = new Dictionary<string,ContentItem>();
			Asset = asset;

			Identity = new ContentIdentity(filename);
			return;
		}

		public void AddExternalReference<TContent>(ContentProcessorContext context, string filename, string? processor_name = null, OpaqueDataDictionary? parameters = null, string? importer_name = null, string? asset_name = null) where TContent : notnull
		{
			// To ensure caching works correctly, we need to use the EXACT same path every time, so we use the absolute path with all .. shenanigans stripped out
			string path = Path.GetFullPath(filename);

			ExternalReferences.Add(path,context.BuildAsset<TContent,TContent>(new ExternalReference<TContent>(path),processor_name,parameters,importer_name,asset_name));
			return;
		}

		public ExternalReference<TContent> GetExternalReference<TContent>(string filename) where TContent : notnull
		{
			if(!ExternalReferences.TryGetValue(Path.GetFullPath(filename),out ContentItem? item))
				throw new ArgumentException("Failed to find external reference " + filename); // We provide the raw filename to allow for superior diagnostic information
			
			return (ExternalReference<TContent>)item;
		}

		/// <summary>
		/// The asset data for the content.
		/// </summary>
		public T Asset
		{get;}

		/// <summary>
		/// The external references that we need to load later.
		/// </summary>
		private Dictionary<string,ContentItem> ExternalReferences
		{get;}
	}
}
