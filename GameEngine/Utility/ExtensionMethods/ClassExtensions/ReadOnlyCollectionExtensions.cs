using System.Collections.ObjectModel;

namespace GameEngine.Utility.ExtensionMethods.ClassExtensions
{
	/// <summary>
	/// Extensions to the ReadOnlyCollection class.
	/// </summary>
	public static class ReadOnlyCollectionExtensions
	{
		/// <summary>
		/// Creates a read only subcollection from <paramref name="l"/> to <paramref name="r"/> in <paramref name="c"/>, both inclusive.
		/// </summary>
		/// <typeparam name="T">The type stored in the collection.</typeparam>
		/// <param name="c">The collection.</param>
		/// <param name="l">The inclusive left index to start the subcollection from.</param>
		/// <param name="r">The inclusive right index to start the subcollection from.</param>
		/// <returns>Returns a read only collection spanning from <paramref name="l"/> to <paramref name="r"/>.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="l"/> or <paramref name="r"/> are out of range for <paramref name="c"/> or if <paramref name="l"/> > <paramref name="r"/>.</exception>
		public static ReadOnlyCollection<T> Subcollection<T>(this ReadOnlyCollection<T> c, int l, int r)
		{
			if(l < 0 || l > r || r >= c.Count)
				throw new ArgumentOutOfRangeException();

			return new ReadOnlyCollection<T>(new List<T>(c.Skip(l - 1).Take(r - l + 1)));
		}
	}
}
