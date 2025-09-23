using GameEngine.DataStructures.Collections;
using GameEngine.DataStructures.Utility;
using GameEngine.Utility.ExtensionMethods.PrimitiveExtensions;
using GameEngine.Utility.Serialization;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameEngine.DataStructures.Maps
{
	/// <summary>
	/// A bijective dictionary.
	/// It maps keys to values and maps values back to keys.
	/// <para/>
	/// Each key and value must be unique.
	/// </summary>
	/// <typeparam name="K">The key type.</typeparam>
	/// <typeparam name="V">The value type.</typeparam>
	[JsonConverter(typeof(JsonBijectionConverter))]
	public class Bijection<K,V> : IEnumerable<KeyValuePair<K,V>>, IDictionary<K,V> where K : notnull where V : notnull
	{
		/// <summary>
		/// Creates an empty bijection.
		/// </summary>
		public Bijection()
		{
			Map = new Dictionary<K,V>();
			InverseMap = new Dictionary<V,K>();
			
			return;
		}

		/// <summary>
		/// Used to construct an inverse bijection.
		/// </summary>
		/// <param name="map">The forward map.</param>
		/// <param name="inverseMap">The inverse map.</param>
		/// <remarks>The parameters are not deep copied, so the values in them are kept in sync with the forward bijection.</remarks>
		protected Bijection(Dictionary<K,V> map, Dictionary<V,K> inverseMap)
		{
			Map = map;
			InverseMap = inverseMap;

			return;
		}

		/// <summary>
		/// Adds a mapping of <paramref name="key"/> to <paramref name="value"/>.
		/// This fails if <paramref name="key"/> is already in the domain or <paramref name="value"/> is already in the image.
		/// </summary>
		/// <param name="key">The new input to add.</param>
		/// <param name="value">The new output to map <paramref name="key"/> to.</param>
		/// <exception cref="ArgumentException">Thrown if <paramref name="key"/> already exists in the domain or if <paramref name="value"/> already exists in the image.</exception>
		public void Add(K key, V value)
		{
			Map.Add(key,value);
			InverseMap.Add(value,key);
			
			return;
		}

		/// <summary>
		/// Adds a mapping to this bijection.
		/// This fails if the input is already in the domain or the output is already in the image.
		/// </summary>
		/// <param name="item">The mapping to add.</param>
		/// <exception cref="ArgumentException">Thrown if the key already exists in the domain or if value already exists in the image.</exception>
		public void Add(KeyValuePair<K,V> item) => Add(item.Key,item.Value);

		/// <summary>
		/// Attempts to obtain the output of <paramref name="key"/> and places it into <paramref name="value"/>.
		/// </summary>
		/// <param name="key">The value to obtain f(<paramref name="key"/>) of.</param>
		/// <param name="value">The place to store the output f(<paramref name="key"/>). This value will default if no such output exists.</param>
		/// <returns>Returns true if <paramref name="key"/> had an output and false otherwise.</returns>
		public bool TryGetValue(K key, [MaybeNullWhen(false)] out V value)
		{return Map.TryGetValue(key,out value);}

		/// <summary>
		/// Computes the inverse value of <paramref name="value"/>.
		/// </summary>
		/// <param name="value">The value to invert.</param>
		/// <returns>Returns f^-1(<paramref name="value"/>).</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
		/// <exception cref="KeyNotFoundException">Thrown if <paramref name="value"/> is not the output of some key.</exception>
		public K Invert(V value)
		{return InverseMap[value];}

		/// <summary>
		/// Attempts to obtain the inverse value of <paramref name="value"/> and places it into <paramref name="result"/>.
		/// </summary>
		/// <param name="value">The value to invert.</param>
		/// <param name="result">The place to store the inverted output. This value will default if no such inverse exists.</param>
		/// <returns>Returns true if <paramref name="value"/> could be inverted and false otherwise.</returns>
		public bool TryInvert(V value, [MaybeNullWhen(false)] out K result)
		{return InverseMap.TryGetValue(value,out result);}

		public bool Remove(K key)
		{
			if(!Map.TryGetValue(key,out V? value))
				return false;

			return Map.Remove(key) && InverseMap.Remove(value);
		}

		/// <summary>
		/// Removes the input/output pair with output <paramref name="value"/>.
		/// </summary>
		/// <param name="value">The value to remove from the bijection.</param>
		/// <returns>Returns true if the mapping was changed and false otherwise.</returns>
		public bool RemoveByValue(V value)
		{
			if(!InverseMap.TryGetValue(value,out K? key))
				return false;
			
			return Map.Remove(key) && InverseMap.Remove(value);
		}

		public bool Remove(KeyValuePair<K,V> item)
		{return Map.Remove(item.Key) && InverseMap.Remove(item.Value);}

		public bool ContainsKey(K key) => Map.ContainsKey(key);
		public bool ContainsValue(V value) => InverseMap.ContainsKey(value);
		public bool Contains(KeyValuePair<K,V> item) => Map.Contains(item);

		public void Clear()
		{
			Map.Clear();
			InverseMap.Clear();

			return;
		}

		public void CopyTo(KeyValuePair<K,V>[] array, int arrayIndex)
		{
			ArgumentNullException.ThrowIfNull(array);
			ArgumentOutOfRangeException.ThrowIfNegative(arrayIndex);

			if(Count > 0 && arrayIndex + Count > array.Length)
				throw new ArgumentException();

			int i = arrayIndex;

			foreach(KeyValuePair<K,V> p in Map)
				array[i++] = p;

			return;
		}

		public IEnumerator<KeyValuePair<K,V>> GetEnumerator() => Map.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => Map.GetEnumerator();

		public override string ToString()
		{
			string ret = "{";

			foreach(KeyValuePair<K,V> p in Map)
				ret += p.Key + "->" + p.Value + ",";
			
			return ret.Substring(0,ret.Length - 1) + "}";
		}

		/// <summary>
		/// Sets or obtains the output of f(<paramref name="key"/>).
		/// When setting, this will replace already extant values.
		/// </summary>
		/// <param name="key">The input value.</param>
		/// <returns>Returns the output when <paramref name="key"/> is provided as input.</returns>
		/// <exception cref="ArgumentException">Thrown if replacing a value that could not be removed for some reason.</exception>
		/// <exception cref="KeyNotFoundException">Thrown in a get operation if <paramref name="key"/> does not belong to the domain.</exception>
		public V this[K key]
		{
			get => Map[key];
			
			set
			{
				// If value is already in the bijection, then we should throw an error if this isn't the identity assignment
				if(InverseMap.ContainsKey(value))
					if(Map.TryGetValue(key,out V? v) && v.Equals(value))
						return;
					else if(!Remove(key))
						throw new ArgumentException();

				Map[key] = value;
				InverseMap[value] = key;

				return;
			}
		}

		/// <summary>
		/// The inverse bijection.
		/// <para/>
		/// The resulting bijection will be kept in sync with this forward bijection.
		/// Changes to one will make changes to the other.
		/// </summary>
		public Bijection<V,K> Inverse => new Bijection<V,K>(InverseMap,Map);

		/// <summary>
		/// The map.
		/// </summary>
		protected Dictionary<K,V> Map
		{get;}

		/// <summary>
		/// The inverse map.
		/// </summary>
		protected Dictionary<V,K> InverseMap
		{get;}

		/// <summary>
		/// The number of mappings in this bijection.
		/// </summary>
		public int Count => Map.Count;

		/// <summary>
		/// The domain values.
		/// </summary>
		public ICollection<K> Domain => Map.Keys;

		/// <summary>
		/// The image values.
		/// </summary>
		public ICollection<V> Image => Map.Values;

		// Dictionary properties that need no documentation
		public ICollection<K> Keys => Map.Keys;
		public ICollection<V> Values => Map.Values;
		public bool IsReadOnly => false;
	}

	/// <summary>
	/// Creates JSON converters for key value pairs.
	/// </summary>
	public class JsonBijectionConverter : JsonBaseConverterFactory
	{
		/// <summary>
		/// Constructs the factory.
		/// </summary>
		public JsonBijectionConverter() : base((t,ops) => [ops],typeof(Bijection<,>),typeof(BC<,>))
		{return;}

		/// <summary>
		/// Performs the JSON conversion for a key value pair.
		/// </summary>
		/// <typeparam name="K">The key type.</typeparam>
		/// <typeparam name="V">The value type.</typeparam>
		private class BC<K,V> : JsonBaseConverter<Bijection<K,V>> where K : notnull where V : notnull
		{
			public BC(JsonSerializerOptions ops)
			{
				KVPConverter = (JsonConverter<SecretKeyValuePair<K,V>>)ops.GetConverter(typeof(SecretKeyValuePair<K,V>));
				return;
			}

			protected override object? ReadProperty(ref Utf8JsonReader reader, string property, JsonSerializerOptions ops)
			{
				// We only have the one property
				if(property != "Items")
					throw new JsonException();

				// We need an array opener
				if(!reader.HasNextArrayStart())
					throw new JsonException();
				
				reader.Read();

				// Read the array until we reach the end
				IndexedQueue<SecretKeyValuePair<K,V>> ret = new IndexedQueue<SecretKeyValuePair<K,V>>();

				while(!reader.HasNextArrayEnd())
				{
					ret.Enqueue(KVPConverter.Read(ref reader,typeof(SecretKeyValuePair<K,V>),ops)!);
					reader.Read();
				}

				return ret;
			}

			protected override Bijection<K,V> ConstructT(Dictionary<string,object?> properties)
			{
				if(properties.Count != 1)
					throw new JsonException();

				Bijection<K,V> ret = new Bijection<K,V>();

				foreach(SecretKeyValuePair<K,V> kvp in (IEnumerable<SecretKeyValuePair<K,V>>)properties["Items"]!)
					ret[kvp.Key] = kvp.Value;

				return ret;
			}

			protected override void WriteProperties(Utf8JsonWriter writer, Bijection<K,V> value, JsonSerializerOptions ops)
			{
				writer.WriteStartArray("Items");

				foreach(SecretKeyValuePair<K,V> kvp in value)
					KVPConverter.Write(writer,kvp,ops);

				writer.WriteEndArray();
				return;
			}

			private JsonConverter<SecretKeyValuePair<K,V>> KVPConverter;
		}
	}
}
