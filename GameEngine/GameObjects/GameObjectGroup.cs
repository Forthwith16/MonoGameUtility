using GameEngine.Assets.Sprites;
using GameEngine.DataStructures.Absorbing;
using GameEngine.Framework;
using GameEngine.Utility.ExtensionMethods.InterfaceFunctions;
using GameEngine.Utility.ExtensionMethods.PrimitiveExtensions;
using Microsoft.Xna.Framework;

namespace GameEngine.GameObjects
{
	/// <summary>
	/// A collection of game objects grouped together for administrative purposes.
	/// They are initialized together, drawn together, and disposed of together.
	/// Their other properties are managed independently.
	/// </summary>
	public class GameObjectGroup : DrawableAffineObject
	{
		/// <summary>
		/// Creates a group of game objects.
		/// </summary>
		/// <param name="objs">The game object to add to the group.</param>
		public GameObjectGroup(params DrawableAffineObject?[] objs) : this((IEnumerable<DrawableAffineObject?>)objs)
		{return;}

		/// <summary>
		/// Creates a group of game objects.
		/// </summary>
		/// <param name="objs">The game object to add to the group.</param>
		public GameObjectGroup(IEnumerable<DrawableAffineObject?> objs) : base()
		{
			Components = new AbsorbingList<DrawableAffineObject>(objs.NotNull());
			return;
		}

		public override void Initialize()
		{
			if(Initialized)
				return;

			foreach(DrawableAffineObject component in Components)
				component.Initialize();

			base.Initialize();
			return;
		}

		public override void Update(GameTime delta)
		{
			foreach(DrawableAffineObject component in Components)
				component.Update(delta);

			return;
		}

		public override void Draw(GameTime delta)
		{
			foreach(DrawableAffineObject component in Components)
				component.Draw(delta);

			return;
		}

		protected override void Dispose(bool disposing)
		{
			if(Disposed)
				return;

			foreach(DrawableAffineObject component in Components)
				component.Dispose();
			
			base.Dispose(disposing);
			return;
		}

		/// <summary>
		/// Gets the game object at position <paramref name="index"/>.
		/// </summary>
		/// <param name="index">The index of the game object to obtain.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index"/> is negative or at least Count.</exception>
		public DrawableAffineObject this[int index] => Components[index];

		/// <summary>
		/// The game object in this group.
		/// </summary>
		protected AbsorbingList<DrawableAffineObject> Components
		{get; init;}

		/// <summary>
		/// Obtains the number of game object in this ComponentGroup.
		/// </summary>
		public int Count => Components.Count;

		public override RenderTargetFriendlyGame? Game
		{
			get => base.Game;

			protected internal set
			{
				foreach(DrawableAffineObject component in Components)
					component.Game = value;

				base.Game = value;
				return;
			}
		}

		public override SpriteRenderer? Renderer
		{
			get => base.Renderer;

			set
			{
				if(ReferenceEquals(base.Renderer,value))
					return;

				base.Renderer = value;

				foreach(DrawableAffineObject component in Components)
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
