using GameEngine.Input.Bindings;

namespace GameEngine.Input
{
	/// <summary>
	/// Encapsulates an input binding.
	/// </summary>
	public abstract class InputBinding
	{
		/// <summary>
		/// Creates an input binding that always evaluates to false or 0.
		/// </summary>
		protected InputBinding() : this((state) => false,(state) => 0.0f)
		{return;}

		/// <summary>
		/// Creates an input binding with the specified evaluation functions.
		/// </summary>
		/// <param name="digital">The means by which this input is evaluated digitally.</param>
		/// <param name="analog">The means by which this input's analog value is obtained.</param>
		protected InputBinding(InputDigitalValue digital, InputAnalogValue analog)
		{
			DigitalEvaluation = digital;
			AnalogEvaluation = analog;
			
			return;
		}

		/// <summary>
		/// Creates a new input binding that performs an AND operation of the evaluations of <paramref name="lhs"/> and <paramref name="rhs"/>.
		/// The AND of digital values is a simple logical and.
		/// The AND of analog values is a minimum (in magnitude).
		/// </summary>
		public static InputBinding operator &(InputBinding lhs, InputBinding rhs)
		{return new CustomBinding((state) => lhs.DigitalEvaluation(state) && rhs.DigitalEvaluation(state),(state) => MathF.MinMagnitude(lhs.AnalogEvaluation(state),rhs.AnalogEvaluation(state)));}

		/// <summary>
		/// Creates a new input binding that performs an OR operation of the evaluations of <paramref name="lhs"/> and <paramref name="rhs"/>.
		/// The OR of digital values is a simple logical or.
		/// The OR of analog values is a maximum (in magnitude).
		/// </summary>
		public static InputBinding operator |(InputBinding lhs, InputBinding rhs)
		{return new CustomBinding((state) => lhs.DigitalEvaluation(state) || rhs.DigitalEvaluation(state),(state) => MathF.MaxMagnitude(lhs.AnalogEvaluation(state),rhs.AnalogEvaluation(state)));}

		/// <summary>
		/// Creates a new input binding that performs an XOR operation of the evaluations of <paramref name="lhs"/> and <paramref name="rhs"/>.
		/// The XOR of digital values is a simple logical xor.
		/// The XOR of analog values is a subtraction of <paramref name="rhs"/> from <paramref name="lhs"/>.
		/// </summary>
		public static InputBinding operator ^(InputBinding lhs, InputBinding rhs)
		{return new CustomBinding((state) => lhs.DigitalEvaluation(state) ^ rhs.DigitalEvaluation(state),(state) => lhs.AnalogEvaluation(state) - rhs.AnalogEvaluation(state));}

		/// <summary>
		/// Creates a new input binding that performs a NOT operation of the evaluations of <paramref name="binding"/>.
		/// The NOT of digital values is a simple logical not.
		/// The NOT of analog values is an inversion (multiplication by -1).
		/// </summary>
		public static InputBinding operator ~(InputBinding binding)
		{return new CustomBinding((state) => !binding.DigitalEvaluation(state),(state) => -binding.AnalogEvaluation(state));}

		/// <summary>
		/// Creates a new input binding that performs a magnitude calculation of the evaluations of <paramref name="binding"/>.
		/// The magnitude calculation of digital values does nothing.
		/// The magnitude calculation of analog values performs an absolute value.
		/// </summary>
		public static InputBinding operator +(InputBinding binding) // binding is immutable, so we don't need to be concerned with accounting for it's DigitalEvaluation changing
		{return new CustomBinding(binding.DigitalEvaluation,(state) => MathF.Abs(binding.AnalogEvaluation(state)));}

		/// <summary>
		/// Creates a new input binding that performs an addition calculation of the evaluations of <paramref name="lhs"/> and <paramref name="rhs"/>.
		/// The addition calculation of digital values is a simple logical or.
		/// The addition calculation of analog values is addition.
		/// </summary>
		public static InputBinding operator +(InputBinding lhs, InputBinding rhs)
		{return new CustomBinding((state) => lhs.DigitalEvaluation(state) || rhs.DigitalEvaluation(state),(state) => lhs.AnalogEvaluation(state) + rhs.AnalogEvaluation(state));}

		/// <summary>
		/// Creates a new input binding that performs a negation calculation of the evaluations of <paramref name="binding"/>.
		/// The negation calculation of digital values is a simple logical not.
		/// The negation calculation of analog values is an inversion (multiplication by -1).
		/// </summary>
		public static InputBinding operator -(InputBinding binding)
		{return new CustomBinding((state) => !binding.DigitalEvaluation(state),(state) => -binding.AnalogEvaluation(state));}

		/// <summary>
		/// Creates a new input binding that performs a subtraction calculation of the evaluations of <paramref name="lhs"/> and <paramref name="rhs"/>.
		/// The subtraction calculation of digital values is a logical xor.
		/// The subtraction calculation of analog values is subtraction.
		/// </summary>
		public static InputBinding operator -(InputBinding lhs, InputBinding rhs)
		{return new CustomBinding((state) => lhs.DigitalEvaluation(state) ^ rhs.DigitalEvaluation(state),(state) => lhs.AnalogEvaluation(state) - rhs.AnalogEvaluation(state));}

		/// <summary>
		/// Creates a new input binding that performs a multiplication calculation of the evaluations of <paramref name="lhs"/> and <paramref name="rhs"/>.
		/// The multiplication calculation of digital values is a logical and.
		/// The multiplication calculation of analog values is multiplication.
		/// </summary>
		public static InputBinding operator *(InputBinding lhs, InputBinding rhs)
		{return new CustomBinding((state) => lhs.DigitalEvaluation(state) && rhs.DigitalEvaluation(state),(state) => lhs.AnalogEvaluation(state) * rhs.AnalogEvaluation(state));}

		/// <summary>
		/// Evaluates this binding digitally.
		/// <para/>
		/// This value is never null.
		/// </summary>
		public InputDigitalValue DigitalEvaluation
		{get; init;}

		/// <summary>
		/// Evaluates this binding in an analog range.
		/// <para/>
		/// If this binding is from a pure digital source, most binding implementations should evaluate it to 0 or 1.
		/// <para/>
		/// This value is never null.
		/// </summary>
		public InputAnalogValue AnalogEvaluation
		{get; init;}

		/// <summary>
		/// True iff this is a pure key binding.
		/// </summary>
		public virtual bool IsKeyBinding
		{get => false;}

		/// <summary>
		/// True iff this is a pure mouse binding.
		/// </summary>
		public virtual bool IsMouseBinding
		{get => false;}

		/// <summary>
		/// True iff this is a pure gamepad binding.
		/// </summary>
		public virtual bool IsGamepadBinding
		{get => false;}
	}

	/// <summary>
	/// Evaluates a binding digitally.
	/// </summary>
	/// <param name="state">The input manager used to access state information.</param>
	/// <returns>Returns true if the binding is satisfied and false if it is not.</returns>
	public delegate bool InputDigitalValue(InputManager state);

	/// <summary>
	/// Evaluates a binding within an analog range.
	/// </summary>
	/// <param name="state">The input manager used to access state information.</param>
	/// <returns>Returns the analog value of the input.</returns>
	public delegate float InputAnalogValue(InputManager state);
}
