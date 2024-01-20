using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngine.GameComponents
{
	/// <summary>
	/// Allows for multiple components to be specified to fill a single role and freely switched between.
	/// </summary>
	public class ComponentLibrary : DrawableAffineComponent
	{
		/// <summary>
		/// Creates a new component library with no components.
		/// </summary>
		/// <param name="game">The game this component library will be part of.</param>
		public ComponentLibrary(Game game) : base(game,null,null)
		{
			Components = new Dictionary<string,(DrawableAffineComponent,OnActivated,OnDeactivated)>();
			_acn = null;

			StateChange += (a,b,c) => {}; // Now we don't have to think about null delegates
			return;
		}
		
		/// <summary>
		/// Initializes all of the components in this library.
		/// </summary>
		public override void Initialize()
		{
			foreach(DrawableAffineComponent component in Components.Select(a => a.Value.Item1))
				component.Initialize();

			base.Initialize();
			return;
		}

		/// <summary>
		/// Updates teh active component of this library.
		/// </summary>
		/// <param name="delta">The elapsed time since the last Update call.</param>
		public override void Update(GameTime delta)
		{
			if(ActiveComponentName is not null && ActiveComponent!.Enabled)
				ActiveComponent!.Update(delta);

			return;
		}

		/// <summary>
		/// Draws the active component of this library.
		/// </summary>
		/// <param name="delta">The elapsed time since the last Draw call.</param>
		public override void Draw(GameTime delta)
		{
			if(ActiveComponentName is not null && ActiveComponent!.Visible)
				ActiveComponent!.Draw(delta);

			return;
		}

		/// <summary>
		/// Adds a component to this library and sets its parent to this.
		/// </summary>
		/// <param name="name">The name of the component to add.</param>
		/// <param name="component">The component to add.</param>
		/// <param name="activator">Called when <paramref name="component"/> is made active.</param>
		/// <param name="deactivator">Called when <paramref name="component"/> is made disactive.</param>
		/// <returns>Returns true if the component was added to the library and false otherwise, such as when <paramref name="name"/> is already the name of a component.</returns>
		/// <remarks>
		///	This component library will call each component's Initialize method during its own Initialize or upon adding if already initialized.
		///	However, it takes no responsibility for diposing of objects during its own disposal and leaves them to other resource managers or the finalizer to dispose of.
		/// </remarks>
		public bool Add(string name, DrawableAffineComponent component, OnActivated? activator = null, OnDeactivated? deactivator = null)
		{
			if(Components.ContainsKey(name))
				return false;

			Components.Add(name,(component,activator ?? ((a,b,c) => {}),deactivator ?? ((a,b,c) => {}))); // We don't want to think about null delegates
			component.Parent = this;
			component.Renderer = Renderer;
			component.Tint = Tint;
			component.LayerDepth = LayerDepth;

			if(Initialized)
				component.Initialize();

			return true;
		}

		/// <summary>
		/// Adds a component to this library.
		/// If there is already a component named <paramref name="name"/>, it is replaced.
		/// </summary>
		/// <param name="name">The name of the component to add.</param>
		/// <param name="component">The component to add.</param>
		/// <param name="activator">Called when <paramref name="component"/> is made active.</param>
		/// <param name="deactivator">Called when <paramref name="component"/> is made disactive.</param>
		/// <remarks>
		///	This component library will call each component's Initialize method during its own Initialize or upon adding if already initialized.
		///	However, it takes no responsibility for diposing of objects during its own disposal and leaves them to other resource managers or the finalizer to dispose of.
		/// </remarks>
		public void Put(string name, DrawableAffineComponent component, OnActivated? activator = null, OnDeactivated? deactivator = null)
		{
			Components[name] = (component,activator ?? ((a,b,c) => {}),deactivator ?? ((a,b,c) => {})); // We don't want to think about null delegates
			component.Parent = this;
			component.Renderer = Renderer;
			component.Tint = Tint;
			component.LayerDepth = LayerDepth;

			if(Initialized)
				component.Initialize();
			
			return;
		}

		/// <summary>
		/// Removes a component from this library.
		/// </summary>
		/// <param name="name">The name of the component to add.</param>
		/// <returns>Returns true if a component was removed and false otherwise.</returns>
		public bool Remove(string name)
		{return Components.Remove(name);}

		/// <summary>
		/// The active component or null if no component is active.
		/// </summary>
		public DrawableAffineComponent? ActiveComponent => ActiveComponentName is not null ? this[ActiveComponentName] : null;

		/// <summary>
		/// The name of the active component or null if no component is active.
		/// <para/>
		/// This may be set to null to indicate no component should be active.
		/// Attempting to set this to a component name that does not exist will set this to null.
		/// </summary>
		public string? ActiveComponentName
		{
			get => _acn;
			
			set
			{
				// If we're asked to do nothing, then do nothing
				if(ReferenceEquals(_acn,value) || _acn == value)
					return;

				string prev = _acn!;

				if(value is null || !Components.ContainsKey(value))
				{
					_acn = null;

					if(prev is not null)
						Components[prev].Item3(this,null,prev);

					StateChange(this,null,prev);
					return;
				}

				_acn = value;

				Components[_acn].Item2(this,_acn,prev);

				if(prev is not null)
					Components[prev].Item3(this,_acn,prev);

				StateChange(this,_acn,prev);
				return;
			}
		}

		protected string? _acn;

		/// <summary>
		/// Gets the component named <paramref name="name"/>.
		/// </summary>
		/// <param name="name">The name of the component to obtain.</param>
		/// <exception cref="KeyNotFoundException">Thrown if <paramref name="name"/> is not the name of a component in this library.</exception>
		public DrawableAffineComponent this[string name] => Components[name].Item1;

		/// <summary>
		/// The components in this library.
		/// </summary>
		protected Dictionary<string,(DrawableAffineComponent,OnActivated,OnDeactivated)> Components
		{get; init;}

		/// <summary>
		/// The names of each component in this library in no particular order.
		/// </summary>
		protected IEnumerable<string> ComponentNames => Components.Keys;

		/// <summary>
		/// Obtains the number of components in this ComponentLibrary.
		/// </summary>
		public int Count => Components.Count;

		public override SpriteBatch? Renderer
		{
			get => base.Renderer;

			set
			{
				if(ReferenceEquals(base.Renderer,value))
					return;

				base.Renderer = value;

				// Keep all the children up to date
				foreach(DrawableAffineComponent component in Components.Select(a => a.Value.Item1))
					component.Renderer = value;

				return;
			}
		}

		public override Color Tint
		{
			get => base.Tint;

			set
			{
				if(base.Tint == value)
					return;

				base.Tint = value;

				// We have to do this because we can't avoid the base class forcing the matter
				if(Components is null)
					return;

				foreach(DrawableAffineComponent component in Components.Select(a => a.Value.Item1))
					component.Tint = value;

				return;
			}
		}

		public override float LayerDepth
		{
			get => base.LayerDepth;

			set
			{
				if(base.LayerDepth == value)
					return;

				base.LayerDepth = value; // We might as well keep a copy of it

				// Keep all the children up to date
				foreach(DrawableAffineComponent component in Components.Select(a => a.Value.Item1))
					component.LayerDepth = value;

				return;
			}
		}

		/// <summary>
		/// The width of the active item (sans transformations).
		/// </summary>
		public override int Width => ActiveComponentName is null ? 0 : ActiveComponent!.Width;

		/// <summary>
		/// The height of the active item (sans transformations).
		/// </summary>
		public override int Height => ActiveComponentName is null ? 0 : ActiveComponent!.Height;

		/// <summary>
		/// A bounding box for the active item (including any potential children) [post transformations].
		/// </summary>
		public override Rectangle Bounds
		{get => ActiveComponentName is null ? Rectangle.Empty : ActiveComponent!.Bounds;}

		/// <summary>
		/// Called when any state of this library changes.
		/// </summary>
		public event OnStateChange StateChange;
	}

	/// <summary>
	/// Called when the active component of a ComponentLibrary changes.
	/// </summary>
	/// <param name="sender">The ComponentLibrary that changed its active component.</param>
	/// <param name="active">The new active component.</param>
	/// <param name="prev">The old active component.</param>
	public delegate void OnActivated(ComponentLibrary sender, string? active, string? prev);

	/// <summary>
	/// Called when the active component of a ComponentLibrary changes.
	/// </summary>
	/// <param name="sender">The ComponentLibrary that changed its active component.</param>
	/// <param name="active">The new active component.</param>
	/// <param name="prev">The old active component.</param>
	public delegate void OnDeactivated(ComponentLibrary sender, string? active, string? prev);

	/// <summary>
	/// Called when the active component of a ComponentLibrary changes.
	/// </summary>
	/// <param name="sender">The ComponentLibrary that changed its active component.</param>
	/// <param name="active">The new active component.</param>
	/// <param name="prev">The old active component.</param>
	public delegate void OnStateChange(ComponentLibrary sender, string? active, string? prev);
}
