using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

using TAsset = GameEngine.Assets.Sprites.SpriteRendererAsset;
using TRead = GameEngine.Resources.Sprites.SpriteRenderer;
using TReader = GameEnginePipeline.Readers.Sprites.SpriteRendererReader;
using TWrite = GameEnginePipeline.Contents.Sprites.SpriteRendererContent;

namespace GameEnginePipeline.Writers.Sprites
{
	/// <summary>
	/// Allows for writing assets to the pipeline.
	/// </summary>
	[ContentTypeWriter]
	public sealed class SpriteRendererWriter : Writer<TWrite,TReader,TRead>
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

			// First output the sprite renderer's name
			cout.Write(value.Name);

			// Next list the effect source if we have one (and send a flag with it)
			cout.Write(value.ShaderSourceFullPath is not null);

			if(value.ShaderSourceFullPath is not null)
			{
				cout.WriteExternalReference(value.GetExternalReference<EffectContent>(value.ShaderSourceFullPath));
				cout.Write(Path.GetExtension(value.ShaderSourceFullPath));
			}
			
			// We need to write out every enum
			cout.Write(asset.Order);
			cout.Write(asset.Blend);
			cout.Write(asset.Wrap);
			cout.Write(asset.DepthStencil);
			cout.Write(asset.Cull);

			return;
		}
	}
}
