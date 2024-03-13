using GameEngine.Input;
using GameEngine.Input.Bindings;
using GameEngine.Input.Bindings.MouseBindings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GameEngine.Framework
{
	/// <summary>
	/// A set of global constants.
	/// </summary>
	public static class GlobalConstants
	{
		/// <summary>
		/// Constructs certain objects where order matters.
		/// </summary>
		/// <remarks>This is called last in static initialization order.</remarks>
		static GlobalConstants()
		{
			GUIDigitalClick = GUIDigitalClickMain | GUIDigitalClickAlt;
			GUIDigitalUp = GUIDigitalUpMain | GUIDigitalUpAlt;
			GUIDigitalDown = GUIDigitalDownMain | GUIDigitalDownAlt;
			GUIDigitalLeft = GUIDigitalLeftMain | GUIDigitalLeftAlt;
			GUIDigitalRight = GUIDigitalRightMain | GUIDigitalRightAlt;

			return;
		}

		#region Debug Settings
		/// <summary>
		/// Flags if debug information should be drawn (true) or not (false).
		/// Game classes are primarily responsible for checking this value, and RenderTargetFriendlyGames do this by default.
		/// </summary>
		public static bool DrawDebugInformation = true;
		#endregion

		#region GUI Input Settings
		/// <summary>
		/// The mouse left click binding for GUIs.
		/// </summary>
		public static InputBinding GUIMouseLeftClickDefault = new MouseButtonBinding(MouseButton.Left);

		/// <summary>
		/// The mouse x-axis binding for GUIs.
		/// </summary>
		public static InputBinding GUIMouseXAxis = new MouseAxisBinding(true);

		/// <summary>
		/// The mouse y-axis binding for GUIs.
		/// </summary>
		public static InputBinding GUIMouseYAxis = new MouseAxisBinding(false);

		/// <summary>
		/// The mouse x-axis delta binding for GUIs.
		/// </summary>
		public static InputBinding GUIMouseXAxisDelta = new MouseAxisBinding(true,true);

		/// <summary>
		/// The mouse y-axis delta binding for GUIs.
		/// </summary>
		public static InputBinding GUIMouseYAxisDelta = new MouseAxisBinding(false,true);

		/// <summary>
		/// The digital click binding for GUIs.
		/// </summary>
		public static InputBinding GUIDigitalClick
		{get; private set;}

		/// <summary>
		/// The digital click main binding for GUIs.
		/// </summary>
		public static InputBinding GUIDigitalClickMain
		{
			get => _gdcm;

			set
			{
				if(_gdcm == value)
				return;

				_gdcm = value;
				GUIDigitalClick = GUIDigitalClickMain | GUIDigitalClickAlt;

				return;
			}
		}

		private static InputBinding _gdcm = new KeyBinding(Keys.Enter);

		/// <summary>
		/// The digital click alt binding for GUIs.
		/// </summary>
		public static InputBinding GUIDigitalClickAlt
		{
			get => _gdca;

			set
			{
				if(_gdca == value)
				return;

				_gdca = value;
				GUIDigitalClick = GUIDigitalClickMain | GUIDigitalClickAlt;

				return;
			}
		}

		private static InputBinding _gdca = new GamePadButtonBinding(PlayerIndex.One,Buttons.A);

		/// <summary>
		/// The digital up binding for GUIs.
		/// </summary>
		public static InputBinding GUIDigitalUp
		{get; private set;}

		/// <summary>
		/// The digital up main binding for GUIs.
		/// </summary>
		public static InputBinding GUIDigitalUpMain
		{
			get => _gdum;

			set
			{
				if(_gdum == value)
				return;

				_gdum = value;
				GUIDigitalUp = GUIDigitalUpMain | GUIDigitalUpAlt;

				return;
			}
		}

		private static InputBinding _gdum = new KeyBinding(Keys.Up);

		/// <summary>
		/// The digital up alt binding for GUIs.
		/// </summary>
		public static InputBinding GUIDigitalUpAlt
		{
			get => _gdua;

			set
			{
				if(_gdua == value)
				return;

				_gdua = value;
				GUIDigitalUp = GUIDigitalUpMain | GUIDigitalUpAlt;

				return;
			}
		}

		private static InputBinding _gdua = new GamePadButtonBinding(PlayerIndex.One,Buttons.DPadUp);

		/// <summary>
		/// The digital down binding for GUIs.
		/// </summary>
		public static InputBinding GUIDigitalDown
		{get; private set;}

		/// <summary>
		/// The digital down main binding for GUIs.
		/// </summary>
		public static InputBinding GUIDigitalDownMain
		{
			get => _gddm;

			set
			{
				if(_gddm == value)
				return;

				_gddm = value;
				GUIDigitalDown = GUIDigitalDownMain | GUIDigitalDownAlt;

				return;
			}
		}

		private static InputBinding _gddm = new KeyBinding(Keys.Down);

		/// <summary>
		/// The digital down alt binding for GUIs.
		/// </summary>
		public static InputBinding GUIDigitalDownAlt
		{
			get => _gdda;

			set
			{
				if(_gdda == value)
				return;

				_gdda = value;
				GUIDigitalDown = GUIDigitalDownMain | GUIDigitalDownAlt;

				return;
			}
		}

		private static InputBinding _gdda = new GamePadButtonBinding(PlayerIndex.One,Buttons.DPadDown);

		/// <summary>
		/// The digital left binding for GUIs.
		/// </summary>
		public static InputBinding GUIDigitalLeft
		{get; private set;}

		/// <summary>
		/// The digital left main binding for GUIs.
		/// </summary>
		public static InputBinding GUIDigitalLeftMain
		{
			get => _gdlm;

			set
			{
				if (_gdlm == value)
				return;

				_gdlm = value;
				GUIDigitalLeft = GUIDigitalLeftMain | GUIDigitalLeftAlt;

				return;
			}
		}

		private static InputBinding _gdlm = new KeyBinding(Keys.Left);

		/// <summary>
		/// The digital left alt binding for GUIs.
		/// </summary>
		public static InputBinding GUIDigitalLeftAlt
		{
			get => _gdla;

			set
			{
				if (_gdla == value)
				return;

				_gdla = value;
				GUIDigitalLeft = GUIDigitalLeftMain | GUIDigitalLeftAlt;

				return;
			}
		}

		private static InputBinding _gdla = new GamePadButtonBinding(PlayerIndex.One,Buttons.DPadLeft);

		/// <summary>
		/// The digital right binding for GUIs.
		/// </summary>
		public static InputBinding GUIDigitalRight
		{ get; private set; }

		/// <summary>
		/// The digital right main binding for GUIs.
		/// </summary>
		public static InputBinding GUIDigitalRightMain
		{
			get => _gdrm;

			set
			{
				if(_gdrm == value)
				return;

				_gdrm = value;
				GUIDigitalRight = GUIDigitalRightMain | GUIDigitalRightAlt;

				return;
			}
		}

		private static InputBinding _gdrm = new KeyBinding(Keys.Right);

		/// <summary>
		/// The digital right alt binding for GUIs.
		/// </summary>
		public static InputBinding GUIDigitalRightAlt
		{
			get => _gdra;

			set
			{
				if(_gdra == value)
				return;

				_gdra = value;
				GUIDigitalRight = GUIDigitalRightMain | GUIDigitalRightAlt;

				return;
			}
		}

		private static InputBinding _gdra = new GamePadButtonBinding(PlayerIndex.One,Buttons.DPadRight);
		#endregion

		#region GUI Draw Settings
		/// <summary>
		/// The draw layer for GUI tooltips.
		/// </summary>
		public const float TooltipDrawLayer = 0.0f; // Within a GUICore, this is always the topmost layer
		#endregion

		#region Math Constants
		/// <summary>
		/// If two floats are within this distance, they are to be considered equal.
		/// </summary>
		public const float EPSILON = 0.00001f;
		
		/// <summary>
		/// If a float squared is at most this, then it should be considered zero
		/// </summary>
		public const float EPSILON_SQUARED = EPSILON * EPSILON;

		/// <summary>
		/// The square root of two.
		/// </summary>
		public const float SQRT2 = 1.41421356f;
		#endregion
	}
}
