using GameEngine.Input;
using GameEngine.Input.Bindings;
using GameEngine.Input.Bindings.GamePadBindings;
using GameEngine.Input.Bindings.MouseBindings;
using GameEngine.Input.Bindings.TouchBindings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GameEngine.Utility.ExtensionMethods.ClassExtensions
{
	/// <summary>
	/// Extends the functionality of the InputManager.
	/// </summary>
	public static class InputManagerExtensions
	{
		#region Constant Bindings
		/// <summary>
		/// Adds an empty binding that always evaluates to false and 0.
		/// </summary>
		/// <param name="input">The input manager to add the binding to.</param>
		/// <param name="name">The name of the binding to create.</param>
		/// <param name="put">If true, we will put the binding into <paramref name="input"/>. If false, we will add the binding instead.</param>
		/// <returns>Returns true if the binding was added and false otherwise.</returns>
		public static bool AddEmptyInput(this InputManager input, string name, bool put = false)
		{return input.AddConstantInput(name,false,0.0f,put);}

		/// <summary>
		/// Adds a constant binding.
		/// </summary>
		/// <param name="input">The input manager to add the binding to.</param>
		/// <param name="name">The name of the binding to create.</param>
		/// <param name="b_value">The digital constant value.</param>
		/// <param name="f_value">The analog constant value.</param>
		/// <param name="put">If true, we will put the binding into <paramref name="input"/>. If false, we will add the binding instead.</param>
		/// <returns>Returns true if the binding was added and false otherwise.</returns>
		public static bool AddConstantInput(this InputManager input,string name,bool b_value = false,float f_value = 0.0f,bool put = false)
		{
			if(put)
				input.PutBinding(name,new ConstantBinding(b_value,f_value));
			else
				return input.AddBinding(name,new ConstantBinding(b_value,f_value));

			return true;
		}
		#endregion

		#region Raw Input Bindings
		/// <summary>
		/// Adds a custom binding with <paramref name="input"/>.
		/// </summary>
		/// <param name="input">The input manager to add the binding to.</param>
		/// <param name="name">The name of the binding to create.</param>
		/// <param name="digital">The means by which this input is evaluated digitally.</param>
		/// <param name="analog">The means by which this input's analog value is obtained.</param>
		/// <param name="put">If true, we will put the binding into <paramref name="input"/>. If false, we will add the binding instead.</param>
		/// <returns>Returns true if the binding was added and false otherwise.</returns>
		public static bool AddCustomBinding(this InputManager input, string name, InputDigitalValue digital, InputAnalogValue analog, bool put = false)
		{
			if(put)
				input.PutBinding(name,new CustomBinding(digital,analog));
			else
				return input.AddBinding(name,new CustomBinding(digital,analog));
			
			return true;
		}

		/// <summary>
		/// Adds an key binding with <paramref name="input"/>.
		/// </summary>
		/// <param name="input">The input manager to add the binding to.</param>
		/// <param name="name">The name of the binding to create.</param>
		/// <param name="key">The key to bind.</param>
		/// <param name="pressed">If true, we require the key to be pressed. If false, we require it to be released.</param>
		/// <param name="put">If true, we will put the binding into <paramref name="input"/>. If false, we will add the binding instead.</param>
		/// <returns>Returns true if the binding was added and false otherwise.</returns>
		public static bool AddKeyInput(this InputManager input, string name, Keys key, bool pressed = true, bool put = false)
		{
			if(put)
				input.PutBinding(name,new KeyBinding(key,pressed));
			else
				return input.AddBinding(name,new KeyBinding(key,pressed));

			return true;
		}

		/// <summary>
		/// Adds a mouse button binding with <paramref name="input"/>.
		/// </summary>
		/// <param name="input">The input manager to add the binding to.</param>
		/// <param name="name">The name of the binding to create.</param>
		/// <param name="button">The mouse button to bind.</param>
		/// <param name="pressed">If true, we require the button to be pressed. If false, we require it to be released.</param>
		/// <param name="put">If true, we will put the binding into <paramref name="input"/>. If false, we will add the binding instead.</param>
		/// <returns>Returns true if the binding was added and false otherwise.</returns>
		public static bool AddMouseButtonInput(this InputManager input, string name, MouseButton button, bool pressed = true, bool put = false)
		{
			if(put)
				input.PutBinding(name,new MouseButtonBinding(button,pressed));
			else
				return input.AddBinding(name,new MouseButtonBinding(button,pressed));

			return true;
		}

		/// <summary>
		/// Adds a mouse axis binding with <paramref name="input"/>.
		/// </summary>
		/// <param name="input">The input manager to add the binding to.</param>
		/// <param name="name">The name of the binding to create.</param>
		/// <param name="x">If true, this binds the mouse x-axis. If false, this binds the mouse y-axis.</param>
		/// <param name="delta">If true, we bind mouse deltas. If false, we bind absolute mouse positions.</param>
		/// <param name="put">If true, we will put the binding into <paramref name="input"/>. If false, we will add the binding instead.</param>
		/// <returns>Returns true if the binding was added and false otherwise.</returns>
		public static bool AddMouseAxisInput(this InputManager input, string name, bool x, bool delta = true, bool put = false)
		{
			if(put)
				input.PutBinding(name,new MouseAxisBinding(x,delta));
			else
				return input.AddBinding(name,new MouseAxisBinding(x,delta));

			return true;
		}

		/// <summary>
		/// Adds a mouse scroll wheel binding with <paramref name="input"/>.
		/// </summary>
		/// <param name="input">The input manager to add the binding to.</param>
		/// <param name="name">The name of the binding to create.</param>
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
		/// <param name="put">If true, we will put the binding into <paramref name="input"/>. If false, we will add the binding instead.</param>
		/// <returns>Returns true if the binding was added and false otherwise.</returns>
		public static bool AddMouseScrollInput(this InputManager input, string name, bool vertical = true, bool delta = true, int threshold = 0, bool exceed = true, bool abs = false, bool put = false)
		{
			if(put)
				input.PutBinding(name,new MouseScrollBinding(vertical,delta,threshold,exceed,abs));
			else
				return input.AddBinding(name,new MouseScrollBinding(vertical,delta,threshold,exceed,abs));
			
			return true;
		}

		/// <summary>
		/// Adds a gamepad button binding with <paramref name="input"/>.
		/// </summary>
		/// <param name="input">The input manager to add the binding to.</param>
		/// <param name="name">The name of the binding to create.</param>
		/// <param name="gamepad">The gamepad to bind.</param>
		/// <param name="button">The gamepad button to bind.</param>
		/// <param name="pressed">If true, we require the button to be pressed. If false, we require it to be released.</param>
		/// <param name="put">If true, we will put the binding into <paramref name="input"/>. If false, we will add the binding instead.</param>
		/// <returns>Returns true if the binding was added and false otherwise.</returns>
		public static bool AddGamePadButtonInput(this InputManager input, string name, PlayerIndex gamepad, Buttons button, bool pressed = true, bool put = false)
		{
			if(put)
				input.PutBinding(name,new GamePadButtonBinding(gamepad,button,pressed));
			else
				return input.AddBinding(name,new GamePadButtonBinding(gamepad,button,pressed));

			return true;
		}

		/// <summary>
		/// Adds a gamepad axis binding with <paramref name="input"/>.
		/// </summary>
		/// <param name="input">The input manager to add the binding to.</param>
		/// <param name="name">The name of the binding to create.</param>
		/// <param name="gamepad">The gamepad to use for the binding.</param>
		/// <param name="left">If true, this binds the left stick. If false, this binds the right stick.</param>
		/// <param name="x">If true, this binds an x-axis. If false, this binds a y-axis.</param>
		/// <param name="threshold">
		///	This is the (inclusive) threshold the axis must meet to evaluate digitally to true.
		///	If <paramref name="exceed"/> is true, then the axis must be at least the threshold to satisfy the input.
		///	If <paramref name="exceed"/> is false, then the axis must be at most the threshold to satisfy the input.
		///	<para/>
		///	Axis values range within [-1,1].
		/// </param>
		/// <param name="exceed">If true, then the threshold must be exceeded to satisfy the binding. If false, it must be at most the threshold.</param>
		/// <param name="abs">If true, then the axis value will use its absolute value in evaluation calculations. If false, its raw value is used instead.</param>
		/// <param name="put">If true, we will put the binding into <paramref name="input"/>. If false, we will add the binding instead.</param>
		/// <returns>Returns true if the binding was added and false otherwise.</returns>
		public static bool AddGamePadAxisInput(this InputManager input, string name, PlayerIndex gamepad, bool left, bool x, float threshold = 1.0f, bool exceed = true, bool abs = false, bool put = false)
		{
			if(put)
				input.PutBinding(name,new GamePadAxisBinding(gamepad,left,x,threshold,exceed,abs));
			else
				return input.AddBinding(name,new GamePadAxisBinding(gamepad,left,x,threshold,exceed,abs));

			return true;
		}

		/// <summary>
		/// Adds a gamepad trigger binding with <paramref name="input"/>.
		/// </summary>
		/// <param name="input">The input manager to add the binding to.</param>
		/// <param name="name">The name of the binding to create.</param>
		/// <param name="gamepad">The gamepad to use for the binding.</param>
		/// <param name="left">If true, this binds the left trigger. If false, this binds the right trigger.</param>
		/// <param name="threshold">
		///	This is the (inclusive) threshold the trigger must meet to evaluate digitally to true.
		///	<para/>
		///	Trigger values are clamped to [0,1].
		///	Thresholds at most 0 will always be true and thresholds greater than 1 will always be false.
		/// </param>
		/// <param name="invert">If true, then the trigger value will be reported as 1 - the actual trigger value. This new value will be used for both analog and digital evaluation.</param>
		/// <param name="put">If true, we will put the binding into <paramref name="input"/>. If false, we will add the binding instead.</param>
		/// <returns>Returns true if the binding was added and false otherwise.</returns>
		public static bool AddGamePadTriggerInput(this InputManager input, string name, PlayerIndex gamepad, bool left, float threshold = 1.0f, bool invert = false, bool put = false)
		{
			if(put)
				input.PutBinding(name,new GamePadTriggerBinding(gamepad,left,threshold,invert));
			else
				return input.AddBinding(name,new GamePadTriggerBinding(gamepad,left,threshold,invert));

			return true;
		}

		/// <summary>
		/// Adds a gamepad stick binding with <paramref name="input"/>.
		/// </summary>
		/// <param name="input">The input manager to add the binding to.</param>
		/// <param name="name">The name of the binding to create.</param>
		/// <param name="gamepad">The gamepad to use for the binding.</param>
		/// <param name="left">If true, this binds the left stick. If false, this binds the right stick.</param>
		/// <param name="threshold">
		///	This is the (inclusive) threshold the stick must meet to evaluate digitally to true.
		///	<para/>
		///	Stick values are clamped to [0,1].
		///	Thresholds at most 0 will always be true and thresholds greater than 1 will always be false.
		/// </param>
		/// <param name="invert">If true, then the stick value will be reported as 1 - the actual stick value. This new value will be used for both analog and digital evaluation.</param>
		/// <param name="put">If true, we will put the binding into <paramref name="input"/>. If false, we will add the binding instead.</param>
		/// <returns>Returns true if the binding was added and false otherwise.</returns>
		public static bool AddGamePadStickInput(this InputManager input, string name, PlayerIndex gamepad, bool left, float threshold = 1.0f, bool invert = false, bool put = false)
		{
			if(put)
				input.PutBinding(name,new GamePadStickBinding(gamepad,left,threshold,invert));
			else
				return input.AddBinding(name,new GamePadStickBinding(gamepad,left,threshold,invert));

			return true;
		}

		/// <summary>
		/// Adds a touch press binding with <paramref name="input"/>.
		/// </summary>
		/// <param name="input">The input manager to add the binding to.</param>
		/// <param name="name">The name of the binding to create.</param>
		/// <param name="id">The touch ID to bind.</param>
		/// <param name="pressed">If true, then a touch press is required. If false, then a touch release is required.</param>
		/// <param name="put">If true, we will put the binding into <paramref name="input"/>. If false, we will add the binding instead.</param>
		/// <returns>Returns true if the binding was added and false otherwise.</returns>
		public static bool AddTouchPressInput(this InputManager input, string name, int id, bool pressed = true, bool put = false)
		{
			if(put)
				input.PutBinding(name,new TouchPressBinding(id,pressed));
			else
				return input.AddBinding(name,new TouchPressBinding(id,pressed));

			return true;
		}

		/// <summary>
		/// Adds a touch axis binding with <paramref name="input"/>.
		/// </summary>
		/// <param name="input">The input manager to add the binding to.</param>
		/// <param name="name">The name of the binding to create.</param>
		/// <param name="id">The touch ID to bind.</param>
		/// <param name="x">If true, this binds the touch x-axis. If false, this binds the touch y-axis.</param>
		/// <param name="delta">If true, we bind touch deltas. If false, we bind absolute touch positions.</param>
		/// <param name="put">If true, we will put the binding into <paramref name="input"/>. If false, we will add the binding instead.</param>
		/// <returns>Returns true if the binding was added and false otherwise.</returns>
		public static bool AddTouchAxisInput(this InputManager input, string name, int id, bool x, bool delta = true, bool put = false)
		{
			if(put)
				input.PutBinding(name,new TouchAxisBinding(id,x,delta));
			else
				return input.AddBinding(name,new TouchAxisBinding(id,x,delta));

			return true;
		}
		#endregion

		#region Math Bindings
		/// <summary>
		/// Registers a binding with <paramref name="input"/> with the following evaluations.
		/// <para/>
		/// The boolean evaluation performs a simple logical and of <paramref name="lhs"/> and <paramref name="rhs"/>.
		/// <para/>
		/// The analog evaluation performs a minimum (in magnitude).
		/// </summary>
		/// <param name="input">The input manager to add the binding to.</param>
		/// <param name="name">The name of the binding to create.</param>
		/// <param name="lhs">The left hand side of the operation.</param>
		/// <param name="rhs">The right hand side of the operation.</param>
		/// <param name="put">If true, we will put the binding into <paramref name="input"/>. If false, we will add the binding instead.</param>
		/// <returns>Returns true if the binding was added and false otherwise.</returns>
		/// <remarks>
		///	This is distinct from directly creating the combined binding via operator overloading.
		///	If what <paramref name="lhs"/> or <paramref name="rhs"/> references changes, this will reference the changed bindings.
		/// </remarks>
		public static bool AddAndInput(this InputManager input, string name, string lhs, string rhs, bool put = false)
		{
			if(put)
				input.PutBinding(name,new CustomBinding((state) => state[lhs].CurrentDigitalValue && state[rhs].CurrentDigitalValue,(state) => MathF.MinMagnitude(state[lhs].CurrentAnalogValue,state[rhs].CurrentAnalogValue)));
			else
				return input.AddBinding(name,new CustomBinding((state) => state[lhs].CurrentDigitalValue && state[rhs].CurrentDigitalValue,(state) => MathF.MinMagnitude(state[lhs].CurrentAnalogValue,state[rhs].CurrentAnalogValue)));

			return true;
		}

		/// <summary>
		/// Registers a binding with <paramref name="input"/> with the following evaluations.
		/// <para/>
		/// The boolean evaluation performs a simple logical or of <paramref name="lhs"/> and <paramref name="rhs"/>.
		/// <para/>
		/// The analog evaluation performs a maximum (in magnitude).
		/// </summary>
		/// <param name="input">The input manager to add the binding to.</param>
		/// <param name="name">The name of the binding to create.</param>
		/// <param name="lhs">The left hand side of the operation.</param>
		/// <param name="rhs">The right hand side of the operation.</param>
		/// <param name="put">If true, we will put the binding into <paramref name="input"/>. If false, we will add the binding instead.</param>
		/// <returns>Returns true if the binding was added and false otherwise.</returns>
		/// <remarks>
		///	This is distinct from directly creating the combined binding via operator overloading.
		///	If what <paramref name="lhs"/> or <paramref name="rhs"/> references changes, this will reference the changed bindings.
		/// </remarks>
		public static bool AddOrInput(this InputManager input, string name, string lhs, string rhs, bool put = false)
		{
			if(put)
				input.PutBinding(name,new CustomBinding((state) => state[lhs].CurrentDigitalValue || state[rhs].CurrentDigitalValue,(state) => MathF.MaxMagnitude(state[lhs].CurrentAnalogValue,state[rhs].CurrentAnalogValue)));
			else
				return input.AddBinding(name,new CustomBinding((state) => state[lhs].CurrentDigitalValue || state[rhs].CurrentDigitalValue,(state) => MathF.MaxMagnitude(state[lhs].CurrentAnalogValue,state[rhs].CurrentAnalogValue)));

			return true;
		}

		/// <summary>
		/// Registers a binding with <paramref name="input"/> with the following evaluations.
		/// <para/>
		/// The boolean evaluation performs a simple logical xor of <paramref name="lhs"/> and <paramref name="rhs"/>.
		/// <para/>
		/// The analog evaluation performs a subtraction of <paramref name="rhs"/> from <paramref name="lhs"/>.
		/// </summary>
		/// <param name="input">The input manager to add the binding to.</param>
		/// <param name="name">The name of the binding to create.</param>
		/// <param name="lhs">The left hand side of the operation.</param>
		/// <param name="rhs">The right hand side of the operation.</param>
		/// <param name="put">If true, we will put the binding into <paramref name="input"/>. If false, we will add the binding instead.</param>
		/// <returns>Returns true if the binding was added and false otherwise.</returns>
		/// <remarks>
		///	This is distinct from directly creating the combined binding via operator overloading.
		///	If what <paramref name="lhs"/> or <paramref name="rhs"/> references changes, this will reference the changed bindings.
		/// </remarks>
		public static bool AddXorInput(this InputManager input, string name, string lhs, string rhs, bool put = false)
		{
			if(put)
				input.PutBinding(name,new CustomBinding((state) => state[lhs].CurrentDigitalValue ^ state[rhs].CurrentDigitalValue,(state) => state[lhs].CurrentAnalogValue - state[rhs].CurrentAnalogValue));
			else
				return input.AddBinding(name,new CustomBinding((state) => state[lhs].CurrentDigitalValue ^ state[rhs].CurrentDigitalValue,(state) => state[lhs].CurrentAnalogValue - state[rhs].CurrentAnalogValue));

			return true;
		}

		/// <summary>
		/// Registers a binding with <paramref name="input"/> with the following evaluations.
		/// <para/>
		/// The boolean evaluation performs a simple logical not of <paramref name="binding"/>.
		/// <para/>
		/// The analog evaluation performs a additive negation of <paramref name="binding"/>.
		/// </summary>
		/// <param name="input">The input manager to add the binding to.</param>
		/// <param name="name">The name of the binding to create.</param>
		/// <param name="binding">The operand.</param>
		/// <param name="put">If true, we will put the binding into <paramref name="input"/>. If false, we will add the binding instead.</param>
		/// <returns>Returns true if the binding was added and false otherwise.</returns>
		/// <remarks>
		///	This is distinct from directly creating the combined binding via operator overloading.
		///	If what <paramref name="binding"/> references changes, this will reference the changed binding.
		/// </remarks>
		public static bool AddNotInput(this InputManager input, string name, string binding, bool put = false)
		{
			if(put)
				input.PutBinding(name,new CustomBinding((state) => !state[binding].CurrentDigitalValue,(state) => -state[binding].CurrentAnalogValue));
			else
				return input.AddBinding(name,new CustomBinding((state) => !state[binding].CurrentDigitalValue,(state) => -state[binding].CurrentAnalogValue));

			return true;
		}

		/// <summary>
		/// Registers a binding with <paramref name="input"/> with the following evaluations.
		/// <para/>
		/// The boolean evaluation is identical to <paramref name="binding"/>.
		/// <para/>
		/// The analog evaluation performs a absolute value calculation of <paramref name="binding"/>.
		/// </summary>
		/// <param name="input">The input manager to add the binding to.</param>
		/// <param name="name">The name of the binding to create.</param>
		/// <param name="binding">The operand.</param>
		/// <param name="put">If true, we will put the binding into <paramref name="input"/>. If false, we will add the binding instead.</param>
		/// <returns>Returns true if the binding was added and false otherwise.</returns>
		/// <remarks>
		///	This is distinct from directly creating the combined binding via operator overloading.
		///	If what <paramref name="binding"/> references changes, this will reference the changed binding.
		/// </remarks>
		public static bool AddMagnitudeInput(this InputManager input, string name, string binding, bool put = false)
		{
			if(put)
				input.PutBinding(name,new CustomBinding((state) => state[binding].CurrentDigitalValue,(state) => MathF.Abs(state[binding].CurrentAnalogValue)));
			else
				return input.AddBinding(name,new CustomBinding((state) => state[binding].CurrentDigitalValue,(state) => MathF.Abs(state[binding].CurrentAnalogValue)));

			return true;
		}

		/// <summary>
		/// Registers a binding with <paramref name="input"/> with the following evaluations.
		/// <para/>
		/// The boolean evaluation performs a simple logical or of <paramref name="lhs"/> and <paramref name="rhs"/>.
		/// <para/>
		/// The analog evaluation performs an addition of <paramref name="lhs"/> from <paramref name="rhs"/>.
		/// </summary>
		/// <param name="input">The input manager to add the binding to.</param>
		/// <param name="name">The name of the binding to create.</param>
		/// <param name="lhs">The left hand side of the operation.</param>
		/// <param name="rhs">The right hand side of the operation.</param>
		/// <param name="put">If true, we will put the binding into <paramref name="input"/>. If false, we will add the binding instead.</param>
		/// <returns>Returns true if the binding was added and false otherwise.</returns>
		/// <remarks>
		///	This is distinct from directly creating the combined binding via operator overloading.
		///	If what <paramref name="lhs"/> or <paramref name="rhs"/> references changes, this will reference the changed bindings.
		/// </remarks>
		public static bool AddAdditionInput(this InputManager input, string name, string lhs, string rhs, bool put = false)
		{
			if(put)
				input.PutBinding(name,new CustomBinding((state) => state[lhs].CurrentDigitalValue || state[rhs].CurrentDigitalValue,(state) => state[lhs].CurrentAnalogValue + state[rhs].CurrentAnalogValue));
			else
				return input.AddBinding(name,new CustomBinding((state) => state[lhs].CurrentDigitalValue || state[rhs].CurrentDigitalValue,(state) => state[lhs].CurrentAnalogValue + state[rhs].CurrentAnalogValue));

			return true;
		}

		/// <summary>
		/// Registers a binding with <paramref name="input"/> with the following evaluations.
		/// <para/>
		/// The boolean evaluation performs a simple logical not of <paramref name="binding"/>.
		/// <para/>
		/// The analog evaluation performs a additive negation of <paramref name="binding"/>.
		/// </summary>
		/// <param name="input">The input manager to add the binding to.</param>
		/// <param name="name">The name of the binding to create.</param>
		/// <param name="binding">The operand.</param>
		/// <param name="put">If true, we will put the binding into <paramref name="input"/>. If false, we will add the binding instead.</param>
		/// <returns>Returns true if the binding was added and false otherwise.</returns>
		/// <remarks>
		///	This is distinct from directly creating the combined binding via operator overloading.
		///	If what <paramref name="binding"/> references changes, this will reference the changed binding.
		/// </remarks>
		public static bool AddInvertedInput(this InputManager input, string name, string binding, bool put = false)
		{
			if(put)
				input.PutBinding(name,new CustomBinding((state) => !state[binding].CurrentDigitalValue,(state) => -state[binding].CurrentAnalogValue));
			else
				return input.AddBinding(name,new CustomBinding((state) => !state[binding].CurrentDigitalValue,(state) => -state[binding].CurrentAnalogValue));

			return true;
		}

		/// <summary>
		/// Registers a binding with <paramref name="input"/> with the following evaluations.
		/// <para/>
		/// The boolean evaluation performs a simple logical xor of <paramref name="lhs"/> and <paramref name="rhs"/>.
		/// <para/>
		/// The analog evaluation performs a subtraction of <paramref name="rhs"/> from <paramref name="lhs"/>.
		/// </summary>
		/// <param name="input">The input manager to add the binding to.</param>
		/// <param name="name">The name of the binding to create.</param>
		/// <param name="lhs">The left hand side of the operation.</param>
		/// <param name="rhs">The right hand side of the operation.</param>
		/// <param name="put">If true, we will put the binding into <paramref name="input"/>. If false, we will add the binding instead.</param>
		/// <returns>Returns true if the binding was added and false otherwise.</returns>
		/// <remarks>
		///	This is distinct from directly creating the combined binding via operator overloading.
		///	If what <paramref name="lhs"/> or <paramref name="rhs"/> references changes, this will reference the changed bindings.
		/// </remarks>
		public static bool AddSubtractionInput(this InputManager input, string name, string lhs, string rhs, bool put = false)
		{
			if(put)
				input.PutBinding(name,new CustomBinding((state) => state[lhs].CurrentDigitalValue ^ state[rhs].CurrentDigitalValue,(state) => state[lhs].CurrentAnalogValue - state[rhs].CurrentAnalogValue));
			else
				return input.AddBinding(name,new CustomBinding((state) => state[lhs].CurrentDigitalValue ^ state[rhs].CurrentDigitalValue,(state) => state[lhs].CurrentAnalogValue - state[rhs].CurrentAnalogValue));

			return true;
		}

		/// <summary>
		/// Registers a binding with <paramref name="input"/> with the following evaluations.
		/// <para/>
		/// The boolean evaluation performs a simple logical and of <paramref name="lhs"/> and <paramref name="rhs"/>.
		/// <para/>
		/// The analog evaluation performs a multiplication of <paramref name="lhs"/> from <paramref name="rhs"/>.
		/// </summary>
		/// <param name="input">The input manager to add the binding to.</param>
		/// <param name="name">The name of the binding to create.</param>
		/// <param name="lhs">The left hand side of the operation.</param>
		/// <param name="rhs">The right hand side of the operation.</param>
		/// <param name="put">If true, we will put the binding into <paramref name="input"/>. If false, we will add the binding instead.</param>
		/// <returns>Returns true if the binding was added and false otherwise.</returns>
		/// <remarks>
		///	This is distinct from directly creating the combined binding via operator overloading.
		///	If what <paramref name="lhs"/> or <paramref name="rhs"/> references changes, this will reference the changed bindings.
		/// </remarks>
		public static bool AddMultiplicationInput(this InputManager input, string name, string lhs, string rhs, bool put = false)
		{
			if(put)
				input.PutBinding(name,new CustomBinding((state) => state[lhs].CurrentDigitalValue && state[rhs].CurrentDigitalValue,(state) => state[lhs].CurrentAnalogValue * state[rhs].CurrentAnalogValue));
			else
				return input.AddBinding(name,new CustomBinding((state) => state[lhs].CurrentDigitalValue && state[rhs].CurrentDigitalValue,(state) => state[lhs].CurrentAnalogValue * state[rhs].CurrentAnalogValue));

			return true;
		}
		#endregion

		#region Utility Bindings
		/// <summary>
		/// Adds an alias for <paramref name="binding"/>.
		/// </summary>
		/// <param name="input">The input manager to add the binding to.</param>
		/// <param name="name">The name of the binding to create.</param>
		/// <param name="binding">The binding to create an alias for.</param>
		/// <param name="put">If true, we will put the binding into <paramref name="input"/>. If false, we will add the binding instead.</param>
		/// <returns>Returns true if the binding was added and false otherwise.</returns>
		public static bool AddAliasInput(this InputManager input, string name, string binding, bool put = false)
		{
			if(put)
				input.PutBinding(name,new CustomBinding((state) => state[binding].CurrentDigitalValue,(state) => state[binding].CurrentAnalogValue));
			else
				return input.AddBinding(name,new CustomBinding((state) => state[binding].CurrentDigitalValue,(state) => state[binding].CurrentAnalogValue));

			return true;
		}

		/// <summary>
		/// Registers a binding with <paramref name="input"/> with the following evaluations.
		/// <para/>
		/// The boolean evaluation remains the same as <paramref name="binding"/>.
		/// <para/>
		/// The analog evaluation is 0 when !<paramref name="binding"/> and is <paramref name="binding"/> when <paramref name="binding"/> if <paramref name="satisfied"/>.
		/// The analog evaluation is 0 when <paramref name="binding"/> and is <paramref name="binding"/> when !<paramref name="binding"/> if !<paramref name="satisfied"/>.
		/// </summary>
		/// <param name="input">The input manager to add the binding to.</param>
		/// <param name="name">The name of the binding to create.</param>
		/// <param name="binding">The binding to turn into a delta function.</param>
		/// <param name="satisfied">If true, then <paramref name="binding"/> is clamped to 0 when <paramref name="binding"/>. Otherwise it is clamped to 0 when !<paramref name="binding"/>.</param>
		/// <param name="put">If true, we will put the binding into <paramref name="input"/>. If false, we will add the binding instead.</param>
		/// <returns>Returns true if the binding was added and false otherwise.</returns>
		public static bool AddDeltaInput(this InputManager input, string name, string binding, bool satisfied, bool put = false)
		{
			if(put)
				if(satisfied)
					input.PutBinding(name,new CustomBinding((state) => state[binding].CurrentDigitalValue,(state) => state[binding].CurrentDigitalValue ? state[binding].CurrentAnalogValue : 0.0f));
				else
					input.PutBinding(name,new CustomBinding((state) => state[binding].CurrentDigitalValue,(state) => state[binding].CurrentDigitalValue ? 0.0f : state[binding].CurrentAnalogValue));
			else
				if(satisfied)
					return input.AddBinding(name,new CustomBinding((state) => state[binding].CurrentDigitalValue,(state) => state[binding].CurrentDigitalValue ? state[binding].CurrentAnalogValue : 0.0f));
				else
					return input.AddBinding(name,new CustomBinding((state) => state[binding].CurrentDigitalValue,(state) => state[binding].CurrentDigitalValue ? 0.0f : state[binding].CurrentAnalogValue));

			return true;
		}

		/// <summary>
		/// Registers a binding with <paramref name="input"/> with the following boolean evaluation.
		/// <para/>
		/// If <paramref name="rising"/>, then the binding will evaluate to true only when <paramref name="binding"/> first goes from unsatisfied to satisfied.
		/// If !<paramref name="rising"/>, then the binding will evaluate to true only when <paramref name="binding"/> first goes from satisfied to unsatisfied.
		/// <para/>
		/// The analog evaluation function will simply map to <paramref name="binding"/>'s.
		/// </summary>
		/// <param name="input">The input manager to add the binding to.</param>
		/// <param name="name">The name of the binding to create.</param>
		/// <param name="binding">The binding to check for rising or falling edges.</param>
		/// <param name="rising">If true, we will look for rising edges of <paramref name="binding"/>. If false, we will look for falling edges instead.</param>
		/// <param name="put">If true, we will put the binding into <paramref name="input"/>. If false, we will add the binding instead.</param>
		/// <returns>Returns true if the binding was added and false otherwise.</returns>
		public static bool AddEdgeTriggeredInput(this InputManager input, string name, string binding, bool rising, bool put = false)
		{
			if(put)
				if(rising)
					input.PutBinding(name,new CustomBinding((state) => state[binding].IsRisingEdge,(state) => state[binding].CurrentAnalogValue));
				else
					input.PutBinding(name,new CustomBinding((state) => state[binding].IsFallingEdge,(state) => state[binding].CurrentAnalogValue));
			else // Add
				if(rising)
					return input.AddBinding(name,new CustomBinding((state) => state[binding].IsRisingEdge,(state) => state[binding].CurrentAnalogValue));
				else
					return input.AddBinding(name,new CustomBinding((state) => state[binding].IsFallingEdge,(state) => state[binding].CurrentAnalogValue));

			return true;
		}

		/// <summary>
		/// Adds a gamepad axis binding with a deadzone to <paramref name="input"/>.
		/// When this binding does not evaluate to true, it's analog value is fixed at 0.
		/// </summary>
		/// <param name="input">The input manager to add the binding to.</param>
		/// <param name="name">The name of the binding to create.</param>
		/// <param name="gamepad">The gamepad to use for the binding.</param>
		/// <param name="left">If true, this binds the left stick. If false, this binds the right stick.</param>
		/// <param name="x">If true, this binds an x-axis. If false, this binds a y-axis.</param>
		/// <param name="threshold">
		///	This is the (inclusive) threshold the axis must meet (in magnitude) to evaluate digitally to true.
		///	If <paramref name="exceed"/> is true, then the axis must be at least the threshold to satisfy the input.
		///	If <paramref name="exceed"/> is false, then the axis must be at most the threshold to satisfy the input.
		///	<para/>
		///	Axis values range within [-1,1].
		/// </param>
		/// <param name="exceed">
		///	If true, then the axis must be at least the threshold to satisfy the input.
		///	If false, it must be at most the threshold instead.
		///	<para/>
		///	Note that toggling this value will have the same effect on this binding as an analogous satisfied variable would have.
		/// </param>
		/// <param name="put">If true, we will put the binding into <paramref name="input"/>. If false, we will add the binding instead.</param>
		/// <returns>Returns true if the binding was added and false otherwise.</returns>
		public static bool AddGamePadAxisDeadZoneInput(this InputManager input, string name, PlayerIndex gamepad, bool left, bool x, float threshold = 1.0f, bool exceed = true, bool put = false)
		{
			InputBinding capture_me = new GamePadAxisBinding(gamepad,left,x,threshold,exceed,true);
			InputBinding no_capture_me = new GamePadAxisBinding(gamepad,left,x,threshold);

			if(put)
				input.PutBinding(name,new CustomBinding(capture_me.DigitalEvaluation,(state) => capture_me.DigitalEvaluation(state) ? no_capture_me.AnalogEvaluation(state) : 0.0f));
			else
				return input.AddBinding(name,new CustomBinding(capture_me.DigitalEvaluation,(state) => capture_me.DigitalEvaluation(state) ? no_capture_me.AnalogEvaluation(state) : 0.0f));

			return true;
		}

		/// <summary>
		/// Adds a axis binding with <paramref name="input"/>.
		/// It simulates an axis by binding two digital inputs to a single new input <paramref name="name"/>.
		/// The axis is negative when <paramref name="negative"/> is true and positive when <paramref name="positive"/> is true.
		/// If both are true, then the axis is zero.
		/// <para/>
		/// The digital evaluation of this input is simply the logical or of <paramref name="negative"/> and <paramref name="positive"/>.
		/// </summary>
		/// <param name="input">The input manager to add the binding to.</param>
		/// <param name="name">The name of the binding to create.</param>
		/// <param name="negative">The digital input that makes the axis go negative.</param>
		/// <param name="positive">The digital input that makes the axis go positive.</param>
		/// <param name="scale">The magnitude of the axis when <paramref name="negative"/> or <paramref name="positive"/> moves it away from zero.</param>
		/// <param name="put">If true, we will put the binding into <paramref name="input"/>. If false, we will add the binding instead.</param>
		/// <returns>Returns true if the binding was added and false otherwise.</returns>
		public static bool AddDigitalAxis(this InputManager input, string name, string negative, string positive, float scale = 1.0f, bool put = false)
		{
			float nscale = -scale; // Do this once and capture it

			return input.AddCustomBinding(name,(state) => state[negative].CurrentDigitalValue || state[positive].CurrentDigitalValue,(state) =>
			{
				if(state[negative].CurrentDigitalValue)
					if(state[positive].CurrentDigitalValue)
						return 0.0f; // If both are true, they cancel each other out
					else // Positive is false
						return nscale;
				else // Negative is false
					if(state[positive].CurrentDigitalValue)
						return scale;
					else // Positive is false
						return 0.0f;
			},put);
		}

		/// <summary>
		/// Adds a reference binding for binding 'by reference'.
		/// In C# 10, this will allow for accessing an InputBinding variable by reference (i.e. we obtain the variable's address rather than its pointer value).
		/// </summary>
		/// <param name="input">The input manager to add the binding to.</param>
		/// <param name="name">The name of the binding to create.</param>
		/// <param name="binding">The roundabout function to provide the input binding by reference.</param>
		/// <param name="put">If true, we will put the binding into <paramref name="input"/>. If false, we will add the binding instead.</param>
		/// <returns>Returns true if the binding was added and false otherwise.</returns>
		public static bool AddReferenceInput(this InputManager input, string name, RefInputBinding binding, bool put = false)
		{
			if(put)
				input.PutBinding(name,new ReferenceBinding(binding));
			else
				return input.AddBinding(name,new ReferenceBinding(binding));

			return true;
		}

		/// <summary>
		/// Adds an input that repeats every <paramref name="turbo_delay"/> seconds after <paramref name="turbo_delay"/> seconds while the backing input binding <paramref name="binding"/> is true.
		/// <para/>
		/// This input will first be true when <paramref name="binding"/>'s rising edge occurs (unless <paramref name="skip_first"/> is true).
		/// It will them produce rising edges every 2 * <paramref name="turbo_delay"/> seconds after <paramref name="initial_delay"/> seconds so long as <paramref name="binding"/> remains true.
		/// A falling edge occurs evenly interleaved between rising edges.
		/// <para/>
		/// When permitted, the initial <paramref name="binding"/> satisfaction will create a rising edge for <paramref name="turbo_delay"/> seconds.
		/// If this is longer than <paramref name="initial_delay"/>, then the initial satisfaction will overflow into the first however many turbo triggers.
		/// <para/>
		/// The analog evaluation function will produce the number of up edges this has produced (including the initial one if not skipped).
		/// This value resets every time <paramref name="binding"/> becomes unsatisfied.
		/// </summary>
		/// <param name="input">The input manager to add the binding to.</param>
		/// <param name="name">The name of the binding to create.</param>
		/// <param name="binding">The base binding this input will build on top of as described above.</param>
		/// <param name="initial_delay">The delay before turbo kicks in. If this value is negative, it will be set to 0.0f.</param>
		/// <param name="turbo_delay">The delay between turbo edges (rising and falling). This value is only as useful as the update time deltas are grainular. If this value is negative, it will be set to 0.0f.</param>
		/// <param name="skip_first">If true, then the initial <paramref name="binding"/> satisfaction will not create a rising edge (or the subsequent falling edge).</param>
		/// <param name="put">If true, we will put the binding into <paramref name="input"/>. If false, we will add the binding instead.</param>
		/// <returns>Returns true if the binding was added and false otherwise.</returns>
		public static bool AddTurboInput(this InputManager input, string name, string binding, float initial_delay = 1.0f, float turbo_delay = 0.1f, bool skip_first = false, bool put = false)
		{
			// Sanity check delays
			initial_delay = MathF.Max(initial_delay,0.0f);
			turbo_delay = MathF.Max(turbo_delay,0.0f);

			// Variables to capture and remember in the input function
			// One is created for every input created
			bool repeating = true;
			bool first_unfinished = true;
			bool was_true = false;
			float time_stamp = 0.0f;
			int total = 0;

			InputBinding cb = new CustomBinding((state) =>
			{
				// If our binding is not satisfied, then just reset the system state
				if(!state[binding].CurrentDigitalValue)
				{
					first_unfinished = true;
					total = 0;

					return false;
				}

				// If this is the first time the binding is satisfied, then we initialize the system
				if(first_unfinished)
				{
					// We start the wait for repeating
					first_unfinished = false;

					// If we produce an initial up edge, record it
					total += skip_first ? 0 : 1;

					// We may need to skip the first delay
					repeating = skip_first;
					was_true = !skip_first;

					// The delay to drop from an up edge is always the same
					time_stamp = state.CurrentTime + (skip_first ? initial_delay : turbo_delay);

					// Create the first rising edge if necessary
					return !skip_first;
				}
					
				// We know we've started the first up edge, so if we've passed the time we need to get to, it's time 
				if(time_stamp <= state.CurrentTime)
				{
					// Toggle the edge from up to down or down to up
					was_true = !was_true;

					if(repeating)
						time_stamp = state.CurrentTime + turbo_delay; // This will ensure that even if something weird has happened, the edge will toggle in turbo_delay perceived seconds
					else
					{
						// We've passed the initial delay and should start repeating
						repeating = true;

						// If we skipped the first up edge, then we want to immediately hit an up edge on the next cycle and so don't update our timestamp
						if(skip_first)
							was_true = false; // If we skipped the first up edge, then we've incorrectly set was_true to true and should make it false
						else // If we didn't skip the first up edge, then we need to finish our initial delay
							time_stamp += initial_delay - turbo_delay;
					}

					// If we created a rising edge, record it
					if(was_true)
						total += 1;
				}

				// This will either maintain the status quo or create a rising/falling edge
				return was_true;
			},(state) => total);

			if(put)
				input.PutBinding(name,cb);
			else
				input.AddBinding(name,cb);

			return true;
		}
		#endregion
	}
}
