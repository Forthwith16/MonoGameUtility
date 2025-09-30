using GameEngine.Framework;
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
			if(JsonGameObjectConverter.ReadStandardGameObjectProperty(ref reader,property,ops,out object? p))
				return p;

			throw new JsonException();
		}

		protected override DummyGameObject ConstructT(Dictionary<string,object?> properties)
		{
			if(properties.Count != 2)
				throw new JsonException();

			DummyGameObject ret = new DummyGameObject();

			ret.Enabled = (bool)properties["Enabled"]!;
			ret.UpdateOrder = (int)properties["UpdateOrder"]!;

			return ret;
		}

		protected override void WriteProperties(Utf8JsonWriter writer, DummyGameObject value, JsonSerializerOptions ops)
		{
			JsonGameObjectConverter.WriteStandardProperties(writer,value,ops);
			return;
		}
	}
}
