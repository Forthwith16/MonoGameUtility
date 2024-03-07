namespace GameEngine.Utility.ExtensionMethods.InterfaceFunctions
{
	/// <summary>
	/// Provides extension methods to the IList interface.
	/// </summary>
	public static class ListExtensions
	{
		/// <summary>
		/// Performs a binary search in the sorted list <paramref name="l"/> for <paramref name="value"/>.
		/// </summary>
		/// <typeparam name="T">The type of the list to search through.</typeparam>
		/// <param name="l">The list to search in.</param>
		/// <param name="value">The value to search for.</param>
		/// <returns>
		///	If <paramref name="value"/> is in the list, returns the index of an index containing <paramref name="value"/>.
		///	If not, then a negative number is returned.
		///	<list type="bullet">
		///		<item>If <paramref name="value"/> is less than one or more elements of <paramref name="l"/>, then the negative number is the bitwise complement of the idnex of the first element that is larger than <paramref name="value"/>.</item>
		///		<item>If <paramref name="value"/> is greater than all elements of <paramref name="l"/>, then the negative number returned is the bitwise complement of the size (Count) of the list.</item>
		///	</list>
		///	If this method is called on a nonsorted list <paramref name="l"/>, then its behavior is undefined.
		/// </returns>
		public static int BinarySearch<T>(this IList<T> l, T value) where T : IComparable<T>
		{
			int lower = 0;
			int upper = l.Count - 1;

			while(lower <= upper)
			{
				int mid = (lower + upper) >> 1;
				int cmp_result = value.CompareTo(l[mid]);

				if(cmp_result == 0)
					return mid;
				else if(cmp_result < 0)
					upper = mid - 1;
				else
					lower = mid + 1;
			}

			return ~lower;
		}

		/// <summary>
		/// Performs a binary search in the sorted list <paramref name="l"/> for <paramref name="value"/>.
		/// </summary>
		/// <typeparam name="T">The type of the list to search through.</typeparam>
		/// <param name="l">The list to search in.</param>
		/// <param name="value">The value to search for.</param>
		/// <param name="cmp">The means by which <typeparamref name="T"/> types are compared.</param>
		/// <returns>
		///	If <paramref name="value"/> is in the list, returns the index of an index containing <paramref name="value"/>.
		///	If not, then a negative number is returned.
		///	<list type="bullet">
		///		<item>If <paramref name="value"/> is less than one or more elements of <paramref name="l"/>, then the negative number is the bitwise complement of the idnex of the first element that is larger than <paramref name="value"/>.</item>
		///		<item>If <paramref name="value"/> is greater than all elements of <paramref name="l"/>, then the negative number returned is the bitwise complement of the size (Count) of the list.</item>
		///	</list>
		///	If this method is called on a nonsorted list <paramref name="l"/>, then its behavior is undefined.
		/// </returns>
		public static int BinarySearch<T>(this IList<T> l, T value, IComparer<T> cmp)
		{
			int lower = 0;
			int upper = l.Count - 1;

			while(lower <= upper)
			{
				int mid = (lower + upper) >> 1;
				int cmp_result = cmp.Compare(l[mid],value);

				if(cmp_result == 0)
					return mid;
				else if(cmp_result < 0)
					upper = mid - 1;
				else
					lower = mid + 1;
			}

			return ~lower;
		}
	}
}
