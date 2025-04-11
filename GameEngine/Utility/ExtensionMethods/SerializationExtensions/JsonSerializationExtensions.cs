using System.Text.Json;

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
		public static void SerializeJson<T>(this T value, Stream stream) where T : class
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
		public static void SerializeJson<T>(this T value, Stream stream, JsonSerializerOptions ops) where T : class
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
		public static void SerializeJson<T>(this T value, string path) where T : class
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
		public static void SerializeJson<T>(this T value, string path, JsonSerializerOptions ops) where T : class
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
		public static string ToJsonString<T>(this T value) where T : class
		{
			using(MemoryStream sout = new MemoryStream())
			{
				value.SerializeJson(sout);

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
		public static string ToJsonString<T>(this T value, JsonSerializerOptions ops) where T : class
		{
			using(MemoryStream sout = new MemoryStream())
			{
				value.SerializeJson(sout,ops);

				using(StreamReader sin = new StreamReader(sout))
					return sin.ReadToEnd();
			}
		}

		/// <summary>
		/// The JSON options to serialize everything that needs to be serialized to recover the full variable state later.
		/// </summary>
		public static readonly JsonSerializerOptions SufficientReadWrite = new JsonSerializerOptions
		{
			IncludeFields = true,
			IgnoreReadOnlyFields = true,
			IgnoreReadOnlyProperties = true,
			WriteIndented = true
		};
	}
}
