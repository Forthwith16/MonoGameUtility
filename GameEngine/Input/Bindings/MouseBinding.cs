namespace GameEngine.Input.Bindings
{
	/// <summary>
	/// Encapsulates a mouse binding.
	/// </summary>
	public abstract class MouseBinding : InputBinding
	{
		/// <summary>
		/// Creates a mouse binding.
		/// </summary>
		protected MouseBinding() : base()
		{return;}

		public override string ToString()
		{return "Mouse Binding";}

		/// <summary>
		/// True iff this is a mouse button binding.
		/// </summary>
		public virtual bool IsButtonBinding
		{get => false;}

		/// <summary>
		/// True iff this is a mouse axis binding.
		/// </summary>
		public virtual bool IsAxisBinding
		{get => false;}

		/// <summary>
		/// True iff this is a mouse scroll binding.
		/// </summary>
		public virtual bool IsScrollBinding
		{get => false;}

		public override bool IsMouseBinding
		{get => true;}
	}
}
