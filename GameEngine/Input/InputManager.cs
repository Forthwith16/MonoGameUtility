using GameEngine.GameComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace GameEngine.Input
{
	/// <summary>
	/// Manages and maps input while keeping track of any additional state information for registered inputs.
	/// This synchronizes input states so that all input bindings use the same state information.
	/// This also has the advantage of limiting the number of state queries to MonoGame or lower level system calls as the information is centralized.
	/// <para/>
	/// The binding update order occurs in the order that the bindings were added.
	/// Bindings updated later will be able to rely upon up to date values of bindings updated sooner.
	/// </summary>
	public class InputManager : BareGameComponent
	{
		/// <summary>
		/// Creates a new input manager.
		/// </summary>
		public InputManager()
		{
			Records = new Dictionary<string,DynamicInputRecord>();
			Bindings = new LinkedList<string>();

			// In case this are different from the default constructor's values
			GamepadOne = GamePadState.Default;
			GamepadTwo = GamePadState.Default;
			GamepadThree = GamePadState.Default;
			GamepadFour = GamePadState.Default;
			
			// Assign the default fetch functions
			KeyboardFetch = Keyboard.GetState;
			MouseFetch = Mouse.GetState;
			GamePadOneFetch = () => GamePad.GetState(PlayerIndex.One); // These will default if the controller isn't connected
			GamePadTwoFetch = () => GamePad.GetState(PlayerIndex.Two);
			GamePadThreeFetch = () => GamePad.GetState(PlayerIndex.Three);
			GamePadFourFetch = () => GamePad.GetState(PlayerIndex.Four);
			TouchPanelFetch = () => TouchPanel.GetState();
			
			CurrentTime = 0.0f;
			return;
		}

		public override void Update(GameTime delta)
		{
			// Update the keyboard state
			Keys = KeyboardFetch();
			
			// Update the mouse state
			PreviousMouse = CurrentMouse;
			CurrentMouse = MouseFetch();
			
			// Update the gamepad states
			GamepadOne = GamePadOneFetch();
			GamepadTwo = GamePadTwoFetch();
			GamepadThree = GamePadThreeFetch();
			GamepadFour = GamePadFourFetch();
			
			// Update the touch state
			PreviousTouch = CurrentTouch;
			CurrentTouch = TouchPanelFetch();

			// Update the time
			CurrentTime += (float)delta.ElapsedGameTime.TotalSeconds;

			// Update our record of input satisfaction states
			foreach(string name in Bindings)
				Records[name].Update();
			
			return;
		}

		/// <summary>
		/// Gets the current state of a gamepad.
		/// </summary>
		/// <param name="index">The index of the gamepad to fetch.</param>
		/// <returns>Returns the state of the <paramref name="index"/>th gamepad.</returns>
		/// <exception cref="ArgumentException">Thrown if <paramref name="index"/> is not valid.</exception>
		public GamePadState Gamepad(PlayerIndex index)
		{
			switch(index)
			{
			case PlayerIndex.One:
				return GamepadOne;
			case PlayerIndex.Two:
				return GamepadTwo;
			case PlayerIndex.Three:
				return GamepadThree;
			case PlayerIndex.Four:
				return GamepadFour;
			}

			throw new ArgumentException("The provided player index was invalid");
		}

		/// <summary>
		/// Adds a new input binding to this manager.
		/// </summary>
		/// <param name="name">The name of the new binding.</param>
		/// <param name="binding">The means by which the binding is evaluated.</param>
		/// <returns>Returns true if the binding was added and false otherwise, such as if there was already a binding named <paramref name="name"/>.</returns>
		public bool AddBinding(string name, InputBinding binding)
		{
			if(Records.TryAdd(name,new DynamicInputRecord(this,binding)))
			{
				Bindings.AddLast(name);
				return true;
			}

			return false;
		}

		/// <summary>
		/// Adds or replaces an input binding.
		/// If replacing, the old record is repurposed, preserving its state information.
		/// The replacement also retains the old binding's update order.
		/// </summary>
		/// <param name="name">The name of the binding to add/replace.</param>
		/// <param name="binding">The means by which the binding is evaluated.</param>
		public void PutBinding(string name, InputBinding binding)
		{
			if(Records.TryGetValue(name,out DynamicInputRecord? record))
				record.Binding = binding;
			else
				AddBinding(name,binding);

			return;
		}

		/// <summary>
		/// Removes the binding named <paramref name="name"/>.
		/// </summary>
		/// <param name="name">The name of the binding to remove.</param>
		/// <returns>Returns true if the binding was removed and false otherwise.</returns>
		public bool RemoveBinding(string name)
		{
			if(Records.Remove(name))
			{
				Bindings.Remove(name);
				return true;
			}

			return false;
		}

		/// <summary>
		/// Determines if this with a binding named <paramref name="name"/>.
		/// </summary>
		/// <param name="name">The name of the binding to check.</param>
		/// <returns>Returns true if this has a binding named <paramref name="name"/> and false otherwise.</returns>
		public bool HasBinding(string name)
		{return Records.ContainsKey(name);}

		/// <summary>
		/// Eat's the binding named <paramref name="name"/>, indicating that it should, in general, not be processed further this update.
		/// Users are free to ignore this flag, but it has a compelling use case in GUI processing.
		/// </summary>
		/// <param name="name">The name of the binding.</param>
		/// <exception cref="KeyNotFoundException">Thrown if <paramref name="name"/> is not a binding name.</exception>
		public void EatBinding(string name)
		{
			Records[name].Eaten = true;
			return;
		}

		/// <summary>
		/// Obtains the current record of an input binding named <paramref name="name"/>.
		/// </summary>
		/// <param name="name">The name of the binding.</param>
		/// <exception cref="KeyNotFoundException">Thrown if <paramref name="name"/> is not a binding name.</exception>
		public InputRecord this[string name] => Records[name];

		/// <summary>
		/// The input binding records.
		/// </summary>
		protected Dictionary<string,DynamicInputRecord> Records
		{get; init;}

		/// <summary>
		/// The order in which extant bindings were added.
		/// We update the bindings in the order they appear here.
		/// </summary>
		/// <remarks>We don't expect to be adding/remove bindings very often, so this data structure is perfect for traversal while retaining order.</remarks>
		protected LinkedList<string> Bindings
		{get; init;}

		/// <summary>
		/// The number of bindings in this input manager.
		/// </summary>
		public int Count => Records.Count;

		/// <summary>
		/// The current state of the keyboard.
		/// </summary>
		public KeyboardState Keys
		{get; protected set;}

		/// <summary>
		/// The means by which we obtain keyboard input.
		/// This will default to simply grabbing MonoGame's input.
		/// Changing this value will allow for total/partial input spoofing, but this can be dangerous if improperly done.
		/// </summary>
		public KeyboardStateFetcher KeyboardFetch
		{get; set;}

		/// <summary>
		/// The current mouse state.
		/// </summary>
		public MouseState CurrentMouse
		{get; protected set;}

		/// <summary>
		/// The previous mouse state.
		/// </summary>
		public MouseState PreviousMouse
		{get; protected set;}

		/// <summary>
		/// The means by which we obtain mouse input.
		/// This will default to simply grabbing MonoGame's input.
		/// Changing this value will allow for total/partial input spoofing, but this can be dangerous if improperly done.
		/// </summary>
		public MouseStateFetcher MouseFetch
		{get; set;}

		/// <summary>
		/// The current state of the first gamepad.
		/// </summary>
		public GamePadState GamepadOne
		{get; protected set;}

		/// <summary>
		/// The means by which we obtain game pad input for the first game pad.
		/// This will default to simply grabbing MonoGame's input.
		/// Changing this value will allow for total/partial input spoofing, but this can be dangerous if improperly done.
		/// </summary>
		public GamePadStateFetcher GamePadOneFetch
		{get; set;}

		/// <summary>
		/// The current state of the first gamepad.
		/// </summary>
		public GamePadState GamepadTwo
		{get; protected set;}

		/// <summary>
		/// The means by which we obtain game pad input for the second game pad.
		/// This will default to simply grabbing MonoGame's input.
		/// Changing this value will allow for total/partial input spoofing, but this can be dangerous if improperly done.
		/// </summary>
		public GamePadStateFetcher GamePadTwoFetch
		{get; set;}

		/// <summary>
		/// The current state of the third gamepad.
		/// </summary>
		public GamePadState GamepadThree
		{get; protected set;}

		/// <summary>
		/// The means by which we obtain game pad input for the third game pad.
		/// This will default to simply grabbing MonoGame's input.
		/// Changing this value will allow for total/partial input spoofing, but this can be dangerous if improperly done.
		/// </summary>
		public GamePadStateFetcher GamePadThreeFetch
		{get; set;}

		/// <summary>
		/// The current state of the fourth gamepad.
		/// </summary>
		public GamePadState GamepadFour
		{get; protected set;}

		/// <summary>
		/// The means by which we obtain game pad input for the fourth game pad.
		/// This will default to simply grabbing MonoGame's input.
		/// Changing this value will allow for total/partial input spoofing, but this can be dangerous if improperly done.
		/// </summary>
		public GamePadStateFetcher GamePadFourFetch
		{get; set;}

		/// <summary>
		/// The current state of the touch input.
		/// </summary>
		public TouchCollection CurrentTouch
		{get; protected set;}

		/// <summary>
		/// The previous state of the touch input.
		/// </summary>
		public TouchCollection PreviousTouch
		{get; protected set;}

		/// <summary>
		/// The means by which we obtain touch input.
		/// This will default to simply grabbing MonoGame's default touch panel input.
		/// Changing this will allow for total/partial input spoofing, but this can be dangerous if improperly done.
		/// It can also change the Window the touch input is coming from.
		/// </summary>
		public TouchPanelStateFetcher TouchPanelFetch
		{get; set;}

		/// <summary>
		/// The current time (in seconds) according to this input manager.
		/// This is also the total <i>enabled update</i> time this input manager has experienced and may be distinct from the total elapsed game time.
		/// When syncing times to inputs, use this value for inputs managed from this manager.
		/// </summary>
		public float CurrentTime
		{get; protected set;}
	}

	/// <summary>
	/// A record of an input binding's digital and analog values.
	/// </summary>
	public class DynamicInputRecord
	{
		/// <summary>
		/// Creates a new input record initialized so that it has never been satisfied nor unsatisfied.
		/// </summary>
		/// <param name="parent">The parent input manager.</param>
		/// <param name="binding">The input binding.</param>
		public DynamicInputRecord(InputManager parent, InputBinding binding)
		{
			Parent = parent;
			Binding = binding;

			CurrentDigitalValue = false;
			PreviousDigitalValue = false;

			CurrentAnalogValue = 0.0f;
			PreviousAnalogValue = 0.0f;

			WhenLastSatisfied = float.NegativeInfinity;
			WhenLastUnsatisfied = float.NegativeInfinity;

			State = RecordState.New;
			Eaten = false;

			return;
		}

		/// <summary>
		/// Converts a dynamic input record into a static version.
		/// </summary>
		/// <param name="record">The record to convert.</param>
		public static implicit operator InputRecord(DynamicInputRecord record)
		{return new InputRecord(record);}

		/// <summary>
		/// Updates this record.
		/// </summary>
		public void Update()
		{
			// First thing's first, uneat this input (regardless of if it was before or not)
			Eaten = false;

			// The first update doesn't care about previous values, but it makes the code far more readable to just drop two clock cycles on it anyway
			PreviousDigitalValue = CurrentDigitalValue;
			PreviousAnalogValue = CurrentAnalogValue;

			CurrentDigitalValue = Digital(Parent);
			CurrentAnalogValue = Analog(Parent);

			switch(State)
			{
			case RecordState.New: // Only current values are valid
				if(CurrentDigitalValue)
					WhenLastSatisfied = Parent.CurrentTime;
				else
					WhenLastUnsatisfied = Parent.CurrentTime;

				State = RecordState.Once; // We ALWAYS go down this route
				break;
			case RecordState.Once: // Current and previous values are valid, as is one of the whens
				if(CurrentDigitalValue)
					if(PreviousDigitalValue)
						State = RecordState.Twice;
					else
					{
						WhenLastSatisfied = Parent.CurrentTime;
						State = RecordState.Ready;
					}
				else
					if(PreviousDigitalValue)
					{
						WhenLastUnsatisfied = Parent.CurrentTime;
						State = RecordState.Ready;
					}
					else
						State = RecordState.Twice;

				break;
			case RecordState.Twice: // Current and previous values are avlid, as it one of the whens
				if(CurrentDigitalValue)
				{
					if(!PreviousDigitalValue)
					{
						WhenLastSatisfied = Parent.CurrentTime;
						State = RecordState.Ready;
					}
				}
				else if(PreviousDigitalValue)
				{
					WhenLastUnsatisfied = Parent.CurrentTime;
					State = RecordState.Ready;
				}
				
				break;
			case RecordState.Ready: // Everything is valid, so just update the whens
				if(CurrentDigitalValue)
				{
					if(!PreviousDigitalValue)
						WhenLastSatisfied = Parent.CurrentTime;
				}
				else if(PreviousDigitalValue)
					WhenLastUnsatisfied = Parent.CurrentTime;

				break;
			}

			return;
		}

		/// <summary>
		/// The input manager this record belongs to.
		/// </summary>
		public InputManager Parent
		{get; init;}

		/// <summary>
		/// The input binding.
		/// </summary>
		public InputBinding Binding
		{get; set;}

		/// <summary>
		/// The digital value evaluation.
		/// </summary>
		public InputDigitalValue Digital => Binding.DigitalEvaluation;

		/// <summary>
		/// The analog value evaluation.
		/// </summary>
		public InputAnalogValue Analog => Binding.AnalogEvaluation;

		/// <summary>
		/// If true, then this is a new record that has never been updated.
		/// </summary>
		public RecordState State
		{get; private set;}

		/// <summary>
		/// If true, this input has been eaten and, in general, should not be processed further.
		/// </summary>
		/// <remarks>The setter is fine to make public because the InputManager does not expose its DynamicRecords to users.</remarks>
		public bool Eaten
		{get; set;}

		/// <summary>
		/// The digital value of this record.
		/// </summary>
		public bool CurrentDigitalValue
		{get; private set;}

		/// <summary>
		/// The analog value of this record.
		/// </summary>
		public float CurrentAnalogValue
		{get; private set;}

		/// <summary>
		/// The previous digital value of this record.
		/// </summary>
		public bool PreviousDigitalValue
		{get; private set;}

		/// <summary>
		/// The previous analog value of this record.
		/// </summary>
		public float PreviousAnalogValue
		{get; private set;}

		/// <summary>
		/// True when this is a rising edge.
		/// </summary>
		public bool IsRisingEdge => CurrentDigitalValue && !PreviousDigitalValue;

		/// <summary>
		/// True when this is a falling edge.
		/// </summary>
		public bool IsFallingEdge => PreviousDigitalValue && !CurrentDigitalValue;

		/// <summary>
		/// The last time this record was first digitally satisfied.
		/// </summary>
		public float WhenLastSatisfied
		{get; private set;}

		/// <summary>
		/// The duration (in seconds) that this record has currently been satisfied.
		/// <para/>
		/// This value only has meaning when this record is satisfied and in at least a Once state.
		/// </summary>
		public float LengthSatisfied => Parent.CurrentTime - WhenLastSatisfied;

		/// <summary>
		/// The duration (in seconds) that this record last remained satisfied.
		/// <para/>
		/// This value only has meaning when this record is currently unsatisfied and in a Ready state
		/// </summary>
		public float LastLengthSatisfied => WhenLastUnsatisfied - WhenLastSatisfied;

		/// <summary>
		/// The last time when this record was first digitally unsatisfied.
		/// </summary>
		public float WhenLastUnsatisfied
		{get; private set;}

		/// <summary>
		/// The duration (in seconds) that this record has currently been unsatisfied.
		/// <para/>
		/// This value only has meaning when this record is unsatisfied and in at least a Once state.
		/// </summary>
		public float LengthUnsatisfied => Parent.CurrentTime - WhenLastUnsatisfied;

		/// <summary>
		/// The duration (in seconds) that this record last remained unsatisfied.
		/// <para/>
		/// This value only has meaning when this record is currently unsatisfied and in a Ready state
		/// </summary>
		public float LastLengthUnatisfied => WhenLastSatisfied - WhenLastUnsatisfied;
	}

	/// <summary>
	/// An input record for a binding.
	/// </summary>
	public readonly struct InputRecord
	{
		/// <summary>
		/// Copies a dynamic record into an immutable, static form.
		/// </summary>
		/// <param name="record">The input record to copy.</param>
		public InputRecord(DynamicInputRecord record)
		{
			Binding = record.Binding;
			CurrentTime = record.Parent.CurrentTime;

			CurrentDigitalValue = record.CurrentDigitalValue;
			PreviousDigitalValue = record.PreviousDigitalValue;

			CurrentAnalogValue = record.CurrentAnalogValue;
			PreviousAnalogValue = record.PreviousAnalogValue;

			WhenLastSatisfied = record.WhenLastSatisfied;
			WhenLastUnsatisfied = record.WhenLastUnsatisfied;

			State = record.State;
			Eaten = record.Eaten;

			return;
		}

		/// <summary>
		/// The input binding.
		/// </summary>
		public InputBinding Binding
		{get; init;}

		/// <summary>
		/// When this input record was made.
		/// </summary>
		public float CurrentTime
		{get; init;}

		/// <summary>
		/// If true, then this is a new record that has never been updated.
		/// </summary>
		public RecordState State
		{get; init;}

		/// <summary>
		/// If true, this input has been eaten and, in general, should not be processed further.
		/// </summary>
		public bool Eaten
		{get; init;}

		/// <summary>
		/// The digital value of this record.
		/// </summary>
		public bool CurrentDigitalValue
		{get; init;}

		/// <summary>
		/// The analog value of this record.
		/// </summary>
		public float CurrentAnalogValue
		{get; init;}

		/// <summary>
		/// The previous digital value of this record.
		/// </summary>
		public bool PreviousDigitalValue
		{get; init;}

		/// <summary>
		/// The previous analog value of this record.
		/// </summary>
		public float PreviousAnalogValue
		{get; init;}

		/// <summary>
		/// True when this is a rising edge.
		/// </summary>
		public bool IsRisingEdge => CurrentDigitalValue && !PreviousDigitalValue;

		/// <summary>
		/// True when this is a falling edge.
		/// </summary>
		public bool IsFallingEdge => PreviousDigitalValue && !CurrentDigitalValue;

		/// <summary>
		/// The last time this record was first digitally satisfied.
		/// </summary>
		public float WhenLastSatisfied
		{get; init;}

		/// <summary>
		/// The duration (in seconds) that this record has currently been satisfied.
		/// <para/>
		/// This value only has meaning when this record is satisfied and in at least a Once state.
		/// </summary>
		public float LengthSatisfied => CurrentTime - WhenLastSatisfied;

		/// <summary>
		/// The duration (in seconds) that this record last remained satisfied.
		/// <para/>
		/// This value only has meaning when this record is currently unsatisfied and in a Ready state
		/// </summary>
		public float LastLengthSatisfied => WhenLastUnsatisfied - WhenLastSatisfied;

		/// <summary>
		/// The last time when this record was first digitally unsatisfied.
		/// </summary>
		public float WhenLastUnsatisfied
		{get; init;}

		/// <summary>
		/// The duration (in seconds) that this record has currently been unsatisfied.
		/// <para/>
		/// This value only has meaning when this record is unsatisfied and in at least a Once state.
		/// </summary>
		public float LengthUnsatisfied => CurrentTime - WhenLastUnsatisfied;

		/// <summary>
		/// The duration (in seconds) that this record last remained unsatisfied.
		/// <para/>
		/// This value only has meaning when this record is currently unsatisfied and in a Ready state
		/// </summary>
		public float LastLengthUnatisfied => WhenLastSatisfied - WhenLastUnsatisfied;
	}

	/// <summary>
	/// Denotes the state of an input record.
	/// <para/>
	/// New means it has never been updated.
	/// None of its values can be trusted.
	/// <para/>
	/// Once means it been updated at least once.
	/// Its current values can be trusted as can the timing information for its current digital evaluation.
	/// <para/>
	/// Twice means it has been updated at least twice.
	/// Its current and previous values can be trusted as can the timing information for its current digital evaluation.
	/// <para/>
	/// Ready means all of its values can be trusted.
	/// This state is entered into once its boolean value has flipped at least twice
	/// </summary>
	public enum RecordState
	{
		New,
		Once,
		Twice,
		Ready
	}

	/// <summary>
	/// Obtains the state of the keyboard.
	/// </summary>
	/// <remarks>This abstraction allows for total/partial keyboard input faking or just pulling the actual input values.</remarks>
	public delegate KeyboardState KeyboardStateFetcher();

	/// <summary>
	/// Obtains the state of the mouse.
	/// </summary>
	/// <remarks>This abstraction allows for total/partial mouse input faking or just pulling the actual input values.</remarks>
	public delegate MouseState MouseStateFetcher();

	/// <summary>
	/// Obtains the state of a gamepad.
	/// </summary>
	/// <remarks>This abstraction allows for total/partial gamepad input faking or just pulling the actual input values.</remarks>
	public delegate GamePadState GamePadStateFetcher();

	/// <summary>
	/// Obtains the state of the touch panel.
	/// </summary>
	/// <returns>This abstraction allows for total/partial touch input faking or just pulling the actual input values. It also allows us to differentiate between touch inputs from different Window sources.</returns>
	public delegate TouchCollection TouchPanelStateFetcher();
}
