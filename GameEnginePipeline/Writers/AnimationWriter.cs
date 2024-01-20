using GameEnginePipeline.Assets.AnimationAssets;
using GameEnginePipeline.Contents;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

using TRead = GameEngine.Sprites.Animation;
using TReader = GameEngine.Readers.AnimationReader;
using TWrite = GameEnginePipeline.Contents.Animation2DContent;

namespace GameEnginePipeline.Writers
{
	/// <summary>
	/// Allows for writing assets to the pipeline.
	/// </summary>
	[ContentTypeWriter]
	public sealed class AnimationWriter : Writer<TWrite,TReader,TRead>
	{
		/// <summary>
		/// Writes the content <paramref name="value"/> to <paramref name="output"/>.
		/// </summary>
		/// <param name="cout">The writer context.</param>
		/// <param name="value">The content to write.</param>
		protected override void Write(ContentWriter cout, TWrite value)
		{
			// First write out what kind of animation the reader must deal with
			cout.Write((int)value.Type);

			// Grab the asset for convenience
			Animation2DAsset asset = value.Asset;

			// First list the source texture
			cout.WriteExternalReference(value.GetReference<SpriteSheetContent>(value.SourceFullName));

			// Write the number of frames
			cout.Write(asset.Frames!.Length);

			// Write out each frame's data with indices interleaved with duration in that order
			foreach(Animation2DKeyFrame frame in asset.Frames)
			{
				cout.Write(frame.Sprite);
				cout.Write(frame.Duration);
				cout.Write(frame.Translation,frame.Scale,frame.Rotation);
			}
			
			// Lastly, write out our loop information
			cout.Write(asset.Loops);
			cout.Write(asset.LoopStart!.Value);
			cout.Write(asset.LoopEnd!.Value);

			return;
		}
	}
}
