using GameEnginePipeline.Assets.Sprites;

namespace GameEnginePipeline.Contents.Sprites
{
	/// <summary>
	/// Represents sprite renderer content.
	/// </summary>
	public class SpriteRendererContent : ContentItem<SpriteRendererAsset>
	{
		/// <summary>
		/// Transforms an asset into content.
		/// </summary>
		/// <param name="asset">The asset associated with this content.</param>
		/// <param name="filename">The filename of the asset.</param>
		public SpriteRendererContent(SpriteRendererAsset asset, string filename) : base(asset,filename)
		{
			if(asset.ShaderSource is null)
				SourceFullName = null;
			else
				SourceFullName = Path.Combine(Path.GetDirectoryName(filename) ?? "",asset.ShaderSource);

			return;
		}

		/// <summary>
		/// The full name of the shader (if any; null otherwise).
		/// This allows for the use of paths relative to the asset rather than to the content root.
		/// </summary>
		public string? SourceFullName
		{get;}
	}
}
