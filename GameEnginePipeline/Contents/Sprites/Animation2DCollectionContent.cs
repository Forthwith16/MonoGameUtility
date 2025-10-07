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
		/// <param name="path">The absolute path to the asset file. This includes the filename itself.</param>
		public Animation2DCollectionContent(Animation2DCollectionAsset asset, string path) : base(asset,path)
		{
			AnimationFullNames = new string[asset.Animations.Length];

			for(int i = 0;i < AnimationFullNames.Length;i++)
				AnimationFullNames[i] = Path.GetFullPath(Path.Combine(AbsoluteDirectory,asset.Animations[i].Source ?? ""));

			return;
		}

		/// <summary>
		/// The full paths of the source animation files for the animations.
		/// </summary>
		public string[] AnimationFullNames
		{get;}
	}
}
