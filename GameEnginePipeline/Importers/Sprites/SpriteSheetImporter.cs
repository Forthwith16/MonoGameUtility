using GameEnginePipeline.Processors.Sprites;
using Microsoft.Xna.Framework.Content.Pipeline;

using TInput = GameEnginePipeline.Assets.Sprites.SpriteSheetAsset;
using TOutput = GameEnginePipeline.Contents.Sprites.SpriteSheetContent;

namespace GameEnginePipeline.Importers.Sprites
{
	/// <summary>
	/// Loads an asset into memory in content form.
	/// </summary>
	[ContentImporter(".ss",DisplayName = "Sprite Sheet Importer - " + Constants.DLLIdentifier,DefaultProcessor = nameof(SpriteSheetProcessor))]
	public sealed class SpriteSheetImporter : Importer<TInput,TOutput>
	{
		protected override TInput? Deserialize(string filename)
		{return TInput.Deserialize(filename);}

		protected override bool AddDependencies(string filename, ContentImporterContext context, TInput asset)
		{
			if(asset.Source is null)
				return false;

			context.AddDependency(Path.Combine(Path.GetDirectoryName(filename) ?? "",asset.Source));
			return true;
		}

		protected override TOutput ToContent(TInput asset, string filename)
		{return new TOutput(asset,filename);}
	}
}
