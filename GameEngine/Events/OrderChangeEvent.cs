namespace GameEngine.Events
{
	/// <summary>
	/// An event argument that occurs when the order of something changes.
	/// </summary>
	public class OrderChangeEvent : EventArgs
	{
		/// <summary>
		/// Creates a new event.
		/// </summary>
		/// <param name="old_order">The old order value.</param>
		/// <param name="new_order">The new order value.</param>
		public OrderChangeEvent(int old_order, int new_order)
		{
			OldOrder = old_order;
			NewOrder = new_order;

			return;
		}

		/// <summary>
		/// The old ordering.
		/// </summary>
		public int OldOrder
		{get;}

		/// <summary>
		/// The new ordering.
		/// </summary>
		public int NewOrder
		{get;}
	}
}
