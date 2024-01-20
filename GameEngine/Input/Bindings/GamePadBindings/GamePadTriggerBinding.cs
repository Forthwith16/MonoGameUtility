using Microsoft.Xna.Framework;

namespace GameEngine.Input.Bindings.GamePadBindings
{
	/// <summary>
	/// Encapsulates a gamepad trigger binding.
	/// </summary>
	public class GamePadTriggerBinding : GamePadBinding
	{
		/// <summary>
		/// Creates a gamepad trigger binding.
		/// </summary>
		/// <param name="gamepad">The gamepad to use for the binding.</param>
		/// <param name="left">If true, this binds the left trigger. If false, this binds the right trigger.</param>
		/// <param name="threshold">
		///	This is the (inclusive) threshold the trigger must meet to evaluate digitally to true.
		///	<para/>
		///	Trigger values are clamped to [0,1].
		///	Thresholds at most 0 will always be true and thresholds greater than 1 will always be false.
		/// </param>
		/// <param name="invert">If true, then the trigger value will be reported as 1 - the actual trigger value. This new value will be used for both analog and digital evaluation.</param>
		public GamePadTriggerBinding(PlayerIndex gamepad, bool left, float threshold = 1.0f, bool invert = false) : base(gamepad)
		{
			LeftTrigger = left;
			Inverted = invert;
			Threshold = threshold;

			if(Inverted)
				if(LeftTrigger)
					DigitalEvaluation = (state) => 1.0f - state.Gamepad(Gamepad).Triggers.Left >= Threshold;
				else
					DigitalEvaluation = (state) => 1.0f - state.Gamepad(Gamepad).Triggers.Right >= Threshold;
			else
				if(LeftTrigger)
					DigitalEvaluation = (state) => state.Gamepad(Gamepad).Triggers.Left >= Threshold;
				else
					DigitalEvaluation = (state) => state.Gamepad(Gamepad).Triggers.Right >= Threshold;


			if(Inverted)
				if(LeftTrigger)
					AnalogEvaluation = (state) => 1.0f - state.Gamepad(Gamepad).Triggers.Left;
				else
					AnalogEvaluation = (state) => 1.0f - state.Gamepad(Gamepad).Triggers.Right;
			else
				if(LeftTrigger)
					AnalogEvaluation = (state) => state.Gamepad(Gamepad).Triggers.Left;
				else
					AnalogEvaluation = (state) => state.Gamepad(Gamepad).Triggers.Right;

			return;
		}

		public override string ToString()
		{return base.ToString() + " (" + (LeftTrigger ? "Left" : "Right") + (Inverted ? " Inverted" : "") + " Trigger [" + Threshold + "])";}

		/// <summary>
		/// True iff this binds the left trigger.
		/// </summary>
		public bool LeftTrigger
		{get; init;}

		/// <summary>
		/// True iff this binds the right trigger.
		/// </summary>
		public bool RightTrigger => !LeftTrigger;

		/// <summary>
		/// True iff this binding inverts the trigger value.
		/// By inverstion here, we mean that it will use 1 - the actual trigger value for its digital and analog evaluations.
		/// </summary>
		public bool Inverted
		{get; init;}

		/// <summary>
		/// The threshold that the trigger must meet in order for this binding to digitally evaluate to true.
		/// <para/>
		/// Trigger values are clamped to [0,1].
		/// Thresholds at most 0 will always be true and thresholds greater than 1 will always be false.
		/// </summary>
		public float Threshold
		{get; init;}

		public override bool IsTriggerBinding
		{get => true;}
	}
}
