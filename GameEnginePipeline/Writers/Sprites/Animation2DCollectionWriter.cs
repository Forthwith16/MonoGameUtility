using GameEnginePipeline.Contents.Sprites;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

using TAsset = GameEnginePipeline.Assets.Sprites.Animation2DCollectionAsset;
using TRead = GameEngine.Sprites.Animation2DCollection;
using TReader = GameEnginePipeline.Readers.Sprites.Animation2DCollectionReader;
using TWrite = GameEnginePipeline.Contents.Sprites.Animation2DCollectionContent;

namespace GameEnginePipeline.Writers.Sprites
{
	/// <summary>
	/// Allows for writing assets to the pipeline.
	/// </summary>
	[ContentTypeWriter]
	public sealed class Animation2DCollectionWriter : Writer<TWrite,TReader,TRead>
	{
		/// <summary>
		/// Writes the content <paramref name="value"/> to <paramref name="output"/>.
		/// </summary>
		/// <param name="cout">The writer context.</param>
		/// <param name="value">The content to write.</param>
		protected override void Write(ContentWriter cout, TWrite value)
		{
			// Grab the asset for convenience
			TAsset asset = value.Asset;

			// First write the number of animations
			cout.Write(asset.Animations.Length);

			// Now write out each animation with name and then the external reference
			for(int i = 0;i < asset.Animations.Length;i++)
			{
				cout.Write(asset.Animations[i].Name!);
				cout.WriteExternalReference(value.GetExternalReference<Animation2DContent>(value.SourceFullNames[i]));
			}

			// Now write the idle animation name
			cout.Write(asset.IdleAnimation!);
			
			// Lastly, write out the reset flag
			cout.Write(asset.ResetOnSwap);

			return;
		}
	}
}
