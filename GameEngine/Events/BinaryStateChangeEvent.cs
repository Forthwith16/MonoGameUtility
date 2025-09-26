namespace GameEngine.Events
{
	/// <summary>
	/// An event argument that occurs when a binary state of something changes.
	/// </summary>
	public class BinaryStateChangeEvent : EventArgs
	{
		/// <summary>
		/// Creates a new event.
		/// </summary>
		/// <param name="new_state">The new state (opposite the old one).</param>
		public BinaryStateChangeEvent(bool new_state)
		{
			NewState = new_state;
			return;
		}

		/// <summary>
		/// The old ordering.
		/// </summary>
		public bool OldState => !NewState;

		/// <summary>
		/// The new state.
		/// </summary>
		public bool NewState
		{get;}
	}
}
