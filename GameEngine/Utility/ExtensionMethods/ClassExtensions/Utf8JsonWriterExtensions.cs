using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameEngine.Utility.ExtensionMethods.ClassExtensions
{
	/// <summary>
	/// Extensions for the JSON writer.
	/// </summary>
	public static class Utf8JsonWriterExtensions
	{
		/// <summary>
		/// Writes an enum.
		/// </summary>
		public static void WriteEnum<T>(this Utf8JsonWriter writer, T value) where T : Enum => writer.WriteStringValue(value.ToString());

		/// <summary>
		/// Writes an enum.
		/// </summary>
		public static void WriteEnum<T>(this Utf8JsonWriter writer, string property_name, T value) where T : Enum => writer.WriteString(property_name,value.ToString());

		/// <summary>
		/// Writes a generic object.
		/// It will use the JsonConverter for the type of <paramref name="value"/> located in <paramref name="ops"/> for this.
		/// </summary>
		public static void WriteObject(this Utf8JsonWriter writer, object? value, JsonSerializerOptions ops)
		{
			if(value is null)
			{
				writer.WriteStartObject();
			
				writer.WriteNull("Type");
				writer.WriteNull("Value");
				
				writer.WriteEndObject();

				return;
			}

			Type to_convert = value.GetType();

			JsonConverter Converter = ops.GetConverter(to_convert);
			MethodInfo? m = Converter.GetType().GetMethod("Write",BindingFlags.Public | BindingFlags.Instance,new Type[] {typeof(Utf8JsonWriter),to_convert,typeof(JsonSerializerOptions)});

			if(m is null)
				throw new JsonException();

			writer.WriteStartObject();
			
			writer.WriteString("Type",to_convert.AssemblyQualifiedName);
			
			writer.WritePropertyName("Value");
			m.Invoke(Converter,new object?[] {writer,value,ops});
			
			writer.WriteEndObject();

			return;
		}

		/// <summary>
		/// Writes a generic object.
		/// It will use the JsonConverter for the type of <paramref name="value"/> located in <paramref name="ops"/> for this.
		/// </summary>
		public static void WriteObject(this Utf8JsonWriter writer, string property_name, object? value, JsonSerializerOptions ops)
		{
			writer.WritePropertyName(property_name);
			writer.WriteObject(value,ops);

			return;
		}
	}
}
