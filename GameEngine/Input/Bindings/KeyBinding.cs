using Microsoft.Xna.Framework.Input;

namespace GameEngine.Input.Bindings
{
	/// <summary>
	/// Encapsulates a keyboard binding.
	/// </summary>
	public class KeyBinding : InputBinding
	{
		/// <summary>
		/// Creates a key binding.
		/// </summary>
		/// <param name="key">The key to bind.</param>
		/// <param name="pressed">
		///	If true, this will evaluate to true/1 if <paramref name="key"/> is pressed and false/0 if released.
		///	If false, this will evaluate to true/1 if <paramref name="key"/> is released and false/0 if pressed.
		/// </param>
		public KeyBinding(Keys key, bool pressed = true) : base()
		{
			Key = key;
			Pressed = pressed;
			
			if(Pressed)
				DigitalEvaluation = (state) => state.Keys.IsKeyDown(Key);
			else
				DigitalEvaluation = (state) => Keyboard.GetState().IsKeyUp(Key);

			if(Pressed)
				AnalogEvaluation = (state) => state.Keys.IsKeyDown(Key) ? 1.0f : 0.0f;
			else
				AnalogEvaluation = (state) => Keyboard.GetState().IsKeyUp(Key) ? 1.0f : 0.0f;

			return;
		}

		public override string ToString()
		{return "Key Binding (" + (Pressed ? "Press " : "Release ") + Key + ")";}

		/// <summary>
		/// The key this binding binds kept for record-keeping purposes.
		/// </summary>
		public Keys Key
		{get; init;}

		/// <summary>
		/// If true, then Key must be pressed to trigger this binding.
		/// If false, then Key must be released to trigger this binding.
		/// </summary>
		public bool Pressed
		{get; init;}

		/// <summary>
		/// If true, then Key must be released to trigger this binding.
		/// If false, then Key must be pressed to trigger this binding.
		/// </summary>
		public bool Released
		{get => !Pressed;}

		public override bool IsKeyBinding
		{get => true;}
	}
}
