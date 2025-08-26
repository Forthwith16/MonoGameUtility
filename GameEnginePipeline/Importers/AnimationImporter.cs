using GameEnginePipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline;
using System.IO;

using TInput = GameEnginePipeline.Assets.AnimationAssets.Animation2DAsset;
using TOutput = GameEnginePipeline.Contents.Animation2DContent;

namespace GameEnginePipeline.Importers
{
	/// <summary>
	/// Loads an asset into memory in content form.
	/// </summary>
	[ContentImporter(".animation",DisplayName = "Animation2D Importer - " + Constants.DLLIdentifier,DefaultProcessor = nameof(Animation2DProcessor))]
	public sealed class AnimationImporter : Importer<TInput,TOutput>
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
