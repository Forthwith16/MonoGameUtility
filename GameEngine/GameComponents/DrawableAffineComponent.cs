using GameEngine.Maths;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngine.GameComponents
{
	/// <summary>
	/// The base requirements for a component to be both drawable and affine.
	public abstract class DrawableAffineComponent : DrawableGameComponent, IAffineComponent
	{
		/// <summary>
		/// Initializes this component to have the identity transformation and no parent.
		/// </summary>
		/// <param name="game">The game this component will belong to.</param>
		/// <param name="renderer">The renderer to draw with (this can be changed later).</param>
		/// <param name="c">The color to write with. If null, the color will default to white.</param>
		protected DrawableAffineComponent(Game game, SpriteBatch? renderer, Color? c = null) : base(game)
		{
			Transform = Matrix2D.Identity;
			Parent = null;
			
			Renderer = renderer;
			Tint = c ?? Color.White;
			Effect = SpriteEffects.None;

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

		public void Translate(float tx, float ty)
		{
			Transform = Transform.Translate(tx,ty);
			return;
		}

		public void Translate(Vector2 t)
		{
			Transform = Transform.Translate(t);
			return;
		}

		public void Rotate(float angle, bool righthanded_chirality = false)
		{
			Transform = Transform.Rotate(angle,righthanded_chirality);
			return;
		}

		public void Rotate(float angle, Vector2 point, bool righthanded_chirality = false)
		{
			Transform = Transform.Rotate(angle,point,righthanded_chirality);
			return;
		}

		public void Rotate(float angle, float x, float y, bool righthanded_chirality = false)
		{
			Transform = Transform.Rotate(angle,x,y,righthanded_chirality);
			return;
		}

		public void Rotate90(int times, bool righthanded_chirality = false)
		{
			Transform = Transform.Rotate90(times,righthanded_chirality);
			return;
		}

		public void Rotate90(int times, Vector2 point, bool righthanded_chirality = false)
		{
			Transform = Transform.Rotate90(times,point,righthanded_chirality);
			return;
		}

		public void Rotate90(int times, float x, float y, bool righthanded_chirality = false)
		{
			Transform = Transform.Rotate90(times,x,y,righthanded_chirality);
			return;
		}

		public void Scale(float s)
		{
			Transform = Transform.Scale(s);
			return;
		}

		public void Scale(float s, float x, float y)
		{
			Transform = Transform.Scale(s,x,y);
			return;
		}

		public void Scale(float s, Vector2 pos)
		{
			Transform = Transform.Scale(s,pos);
			return;
		}

		public void Scale(float sx, float sy)
		{
			Transform = Transform.Scale(sx,sy);
			return;
		}

		public void Scale(float sx, float sy, float x, float y)
		{
			Transform = Transform.Scale(sx,sy,x,y);
			return;
		}

		public void Scale(float sx, float sy, Vector2 pos)
		{
			Transform = Transform.Scale(sx,sy,pos);
			return;
		}

		public void Scale(Vector2 s)
		{
			Transform = Transform.Scale(s);
			return;
		}

		public void Scale(Vector2 s, float x, float y)
		{
			Transform = Transform.Scale(s,x,y);
			return;
		}

		public void Scale(Vector2 s, Vector2 pos)
		{
			Transform = Transform.Scale(s,pos);
			return;
		}

		public Matrix2D World => Parent is null ? Transform : Parent.World * Transform;

		public Matrix2D Transform
		{get; set;}

		public IAffineComponent? Parent
		{get; set;}

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
	}
}
