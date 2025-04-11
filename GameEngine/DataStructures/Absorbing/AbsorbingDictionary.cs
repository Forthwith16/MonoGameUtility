using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace GameEngine.DataStructures.Absorbing
{
	/// <summary>
	/// A dictionary that can only be added to, not removed from.
	/// </summary>
	/// <typeparam name="T">The type to store in the dictionary.</typeparam>
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
}
