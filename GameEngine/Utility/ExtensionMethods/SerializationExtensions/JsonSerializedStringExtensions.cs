using System.Text;
using System.Text.Json;

namespace GameEngine.Utility.ExtensionMethods.SerializationExtensions
{
	/// <summary>
	/// Provides JSON related extensions to strings.
	/// </summary>
	public static class JsonSerializedStringExtensions
	{
		/// <summary>
		/// Converts a JSON string into a <typeparamref name="T"/> type.
		/// </summary>
		/// <typeparam name="T">The type to convert to.</typeparam>
		/// <param name="sjson">The JSON specification of a <typeparamref name="T"/> type.</param>
		/// <returns>Returns the converted object or null if <paramref name="sxml"/> was not a valid specification of a <typeparamref name="T"/> type.</returns>
		public static T? DeserializeJson<T>(this string sjson)
		{
			byte[] bytes = Encoding.GetEncoding(sjson).GetBytes(sjson);

			using(MemoryStream sin = new MemoryStream(bytes))
				return (T?)JsonSerializer.Deserialize(sin,typeof(T),JsonSerializationExtensions.SufficientReadWrite);
		}

		/// <summary>
		/// Converts a JSON string into a <typeparamref name="T"/> type.
		/// </summary>
		/// <typeparam name="T">The type to convert to.</typeparam>
		/// <param name="sjson">The JSON specification of a <typeparamref name="T"/> type.</param>
		/// <param name="ops">The options ot pass to the serializer.</param>
		/// <returns>Returns the converted object or null if <paramref name="sxml"/> was not a valid specification of a <typeparamref name="T"/> type.</returns>
		public static T? DeserializeJson<T>(this string sjson, JsonSerializerOptions ops)
		{
			byte[] bytes = Encoding.GetEncoding(sjson).GetBytes(sjson);

			using(MemoryStream sin = new MemoryStream(bytes))
				return (T?)JsonSerializer.Deserialize(sin,typeof(T),ops);
		}

		/// <summary>
		/// Converts a JSON file specified by <paramref name="path"/> into a <typeparamref name="T"/> type.
		/// </summary>
		/// <typeparam name="T">The type to convert to.</typeparam>
		/// <param name="path">The path to an JSON file specifying a <typeparamref name="T"/> type.</param>
		/// <returns>Returns the converted object or null if <paramref name="path"/> was not a valid file specifying a <typeparamref name="T"/> type.</returns>
		public static T? DeserializeJsonFile<T>(this string path)
		{
			using(FileStream sin = File.OpenRead(path))
				return (T?)JsonSerializer.Deserialize(sin,typeof(T),JsonSerializationExtensions.SufficientReadWrite);
		}

		/// <summary>
		/// Converts a JSON file specified by <paramref name="path"/> into a <typeparamref name="T"/> type.
		/// </summary>
		/// <typeparam name="T">The type to convert to.</typeparam>
		/// <param name="path">The path to an JSON file specifying a <typeparamref name="T"/> type.</param>
		/// <param name="ops">The options ot pass to the serializer.</param>
		/// <returns>Returns the converted object or null if <paramref name="path"/> was not a valid file specifying a <typeparamref name="T"/> type.</returns>
		public static T? DeserializeJsonFile<T>(this string path, JsonSerializerOptions ops)
		{
			using(FileStream sin = File.OpenRead(path))
				return (T?)JsonSerializer.Deserialize(sin,typeof(T),ops);
		}
	}
}
