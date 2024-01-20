namespace GameEngine.GUI
{
	/// <summary>
	/// Compares IGUI objects by update order or draw order.
	/// </summary>
	public class OrderComparer : IComparer<IGUI>
	{
		/// <summary>
		/// Creates a new order comparer.
		/// </summary>
		/// <param name="update">If true, we will compare by update order. If false, we use draw order instead.</param>
		public OrderComparer(bool update)
		{
			UpdateOrder = update;
			return;
		}

		public int Compare(IGUI? x, IGUI? y)
		{
			int val;

			if(UpdateOrder)
				val = x!.UpdateOrder.CompareTo(y!.UpdateOrder);
			else
				val = x!.DrawOrder.CompareTo(y!.DrawOrder);

			if(val == 0)
				return x!.Name.CompareTo(y!.Name);

			return val;
		}

		/// <summary>
		/// If true, we will compare objects by their update order.
		/// If false, we will compare objects by their draw order.
		/// </summary>
		public bool UpdateOrder
		{get; init;}
	}
}
