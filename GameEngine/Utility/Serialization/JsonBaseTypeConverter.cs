using GameEngine.Utility.ExtensionMethods.ClassExtensions;
using GameEngine.Utility.ExtensionMethods.PrimitiveExtensions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameEngine.Utility.Serialization
{
	/// <summary>
	/// Converts a base type to/from a JSON format.
	/// To do so, it serializes/deserializes <typeparamref name="T"/> types as a generic object to hunt down the appropriate converter.
	/// </summary>
	/// <typeparam name="T">This will be the base type converted. More derived types from <typeparamref name="T"/> should have their own specialized conveters.</typeparam>
	public abstract class JsonBaseTypeConverter<T> : JsonConverter<T>
	{
		public JsonBaseTypeConverter() : base()
		{
			t = typeof(T);
			return;
		}

		public override bool CanConvert(Type target_type) => t.IsAssignableFrom(target_type);

		public override T Read(ref Utf8JsonReader reader, Type type_to_convert, JsonSerializerOptions ops) => reader.ReadObject<T>(ops) ?? throw new JsonException();
		public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions ops) => writer.WriteObject(value,ops);

		private Type t;
	}
}
