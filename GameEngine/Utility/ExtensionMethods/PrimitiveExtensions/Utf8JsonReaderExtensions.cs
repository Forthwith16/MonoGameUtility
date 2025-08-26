using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShadowDogeEngine.Utilities.ExtensionMethods.StructExtensions
{
	/// <summary>
	/// Extension methods for the Utf8JsonReader struct.
	/// </summary>
	public static class Utf8JsonReaderExtensions
	{
		/// <summary>
		/// Determines if the next token in <paramref name="reader"/> is an array start.
		/// </summary>
		public static bool HasNextArrayStart(this ref Utf8JsonReader reader) => reader.TokenType == JsonTokenType.StartArray;

		/// <summary>
		/// Determines if the next token in <paramref name="reader"/> is an array end.
		/// </summary>
		public static bool HasNextArrayEnd(this ref Utf8JsonReader reader) => reader.TokenType == JsonTokenType.EndArray;

		/// <summary>
		/// Determines if the next token in <paramref name="reader"/> is a boolean.
		/// </summary>
		public static bool HasNextBool(this ref Utf8JsonReader reader) => reader.TokenType == JsonTokenType.True || reader.TokenType == JsonTokenType.False;

		/// <summary>
		/// Determines if the next token in <paramref name="reader"/> is an enum value (to the best of our ability).
		/// </summary>
		public static bool HasNextEnum(this ref Utf8JsonReader reader) => reader.HasNextString();

		/// <summary>
		/// Determines if the next token in <paramref name="reader"/> is a null value.
		/// </summary>
		public static bool HasNextNull(this ref Utf8JsonReader reader) => reader.TokenType == JsonTokenType.Null;

		/// <summary>
		/// Determines if the next token in <paramref name="reader"/> is a number.
		/// </summary>
		public static bool HasNextNumber(this ref Utf8JsonReader reader) => reader.TokenType == JsonTokenType.Number;

		/// <summary>
		/// Determines if the next token in <paramref name="reader"/> is an array start.
		/// </summary>
		public static bool HasNextObjectStart(this ref Utf8JsonReader reader) => reader.TokenType == JsonTokenType.StartObject;

		/// <summary>
		/// Determines if the next token in <paramref name="reader"/> is an array end.
		/// </summary>
		public static bool HasNextObjectEnd(this ref Utf8JsonReader reader) => reader.TokenType == JsonTokenType.EndObject;

		/// <summary>
		/// Determines if the next token in <paramref name="reader"/> is a property.
		/// </summary>
		public static bool HasNextProperty(this ref Utf8JsonReader reader) => reader.TokenType == JsonTokenType.PropertyName;

		/// <summary>
		/// Determines if the next token in <paramref name="reader"/> is a string.
		/// </summary>
		public static bool HasNextString(this ref Utf8JsonReader reader) => reader.TokenType == JsonTokenType.String;

		/// <summary>
		/// Reads an enum value.
		/// </summary>
		/// <typeparam name="T">The enum type to read.</typeparam>
		/// <param name="reader">The JSON reader.</param>
		/// <returns>Returns the read enum value.</returns>
		/// <exception cref="InvalidOperationException">Thrown if reader.GetString() errors.</exception>
		/// <exception cref="JsonException">Thrown if reader.GetString() returns null.</exception>
		public static T ReadEnum<T>(this ref Utf8JsonReader reader) where T : struct, Enum => Enum.Parse<T>(reader.GetString() ?? throw new JsonException());

		/// <summary>
		/// Reads an object written via WriteObject.
		/// </summary>
		public static object? ReadObject(this ref Utf8JsonReader reader, JsonSerializerOptions ops)
		{
			Utf8JsonReader read_ahead = reader;

			// We start with the object opening
			if(!read_ahead.HasNextObjectStart())
				throw new JsonException();
			
			read_ahead.Read();

			// Create the return value
			object? ret = null;
			bool found = false;

			// Loop until we reach the end of the object
			while(!read_ahead.HasNextObjectEnd())
			{
				if(!read_ahead.HasNextProperty())
					throw new JsonException();

				string property_name = read_ahead.GetString()!;
				read_ahead.Read();

				switch(property_name)
				{
				case "Type":
					// Check that we have a type name
					if(!read_ahead.HasNextString())
						throw new JsonException();

					// Figure out what we're trying to make
					Type? to_convert = Type.GetType(read_ahead.GetString()!);
					read_ahead.Read();

					if(to_convert is null)
						throw new JsonException();

					// Grab the converter we need to make the thing
					Type converter_type = typeof(JsonConverter<>);
					converter_type.MakeGenericType(to_convert);

					// Grab the converter
					JsonConverter converter = ops.GetConverter(to_convert);
					
					if(converter is null)
						throw new JsonException(); // Shouldn't happen

					// Now we need to copy reader again and move the copy into position
					read_ahead = reader;
					read_ahead.Read();

					while(!read_ahead.HasNextObjectEnd())
					{
						if(!read_ahead.HasNextProperty())
							throw new JsonException();

						string property_name2 = read_ahead.GetString()!;
						read_ahead.Read();

						if(property_name2 == "Value")
							break;
						else
						{
							read_ahead.Skip();
							read_ahead.Read();
						}
					}

					// We need to wrap our reader into something useful
					Wrapper wrapper = new Wrapper(read_ahead);

					// We need a class wrapper around the wrapper
					ClassWrapper c = new ClassWrapper();

					// We also need to get the Effectuate method from the class wrapper
					MethodInfo? effectuate = typeof(ClassWrapper).GetMethod("Effectuate",BindingFlags.Public | BindingFlags.Instance,new Type[] {typeof(Wrapper).MakeByRefType(),typeof(object),typeof(Type),typeof(JsonSerializerOptions)});
					
					if(effectuate is null)
						throw new JsonException(); // This should also never happen
					
					effectuate = effectuate.MakeGenericMethod(new Type[] {to_convert});

					// Now we can FINALLY run the converter...in a roundabout way
					ret = effectuate.CreateDelegate<WrapperDelegate>(c)(ref wrapper,converter,to_convert,ops);

					found = true;
					break;
				default:
					read_ahead.Skip();
					break;
				}

				// If we found our return value, we can just be done
				if(found)
					break;

				read_ahead.Read();
			}
			
			// Check that we read our return value
			if(!found)
				throw new JsonException();

			// Now we need to advance reader itself past the junk we spat out with WriteObject
			// We start with the object opening
			if(!reader.HasNextObjectStart())
				throw new JsonException();
			
			reader.Read();

			// Loop until we reach the end of the object
			while(!reader.HasNextObjectEnd())
			{
				if(!reader.HasNextProperty())
					throw new JsonException();

				reader.Read();

				reader.Skip();
				reader.Read();
			}

			return ret;
		}

		/// <summary>
		/// Reads an object written via WriteObject.
		/// </summary>
		/// <returns>Returns the object read cast to <typeparamref name="T"/> type.</returns>
		public static T? ReadObject<T>(this ref Utf8JsonReader reader, JsonSerializerOptions ops) => (T?)reader.ReadObject(ops);

		#region Reader Wrapper
		private ref struct Wrapper
		{
			public Wrapper(Utf8JsonReader reader) => Reader = reader;
			public T? Effectuate<T>(JsonConverter<T> converter, Type type_to_convert, JsonSerializerOptions ops) => converter.Read(ref Reader,type_to_convert,ops);
			public Utf8JsonReader Reader;
		}

		private class ClassWrapper
		{public object? Effectuate<T>(ref Wrapper wrapper, object converter, Type type_to_convert, JsonSerializerOptions ops) => wrapper.Effectuate<T>((JsonConverter<T>)converter,type_to_convert,ops);}

		private delegate object? WrapperDelegate(ref Wrapper wrapper, object converter, Type type_to_convert, JsonSerializerOptions ops);
		#endregion
	}
}
