using GameEngine.Utility.ExtensionMethods.PrimitiveExtensions;
using GameEngine.Utility.Serialization;
using System.Collections;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameEngine.DataStructures.Absorbing
{
	/// <summary>
	/// A list that can only be added to, not removed from.
	/// </summary>
	/// <typeparam name="T">The type to store in the list.</typeparam>
	[JsonConverter(typeof(JsonAbsorbingListConverter))]
	public class AbsorbingList<T> : IList<T>, IReadOnlyList<T>
	{
		/// <summary>
		/// Creates an empty absorbing list.
		/// </summary>
		public AbsorbingList()
		{
			BackingArray = new T[DefaultCapacity];
			Count = 0;

			return;
		}

		/// <summary>
		/// Creates an absorbing list initially populated with <paramref name="seed"/>.
		/// </summary>
		/// <param name="seed">The initial items to add to the list.</param>
		public AbsorbingList(IEnumerable<T> seed) : this()
		{
			foreach(T t in seed)
				Add(t); // Slightly less efficient than a local for loop, but who cares

			return;
		}

		/// <summary>
		/// Creates an empty absorbing list with initial capacity <paramref name="capacity"/>.
		/// </summary>
		/// <param name="capacity">The initial capacity.</param>
		public AbsorbingList(int capacity)
		{
			BackingArray = new T[capacity];
			Count = 0;

			return;
		}

		public virtual void Add(T item)
		{
			EnsureCapacity();
			BackingArray[Count++] = item;

			return;
		}

		public virtual void Insert(int index,T item)
		{
			if(index < 0 || index > Count)
				throw new ArgumentOutOfRangeException(nameof(index));

			EnsureCapacity();

			for(int i = Count;i > index;)
				BackingArray[i] = BackingArray[--i];

			BackingArray[index] = item;
			Count++;

			return;
		}

		/// <summary>
		/// Ensures that there is enough room to insert a new element into this list.
		/// </summary>
		protected void EnsureCapacity()
		{
			if(Count < BackingArray.Length)
				return;

			T[] nba = new T[BackingArray.Length << 1];
			CopyTo(nba);

			BackingArray = nba;
			return;
		}

		public bool Remove(T item)
		{throw new NotSupportedException();}

		public void RemoveAt(int index)
		{throw new NotSupportedException();}

		public bool Contains(T item)
		{return IndexOf(item) > -1;}

		public int IndexOf(T item)
		{
			for(int i = 0;i < Count;i++)
			{
				T t = BackingArray[i];

				if(t is null)
				{
					if(item is null)
						return i;
				}
				else if(t.Equals(item))
					return i;
			}

			return -1;
		}

		public void Clear()
		{throw new NotSupportedException();}

		public void CopyTo(T[] array,int arrayIndex = 0)
		{
			if(array is null)
				throw new ArgumentNullException(nameof(array));

			if(arrayIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(arrayIndex));

			if(array.Length < Count - arrayIndex)
				throw new ArithmeticException();

			for(int i = arrayIndex, j = 0;i < Count;i++, j++)
				array[j] = BackingArray[i];

			return;
		}

		public IEnumerator<T> GetEnumerator()
		{return new AbsorbingEnumerator(this);}

		IEnumerator IEnumerable.GetEnumerator()
		{return GetEnumerator();}

		public override string ToString()
		{
			if(Count == 0)
				return "[]";

			string ret = "[" + this[0];

			for(int i = 1;i < Count;i++)
				ret += "," + this[i];

			return ret + "]";
		}

		/// <summary>
		/// The backing array of this list.
		/// </summary>
		protected T[] BackingArray
		{get; set;}

		public T this[int index]
		{
			get
			{
				if(index < 0 || index >= Count)
					throw new ArgumentOutOfRangeException(nameof(index));

				return BackingArray[index];
			}

			set => throw new NotSupportedException();
		}

		public int Count
		{get; protected set;}

		public bool IsReadOnly => false;

		/// <summary>
		/// The default capacity of absorbing lists.
		/// </summary>
		protected const int DefaultCapacity = 10;

		/// <summary>
		/// An enumerator over an AbsorbingList.
		/// </summary>
		protected class AbsorbingEnumerator : IEnumerator<T>
		{
			/// <summary>
			/// Creates a new absorbing enumerator.
			/// </summary>
			/// <param name="list">The list to iterate over.</param>
			public AbsorbingEnumerator(AbsorbingList<T> list)
			{
				List = list;
				Index = -1;

				return;
			}

			public void Dispose()
			{return;}

			public bool MoveNext()
			{
				if(Index >= List.Count)
					return false;

				return ++Index < List.Count;
			}

			public void Reset()
			{
				Index = -1;
				return;
			}

			/// <summary>
			/// The list we are iterating over.
			/// </summary>
			protected AbsorbingList<T> List
			{get; set;}

			/// <summary>
			/// The current index.
			/// </summary>
			protected int Index
			{get; set;}

			public T Current => List[Index];

			object? IEnumerator.Current => Current;
		}
	}

	/// <summary>
	/// Creates JSON converters for absorbing lists.
	/// </summary>
	file class JsonAbsorbingListConverter : JsonBaseConverterFactory
	{
		/// <summary>
		/// Constructs the factory.
		/// </summary>
		public JsonAbsorbingListConverter() : base((t,ops) => [],typeof(AbsorbingList<>),typeof(ALC<>))
		{return;}

		/// <summary>
		/// Performs the JSON conversion for an absorbing list.
		/// </summary>
		/// <typeparam name="T">The type of object stored in the list.</typeparam>
		private class ALC<T> : JsonBaseConverter<AbsorbingList<T>>
		{
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
				JsonConverter<T> TConverter = (JsonConverter<T>)ops.GetConverter(typeof(T));
				AbsorbingList<T> ret = new AbsorbingList<T>();

				while(!reader.HasNextArrayEnd())
				{
					ret.Add(TConverter.Read(ref reader,typeof(T),ops)!); // We allow null values, so let's just assume this works out on all return values, since we can't check at runtime if T was actually T?
					reader.Read();
				}

				return ret;
			}

			protected override AbsorbingList<T> ConstructT(Dictionary<string,object?> properties)
			{
				if(properties.Count != 1)
					throw new JsonException();

				return properties["Items"] as AbsorbingList<T> ?? throw new JsonException();
			}

			protected override void WriteProperties(Utf8JsonWriter writer, AbsorbingList<T> value, JsonSerializerOptions ops)
			{
				JsonConverter<T> TConverter = (JsonConverter<T>)ops.GetConverter(typeof(T));

				writer.WriteStartArray("Items");

				foreach(T t in value)
					TConverter.Write(writer,t,ops);

				writer.WriteEndArray();
				return;
			}
		}
	}
}
