using GameEngine.Assets.Sprites;
using GameEnginePipeline.Processors.Sprites;
using GameEnginePipeline.Serialization;
using Microsoft.Xna.Framework.Content.Pipeline;

using TInput = GameEnginePipeline.Assets.Sprites.Animation2DCollectionAsset;
using TOutput = GameEnginePipeline.Contents.Sprites.Animation2DCollectionContent;

namespace GameEnginePipeline.Importers.Sprites
{
	/// <summary>
	/// Loads an asset into memory in content form.
	/// </summary>
	[ContentImporter(".a2ds",DisplayName = "Animation2D Collection Importer - " + Constants.DLLIdentifier,DefaultProcessor = nameof(Animation2DCollectionProcessor))]
	public class Animation2DCollectionImporter : Importer<TInput,TOutput>
	{
		protected override TInput? Deserialize(string path)
		{return TInput.Deserialize(path);}

		protected override bool AddDependencies(string path, ContentImporterContext context, TInput asset)
		{
			path = Path.GetDirectoryName(path) ?? "";

			foreach(AssetSource<Animation> a_source in asset.Animations.Select(na => na.Source))
				if(!a_source.GetFullPath(Path.GetDirectoryName(path)!,Path.GetDirectoryName(".")!,out string? src))
					return false;
				else
					context.AddDependency(src);
			
			return true;
		}

		protected override TOutput ToContent(TInput asset, string filename)
		{return new TOutput(asset,filename);}
	}
}
