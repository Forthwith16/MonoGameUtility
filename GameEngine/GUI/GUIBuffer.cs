namespace GameEngine.GUI
{
	/// <summary>
	/// Contains buffer information for a GUI component.
	/// <para/>
	/// Padding is a buffer of empty space around a component's exterior.
	/// Margins are a buffer of empty space around a component's interior.
	/// </summary>
	public struct GUIBuffer
	{
		/// <summary>
		/// Creates a new buffer.
		/// </summary>
		/// <param name="left">The left-side buffer size.</param>
		/// <param name="right">The right-side buffer size.</param>
		/// <param name="top">The top-side buffer size.</param>
		/// <param name="bottom">The bottom-side buffer size.</param>
		public GUIBuffer(float left, float right, float top, float bottom)
		{
			Top = top;
			Bottom = bottom;
			Left = left;
			Right = right;

			return;
		}

		/// <summary>
		/// The top-side buffer.
		/// </summary>
		public float Top
		{get; set;}

		/// <summary>
		/// The bottom-side buffer.
		/// </summary>
		public float Bottom
		{get; set;}

		/// <summary>
		/// The left-side buffer.
		/// </summary>
		public float Left
		{get; set;}

		/// <summary>
		/// The right-side buffer.
		/// </summary>
		public float Right
		{get; set;}
	}
}
