using GameEnginePipeline.Assets.Sprites;

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
		public SpriteSheetContent(SpriteSheetAsset asset, string path) : base(asset,path)
		{
			SourceFullPath = Path.GetFullPath(Path.Combine(AbsoluteDirectory,asset.Source!)); // If Source is null, we should seg fault to interrupt the build
			return;
		}

		/// <summary>
		/// The full path of the source texture for the sprite sheet.
		/// </summary>
		public string SourceFullPath
		{get;}
	}
}
