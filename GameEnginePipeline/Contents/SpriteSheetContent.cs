using GameEnginePipeline.Assets;
using System.IO;

namespace GameEnginePipeline.Contents
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
		/// <param name="filename">The filename of the asset.</param>
		public SpriteSheetContent(SpriteSheetAsset asset, string filename) : base(asset,filename)
		{
			SourceFullName = Path.Combine(Path.GetDirectoryName(filename) ?? "",asset.Source!);
			return;
		}

		/// <summary>
		/// The full name of the source texture for the sprite sheet.
		/// This allows for the use of paths relative to the asset rather than to the content root.
		/// </summary>
		public string SourceFullName
		{get;}
	}
}
