using Microsoft.Xna.Framework;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace GameEngine.DataStructures.Geometry
{
	/// <summary>
	/// A rectangle that supports floating point positions.
	/// </summary>
	[DataContract]
	[DebuggerDisplay("{DebugDisplayString,nq}")]
	public struct FRectangle : IEquatable<FRectangle>
	{
		/// <summary>
		/// Creates a new rectangle.
		/// </summary>
		/// <param name="x">The x position of the rectangle.</param>
		/// <param name="y">The y position of the rectangle.</param>
		/// <param name="width">The width of the rectangle.</param>
		/// <param name="height">The height of the rectangle.</param>
		public FRectangle(int x, int y, int width, int height)
		{
			X = x;
			Y = y;

			Width = width;
			Height = height;
			
			return;
		}

		/// <summary>
		/// Creates a new rectangle.
		/// </summary>
		/// <param name="x">The x position of the rectangle.</param>
		/// <param name="y">The y position of the rectangle.</param>
		/// <param name="width">The width of the rectangle.</param>
		/// <param name="height">The height of the rectangle.</param>
		public FRectangle(float x, float y, float width, float height)
		{
			X = x;
			Y = y;

			Width = width;
			Height = height;
			
			return;
		}

		/// <summary>
		/// Creates a new rectangle.
		/// </summary>
		/// <param name="location">The position of the rectangle.</param>
		/// <param name="size">The size of the rectangle.</param>
		public FRectangle(Point location, Point size)
		{
			X = location.X;
			Y = location.Y;

			Width = size.X;
			Height = size.Y;
			
			return;
		}

		/// <summary>
		/// Creates a new rectangle.
		/// </summary>
		/// <param name="location">The position of the rectangle.</param>
		/// <param name="size">The size of the rectangle.</param>
		public FRectangle(Vector2 location, Vector2 size)
		{
			X = location.X;
			Y = location.Y;

			Width = size.X;
			Height = size.Y;
			
			return;
		}

		/// <summary>
		/// Converts a Rectangle into an FRectangle.
		/// </summary>
		public static implicit operator FRectangle(Rectangle r) => new FRectangle(r.X,r.Y,r.Width,r.Height);

		/// <summary>
		/// Converts an FRectangle into a Rectangle.
		/// </summary>
		public static explicit operator Rectangle(FRectangle r) => new Rectangle((int)r.X,(int)r.Y,(int)MathF.Ceiling(r.Width),(int)MathF.Ceiling(r.Height));

		public static bool operator ==(FRectangle a, FRectangle b) => a.X == b.X && a.Y == b.Y && a.Width == b.Width && a.Height == b.Height;

		public static bool operator !=(FRectangle a, FRectangle b) => !(a == b);

		public override readonly bool Equals(object? obj) => obj is FRectangle r && this == r;

		public readonly bool Equals(FRectangle other) => this == other;

		public override readonly int GetHashCode() => (((17 * 23 + X.GetHashCode()) * 23 + Y.GetHashCode()) * 23 + Width.GetHashCode()) * 23 + Height.GetHashCode();

		/// <summary>
		/// Inflates the rectangle by the provided amount.
		/// This inflation decreases the (X,Y) position by (<paramref name="x_amount"/>,<paramref name="y_amount"/>) and increases the dimensions by twice this amount.
		/// In short, it adds a horizontal and vertical padding around this rectangle.
		/// </summary>
		/// <param name="x_amount">The horizontal padding to add.</param>
		/// <param name="y_amount">The vertical padding to add.</param>
		public void Inflate(int horizontalAmount, int verticalAmount)
		{
			X -= horizontalAmount;
			Y -= verticalAmount;

			Width += horizontalAmount * 2;
			Height += verticalAmount * 2;
			
			return;
		}

		/// <summary>
		/// Inflates the rectangle by the provided amount.
		/// This inflation decreases the (X,Y) position by (<paramref name="x_amount"/>,<paramref name="y_amount"/>) and increases the dimensions by twice this amount.
		/// In short, it adds a horizontal and vertical padding around this rectangle.
		/// </summary>
		/// <param name="x_amount">The horizontal padding to add.</param>
		/// <param name="y_amount">The vertical padding to add.</param>
		public void Inflate(float x_amount, float y_amount)
		{
			X -= x_amount;
			Y -= y_amount;

			Width += x_amount * 2.0f;
			Height += y_amount * 2.0f;
			
			return;
		}

		/// <summary>
		/// Moves this rectangle by (<paramref name="offset_x"/>,<paramref name="offset_y"/>).
		/// </summary>
		public void Offset(int offset_x, int offset_y)
		{
			X += offset_x;
			Y += offset_y;
			
			return;
		}

		/// <summary>
		/// Moves this rectangle by (<paramref name="offset_x"/>,<paramref name="offset_y"/>).
		/// </summary>
		public void Offset(float offset_x, float offset_y)
		{
			X += (int)offset_x;
			Y += (int)offset_y;
			
			return;
		}

		/// <summary>
		/// Moves this rectangle by <paramref name="amount"/>.
		/// </summary>
		public void Offset(Point amount)
		{
			X += amount.X;
			Y += amount.Y;
			
			return;
		}

		/// <summary>
		/// Moves this rectangle by <paramref name="amount"/>.
		/// </summary>
		public void Offset(Vector2 amount)
		{
			X += amount.X;
			Y += amount.Y;
			
			return;
		}

		/// <summary>
		/// Determines if this rectangle contains the point (<paramref name="x"/>,<paramref name="y"/>).
		/// </summary>
		/// <returns>Returns true if this rectangle contains the point (<paramref name="x"/>,<paramref name="y"/>) and false otherwise.</returns>
		public bool Contains(int x, int y) => X <= x && x < X + Width && Y <= y && y < Y + Height;

		/// <summary>
		/// Determines if this rectangle contains the point (<paramref name="x"/>,<paramref name="y"/>) and stores the result in <paramref name="result"/>.
		/// </summary>
		public bool Contains(float x, float y) => X <= x && x < X + Width && Y <= y && y < Y + Height;
		
		/// <summary>
		/// Determines if this rectangle contains <paramref name="p"/>.
		/// </summary>
		/// <returns>Returns true if this rectangle contains the point <paramref name="p"/> and false otherwise.</returns>
		public bool Contains(Point p) => X <= p.X && p.X < X + Width && Y <= p.Y && p.Y < Y + Height;

		/// <summary>
		/// Determines if this rectangle contains the point <paramref name="p"/> and stores the result in <paramref name="result"/>.
		/// </summary>
		public void Contains(ref Point p, out bool result)
		{
			result = X <= p.X && p.X < X + Width && Y <= p.Y && p.Y < Y + Height;
			return;
		}

		/// <summary>
		/// Determines if this rectangle contains <paramref name="v"/>.
		/// </summary>
		/// <returns>Returns true if this rectangle contains the point <paramref name="v"/> and false otherwise.</returns>
		public bool Contains(Vector2 v) => X <= v.X && v.X < X + Width && Y <= v.Y && v.Y < Y + Height;

		/// <summary>
		/// Determines if this rectangle contains the point <paramref name="v"/> and stores the result in <paramref name="result"/>.
		/// </summary>
		public void Contains(ref Vector2 v, out bool result)
		{
			result = X <= v.X && v.X < X + Width && Y <= v.Y && v.Y < Y + Height;
			return;
		}

		/// <summary>
		/// Determines if this rectangle contains the rectangle <paramref name="r"/>.
		/// </summary>
		/// <returns>Returns true if this rectangle contains <paramref name="r"/> and false otherwise.</returns>
		public bool Contains(Rectangle r) => X <= r.X && r.X + r.Width <= X + Width && Y <= r.Y && r.Y + r.Height <= Y + Height;

		/// <summary>
		/// Determines if this rectangle contains the rectangle <paramref name="r"/> and stores the result in <paramref name="rresult"/>.
		/// </summary>
		public void Contains(ref FRectangle r, out bool result)
		{
			result = X <= r.X && r.X + r.Width <= X + Width && Y <= r.Y && r.Y + r.Height <= Y + Height;
			return;
		}

		/// <summary>
		/// Unions this rectangle with <paramref name="r"/>.
		/// </summary>
		/// <returns>Returns the union of this with <paramref name="r"/> as a new rectangle.</returns>
		public FRectangle Union(FRectangle r)
		{
			float num = MathF.Min(X,r.X);
			float num2 = MathF.Min(Y,r.Y);

			return new FRectangle(num,num2,MathF.Max(Right,r.Right) - num,MathF.Max(Bottom,r.Bottom) - num2);
		}

		/// <summary>
		/// Unions two rectangles together.
		/// </summary>
		/// <param name="r1">The first rectangle to union.</param>
		/// <param name="r2">The second rectangle to union.</param>
		/// <returns>Returns the union of <paramref name="r1"/> and <paramref name="r2"/>.</returns>
		public static FRectangle Union(FRectangle r1, FRectangle r2)
		{
			float num = MathF.Min(r1.X,r2.X);
			float num2 = MathF.Min(r1.Y,r2.Y);

			return new FRectangle(num,num2,MathF.Max(r1.Right,r2.Right) - num,MathF.Max(r1.Bottom,r2.Bottom) - num2);
		}

		/// <summary>
		/// Unions two rectangles together.
		/// </summary>
		/// <param name="r1">The first rectangle to union.</param>
		/// <param name="r2">The second rectangle to union.</param>
		/// <param name="result">The resulting rectangle.</param>
		public static void Union(ref FRectangle r1, ref FRectangle r2, out FRectangle result)
		{
			result.X = Math.Min(r1.X,r2.X);
			result.Y = Math.Min(r1.Y,r2.Y);

			result.Width = Math.Max(r1.Right,r2.Right) - result.X;
			result.Height = Math.Max(r1.Bottom,r2.Bottom) - result.Y;

			return;
		}

		/// <summary>
		/// Determines if this rectangle intersects with <paramref name="r"/>.
		/// </summary>
		/// <returns>Returns true if the two rectangles intersect nontrivially (with nonzero area) and false otherwise.</returns>
		public bool Intersects(FRectangle r) => r.Left < Right && Left < r.Right && r.Top < Bottom && Top < r.Bottom;

		/// <summary>
		/// Determines if this rectangle intersects with <paramref name="r"/> and stores the result in <paramref name="result"/>.
		/// </summary>
		public void Intersects(ref FRectangle r, out bool result)
		{
			result = r.Left < Right && Left < r.Right && r.Top < Bottom && Top < r.Bottom;
			return;
		}

		/// <summary>
		/// Intersects this rectangle with <paramref name="r"/> and returns the result as a new rectangle.
		/// </summary>
		/// <param name="r">The rectangle to intersect with.0</param>
		/// <returns>Returns the resuting intersection or the empty rectangle if this rectangle and <paramref name="r"/> do not intersect.</returns>
		public FRectangle Intersect(FRectangle r) => Intersect(this,r);

		/// <summary>
		/// Computes the intersection of <paramref name="r1"/> and <paramref name="r2"/> and returns the result as a new rectangle.
		/// </summary>
		public static FRectangle Intersect(FRectangle r1, FRectangle r2)
		{
			Intersect(ref r1,ref r2,out var result);
			return result;
		}

		/// <summary>
		/// Intersects <paramref name="r1"/> with <paramref name="r2"/> and stores the result in <paramref name="result"/>.
		/// </summary>
		public static void Intersect(ref FRectangle r1, ref FRectangle r2, out FRectangle result)
		{
			if(r1.Intersects(r2))
			{
				float num = MathF.Min(r1.X + r1.Width,r2.X + r2.Width);
				float num2 = MathF.Max(r1.X,r2.X);
				float num3 = MathF.Max(r1.Y,r2.Y);
				float num4 = MathF.Min(r1.Y + r1.Height,r2.Y + r2.Height);

				result = new FRectangle(num2,num3,num - num2,num4 - num3);
			}
			else
				result = Empty;
			
			return;
		}

		/// <summary>
		/// Deconstructs this rectangle's position and dimensions into the provided parameters.
		/// </summary>
		public void Deconstruct(out float x, out float y, out float width, out float height)
		{
			x = X;
			y = Y;

			width = Width;
			height = Height;

			return;
		}

		public override string ToString() => "{X:" + X + " Y:" + Y + " Width:" + Width + " Height:" + Height + "}";

		/// <summary>
		/// The x position of the rectangle.
		/// </summary>
		[DataMember]
		public float X;

		/// <summary>
		/// The y position of the rectangle.
		/// </summary>
		[DataMember]
		public float Y;

		/// <summary>
		/// The width of the rectangle.
		/// </summary>
		[DataMember]
		public float Width;

		/// <summary>
		/// The height of the rectangle.
		/// </summary>
		[DataMember]
		public float Height;

		/// <summary>
		/// The left position of the rectangle.
		/// This is equal to X.
		/// </summary>
		public float Left => X;

		/// <summary>
		/// The right position of the rectangle.
		/// This is equal to X + Width.
		/// </summary>
		public float Right => X + Width;

		/// <summary>
		/// The bottom position of the rectangle.
		/// This is equal to Y.
		/// </summary>
		public float Bottom => Y;

		/// <summary>
		/// The top position of the rectangle.
		/// This is equal to Y + Height.
		/// </summary>
		public float Top => Y + Height;

		/// <summary>
		/// The empty rectangle.
		/// </summary>
		public static FRectangle Empty => emptyRectangle;

		/// <summary>
		/// The empty rectangle.
		/// </summary>
		private static FRectangle emptyRectangle = new FRectangle(0.0f,0.0f,0.0f,0.0f);

		/// <summary>
		/// Determines if this rectangle is the empty rectangle.
		/// </summary>
		public bool IsEmpty => Width == 0.0f && Height == 0.0f && X == 0.0f && Y == 0.0f;

		/// <summary>
		/// The (x,y) position of the rectangle.
		/// </summary>
		public Vector2 Location
		{
			get => new Vector2(X,Y);

			set
			{
				X = value.X;
				Y = value.Y;

				return;
			}
		}

		/// <summary>
		/// The (width,height) of this rectangle.
		/// </summary>
		public Vector2 Size
		{
			get => new Vector2(Width,Height);

			set
			{
				Width = value.X;
				Height = value.Y;

				return;
			}
		}

		/// <summary>
		/// The center of this rectangle.
		/// </summary>
		public Vector2 Center => new Vector2(X + Width / 2.0f,Y + Height / 2.0f);

		/// <summary>
		/// A mysterious string copied from MonoGame's Rectangle class.
		/// </summary>
		internal string DebugDisplayString => X + "  " + Y + "  " + Width + "  " + Height;
	}
}
