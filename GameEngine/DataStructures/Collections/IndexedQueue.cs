using GameEngine.Utility.ExtensionMethods.PrimitiveExtensions;
using System.Collections;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameEngine.DataStructures.Collections
{
	/// <summary>
	/// A queue that can be indexed into and modified 
	/// </summary>
	/// <typeparam name="T">The type of object to store in the queue.</typeparam>
	[JsonConverter(typeof(IndexedQueueConverter))]
	public class IndexedQueue<T> : IList<T>
	{
		/// <summary>
		/// Creates an empty indexed queue.
		/// </summary>
		public IndexedQueue() : this(MinSize)
		{return;}

		/// <summary>
		/// Creates an indexed queue.
		/// </summary>
		/// <param name="seed">The elements of this are added to the queue in the order they appear.</param>
		public IndexedQueue(IEnumerable<T> seed) : this(seed,MinSize)
		{return;}

		/// <summary>
		/// Creates an empty indexed queue.
		/// </summary>
		/// <param name="capacity">The initial capacity of the queue.</param>
		public IndexedQueue(int capacity)
		{
			Array = new T[Math.Max(MinSize,capacity)];
			
			HeadIndex = 0;
			Count = 0;

			return;
		}

		/// <summary>
		/// Creates an indexed queue.
		/// </summary>
		/// <param name="seed">The elements of this are added to the queue in the order they appear.</param>
		/// <param name="capacity">The initial capacity of the queue.</param>
		public IndexedQueue(IEnumerable<T> seed, int capacity) : this(capacity)
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
		public virtual void Enqueue(T item)
		{
			EnsureCapacity(true);

			Array[ToLinearAddress(Count++)] = item;
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

		public virtual void Insert(int index, T item)
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

		/// <summary>
		/// Moves an element of this queue from index <paramref name="from"/> to index <paramref name="to"/>.
		/// In so doing, it will shift all elements between it's current position and it's destination one toward its current position.
		/// </summary>
		/// <param name="from">The index of the element to move.</param>
		/// <param name="to">The index to put the element at.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if either <paramref name="from"/> or <paramref name="to"/> is less than 0 or at least Count.</exception>
		public void Move(int from, int to)
		{
			// Make sure the request makes sense
			if(from < 0 || from >= Count || to < 0 || to >= Count)
				throw new ArgumentOutOfRangeException();

			// No need to answer trivial requests
			if(from == to)
				return;

			// To avoid splitting on moving left/right, we'll define the increment amount as a var instead of a constant
			int inc = from < to ? 1 : -1;

			// Take the shifting element out of the array
			T save = this[from];

			// Shift everything left/right by one
			for(int i = from;i != to;i += inc)
				this[i] = this[i + inc];

			this[to] = save;
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

		public IEnumerator<T> GetEnumerator() => new IQE(this);
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		/// <summary>
		/// Produces a version of this indexed queue that modifies this but prevents adding.
		/// </summary>
		public IndexedQueue<T> ToAddless() => new NoAddIndexedQueue(this);

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
		/// This is the minimum size of the backing array.
		/// </summary>
		private const int MinSize = 8;

		/// <summary>
		/// Any fill percentage of the backing array below this should cause the array to contract.
		/// </summary>
		private const float ContractionFraction = 0.35f;

		/// <summary>
		/// Enumerates an IndexedQueue<T> object.
		/// </summary>
		private class IQE : IEnumerator<T>
		{
			public IQE(IndexedQueue<T> array)
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

			private IndexedQueue<T> Array;
			private int Index;
		}

		/// <summary>
		/// Decorates an indexed queue to prevent any additions to the queue.
		/// </summary>
		[JsonConverter(typeof(IndexedQueueConverter))]
		private sealed class NoAddIndexedQueue : IndexedQueue<T>
		{
			/// <summary>
			/// Constructs a decorator around an indexed queue.
			/// </summary>
			/// <param name="inner">The indexed queue to decorate.</param>
			public NoAddIndexedQueue(IndexedQueue<T> inner) : base()
			{
				Innards = inner;
				Initialized = true;

				return;
			}
			
			public override void Enqueue(T item) => new NotSupportedException();
			public override void Add(T item) => new NotSupportedException();
			public override void Insert(int index, T item) => new NotSupportedException();

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
			private IndexedQueue<T> Innards;

			/// <summary>
			/// If true, then this has been initialized.
			/// If false, the Array setter should not be pass through.
			/// </summary>
			private bool Initialized = false;
		}
	}

	/// <summary>
	/// Creates JSON converters for indexed queues.
	/// </summary>
	public class IndexedQueueConverter : JsonConverterFactory
	{
		public override bool CanConvert(Type t) => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IndexedQueue<>);
		public override JsonConverter? CreateConverter(Type t, JsonSerializerOptions ops) => (JsonConverter?)Activator.CreateInstance(typeof(IQC<>).MakeGenericType(t.GetGenericArguments()[0]),BindingFlags.Instance | BindingFlags.Public,null,new object?[] {},null);

		/// <summary>
		/// Performs the JSON conversion for an indexed queue.
		/// </summary>
		/// <typeparam name="T">The type of object stored in the queue.</typeparam>
		private class IQC<T> : JsonConverter<IndexedQueue<T>>
		{
			public override IndexedQueue<T> Read(ref Utf8JsonReader reader, Type type_to_convert, JsonSerializerOptions ops)
			{
				JsonConverter<T> TConverter = (JsonConverter<T>)ops.GetConverter(typeof(T));

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
				IndexedQueue<T> ret = new IndexedQueue<T>();

				while(!reader.HasNextArrayEnd())
				{
					ret.Enqueue(TConverter.Read(ref reader,typeof(T),ops)!); // We allow null values, so let's just assume this works out on all return values, since we can't check at runtime if T was actually T?
					reader.Read();
				}
				
				// Clean up the array end
				reader.Read();

				// Make sure we're done
				if(!reader.HasNextObjectEnd())
					throw new JsonException();

				return ret;
			}

			public override void Write(Utf8JsonWriter writer, IndexedQueue<T> value, JsonSerializerOptions ops)
			{
				JsonConverter<T> TConverter = (JsonConverter<T>)ops.GetConverter(typeof(T));
				
				writer.WriteStartObject();
				writer.WriteStartArray("Items");

				foreach(T t in value)
					TConverter.Write(writer,t,ops);

				writer.WriteEndArray();
				writer.WriteEndObject();

				return;
			}
		}
	}
}
