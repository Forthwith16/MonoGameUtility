using GameEnginePipeline.Contents;
using Microsoft.Xna.Framework.Content.Pipeline;
using System.Linq;

using TAsset = GameEnginePipeline.Assets.AnimationAssets.Animation2DAsset;
using TInput = GameEnginePipeline.Contents.Animation2DContent;
using TOutput = GameEnginePipeline.Contents.Animation2DContent;

namespace GameEnginePipeline.Processors
{
	/// <summary>
	/// Validates content and performs any additional logic necessary to prepare content to be written into the content pipeline.
	/// </summary>
	[ContentProcessor(DisplayName = "Animation2D Processor - " + Constants.DLLIdentifier)]
	public sealed class Animation2DProcessor : Processor<TInput,TOutput,TAsset>
	{
		protected override TOutput? ValidateContent(TInput input, ContentProcessorContext context)
		{
			// Get the asset for convenience
			TAsset asset = input.Asset;

			// We know Source is not null and the content builder will error if it doesn't exist, so we don't have to worry about that
			// We do, however, need to check that we have frame data
			if(asset.Frames is null || asset.Frames.Length == 0)
				return null;

			// We will have to trust that the textures all exist in the sprite sheet, but we can check that the durations are nontrivial
			float[] segs = new float[asset.Frames.Length + 1];
			segs[0] = 0.0f;

			int i = 0;

			foreach(float d in asset.Frames.Select((value,index) => value.Duration))
			{
				segs[++i] = segs[i - 1] + d;

				if(d <= 0.0f)
					return null;
			}

			// If we have the wrong loop data, we'll have to fix that
			float loop_start = asset.LoopStart is null ? (asset.LoopFrameStart is null ? 0.0f : segs[asset.LoopFrameStart.Value]) : asset.LoopStart.Value;
			float loop_end = asset.LoopEnd is null ? (asset.LoopFrameEnd is null ? segs[asset.Frames.Length] : segs[asset.LoopFrameEnd.Value + 1]) : asset.LoopEnd.Value;

			// All that we have left to do is check that the loop data makes sense (and assign anything that we're missing)
			if(loop_end <= loop_start || loop_start < 0.0f || loop_end > segs[asset.Frames.Length])
				return null;

			// Now that everything is validated, we'll commit any changes we need to make
			asset.LoopStart = loop_start;
			asset.LoopEnd = loop_end;

			return input;
		}

		protected override void CreateDependencies(TOutput output, ContentProcessorContext context)
		{
			output.AddReference<SpriteSheetContent>(context,output.SourceFullName,new OpaqueDataDictionary());
			return;
		}
	}
}