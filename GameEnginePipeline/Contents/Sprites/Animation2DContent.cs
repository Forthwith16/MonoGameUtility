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
		/// <param name="filename">The filename of the asset.</param>
		public Animation2DContent(Animation2DAsset asset, string filename) : base(asset,filename)
		{
			SourceFullName = Path.Combine(Path.GetDirectoryName(filename) ?? "",asset.Source!);
			return;
		}

		/// <summary>
		/// The full name of the source sprite sheet for the animation.
		/// This allows for the use of paths relative to the asset rather than to the content root.
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
