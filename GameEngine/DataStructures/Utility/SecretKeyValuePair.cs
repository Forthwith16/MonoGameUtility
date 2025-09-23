using GameEngine.Utility.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameEngine.DataStructures.Utility
{
	/// <summary>
	/// Represents a key value pair, but better.
	/// </summary>
	/// <typeparam name="K">The key type.</typeparam>
	/// <typeparam name="V">The value type.</typeparam>
	[JsonConverter(typeof(JsonSecretKeyValuePairConverter))]
	public readonly struct SecretKeyValuePair<K,V> where K : notnull
	{
		public SecretKeyValuePair(K k, V v)
		{
			Key = k;
			Value = v;

			return;
		}

		public static implicit operator SecretKeyValuePair<K,V>(KeyValuePair<K,V> kvp) => new SecretKeyValuePair<K,V>(kvp.Key,kvp.Value);

		public readonly K Key;
		public readonly V Value;
	}

	/// <summary>
	/// Creates JSON converters for key value pairs.
	/// </summary>
	public class JsonSecretKeyValuePairConverter : JsonBaseConverterFactory
	{
		/// <summary>
		/// Constructs the factory.
		/// </summary>
		public JsonSecretKeyValuePairConverter() : base((t,ops) => [ops],typeof(SecretKeyValuePair<,>),typeof(SKVPC<,>))
		{return;}

		/// <summary>
		/// Performs the JSON conversion for a key value pair.
		/// </summary>
		/// <typeparam name="K">The key type.</typeparam>
		/// <typeparam name="V">The value type.</typeparam>
		private class SKVPC<K, V> : JsonBaseConverter<SecretKeyValuePair<K,V>> where K : notnull
		{
			public SKVPC(JsonSerializerOptions ops)
			{
				KConverter = (JsonConverter<K>)ops.GetConverter(typeof(K));
				VConverter = (JsonConverter<V>)ops.GetConverter(typeof(V));

				return;
			}

			protected override object? ReadProperty(ref Utf8JsonReader reader, string property, JsonSerializerOptions ops)
			{
				switch(property)
				{
				case "Key":
					return KConverter.Read(ref reader,typeof(K),ops) ?? throw new JsonException(); // Keys cannot be null
				case "Value":
					return VConverter.Read(ref reader,typeof(V),ops); // Values CAN be null
				default:
					throw new JsonException();
				}
			}

			protected override SecretKeyValuePair<K,V> ConstructT(Dictionary<string,object?> properties)
			{
				if(properties.Count != 2)
					throw new JsonException();

				return new SecretKeyValuePair<K,V>((K)properties["Key"]!,(V)properties["Value"]!);
			}

			protected override void WriteProperties(Utf8JsonWriter writer, SecretKeyValuePair<K,V> value, JsonSerializerOptions ops)
			{
				writer.WritePropertyName("Key");
				KConverter.Write(writer,value.Key,ops);

				writer.WritePropertyName("Value");
				VConverter.Write(writer,value.Value,ops);

				return;
			}

			private JsonConverter<K> KConverter;
			private JsonConverter<V> VConverter;
		}
	}
}
