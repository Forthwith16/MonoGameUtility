using GameEnginePipeline.Assets;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace GameEnginePipeline.Processors
{
	/// <summary>
	/// Validates content and performs any additional logic necessary to prepare it to be written into the content pipeline.
	/// </summary>
	/// <typeparam name="TInput">The content type to process.</typeparam>
	/// <typeparam name="TOutput">The type to output. Usually this should be the same as <typeparamref name="TInput"/>.</typeparam>
	/// <typeparam name="TAsset">The asset type.</typeparam>
	/// <remarks>
	/// To add content builder parameters, currently see: <a href="https://docs.monogame.net/articles/getting_to_know/whatis/content_pipeline/CP_CustomParamProcs.html">What are Parameterized Processors?</a>.
	/// </remarks>
	public abstract class Processor<TInput,TOutput,TAsset> : ContentProcessor<TInput,TOutput> where TInput : ContentItem where TOutput : ContentItem<TAsset> where TAsset : IAsset
	{
		/// <summary>
		/// Transforms content into its final form one step removed from a game component.
		/// </summary>
		/// <param name="input">The content to transform.</param>
		/// <param name="context">The pipeline processor content.</param>
		/// <returns>Returns the generated game content.</returns>
		public override TOutput Process(TInput input, ContentProcessorContext context)
		{
			context.Logger.LogMessage("Processing asset: " + input.Identity.SourceFilename);
			TOutput? ret = ValidateContent(input,context);

			if(ret is null)
				throw new InvalidContentException("The asset " + input.Identity.SourceFilename + " was invalid and could not be processed");

			context.Logger.LogMessage("Creating dependencies for asset: " + input.Identity.SourceFilename);
			CreateExternalDependencies(ret,context);
			
			context.Logger.LogMessage("Finished processing asset: " + input.Identity.SourceFilename);
			return ret;
		}

		/// <summary>
		/// Validates <paramref name="input"/> and performs any logic necessary to fill in missing data.
		/// </summary>
		/// <param name="input">The content to validate.</param>
		/// <param name="context">The processor context.</param>
		/// <returns>Returns the output object post processing or null if <paramref name="input"/> was invalid. This will often be the same object as <paramref name="input"/>.</returns>
		protected abstract TOutput? ValidateContent(TInput input, ContentProcessorContext context);

		/// <summary>
		/// Adds any external reference dependencies required by <paramref name="output"/> to the pipeline.
		/// </summary>
		/// <param name="output">The content to add dependencies for.</param>
		/// <param name="context">The processor context.</param>
		protected abstract void CreateExternalDependencies(TOutput output, ContentProcessorContext context);
	}
}
