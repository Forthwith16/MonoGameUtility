using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

using TAsset = GameEnginePipeline.Assets.SpriteSheetAsset;
using TInput = GameEnginePipeline.Contents.SpriteSheetContent;
using TOutput = GameEnginePipeline.Contents.SpriteSheetContent;

namespace GameEnginePipeline.Processors
{
	/// <summary>
	/// Validates content and performs any additional logic necessary to prepare content to be written into the content pipeline.
	/// </summary>
	[ContentProcessor(DisplayName = "Sprite Sheet Processor - " + Constants.DLLIdentifier)]
	public sealed class SpriteSheetProcessor : Processor<TInput,TOutput,TAsset>
	{
		protected override TOutput? ValidateContent(TInput input, ContentProcessorContext context)
		{
			// Grab the asset for convenience
			TAsset asset = input.Asset;

			// We know Source is not null, so we need only check if we have at least one Sprite or valid tiling data
			if(asset.Sprites is not null && asset.Sprites.Length > 0)
				return input;

			// We have a tiling system, so we MUST have positive tile widths and heights
			if(asset.TileWidth is null || asset.TileWidth < 1 || asset.TileHeight is null || asset.TileHeight < 1)
				return null;

			// We need EXACTLY two out of three of the remaining positive tile parameters OR they can all be in agreement
			if(asset.TileVCount is null)
				if(asset.TileHCount is null)
					return null;
				else
					if(asset.TileCount is null || asset.TileHCount < 1 || asset.TileCount < 1)
						return null;
					else
						asset.TileVCount = (asset.TileCount + asset.TileHCount - 1) / asset.TileHCount;
			else
				if(asset.TileHCount is null)
					if(asset.TileCount is null || asset.TileVCount < 1 || asset.TileCount < 1)
						return null;
					else
						asset.TileHCount = (asset.TileCount + asset.TileVCount - 1) / asset.TileVCount;
				else
					if(asset.TileHCount < 1 || asset.TileVCount < 1)
						return null;
					else if(asset.TileCount is null)
						asset.TileCount = asset.TileVCount * asset.TileHCount;
					else if(asset.TileCount != asset.TileVCount * asset.TileHCount)
						return null;
			
			return input;
		}

		protected override void CreateDependencies(TOutput output, ContentProcessorContext context)
		{
			output.AddReference<Texture2DContent>(context,output.SourceFullName,new OpaqueDataDictionary());
			return;
		}
	}
}