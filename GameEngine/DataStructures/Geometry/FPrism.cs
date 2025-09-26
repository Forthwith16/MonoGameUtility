using GameEngine.Utility.ExtensionMethods.PrimitiveExtensions;
using GameEngine.Utility.Serialization;
using Microsoft.Xna.Framework;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameEngine.DataStructures.Geometry
{
	/// <summary>
	/// Represents a three dimensional box, the spacial analog of FRectangle.
	/// </summary>
	[JsonConverter(typeof(JsonFPrismConverter))]
	public readonly struct FPrism : IBoundingBox<FPrism>
	{
		/// <summary>
		/// Creates a new prism.
		/// </summary>
		/// <param name="x">The x position of the prism.</param>
		/// <param name="y">The y position of the prism.</param>
		/// <param name="z">The z position of the prism.</param>
		/// <param name="width">The width of the prism.</param>
		/// <param name="height">The height of the prism.</param>
		/// <param name="depth">The depth of the prism.</param>
		public FPrism(int x, int y, int z, int width, int height, int depth)
		{
			X = x;
			Y = y;
			Z = z;

			Width = width;
			Height = height;
			Depth = depth;
			
			return;
		}

		/// <summary>
		/// Creates a new prism.
		/// </summary>
		/// <param name="x">The x position of the prism.</param>
		/// <param name="y">The y position of the prism.</param>
		/// <param name="z">The z position of the prism.</param>
		/// <param name="width">The width of the prism.</param>
		/// <param name="height">The height of the prism.</param>
		/// <param name="depth">The depth of the prism.</param>
		public FPrism(float x, float y, float z, float width, float height, float depth)
		{
			X = x;
			Y = y;
			Z = z;

			Width = width;
			Height = height;
			Depth = depth;
			
			return;
		}

		/// <summary>
		/// Creates a new prism.
		/// </summary>
		/// <param name="location">The position of the prism.</param>
		/// <param name="size">The size of the prism.</param>
		public FPrism(Vector3 location, Vector3 size)
		{
			X = location.X;
			Y = location.Y;
			Z = location.Z;

			Width = size.X;
			Height = size.Y;
			Depth = size.Z;
			
			return;
		}

		public static bool operator ==(FPrism a, FPrism b) => a.X == b.X && a.Y == b.Y && a.Z == b.Z && a.Width == b.Width && a.Height == b.Height && a.Depth == b.Depth;

		public static bool operator !=(FPrism a, FPrism b) => !(a == b);

		public override readonly bool Equals(object? obj) => obj is FPrism r && this == r;

		public readonly bool Equals(FPrism other) => this == other;

		/// <summary>
		/// Creats a new prism with (X,Y,Z) position offset by <paramref name="offset"/>.
		/// </summary>
		/// <param name="r">The prism to offset.</param>
		/// <param name="offset">The offset to apply to the prism.</param>
		/// <returns>Returns a new offset prism.</returns>
		public static FPrism operator +(FPrism r, Vector3 offset) => new FPrism(r.X + offset.X,r.Y + offset.Y,r.Z + offset.Z,r.Width,r.Height,r.Depth);

		/// <summary>
		/// Creats a new prism with (X,Y,Z) position offset by <paramref name="offset"/>.
		/// </summary>
		/// <param name="offset">The offset to apply to the prism.</param>
		/// <param name="r">The prism to offset.</param>
		/// <returns>Returns a new offset prism.</returns>
		public static FPrism operator +(Vector3 offset, FPrism r) => new FPrism(r.X + offset.X,r.Y + offset.Y,r.Z + offset.Z,r.Width,r.Height,r.Depth);

		/// <summary>
		/// Creats a new prism with (X,Y,Z) position offset by -<paramref name="offset"/>.
		/// </summary>
		/// <param name="r">The prism to offset.</param>
		/// <param name="offset">The offset to apply to the prism.</param>
		/// <returns>Returns a new offset prism.</returns>
		public static FPrism operator -(FPrism r, Vector3 offset) => new FPrism(r.X - offset.X,r.Y - offset.Y,r.Z - offset.Z,r.Width,r.Height,r.Depth);

		/// <summary>
		/// Creats a new prism with -(X,Y,Z) position offset by <paramref name="offset"/>.
		/// </summary>
		/// <param name="offset">The offset to apply to the prism.</param>
		/// <param name="r">The prism to offset.</param>
		/// <returns>Returns a new offset prism.</returns>
		public static FPrism operator -(Vector3 offset, FPrism r) => new FPrism(offset.X - r.X,offset.Y - r.Y,offset.Z - r.Z,r.Width,r.Height,r.Depth);

		public override readonly int GetHashCode() => (((((17 * 23 + X.GetHashCode()) * 23 + Y.GetHashCode()) * 23 + Z.GetHashCode()) * 23 + Width.GetHashCode()) * 23 + Height.GetHashCode()) * 23 + Depth.GetHashCode();

		/// <summary>
		/// Determines if this prism contains the point (<paramref name="x"/>,<paramref name="y"/>,<paramref name="z"/>).
		/// </summary>
		/// <returns>Returns true if this prism contains the point (<paramref name="x"/>,<paramref name="y"/>,<paramref name="z"/>) and false otherwise.</returns>
		public bool Contains(int x, int y, int z) => X <= x && x <= X + Width && Y <= y && y <= Y + Height && Z <= z && z <= Z + Depth;

		/// <summary>
		/// Determines if this prism contains the point (<paramref name="x"/>,<paramref name="y"/>,<paramref name="z"/>) and stores the result in <paramref name="result"/>.
		/// </summary>
		public bool Contains(float x, float y, float z) => X <= x && x <= X + Width && Y <= y && y <= Y + Height && Z <= z && z <= Z + Depth;
		
		/// <summary>
		/// Determines if this prism contains <paramref name="v"/>.
		/// </summary>
		/// <returns>Returns true if this prism contains the point <paramref name="v"/> and false otherwise.</returns>
		public bool Contains(Vector3 v) => X <= v.X && v.X <= X + Width && Y <= v.Y && v.Y <= Y + Height && Z <= v.Z && v.Z <= Z + Depth;

		/// <summary>
		/// Determines if this prism contains the point <paramref name="v"/> and stores the result in <paramref name="result"/>.
		/// </summary>
		public void Contains(ref Vector3 v, out bool result)
		{
			result = X <= v.X && v.X <= X + Width && Y <= v.Y && v.Y <= Y + Height && Z <= v.Z && v.Z <= Z + Depth;
			return;
		}

		/// <summary>
		/// Determines if this prism contains the prism <paramref name="r"/>.
		/// </summary>
		/// <returns>Returns true if this prism contains <paramref name="r"/> and false otherwise.</returns>
		public bool Contains(FPrism r) => X <= r.X && r.X + r.Width <= X + Width && Y <= r.Y && r.Y + r.Height <= Y + Height && Z <= r.Z && r.Z <= Z + Depth;

		/// <summary>
		/// Determines if this prism contains the rectangle <paramref name="r"/> and stores the result in <paramref name="result"/>.
		/// </summary>
		public void Contains(ref FPrism r, out bool result)
		{
			result = X <= r.X && r.X + r.Width <= X + Width && Y <= r.Y && r.Y + r.Height <= Y + Height && Z <= r.Z && r.Z <= Z + Depth;
			return;
		}

		/// <summary>
		/// Unions this prism with <paramref name="r"/>.
		/// </summary>
		/// <returns>Returns the union of this with <paramref name="r"/> as a new prism.</returns>
		public FPrism Union(FPrism r)
		{
			float num = MathF.Min(X,r.X);
			float num2 = MathF.Min(Y,r.Y);
			float num3 = MathF.Min(Z,r.Z);

			return new FPrism(num,num2,num3,MathF.Max(Right,r.Right) - num,MathF.Max(Top,r.Top) - num2,MathF.Max(Far,r.Far) - num3);
		}

		/// <summary>
		/// Unions two prisms together.
		/// </summary>
		/// <param name="r1">The first prism to union.</param>
		/// <param name="r2">The second prism to union.</param>
		/// <returns>Returns the union of <paramref name="r1"/> and <paramref name="r2"/>.</returns>
		public static FPrism Union(FPrism r1, FPrism r2)
		{
			float num = MathF.Min(r1.X,r2.X);
			float num2 = MathF.Min(r1.Y,r2.Y);
			float num3 = MathF.Min(r1.Z,r2.Z);

			return new FPrism(num,num2,num3,MathF.Max(r1.Right,r2.Right) - num,MathF.Max(r1.Top,r2.Top) - num2,MathF.Max(r1.Far,r2.Far) - num3);
		}

		/// <summary>
		/// Unions two prisms together.
		/// </summary>
		/// <param name="r1">The first prism to union.</param>
		/// <param name="r2">The second prism to union.</param>
		/// <param name="result">The resulting prism.</param>
		public static void Union(ref FPrism r1, ref FPrism r2, out FPrism result)
		{
			float rx = Math.Min(r1.X,r2.X);
			float ry = Math.Min(r1.Y,r2.Y);
			float rz = Math.Min(r1.Z,r2.Z);

			result = new FPrism(rx,ry,rz,MathF.Max(r1.Right,r2.Right) - rx,MathF.Max(r1.Top,r2.Top) - ry,MathF.Max(r1.Far,r2.Far) - rz);
			return;
		}

		/// <summary>
		/// Determines if this prism intersects with <paramref name="r"/>.
		/// </summary>
		/// <returns>Returns true if the two prisms intersect nontrivially (with nonzero volume) and false otherwise.</returns>
		public bool Intersects(FPrism r) => r.Left < Right && Left < r.Right && r.Bottom < Top && Bottom < r.Top && r.Near < Far && Near < r.Far;

		/// <summary>
		/// Determines if this prism intersects with <paramref name="r"/> and stores the result in <paramref name="result"/>.
		/// </summary>
		public void Intersects(ref FPrism r, out bool result)
		{
			result = r.Left < Right && Left < r.Right && r.Bottom < Top && Bottom < r.Top && r.Near < Far && Near < r.Far;
			return;
		}

		/// <summary>
		/// Intersects this prism with <paramref name="r"/> and returns the result as a new prism.
		/// </summary>
		/// <param name="r">The prism to intersect with.</param>
		/// <returns>Returns the resulting intersection or the empty prism if this prism and <paramref name="r"/> do not intersect.</returns>
		public FPrism Intersect(FPrism r) => Intersect(this,r);

		/// <summary>
		/// Intersects this prism with <paramref name="r"/> and returns the result as a new prism.
		/// </summary>
		/// <param name="r">The prism to intersect with.</param>
		/// <returns>Returns the resulting intersection or the empty prism if this prism and <paramref name="r"/> do not intersect.</returns>
		public FPrism Intersection(FPrism r) => Intersect(this,r);

		/// <summary>
		/// Computes the intersection of <paramref name="r1"/> and <paramref name="r2"/> and returns the result as a new prism.
		/// </summary>
		public static FPrism Intersect(FPrism r1, FPrism r2)
		{
			Intersect(ref r1,ref r2,out var result);
			return result;
		}

		/// <summary>
		/// Intersects <paramref name="r1"/> with <paramref name="r2"/> and stores the result in <paramref name="result"/>.
		/// </summary>
		public static void Intersect(ref FPrism r1, ref FPrism r2, out FPrism result)
		{
			if(r1.Intersects(r2))
			{
				float num = MathF.Min(r1.Right,r2.Right);
				float num2 = MathF.Max(r1.X,r2.X);
				float num3 = MathF.Max(r1.Y,r2.Y);
				float num4 = MathF.Min(r1.Top,r2.Top);
				float num5 = MathF.Max(r1.Z,r2.Z);
				float num6 = MathF.Min(r1.Far,r2.Far);

				result = new FPrism(num2,num3,num5,num - num2,num4 - num3,num6 - num5);
			}
			else
				result = Empty;
			
			return;
		}
		
		/// <summary>
		/// Deconstructs this prism's position and dimensions into the provided parameters.
		/// </summary>
		public void Deconstruct(out float x, out float y, out float z, out float width, out float height, out float depth)
		{
			x = X;
			y = Y;
			z = Z;

			width = Width;
			height = Height;
			depth = Depth;

			return;
		}

		public override string ToString() => "(X: " + X + " Y: " + Y + " Z: " + Z + " Width: " + Width + " Height: " + Height + " Depth: " + Depth + ")";

		/// <summary>
		/// The x position of the prism.
		/// </summary>
		public float X
		{get;}

		/// <summary>
		/// The y position of the prism.
		/// </summary>
		public float Y
		{get;}

		/// <summary>
		/// The z position of the prism.
		/// </summary>
		public float Z
		{get;}

		/// <summary>
		/// The width of the prism.
		/// </summary>
		public float Width
		{get;}

		/// <summary>
		/// The height of the prism.
		/// </summary>
		public float Height
		{get;}

		/// <summary>
		/// The depth of the prism.
		/// </summary>
		public float Depth
		{get;}

		/// <summary>
		/// The left position of the prism.
		/// This is equal to X.
		/// </summary>
		public float Left => X;

		/// <summary>
		/// The right position of the prism.
		/// This is equal to X + Width.
		/// </summary>
		public float Right => X + Width;

		/// <summary>
		/// The bottom position of the prism.
		/// This is equal to Y.
		/// </summary>
		public float Bottom => Y;

		/// <summary>
		/// The top position of the prism.
		/// This is equal to Y + Height.
		/// </summary>
		public float Top => Y + Height;

		/// <summary>
		/// The near position of the prism.
		/// This is equal to Z.
		/// </summary>
		public float Near => Z;

		/// <summary>
		/// The far position of the prism.
		/// This is equal to Z + Depth.
		/// </summary>
		public float Far => Z + Depth;

		/// <summary>
		/// The center of this prism.
		/// </summary>
		public Vector3 Center => new Vector3(X + Width / 2.0f,Y + Height / 2.0f,Z + Depth / 2.0f);

		/// <summary>
		/// Determines if this prism is the empty prism.
		/// </summary>
		public bool IsEmpty => Width == 0.0f && Height == 0.0f && Depth == 0.0f && X == 0.0f && Y == 0.0f && Z == 0.0f;

		/// <summary>
		/// The (x,y,z) position of the prism.
		/// </summary>
		public Vector3 Location => new Vector3(X,Y,Z);

		/// <summary>
		/// The (width,height,depth) of this prism.
		/// </summary>
		public Vector3 Size => new Vector3(Width,Height,Depth);

		/// <summary>
		/// The volume of this prism.
		/// </summary>
		public float Volume => Width * Height * Depth;

		public float BoxSpace => Volume;

		/// <summary>
		/// The empty prism.
		/// </summary>
		public static FPrism Empty => _ep;

		/// <summary>
		/// The empty prism.
		/// </summary>
		private static FPrism _ep = new FPrism(0.0f,0.0f,0.0f,0.0f,0.0f,0.0f);
	}

	/// <summary>
	/// Performs the JSON conversion for a prism.
	/// </summary>
	file class JsonFPrismConverter : JsonBaseConverter<FPrism>
	{
		protected override object? ReadProperty(ref Utf8JsonReader reader, string property, JsonSerializerOptions ops)
		{
			// We only have number properties, so just check it
			if(!reader.HasNextNumber())
				throw new JsonException();

			switch(property)
			{
			case "X":
			case "Y":
			case "Z":
			case "Width":
			case "Height":
			case "Depth":
				return reader.GetSingle();
			default:
				throw new JsonException();
			}
		}

		protected override FPrism ConstructT(Dictionary<string,object?> properties)
		{
			if(properties.Count != 6)
				throw new JsonException();

			return new FPrism((float)properties["X"]!,(float)properties["Y"]!,(float)properties["Z"]!,(float)properties["Width"]!,(float)properties["Height"]!,(float)properties["Depth"]!);
		}

		protected override void WriteProperties(Utf8JsonWriter writer, FPrism value, JsonSerializerOptions ops)
		{
			writer.WriteNumber("X",value.X);
			writer.WriteNumber("Y",value.Y);
			writer.WriteNumber("Z",value.Z);

			writer.WriteNumber("Width",value.Width);
			writer.WriteNumber("Height",value.Height);
			writer.WriteNumber("Depth",value.Depth);

			return;
		}
	}
}
