using Microsoft.Xna.Framework;

namespace GameEngine.Utility.ExtensionMethods.PrimitiveExtensions
{
	/// <summary>
	/// Extensions for the Rectangle class.
	/// </summary>
	public static class RectangleExtensions
	{
		/// <summary>
		/// Determines the area of this rectangle.
		/// </summary>
		/// <param name="rect">This rectangle.</param>
		/// <returns>Returns the area of this rectangle.</returns>
		public static int Area(this Rectangle rect)
		{return rect.Width * rect.Height;}

		/// <summary>
		/// Obtains the bottom left position of this rectangle.
		/// </summary>
		/// <param name="rect">The rectangle to obtain a corner of.</param>
		/// <returns>Returns the bottom left corner of the rectangle.</returns>
		public static Point BottomLeft(this Rectangle rect) => new Point(rect.Left,rect.Bottom);

		/// <summary>
		/// Obtains the bottom right position of this rectangle.
		/// </summary>
		/// <param name="rect">The rectangle to obtain a corner of.</param>
		/// <returns>Returns the bottom right corner of the rectangle.</returns>
		public static Point BottomRight(this Rectangle rect) => new Point(rect.Right,rect.Bottom);

		/// <summary>
		/// Obtains the top left position of this rectangle.
		/// </summary>
		/// <param name="rect">The rectangle to obtain a corner of.</param>
		/// <returns>Returns the top left corner of the rectangle.</returns>
		public static Point TopLeft(this Rectangle rect) => new Point(rect.Left,rect.Top);

		/// <summary>
		/// Obtains the top right position of this rectangle.
		/// </summary>
		/// <param name="rect">The rectangle to obtain a corner of.</param>
		/// <returns>Returns the top right corner of the rectangle.</returns>
		public static Point TopRight(this Rectangle rect) => new Point(rect.Right,rect.Top);

		/// <summary>
		/// Unions two rectangles together by creating the minimum bounding rectangle containing both of them.
		/// </summary>
		/// <param name="rect">The first rectangle.</param>
		/// <param name="other">The rectangle to union with.</param>
		/// <returns>Returns the minimum bounding rectangle containing both rectangles.</returns>
		public static Rectangle Union(this Rectangle rect, Rectangle other)
		{
			int left = Math.Min(rect.Left,other.Left);
			int right = Math.Max(rect.Right,other.Right);

			int top = Math.Min(rect.Top,other.Top);
			int bottom = Math.Max(rect.Bottom,other.Bottom);

			return new Rectangle(left,top,right - left,bottom - top);
		}
	}
}
