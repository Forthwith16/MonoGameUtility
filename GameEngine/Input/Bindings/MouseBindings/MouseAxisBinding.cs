using GameEngine.Utility.ExtensionMethods.PrimitiveExtensions;

namespace GameEngine.Input.Bindings.MouseBindings
{
	/// <summary>
	/// Encapsulates a mouse axis binding.
	/// </summary>
	public class MouseAxisBinding : MouseBinding
	{
		/// <summary>
		/// Creates a new mouse axis binding.
		/// </summary>
		/// <param name="x">If true, we use the mouse x-axis. If false, we use the mouse y-axis.</param>
		/// <param name="delta">
		///	If true, we use mouse delta values.
		///	If false, we use raw mouse values.
		///	<para/>
		///	Mouse axis values are theoretically unbounded.
		/// </param>
		public MouseAxisBinding(bool x, bool delta = false) : base()
		{
			XAxis = x;
			Delta = delta;

			if(XAxis)
				if(Delta)
				{
					DigitalEvaluation = (state) => state.CurrentMouse.XDelta(state.PreviousMouse) != 0;
					AnalogEvaluation = (state) => state.CurrentMouse.XDelta(state.PreviousMouse);
				}
				else
				{
					// There's no good way to turn an absolute mouse position into a boolean without context or intent, so let's just go with if it's moved, it counts
					DigitalEvaluation = (state) => state.CurrentMouse.XDelta(state.PreviousMouse) != 0;
					AnalogEvaluation = (state) => state.CurrentMouse.X;
				}
			else // YAxis
				if(Delta)
				{
					DigitalEvaluation = (state) => state.CurrentMouse.YDelta(state.PreviousMouse) != 0;
					AnalogEvaluation = (state) => state.CurrentMouse.YDelta(state.PreviousMouse);
				}
				else
				{
					// There's no good way to turn an absolute mouse position into a boolean without context or intent, so let's just go with if it's moved, it counts
					DigitalEvaluation = (state) => state.CurrentMouse.YDelta(state.PreviousMouse) != 0;
					AnalogEvaluation = (state) => state.CurrentMouse.Y;
				}

			return;
		}

		public override string ToString()
		{return base.ToString() + " (" + (Delta ? "Delta " : "") + (XAxis ? "X" : "Y") + "-Axis)";}

		/// <summary>
		/// If true, we use the mouse x-axis.
		/// If false, we use the mouse y-axis.
		/// </summary>
		public bool XAxis
		{get; init;}

		/// <summary>
		/// If true, we use the mouse y-axis.
		/// If false, we use the mouse x-axis.
		/// </summary>
		public bool YAxis => !XAxis;

		/// <summary>
		/// If true, we use mouse delta values.
		/// If false, we use raw mouse values.
		/// </summary>
		public bool Delta
		{get; init;}

		/// <summary>
		/// If true, we use raw mouse values.
		/// If false, we use mouse delta values.
		/// </summary>
		public bool Raw => !Delta;

		public override bool IsAxisBinding
		{get => true;}
	}
}
