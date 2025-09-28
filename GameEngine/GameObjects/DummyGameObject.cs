using GameEngine.Framework;
using GameEngine.Utility.ExtensionMethods.PrimitiveExtensions;
using GameEngine.Utility.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameEngine.GameObjects
{
	/// <summary>
	/// A ultra lightweight dummy game object whose only purpose is to exist.
	/// </summary>
	[JsonConverter(typeof(DummyGameObjectConverter))]
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

	/// <summary>
	/// Converts DummyGameObjects to/from JSON.
	/// </summary>
	file class DummyGameObjectConverter : JsonBaseConverter<DummyGameObject>
	{
		protected override object? ReadProperty(ref Utf8JsonReader reader, string property, JsonSerializerOptions ops)
		{
			switch(property)
			{
			case "Enabled":
				if(!reader.HasNextBool())
					throw new JsonException();

				return reader.GetBoolean();
			case "UpdateOrder":
				if(!reader.HasNextNumber())
					throw new JsonException();

				return reader.GetInt32();
			default:
				throw new JsonException();
			}
		}

		protected override DummyGameObject ConstructT(Dictionary<string,object?> properties)
		{
			DummyGameObject ret = new DummyGameObject();

			ret.Enabled = (bool)properties["Enabled"]!;
			ret.UpdateOrder = (int)properties["UpdateOrder"]!;

			return ret;
		}

		protected override void WriteProperties(Utf8JsonWriter writer, DummyGameObject value, JsonSerializerOptions ops)
		{
			writer.WriteBoolean("Enabled",value.Enabled);
			writer.WriteNumber("UpdateOrder",value.UpdateOrder);
			
			return;
		}
	}
}
