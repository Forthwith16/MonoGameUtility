using GameEngine.Utility.ExtensionMethods.ClassExtensions;
using GameEngine.Utility.ExtensionMethods.PrimitiveExtensions;
using GameEngine.Utility.Serialization;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameEngine.DataStructures.Absorbing
{
	/// <summary>
	/// A dictionary that can only be added to, not removed from.
	/// </summary>
	/// <typeparam name="T">The type to store in the dictionary.</typeparam>
	[JsonConverter(typeof(JsonAbsorbingDictionaryConverter))]
	public class AbsorbingDictionary<K,V> : IDictionary<K,V>, IReadOnlyDictionary<K,V> where K : notnull
	{
		/// <summary>
		/// Creates an empty absorbing dictionary.
		/// </summary>
		public AbsorbingDictionary()
		{
			BackingDictionary = new Dictionary<K,V>();
			return;
		}

		/// <summary>
		/// Creates an absorbing dictionary initially populated with <paramref name="seed"/>.
		/// </summary>
		/// <param name="seed">The initial contents of this absorbing dictionary.</param>
		/// <exception cref="ArgumentException">Thrown if <paramref name="seed"/> contains duplicate keys.</exception>
		public AbsorbingDictionary(IEnumerable<KeyValuePair<K,V>> seed)
		{
			BackingDictionary = new Dictionary<K,V>(seed);
			return;
		}

		public void Add(K key, V value)
		{
			BackingDictionary.Add(key,value);
			return;
		}

		public void Add(KeyValuePair<K,V> item)
		{
			BackingDictionary.Add(item.Key,item.Value);
			return;
		}

		public void Clear()
		{throw new NotSupportedException();}

		public bool Contains(KeyValuePair<K,V> item)
		{return BackingDictionary.Contains(item);}

		public bool ContainsKey(K key)
		{return BackingDictionary.ContainsKey(key);}

		public bool Remove(K key)
		{throw new NotSupportedException();}

		public bool Remove(KeyValuePair<K,V> item)
		{throw new NotSupportedException();}

		public bool TryGetValue(K key, [MaybeNullWhen(false)] out V value)
		{return BackingDictionary.TryGetValue(key,out value);}

		public IEnumerator<KeyValuePair<K,V>> GetEnumerator()
		{return BackingDictionary.GetEnumerator();}

		IEnumerator IEnumerable.GetEnumerator()
		{return BackingDictionary.GetEnumerator();}

		public void CopyTo(KeyValuePair<K,V>[] array, int arrayIndex)
		{
			(BackingDictionary as ICollection<KeyValuePair<K,V>>).CopyTo(array,arrayIndex);
			return;
		}

		public override string ToString()
		{
			StringBuilder ret = new StringBuilder("{");

			foreach(KeyValuePair<K,V> kvp in this)
				ret.Append(kvp.Key + " -> " + kvp.Value + ",");

			ret.Remove(ret.Length - 1, 1);
			ret.Append('}');
			
			return ret.ToString();
		}

		public V this[K key]
		{
			get => BackingDictionary[key];

			set
			{
				if(ContainsKey(key))
					throw new NotSupportedException();

				BackingDictionary[key] = value;
				return;
			}
		}

		/// <summary>
		/// The backing data structure of this AbsorbingDictionary.
		/// </summary>
		protected Dictionary<K,V> BackingDictionary
		{get; set;}

		public int Count => BackingDictionary.Count;

		public ICollection<K> Keys => BackingDictionary.Keys;
		IEnumerable<K> IReadOnlyDictionary<K,V>.Keys => BackingDictionary.Keys;

		public ICollection<V> Values => BackingDictionary.Values;
		IEnumerable<V> IReadOnlyDictionary<K,V>.Values => BackingDictionary.Values;

		public bool IsReadOnly => false;
	}

	/// <summary>
	/// Creates JSON converters for absorbing dictionaries.
	/// </summary>
	public class JsonAbsorbingDictionaryConverter : JsonBaseConverterFactory
	{
		/// <summary>
		/// Constructs the factory.
		/// </summary>
		public JsonAbsorbingDictionaryConverter() : base((t,ops) => [],typeof(AbsorbingDictionary<,>),typeof(ADC<,>))
		{return;}

		/// <summary>
		/// Performs the JSON conversion for an absorbing dictionary.
		/// </summary>
		/// <typeparam name="K">The key type.</typeparam>
		/// <typeparam name="V">The value type.</typeparam>
		private class ADC<K,V> : JsonConverter<AbsorbingDictionary<K,V>> where K : notnull
		{
			public override AbsorbingDictionary<K,V> Read(ref Utf8JsonReader reader, Type type_to_convert, JsonSerializerOptions ops)
			{
				// We start with the object opening
				if(!reader.HasNextObjectStart())
					throw new JsonException();
				
				reader.Read();

				// We only have one property, so we better get it right away
				if(!reader.HasNextProperty() || reader.GetString()! != "Items")
					throw new JsonException();

				reader.Read();

				// We need an array opener
				if(!reader.HasNextArrayStart())
					throw new JsonException();
				
				reader.Read();

				// Read the array until we reach the end
				AbsorbingDictionary<K,V> ret = new AbsorbingDictionary<K,V>();

				while(!reader.HasNextArrayEnd())
				{
					ret.Add(ReadKVP(ref reader,ops));
					reader.Read();
				}
				
				// Clean up the array end
				reader.Read();

				// Make sure we're done
				if(!reader.HasNextObjectEnd())
					throw new JsonException();

				return ret;
			}

			protected KeyValuePair<K,V> ReadKVP(ref Utf8JsonReader reader, JsonSerializerOptions ops)
			{
				// We start with the object opening
				if(!reader.HasNextObjectStart())
					throw new JsonException();
			
				reader.Read();

				// We'll need to track what properties we've already done, though this is not strictly necessary
				HashSet<string> processed = new HashSet<string>();

				// Create a place to store the stuff we read
				K? key = default;
				V? value = default;
			
				// Loop until we reach the end of the object
				while(!reader.HasNextObjectEnd())
				{
					if(!reader.HasNextProperty())
						throw new JsonException();

					string property_name = reader.GetString()!;
					reader.Read();

					if(processed.Contains(property_name))
						throw new JsonException();

					switch(property_name)
					{
					case "Key":
						key = reader.ReadObject<K>(ops) ?? throw new JsonException();
						break;
					case "Value":
						value = reader.ReadObject<V>(ops); // value can be null in theory
						break;
					default:
						throw new JsonException();
					}

					processed.Add(property_name);
					reader.Read();
				}

				// Make sure we got the properties we absolutely must have
				if(processed.Count != 2)
					throw new JsonException();

				return new KeyValuePair<K,V>(key!,value!); // These values are assigned, so they're fine
			}

			public override void Write(Utf8JsonWriter writer, AbsorbingDictionary<K,V> value, JsonSerializerOptions ops)
			{
				writer.WriteStartObject();
				writer.WriteStartArray("Items");

				foreach(KeyValuePair<K,V> kvp in value)
				{
					writer.WriteStartObject();

					writer.WriteObject("Key",kvp.Key,ops);
					writer.WriteObject("Value",kvp.Value,ops);

					writer.WriteEndObject();
				}

				writer.WriteEndArray();
				writer.WriteEndObject();

				return;
			}
		}
	}
}
