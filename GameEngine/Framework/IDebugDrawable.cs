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
		public void DebugDraw(GameTime delta);
		
		/// <summary>
		/// If true, this is visible and <see cref="DebugDraw(GameTime)"/> will be called.
		/// If false, this is not visible and <see cref="DebugDraw(GameTime)"/> will not be called.
		/// </summary>
		public bool Visible
		{get;}

		/// <summary>
		/// The debug draw order for drawing debug information.
		/// Smaller values are typically drawn before larger values.
		/// </summary>
		public int DebugDrawOrder
		{get; set;}

		/// <summary>
		/// Called when the visibility of this changes.
		/// <para/>
		/// The first parameter will be this.
		/// The second parameter will be a <see cref="BinaryStateChangeEvent"/>.
		/// </summary>
		public event EventHandler<EventArgs>? VisibleChanged;

		/// <summary>
		/// An event called when the debug draw order for this changes.
		/// <para/>
		/// The first parameter will be this.
		/// The second parameter will be an <see cref="OrderChangeEvent"/>.
		/// </summary>
		public event EventHandler<EventArgs>? DebugDrawOrderChanged;
	}
}
