using GameEngine.Framework;

namespace GameEngine.GameObjects
{
	/// <summary>
	/// A ultra lightweight dummy game object whose only purpose is to exist.
	/// </summary>
	public sealed class DummyGameObject : GameObject
	{
		/// <summary>
		/// Creates a dummy game object with all default values.
		/// </summary>
		public DummyGameObject() : base()
		{return;}

		/// <summary>
		/// Creates a dummy game object with the given ID.
		/// This is particularly useful for searching for game objects by ID, since game object equality is checked via ID.
		/// </summary>
		public DummyGameObject(GameObjectID id) : base(id)
		{return;}
	}
}
