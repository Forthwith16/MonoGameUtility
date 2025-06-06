﻿using GameEnginePipeline.Assets;
using GameEnginePipeline.Contents;
using Microsoft.Xna.Framework.Content.Pipeline;
using System.Collections.Generic;
using TAsset = GameEnginePipeline.Assets.Animation2DCollectionAsset;
using TInput = GameEnginePipeline.Contents.Animation2DCollectionContent;
using TOutput = GameEnginePipeline.Contents.Animation2DCollectionContent;

namespace GameEnginePipeline.Processors
{
	/// <summary>
	/// Validates content and performs any additional logic necessary to prepare content to be written into the content pipeline.
	/// </summary>
	[ContentProcessor(DisplayName = "Animation2D Collection Processor - Paradox")]
	public sealed class Animation2DCollectionProcessor : Processor<TInput,TOutput,TAsset>
	{
		protected override TOutput? ValidateContent(TInput input, ContentProcessorContext context)
		{
			// Grab the asset for convenience
			TAsset asset = input.Asset;

			// We neeed to check if each animation name is unique, not null, and one of them is the idle animation (the latter will incidentally check if we have at least 1 animation)
			HashSet<string> names = new HashSet<string>();
			
			foreach(NamedAnimation2D a in asset.Animations!)
				if(a.Name is null || !names.Add(a.Name))
					return null;

			if(asset.IdleAnimation is null || !names.Contains(asset.IdleAnimation))
				return null;

			return input;
		}

		protected override void CreateDependencies(TOutput output, ContentProcessorContext context)
		{
			foreach(string path in output.SourceFullNames)
				output.AddReference<Animation2DContent>(context,path,new OpaqueDataDictionary());

			return;
		}
	}
}
