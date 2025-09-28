using GameEngine.Events;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Text.Json.Serialization;

namespace GameEngine.Framework
{
	/// <summary>
	/// The base requirements for a game object to be both drawable and affine.
	/// </summary>
	[JsonConverter(typeof(JsonGameObjectConverter))]
	public abstract class DrawableAffineObject : AffineObject, IDrawable
	{
		/// <summary>
		/// Initializes this game object to have the identity transformation and no parent.
		/// </summary>
		/// <param name="game">The game this game object will belong to.</param>
		/// <param name="renderer">The renderer to draw with (this can be changed later).</param>
		/// <param name="c">The color to write with. If null, the color will default to white.</param>
		protected DrawableAffineObject(SpriteBatch? renderer = null, Color? c = null)
		{
			_v = true;
			_do = 0;

			Renderer = renderer;
			Tint = c ?? Color.White;
			Effect = SpriteEffects.None;
			LayerDepth = 0.0f;

			return;
		}

		/// <summary>
		/// Makes a (sorta) deep copy of <paramref name="other"/>.
		/// <list type="bullet">
		///	<item>This will have a fresh ID.</item>
		///	<item>This will have the same parent as <paramref name="other"/>, but it will leave Children unpopulated.</item>
		///	<item>This will not match the initialization/disposal state of <paramref name="other"/>. It will be uninitialized.</item>
		///	<item>This will not copy event handlers.</item>
		/// </list>
		/// Note that this will not initialize, dispose, or otherwise modify <paramref name="other"/>.
		/// </summary>
		protected DrawableAffineObject(DrawableAffineObject other) : base(other)
		{
			Visible = other.Visible;
			DrawOrder = other.DrawOrder;

			Renderer = other.Renderer;
			Tint = other.Tint;
			Effect = other.Effect;
			LayerDepth = other.LayerDepth;
			
			return;
		}

		protected override void Dispose(bool disposing)
		{
			if(Disposed)
				return;

			base.Dispose(disposing);
			UnloadContent();

			return;
		}

		public override void Initialize()
		{
			if(Initialized)
				return;
			
			base.Initialize();
			LoadContent();

			return;
		}

		/// <summary>
		/// Loads content immediately after this is initialized.
		/// </summary>
		protected virtual void LoadContent()
		{return;}

		/// <summary>
		/// Unloads content immediately after this is disposed of.
		/// </summary>
		protected virtual void UnloadContent()
		{return;}

		/// <summary>
		/// Called when a new frame is being drawn.
		/// </summary>
		/// <param name="delta">The elapsed game time since the last draw.</param>
		public virtual void Draw(GameTime delta)
		{return;}

		/// <summary>
		/// If true, then this will be drawn during a game's Draw call.
		/// If false, then it will be skipped over.
		/// </summary>
		public virtual bool Visible
		{
			get => _v;

			set
			{
				if(_v == value)
					return;

				_v = value;

				if(VisibleChanged is not null)
					VisibleChanged(this,new BinaryStateChangeEvent(_v));

				return;
			}
		}

		private bool _v;

		/// <summary>
		/// The draw order of this game object relative to other game objects.
		/// Lower values are drawn before higher values.
		/// </summary>
		public virtual int DrawOrder
		{
			get => _do;

			set
			{
				if(_do == value)
					return;

				int old = _do;
				_do = value;

				if(DrawOrderChanged is not null)
					DrawOrderChanged(this,new OrderChangeEvent(old,_do));

				return;
			}
		}

		private int _do;

		/// <summary>
		/// This is the SpriteBatch used to draw this game object.
		/// If this is unassigned (null), then the draw will be skipped.
		/// </summary>
		public virtual SpriteBatch? Renderer
		{get; set;}

		/// <summary>
		/// The tint/color to apply to this game object.
		/// <para/>
		/// This defaults to Color.White for true color.
		/// </summary>
		public virtual Color Tint
		{get; set;}

		/// <summary>
		/// The sprite effect to apply to this game object.
		/// <para/>
		/// This defaults to SpriteEffects.None.
		/// </summary>
		public virtual SpriteEffects Effect
		{get; set;}

		/// <summary>
		/// This is the drawing layer.
		/// This value must be within [0,1].
		/// <para/>
		/// With respect to the drawing order, larger values are the 'front' and smaller values are the 'back'.
		/// This value only has significance when the SpriteBatch's SpriteSortMode is BackToFront (in which case smaller values are drawn on top) or FrontToBack (in which case larger values are drawn on top).
		/// <para/>
		/// This value defaults to 0.0f.
		/// </summary>
		public virtual float LayerDepth
		{get; set;}

		/// <summary>
		/// The width of the game object (sans transformations).
		/// </summary>
		public abstract int Width
		{get;}

		/// <summary>
		/// The height of the game object (sans transformations).
		/// </summary>
		public abstract int Height
		{get;}

		/// <summary>
		/// A bounding box for this game object (including any potential children) [post transformations].
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
		/// Called when this game object's visibility changes.
		/// <para/>
		/// The first parameter will be this GameObject.
		/// The second parameter will be a <see cref="BinaryStateChangeEvent"/>.
		/// </summary>
		public event EventHandler<EventArgs>? VisibleChanged;

		/// <summary>
		/// Called when this game object's draw order changes.
		/// <para/>
		/// The first parameter will be this GameObject.
		/// The second parameter will be an <see cref="OrderChangeEvent"/>.
		/// </summary>
		public event EventHandler<EventArgs>? DrawOrderChanged;
	}
}
