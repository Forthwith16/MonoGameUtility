using GameEngine.Framework;
using GameEngine.Utility.ExtensionMethods.SerializationExtensions;
using GameEnginePipeline.Assets.Sprites;

namespace GameEnginePipeline.Assets.Framework
{
	/// <summary>
	/// Contains the raw asset data of a GameObject.
	/// </summary>
	
	public abstract class GameObjectAsset : IAsset
	{
		/// <summary>
		/// Serializes an asset to <paramref name="path"/>.
		/// </summary>
		/// <param name="path">The desired path to the asset.</param>
		public void Serialize(string path) => this.SerializeJson(path);

		/// <summary>
		/// Deserializes an asset from <paramref name="path"/>.
		/// </summary>
		/// <param name="path">The path to the asset.</param>
		public static SpriteSheetAsset? Deserialize(string path) => path.DeserializeJsonFile<SpriteSheetAsset>();


		protected GameObjectAsset()
		{
			Enabled = true;
			UpdateOrder = 0;
			ID = AssetID.NULL;

			return;
		}


		protected GameObjectAsset(GameObject obj)
		{

			return;
		}

		/// <summary>
		/// If true, the game object is enabled.
		/// If false, it is disabled.
		/// </summary>
		public bool Enabled;

		/// <summary>
		/// The update order of the game object.
		/// Smaller valuse are updated before larger values.
		/// </summary>
		public int UpdateOrder;

		/// <summary>
		/// The ID of the game object within the asset domain.
		/// </summary>
		public AssetID ID;
	}
}
