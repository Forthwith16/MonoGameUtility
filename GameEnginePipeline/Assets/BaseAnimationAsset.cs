using GameEngine.Readers;
using GameEngine.Utility.ExtensionMethods.SerializationExtensions;

namespace GameEnginePipeline.Assets
{
	/// <summary>
	/// Contains the base raw asset data of an animation.
	/// </summary>
	public abstract class BaseAnimationAsset<TSelf> : IAsset where TSelf : BaseAnimationAsset<TSelf>
	{
		/// <summary>
		/// Serializes an asset to <paramref name="path"/>.
		/// </summary>
		/// <param name="path">The desired path to the asset.</param>
		public void Serialize(string path)
		{
			this.SerializeXml(path);
			return;
		}

		/// <summary>
		/// Deserializes an asset from <paramref name="path"/>.
		/// </summary>
		/// <param name="path">The path to the asset.</param>
		public static TSelf? Deserialize(string path)
		{return path.DeserializeXmlFile<TSelf>();}

		/// <summary>
		/// Initializes the base animaiton data.
		/// </summary>
		/// <param name="type">The type of the animation.</param>
		protected BaseAnimationAsset(AnimationType type)
		{
			Type = type;
			return;
		}

		/// <summary>
		/// The animation type.
		/// </summary>
		public AnimationType Type
		{get; init;}

		public bool ShouldSerializeType()
		{return true;}
	}

	/// <summary>
	/// The types of animation assets that we can load.
	/// </summary>
	public enum AnimationAssetType
	{
		None,
		Animation2DAsset
	}
}
