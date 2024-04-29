using Microsoft.Xna.Framework.Input.Touch;

namespace GameEngine.Input.Bindings.TouchBindings
{
	/// <summary>
	/// Encapsulates a touch press binding.
	/// </summary>
	public class TouchPressBinding : TouchBinding
	{
		/// <summary>
		/// Creates a touch binding.
		/// </summary>
		/// <param name="id">The touch ID to bind.</param>
		/// <param name="pressed">If true, this binding requires a touch press. If false, it requires a touch release.</param>
		public TouchPressBinding(int id, bool pressed = true) : base()
		{
			ID = id;
			Pressed = pressed;

			if(Pressed)
			{
				DigitalEvaluation = (state) => state.CurrentTouch.Count > ID && (state.CurrentTouch[ID].State == TouchLocationState.Moved || state.CurrentTouch[ID].State == TouchLocationState.Pressed);
				AnalogEvaluation = (state) => state.CurrentTouch.Count > ID && (state.CurrentTouch[ID].State == TouchLocationState.Moved || state.CurrentTouch[ID].State == TouchLocationState.Pressed) ? 1.0f : 0.0f;
			}
			else // Released
			{
				DigitalEvaluation = (state) => state.CurrentTouch.Count > ID && state.CurrentTouch[ID].State == TouchLocationState.Released || state.CurrentTouch.Count <= ID;
				AnalogEvaluation = (state) => state.CurrentTouch.Count > ID && state.CurrentTouch[ID].State == TouchLocationState.Released || state.CurrentTouch.Count <= ID ? 1.0f : 0.0f;
			}
			
			return;
		}

		/// <summary>
		/// The touch ID bound.
		/// </summary>
		public int ID
		{get;}

		/// <summary>
		/// If true, this binding requires a touch press.
		/// If false, this binding requires a touch release.
		/// </summary>
		public bool Pressed
		{get;}

		public override bool IsPressBinding => true;
	}
}
