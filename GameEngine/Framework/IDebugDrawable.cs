using Microsoft.Xna.Framework;

namespace GameEngine.Framework
{
	/// <summary>
	/// Represents something that may need to drawn additional information during debugging.
	/// Any functionality added in debug mode will only be utilized if compiling for debugging (or otherwise defining the symbol DEBUG).
	/// </summary>
	public interface IDebugDrawable
	{
		/// <summary>
		/// Draws debug information.
		/// </summary>
		/// <param name="delta">The elapsed game time since the last draw.</param>
		public void DrawDebugInfo(GameTime delta);

		/// <summary>
		/// The debug draw order for drawing debug information.
		/// </summary>
		public int DrawDebugOrder
		{get; set;}

		/// <summary>
		/// An event called when the debug draw order for this changes.
		/// </summary>
		public event DrawDebugOrderChanged OnDrawDebugOrderChanged;
	}

	/// <summary>
	/// Represents an event called when the debug drawing order changes.
	/// </summary>
	/// <param name="sender">The object whose debug draw order changed.</param>
	/// <param name="new_order">The new debug draw order.</param>
	/// <param name="old_order">The old debug draw order.</param>
	public delegate void DrawDebugOrderChanged(IDebugDrawable sender, int new_order, int old_order);
}
