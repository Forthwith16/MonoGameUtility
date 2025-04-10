using GameEngine.DataStructures.Absorbing;
using GameEngine.Utility.ExtensionMethods.InterfaceFunctions;
using GameEngine.Utility.ExtensionMethods.PrimitiveExtensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngine.GameComponents
{
	/// <summary>
	/// A collection of game components grouped together for administrative purposes.
	/// They are initialized together, drawn together, and disposed of together.
	/// Their other properties are managed independently.
	/// </summary>
	public class ComponentGroup : DrawableAffineComponent
	{
		/// <summary>
		/// Creates a group of game components.
		/// </summary>
		/// <param name="components">The components to add to the group.</param>
		public ComponentGroup(Game game, params DrawableAffineComponent?[] components) : this(game,(IEnumerable<DrawableAffineComponent?>)components)
		{return;}

		/// <summary>
		/// Creates a group of game components.
		/// </summary>
		/// <param name="components">The components to add to the group.</param>
		public ComponentGroup(Game game, IEnumerable<DrawableAffineComponent?> components) : base(game,null,null)
		{
			Components = new AbsorbingList<DrawableAffineComponent>(components.NotNull());
			return;
		}

		public override void Initialize()
		{
			if(Initialized)
				return;

			foreach(DrawableAffineComponent component in Components)
				component.Initialize();

			base.Initialize();
			return;
		}

		public override void Update(GameTime delta)
		{
			foreach(DrawableAffineComponent component in Components)
				component.Update(delta);

			return;
		}

		public override void Draw(GameTime delta)
		{
			foreach(DrawableAffineComponent component in Components)
				component.Draw(delta);

			return;
		}

		protected override void Dispose(bool disposing)
		{
			foreach(DrawableAffineComponent component in Components)
				component.Dispose();
			
			base.Dispose(disposing);
			return;
		}

		/// <summary>
		/// Gets the component at position <paramref name="index"/>.
		/// </summary>
		/// <param name="index">The index of the component to obtain.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index"/> is negative or at least Count.</exception>
		public DrawableAffineComponent this[int index] => Components[index];

		/// <summary>
		/// The components in this group.
		/// </summary>
		protected AbsorbingList<DrawableAffineComponent> Components
		{get; init;}

		/// <summary>
		/// Obtains the number of components in this ComponentGroup.
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

				foreach(DrawableAffineComponent component in Components)
					component.Renderer = base.Renderer;

				return;
			}
		}

		public override int Width => Bounds.Width; // These are slow, but they exist

		public override int Height => Bounds.Height; // These are slow, but they exist

		public override Rectangle Bounds
		{
			get
			{
				if(Count == 0)
					return Rectangle.Empty;

				Rectangle ret = this[0].Bounds;

				for(int i = 1;i < Count;i++)
					ret.Union(this[i].Bounds);
				
				return ret;
			}
		}
	}
}
