using GameEngine.Utility.ExtensionMethods.ClassExtensions;
using GameEngine.Utility.ExtensionMethods.PrimitiveExtensions;
using GameEngine.Utility.Serialization;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameEngine.DataStructures.Collections
{
	/// <summary>
	/// A fully serializable wrapper around a dictionary intended to model discrete distributions.
	/// </summary>
	/// <typeparam name="K">The key types.</typeparam>
	/// <typeparam name="V">The value type.</typeparam>
	[JsonConverter(typeof(JsonDistributionConverter))]
	public class Distribution<K,V> : IDictionary<K,V> where K : notnull
	{
		#region Construction
		/// <summary>
		/// Creates an empty distribution.
		/// </summary>
		public Distribution()
		{
			BackingDictionary = new Dictionary<K,V>();
			return;
		}

		/// <summary>
		/// Creates an empty distribution.
		/// </summary>
		/// <param name="capacity">The initial capacity of the backing dictionary.</param>
		public Distribution(int capacity)
		{
			BackingDictionary = new Dictionary<K,V>(capacity);
			return;
		}

		/// <summary>
		/// Creates a distribution initially populated with <paramref name="seed"/>.
		/// </summary>
		public Distribution(IEnumerable<KeyValuePair<K,V>> seed)
		{
			BackingDictionary = new Dictionary<K,V>(seed);
			return;
		}

		/// <summary>
		/// Creates an empty distrubtion.
		/// </summary>
		/// <param name="cmp">The means by which keys are compared for equality.</param>
		public Distribution(IEqualityComparer<K>? cmp)
		{
			BackingDictionary = new Dictionary<K,V>(cmp);
			return;
		}

		/// <summary>
		/// Creates an empty distrubtion.
		/// </summary>
		/// <param name="cmp">The means by which keys are compared for equality.</param>
		public Distribution(int capacity, IEqualityComparer<K>? cmp)
		{
			BackingDictionary = new Dictionary<K,V>(capacity,cmp);
			return;
		}

		/// <summary>
		/// Creates a distribution initially populated with <paramref name="seed"/>.
		/// </summary>
		public Distribution(IDictionary<K,V> seed)
		{
			BackingDictionary = new Dictionary<K,V>(seed);
			return;
		}

		/// <summary>
		/// Creates a distribution initially populated with <paramref name="seed"/>.
		/// </summary>
		/// <param name="cmp">The means by which keys are compared for equality.</param>
		public Distribution(IEnumerable<KeyValuePair<K,V>> seed, IEqualityComparer<K>? cmp)
		{
			BackingDictionary = new Dictionary<K,V>(seed,cmp);
			return;
		}

		/// <summary>
		/// Creates a distribution initially populated with <paramref name="seed"/>.
		/// </summary>
		/// <param name="cmp">The means by which keys are compared for equality.</param>
		public Distribution(IDictionary<K,V> seed, IEqualityComparer<K>? cmp)
		{
			BackingDictionary = new Dictionary<K,V>(seed,cmp);
			return;
		}
		#endregion

		public void Add(K key, V value) => BackingDictionary.Add(key,value);
		public void Add(KeyValuePair<K,V> item) => BackingDictionary.Add(item.Key,item.Value);

		public bool Remove(K key) => BackingDictionary.Remove(key);
		public bool Remove(KeyValuePair<K,V> item) => Contains(item) && BackingDictionary.Remove(item.Key);

		public bool TryGetValue(K key, [MaybeNullWhen(false)] out V value) => BackingDictionary.TryGetValue(key,out value);

		public bool Contains(KeyValuePair<K,V> item) => BackingDictionary.ContainsKey(item.Key) ? item.Value is null ? BackingDictionary[item.Key] is null : item.Value.Equals(BackingDictionary[item.Key]) : false;
		public bool ContainsKey(K key) => BackingDictionary.ContainsKey(key);

		public void Clear() => BackingDictionary.Clear();

		public IEnumerator<KeyValuePair<K,V>> GetEnumerator() => BackingDictionary.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => BackingDictionary.GetEnumerator();

		public void CopyTo(KeyValuePair<K,V>[] array, int start)
		{
			if(array is null)
				throw new ArgumentNullException();

			if(start < 0)
				throw new ArgumentOutOfRangeException();

			if(Count + start > array.Length)
				throw new ArgumentException();

			int i = start;

			foreach(KeyValuePair<K,V> kvp in this)
				array[i++] = kvp;

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
			set => BackingDictionary[key] = value;
		}

		/// <summary>
		/// The backing dictionary.
		/// </summary>
		protected Dictionary<K,V> BackingDictionary
		{get;}

		public ICollection<K> Keys => BackingDictionary.Keys;
		public ICollection<V> Values => BackingDictionary.Values;
		public int Count => BackingDictionary.Count;
		public bool IsReadOnly => false;
	}

	/// <summary>
	/// Creates JSON converters for distributions.
	/// </summary>
	file class JsonDistributionConverter : JsonBaseConverterFactory
	{
		/// <summary>
		/// Constructs the factory.
		/// </summary>
		public JsonDistributionConverter() : base((t,ops) => [],typeof(Distribution<,>),typeof(DC<,>))
		{return;}

		/// <summary>
		/// Performs the JSON conversion for a distribution.
		/// </summary>
		/// <typeparam name="K">The key type.</typeparam>
		/// <typeparam name="V">The value type.</typeparam>
		private class DC<K,V> : JsonConverter<Distribution<K,V>> where K : notnull
		{
			public override Distribution<K,V> Read(ref Utf8JsonReader reader, Type type_to_convert, JsonSerializerOptions ops)
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
				Distribution<K,V> ret = new Distribution<K,V>();

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

			public override void Write(Utf8JsonWriter writer, Distribution<K,V> value, JsonSerializerOptions ops)
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
