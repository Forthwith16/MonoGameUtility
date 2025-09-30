using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

using TAsset = GameEnginePipeline.Assets.Sprites.SpriteSheetAsset;
using TRead = GameEngine.Sprites.SpriteSheet;
using TReader = GameEngine.Readers.SpriteSheetReader;
using TWrite = GameEnginePipeline.Contents.Sprites.SpriteSheetContent;

namespace GameEnginePipeline.Writers.Sprites
{
	/// <summary>
	/// Allows for writing assets to the pipeline.
	/// </summary>
	[ContentTypeWriter]
	public sealed class SpriteSheetWriter : Writer<TWrite,TReader,TRead>
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

			// First list the source texture
			cout.WriteExternalReference(value.GetExternalReference<Texture2DContent>(value.SourceFullName));
			
			// Next write out if we're specifying sprites manually (false) or if we're specifying a tile system (true)
			bool tile = asset.Sprites is null || asset.Sprites.Length == 0;
			cout.Write(tile);

			// The tile system specification is compact, so just write out what we need for it
			if(tile)
			{
				cout.Write(asset.TileFillRowFirst);
				cout.Write(asset.TileVCount!.Value);
				cout.Write(asset.TileHCount!.Value);
				cout.Write(asset.TileCount!.Value); // We're writting binary, so it's no problem to just read in a number without having to parse it or do multiplication
				cout.Write(asset.TileWidth!.Value);
				cout.Write(asset.TileHeight!.Value);

				return;
			}

			// We have a manual set of sprites, so just write out how many of them there are and then dump them all into the stream
			cout.Write(asset.Sprites!.Length);

			foreach(Rectangle rect in asset.Sprites)
				cout.Write(rect);

			return;
		}
	}
}
