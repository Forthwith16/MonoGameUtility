using Microsoft.Xna.Framework;

namespace GameEngine.DataStructures.Geometry
{
	/// <summary>
	/// A rectangle that supports floating point positions.
	/// </summary>
	public readonly struct FRectangle : IBoundingBox<FRectangle>
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

		/// <summary>
		/// Creats a new rectangle with (X,Y) position offset by <paramref name="offset"/>.
		/// </summary>
		/// <param name="r">The rectangle to offset.</param>
		/// <param name="offset">The offset to apply to the rectangle.</param>
		/// <returns>Returns a new offset rectangle.</returns>
		public static FRectangle operator +(FRectangle r, Vector2 offset) => new FRectangle(r.X + offset.X,r.Y + offset.Y,r.Width,r.Height);

		/// <summary>
		/// Creats a new rectangle with (X,Y) position offset by <paramref name="offset"/>.
		/// </summary>
		/// <param name="offset">The offset to apply to the rectangle.</param>
		/// <param name="r">The rectangle to offset.</param>
		/// <returns>Returns a new offset rectangle.</returns>
		public static FRectangle operator +(Vector2 offset, FRectangle r) => new FRectangle(r.X + offset.X,r.Y + offset.Y,r.Width,r.Height);

		/// <summary>
		/// Creats a new rectangle with (X,Y) position offset by -<paramref name="offset"/>.
		/// </summary>
		/// <param name="r">The rectangle to offset.</param>
		/// <param name="offset">The offset to apply to the rectangle.</param>
		/// <returns>Returns a new offset rectangle.</returns>
		public static FRectangle operator -(FRectangle r, Vector2 offset) => new FRectangle(r.X - offset.X,r.Y - offset.Y,r.Width,r.Height);

		/// <summary>
		/// Creats a new rectangle with -(X,Y) position offset by <paramref name="offset"/>.
		/// </summary>
		/// <param name="offset">The offset to apply to the rectangle.</param>
		/// <param name="r">The rectangle to offset.</param>
		/// <returns>Returns a new offset rectangle.</returns>
		public static FRectangle operator -(Vector2 offset, FRectangle r) => new FRectangle(offset.X - r.X,offset.Y - r.Y,r.Width,r.Height);

		public override readonly int GetHashCode() => (((17 * 23 + X.GetHashCode()) * 23 + Y.GetHashCode()) * 23 + Width.GetHashCode()) * 23 + Height.GetHashCode();

		/// <summary>
		/// Determines if this rectangle contains the point (<paramref name="x"/>,<paramref name="y"/>).
		/// </summary>
		/// <returns>Returns true if this rectangle contains the point (<paramref name="x"/>,<paramref name="y"/>) and false otherwise.</returns>
		public bool Contains(int x, int y) => X <= x && x <= X + Width && Y <= y && y <= Y + Height;

		/// <summary>
		/// Determines if this rectangle contains the point (<paramref name="x"/>,<paramref name="y"/>) and stores the result in <paramref name="result"/>.
		/// </summary>
		public bool Contains(float x, float y) => X <= x && x <= X + Width && Y <= y && y <= Y + Height;
		
		/// <summary>
		/// Determines if this rectangle contains <paramref name="p"/>.
		/// </summary>
		/// <returns>Returns true if this rectangle contains the point <paramref name="p"/> and false otherwise.</returns>
		public bool Contains(Point p) => X <= p.X && p.X <= X + Width && Y <= p.Y && p.Y <= Y + Height;

		/// <summary>
		/// Determines if this rectangle contains the point <paramref name="p"/> and stores the result in <paramref name="result"/>.
		/// </summary>
		public void Contains(ref Point p, out bool result)
		{
			result = X <= p.X && p.X <= X + Width && Y <= p.Y && p.Y <= Y + Height;
			return;
		}

		/// <summary>
		/// Determines if this rectangle contains <paramref name="v"/>.
		/// </summary>
		/// <returns>Returns true if this rectangle contains the point <paramref name="v"/> and false otherwise.</returns>
		public bool Contains(Vector2 v) => X <= v.X && v.X <= X + Width && Y <= v.Y && v.Y <= Y + Height;

		/// <summary>
		/// Determines if this rectangle contains the point <paramref name="v"/> and stores the result in <paramref name="result"/>.
		/// </summary>
		public void Contains(ref Vector2 v, out bool result)
		{
			result = X <= v.X && v.X <= X + Width && Y <= v.Y && v.Y <= Y + Height;
			return;
		}

		/// <summary>
		/// Determines if this rectangle contains the rectangle <paramref name="r"/>.
		/// </summary>
		/// <returns>Returns true if this rectangle contains <paramref name="r"/> and false otherwise.</returns>
		public bool Contains(FRectangle r) => X <= r.X && r.X + r.Width <= X + Width && Y <= r.Y && r.Y + r.Height <= Y + Height;

		/// <summary>
		/// Determines if this rectangle contains the rectangle <paramref name="r"/> and stores the result in <paramref name="result"/>.
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

			return new FRectangle(num,num2,MathF.Max(Right,r.Right) - num,MathF.Max(Top,r.Top) - num2);
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

			return new FRectangle(num,num2,MathF.Max(r1.Right,r2.Right) - num,MathF.Max(r1.Top,r2.Top) - num2);
		}

		/// <summary>
		/// Unions two rectangles together.
		/// </summary>
		/// <param name="r1">The first rectangle to union.</param>
		/// <param name="r2">The second rectangle to union.</param>
		/// <param name="result">The resulting rectangle.</param>
		public static void Union(ref FRectangle r1, ref FRectangle r2, out FRectangle result)
		{
			float rx = Math.Min(r1.X,r2.X);
			float ry = Math.Min(r1.Y,r2.Y);

			result = new FRectangle(rx,ry,MathF.Max(r1.Right,r2.Right) - rx,MathF.Max(r1.Top,r2.Top) - ry);
			return;
		}

		/// <summary>
		/// Determines if this rectangle intersects with <paramref name="r"/>.
		/// </summary>
		/// <returns>Returns true if the two rectangles intersect nontrivially (with nonzero area) and false otherwise.</returns>
		public bool Intersects(FRectangle r) => r.Left < Right && Left < r.Right && r.Bottom < Top && Bottom < r.Top;

		/// <summary>
		/// Determines if this rectangle intersects with <paramref name="r"/> and stores the result in <paramref name="result"/>.
		/// </summary>
		public void Intersects(ref FRectangle r, out bool result)
		{
			result = r.Left < Right && Left < r.Right && r.Bottom < Top && Bottom < r.Top;
			return;
		}

		/// <summary>
		/// Intersects this rectangle with <paramref name="r"/> and returns the result as a new rectangle.
		/// </summary>
		/// <param name="r">The rectangle to intersect with.</param>
		/// <returns>Returns the resulting intersection or the empty rectangle if this rectangle and <paramref name="r"/> do not intersect.</returns>
		public FRectangle Intersect(FRectangle r) => Intersect(this,r);

		/// <summary>
		/// Intersects this rectangle with <paramref name="r"/> and returns the result as a new rectangle.
		/// </summary>
		/// <param name="r">The rectangle to intersect with.</param>
		/// <returns>Returns the resulting intersection or the empty rectangle if this rectangle and <paramref name="r"/> do not intersect.</returns>
		public FRectangle Intersection(FRectangle r) => Intersect(this,r);

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
				float num = MathF.Min(r1.Right,r2.Right);
				float num2 = MathF.Max(r1.X,r2.X);
				float num3 = MathF.Max(r1.Y,r2.Y);
				float num4 = MathF.Min(r1.Top,r2.Top);

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

		public override string ToString() => "(X: " + X + " Y: " + Y + " Width: " + Width + " Height: " + Height + ")";

		/// <summary>
		/// The x position of the rectangle.
		/// </summary>
		public float X
		{get;}

		/// <summary>
		/// The y position of the rectangle.
		/// </summary>
		public float Y
		{get;}

		/// <summary>
		/// The width of the rectangle.
		/// </summary>
		public float Width
		{get;}

		/// <summary>
		/// The height of the rectangle.
		/// </summary>
		public float Height
		{get;}

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
		/// The center of this rectangle.
		/// </summary>
		public Vector2 Center => new Vector2(X + Width / 2.0f,Y + Height / 2.0f);

		/// <summary>
		/// Determines if this rectangle is the empty rectangle.
		/// </summary>
		public bool IsEmpty => Width == 0.0f && Height == 0.0f && X == 0.0f && Y == 0.0f;

		/// <summary>
		/// The (x,y) position of the rectangle.
		/// </summary>
		public Vector2 Location => new Vector2(X,Y);

		/// <summary>
		/// The (width,height) of this rectangle.
		/// </summary>
		public Vector2 Size => new Vector2(Width,Height);

		/// <summary>
		/// The area of this rectangle.
		/// </summary>
		public float Area => Width * Height;

		public float BoxSpace => Area;
		public FRectangle EmptyBox => Empty;

		/// <summary>
		/// The empty rectangle.
		/// </summary>
		public static FRectangle Empty => _er;

		/// <summary>
		/// The empty rectangle.
		/// </summary>
		private static FRectangle _er = new FRectangle(0.0f,0.0f,0.0f,0.0f);
	}
}
