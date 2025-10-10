using GameEngine.Assets.Sprites;
using GameEngine.Framework;
using Microsoft.Xna.Framework;

namespace GameEngine.GameObjects
{
	/// <summary>
	/// Allows for multiple game objects to be specified to fill a single role and freely switched between.
	/// </summary>
	/// <remarks>
	/// This class updates its children on only some basic properties.
	/// Of particular note is DrawOrder, which is not sent out to its children.
	/// If IGUI elements are put in here, this will not update their LayerDepth correctly and must be done manually.
	/// In general, this is meant for use only with ordinary DrawableAffineObjects.
	/// </remarks>
	public class GameObjectLibrary : DrawableAffineObject
	{
		/// <summary>
		/// Creates a new game object library with no game objects.
		/// </summary>
		public GameObjectLibrary() : base()
		{
			GameObjects = new Dictionary<string,(DrawableAffineObject,OnActivated,OnDeactivated)>();
			_agon = null;

			StateChange += (a,b,c) => {}; // Now we don't have to think about null delegates
			return;
		}
		
		/// <summary>
		/// Initializes all of the game objects in this library.
		/// </summary>
		public override void Initialize()
		{
			if(Initialized)
				return;

			foreach(DrawableAffineObject obj in GameObjects.Select(a => a.Value.Item1))
				obj.Initialize();

			base.Initialize();
			return;
		}

		/// <summary>
		/// Updates the active game object of this library.
		/// </summary>
		/// <param name="delta">The elapsed time since the last Update call.</param>
		public override void Update(GameTime delta)
		{
			if(ActiveGameObjectName is not null && ActiveGameObject!.Enabled)
				ActiveGameObject!.Update(delta);

			return;
		}

		/// <summary>
		/// Draws the active game object of this library.
		/// </summary>
		/// <param name="delta">The elapsed time since the last Draw call.</param>
		public override void Draw(GameTime delta)
		{
			if(ActiveGameObjectName is not null && ActiveGameObject!.Visible)
				ActiveGameObject!.Draw(delta);

			return;
		}

		/// <summary>
		/// Adds a game object to this library and sets its parent to this.
		/// </summary>
		/// <param name="name">The name of the game object to add.</param>
		/// <param name="obj">The game object to add.</param>
		/// <param name="activator">Called when <paramref name="obj"/> is made active.</param>
		/// <param name="deactivator">Called when <paramref name="obj"/> is made disactive.</param>
		/// <returns>Returns true if the game object was added to the library and false otherwise, such as when <paramref name="name"/> is already the name of a game object.</returns>
		/// <remarks>
		///	This game object library will call each game object's Initialize method during its own Initialize or upon adding if already initialized.
		///	However, it takes no responsibility for diposing of objects during its own disposal and leaves them to other resource managers or the finalizer to dispose of.
		/// </remarks>
		public bool Add(string name, DrawableAffineObject obj, OnActivated? activator = null, OnDeactivated? deactivator = null)
		{
			if(GameObjects.ContainsKey(name))
				return false;

			GameObjects.Add(name,(obj,activator ?? ((a,b,c) => {}),deactivator ?? ((a,b,c) => {}))); // We don't want to think about null delegates
			obj.Parent = this;
			obj.Renderer = Renderer;
			obj.Tint = Tint;
			obj.LayerDepth = LayerDepth;

			if(Initialized)
				obj.Initialize();

			return true;
		}

		/// <summary>
		/// Adds a game object to this library.
		/// If there is already a game object named <paramref name="name"/>, it is replaced.
		/// </summary>
		/// <param name="name">The name of the game object to add.</param>
		/// <param name="obj">The game object to add.</param>
		/// <param name="activator">Called when <paramref name="obj"/> is made active.</param>
		/// <param name="deactivator">Called when <paramref name="obj"/> is made disactive.</param>
		/// <remarks>
		///	This game object library will call each game object's Initialize method during its own Initialize or upon adding if already initialized.
		///	However, it takes no responsibility for diposing of objects during its own disposal and leaves them to other resource managers or the finalizer to dispose of.
		/// </remarks>
		public void Put(string name, DrawableAffineObject obj, OnActivated? activator = null, OnDeactivated? deactivator = null)
		{
			GameObjects[name] = (obj,activator ?? ((a,b,c) => {}),deactivator ?? ((a,b,c) => {})); // We don't want to think about null delegates
			obj.Parent = this;
			obj.Renderer = Renderer;
			obj.Tint = Tint;
			obj.LayerDepth = LayerDepth;

			if(Initialized)
				obj.Initialize();
			
			return;
		}

		/// <summary>
		/// Removes a game object from this library.
		/// </summary>
		/// <param name="name">The name of the game object to add.</param>
		/// <returns>Returns true if a game object was removed and false otherwise.</returns>
		public bool Remove(string name)
		{return GameObjects.Remove(name);}

		/// <summary>
		/// The active game object or null if no game object is active.
		/// </summary>
		public DrawableAffineObject? ActiveGameObject => ActiveGameObjectName is not null ? this[ActiveGameObjectName] : null;

		/// <summary>
		/// The name of the active game object or null if no game object is active.
		/// <para/>
		/// This may be set to null to indicate no game object should be active.
		/// Attempting to set this to a game object name that does not exist will set this to null.
		/// </summary>
		public string? ActiveGameObjectName
		{
			get => _agon;
			
			set
			{
				// If we're asked to do nothing, then do nothing
				if(ReferenceEquals(_agon,value) || _agon == value)
					return;

				string prev = _agon!;

				if(value is null || !GameObjects.ContainsKey(value))
				{
					_agon = null;

					if(prev is not null)
						GameObjects[prev].Item3(this,null,prev);

					StateChange(this,null,prev);
					return;
				}

				_agon = value;

				GameObjects[_agon].Item2(this,_agon,prev);

				if(prev is not null)
					GameObjects[prev].Item3(this,_agon,prev);

				StateChange(this,_agon,prev);
				return;
			}
		}

		protected string? _agon;

		/// <summary>
		/// Gets the game object named <paramref name="name"/>.
		/// </summary>
		/// <param name="name">The name of the game object to obtain.</param>
		/// <exception cref="KeyNotFoundException">Thrown if <paramref name="name"/> is not the name of a game object in this library.</exception>
		public DrawableAffineObject this[string name] => GameObjects[name].Item1;

		/// <summary>
		/// The game objects in this library.
		/// </summary>
		protected Dictionary<string,(DrawableAffineObject,OnActivated,OnDeactivated)> GameObjects
		{get; init;}

		/// <summary>
		/// The names of each game object in this library in no particular order.
		/// </summary>
		protected IEnumerable<string> GameObjectNames => GameObjects.Keys;

		/// <summary>
		/// Obtains the number of game objects in this GameObjectLibrary.
		/// </summary>
		public int Count => GameObjects.Count;

		public override RenderTargetFriendlyGame? Game
		{
			protected internal set
			{
				foreach(DrawableAffineObject objs in GameObjects.Select(a => a.Value.Item1))
					objs.Game = value;

				base.Game = value;
				return;
			}
		}

		public override SpriteRenderer? Renderer
		{
			set
			{
				if(ReferenceEquals(base.Renderer,value))
					return;

				base.Renderer = value;

				// Keep all the children up to date
				foreach(DrawableAffineObject obj in GameObjects.Select(a => a.Value.Item1))
					obj.Renderer = value;

				return;
			}
		}

		public override Color Tint
		{
			set
			{
				if(base.Tint == value)
					return;

				base.Tint = value;

				// We have to do this because we can't avoid the base class forcing the matter
				if(GameObjects is null)
					return;

				foreach(DrawableAffineObject obj in GameObjects.Select(a => a.Value.Item1))
					obj.Tint = value;

				return;
			}
		}

		public override float LayerDepth
		{
			set
			{
				if(base.LayerDepth == value)
					return;

				base.LayerDepth = value; // We might as well keep a copy of it

				// Keep all the children up to date
				foreach(DrawableAffineObject obj in GameObjects.Select(a => a.Value.Item1))
					obj.LayerDepth = value;

				return;
			}
		}

		/// <summary>
		/// The width of the active item (sans transformations).
		/// </summary>
		public override int Width => ActiveGameObjectName is null ? 0 : ActiveGameObject!.Width;

		/// <summary>
		/// The height of the active item (sans transformations).
		/// </summary>
		public override int Height => ActiveGameObjectName is null ? 0 : ActiveGameObject!.Height;

		/// <summary>
		/// A bounding box for the active item (including any potential children) [post transformations].
		/// </summary>
		public override Rectangle Bounds
		{get => ActiveGameObjectName is null ? Rectangle.Empty : ActiveGameObject!.Bounds;}

		/// <summary>
		/// Called when any state of this library changes.
		/// </summary>
		public event OnStateChange StateChange;
	}

	/// <summary>
	/// Called when the active game object of a GameObjectLibrary changes.
	/// </summary>
	/// <param name="sender">The GameObjectLibrary that changed its active game object.</param>
	/// <param name="active">The new active game object.</param>
	/// <param name="prev">The old active game object.</param>
	public delegate void OnActivated(GameObjectLibrary sender, string? active, string? prev);

	/// <summary>
	/// Called when the active game object of a GameObjectLibrary changes.
	/// </summary>
	/// <param name="sender">The GameObjectLibrary that changed its active game object.</param>
	/// <param name="active">The new active game object.</param>
	/// <param name="prev">The old active game object.</param>
	public delegate void OnDeactivated(GameObjectLibrary sender, string? active, string? prev);

	/// <summary>
	/// Called when the active game object of a GameObjectLibrary changes.
	/// </summary>
	/// <param name="sender">The GameObjectLibrary that changed its active game object.</param>
	/// <param name="active">The new active game object.</param>
	/// <param name="prev">The old active game object.</param>
	public delegate void OnStateChange(GameObjectLibrary sender, string? active, string? prev);
}
