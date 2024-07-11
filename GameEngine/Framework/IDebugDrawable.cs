using Microsoft.Xna.Framework;

namespace GameEngine.Framework
{
	/// <summary>
	/// Represents something that may need to drawn additional information during debugging.
	/// Any functionality added in debug mode will only be utilized if compiling for debugging (or otherwise defining the symbol DEBUG).
	/// <para/>
	/// To disable debug drawing in debug compiliations, set <see cref="GlobalConstants.DrawDebugInformation"/> to false.
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
		/// Smaller values are typically drawn before larger values.
		/// </summary>
		public int DrawDebugOrder
		{get; set;}
		
		/// <summary>
		/// If true, this is visible and DrawDebugInfo will be called.
		/// If false, this is not visible and DrawDebugInfo will not be called.
		/// </summary>
		public bool Visible
		{get;}

		/// <summary>
		/// An event called when the debug draw order for this changes.
		/// </summary>
		public event DrawDebugOrderChanged OnDrawDebugOrderChanged;

		/// <summary>
		/// Called when the visibility of this changes.
		/// </summary>
		public event EventHandler<EventArgs> VisibleChanged;
	}

	/// <summary>
	/// Represents an event called when the debug drawing order changes.
	/// </summary>
	/// <param name="sender">The object whose debug draw order changed.</param>
	/// <param name="new_order">The new debug draw order.</param>
	/// <param name="old_order">The old debug draw order.</param>
	public delegate void DrawDebugOrderChanged(IDebugDrawable sender, int new_order, int old_order);
}
