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
		/// <param name="path">The absolute path to the asset file. This includes the filename itself.</param>
		protected ContentItem(T asset, string path) : base()
		{
			ExternalReferences = new Dictionary<string,ContentItem>();
			Asset = asset;
			
			Identity = new ContentIdentity(path);
			return;
		}

		public void AddExternalReference<TContent>(ContentProcessorContext context, string path, string? processor_name = null, OpaqueDataDictionary? parameters = null, string? importer_name = null, string? asset_name = null) where TContent : notnull
		{
			// To ensure caching works correctly, we need to use the EXACT same path every time, so we use the absolute path with all .. shenanigans stripped out
			string simple_path = Path.GetFullPath(path);

#if VERBOSE
			context.Logger.LogMessage("Building asset: " + simple_path);
			context.Logger.Indent();
#endif
			
			ExternalReferences.Add(simple_path,context.BuildAsset<TContent,TContent>(new ExternalReference<TContent>(simple_path),processor_name,parameters,importer_name,asset_name));

#if VERBOSE
			context.Logger.Unindent();
#endif

			return;
		}

		public ExternalReference<TContent> GetExternalReference<TContent>(string path) where TContent : notnull
		{
			if(!ExternalReferences.TryGetValue(Path.GetFullPath(path),out ContentItem? item))
				throw new ArgumentException("Failed to find external reference " + path); // We provide the raw filename to allow for superior diagnostic information
			
			return (ExternalReference<TContent>)item;
		}

		/// <summary>
		/// The asset data for the content.
		/// </summary>
		public T Asset
		{get;}

		/// <summary>
		/// The absolute path to the asset file.
		/// </summary>
		public string AbsolutePath => Identity.SourceFilename;

		/// <summary>
		/// The absolute path to the directory in which the asset file is located.
		/// </summary>
		public string AbsoluteDirectory => Path.GetDirectoryName(AbsolutePath) ?? AbsolutePath; // AbsolutePath should never not be a file, so the null case should never fire, but just in case

		/// <summary>
		/// The external references that we need to load later.
		/// </summary>
		private Dictionary<string,ContentItem> ExternalReferences
		{get;}
	}
}
