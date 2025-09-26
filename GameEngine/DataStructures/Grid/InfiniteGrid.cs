using GameEngine.DataStructures.Collections;
using GameEngine.DataStructures.Maps;
using GameEngine.DataStructures.Utility;
using GameEngine.Utility.ExtensionMethods.PrimitiveExtensions;
using GameEngine.Utility.Serialization;
using Microsoft.Xna.Framework;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameEngine.DataStructures.Grid
{
	/// <summary>
	/// Represents a grid with no boundaries, though everything it contains must fall on a square lattice point.
	/// In addition, each item contained within the grid must be unique.
	/// </summary>
	/// <typeparam name="T">The data type.</typeparam>
	[JsonConverter(typeof(JsonInfiniteGridConverter))]
	public class InfiniteGrid<T> : IEnumerable<KeyValuePair<Point,T>> where T : notnull
	{
		/// <summary>
		/// Creates an empty grid.
		/// </summary>
		/// <param name="nonnegative">If true, only nonnegative point positions are permitted (i.e. only in the first quadrant).</param>
		public InfiniteGrid(bool nonnegative = false)
		{
			Grid = new Bijection<Point,T>();

			HorizontalList = new SortedSet<Point>(Comparer<Point>.Create((a,b) =>
			{
				int c = a.X.CompareTo(b.X);

				if(c != 0)
					return c;

				return a.Y.CompareTo(b.Y);
			}));

			VerticalList = new SortedSet<Point>(Comparer<Point>.Create((a,b) =>
			{
				int c = a.Y.CompareTo(b.Y);

				if(c != 0)
					return c;

				return a.X.CompareTo(b.X);
			}));

			Nonnegative = nonnegative;
			return;
		}

		/// <summary>
		/// Adds a mapping of (<paramref name="x"/>,<paramref name="y"/>) to <paramref name="item"/>.
		/// This fails if (<paramref name="x"/>,<paramref name="y"/>) is already in the domain or <paramref name="item"/> is already in the image.
		/// </summary>
		/// <param name="x">The x position to add <paramref name="item"/> at.</param>
		/// <param name="y">The y position to add <paramref name="item"/> at.</param>
		/// <param name="item">The new output to map (<paramref name="x"/>,<paramref name="y"/>) to.</param>
		public bool Add(int x, int y, T item) => Add(new Point(x,y),item);

		/// <summary>
		/// Adds a mapping of <paramref name="p"/> to <paramref name="item"/>.
		/// This fails if <paramref name="p"/> is already in the domain or <paramref name="item"/> is already in the image.
		/// </summary>
		/// <param name="p">The position to add <paramref name="item"/> at.</param>
		/// <param name="item">The new output to map <paramref name="p"/> to.</param>
		public bool Add(Point p, T item)
		{
			if(Nonnegative && (p.X < 0 || p.Y < 0) || PositionOccupied(p) || ContainsItem(item))
				return false;

			Grid.Add(p,item);

			HorizontalList.Add(p);
			VerticalList.Add(p);

			return true;
		}

		/// <summary>
		/// Adds an item at a position to this grid.
		/// This fails if the position is already occupied or if the item is already present in the grid.
		/// </summary>
		/// <param name="item">The item to add and the position to add it at.</param>
		public bool Add(KeyValuePair<Point,T> item) => Add(item.Key,item.Value);

		/// <summary>
		/// Attempts to obtain the item at position (<paramref name="x"/>,<paramref name="y"/>) and places it into <paramref name="item"/>.
		/// </summary>
		/// <param name="x">The x position to get an item at.</param>
		/// <param name="y">The y position to get an item at.</param>
		/// <param name="item">The place to store the item. This value will default if no such output exists.</param>
		/// <returns>Returns true if (<paramref name="x"/>,<paramref name="y"/>) had an item and false otherwise.</returns>
		public bool TryGetItem(int x, int y, [MaybeNullWhen(false)] out T item)
		{return TryGetItem(new Point(x,y),out item);}

		/// <summary>
		/// Attempts to obtain the item at position <paramref name="p"/> and places it into <paramref name="item"/>.
		/// </summary>
		/// <param name="p">The position to get an item at.</param>
		/// <param name="item">The place to store the item. This value will default if no such output exists.</param>
		/// <returns>Returns true if <paramref name="p"/> had an item and false otherwise.</returns>
		public bool TryGetItem(Point p, [MaybeNullWhen(false)] out T item)
		{return Grid.TryGetValue(p,out item);}

		/// <summary>
		/// Locates <paramref name="item"/>.
		/// </summary>
		/// <param name="item">The item to locate.</param>
		/// <returns>Returns the position of <paramref name="item"/> in the grid.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="item"/> is null.</exception>
		/// <exception cref="KeyNotFoundException">Thrown if <paramref name="item"/> is not in the grid.</exception>
		public Point GetPosition(T item)
		{return Grid.Invert(item);}

		/// <summary>
		/// Attempts to obtain the position of the item <paramref name="item"/>.
		/// </summary>
		/// <param name="item">The item to locate.</param>
		/// <param name="x">The place to store the x position of <paramref name="item"/>. This value will default if no such item exists.</param>
		/// <param name="y">The place to store the y position of <paramref name="item"/>. This value will default if no such item exists.</param>
		/// <returns>Returns true if <paramref name="item"/>'s position could be obtained and false otherwise.</returns>
		public bool TryGetPosition(T item, out int x, out int y)
		{
			if(!Grid.TryInvert(item,out Point pos))
			{
				x = default;
				y = default;

				return false;
			}

			x = pos.X;
			y = pos.Y;

			return true;
		}

		/// <summary>
		/// Attempts to obtain the position of the item <paramref name="item"/>.
		/// </summary>
		/// <param name="item">The item to locate.</param>
		/// <param name="pos">The place to store the position of <paramref name="item"/>. This value will default if no such item exists.</param>
		/// <returns>Returns true if <paramref name="item"/>'s position could be obtained and false otherwise.</returns>
		public bool TryGetPosition(T item, [MaybeNullWhen(false)] out Point pos)
		{return Grid.TryInvert(item,out pos);}

		/// <summary>
		/// Removes the item at the position.
		/// </summary>
		/// <param name="x">The x position to remove.</param>
		/// <param name="y">The y position to remove.</param>
		/// <returns>Returns true if something was removed from the grid and false otherwise.</returns>
		public bool Remove(int x, int y)
		{return Remove(new Point(x,y));}

		/// <summary>
		/// Removes the item at position <paramref name="p"/>.
		/// </summary>
		/// <param name="p">The postion to clear in the grid.</param>
		/// <returns>Returns true if something was removed from the grid and false otherwise.</returns>
		public bool Remove(Point p)
		{return Grid.Remove(p) && HorizontalList.Remove(p) && VerticalList.Remove(p);}

		/// <summary>
		/// Removes the item <paramref name="item"/> from the grid.
		/// </summary>
		/// <param name="item">The item to remove from the grid.</param>
		/// <returns>Returns true if the grid was changed and false otherwise.</returns>
		public bool RemoveItem(T item)
		{
			if(!TryGetPosition(item,out Point p))
				return false;
			
			return Grid.Remove(p) && HorizontalList.Remove(p) && VerticalList.Remove(p);
		}

		/// <summary>
		/// Removes the item at the given position but only if it matches the item provided.
		/// </summary>
		/// <param name="item">The position and item to remove.</param>
		/// <returns>Returns true if the remove was successful and false otherwise.</returns>
		public bool Remove(KeyValuePair<Point,T> item)
		{return Grid.Remove(item) && HorizontalList.Remove(item.Key) && VerticalList.Remove(item.Key);}

		/// <summary>
		/// Determines if this grid has an item at the given position.
		/// </summary>
		/// <param name="x">The x position to check for occupancy.</param>
		/// <param name="y">The y position to check for occupancy.</param>
		/// <returns>Returns true if position <paramref name="p"/> is occupied and false otherwise.</returns>
		public bool PositionOccupied(int x, int y) => Grid.ContainsKey(new Point(x,y));

		/// <summary>
		/// Determines if this grid has an item at the given position.
		/// </summary>
		/// <param name="p">The position to check for occupancy.</param>
		/// <returns>Returns true if position <paramref name="p"/> is occupied and false otherwise.</returns>
		public bool PositionOccupied(Point p) => Grid.ContainsKey(p);

		/// <summary>
		/// Determines if this grid does not have an item at the given position.
		/// </summary>
		/// <param name="x">The x position to check for vacancy.</param>
		/// <param name="y">The y position to check for vacancy.</param>
		/// <returns>Returns true if position <paramref name="p"/> is unoccupied and false otherwise.</returns>
		public bool PositionEmpty(int x,int y) => !PositionOccupied(new Point(x,y));

		/// <summary>
		/// Determines if this grid does not have an item at the given position.
		/// </summary>
		/// <param name="p">The position to check for vacancy.</param>
		/// <returns>Returns true if position <paramref name="p"/> is unoccupied and false otherwise.</returns>
		public bool PositionEmpty(Point p) => !PositionOccupied(p);

		/// <summary>
		/// Determines if this grid contains <paramref name="value"/> somewhere.
		/// </summary>
		/// <param name="value">The item to look for.</param>
		/// <returns>Returns true if <paramref name="value"/> is present in the grid and false otherwise.</returns>
		public bool ContainsItem(T value) => Grid.ContainsValue(value);

		/// <summary>
		/// Determines if a particular point contains a specific item.
		/// </summary>
		/// <param name="item">The point and item to check for containment.</param>
		/// <returns>Returns true if <paramref name="item"/> belongs to the grid and false otherwise.</returns>
		public bool Contains(KeyValuePair<Point,T> item) => Grid.Contains(item);

		/// <summary>
		/// Clears this grid of all items.
		/// </summary>
		public void Clear()
		{
			Grid.Clear();

			HorizontalList.Clear();
			VerticalList.Clear();

			return;
		}

		public IEnumerator<KeyValuePair<Point,T>> GetEnumerator() => Grid.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => Grid.GetEnumerator();

		/// <summary>
		/// Sweeps across the grid from left to right (and internally to a column low to high).
		/// </summary>
		public IEnumerator<Point> SweepRight() => HorizontalList.GetEnumerator();

		/// <summary>
		/// Sweeps across the grid from low to high (and internally to a row left to right).
		/// </summary>
		public IEnumerator<Point> SweepUp() => VerticalList.GetEnumerator();

		/// <summary>
		/// Indexes into this grid at position (<paramref name="x"/>,<paramref name="y"/>).
		/// </summary>
		/// <param name="x">The x position to index into.</param>
		/// <param name="x">The y position to index into.</param>
		/// <returns>Returns the item at position (<paramref name="x"/>,<paramref name="y"/>).</returns>
		/// <exception cref="ArgumentException">Thrown in a set operation if the provided value exists somewhere other than (<paramref name="x"/>,<paramref name="y"/>).</exception>
		/// <exception cref="KeyNotFoundException">Thrown in a get operation if position (<paramref name="x"/>,<paramref name="y"/>) does not contain an item.</exception>
		public T this[int x, int y]
		{
			get => this[new Point(x,y)];
			set => this[new Point(x,y)] = value;
		}

		/// <summary>
		/// Indexes into this grid at position <paramref name="p"/>.
		/// </summary>
		/// <param name="p">The position to index into.</param>
		/// <returns>Returns the item at position <paramref name="p"/>.</returns>
		/// <exception cref="ArgumentException">Thrown in a set operation if the provided value exists somewhere other than <paramref name="p"/>.</exception>
		/// <exception cref="KeyNotFoundException">Thrown in a get operation if position <paramref name="p"/> does not contain an item.</exception>
		public T this[Point p]
		{
			get => Grid[p];

			set
			{
				if(Nonnegative && (p.X < 0 || p.Y < 0))
					return;

				bool extant = Grid.ContainsKey(p);

				// If we're going to throw an error, do it early so that we can potentially recover without having modified other parts of the data structure
				Grid[p] = value;

				if(!extant)
				{
					HorizontalList.Add(p);
					VerticalList.Add(p);
				}
				
				return;
			}
		}

		/// <summary>
		/// If true, then only nonnegative point positions are permitted.
		/// In other words, the grid is restricted to the first quadrant.
		/// </summary>
		public bool Nonnegative
		{get; protected set;}

		/// <summary>
		/// The number of items in the grid.
		/// </summary>
		public int Count => Grid.Count;

		/// <summary>
		/// The set of positions with an item.
		/// </summary>
		public ICollection<Point> Positions => Grid.Domain;

		/// <summary>
		/// The set of items in the grid.
		/// </summary>
		public ICollection<T> Items => Grid.Image;

		/// <summary>
		/// The minimum x position of all items in the grid.
		/// </summary>
		public int MinX => HorizontalList.Min.X;

		/// <summary>
		/// The maximum x position of all items in the grid.
		/// </summary>
		public int MaxX => HorizontalList.Max.X;

		/// <summary>
		/// The minimum y position of all items in the grid.
		/// </summary>
		public int MinY => VerticalList.Min.Y;

		/// <summary>
		/// The maximum y position of all items in the grid.
		/// </summary>
		public int MaxY => VerticalList.Max.Y;

		/// <summary>
		/// The grid containing the elements we need.
		/// </summary>
		protected Bijection<Point,T> Grid
		{get;}

		/// <summary>
		/// The items in this grid sorted by their horizontal component.
		/// </summary>
		protected SortedSet<Point> HorizontalList
		{get;}

		/// <summary>
		/// The items in this grid sorted by their vertical component.
		/// </summary>
		protected SortedSet<Point> VerticalList
		{get;}
	}

	/// <summary>
	/// Creates JSON converters for infinite grids.
	/// </summary>
	file class JsonInfiniteGridConverter : JsonBaseConverterFactory
	{
		/// <summary>
		/// Constructs the factory.
		/// </summary>
		public JsonInfiniteGridConverter() : base((t,ops) => [ops],typeof(InfiniteGrid<>),typeof(IGC<>))
		{return;}

		/// <summary>
		/// Performs the JSON conversion for an infinite grid.
		/// </summary>
		/// <typeparam name="T">The type of object stored in the queue.</typeparam>
		private class IGC<T> : JsonBaseConverter<InfiniteGrid<T>> where T : notnull
		{
			public IGC(JsonSerializerOptions ops)
			{
				ItemConverter = (JsonConverter<SecretKeyValuePair<Point,T>>)ops.GetConverter(typeof(SecretKeyValuePair<Point,T>));
				return;
			}

			protected override object? ReadProperty(ref Utf8JsonReader reader, string property, JsonSerializerOptions ops)
			{
				switch(property)
				{
				case "Nonnegative":
					if(!reader.HasNextBool())
						throw new JsonException();

					return reader.GetBoolean();
				case "Items":
					// We need an array opener
					if(!reader.HasNextArrayStart())
						throw new JsonException();
				
					reader.Read();

					// Read the array until we reach the end
					IndexedQueue<SecretKeyValuePair<Point,T>> ret = new IndexedQueue<SecretKeyValuePair<Point,T>>();

					while(!reader.HasNextArrayEnd())
					{
						ret.Enqueue(ItemConverter.Read(ref reader,typeof(SecretKeyValuePair<Point,T>),ops));
						reader.Read();
					}

					return ret;
				default:
					throw new JsonException();
				}
			}

			protected override InfiniteGrid<T> ConstructT(Dictionary<string,object?> properties)
			{
				if(properties.Count != 2)
					throw new JsonException();

				InfiniteGrid<T> ret = new InfiniteGrid<T>((bool)properties["Nonnegative"]!);

				foreach(SecretKeyValuePair<Point,T> kvp in (properties["Items"] as IEnumerable<SecretKeyValuePair<Point,T>>)!)
					ret[kvp.Key] = kvp.Value;

				return ret;
			}

			protected override void WriteProperties(Utf8JsonWriter writer, InfiniteGrid<T> value, JsonSerializerOptions ops)
			{
				writer.WriteBoolean("Nonnegative",value.Nonnegative);
				writer.WriteStartArray("Items");

				foreach(SecretKeyValuePair<Point,T> kvp in value)
					ItemConverter.Write(writer,kvp,ops);

				writer.WriteEndArray();
				return;
			}

			private JsonConverter<SecretKeyValuePair<Point,T>> ItemConverter;
		}
	}
}
