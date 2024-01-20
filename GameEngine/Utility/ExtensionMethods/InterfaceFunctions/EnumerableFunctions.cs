using System.Collections;

namespace GameEngine.Utility.ExtensionMethods.InterfaceFunctions
{
	/// <summary>
	/// Adds additional functionality to IEnumerables.
	/// </summary>
	public static class EnumerableFunctions
	{
		/// <summary>
		/// Guarantees that the resultant enumerable is free of null values.
		/// </summary>
		/// <typeparam name="T">The type to enumerate.</typeparam>
		/// <param name="src">The source enumerable object.</param>
		/// <returns>Returns a new enumerable with no null values.</returns>
		public static IEnumerable<T> NotNull<T>(this IEnumerable<T?> src)
		{return new WhereNotNullEnumerable<T>(src);}

		protected class WhereNotNullEnumerable<T> : IEnumerable<T>
		{
			public WhereNotNullEnumerable(IEnumerable<T?> src)
			{
				Source = src;
				return;
			}

			public IEnumerator<T> GetEnumerator()
			{return new WhereNotNullEnumerator<T>(Source);}

			IEnumerator IEnumerable.GetEnumerator()
			{return GetEnumerator();}

			protected IEnumerable<T?> Source
			{get; init;}
		}

		protected class WhereNotNullEnumerator<T> : IEnumerator<T>
		{
			public WhereNotNullEnumerator(IEnumerable<T?> src)
			{
				Iter = src.GetEnumerator();
				return;
			}

			public void Dispose()
			{return;}

			public bool MoveNext()
			{
				do
				{
					if(!Iter.MoveNext())
						return false;
				}
				while(Iter.Current is null);

				return true;
			}

			public void Reset()
			{
				Iter.Reset();
				return;
			}

			public T Current => Iter.Current!;

			object IEnumerator.Current => Iter.Current!;

			protected IEnumerator<T?> Iter
			{get; init;}
		}
	}
}
