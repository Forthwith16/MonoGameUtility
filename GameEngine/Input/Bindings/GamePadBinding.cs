using Microsoft.Xna.Framework;

namespace GameEngine.Input.Bindings
{
	/// <summary>
	/// Encapsulates a gamepad binding.
	/// </summary>
	public abstract class GamePadBinding : InputBinding
	{
		/// <summary>
		/// Creates a gamepad binding.
		/// </summary>
		/// <param name="gamepad">The gamepad to use for the binding.</param>
		protected GamePadBinding(PlayerIndex gamepad) : base()
		{
			Gamepad = gamepad;
			return;
		}

		public override string ToString()
		{return "GamePad Binding";}

		/// <summary>
		/// The index of the gamepad this binding uses kept for record-keeping purposes.
		/// </summary>
		public PlayerIndex Gamepad
		{get; init;}

		/// <summary>
		/// True iff this is a gamepad button binding.
		/// </summary>
		public virtual bool IsButtonBinding
		{get => false;}

		/// <summary>
		/// True iff this is a gamepad trigger binding.
		/// </summary>
		public virtual bool IsTriggerBinding
		{get => false;}

		/// <summary>
		/// True iff this is a gamepad stick binding.
		/// </summary>
		public virtual bool IsStickBinding
		{get => false;}

		/// <summary>
		/// True iff this is a gamepad axis binding.
		/// </summary>
		public virtual bool IsAxisBinding
		{get => false;}

		public override bool IsGamepadBinding
		{get => true;}
	}
}
