using Microsoft.Xna.Framework.Content.Pipeline;
using System;

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
		/// <param name="filename">The path to the asset file being referenced.</param>
		/// <param name="processorParameters">Optional parameters used during the building of the external asset.</param>
		public void AddReference<TContent>(ContentProcessorContext context, string filename, OpaqueDataDictionary processorParameters) where TContent : notnull;

		/// <summary>
		/// Retrieves a previously referenced external asset from this content.
		/// </summary>
		/// <typeparam name="TContent">The type of externally referenced asset data.</typeparam>
		/// <param name="filename">The path to the externally referenced asset file.</param>
		/// <returns>A <see cref="ExternalReference{TContent}"/> instance for the referenced asset build from data found at <c>filename</c>.</returns>
		/// <exception cref="ArgumentException"><c>filename</c> does not point to an asset that was previously built by and added as a reference to this content.</exception>
		public ExternalReference<TContent> GetReference<TContent>(string filename) where TContent : notnull;
	}
}