using Microsoft.Xna.Framework;

namespace GameEngine.Input.Bindings.GamePadBindings
{
	/// <summary>
	/// Encapsulates a gamepad axis binding.
	/// </summary>
	public class GamePadAxisBinding : GamePadBinding
	{
		/// <summary>
		/// Creates a gamepad axis binding.
		/// </summary>
		/// <param name="gamepad">The gamepad to use for the binding.</param>
		/// <param name="left">If true, this binds the left stick. If false, this binds the right stick.</param>
		/// <param name="x">If true, this binds an x-axis. If false, this binds a right-axis.</param>
		/// <param name="threshold">
		///	This is the (inclusive) threshold the axis must meet to evaluate digitally to true.
		///	If <paramref name="exceed"/> is true, then the axis must be at least the threshold to satisfy the input.
		///	If <paramref name="exceed"/> is false, then the axis must be at most the threshold to satisfy the input.
		///	<para/>
		///	Axis values range within [-1,1].
		/// </param>
		/// <param name="exceed">If true, then the threshold must be exceeded to satisfy the binding. If false, it must be at most the threshold.</param>
		/// <param name="abs">If true, then the axis value will use its absolute value in evaluation calculations. If false, its raw value is used instead.</param>
		public GamePadAxisBinding(PlayerIndex gamepad, bool left, bool x, float threshold = 1.0f, bool exceed = true, bool abs = false) : base(gamepad)
		{
			LeftStick = left;
			XAxis = x;
			Threshold = threshold;
			Exceed = exceed;
			Absolute = abs;

			if(LeftStick)
			{
				if(XAxis)
				{
					if(Absolute)
					{
						if(Exceed)
							DigitalEvaluation = (state) => MathF.Abs(state.Gamepad(Gamepad).ThumbSticks.Left.X) >= Threshold;
						else
							DigitalEvaluation = (state) => MathF.Abs(state.Gamepad(Gamepad).ThumbSticks.Left.X) <= Threshold;

						AnalogEvaluation = (state) => MathF.Abs(state.Gamepad(Gamepad).ThumbSticks.Left.X);
					}
					else
					{
						if(Exceed)
							DigitalEvaluation = (state) => state.Gamepad(Gamepad).ThumbSticks.Left.X >= Threshold;
						else
							DigitalEvaluation = (state) => state.Gamepad(Gamepad).ThumbSticks.Left.X <= Threshold;

						AnalogEvaluation = (state) => state.Gamepad(Gamepad).ThumbSticks.Left.X;
					}
				}
				else // YAxis
				{
					if(Absolute)
					{
						if(Exceed)
							DigitalEvaluation = (state) => MathF.Abs(state.Gamepad(Gamepad).ThumbSticks.Left.Y) >= Threshold;
						else
							DigitalEvaluation = (state) => MathF.Abs(state.Gamepad(Gamepad).ThumbSticks.Left.Y) <= Threshold;

						AnalogEvaluation = (state) => MathF.Abs(state.Gamepad(Gamepad).ThumbSticks.Left.Y);
					}
					else
					{
						if(Exceed)
							DigitalEvaluation = (state) => state.Gamepad(Gamepad).ThumbSticks.Left.Y >= Threshold;
						else
							DigitalEvaluation = (state) => state.Gamepad(Gamepad).ThumbSticks.Left.Y <= Threshold;

						AnalogEvaluation = (state) => state.Gamepad(Gamepad).ThumbSticks.Left.Y;
					}
				}
			}
			else // Right stick
			{
				if(XAxis)
				{
					if(Absolute)
					{
						if(Exceed)
							DigitalEvaluation = (state) => MathF.Abs(state.Gamepad(Gamepad).ThumbSticks.Right.X) >= Threshold;
						else
							DigitalEvaluation = (state) => MathF.Abs(state.Gamepad(Gamepad).ThumbSticks.Right.X) <= Threshold;

						AnalogEvaluation = (state) => MathF.Abs(state.Gamepad(Gamepad).ThumbSticks.Right.X);
					}
					else
					{
						if(Exceed)
							DigitalEvaluation = (state) => state.Gamepad(Gamepad).ThumbSticks.Right.X >= Threshold;
						else
							DigitalEvaluation = (state) => state.Gamepad(Gamepad).ThumbSticks.Right.X <= Threshold;

						AnalogEvaluation = (state) => state.Gamepad(Gamepad).ThumbSticks.Right.X;
					}
				}
				else // YAxis
				{
					if(Absolute)
					{
						if(Exceed)
							DigitalEvaluation = (state) => MathF.Abs(state.Gamepad(Gamepad).ThumbSticks.Right.Y) >= Threshold;
						else
							DigitalEvaluation = (state) => MathF.Abs(state.Gamepad(Gamepad).ThumbSticks.Right.Y) <= Threshold;

						AnalogEvaluation = (state) => MathF.Abs(state.Gamepad(Gamepad).ThumbSticks.Right.Y);
					}
					else
					{
						if(Exceed)
							DigitalEvaluation = (state) => state.Gamepad(Gamepad).ThumbSticks.Right.Y >= Threshold;
						else
							DigitalEvaluation = (state) => state.Gamepad(Gamepad).ThumbSticks.Right.Y <= Threshold;

						AnalogEvaluation = (state) => state.Gamepad(Gamepad).ThumbSticks.Right.Y;
					}
				}
			}

			return;
		}

		public override string ToString()
		{return base.ToString() + " (" + (LeftStick ? "Left" : "Right") + (Absolute ? " Absolute" : "") + (XAxis ? " X" : " Y") + "-Axis [" + Threshold + "])";}

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
		/// True iff this binds an x-axis.
		/// </summary>
		public bool XAxis
		{get; init;}

		/// <summary>
		/// True iff this binds a y-axis.
		/// </summary>
		public bool YAxis
		{get => !XAxis;}

		/// <summary>
		/// True iff this binding uses the absolute value of the axis value.
		/// </summary>
		public bool Absolute
		{get; init;}

		/// <summary>
		/// This is the (inclusive) threshold the axis must meet to evaluate digitally to true.
		/// If Exceed is true, then the axis must be at least the threshold to satisfy the input.
		/// If Exceed is false, then the axis must be at most the threshold to satisfy the input.
		/// <para/>
		/// Axis values range within [-1,1].
		/// </summary>
		public float Threshold
		{get; init;}

		/// <summary>
		/// If true, then the threshold must be exceeded to satisfy the binding. If false, it must be at most the threshold.
		/// </summary>
		public bool Exceed
		{get; init;}

		public override bool IsAxisBinding
		{get => true;}
	}
}
