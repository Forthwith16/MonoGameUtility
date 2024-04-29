using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace GameEngine.Utility.ExtensionMethods.PrimitiveExtensions
{
	/// <summary>
	/// Extension methods for the TouchLocation struct.
	/// </summary>
	public static class TouchLocationExtensions
	{
		/// <summary>
		/// Calculates the x delta between <paramref name="current"/> and <paramref name="previous"/>.
		/// </summary>
		/// <param name="current">The current touch locations.</param>
		/// <param name="previous">The previous touch locations.</param>
		/// <returns>Returns the x delta between the current and previous touch locations.</returns>
		public static float XDelta(this TouchLocation current, TouchLocation previous) => current.Position.X - previous.Position.X;

		/// <summary>
		/// Calculates the y delta between <paramref name="current"/> and <paramref name="previous"/>.
		/// </summary>
		/// <param name="current">The current touch locations.</param>
		/// <param name="previous">The previous touch locations.</param>
		/// <returns>Returns the y delta between the current and previous touch locations.</returns>
		public static float YDelta(this TouchLocation current, TouchLocation previous) => current.Position.Y - previous.Position.Y;

		/// <summary>
		/// Calculates the delta between <paramref name="current"/> and <paramref name="previous"/>.
		/// </summary>
		/// <param name="current">The current touch locations.</param>
		/// <param name="previous">The previous touch locations.</param>
		/// <returns>Returns the delta between the current and previous touch locations.</returns>
		public static Vector2 Delta(this TouchLocation current, TouchLocation previous) => current.Position - previous.Position;
	}
}
