using GameEngine.Framework;
using GameEngine.Maths;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngine.GameComponents
{
	/// <summary>
	/// The base requirements for a component to be both drawable and affine.
	/// </summary>
	public abstract class DrawableAffineComponent : DrawableGameComponent, IDebugDrawable, IAffineComponent2D
	{
		/// <summary>
		/// Initializes this component to have the identity transformation and no parent.
		/// </summary>
		/// <param name="game">The game this component will belong to.</param>
		/// <param name="renderer">The renderer to draw with (this can be changed later).</param>
		/// <param name="c">The color to write with. If null, the color will default to white.</param>
		protected DrawableAffineComponent(Game game, SpriteBatch? renderer, Color? c = null) : base(game)
		{
			WorldRevision = 2;
			InverseWorldRevision = 2;

			Transform = Matrix2D.Identity;
			Parent = null;
			
			Renderer = renderer;
			Tint = c ?? Color.White;
			Effect = SpriteEffects.None;

			OnDrawDebugOrderChanged += (a,b,c) => {};
			_ddo = 0;

			Initialized = false;
			return;
		}

		public override void Initialize()
		{
			if(Initialized)
				return;

			Initialized = true;
			base.Initialize();

			return;
		}

		public virtual void DrawDebugInfo(GameTime delta)
		{return;}

		public virtual Matrix2D Transform
		{
			get => _t;

			set
			{
				// It's not worth checking if _t == value, so we'll just blindly mark these as stale
				StaleInverse = true;

				// Instead of keeping a stale boolean, we double our value in using the revision number to mark these as stale
				ParentWorldRevision = 0u;
				ParentInverseWorldRevision = 0u;

				_t = value;
				return;
			}
		}

		protected Matrix2D _t;

		public Matrix2D World
		{
			get
			{
				if(StaleWorld)
				{
					// Our behavior differs depending on if we have a parent or not
					if(Parent is null)
					{
						ParentWorldRevision = 1u; // A 1 value means our world is up to date when we have no parent
						_w = Transform;
					}
					else
					{
						ParentWorldRevision = Parent.WorldRevision; // Our parent's revision number means our world is up to date when we have a parent
						_w = Parent.World * Transform;
					}

					++WorldRevision;
				}

				return _w;
			}
		}

		protected Matrix2D _w;

		public uint WorldRevision
		{get; protected set;}
		
		/// <summary>
		/// The currently known revision of Parent.World.
		/// A different (higher) value means that our World matrix is stale.
		/// </summary>
		protected uint ParentWorldRevision
		{get; set;}

		/// <summary>
		/// If true, then the world matrix is stale and needs to be updated.
		/// </summary>
		public bool StaleWorld => Parent is null ? ParentWorldRevision != 1u : ParentWorldRevision != Parent.WorldRevision || Parent.StaleWorld;

		public Matrix2D InverseTransform
		{
			get
			{
				if(StaleInverse)
					InverseTransform = Transform.Invert();

				return _it;
			}
			
			private set
			{
				_it = value;
				StaleInverse = false;

				return;
			}
		}

		protected Matrix2D _it;

		/// <summary>
		/// If true, then our inverse is stale an needs to be recalculated.
		/// If false, then our inverse is up to date.
		/// </summary>
		private bool StaleInverse
		{get; set;}

		public Matrix2D InverseWorld
		{
			get
			{
				if(StaleInverseWorld)
				{
					// Our behavior differs depending on if we have a parent or not
					if(Parent is null)
					{
						ParentInverseWorldRevision = 1u; // A 1 value means our inverse world is up to date when we have no parent
						_iw = InverseTransform;
					}
					else
					{
						ParentInverseWorldRevision = Parent.InverseWorldRevision; // Our parent's revision number means our inverse world is up to date when we have a parent
						_iw = InverseTransform * Parent.InverseWorld;
					}

					++InverseWorldRevision;
				}

				return _iw;
			}
		}

		protected Matrix2D _iw;

		public uint InverseWorldRevision
		{get; protected set;}

		/// <summary>
		/// If true, then the inverse world matrix is stale and needs to be updated.
		/// </summary>
		public bool StaleInverseWorld => StaleInverse || (Parent is null ? ParentInverseWorldRevision != 1u : ParentInverseWorldRevision != Parent.InverseWorldRevision || Parent.StaleInverseWorld);

		/// <summary>
		/// The currently known revision of Parent.InverseWorld.
		/// A different (higher) value means that our World matrix is stale.
		/// </summary>
		protected uint ParentInverseWorldRevision
		{get; set;}

		public IAffineComponent2D? Parent
		{
			get => _p;
			
			set
			{
				ParentWorldRevision = 0u;
				ParentInverseWorldRevision = 0u;

				_p = value;
				return;
			}
		}

		protected IAffineComponent2D? _p;

		/// <summary>
		/// This is the SpriteBatch used to draw this component.
		/// If this is unassigned (null), then the draw will be skipped.
		/// </summary>
		public virtual SpriteBatch? Renderer
		{get; set;}

		/// <summary>
		/// The tint/color to apply to this component.
		/// <para/>
		/// This defaults to Color.White for true color.
		/// </summary>
		public virtual Color Tint
		{get; set;}

		/// <summary>
		/// The sprite effect to apply to this component.
		/// <para/>
		/// This defaults to SpriteEffects.None.
		/// </summary>
		public virtual SpriteEffects Effect
		{get; set;}

		/// <summary>
		/// This is the drawing layer.
		/// This value must be within [0,1].
		/// With respect to the drawing order, larger values are the 'front' and smaller values are the 'back'.
		/// This value only has significance when the SpriteBatch's SpriteSortMode is BackToFront (in which case smaller values are drawn on top) or FrontToBack (in which case larger values are drawn on top).
		/// <para/>
		/// This value defaults to 0.0f.
		/// </summary>
		public virtual float LayerDepth
		{get; set;}

		/// <summary>
		/// The width of the component (sans transformations).
		/// </summary>
		public abstract int Width
		{get;}

		/// <summary>
		/// The height of the component (sans transformations).
		/// </summary>
		public abstract int Height
		{get;}

		/// <summary>
		/// A bounding box for this component (including any potential children) [post transformations].
		/// </summary>
		public virtual Rectangle Bounds
		{
			get
			{
				// We need to know where the four corners end up as the most extreme points
				// Then we can pick the min and max values to get a bounding rectangle
				int W = Width; // Who knows how long these take to calculate
				int H = Height;

				Vector2 TL = World * new Vector2(0,0); // Screen coorinates (and thus world coordinates) put the origin at the top left
				Vector2 TR = World * new Vector2(W,0);
				Vector2 BL = World * new Vector2(0,H);
				Vector2 BR = World * new Vector2(W,H);

				float left = MathF.Min(MathF.Min(TL.X,TR.X),MathF.Min(BL.X,BR.X));
				float right = MathF.Max(MathF.Max(TL.X,TR.X),MathF.Max(BL.X,BR.X));
				float top = MathF.Min(MathF.Min(TL.Y,TR.Y),MathF.Min(BL.Y,BR.Y));
				float bottom = MathF.Max(MathF.Max(TL.Y,TR.Y),MathF.Max(BL.Y,BR.Y));

				return new Rectangle((int)left,(int)top,(int)MathF.Ceiling(right - left),(int)MathF.Ceiling(bottom - top));
			}
		}

		/// <summary>
		/// If true, then this has been initialized.
		/// If false, it has not.
		/// </summary>
		public bool Initialized
		{get; protected set;}

		public int DrawDebugOrder
		{
			get => _ddo;
			
			set
			{
				if(_ddo == value)
					return;

				int old = _ddo;
				_ddo = value;
				OnDrawDebugOrderChanged(this,_ddo,old);

				return;
			}
		}

		protected int _ddo;

		public event DrawDebugOrderChanged OnDrawDebugOrderChanged;
	}
}
