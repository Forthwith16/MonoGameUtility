using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

using TAsset = GameEnginePipeline.Assets.Sprites.SpriteRendererAsset;
using TInput = GameEnginePipeline.Contents.Sprites.SpriteRendererContent;
using TOutput = GameEnginePipeline.Contents.Sprites.SpriteRendererContent;

namespace GameEnginePipeline.Processors.Sprites
{
	/// <summary>
	/// Validates content and performs any additional logic necessary to prepare content to be written into the content pipeline.
	/// </summary>
	[ContentProcessor(DisplayName = "Sprite Renderer Processor - " + Constants.DLLIdentifier)]
	public sealed class SpriteRendererProcessor : Processor<TInput,TOutput,TAsset>
	{
		protected override TOutput? ValidateContent(TInput input, ContentProcessorContext context) => input; // There is nothing to validate with sprite renderers; the are always in a valid state

		protected override void CreateExternalDependencies(TOutput output, ContentProcessorContext context)
		{
			if(output.SourceFullName is not null)
				output.AddExternalReference<EffectContent>(context,output.SourceFullName);

			return;
		}
	}
}
