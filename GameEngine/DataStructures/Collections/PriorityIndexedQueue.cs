using System.Collections;
using System.Text;

namespace GameEngine.DataStructures.Collections
{
	/// <summary>
	/// A priority queue that can be indexed into and modified 
	/// </summary>
	/// <typeparam name="T">The type of object to store in the queue.</typeparam>
	/// <remarks>This is a bad data structure and should not be used except in special circumstances.</remarks>
	public class PriorityIndexedQueue<T> : IList<T>
	{
		/// <summary>
		/// Creates an empty priority indexed queue.
		/// </summary>
		/// <param name="cmp">The means by which items are compared.</param>
		public PriorityIndexedQueue(IComparer<T> cmp) : this(MinSize,cmp)
		{return;}

		/// <summary>
		/// Creates a priority indexed queue.
		/// </summary>
		/// <param name="seed">The elements of this are added to the queue in the order they appear.</param>
		public PriorityIndexedQueue(IEnumerable<T> seed, IComparer<T> cmp) : this(seed,MinSize,cmp)
		{return;}

		/// <summary>
		/// Creates an empty priority indexed queue.
		/// </summary>
		/// <param name="capacity">The initial capacity of the queue.</param>
		public PriorityIndexedQueue(int capacity, IComparer<T> cmp)
		{
			Array = new T[Math.Max(MinSize,capacity)];
			
			HeadIndex = 0;
			Count = 0;

			Scale = cmp;
			return;
		}

		/// <summary>
		/// Creates a priority indexed queue.
		/// </summary>
		/// <param name="seed">The elements of this are added to the queue in the order they appear.</param>
		/// <param name="capacity">The initial capacity of the queue.</param>
		public PriorityIndexedQueue(IEnumerable<T> seed, int capacity, IComparer<T> cmp) : this(capacity,cmp)
		{
			foreach(T t in seed)
				Enqueue(t);

			return;
		}

		/// <summary>
		/// Converts a virtual index into a linear index into the backing array.
		/// </summary>
		/// <param name="virtual_address">
		///	The virtual index into the array.
		///	This value will be shifted right by HeadIndex and then brought back into range for indexing into the backing array.
		///	This value should be in the interval [0,Count).
		/// </param>
		protected int ToLinearAddress(int virtual_address)
		{
			int addr = virtual_address + HeadIndex;
			return addr >= Array.Length ? addr - Array.Length : addr;
		}

		/// <summary>
		/// Ensures the array has an acceptable amount of capacity.
		/// </summary>
		/// <param name="expand">If true, then the array may need to expand. Otherwise, it may need to contract. The opposite condition is never checked.</param>
		protected void EnsureCapacity(bool expand)
		{
			if(expand)
			{
				if(Count == Array.Length)
				{
					T[] bigger = new T[Array.Length << 1];

					for(int i = 0;i < Count;i++)
						bigger[i] = this[i]; // Might as well make the array linear when we copy/paste

					Array = bigger;
					HeadIndex = 0;
				}
			}
			else
			{
				if(Array.Length > MinSize && Count < Array.Length * ContractionFraction)
				{
					T[] smaller = new T[Math.Max(MinSize,Array.Length >> 1)]; // Never go below the MinSize in length

					for(int i = 0;i < Count;i++)
						smaller[i] = this[i]; // Might as well make the array linear when we copy/paste

					Array = smaller;
					HeadIndex = 0;
				}
				else if(Empty) // We do this in case we empty the array when the head is not at the start; this is not important, but it does make it easier to maintain
					HeadIndex = 0;
			}

			return;
		}
		
		/// <summary>
		/// Enqueues <paramref name="item"/> to the end of the queue.
		/// </summary>
		public virtual bool Enqueue(T item)
		{
			EnsureCapacity(true);

			// We have no real better option, so just put the item at the end and move it forward until it's too heavy
			Array[ToLinearAddress(Count++)] = item;

			for(int i = Count - 2;i >= 0;i--)
			{
				if(Scale.Compare(this[i],this[i + 1]) <= 0)
					break;
				
				item = this[i + 1];
				this[i + 1] = this[i];
				this[i] = item;
			}

			return true;
		}

		/// <summary>
		/// Requeues the item at <paramref name="index"/> with whatever new priority it has internally.
		/// </summary>
		/// <param name="index">The index of the item to requeue.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index"/> is less than 0 or at least Count.</exception>
		public virtual void Requeue(int index)
		{
			// If index is out of bounds, then do nothing
			if(index < 0 || index >= Count)
				throw new ArgumentOutOfRangeException();

			// Move as far left as needed
			for(;index > 0 && Scale.Compare(this[index - 1],this[index]) > 0;index--)
			{
				T item = this[index];
				this[index] = this[index - 1];
				this[index - 1] = item;
			}

			// Move as far right as needed
			for(;index < Count - 1 && Scale.Compare(this[index],this[index + 1]) > 0;index++)
			{
				T item = this[index];
				this[index] = this[index - 1];
				this[index - 1] = item;
			}

			return;
		}

		/// <summary>
		/// Requeues <paramref name="item"/> in this with whatever new priority it has.
		/// If <paramref name="item"/> is not already part of this, then it will be enequeued normally.
		/// </summary>
		/// <param name="item">The item to requeue.</param>
		public void Requeue(T item)
		{
			int index = IndexOf(item);

			if(index < 0 || index >= Count)
			{
				Enqueue(item);
				return;
			}

			Requeue(index);
			return;
		}

		/// <summary>
		/// Removes and returns the front element of the queue.
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown if the queue is empty.</exception>
		public T Dequeue()
		{
			if(Count == 0)
				throw new InvalidOperationException();

			T ret = this[0];

			HeadIndex++;
			Count--;

			if(HeadIndex >= Array.Length)
				HeadIndex = 0;

			EnsureCapacity(false);
			return ret;
		}

		public virtual void Add(T item) => Enqueue(item);

		public virtual void Insert(int index, T item) => throw new NotSupportedException();

		/// <summary>
		/// Performs an insertion.
		/// </summary>
		/// <param name="index">What index to insert into.</param>
		/// <param name="item">The item to insert.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index"/> is less than 0 or greater than Count.</exception>
		protected virtual void SecretInsert(int index, T item)
		{
			if(index < 0 || index > Count)
				throw new ArgumentOutOfRangeException();

			// We will insert the new item and bump everything but the last element around
			for(int i = index;i < Count;i++)
			{
				T temp = this[i];
				this[i] = item;
				item = temp;
			}

			// To finish, we will enqueue the last element
			Enqueue(item);

			return;
		}

		public bool Remove(T item)
		{
			int index = IndexOf(item);

			if(index == -1)
				return false;

			RemoveAt(index);
			return true;
		}

		public void RemoveAt(int index)
		{
			if(index < 0 || index >= Count)
				throw new ArgumentOutOfRangeException();

			// We will simply shift items from index back to 0 right one and then dequeue the duplicate front element
			for(int i = index - 1;i >= 0;i--)
				this[i + 1] = this[i];

			Dequeue();
			return;
		}

		/// <summary>
		/// Removes all elements of this queue occurring at or after index <paramref name="start"/>.
		/// </summary>
		/// <param name="start">The start index. All elements at or after this index are removed.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="start"/> is less than 0 or at least Count.</exception>
		public void RemoveFrom(int start)
		{
			if(start < 0 || start >= Count)
				throw new ArgumentOutOfRangeException();

			Count = start;
			EnsureCapacity(false);

			return;
		}

		public int IndexOf(T item)
		{
			for(int i = 0;i < Count;i++)
				if(item is null ? this[i] is null : item.Equals(this[i]))
					return i;

			return -1;
		}

		public bool Contains(T item) => IndexOf(item) != -1;

		public void Clear()
		{
			HeadIndex = 0;
			Count = 0;

			Array = new T[MinSize];
			return;
		}

		public void CopyTo(T[] array, int offset)
		{
			if(offset < 0)
				throw new ArgumentOutOfRangeException();

			if(array.Length < offset + Count)
				throw new ArgumentException();

			for(int i = 0;i < Count;i++)
				array[offset + i] = this[i];

			return;
		}

		public IEnumerator<T> GetEnumerator() => new PIQE(this);
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		/// <summary>
		/// Produces a version of this indexed queue that modifies this but prevents adding.
		/// </summary>
		public PriorityIndexedQueue<T> ToAddless() => new NoAddPriorityIndexedQueue(this,Scale);

		public override string ToString()
		{
			if(Empty)
				return "{}";

			StringBuilder ret = new StringBuilder();
			ret.Append('{');

			foreach(T t in this)
				ret.Append(t + ",");

			ret.Remove(ret.Length - 1,1);
			ret.Append('}');

			return ret.ToString();
		}

		/// <summary>
		/// Gets or sets a value in an index.
		/// </summary>
		/// <remarks>Do not use the set of this class.</remarks>
		public T this[int index]
		{
			get
			{
				if(index < 0 || index >= Count)
					throw new ArgumentOutOfRangeException();

				return Array[ToLinearAddress(index)];
			}

			set
			{
				if(index < 0 || index >= Count)
					throw new ArgumentOutOfRangeException();

				Array[ToLinearAddress(index)] = value;
				return;
			}
		}

		/// <summary>
		/// The backing array.
		/// </summary>
		protected virtual T[] Array
		{get; set;}

		/// <summary>
		/// The front of the queue.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if the queue is empty.</exception>
		public T Front => this[0];

		public virtual int Count
		{get; protected set;}

		/// <summary>
		/// If true, then the queue is empty.
		/// </summary>
		public bool Empty => Count == 0;

		/// <summary>
		/// The location of index 0 in the backing array.
		/// </summary>
		protected virtual int HeadIndex
		{get; set;}

		public bool IsReadOnly => false;

		/// <summary>
		/// The means by which we compare <typeparamref name="T"/> types.
		/// </summary>
		private IComparer<T> Scale
		{get;}

		/// <summary>
		/// This is the minimum size of the backing array.
		/// </summary>
		private const int MinSize = 8;

		/// <summary>
		/// Any fill percentage of the backing array below this should cause the array to contract.
		/// </summary>
		private const float ContractionFraction = 0.35f;

		/// <summary>
		/// Enumerates an PriorityIndexedQueue<T> object.
		/// </summary>
		private class PIQE : IEnumerator<T>
		{
			public PIQE(PriorityIndexedQueue<T> array)
			{
				Array = array;
				Index = -1;

				return;
			}

			public void Dispose()
			{return;}
			
			public bool MoveNext()
			{
				if(Index >= Array.Count)
					return false;

				return ++Index < Array.Count;
			}

			public void Reset()
			{
				Index = -1;
				return;
			}
			
			public T Current => Index < 0 || Index >= Array.Count ? throw new InvalidOperationException() : Array[Index];
			object? IEnumerator.Current => Current!;

			private PriorityIndexedQueue<T> Array;
			private int Index;
		}

		/// <summary>
		/// Decorates a priority indexed queue to prevent any additions to the queue.
		/// </summary>
		private sealed class NoAddPriorityIndexedQueue : PriorityIndexedQueue<T>
		{
			/// <summary>
			/// Constructs a decorator around an indexed queue.
			/// </summary>
			/// <param name="inner">The indexed queue to decorate.</param>
			public NoAddPriorityIndexedQueue(PriorityIndexedQueue<T> inner, IComparer<T> cmp) : base(cmp)
			{
				Innards = inner;
				Initialized = true;

				return;
			}
			
			public override bool Enqueue(T item) => throw new NotSupportedException();
			public override void Add(T item) => throw new NotSupportedException();
			public override void Insert(int index, T item) => throw new NotSupportedException();

			protected override T[] Array
			{
				get => Innards.Array;
				
				set
				{
					if(Initialized)
						Innards.Array = value;
					
					return;
				}
			}

			protected override int HeadIndex
			{
				get => Innards.HeadIndex;

				set
				{
					if(Initialized)
						Innards.HeadIndex = value;
					
					return;
				}
			}

			public override int Count
			{
				get => Innards.Count;
				
				protected set
				{
					if(Initialized)
						Innards.Count = value;
					
					return;
				}
			}

			/// <summary>
			/// The inner indexed queue we're decorating.
			/// </summary>
			private PriorityIndexedQueue<T> Innards;

			/// <summary>
			/// If true, then this has been initialized.
			/// If false, the Array setter should not be pass through.
			/// </summary>
			private bool Initialized = false;
		}
	}
}
