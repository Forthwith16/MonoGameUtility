using GameEnginePipeline.Assets.Sprites;

namespace GameEnginePipeline.Contents.Sprites
{
	/// <summary>
	/// Represents Animation2DCollection content.
	/// </summary>
	public sealed class Animation2DCollectionContent : ContentItem<Animation2DCollectionAsset>
	{
		/// <summary>
		/// Transforms an asset into content.
		/// </summary>
		/// <param name="asset">The asset associated with this content.</param>
		/// <param name="filename">The filename of the asset.</param>
		public Animation2DCollectionContent(Animation2DCollectionAsset asset, string filename) : base(asset,filename)
		{
			SourceFullNames = new string[asset.Animations is null ? 0 : asset.Animations!.Length];

			for(int i = 0;i < SourceFullNames.Length;i++)
				SourceFullNames[i] = Path.Combine(Path.GetDirectoryName(filename) ?? "",asset.Animations![i].Source ?? "");

			return;
		}

		/// <summary>
		/// The full name of the source texture for the sprite sheet.
		/// This allows for the use of paths relative to the asset rather than to the content root.
		/// </summary>
		public string[] SourceFullNames
		{get;}
	}
}
