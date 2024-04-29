namespace GameEngine.Input.Bindings
{
	/// <summary>
	/// Encapsulates a touch binding.
	/// </summary>
	public abstract class TouchBinding : InputBinding
	{
		/// <summary>
		/// Creates a touch binding.
		/// </summary>
		protected TouchBinding() : base()
		{return;}

		public override string ToString()
		{return "Touch Binding";}

		/// <summary>
		/// True iff this is a touch press binding.
		/// </summary>
		public virtual bool IsPressBinding
		{get => false;}

		/// <summary>
		/// True iff this is a touch axis binding.
		/// </summary>
		public virtual bool IsAxisBinding
		{get => false;}

		public override bool IsTouchBinding
		{get => true;}
	}
}
