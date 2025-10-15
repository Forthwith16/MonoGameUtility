using GameEngine.Assets.Sprites;
using GameEnginePipeline.Readers.Sprites;
using Microsoft.Xna.Framework.Content.Pipeline;

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
			if(!asset.Source.GetFullPath(AbsoluteDirectory,Path.GetDirectoryName(".")!,out string? src))
				throw new InvalidContentException("An animation did not have a path to a sprite sheet source.\nAsset file: " + path);

			SourceFullName = src;
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
