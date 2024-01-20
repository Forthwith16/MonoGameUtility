using Microsoft.Xna.Framework.Input;

namespace GameEngine.Input.Bindings.MouseBindings
{
	/// <summary>
	/// Encapsulates a mouse button binding.
	/// </summary>
	public class MouseButtonBinding : MouseBinding
	{
		/// <summary>
		/// Creates a mouse button binding.
		/// </summary>
		/// <param name="button">The button to bind.</param>
		/// <param name="pressed">If true, this binding requires a button to be pressed. If false, it requires the button to be released.</param>
		public MouseButtonBinding(MouseButton button, bool pressed = true) : base()
		{
			Button = button;
			Pressed = pressed;

			switch(Button)
			{
			case MouseButton.Left:
				if(Pressed)
				{
					DigitalEvaluation = (state) => state.CurrentMouse.LeftButton == ButtonState.Pressed;
					AnalogEvaluation = (state) => state.CurrentMouse.LeftButton == ButtonState.Pressed ? 1.0f : 0.0f;
				}
				else
				{
					DigitalEvaluation = (state) => state.CurrentMouse.LeftButton == ButtonState.Released;
					AnalogEvaluation = (state) => state.CurrentMouse.LeftButton == ButtonState.Released ? 1.0f : 0.0f;
				}

				break;
			case MouseButton.Right:
				if(Pressed)
				{
					DigitalEvaluation = (state) => state.CurrentMouse.RightButton == ButtonState.Pressed;
					AnalogEvaluation = (state) => state.CurrentMouse.RightButton == ButtonState.Pressed ? 1.0f : 0.0f;
				}
				else
				{
					DigitalEvaluation = (state) => state.CurrentMouse.RightButton == ButtonState.Released;
					AnalogEvaluation = (state) => state.CurrentMouse.RightButton == ButtonState.Released ? 1.0f : 0.0f;
				}

				break;
			case MouseButton.Middle:
				if(Pressed)
				{
					DigitalEvaluation = (state) => state.CurrentMouse.MiddleButton == ButtonState.Pressed;
					AnalogEvaluation = (state) => state.CurrentMouse.MiddleButton == ButtonState.Pressed ? 1.0f : 0.0f;
				}
				else
				{
					DigitalEvaluation = (state) => state.CurrentMouse.MiddleButton == ButtonState.Released;
					AnalogEvaluation = (state) => state.CurrentMouse.MiddleButton == ButtonState.Released ? 1.0f : 0.0f;
				}

				break;
			case MouseButton.XButton1:
				if(Pressed)
				{
					DigitalEvaluation = (state) => state.CurrentMouse.XButton1 == ButtonState.Pressed;
					AnalogEvaluation = (state) => state.CurrentMouse.XButton1 == ButtonState.Pressed ? 1.0f : 0.0f;
				}
				else
				{
					DigitalEvaluation = (state) => state.CurrentMouse.XButton1 == ButtonState.Released;
					AnalogEvaluation = (state) => state.CurrentMouse.XButton1 == ButtonState.Released ? 1.0f : 0.0f;
				}

				break;
			case MouseButton.XButton2:
				if(Pressed)
				{
					DigitalEvaluation = (state) => state.CurrentMouse.XButton2 == ButtonState.Pressed;
					AnalogEvaluation = (state) => state.CurrentMouse.XButton2 == ButtonState.Pressed ? 1.0f : 0.0f;
				}
				else
				{
					DigitalEvaluation = (state) => state.CurrentMouse.XButton2 == ButtonState.Released;
					AnalogEvaluation = (state) => state.CurrentMouse.XButton2 == ButtonState.Released ? 1.0f : 0.0f;
				}

				break;
			}

			return;
		}

		/// <summary>
		/// The mouse button bound.
		/// </summary>
		public MouseButton Button
		{get; init;}

		/// <summary>
		/// If true, this binding requires a mouse button to be pressed.
		/// If false, this binding requires a mouse button to be released.
		/// </summary>
		public bool Pressed
		{get; init;}

		public override bool IsButtonBinding
		{get => true;}
	}

	/// <summary>
	/// Represents a mouse button.
	/// </summary>
	public enum MouseButton
	{
		Left,
		Right,
		Middle,
		XButton1,
		XButton2
	}
}
