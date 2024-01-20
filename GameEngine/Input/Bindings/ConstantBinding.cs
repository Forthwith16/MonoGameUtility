namespace GameEngine.Input.Bindings
{
	/// <summary>
	/// Encapsulates a input binding with a constant value.
	/// </summary>
	public class ConstantBinding : InputBinding
	{
		/// <summary>
		/// Creates a binding with a constant digital value corresponding to <paramref name="value"/>.
		/// Its analog value is 1 if <paramref name="value"/> is true and 0 if <paramref name="value"/> is false.
		/// </summary>
		/// <param name="value">This binding will always evalute to this.</param>
		public ConstantBinding(bool value) : this(value,value ? 1.0f : 0.0f)
		{return;}

		/// <summary>
		/// Creates a binding with a constant digital value corresponding to <paramref name="b_value"/> and a constant analog value corresponding to <paramref name="a_value"/>.
		/// </summary>
		/// <param name="b_value">This binding's digital value will always evalute to this.</param>
		/// <param name="b_value">This binding's analog value will always evalute to this.</param>
		public ConstantBinding(bool b_value, float a_value) : base()
		{
			DigitalConstantValue = b_value;
			AnalogConstantValue = a_value;

			DigitalEvaluation = (state) => DigitalConstantValue;
			AnalogEvaluation = (state) => AnalogConstantValue;

			return;
		}

		public override string ToString()
		{return "Constant Binding (" + DigitalConstantValue + "," + AnalogConstantValue + ")";}

		/// <summary>
		/// The digital constant value kept for record-keeping purposes.
		/// </summary>
		public bool DigitalConstantValue
		{get; init;}

		/// <summary>
		/// The analog constant value kept for record-keeping purposes.
		/// </summary>
		public float AnalogConstantValue
		{get; init;}
	}
}
