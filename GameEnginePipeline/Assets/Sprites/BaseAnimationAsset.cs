using GameEngine.Utility.ExtensionMethods.SerializationExtensions;
using GameEnginePipeline.Readers.Sprites;

namespace GameEnginePipeline.Assets.Sprites
{
	/// <summary>
	/// Contains the base raw asset data of an animation.
	/// </summary>
	public abstract class BaseAnimationAsset<TSelf> : AssetBase where TSelf : BaseAnimationAsset<TSelf>
	{
		public static TSelf? Deserialize(string path, bool overwrite_dependencies = false) => path.DeserializeJsonFile<TSelf>();

		/// <summary>
		/// Initializes the base animaiton data.
		/// </summary>
		/// <param name="type">The type of the animation.</param>
		protected BaseAnimationAsset(AnimationType type) : base()
		{
			Type = type;
			return;
		}

		/// <summary>
		/// The animation type.
		/// </summary>
		public AnimationType Type
		{get;}
	}

	/// <summary>
	/// The types of animation assets that we can load.
	/// </summary>
	public enum AnimationAssetType
	{
		Animation2DAsset
	}
}
