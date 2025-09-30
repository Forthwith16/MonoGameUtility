using GameEnginePipeline.Processors.Sprites;
using Microsoft.Xna.Framework.Content.Pipeline;

using TInput = GameEnginePipeline.Assets.Sprites.SpriteRendererAsset;
using TOutput = GameEnginePipeline.Contents.Sprites.SpriteRendererContent;

namespace GameEnginePipeline.Importers.Sprites
{
	/// <summary>
	/// Loads an asset into memory in content form.
	/// </summary>
	[ContentImporter(".ren",DisplayName = "Sprite Renderer Importer - " + Constants.DLLIdentifier,DefaultProcessor = nameof(SpriteRendererProcessor))]
	public sealed class SpriteRendererImporter : Importer<TInput,TOutput>
	{
		protected override TInput? Deserialize(string filename)
		{return TInput.Deserialize(filename);}

		protected override bool AddDependencies(string filename, ContentImporterContext context, TInput asset)
		{
			if(asset.ShaderSource is null)
				return true;
			
			context.AddDependency(Path.Combine(Path.GetDirectoryName(filename) ?? "",asset.ShaderSource));
			return true;
		}

		protected override TOutput ToContent(TInput asset, string filename)
		{return new TOutput(asset,filename);}
	}
}
