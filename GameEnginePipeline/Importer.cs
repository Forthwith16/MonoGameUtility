using Microsoft.Xna.Framework.Content.Pipeline;
using System.IO;

namespace GameEnginePipeline
{
	/// <summary>
	/// Loads an asset into memory in content form.
	/// </summary>
	/// <typeparam name="TInput">The type of asset to import.</typeparam>
	/// <typeparam name="TOutput">The type of content to turn the asset into.</typeparam>
	public abstract class Importer<TInput,TOutput> : ContentImporter<TOutput> where TInput : IAsset where TOutput : ContentItem<TInput>
	{
		/// <summary>
		/// Imports an asset from a file.
		/// </summary>
		/// <param name="filename">The source file name of the asset.</param>
		/// <param name="context">The pipeline importer context.</param>
		/// <returns>Returns the asset loaded.</returns>
		/// <exception cref="PipelineException">Thrown if <paramref name="filename"/> does not exist.</exception>
		/// <exception cref="InvalidContentException">Thrown if the content is not able to be imported.</exception>
		public override TOutput Import(string filename, ContentImporterContext context)
		{
			// Check to see if the asset even exists
			if(!File.Exists(filename))
				throw new PipelineException("The asset " + filename + " does not exist");
			
			// Load the asset from memory
			context.Logger.LogMessage("Importing asset: " + filename);
			TInput? asset = Deserialize(filename);

			// If we failed to load our asset, it's invalid
			if(asset is null)
				throw new InvalidContentException("The asset " + filename + " was invalid and could not be imported");
			
			// We now need to add any dependences we have
			context.Logger.LogMessage("Adding dependencies for asset: " + filename);
			
			if(!AddDependencies(filename,context,asset))
				throw new InvalidContentException("The asset " + filename + " was invalid and could not be imported");

			// And we're done
			context.Logger.LogMessage("Finished importing asset: " + filename);
			return ToContent(asset,filename);
		}

		/// <summary>
		/// Deserializes the file <paramref name="filename"/> into an asset.
		/// </summary>
		/// <param name="filename">The path to the file.</param>
		/// <returns>Returns the deserialized asset or null if it could not be deserialized for any reason.</returns>
		protected abstract TInput? Deserialize(string filename);

		/// <summary>
		/// Adds any dependencies this asset requires.
		/// </summary>
		/// <param name="filename">The source file name of the asset.</param>
		/// <param name="context">The pipeline importer context.</param>
		/// <param name="asset">The asset to add dependencies for.</param>
		/// <returns>Returns true if all dependencies could be added. Returns false if a dependency could not be added or was invalid.</returns>
		protected abstract bool AddDependencies(string filename, ContentImporterContext context, TInput asset);

		/// <summary>
		/// Transforms an asset into content.
		/// </summary>
		/// <param name="asset">The asset to transform.</param>
		/// <param name="filename">The source file name of the asset.</param>
		/// <returns>Returns the asset in content form.</returns>
		protected abstract TOutput ToContent(TInput asset, string filename);
	}
}
