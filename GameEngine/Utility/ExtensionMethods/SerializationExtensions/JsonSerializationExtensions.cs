using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameEngine.Utility.ExtensionMethods.SerializationExtensions
{
	/// <summary>
	/// Adds extension methods to JSON serialization.
	/// </summary>
	public static class JsonSerializationExtensions
	{
		/// <summary>
		/// Serializes an object to a <paramref name="stream"/> in JSON format.
		/// </summary>
		/// <typeparam name="T">The type to serialize.</typeparam>
		/// <param name="value">The value to serialize.</param>
		/// <param name="stream">The stream to write to.</param>
		public static void SerializeJson<T>(this T value, Stream stream)
		{
			JsonSerializer.Serialize(stream,value,typeof(T),SufficientReadWrite);
			return;
		}

		/// <summary>
		/// Serializes an object to a <paramref name="stream"/> in JSON format.
		/// </summary>
		/// <typeparam name="T">The type to serialize.</typeparam>
		/// <param name="value">The value to serialize.</param>
		/// <param name="stream">The stream to write to.</param>
		/// <param name="ops">The options ot pass to the serializer.</param>
		public static void SerializeJson<T>(this T value, Stream stream, JsonSerializerOptions ops)
		{
			JsonSerializer.Serialize(stream,value,typeof(T),ops);
			return;
		}

		/// <summary>
		/// Serializes an object to a <paramref name="stream"/> in JSON format.
		/// </summary>
		/// <typeparam name="T">The type to serialize.</typeparam>
		/// <param name="value">The value to serialize.</param>
		/// <param name="path">The file to write to.</param>
		public static void SerializeJson<T>(this T value, string path)
		{
			using(FileStream fout = File.Create(path))
				value.SerializeJson(fout);

			return;
		}

		/// <summary>
		/// Serializes an object to a <paramref name="stream"/> in JSON format.
		/// </summary>
		/// <typeparam name="T">The type to serialize.</typeparam>
		/// <param name="value">The value to serialize.</param>
		/// <param name="path">The file to write to.</param>
		/// <param name="ops">The options ot pass to the serializer.</param>
		public static void SerializeJson<T>(this T value, string path, JsonSerializerOptions ops)
		{
			using(FileStream fout = File.Create(path))
				value.SerializeJson(fout,ops);

			return;
		}

		/// <summary>
		/// Serializes an object to a string in JSON format.
		/// </summary>
		/// <typeparam name="T">The type to serialize.</typeparam>
		/// <param name="value">The value to serialize.</param>
		public static string ToJsonString<T>(this T value)
		{
			using(MemoryStream sout = new MemoryStream())
			{
				value.SerializeJson(sout);
				sout.Position = 0;

				using(StreamReader sin = new StreamReader(sout))
					return sin.ReadToEnd();
			}
		}

		/// <summary>
		/// Serializes an object to a string in JSON format.
		/// </summary>
		/// <typeparam name="T">The type to serialize.</typeparam>
		/// <param name="value">The value to serialize.</param>
		/// <param name="ops">The options ot pass to the serializer.</param>
		public static string ToJsonString<T>(this T value, JsonSerializerOptions ops)
		{
			using(MemoryStream sout = new MemoryStream())
			{
				value.SerializeJson(sout,ops);
				sout.Position = 0;
				
				using(StreamReader sin = new StreamReader(sout))
					return sin.ReadToEnd();
			}
		}

		/// <summary>
		/// Adds a converter to the default set of JSON options.
		/// </summary>
		/// <param name="c">The converter to add.</param>
		public static void RegisterConverter(JsonConverter c)
		{
			SufficientReadWrite = new JsonSerializerOptions(SufficientReadWrite); // This makes it so we can change options even after having serialized things
			SufficientReadWrite.Converters.Add(c);
			
			return;
		}

		/// <summary>
		/// Removes a converter from the default set of JSON options.
		/// </summary>
		/// <param name="c">The converter to remove.</param>
		public static void UnregisterConverter(JsonConverter c)
		{
			SufficientReadWrite = new JsonSerializerOptions(SufficientReadWrite); // This makes it so we can change options even after having serialized things
			SufficientReadWrite.Converters.Remove(c);
			
			return;
		}

		/// <summary>
		/// The JSON options to serialize everything that needs to be serialized to recover the full variable state later.
		/// </summary>
		internal static JsonSerializerOptions SufficientReadWrite = new JsonSerializerOptions
		{
			IncludeFields = true,
			IgnoreReadOnlyFields = true,
			IgnoreReadOnlyProperties = true,
			WriteIndented = true
		};
	}
}
