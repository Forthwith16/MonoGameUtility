using Microsoft.Xna.Framework.Content.Pipeline;
using System;
using System.Collections.Generic;

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

		public void AddReference<TContent>(ContentProcessorContext context, string filename, OpaqueDataDictionary parameters) where TContent : notnull
		{
			ExternalReferences.Add(filename,context.BuildAsset<TContent,TContent>(new ExternalReference<TContent>(filename),"",parameters,"",""));
			return;
		}

		public ExternalReference<TContent> GetReference<TContent>(string filename) where TContent : notnull
		{
			if(!ExternalReferences.TryGetValue(filename,out ContentItem? item))
				throw new ArgumentException("Failed to find external reference " + filename);
			
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
