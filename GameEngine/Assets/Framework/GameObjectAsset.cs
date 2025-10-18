using GameEngine.Framework;

namespace GameEngine.Assets.Framework
{
	/// <summary>
	/// Contains the raw asset data of a GameObject.
	/// </summary>
	public abstract class GameObjectAsset : AssetBase
	{
		/*
		protected override void Serialize(string path, string root, bool overwrite_dependencies = false) => this.SerializeJson(path,new JsonGameObjectConverter()); // We will need state information, so we need a new converter every time (better than running Clean, b/c it becomes thread safe this way)

		/// <summary>
		/// Deserializes an asset from <paramref name="path"/>.
		/// </summary>
		/// <param name="path">The path to the asset.</param>
		public static GameObjectAsset? FromFile(string path) => path.DeserializeJsonFile(new JsonGameObjectConverter()); // We will need state information, so we need a new converter every time (better than running Clean, b/c it becomes thread safe this way)
		*/

		/// <summary>
		/// Creates an asset version of a game object with all default values.
		/// </summary>
		protected GameObjectAsset() : base()
		{
			Enabled = true;
			UpdateOrder = 0;
			
			return;
		}

		/// <summary>
		/// Creates an asset version of <paramref name="obj"/>.
		/// </summary>
		protected GameObjectAsset(GameObject obj) : base()
		{
			Enabled = obj.Enabled;
			UpdateOrder = obj.UpdateOrder;

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
	}
}
