using GameEngine.Events;
using Microsoft.Xna.Framework;
using System.Text.Json.Serialization;

namespace GameEngine.Framework
{
	/// <summary>
	/// This is the basic game object type for use with all GameEngine tools and derived projects.
	/// </summary>
	[JsonConverter(typeof(JsonGameObjectConverter))]
	public abstract class GameObject : IGameComponent, IUpdateable, IDisposable, IEquatable<GameObject>
	{
		/// <summary>
		/// Creates a new game object.
		/// </summary>
		protected GameObject()
		{
			Initialized = false;
			Disposed = false;
			//Game = null; // We don't set this (it defaults to this anyway) b/c it can be overridden, and that can cause problems
			
			_e = true;
			_uo = 0;

			ID = GameObjectID.GetFreshID(this);
			return;
		}

		/// <summary>
		/// Makes a (sorta) deep copy of <paramref name="other"/>.
		/// <list type="bullet">
		///	<item>This will copy Game.</item>
		///	<item>This will have a fresh ID.</item>
		///	<item>This will not match the initialization/disposal state of <paramref name="other"/>. It will be uninitialized.</item>
		///	<item>This will not copy event handlers.</item>
		/// </list>
		/// Note that this will not initialize, dispose, or otherwise modify <paramref name="other"/>.
		/// </summary>
		protected GameObject(GameObject other) : this()
		{
			Game = other.Game;

			_e = other._e;
			_uo = other._uo;

			return;
		}

		/// <summary>
		/// The finalizer.
		/// </summary>
		~GameObject()
		{
			Dispose(false);
			return;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);

			return;
		}

		/// <summary>
		/// Performs the actual disposal logic.
		/// </summary>
		/// <param name="disposing">
		///	If true, we are disposing of this via <see cref="Dispose()"/>.
		///	If false, then this dispose call is from the finalizer.
		/// </param>
		protected virtual void Dispose(bool disposing)
		{
			if(Disposed)
				return;

			ID.Dispose();
			
			Disposed = true;
			return;
		}

		public static bool operator ==(GameObject? a, GameObject? b) => a is null ? b is null : b is not null && a.ID == b.ID;
		public static bool operator !=(GameObject? a, GameObject? b) => a is null ? b is not null : b is null || a.ID != b.ID;
		public bool Equals(GameObject? obj) => obj is not null && ID == obj.ID;
		public override bool Equals(object? obj) => obj is GameObject s && ID == s.ID;
		public override int GetHashCode() => ID.GetHashCode();

		/// <summary>
		/// Initializes this game object.
		/// </summary>
		public virtual void Initialize()
		{
			Initialized = true;
			return;
		}

		/// <summary>
		/// Updates this game object.
		/// </summary>
		/// <param name="delta">The elapsed time since the last call to Update.</param>
		public virtual void Update(GameTime delta)
		{return;}

		public override string ToString() => ID.ToString();

		/// <summary>
		/// The game this game object will belong to.
		/// This value will be null until it is about to be added to a game.
		/// </summary>
		/// <remarks>If a derived class has any child objects that are not added to the game directly, it should override this and ensure its children have their Game values set.</remarks>
		public virtual RenderTargetFriendlyGame? Game
		{
			get => _g;
			protected internal set => _g = value;
		}

		private RenderTargetFriendlyGame? _g;

		/// <summary>
		/// The ID of this game object.
		/// </summary>
		public GameObjectID ID
		{get;}

		/// <summary>
		/// If true, this game object has been initialized.
		/// </summary>
		public bool Initialized
		{get; private set;}

		/// <summary>
		/// If true, this game object has been disposed.
		/// </summary>
		public bool Disposed
		{get; private set;}

		/// <summary>
		/// If true, then this game object will recieve updates.
		/// If false, then this game object is not updated.
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
					EnabledChanged(this,new BinaryStateChangeEvent(_e));

				return;
			}
		}

		private bool _e;

		/// <summary>
		/// The update order of this game object.
		/// Lower values are updated before larger values.
		/// </summary>
		public int UpdateOrder
		{
			get => _uo;

			set
			{
				if(_uo == value)
					return;
				
				int old = _uo;
				_uo = value;

				if(UpdateOrderChanged is not null)
					UpdateOrderChanged(this,new OrderChangeEvent(old,_uo));

				return;
			}
		}

		private int _uo;

		/// <summary>
		/// Called when this game object's enabled state changes.
		/// <para/>
		/// The first parameter will be this GameObject.
		/// The second parameter will be a <see cref="BinaryStateChangeEvent"/>.
		/// </summary>
		public event EventHandler<EventArgs>? EnabledChanged;

		/// <summary>
		/// Called when this game object's udpate order changes.
		/// <para/>
		/// The first parameter will be this GameObject.
		/// The second parameter will be an <see cref="OrderChangeEvent"/>.
		/// </summary>
		public event EventHandler<EventArgs>? UpdateOrderChanged;
	}
}
