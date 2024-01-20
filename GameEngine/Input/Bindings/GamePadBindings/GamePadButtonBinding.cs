using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GameEngine.Input.Bindings
{
	/// <summary>
	/// Encapsulates a gamepad button binding.
	/// </summary>
	public class GamePadButtonBinding : GamePadBinding
	{
		/// <summary>
		/// Creates a gamepad button binding.
		/// </summary>
		/// <param name="gamepad">The gamepad to use for the binding.</param>
		/// <param name="button">The button to bind.</param>
		/// <param name="pressed">
		///	If true, this will evaluate to true/1 if <paramref name="button"/> is pressed and false/0 if released.
		///	If false, this will evaluate to true/1 if <paramref name="button"/> is released and false/0 if pressed.
		/// </param>
		public GamePadButtonBinding(PlayerIndex gamepad, Buttons button, bool pressed = true) : base(gamepad)
		{
			Gamepad = gamepad;
			Button = button;
			Pressed = pressed;

			if(Pressed)
				DigitalEvaluation = (state) => state.Gamepad(Gamepad).IsButtonDown(Button);
			else
				DigitalEvaluation = (state) => state.Gamepad(Gamepad).IsButtonUp(Button);

			if(Pressed)
				AnalogEvaluation = (state) => state.Gamepad(Gamepad).IsButtonDown(Button) ? 1.0f : 0.0f;
			else
				AnalogEvaluation = (state) => state.Gamepad(Gamepad).IsButtonUp(Button) ? 1.0f : 0.0f;
			
			return;
		}

		public override string ToString()
		{return base.ToString() + " (" + (Pressed ? "Press" : "Release") + " Button " + Button + ")";}

		/// <summary>
		/// If this is a button binding, then this is the button bound.
		/// </summary>
		public Buttons Button
		{get; init;}

		/// <summary>
		/// If true, then Button must be pressed to trigger this binding.
		/// If false, then Button must be released to trigger this binding.
		/// <para/>
		/// This value has no meaning if IsButtonBinding is false.
		/// </summary>
		public bool Pressed
		{get; init;}

		/// <summary>
		/// If true, then Button must be released to trigger this binding.
		/// If false, then Button must be pressed to trigger this binding.
		/// <para/>
		/// This value has no meaning if IsButtonBinding is false.
		/// </summary>
		public bool Released
		{get => !Pressed;}

		/// <summary>
		/// True iff this is a gamepad button binding.
		/// </summary>
		public override bool IsButtonBinding
		{get => true;}
	}
}
