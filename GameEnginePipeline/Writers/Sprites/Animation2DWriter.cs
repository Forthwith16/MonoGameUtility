using GameEnginePipeline.Assets.Sprites;
using GameEnginePipeline.Contents.Sprites;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

using TRead = GameEngine.Assets.Sprites.Animation;
using TReader = GameEnginePipeline.Readers.Sprites.AnimationReader;
using TWrite = GameEnginePipeline.Contents.Sprites.Animation2DContent;

namespace GameEnginePipeline.Writers.Sprites
{
	/// <summary>
	/// Allows for writing assets to the pipeline.
	/// </summary>
	[ContentTypeWriter]
	public sealed class Animation2DWriter : Writer<TWrite,TReader,TRead>
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

			// Next output the animation's name
			cout.Write(value.Name);

			// Grab the asset for convenience
			Animation2DAsset asset = value.Asset;

			// First list the source texture
			cout.WriteExternalReference(value.GetExternalReference<SpriteSheetContent>(value.SourceFullName));

			// Write the number of frames
			cout.Write(asset.Frames!.Length);

			// Write out each frame's data with indices interleaved with duration in that order
			foreach(Animation2DKeyFrame frame in asset.Frames)
			{
				cout.Write(frame.Sprite);
				cout.Write(frame.Duration);
				cout.Write(frame.Translation,frame.Rotation,frame.Scale,frame.Origin);
			}
			
			// Write the start time
			cout.Write(asset.StartTime!.Value);

			// Lastly, write out our loop information
			cout.Write(asset.Loops);
			cout.Write(asset.LoopStart!.Value);
			cout.Write(asset.LoopEnd!.Value);

			return;
		}
	}
}
