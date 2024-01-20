namespace GameEngine.Utility.ExtensionMethods.InterfaceFunctions
{
	/// <summary>
	/// Provides functionality to the ICollection interface.
	/// </summary>
	public static class CollectionFunctions
	{
		/// <summary>
		/// Checks if <paramref name="c"/> is empty.
		/// </summary>
		/// <typeparam name="T">The type of data held in <paramref name="c"/>.</typeparam>
		/// <param name="c">The collection to check for emptiness.</param>
		/// <returns>Returns true if <paramref name="c"/> is empty and false otherwise.</returns>
		public static bool IsEmpty<T>(this ICollection<T> c)
		{return c.Count == 0;}

		/// <summary>
		/// Checks if <paramref name="c"/> is not empty.
		/// </summary>
		/// <typeparam name="T">The type of data held in <paramref name="c"/>.</typeparam>
		/// <param name="c">The collection to check for emptiness.</param>
		/// <returns>Returns true if <paramref name="c"/> is not empty and false otherwise.</returns>
		public static bool IsNotEmpty<T>(this ICollection<T> c)
		{return c.Count > 0;}
	}
}
