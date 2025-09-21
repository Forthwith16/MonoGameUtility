using GameEngine.DataStructures.Collections;
using GameEngine.Utility.ExtensionMethods.PrimitiveExtensions;
using Microsoft.Xna.Framework;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameEngine.DataStructures.Grid
{
	/// <summary>
	/// A finite grid.
	/// </summary>
	/// <typeparam name="T">The type stored in the grid.</typeparam>
	[JsonConverter(typeof(FGConverterFactory))]
	public class FiniteGrid<T> : IEnumerable<KeyValuePair<Point,T>>
	{
		/// <summary>
		/// Creates a finite grid with the given dimensions.
		/// </summary>
		/// <param name="w">The width.</param>
		/// <param name="h">The height.</param>
		/// <exception cref="ArgumentException">Thrown if <paramref name="w"/> or <paramref name="h"/> is less than 0.</exception>
		public FiniteGrid(int w, int h)
		{
			Values = new T[0,0];
			Nonempty = new bool[0,0];

			Size = new Point(w,h);
			return;
		}

		/// <summary>
		/// Attempts to get something at position (<paramref name="x"/>,<paramref name="y"/>).
		/// </summary>
		/// <param name="x">The x position.</param>
		/// <param name="y">The y position.</param>
		/// <param name="output">The obtained value. This value is default(<typeparamref name="T"/>) when the return value is false.</param>
		/// <returns>Returns true if something was obtained and false otherwise.</returns>
		public bool TryGet(int x, int y, [MaybeNullWhen(false)] out T output)
		{
			if(Empty(x,y))
			{
				output = default(T);
				return false;
			}

			output = this[x,y];
			return true;
		}

		/// <summary>
		/// Attempts to get something at position <paramref name="p"/>.
		/// </summary>
		/// <param name="p">The position.</param>
		/// <param name="output">The obtained value. This value is default(<typeparamref name="T"/>) when the return value is false.</param>
		/// <returns>Returns true if something was obtained and false otherwise.</returns>
		public bool TryGet(Point p, [MaybeNullWhen(false)] out T output)
		{
			if(Empty(p))
			{
				output = default(T);
				return false;
			}

			output = this[p];
			return true;
		}

		/// <summary>
		/// Determines if there is something at position (<paramref name="x"/>,<paramref name="y"/>).
		/// </summary>
		/// <param name="x">The x position.</param>
		/// <param name="y">The y position.</param>
		/// <returns>Returns true if there is nothing at the given location and false otherwise.</returns>
		public bool Empty(int x, int y) => !Nonempty[x,y];

		/// <summary>
		/// Determines if there is something at position <paramref name="p"/>.
		/// </summary>
		/// <param name="p">The position.</param>
		/// <returns>Returns true if there is nothing at the given location and false otherwise.</returns>
		public bool Empty(Point p) => !Nonempty[p.X,p.Y];

		/// <summary>
		/// Determines if there is something at position (<paramref name="x"/>,<paramref name="y"/>).
		/// </summary>
		/// <param name="x">The x position.</param>
		/// <param name="y">The y position.</param>
		/// <returns>Returns true if there is something at the given location and false otherwise.</returns>
		public bool NotEmpty(int x, int y) => Nonempty[x,y];

		/// <summary>
		/// Determines if there is something at position <paramref name="p"/>.
		/// </summary>
		/// <param name="p">The position.</param>
		/// <returns>Returns true if there is something at the given location and false otherwise.</returns>
		public bool NotEmpty(Point p) => Nonempty[p.X,p.Y];

		/// <summary>
		/// Removes a single position.
		/// </summary>
		/// <param name="x">The x position.</param>
		/// <param name="y">The y position.</param>
		/// <remarks>This does not release references, if important.</remarks>
		public void Remove(int x, int y)
		{
			Nonempty[x,y] = false;
			return;
		}

		/// <summary>
		/// Removes a single position.
		/// </summary>
		/// <param name="p">The position.</param>
		/// <remarks>This does not release references, if important.</remarks>
		public void Remove(Point p)
		{
			Nonempty[p.X,p.Y] = false;
			return;
		}

		/// <summary>
		/// Clears the grid.
		/// </summary>
		/// <remarks>This does not release references, if important.</remarks>
		public void Clear()
		{
			for(int i = 0;i < Width;i++)
				for(int j = 0;j < Height;j++)
					Remove(i,j);

			return;
		}

		/// <summary>
		/// Performs an operation on every nonempty location of this grid.
		/// </summary>
		/// <param name="operation">The operation to perform.</param>
		/// <remarks>This iterates over the grid from left to right, bottom to top.</remarks>
		public void Batch(GridBatcher<T> operation)
		{
			for(int y = 0;y < Height;y++)
				for(int x = 0;x < Width;x++)
					operation(this[x,y],x,y);

			return;
		}

		public IEnumerator<KeyValuePair<Point,T>> GetEnumerator() => new FGE(this);
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public override string ToString()
		{
			string[,] strs = new string[Width,Height];
			int[,] lens = new int[Width,Height];

			int max = 0;

			for(int x = 0;x < Width;x++)
				for(int y = 0;y<Height;y++)
				{
					strs[x,y] = Empty(x,y) ? "" : this[x,y]?.ToString() ?? "";
					lens[x,y] = strs[x,y].Length;

					max = Math.Max(lens[x,y],max);
				}

			StringBuilder sb = new StringBuilder();

			for(int y = Height - 1;y >= 0;y--)
			{
				for(int x = 0;x < Width;x++)
					if(x == Width - 1)
						sb.Append(strs[x,y]);
					else
						sb.Append(strs[x,y].PadRight(1 + max));
				
				if(y > 0)
					sb.Append("\n");
			}
			
			return sb.ToString();
		}

		#region The Grid
		/// <summary>
		/// Get/sets the value of this grid at (<paramref name="x"/>,<paramref name="y"/>).
		/// </summary>
		/// <param name="x">The x position.</param>
		/// <param name="y">The y position.</param>
		/// <returns>Returns the value at the given position.</returns>
		/// <exception cref="ArgumentException">Thrown on get if the position is empty.</exception>
		/// <exception cref="IndexOutOfRangeException">Thrown if <paramref name="x"/> is less than 0 or at least <see cref="Width"/> or if <paramref name="y"/> is less than 0 or at least <see cref="Height"/>.</exception>
		public T this[int x, int y]
		{
			get => Nonempty[x,y] ? Values[x,y] : throw new ArgumentException();
			
			set
			{
				Values[x,y] = value;
				Nonempty[x,y] = true;

				return;
			}
		}

		/// <summary>
		/// Gets/sets the value of this grid at <paramref name="p"/>.
		/// </summary>
		/// <param name="p">The position.</param>
		/// <returns>Returns the value at the given position.</returns>
		/// <exception cref="IndexOutOfRangeException">Thrown if <paramref name="p"/> lies outside of the grid.</exception>
		public T this[Point p]
		{
			get => this[p.X,p.Y];
			set => this[p.X,p.Y] = value;
		}

		/// <summary>
		/// The grid values.
		/// </summary>
		protected T[,] Values
		{get; set;}

		/// <summary>
		/// Indicates if <see cref="Values"/> is empty (false) or occupied (true) at a certain location.
		/// </summary>
		protected bool[,] Nonempty
		{get; set;}
		#endregion

		#region Dimensions
		/// <summary>
		/// The width of the grid.
		/// </summary>
		/// <exception cref="ArgumentException">Thrown if attempting to set this to a length less than 0.</exception>
		public int Width
		{
			get => _s.X;
			set => Size = new Point(value,_s.Y);
		}

		/// <summary>
		/// The height of the grid.
		/// </summary>
		/// <exception cref="ArgumentException">Thrown if attempting to set this to a length less than 0.</exception>
		public int Height
		{
			get => _s.Y;
			set => Size = new Point(_s.X,value);
		}

		/// <summary>
		/// The size of the grid.
		/// This is always equal to (Width,Height).
		/// </summary>
		/// <exception cref="ArgumentException">Thrown if attempting to set a dimension to a length less than 0.</exception>
		public Point Size
		{
			get => _s;

			set
			{
				if(value.X < 0 || value.Y < 0)
					throw new ArgumentException();

				Resize(value.X,value.Y);
				_s = value;
				
				return;
			}
		}

		private Point _s;

		/// <summary>
		/// Resizes the grid to the given dimensions.
		/// When shrinking, it will always clobber data from higher values before smaller values.
		/// </summary>
		/// <param name="w">The new width.</param>
		/// <param name="h">The new height.</param>
		/// <remarks>The old value of <see cref="Size"/> must be preserved for this to work properly.</remarks>
		protected void Resize(int w, int h)
		{
			T[,] t_temp = new T[w,h];
			bool[,] b_temp = new bool[w,h];

			int x_max = Math.Min(w,Width);
			int y_max = Math.Min(h,Height);

			for(int i = 0;i < x_max;i++)
				for(int j = 0;j < y_max;j++)
				{
					t_temp[i,j] = Values[i,j];
					b_temp[i,j] = Nonempty[i,j];
				}

			Nonempty = b_temp;
			Values = t_temp;

			return;
		}
		#endregion

		/// <summary>
		/// Enumeartes a finite grid.
		/// </summary>
		private sealed class FGE : IEnumerator<KeyValuePair<Point,T>>
		{
			/// <summary>
			/// Creates a new enumerator.
			/// </summary>
			/// <param name="grid">The grid to enumerate.</param>
			public FGE(FiniteGrid<T> grid)
			{
				TheGrid = grid;
				Reset();

				return;
			}

			public void Dispose()
			{return;}

			public bool MoveNext()
			{
				if(CurY >= TheGrid.Height)
					return false;

				if(++CurX >= TheGrid.Width)
				{
					CurX = 0;
					CurY++;
				}

				if(CurY >= TheGrid.Height)
					return false;

				if(TheGrid.Empty(CurX,CurY))
					return MoveNext();

				Current = new KeyValuePair<Point,T>(new Point(CurX,CurY),TheGrid[CurX,CurY]);
				return true;
			}

			public void Reset()
			{
				CurX = TheGrid.Width - 1;
				CurY = -1;

				return;
			}

			public KeyValuePair<Point,T> Current
			{get; private set;}

			object IEnumerator.Current => Current;

			/// <summary>
			/// The current X position.
			/// </summary>
			private int CurX;

			/// <summary>
			/// The current Y position.
			/// </summary>
			private int CurY;

			/// <summary>
			/// The grid to enumerate.
			/// </summary>
			private FiniteGrid<T> TheGrid;
		}
	}

	/// <summary>
	/// A delegate meant for performing batch operations on grids.
	/// </summary>
	/// <typeparam name="T">The generic type of the grid.</typeparam>
	/// <param name="item">The item at position (<paramref name="x"/>,<paramref name="y"/>).</param>
	/// <param name="x">The x position of the current item.</param>
	/// <param name="y">The y position of the current item.</param>
	public delegate void GridBatcher<T>(T item, int x, int y);

	/// <summary>
	/// Creates JSON converters for finite grids.
	/// </summary>
	public sealed class FGConverterFactory : JsonConverterFactory
	{
		public override bool CanConvert(Type t) => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(FiniteGrid<>);
		public override JsonConverter? CreateConverter(Type t, JsonSerializerOptions ops) => (JsonConverter?)Activator.CreateInstance(typeof(FGConverter<>).MakeGenericType(t.GetGenericArguments()),BindingFlags.Instance | BindingFlags.Public,null,new object?[] {ops},null);

		/// <summary>
		/// Converts finite grids to/from JSON.
		/// </summary>
		/// <typeparam name="T">The type stored in the grid.</typeparam>
		private sealed class FGConverter<T> : JsonConverter<FiniteGrid<T>>
		{
			/// <summary>
			/// Creates a new finite grid.
			/// </summary>
			/// <param name="ops">The serialization options.</param>
			public FGConverter(JsonSerializerOptions ops)
			{
				TType = typeof(T);
				DataConverter = (JsonConverter<T>)ops.GetConverter(TType);

				return;
			}

			public override FiniteGrid<T> Read(ref Utf8JsonReader reader, Type type_to_convert, JsonSerializerOptions ops)
			{
				// We start with the object opening
				if(!reader.HasNextObjectStart())
					throw new JsonException();
			
				reader.Read();

				// We'll need to track what properties we've already done, though this is not strictly necessary
				HashSet<string> processed = new HashSet<string>();

				// Create a place to store the stuff we read
				int w = 0;
				int h = 0;

				IEnumerable<KeyValuePair<Point,T>>? items = null;

				// Loop until we reach the end of the object
				while(!reader.HasNextObjectEnd())
				{
					if(!reader.HasNextProperty())
						throw new JsonException();

					string property_name = reader.GetString()!;
					reader.Read();

					if(processed.Contains(property_name))
						throw new JsonException();

					switch(property_name)
					{
					case "Width":
						if(!reader.HasNextNumber())
							throw new JsonException();

						w = reader.GetInt32();
						break;
					case "Height":
						if(!reader.HasNextNumber())
							throw new JsonException();

						h = reader.GetInt32();
						break;
					case "Items":
						items = ReadItems(ref reader,ops);
						break;
					default:
						throw new JsonException();
					}

					processed.Add(property_name);
					reader.Read();
				}

				// Check that we read everything we need
				if(processed.Count != 3)
					throw new JsonException();

				FiniteGrid<T> ret = new FiniteGrid<T>(w,h);

				foreach(KeyValuePair<Point,T> kvp in items!)
					ret[kvp.Key] = kvp.Value;

				return ret;
			}

			private IEnumerable<KeyValuePair<Point,T>> ReadItems(ref Utf8JsonReader reader, JsonSerializerOptions ops)
			{
				if(!reader.HasNextArrayStart())
					throw new JsonException();

				reader.Read();

				IndexedQueue<KeyValuePair<Point,T>> ret = new IndexedQueue<KeyValuePair<Point,T>>();

				while(!reader.HasNextArrayEnd())
				{
					ret.Add(ReadItem(ref reader,ops));
					reader.Read();
				}

				return ret;
			}

			private KeyValuePair<Point,T> ReadItem(ref Utf8JsonReader reader, JsonSerializerOptions ops)
			{
				// We start with the object opening
				if(!reader.HasNextObjectStart())
					throw new JsonException();
				
				reader.Read();

				// We'll need to track what properties we've already done, though this is not strictly necessary
				HashSet<string> processed = new HashSet<string>();

				// Create a place to store the stuff we read
				int x = 0;
				int y = 0;

				T? value = default(T);

				// Loop until we reach the end of the object
				while(!reader.HasNextObjectEnd())
				{
					if(!reader.HasNextProperty())
						throw new JsonException();

					string property_name = reader.GetString()!;
					reader.Read();

					if(processed.Contains(property_name))
						throw new JsonException();

					switch(property_name)
					{
					case "X":
						if(!reader.HasNextNumber())
							throw new JsonException();

						x = reader.GetInt32();
						break;
					case "Y":
						if(!reader.HasNextNumber())
							throw new JsonException();

						y = reader.GetInt32();
						break;
					case "Value":
						value = DataConverter.Read(ref reader,typeof(T),ops);
						break;
					default:
						throw new JsonException();
					}

					processed.Add(property_name);
					reader.Read();
				}

				// Check that we read everything we need
				if(processed.Count != 3)
					throw new JsonException();

				return new KeyValuePair<Point,T>(new Point(x,y),value!); // Whatever value is, it should be correct
			}

			public override void Write(Utf8JsonWriter writer, FiniteGrid<T> value, JsonSerializerOptions ops)
			{
				writer.WriteStartObject();
				
				writer.WriteNumber("Width",value.Width);
				writer.WriteNumber("Height",value.Height);

				writer.WriteStartArray("Items");

				foreach(KeyValuePair<Point,T> kvp in value)
				{
					writer.WriteStartObject();

					writer.WriteNumber("X",kvp.Key.X);	
					writer.WriteNumber("Y",kvp.Key.Y);	

					writer.WritePropertyName("Value");
					DataConverter.Write(writer,kvp.Value,ops);

					writer.WriteEndObject();
				}

				writer.WriteEndArray();

				writer.WriteEndObject();
				return;
			}

			/// <summary>
			/// Converts <typeparamref name="T"/> types into JSON.
			/// </summary>
			private JsonConverter<T> DataConverter
			{get;}

			/// <summary>
			/// <typeparamref name="T"/> as a Type.
			/// </summary>
			private Type TType
			{get;}
		}
	}
}
