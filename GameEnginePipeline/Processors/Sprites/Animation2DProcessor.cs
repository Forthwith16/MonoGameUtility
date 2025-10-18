using GameEnginePipeline.Contents.Sprites;
using Microsoft.Xna.Framework.Content.Pipeline;

using TAsset = GameEngine.Assets.Sprites.Animation2DAsset;
using TInput = GameEnginePipeline.Contents.Sprites.Animation2DContent;
using TOutput = GameEnginePipeline.Contents.Sprites.Animation2DContent;

namespace GameEnginePipeline.Processors.Sprites
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

			foreach(float d in asset.Frames.Select(value => value.Duration))
			{
				segs[++i] = segs[i - 1] + d;

				if(d <= 0.0f)
					return null;
			}

			// Assign the start time if we don't have it
			if(asset.StartTime is null)
				if(asset.StartFrame is null)
					asset.StartTime = 0.0f; // If we have neither start time, then we default to 0
				else
					asset.StartTime = segs[asset.StartFrame.Value];

			// If we have the wrong loop data, we'll have to fix that
			float loop_start;
			
			if(asset.LoopStart is null)
				if(asset.LoopFrameStart is null)
					loop_start = 0.0f;
				else
					if(asset.LoopFrameStart < 0 || asset.LoopFrameStart >= asset.Frames.Length)
						loop_start = -1.0f; // This is straight up not allowed, so give it a value that will cause problems
					else
						loop_start = segs[asset.LoopFrameStart.Value];
			else
				loop_start = asset.LoopStart.Value;

			float loop_end;
			
			if(asset.LoopEnd is null)
				if(asset.LoopFrameEnd is null)
					loop_end = segs[asset.Frames.Length];
				else
					if(asset.LoopFrameEnd < 0 || asset.LoopFrameEnd >= asset.Frames.Length)
						loop_end = -2.0f; // This is straight up not allowed, so give it a value that will cause problems
					else
						loop_end = segs[asset.LoopFrameEnd.Value + 1];
			else
				loop_end = asset.LoopEnd.Value;
			
			// All that we have left to do is check that the start time and loop data makes sense
			if(asset.StartTime < 0.0f || asset.StartTime > segs[asset.Frames.Length] || loop_end <= loop_start || loop_start < 0.0f || loop_end > segs[asset.Frames.Length])
				return null;

			// Now that everything is validated, we'll commit any changes we need to make
			asset.LoopStart = loop_start;
			asset.LoopEnd = loop_end;

			return input;
		}

		protected override void CreateExternalDependencies(TOutput output, ContentProcessorContext context)
		{
			output.AddExternalReference<SpriteSheetContent>(context,output.SourceFullName);
			return;
		}
	}
}