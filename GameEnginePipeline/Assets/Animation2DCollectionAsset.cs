using GameEngine.Utility.ExtensionMethods.SerializationExtensions;

namespace GameEnginePipeline.Assets
{
	/// <summary>
	/// Contains the raw asset data of an animation collection.
	/// </summary>
	public class Animation2DCollectionAsset : IAsset
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
		public static Animation2DCollectionAsset? Deserialize(string path)
		{return path.DeserializeXmlFile<Animation2DCollectionAsset>();}

		/// <summary>
		/// The animations of this collection.
		/// </summary>
		public NamedAnimation2D[]? Animations
		{get; set;}

		public bool ShouldSerializeAnimations()
		{return true;}

		/// <summary>
		/// The name of the idle animation.
		/// </summary>
		public string? IdleAnimation
		{get; set;}

		public bool ShouldSerializeIdleAnimation()
		{return true;}

		/// <summary>
		/// If true, then the collection resets the active animation after swapping in a new one.
		/// </summary>
		public bool ResetOnSwap
		{get; set;} = true; // This defaults to true, so let's be consistent

		public bool ShouldSerializeResetOnSwap()
		{return true;}
	}

	/// <summary>
	/// Encapsulates an animation paired with a name for it.
	/// </summary>
	public struct NamedAnimation2D
	{
		/// <summary>
		/// The source animation file.
		/// </summary>
		public string? Source
		{get; set;}
		
		public bool ShouldSerializeSource()
		{return true;}

		/// <summary>
		/// The animation name.
		/// </summary>
		public string? Name
		{get; set;}
		
		public bool ShouldSerializeName()
		{return true;}
	}
}
