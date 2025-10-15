using GameEngine.Assets;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace GameEnginePipeline.Importers
{
	/// <summary>
	/// Loads an asset into memory in content form.
	/// </summary>
	/// <typeparam name="TInput">The type of asset to import.</typeparam>
	/// <typeparam name="TOutput">The type of content to turn the asset into.</typeparam>
	public abstract class Importer<TInput,TOutput> : ContentImporter<TOutput> where TInput : AssetBase where TOutput : ContentItem<TInput>
	{
		/// <summary>
		/// Imports an asset from a file.
		/// </summary>
		/// <param name="path">The absolute path to the asset including the filename.</param>
		/// <param name="context">The pipeline importer context.</param>
		/// <returns>Returns the asset loaded.</returns>
		/// <exception cref="PipelineException">Thrown if <paramref name="path"/> does not exist.</exception>
		/// <exception cref="InvalidContentException">Thrown if the content is not able to be imported.</exception>
		public override TOutput Import(string path, ContentImporterContext context)
		{
			// Check to see if the asset even exists
			if(!File.Exists(path))
				throw new PipelineException("The asset " + path + " does not exist");

			// Load the asset from memory
#if VERBOSE
			context.Logger.LogMessage("Importing asset: " + path);
#endif
			
			TInput? asset = Deserialize(path);

			// If we failed to load our asset, it's invalid
			if(asset is null)
				throw new InvalidContentException("The asset " + path + " was invalid and could not be imported");
			
			// We now need to add any dependences we have
#if VERBOSE
			context.Logger.LogMessage("Adding dependencies for asset: " + path);
#endif

			if(!AddDependencies(path,context,asset))
				throw new InvalidContentException("The asset " + path + " was invalid and could not be imported");

			// And we're done
#if VERBOSE
			context.Logger.LogMessage("Finished importing asset: " + path);
#endif

			return ToContent(asset,path);
		}

		/// <summary>
		/// Deserializes the file <paramref name="path"/> into an asset.
		/// </summary>
		/// <param name="path">The path to the asset file including the filename.</param>
		/// <returns>Returns the deserialized asset or null if it could not be deserialized for any reason.</returns>
		protected abstract TInput? Deserialize(string path);

		/// <summary>
		/// Adds any dependencies this asset requires.
		/// </summary>
		/// <param name="path">The path to the asset file including the filename.</param>
		/// <param name="context">The pipeline importer context.</param>
		/// <param name="asset">The asset to add dependencies for.</param>
		/// <returns>Returns true if all dependencies could be added. Returns false if a dependency could not be added or was invalid.</returns>
		protected abstract bool AddDependencies(string path, ContentImporterContext context, TInput asset);

		/// <summary>
		/// Transforms an asset into content.
		/// </summary>
		/// <param name="asset">The asset to transform.</param>
		/// <param name="path">The path to the asset file. This includes the filename itself.</param>
		/// <returns>Returns the asset in content form.</returns>
		protected abstract TOutput ToContent(TInput asset, string path);
	}
}
