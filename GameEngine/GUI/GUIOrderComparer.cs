using GameEngine.GUI.Components;

namespace GameEngine.GUI
{
	/// <summary>
	/// Compares IGUI objects by update order or draw order.
	/// </summary>
	public class GUIOrderComparer : IComparer<IGUI>
	{
		/// <summary>
		/// Creates a new order comparer.
		/// </summary>
		/// <param name="static_evaluation">Used to ensure that even when items change, their evaluation doesn't. See <see cref="StaticLookup"/>.</param>
		/// <param name="update">If true, we will compare by update order. If false, we use draw order instead.</param>
		public GUIOrderComparer(Dictionary<string,DummyGUI> static_evaluation, bool update)
		{
			StaticLookup = static_evaluation;
			UpdateOrder = update;

			return;
		}

		public int Compare(IGUI? x, IGUI? y)
		{
			// We generously default to the variable given to us if it currently has no static version
			IGUI dummy_x = x!;
			IGUI dummy_y = y!;
			
			if(StaticLookup.TryGetValue(x!.Name,out DummyGUI? temp))
				dummy_x = temp;
			
			if(StaticLookup.TryGetValue(y!.Name,out temp))
				dummy_y = temp;

			int val;

			if(UpdateOrder)
				val = dummy_x.UpdateOrder.CompareTo(dummy_y.UpdateOrder);
			else
				val = dummy_x.DrawOrder.CompareTo(dummy_y.DrawOrder);

			if(val == 0)
				return dummy_x.Name.CompareTo(dummy_y.Name);

			return val;
		}

		/// <summary>
		/// If true, we will compare objects by their update order.
		/// If false, we will compare objects by their draw order.
		/// </summary>
		public bool UpdateOrder
		{get;}

		/// <summary>
		/// This lets us have a static lookup for item values.
		/// This is important for when we use this for a sorted set/array/etc.
		/// This way, even when items change while inside a sorted set, their proscribed position remains in here unchanged.
		/// To update them, we can simply Remove/Update this/Add.
		/// </summary>
		protected Dictionary<string,DummyGUI> StaticLookup
		{get;}
	}
}
