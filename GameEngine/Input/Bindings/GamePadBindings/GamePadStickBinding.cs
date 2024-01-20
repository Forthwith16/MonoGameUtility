using Microsoft.Xna.Framework;

namespace GameEngine.Input.Bindings.GamePadBindings
{
	/// <summary>
	/// Encapsulates a gamepad stick binding (normalized magnitude of an xy-axis vector).
	/// </summary>
	public class GamePadStickBinding : GamePadBinding
	{
		/// <summary>
		/// Creates a gamepad stick binding.
		/// </summary>
		/// <param name="gamepad">The gamepad to use for the binding.</param>
		/// <param name="left">If true, this binds the left stick. If false, this binds the right stick.</param>
		/// <param name="threshold">
		///	This is the (inclusive) threshold the stick must meet to evaluate digitally to true.
		///	<para/>
		///	Stick values are clamped to [0,1].
		///	Thresholds at most 0 will always be true and thresholds greater than 1 will always be false.
		/// </param>
		/// <param name="invert">If true, then the stick value will be reported as 1 - the actual stick value. This new value will be used for both analog and digital evaluation.</param>
		public GamePadStickBinding(PlayerIndex gamepad, bool left, float threshold = 1.0f, bool invert = false) : base(gamepad)
		{
			LeftStick = left;
			Inverted = invert;
			Threshold = threshold;

			if(Inverted)
				if(LeftStick)
					DigitalEvaluation = (state) => 1.0f - state.Gamepad(Gamepad).ThumbSticks.Left.Length() / MAX_MAGNITUDE >= Threshold;
				else
					DigitalEvaluation = (state) => 1.0f - state.Gamepad(Gamepad).ThumbSticks.Right.Length() / MAX_MAGNITUDE >= Threshold;
			else
				if(LeftStick)
					DigitalEvaluation = (state) => state.Gamepad(Gamepad).ThumbSticks.Left.Length() / MAX_MAGNITUDE >= Threshold;
				else
					DigitalEvaluation = (state) => state.Gamepad(Gamepad).ThumbSticks.Right.Length() / MAX_MAGNITUDE >= Threshold;


			if(Inverted)
				if(LeftStick)
					AnalogEvaluation = (state) => 1.0f - state.Gamepad(Gamepad).ThumbSticks.Left.Length() / MAX_MAGNITUDE;
				else
					AnalogEvaluation = (state) => 1.0f - state.Gamepad(Gamepad).ThumbSticks.Right.Length() / MAX_MAGNITUDE;
			else
				if(LeftStick)
					AnalogEvaluation = (state) => state.Gamepad(Gamepad).ThumbSticks.Left.Length() / MAX_MAGNITUDE;
				else
					AnalogEvaluation = (state) => state.Gamepad(Gamepad).ThumbSticks.Right.Length() / MAX_MAGNITUDE;

			return;
		}

		public override string ToString()
		{return base.ToString() + " (" + (LeftStick ? "Left" : "Right") + (Inverted ? " Inverted" : "") + " Stick [" + Threshold + "])";}

		/// <summary>
		/// True iff this binds the left stick.
		/// </summary>
		public bool LeftStick
		{get; init;}

		/// <summary>
		/// True iff this binds the right stick.
		/// </summary>
		public bool RightStick => !LeftStick;

		/// <summary>
		/// True iff this binding inverts the stick value.
		/// By inverstion here, we mean that it will use 1 - the actual stick value for its digital and analog evaluations.
		/// </summary>
		public bool Inverted
		{get; init;}

		/// <summary>
		/// The threshold that the stick must meet in order for this binding to digitally evaluate to true.
		/// <para/>
		/// Stick values are clamped to [0,1].
		/// Thresholds at most 0 will always be true and thresholds greater than 1 will always be false.
		/// </summary>
		public float Threshold
		{get; init;}

		public override bool IsStickBinding
		{get => true;}

		/// <summary>
		/// This is the maximum magnitude that a stick (an xy-axis) can achieve via the x and y axis both maxing out their magnitudes at 1.
		/// </summary>
		public static readonly float MAX_MAGNITUDE = MathF.Sqrt(2);
	}
}
