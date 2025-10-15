using GameEnginePipeline.Processors.Sprites;
using Microsoft.Xna.Framework.Content.Pipeline;

using TInput = GameEngine.Assets.Sprites.SpriteSheetAsset;
using TOutput = GameEnginePipeline.Contents.Sprites.SpriteSheetContent;

namespace GameEnginePipeline.Importers.Sprites
{
	/// <summary>
	/// Loads an asset into memory in content form.
	/// </summary>
	[ContentImporter(".ss",DisplayName = "Sprite Sheet Importer - " + Constants.DLLIdentifier,DefaultProcessor = nameof(SpriteSheetProcessor))]
	public sealed class SpriteSheetImporter : Importer<TInput,TOutput>
	{
		protected override TInput? Deserialize(string path)
		{return TInput.Deserialize(path);}

		protected override bool AddDependencies(string path, ContentImporterContext context, TInput asset)
		{
			if(!asset.Source.GetFullPath(Path.GetDirectoryName(path)!,Path.GetDirectoryName(".")!,out string? src))
				return false;
			
			context.AddDependency(src);
			return true;
		}

		protected override TOutput ToContent(TInput asset, string path)
		{return new TOutput(asset,path);}
	}
}
