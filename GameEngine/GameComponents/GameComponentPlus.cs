using Microsoft.Xna.Framework;

namespace GameEngine.GameComponents
{
	/// <summary>
	/// The GameComponentPlus is a GameComponent that is documented.
	/// Huzzah!
	/// </summary>
	public abstract class GameComponentPlus : IGameComponent, IUpdateable, IDisposable
	{
		/// <summary>
		/// Creates a new component.
		/// </summary>
		protected GameComponentPlus(Game game)
		{
			Initialized = false;
			Disposed = false;
			
			Game = game;
			return;
		}

		/// <summary>
		/// The finalizer.
		/// </summary>
		~GameComponentPlus()
		{
			Dispose(false);
			return;
		}

		/// <summary>
		/// Initializes this component.
		/// </summary>
		public virtual void Initialize()
		{
			Initialized = true;
			return;
		}

		/// <summary>
		/// Updates this component.
		/// </summary>
		/// <param name="delta">The elapsed time since the last call to Update.</param>
		public virtual void Update(GameTime delta)
		{return;}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);

			return;
		}

		/// <summary>
		/// Performs the actual disposal logic.
		/// </summary>
		/// <param name="disposing">If true, we are disposing of this. If false, then this has already been disposed and is in a finalizer call.</param>
		protected virtual void Dispose(bool disposing)
		{
			Disposed = true;
			return;
		}

		/// <summary>
		/// The game this component will belong to.
		/// </summary>
		public Game Game
		{get; private set;}

		/// <summary>
		/// If true, this component has been initialized.
		/// </summary>
		public bool Initialized
		{get; private set;}

		/// <summary>
		/// If true, this component has been disposed.
		/// </summary>
		public bool Disposed
		{get; private set;}

		/// <summary>
		/// If true, then this component will recieve updates.
		/// If false, then this component is not updated.
		/// </summary>
		public bool Enabled
		{
			get => _e;

			set
			{
				if(_e == value)
					return;

				_e = value;
				
				if(EnabledChanged is not null)
					EnabledChanged(this,EventArgs.Empty);

				return;
			}
		}

		private bool _e = true;

		/// <summary>
		/// The update order of this component.
		/// Lower values are updated before larger values.
		/// </summary>
		public int UpdateOrder
		{
			get => _uo;

			set
			{
				if(_uo == value)
					return;
				
				_uo = value;

				if(UpdateOrderChanged is not null)
					UpdateOrderChanged(this,EventArgs.Empty);

				return;
			}
		}

		private int _uo = 0;

		/// <summary>
		/// Called when this component's enabled state changes.
		/// </summary>
		public event EventHandler<EventArgs>? EnabledChanged;

		/// <summary>
		/// Called when this component's udpate order changes.
		/// </summary>
		public event EventHandler<EventArgs>? UpdateOrderChanged;
	}
}
