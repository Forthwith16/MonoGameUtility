using GameEngine.Utility.ExtensionMethods.PrimitiveExtensions;

namespace GameEngine.Input.Bindings.TouchBindings
{
	/// <summary>
	/// Encapsulates a touch axis binding.
	/// </summary>
	public class TouchAxisBinding : TouchBinding
	{
		/// <summary>
		/// Creates a new touch axis binding.
		/// </summary>
		/// <param name="id">The touch ID to bind.</param>
		/// <param name="x">If true, we use the touch x-axis. If false, we use the touch y-axis.</param>
		/// <param name="delta">
		///	If true, we use touch delta values.
		///	If false, we use raw touch values.
		///	<para/>
		///	Touch axis values are theoretically unbounded.
		/// </param>
		public TouchAxisBinding(int id, bool x, bool delta = false) : base()
		{
			ID = id;
			XAxis = x;
			Delta = delta;

			if(XAxis)
				if(Delta)
				{
					DigitalEvaluation = (state) => state.CurrentTouch.Count > ID && state.PreviousTouch.Count > ID && !state.CurrentTouch[ID].XDelta(state.PreviousTouch[ID]).CloseEnough(0.0f);
					AnalogEvaluation = (state) => state.CurrentTouch.Count > ID && state.PreviousTouch.Count > ID ? state.CurrentTouch[ID].XDelta(state.PreviousTouch[ID]) : 0.0f;
				}
				else
				{
					// There's no good way to turn an absolute touch position into a boolean without context or intent, so let's just go with if it's moved, it counts
					DigitalEvaluation = (state) => state.CurrentTouch.Count > ID && state.PreviousTouch.Count > ID && !state.CurrentTouch[ID].XDelta(state.PreviousTouch[ID]).CloseEnough(0.0f);
					AnalogEvaluation = (state) => state.CurrentTouch.Count > ID ? state.CurrentTouch[ID].Position.X : 0.0f;
				}
			else // YAxis
				if(Delta)
				{
					DigitalEvaluation = (state) => state.CurrentTouch.Count > ID && state.PreviousTouch.Count > ID && !state.CurrentTouch[ID].YDelta(state.PreviousTouch[ID]).CloseEnough(0.0f);
					AnalogEvaluation = (state) => state.CurrentTouch.Count > ID && state.PreviousTouch.Count > ID ? state.CurrentTouch[ID].YDelta(state.PreviousTouch[ID]) : 0.0f;
				}
				else
				{
					// There's no good way to turn an absolute touch position into a boolean without context or intent, so let's just go with if it's moved, it counts
					DigitalEvaluation = (state) => state.CurrentTouch.Count > ID && state.PreviousTouch.Count > ID && !state.CurrentTouch[ID].YDelta(state.PreviousTouch[ID]).CloseEnough(0.0f);
					AnalogEvaluation = (state) => state.CurrentTouch.Count > ID ? state.CurrentTouch[ID].Position.Y : 0.0f;
				}

			return;
		}

		public override string ToString()
		{return base.ToString() + " (ID: " + ID + " " + (Delta ? "Delta " : " ") + (XAxis ? "X" : "Y") + "-Axis)";}

		/// <summary>
		/// The touch ID bound.
		/// </summary>
		public int ID
		{get;}

		/// <summary>
		/// If true, we use the touch x-axis.
		/// If false, we use the touch y-axis.
		/// </summary>
		public bool XAxis
		{get; init;}

		/// <summary>
		/// If true, we use the touch y-axis.
		/// If false, we use the touch x-axis.
		/// </summary>
		public bool YAxis => !XAxis;

		/// <summary>
		/// If true, we use touch delta values.
		/// If false, we use raw touch values.
		/// </summary>
		public bool Delta
		{get; init;}

		/// <summary>
		/// If true, we use raw touch values.
		/// If false, we use touch delta values.
		/// </summary>
		public bool Raw => !Delta;

		public override bool IsAxisBinding
		{get => true;}
	}
}
