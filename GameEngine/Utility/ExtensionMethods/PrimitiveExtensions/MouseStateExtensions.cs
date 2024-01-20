using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GameEngine.Utility.ExtensionMethods.PrimitiveExtensions
{
	/// <summary>
	/// Extends the functionality of MouseState.
	/// </summary>
	public static class MouseStateExtensions
	{
		/// <summary>
		/// Calculates the x delta between <paramref name="current"/> and <paramref name="previous"/>.
		/// </summary>
		/// <param name="current">The current mouse state.</param>
		/// <param name="previous">The previous mouse state.</param>
		/// <returns>Returns the x delta between the current and previous mouse state.</returns>
		public static int XDelta(this MouseState current, in MouseState previous)
		{return current.X - previous.X;}

		/// <summary>
		/// Calculates the y delta between <paramref name="current"/> and <paramref name="previous"/>.
		/// </summary>
		/// <param name="current">The current mouse state.</param>
		/// <param name="previous">The previous mouse state.</param>
		/// <returns>Returns the y delta between the current and previous mouse state.</returns>
		public static int YDelta(this MouseState current, in MouseState previous)
		{return current.Y - previous.Y;}

		/// <summary>
		/// Calculates the position delta between <paramref name="current"/> and <paramref name="previous"/>.
		/// </summary>
		/// <param name="current">The current mouse state.</param>
		/// <param name="previous">The previous mouse state.</param>
		/// <returns>Returns the position delta between the current and previous mouse state.</returns>
		public static Point Delta(this MouseState current, in MouseState previous)
		{return current.Position - previous.Position;}

		/// <summary>
		/// Calculates the scroll wheel delta between <paramref name="current"/> and <paramref name="previous"/>.
		/// </summary>
		/// <param name="current">The current mouse state.</param>
		/// <param name="previous">The previous mouse state.</param>
		/// <returns>Returns the delta between the current and previous mouse state's scroll wheel value.</returns>
		public static int ScrollWheelDelta(this MouseState current, in MouseState previous)
		{return current.ScrollWheelValue - previous.ScrollWheelValue;}

		/// <summary>
		/// Calculates the horizontal scroll wheel delta between <paramref name="current"/> and <paramref name="previous"/>.
		/// </summary>
		/// <param name="current">The current mouse state.</param>
		/// <param name="previous">The previous mouse state.</param>
		/// <returns>Returns the delta between the current and previous mouse state's horizontal scroll wheel value.</returns>
		public static int HorizontalScrollWheelDelta(this MouseState current, in MouseState previous)
		{return current.HorizontalScrollWheelValue - previous.HorizontalScrollWheelValue;}
	}
}
