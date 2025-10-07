using GameEnginePipeline.Assets.Sprites;
using GameEnginePipeline.Readers.Sprites;

namespace GameEnginePipeline.Contents.Sprites
{
	/// <summary>
	/// Represents 2D animation content.
	/// </summary>
	public sealed class Animation2DContent : ContentItem<Animation2DAsset>
	{
		/// <summary>
		/// Transforms an asset into content.
		/// </summary>
		/// <param name="asset">The asset associated with this content.</param>
		/// <param name="path">The absolute path to the asset file. This includes the filename itself.</param>
		public Animation2DContent(Animation2DAsset asset, string path) : base(asset,path)
		{
			SourceFullName = Path.GetFullPath(Path.Combine(AbsoluteDirectory,asset.Source!)); // If Source is null, we should seg fault to interrupt the build
			return;
		}

		/// <summary>
		/// The full path of the source sprite sheet for the animation.
		/// </summary>
		public string SourceFullName
		{get; init;}

		/// <summary>
		/// The animation type.
		/// </summary>
		public AnimationType Type
		{get => AnimationType.Animation2D;}
	}
}
