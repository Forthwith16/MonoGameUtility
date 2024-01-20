namespace GameEngine.Utility.ExtensionMethods.InterfaceFunctions
{
	/// <summary>
	/// Provides extensions to the IDictionary interface.
	/// </summary>
	public static class DictionaryFunctions
	{
		/// <summary>
		/// Replaces (or adds) the key-value pair to the dictionary.
		/// If the key already existed, the old value is returned.
		/// </summary>
		/// <typeparam name="K">The key type.</typeparam>
		/// <typeparam name="V">The value type.</typeparam>
		/// <param name="dic">The dictionary to access.</param>
		/// <param name="key">The key whose value we want to replace.</param>
		/// <param name="value">The value to replace the old key value with.</param>
		/// <returns>Returns the old key value if one existed or default(V) otherwise.</returns>
		public static V? Put<K,V>(this IDictionary<K,V?> dic, K key, V? value) where K : notnull
		{
			if(dic.TryGetValue(key,out V? ret))
			{
				dic[key] = value;
				return ret;
			}

			dic[key] = value;
			return default;
		}

		/// <summary>
		/// Adds the key-value pair to the dictionary if the key isn't already added to it.
		/// </summary>
		/// <typeparam name="K">The key type.</typeparam>
		/// <typeparam name="V">The value type.</typeparam>
		/// <param name="dic">The dictionary to add to.</param>
		/// <param name="key">The key to add.</param>
		/// <param name="value">The value to add paired with the key.</param>
		/// <returns>Returns true if the key-value pair was added and false otherwise.</returns>
		public static bool TryAdd<K,V>(this IDictionary<K,V?> dic, K key, V? value) where K : notnull
		{
			if(dic.ContainsKey(key))
				return false;

			dic.Add(key, value);
			return true;
		}
	}
}