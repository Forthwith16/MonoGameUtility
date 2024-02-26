using Microsoft.Xna.Framework;

namespace ExampleShaders
{
	/// <summary>
	/// A wrapper class.
	/// </summary>
	public class Boundary
	{
		public Boundary(int l, int w, int b, int h)
		{
			Left = l;
			Right = l + w - 1;
			Width = w;

			Bottom = b;
			Top = b + h - 1;
			Height = h;

			return;
		}

		public bool Contains(Vector2 p) => p.X >= Left && (int)p.X <= Right && p.Y >= Bottom && (int)p.Y <= Top;

		public int Left
		{get;}

		public int Right
		{get;}

		public int Width
		{get;}

		public int Bottom
		{get;}

		public int Top
		{get;}

		public int Height
		{get;}
	}
}
