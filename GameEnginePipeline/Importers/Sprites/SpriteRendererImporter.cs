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
		protected override TInput? Deserialize(string path)
		{return TInput.Deserialize(path);}

		protected override bool AddDependencies(string path, ContentImporterContext context, TInput asset)
		{
			if(asset.ShaderSource.GetFullPath(Path.GetDirectoryName(path)!,Path.GetDirectoryName(".")!,out string? src))
				context.AddDependency(src);
			
			return true;
		}

		protected override TOutput ToContent(TInput asset, string path)
		{return new TOutput(asset,path);}
	}
}
