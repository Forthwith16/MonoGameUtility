using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameEngine.Utility.ExtensionMethods.SerializationExtensions
{
	/// <summary>
	/// Provides JSON related extensions to strings.
	/// </summary>
	public static class JsonSerializedStringExtensions
	{
		#region Streams
		/// <summary>
		/// Converts a JSON object held in a stream into a <typeparamref name="T"/> type.
		/// </summary>
		/// <typeparam name="T">The type to convert to.</typeparam>
		/// <param name="stream">The stream to read from containing a JSON serialized object of type <typeparamref name="T"/>.</param>
		/// <returns>Returns the converted object or null if <paramref name="stream"/> did not contain a valid JSON serialization of a <typeparamref name="T"/> type.</returns>
		public static T? DeserializeJson<T>(this Stream stream) => JsonSerializer.Deserialize<T>(stream,JsonSerializationExtensions.SufficientReadWrite);

		/// <summary>
		/// Converts a JSON object held in a stream into a <typeparamref name="T"/> type.
		/// </summary>
		/// <typeparam name="T">The type to convert to.</typeparam>
		/// <param name="stream">The stream to read from containing a JSON serialized object of type <typeparamref name="T"/>.</param>
		/// <param name="converter">
		/// The converter to use for the deserialization.
		/// This will be added to the JsonSerializerOptions's set of converters.
		/// This will take precedence over the default <typeparamref name="T"/> type converter but will lose precedence to a property with a custom JsonConverter attribute.
		/// </param>
		/// <remarks>The strength of this method is that it allows for multiple truly independent conversions of a type to occur simultaneously, allowing for converters to contain member state information exclusive to the object being converted.</remarks>
		public static T? DeserializeJson<T>(this Stream stream, JsonConverter<T> converter)
		{
			JsonSerializerOptions ops = new JsonSerializerOptions(JsonSerializationExtensions.SufficientReadWrite);
			ops.Converters.Add(converter);

			return stream.DeserializeJson<T>(ops);
		}

		/// <summary>
		/// Converts a JSON object held in a stream into a <typeparamref name="T"/> type.
		/// </summary>
		/// <typeparam name="T">The type to convert to.</typeparam>
		/// <param name="stream">The stream to read from containing a JSON serialized object of type <typeparamref name="T"/>.</param>
		/// <param name="converter">
		/// The converter factory to use for the deserialization.
		/// This will be added to the JsonSerializerOptions's set of converters.
		/// This will take precedence over the default <typeparamref name="T"/> type converter but will lose precedence to a property with a custom JsonConverter attribute.
		/// </param>
		/// <remarks>The strength of this method is that it allows for multiple truly independent conversions of a type to occur simultaneously, allowing for converters to contain member state information exclusive to the object being converted.</remarks>
		public static T? DeserializeJson<T>(this Stream stream, JsonConverterFactory converter_factory)
		{
			JsonSerializerOptions ops = new JsonSerializerOptions(JsonSerializationExtensions.SufficientReadWrite);
			ops.Converters.Add(converter_factory);

			return stream.DeserializeJson<T>(ops);
		}

		/// <summary>
		/// Converts a JSON object held in a stream into a <typeparamref name="T"/> type.
		/// </summary>
		/// <typeparam name="T">The type to convert to.</typeparam>
		/// <param name="stream">The stream to read from containing a JSON serialized object of type <typeparamref name="T"/>.</param>
		/// <param name="ops">The options to pass to the serializer.</param>
		/// <returns>Returns the converted object or null if <paramref name="stream"/> did not contain a valid JSON serialization of a <typeparamref name="T"/> type.</returns>
		public static T? DeserializeJson<T>(this Stream stream, JsonSerializerOptions ops) => JsonSerializer.Deserialize<T>(stream,ops);
		#endregion

		#region Files
		/// <summary>
		/// Converts a JSON file specified by <paramref name="path"/> into a <typeparamref name="T"/> type.
		/// </summary>
		/// <typeparam name="T">The type to convert to.</typeparam>
		/// <param name="path">The path to an JSON file specifying a <typeparamref name="T"/> type.</param>
		/// <returns>Returns the converted object or null if <paramref name="path"/> was not a valid file specifying a <typeparamref name="T"/> type.</returns>
		public static T? DeserializeJsonFile<T>(this string path) => path.DeserializeJsonFile<T>(JsonSerializationExtensions.SufficientReadWrite);

		/// <summary>
		/// Converts a JSON file specified by <paramref name="path"/> into a <typeparamref name="T"/> type.
		/// </summary>
		/// <typeparam name="T">The type to convert to.</typeparam>
		/// <param name="path">The path to an JSON file specifying a <typeparamref name="T"/> type.</param>
		/// <param name="converter">
		/// The converter to use for the deserialization.
		/// This will be added to the JsonSerializerOptions's set of converters.
		/// This will take precedence over the default <typeparamref name="T"/> type converter but will lose precedence to a property with a custom JsonConverter attribute.
		/// </param>
		/// <remarks>The strength of this method is that it allows for multiple truly independent conversions of a type to occur simultaneously, allowing for converters to contain member state information exclusive to the object being converted.</remarks>
		public static T? DeserializeJsonFile<T>(this string path, JsonConverter<T> converter)
		{
			JsonSerializerOptions ops = new JsonSerializerOptions(JsonSerializationExtensions.SufficientReadWrite);
			ops.Converters.Add(converter);

			return path.DeserializeJsonFile<T>(ops);
		}

		/// <summary>
		/// Converts a JSON file specified by <paramref name="path"/> into a <typeparamref name="T"/> type.
		/// </summary>
		/// <typeparam name="T">The type to convert to.</typeparam>
		/// <param name="path">The path to an JSON file specifying a <typeparamref name="T"/> type.</param>
		/// <param name="converter">
		/// The converter factory to use for the deserialization.
		/// This will be added to the JsonSerializerOptions's set of converters.
		/// This will take precedence over the default <typeparamref name="T"/> type converter but will lose precedence to a property with a custom JsonConverter attribute.
		/// </param>
		/// <remarks>The strength of this method is that it allows for multiple truly independent conversions of a type to occur simultaneously, allowing for converters to contain member state information exclusive to the object being converted.</remarks>
		public static T? DeserializeJsonFile<T>(this string path, JsonConverterFactory converter_factory)
		{
			JsonSerializerOptions ops = new JsonSerializerOptions(JsonSerializationExtensions.SufficientReadWrite);
			ops.Converters.Add(converter_factory);

			return path.DeserializeJsonFile<T>(ops);
		}

		/// <summary>
		/// Converts a JSON file specified by <paramref name="path"/> into a <typeparamref name="T"/> type.
		/// </summary>
		/// <typeparam name="T">The type to convert to.</typeparam>
		/// <param name="path">The path to an JSON file specifying a <typeparamref name="T"/> type.</param>
		/// <param name="ops">The options to pass to the serializer.</param>
		/// <returns>Returns the converted object or null if <paramref name="path"/> was not a valid file specifying a <typeparamref name="T"/> type.</returns>
		public static T? DeserializeJsonFile<T>(this string path, JsonSerializerOptions ops)
		{
			using(FileStream sin = File.OpenRead(path))
				return sin.DeserializeJson<T>(ops);
		}
		#endregion

		#region Strings
		/// <summary>
		/// Converts a JSON string into a <typeparamref name="T"/> type.
		/// </summary>
		/// <typeparam name="T">The type to convert to.</typeparam>
		/// <param name="sjson">The JSON serialization of a <typeparamref name="T"/> type.</param>
		/// <returns>Returns the converted object or null if <paramref name="sjson"/> was not a valid specification of a <typeparamref name="T"/> type.</returns>
		public static T? DeserializeJson<T>(this string sjson) => sjson.DeserializeJson<T>(JsonSerializationExtensions.SufficientReadWrite);

		/// <summary>
		/// Converts a JSON string into a <typeparamref name="T"/> type.
		/// </summary>
		/// <typeparam name="T">The type to convert to.</typeparam>
		/// <param name="sjson">The JSON serialization of a <typeparamref name="T"/> type.</param>
		/// <param name="converter">
		/// The converter to use for the deserialization.
		/// This will be added to the JsonSerializerOptions's set of converters.
		/// This will take precedence over the default <typeparamref name="T"/> type converter but will lose precedence to a property with a custom JsonConverter attribute.
		/// </param>
		/// <remarks>The strength of this method is that it allows for multiple truly independent conversions of a type to occur simultaneously, allowing for converters to contain member state information exclusive to the object being converted.</remarks>
		public static T? DeserializeJson<T>(this string sjson, JsonConverter<T> converter)
		{
			JsonSerializerOptions ops = new JsonSerializerOptions(JsonSerializationExtensions.SufficientReadWrite);
			ops.Converters.Add(converter);

			return sjson.DeserializeJson<T>(ops);
		}

		/// <summary>
		/// Converts a JSON string into a <typeparamref name="T"/> type.
		/// </summary>
		/// <typeparam name="T">The type to convert to.</typeparam>
		/// <param name="sjson">The JSON serialization of a <typeparamref name="T"/> type.</param>
		/// <param name="converter">
		/// The converter factory to use for the deserialization.
		/// This will be added to the JsonSerializerOptions's set of converters.
		/// This will take precedence over the default <typeparamref name="T"/> type converter but will lose precedence to a property with a custom JsonConverter attribute.
		/// </param>
		/// <remarks>The strength of this method is that it allows for multiple truly independent conversions of a type to occur simultaneously, allowing for converters to contain member state information exclusive to the object being converted.</remarks>
		public static T? DeserializeJson<T>(this string sjson, JsonConverterFactory converter_factory)
		{
			JsonSerializerOptions ops = new JsonSerializerOptions(JsonSerializationExtensions.SufficientReadWrite);
			ops.Converters.Add(converter_factory);

			return sjson.DeserializeJson<T>(ops);
		}

		/// <summary>
		/// Converts a JSON string into a <typeparamref name="T"/> type.
		/// </summary>
		/// <typeparam name="T">The type to convert to.</typeparam>
		/// <param name="sjson">The JSON serialization of a <typeparamref name="T"/> type.</param>
		/// <param name="ops">The options to pass to the serializer.</param>
		/// <returns>Returns the converted object or null if <paramref name="sjson"/> was not a valid specification of a <typeparamref name="T"/> type.</returns>
		public static T? DeserializeJson<T>(this string sjson, JsonSerializerOptions ops) => JsonSerializer.Deserialize<T>(sjson,ops);
		#endregion
	}
}
