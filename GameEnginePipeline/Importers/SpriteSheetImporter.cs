using GameEnginePipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline;
using System.IO;

using TInput = GameEnginePipeline.Assets.SpriteSheetAsset;
using TOutput = GameEnginePipeline.Contents.SpriteSheetContent;

namespace GameEnginePipeline.Importers
{
	/// <summary>
	/// Loads an asset into memory in content form.
	/// </summary>
	[ContentImporter(".spritesheet",DisplayName = "Sprite Sheet Importer - Paradox",DefaultProcessor = nameof(SpriteSheetProcessor))]
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
