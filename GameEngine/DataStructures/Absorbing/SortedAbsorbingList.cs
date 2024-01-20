namespace GameEngine.DataStructures.Absorbing
{
	/// <summary>
	/// An AbsorbingList (a list that can only be added to or read from) that maintains sorted order.
	/// For items that have equal value according to the IComparer, items added first will appear before items added later.
	/// </summary>
	/// <typeparam name="T">The type to store in the list.</typeparam>
	public class SortedAbsorbingList<T> : AbsorbingList<T>
	{
		/// <summary>
		/// Creates an empty absorbing list.
		/// </summary>
		/// <param name="scale">The means by which items are compared.</param>
		public SortedAbsorbingList(IComparer<T> scale) : base()
		{
			Scale = scale;
			return;
		}

		/// <summary>
		/// Creates an absorbing list initially populated with <paramref name="seed"/>.
		/// </summary>
		/// <param name="scale">The means by which items are compared.</param>
		/// <param name="seed">The initial items to add to the list.</param>
		public SortedAbsorbingList(IComparer<T> scale,IEnumerable<T> seed) : this(scale) // base(IEnumerable<T>) uses Add, but we need to assign Scale first for that to work
		{
			foreach(T t in seed.OrderBy(t => t,Scale)) // We pre-sort for speed
				Add(t);

			return;
		}

		/// <summary>
		/// Creates an empty absorbing list with initial capacity <paramref name="capacity"/>.
		/// </summary>
		/// <param name="scale">The means by which items are compared.</param>
		/// <param name="capacity">The initial capacity.</param>
		public SortedAbsorbingList(IComparer<T> scale,int capacity) : base(capacity)
		{
			Scale = scale;
			return;
		}

		public override void Add(T item)
		{
			EnsureCapacity();
			int i = Count++;

			while(i > 0 && Scale.Compare(BackingArray[i - 1],item) > 0)
				BackingArray[i] = BackingArray[--i];

			BackingArray[i] = item;
			return;
		}

		public override void Insert(int index,T item)
		{throw new NotSupportedException();}

		/// <summary>
		/// Searches this list for <paramref name="t"/>.
		/// </summary>
		/// <param name="t">The item to search for.</param>
		/// <returns>
		///	Returns the index of <paramref name="t"/> if found; otherwise, a negative number.
		///	If <paramref name="t"/> is not found and <paramref name="t"/> is less than one or more elements in this list, the negative number returned is the bitwise complement of the index of the first element that is larger than it.
		///	If <paramref name="t"/> is not found and <paramref name="t"/> is greater than all elements in this list, the negative number returned is the bitwise complement of Count.
		/// </returns>
		public int BinarySearch(T t)
		{return Array.BinarySearch(BackingArray,0,Count,t,Scale);}

		/// <summary>
		/// Searches this list for <paramref name="t"/>.
		/// </summary>
		/// <param name="t">The item to search for.</param>
		/// <param name="extractor">The means by which we pull an <typeparamref name="E"/> type out of <typeparamref name="T"/> types.</param>
		/// <param name="scale">The means by which <typeparamref name="E"/> types are compared.</param>
		/// <returns>
		///	Returns the index of <paramref name="t"/> if found; otherwise, a negative number.
		///	If <paramref name="t"/> is not found and <paramref name="t"/> is less than one or more elements in this list, the negative number returned is the bitwise complement of the index of the first element that is larger than it.
		///	If <paramref name="t"/> is not found and <paramref name="t"/> is greater than all elements in this list, the negative number returned is the bitwise complement of Count.
		/// </returns>
		/// <typeparam name="E">A type to compare <typeparamref name="T"/> types against.</typeparam>
		public int BinarySearch<E>(E e,Func<T,E> extractor,IComparer<E> scale)
		{
			int lo = 0;
			int hi = Count - 1;

			while(lo <= hi)
			{
				int i = lo + (hi - lo >> 1); // i might overflow if lo and hi are both large positive numbers.
				int c = scale.Compare(extractor(BackingArray[i]),e);

				if(c == 0)
					return i;

				if(c < 0)
					lo = i + 1;
				else
					hi = i - 1;
			}

			return ~lo;
		}

		/// <summary>
		/// The means by which items are compared.
		/// </summary>
		protected IComparer<T> Scale
		{get; set;}
	}
}
