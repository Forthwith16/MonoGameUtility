using GameEnginePipeline.Assets.Sprites;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace GameEnginePipeline.Contents.Sprites
{
	/// <summary>
	/// Represents sprite sheet content.
	/// </summary>
	public sealed class SpriteSheetContent : ContentItem<SpriteSheetAsset>
	{
		/// <summary>
		/// Transforms an asset into content.
		/// </summary>
		/// <param name="asset">The asset associated with this content.</param>
		/// <param name="path">The absolute path to the asset file. This includes the filename itself.</param>
		/// <exception cref="InvalidContentException">Thrown if <paramref name="asset"/> does not contain a path to the source texture.</exception>
		public SpriteSheetContent(SpriteSheetAsset asset, string path) : base(asset,path)
		{
			if(!asset.Source.GetFullPath(AbsoluteDirectory,Path.GetDirectoryName(".")!,out string? src))
				throw new InvalidContentException("A sprite sheet did not have a path to a texture source.\nAsset file: " + path);

			SourceFullPath = src;
			return;
		}

		/// <summary>
		/// The full path of the source texture for the sprite sheet.
		/// </summary>
		public string SourceFullPath;
	}
}
