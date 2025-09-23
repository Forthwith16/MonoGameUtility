using GameEngine.Utility.ExtensionMethods.PrimitiveExtensions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameEngine.Utility.Serialization
{
	/// <summary>
	/// The base JSON converter for turning <typeparamref name="T"/> types to and from JSON.
	/// </summary>
	/// <typeparam name="T">The type to convert.</typeparam>
	public abstract class JsonBaseConverter<T> : JsonConverter<T>
	{
		/// <inheritdoc/>
		/// <param name="type_to_convert">The type to convert.</param>
		/// <param name="ops">The JSON options.</param>
		/// <returns>Returns the value read.</returns>
		/// <exception cref="JsonException">Thrown if anything goes wrong.</exception>
		public override T Read(ref Utf8JsonReader reader, Type type_to_convert, JsonSerializerOptions ops)
		{
			// We start with the object opening
			if(!reader.HasNextObjectStart())
				throw new JsonException();
			
			reader.Read();

			// We'll need to track what properties we've already done
			// This will also contain the values we read
			Dictionary<string,object?> processed = new Dictionary<string,object?>();

			// Loop until we reach the end of the object
			while(!reader.HasNextObjectEnd())
			{
				if(!reader.HasNextProperty())
					throw new JsonException();

				string property_name = reader.GetString()!;
				reader.Read();

				if(processed.ContainsKey(property_name))
					throw new JsonException();

				processed[property_name] = ReadProperty(ref reader,property_name,ops);
				reader.Read();
			}

			return ConstructT(processed);
		}

		/// <summary>
		/// Reads the value of a property from a JSON reader.
		/// </summary>
		/// <param name="reader">The reader.</param>
		/// <param name="property">The property to read.</param>
		/// <param name="ops">The JSON options.</param>
		/// <returns>Returns the value read.</returns>
		/// <exception cref="JsonException">Thrown if anything goes wrong.</exception>
		protected abstract object? ReadProperty(ref Utf8JsonReader reader, string property, JsonSerializerOptions ops);

		/// <summary>
		/// Constructs a <typeparamref name="T"/> type from the values provided.
		/// </summary>
		/// <param name="properties">The set of properties read from the JSON deserialization process.</param>
		/// <returns>Returns the constructed <typeparamref name="T"/> type.</returns>
		/// <exception cref="JsonException">Thrown if anything goes wrong, particularly if not enough properties or the wrong properties were read.</exception>
		protected abstract T ConstructT(Dictionary<string,object?> properties);

		/// <inheritdoc/>
		/// <param name="ops">The JSON options.</param>
		public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions ops)
		{
			writer.WriteStartObject();
			WriteProperties(writer,value,ops);
			writer.WriteEndObject();

			return;
		}

		/// <summary>
		/// Writes the JSON properties/values of <paramref name="value"/> out.
		/// </summary>
		/// <param name="writer">The JSON serializer.</param>
		/// <param name="value">The value to write out.</param>
		/// <param name="ops">The JSON options.</param>
		/// <remarks>This should not write an object start/end, as that has already been done.</remarks>
		/// <exception cref="JsonException">Thrown if anything goes wrong.</exception>
		protected abstract void WriteProperties(Utf8JsonWriter writer, T value, JsonSerializerOptions ops);
	}
}
