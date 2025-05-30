﻿using GameEnginePipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline;
using System.IO;
using System.Linq;

using TInput = GameEnginePipeline.Assets.Animation2DCollectionAsset;
using TOutput = GameEnginePipeline.Contents.Animation2DCollectionContent;

namespace GameEnginePipeline.Importers
{
	/// <summary>
	/// Loads an asset into memory in content form.
	/// </summary>
	[ContentImporter(".animations",DisplayName = "Animation2D Collection Importer - Paradox",DefaultProcessor = nameof(Animation2DCollectionProcessor))]
	public class Animation2DCollectionImporter : Importer<TInput,TOutput>
	{
		protected override TInput? Deserialize(string filename)
		{return TInput.Deserialize(filename);}

		protected override bool AddDependencies(string filename, ContentImporterContext context, TInput asset)
		{
			if(asset.Animations is null)
				return false;

			foreach(string? path in asset.Animations.Select(na => na.Source))
				if(path is null)
					return false;
				else
					context.AddDependency(Path.Combine(Path.GetDirectoryName(filename) ?? "",path));

			return true;
		}

		protected override TOutput ToContent(TInput asset, string filename)
		{return new TOutput(asset,filename);}
	}
}
