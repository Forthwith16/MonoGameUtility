using GameEnginePipeline.Processors.Sprites;
using Microsoft.Xna.Framework.Content.Pipeline;

using TInput = GameEngine.Assets.Sprites.Animation2DAsset;
using TOutput = GameEnginePipeline.Contents.Sprites.Animation2DContent;

namespace GameEnginePipeline.Importers.Sprites
{
	/// <summary>
	/// Loads an asset into memory in content form.
	/// </summary>
	[ContentImporter(".a2d",DisplayName = "Animation2D Importer - " + Constants.DLLIdentifier,DefaultProcessor = nameof(Animation2DProcessor))]
	public sealed class AnimationImporter : Importer<TInput,TOutput>
	{
		protected override TInput? Deserialize(string path)
		{return TInput.FromFile(path);}

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
