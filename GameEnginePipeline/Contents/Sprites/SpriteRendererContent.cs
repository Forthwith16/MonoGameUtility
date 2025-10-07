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
		/// <param name="path">The absolute path to the asset file. This includes the filename itself.</param>
		public SpriteRendererContent(SpriteRendererAsset asset, string path) : base(asset,path)
		{
			if(asset.ShaderSource is null)
				ShaderSourceFullPath = null;
			else
				ShaderSourceFullPath = Path.GetFullPath(Path.Combine(AbsoluteDirectory,asset.ShaderSource));

			return;
		}

		/// <summary>
		/// The full path to the shader source (if any; null otherwise).
		/// </summary>
		public string? ShaderSourceFullPath
		{get;}
	}
}
