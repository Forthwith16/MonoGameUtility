using GameEngine.Utility.ExtensionMethods.PrimitiveExtensions;

namespace GameEngine.Input.Bindings.MouseBindings
{
	/// <summary>
	/// Encapsulates a mouse scroll wheel binding.
	/// </summary>
	public class MouseScrollBinding : MouseBinding
	{
		/// <summary>
		/// Creates a mouse scroll wheel binding.
		/// </summary>
		/// <param name="vertical">If true, this binds the vertical scroll wheel. If false, this binds the horizontal scroll wheel.</param>
		/// <param name="delta">If true, this queries mouse scroll wheel values. If false, it uses the raw mouse scroll wheel values.</param>
		/// <param name="threshold">
		///	This is the (inclusive) threshold the scroll wheel binding must meet to evaluate digitally to true.
		///	If <paramref name="exceed"/> is true, then the axis must be at least the threshold to satisfy the input.
		///	If <paramref name="exceed"/> is false, then the axis must be at most the threshold to satisfy the input.
		///	<para/>
		///	Scroll wheel values are unbounded but usually come in increments of 120.
		/// </param>
		/// <param name="exceed">If true, then the threshold must be exceeded to satisfy the binding. If false, it must be at most the threshold.</param>
		/// <param name="abs">If true, then the scroll wheel value will use its absolute value in evaluation calculations. If false, its raw value is used instead.</param>
		public MouseScrollBinding(bool vertical = true, bool delta = true, int threshold = 0, bool exceed = true, bool abs = false) : base()
		{
			Vertical = vertical;
			Delta = delta;
			Threshold = threshold;
			Exceed = exceed;
			Absolute = abs;

			if(Vertical)
			{
				if(Delta)
				{
					if(Absolute)
					{
						if(Exceed)
							DigitalEvaluation = (state) => MathF.Abs(state.CurrentMouse.ScrollWheelDelta(state.PreviousMouse)) >= Threshold;
						else
							DigitalEvaluation = (state) => MathF.Abs(state.CurrentMouse.ScrollWheelDelta(state.PreviousMouse)) <= Threshold;

						AnalogEvaluation = (state) => MathF.Abs(state.CurrentMouse.ScrollWheelDelta(state.PreviousMouse));
					}
					else
					{
						if(Exceed)
							DigitalEvaluation = (state) => state.CurrentMouse.ScrollWheelDelta(state.PreviousMouse) >= Threshold;
						else
							DigitalEvaluation = (state) => state.CurrentMouse.ScrollWheelDelta(state.PreviousMouse) <= Threshold;

						AnalogEvaluation = (state) => state.CurrentMouse.ScrollWheelDelta(state.PreviousMouse);
					}
				}
				else // Raw
				{
					if(Absolute)
					{
						if(Exceed)
							DigitalEvaluation = (state) => MathF.Abs(state.CurrentMouse.ScrollWheelValue) >= Threshold;
						else
							DigitalEvaluation = (state) => MathF.Abs(state.CurrentMouse.ScrollWheelValue) <= Threshold;

						AnalogEvaluation = (state) => MathF.Abs(state.CurrentMouse.ScrollWheelValue);
					}
					else
					{
						if(Exceed)
							DigitalEvaluation = (state) => state.CurrentMouse.ScrollWheelValue >= Threshold;
						else
							DigitalEvaluation = (state) => state.CurrentMouse.ScrollWheelValue <= Threshold;

						AnalogEvaluation = (state) => state.CurrentMouse.ScrollWheelValue;
					}
				}
			}
			else // Horizontal
			{
				if(Delta)
				{
					if(Absolute)
					{
						if(Exceed)
							DigitalEvaluation = (state) => MathF.Abs(state.CurrentMouse.HorizontalScrollWheelDelta(state.PreviousMouse)) >= Threshold;
						else
							DigitalEvaluation = (state) => MathF.Abs(state.CurrentMouse.HorizontalScrollWheelDelta(state.PreviousMouse)) <= Threshold;

						AnalogEvaluation = (state) => MathF.Abs(state.CurrentMouse.HorizontalScrollWheelDelta(state.PreviousMouse));
					}
					else
					{
						if(Exceed)
							DigitalEvaluation = (state) => state.CurrentMouse.HorizontalScrollWheelDelta(state.PreviousMouse) >= Threshold;
						else
							DigitalEvaluation = (state) => state.CurrentMouse.HorizontalScrollWheelDelta(state.PreviousMouse) <= Threshold;

						AnalogEvaluation = (state) => state.CurrentMouse.HorizontalScrollWheelDelta(state.PreviousMouse);
					}
				}
				else // Raw
				{
					if(Absolute)
					{
						if(Exceed)
							DigitalEvaluation = (state) => MathF.Abs(state.CurrentMouse.HorizontalScrollWheelValue) >= Threshold;
						else
							DigitalEvaluation = (state) => MathF.Abs(state.CurrentMouse.HorizontalScrollWheelValue) <= Threshold;

						AnalogEvaluation = (state) => MathF.Abs(state.CurrentMouse.HorizontalScrollWheelValue);
					}
					else
					{
						if(Exceed)
							DigitalEvaluation = (state) => state.CurrentMouse.HorizontalScrollWheelValue >= Threshold;
						else
							DigitalEvaluation = (state) => state.CurrentMouse.HorizontalScrollWheelValue <= Threshold;

						AnalogEvaluation = (state) => state.CurrentMouse.HorizontalScrollWheelValue;
					}
				}
			}

			return;
		}

		public override string ToString()
		{return base.ToString() + " (" + (Absolute ? " Absolute" : "") + (Vertical ? "Vertical" : "Horizontal") + " Scroll Wheel" + (Delta ? " Delta" : "") + " [" + Threshold + "])";}

		/// <summary>
		/// True iff this binds the vertical scroll wheel.
		/// </summary>
		public bool Vertical
		{get; init;}

		/// <summary>
		/// True iff this binds the horizontal scroll wheel.
		/// </summary>
		public bool Horizontal => !Vertical;

		/// <summary>
		/// True iff this binding uses the absolute value of the scroll wheel delta value.
		/// </summary>
		public bool Absolute
		{get; init;}

		/// <summary>
		/// If true, this checks scroll wheel deltas against the threshold.
		/// If false, this checks raw scroll wheel values against the threshold.
		/// </summary>
		public bool Delta
		{get; init;}

		/// <summary>
		/// If true, this checks raw scroll wheel values against the threshold.
		/// If false, this checks scroll wheel deltas against the threshold.
		/// </summary>
		public bool Raw => !Delta;

		/// <summary>
		/// This is the (inclusive) threshold the scroll wheel must meet to evaluate digitally to true.
		/// If Exceed is true, then the axis must be at least the threshold to satisfy the input.
		/// If Exceed is false, then the axis must be at most the threshold to satisfy the input.
		/// <para/>
		/// Scroll wheel values are unbounded but usually come in increments of 120.
		/// </summary>
		public float Threshold
		{get; init;}

		/// <summary>
		/// If true, then the threshold must be exceeded to satisfy the binding. If false, it must be at most the threshold.
		/// </summary>
		public bool Exceed
		{get; init;}

		public override bool IsScrollBinding
		{get => true;}
	}
}
