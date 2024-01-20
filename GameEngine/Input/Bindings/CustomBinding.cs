namespace GameEngine.Input.Bindings
{
	/// <summary>
	/// Encalsupates a fully customizable input binding.
	/// </summary>
	public class CustomBinding : InputBinding
	{
		/// <summary>
		/// Creates an input binding with the provided evaluation functions.
		/// </summary>
		/// <param name="digital">The means by which this input is evaluated digitally.</param>
		/// <param name="analog">The means by which this input's analog value is obtained.</param>
		public CustomBinding(InputDigitalValue digital, InputAnalogValue analog) : base(digital,analog)
		{return;}

		public override string ToString()
		{return "Custom Binding";}
	}
}
