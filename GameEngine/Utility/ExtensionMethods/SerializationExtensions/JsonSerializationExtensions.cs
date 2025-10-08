using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameEngine.Utility.ExtensionMethods.SerializationExtensions
{
	/// <summary>
	/// Adds extension methods to JSON serialization.
	/// </summary>
	public static class JsonSerializationExtensions
	{
		#region Streams
		/// <summary>
		/// Serializes an object to a <paramref name="stream"/> in JSON format.
		/// </summary>
		/// <typeparam name="T">The type to serialize.</typeparam>
		/// <param name="value">The value to serialize.</param>
		/// <param name="stream">The stream to write to.</param>
		public static void SerializeJson<T>(this T value, Stream stream) => value.SerializeJson(stream,SufficientReadWrite);

		/// <summary>
		/// Serializes an object to a <paramref name="stream"/> in JSON format.
		/// </summary>
		/// <typeparam name="T">The type to serialize.</typeparam>
		/// <param name="value">The value to serialize.</param>
		/// <param name="stream">The stream to write to.</param>
		/// <param name="converter">
		/// The converter to use for the serialization.
		/// This will be added to the JsonSerializerOptions's set of converters.
		/// This will take precedence over the default <typeparamref name="T"/> type converter but will lose precedence to a property with a custom JsonConverter attribute.
		/// </param>
		/// <remarks>The strength of this method is that it allows for multiple truly independent conversions of a type to occur simultaneously, allowing for converters to contain member state information exclusive to the object being converted.</remarks>
		public static void SerializeJson<T>(this T value, Stream stream, JsonConverter<T> converter)
		{
			JsonSerializerOptions ops = new JsonSerializerOptions(SufficientReadWrite);
			ops.Converters.Add(converter);

			value.SerializeJson(stream,ops);
			return;
		}

		/// <summary>
		/// Serializes an object to a <paramref name="stream"/> in JSON format.
		/// </summary>
		/// <typeparam name="T">The type to serialize.</typeparam>
		/// <param name="value">The value to serialize.</param>
		/// <param name="stream">The stream to write to.</param>
		/// <param name="converter">
		/// The converter factory to use for the serialization.
		/// This will be added to the JsonSerializerOptions's set of converters.
		/// This will take precedence over the default <typeparamref name="T"/> type converter but will lose precedence to a property with a custom JsonConverter attribute.
		/// </param>
		/// <remarks>The strength of this method is that it allows for multiple truly independent conversions of a type to occur simultaneously, allowing for converters to contain member state information exclusive to the object being converted.</remarks>
		public static void SerializeJson<T>(this T value, Stream stream, JsonConverterFactory converter_factory)
		{
			JsonSerializerOptions ops = new JsonSerializerOptions(SufficientReadWrite);
			ops.Converters.Add(converter_factory);

			value.SerializeJson(stream,ops);
			return;
		}

		/// <summary>
		/// Serializes an object to a <paramref name="stream"/> in JSON format.
		/// </summary>
		/// <typeparam name="T">The type to serialize.</typeparam>
		/// <param name="value">The value to serialize.</param>
		/// <param name="stream">The stream to write to.</param>
		/// <param name="ops">The options to pass to the serializer.</param>
		public static void SerializeJson<T>(this T value, Stream stream, JsonSerializerOptions ops) => JsonSerializer.Serialize(stream,value,typeof(T),ops);
		#endregion

		#region Files
		/// <summary>
		/// Serializes an object to a file at <paramref name="path"/> in JSON format.
		/// </summary>
		/// <typeparam name="T">The type to serialize.</typeparam>
		/// <param name="value">The value to serialize.</param>
		/// <param name="path">The file to write to.</param>
		public static void SerializeJson<T>(this T value, string path) => value.SerializeJson(path,SufficientReadWrite);

		/// <summary>
		/// Serializes an object to a file at <paramref name="path"/> in JSON format.
		/// </summary>
		/// <typeparam name="T">The type to serialize.</typeparam>
		/// <param name="value">The value to serialize.</param>
		/// <param name="path">The file to write to.</param>
		/// <param name="converter">
		/// The converter to use for the serialization.
		/// This will be added to the JsonSerializerOptions's set of converters.
		/// This will take precedence over the default <typeparamref name="T"/> type converter but will lose precedence to a property with a custom JsonConverter attribute.
		/// </param>
		/// <remarks>The strength of this method is that it allows for multiple truly independent conversions of a type to occur simultaneously, allowing for converters to contain member state information exclusive to the object being converted.</remarks>
		public static void SerializeJson<T>(this T value, string path, JsonConverter<T> converter)
		{
			JsonSerializerOptions ops = new JsonSerializerOptions(SufficientReadWrite);
			ops.Converters.Add(converter);

			value.SerializeJson(path,ops);
			return;
		}

		/// <summary>
		/// Serializes an object to a file at <paramref name="path"/> in JSON format.
		/// </summary>
		/// <typeparam name="T">The type to serialize.</typeparam>
		/// <param name="value">The value to serialize.</param>
		/// <param name="path">The file to write to.</param>
		/// <param name="converter">
		/// The converter factory to use for the serialization.
		/// This will be added to the JsonSerializerOptions's set of converters.
		/// This will take precedence over the default <typeparamref name="T"/> type converter but will lose precedence to a property with a custom JsonConverter attribute.
		/// </param>
		/// <remarks>The strength of this method is that it allows for multiple truly independent conversions of a type to occur simultaneously, allowing for converters to contain member state information exclusive to the object being converted.</remarks>
		public static void SerializeJson<T>(this T value, string path, JsonConverterFactory converter_factory)
		{
			JsonSerializerOptions ops = new JsonSerializerOptions(SufficientReadWrite);
			ops.Converters.Add(converter_factory);

			value.SerializeJson(path,ops);
			return;
		}

		/// <summary>
		/// Serializes an object to a file at <paramref name="path"/> in JSON format.
		/// </summary>
		/// <typeparam name="T">The type to serialize.</typeparam>
		/// <param name="value">The value to serialize.</param>
		/// <param name="path">The file to write to.</param>
		/// <param name="ops">The options to pass to the serializer.</param>
		public static void SerializeJson<T>(this T value, string path, JsonSerializerOptions ops)
		{
			using(FileStream fout = File.Create(path))
				value.SerializeJson(fout,ops);

			return;
		}
		#endregion

		#region Strings
		/// <summary>
		/// Serializes an object to a string in JSON format.
		/// </summary>
		/// <typeparam name="T">The type to serialize.</typeparam>
		/// <param name="value">The value to serialize.</param>
		public static string ToJsonString<T>(this T value) => value.ToJsonString(SufficientReadWrite);

		/// <summary>
		/// Serializes an object to a string in JSON format.
		/// </summary>
		/// <typeparam name="T">The type to serialize.</typeparam>
		/// <param name="value">The value to serialize.</param>
		/// <param name="converter">
		/// The converter to use for the serialization.
		/// This will be added to the JsonSerializerOptions's set of converters.
		/// This will take precedence over the default <typeparamref name="T"/> type converter but will lose precedence to a property with a custom JsonConverter attribute.
		/// </param>
		/// <returns>Returns the resulting JSON string.</returns>
		/// <remarks>The strength of this method is that it allows for multiple truly independent conversions of a type to occur simultaneously, allowing for converters to contain member state information exclusive to the object being converted.</remarks>
		public static string ToJsonString<T>(this T value, JsonConverter<T> converter)
		{
			JsonSerializerOptions ops = new JsonSerializerOptions(SufficientReadWrite);
			ops.Converters.Add(converter);

			return value.ToJsonString(ops);
		}

		/// <summary>
		/// Serializes an object to a string in JSON format.
		/// </summary>
		/// <typeparam name="T">The type to serialize.</typeparam>
		/// <param name="value">The value to serialize.</param>
		/// <param name="converter">
		/// The converter factory to use for the serialization.
		/// This will be added to the JsonSerializerOptions's set of converters.
		/// This will take precedence over the default <typeparamref name="T"/> type converter but will lose precedence to a property with a custom JsonConverter attribute.
		/// </param>
		/// <returns>Returns the resulting JSON string.</returns>
		/// <remarks>The strength of this method is that it allows for multiple truly independent conversions of a type to occur simultaneously, allowing for converters to contain member state information exclusive to the object being converted.</remarks>
		public static string ToJsonString<T>(this T value, JsonConverterFactory converter_factory)
		{
			JsonSerializerOptions ops = new JsonSerializerOptions(SufficientReadWrite);
			ops.Converters.Add(converter_factory);

			return value.ToJsonString(ops);
		}

		/// <summary>
		/// Serializes an object to a string in JSON format.
		/// </summary>
		/// <typeparam name="T">The type to serialize.</typeparam>
		/// <param name="value">The value to serialize.</param>
		/// <param name="ops">The options to pass to the serializer.</param>
		public static string ToJsonString<T>(this T value, JsonSerializerOptions ops) => JsonSerializer.Serialize(value,typeof(T),ops);
		#endregion

		/// <summary>
		/// The basic JSON serialization/deserialization options.
		/// This will pretty print and ignore readonly fields and properties by default, which cannot be assigned after construction time.
		/// </summary>
		internal static JsonSerializerOptions SufficientReadWrite
		{get;} = new JsonSerializerOptions
		{
			IncludeFields = true,
			IgnoreReadOnlyFields = true,
			IgnoreReadOnlyProperties = true,
			WriteIndented = true
		};
	}
}
