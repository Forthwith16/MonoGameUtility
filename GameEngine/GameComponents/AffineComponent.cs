using GameEngine.Maths;
using Microsoft.Xna.Framework;

namespace GameEngine.GameComponents
{
	/// <summary>
	/// The base requirements for a component to be affine.
	/// </summary>
	/// <remarks>Monogame's game component classes have reinitialization checks baked into them, so we don't bother doing the same here.</remarks>
	public abstract class AffineComponent : GameComponent, IAffineComponent
	{
		/// <summary>
		/// Initializes this component to have the identity transformation and no parent.
		/// </summary>
		/// <param name="game">The game this component will belong to.</param>
		protected AffineComponent(Game game) : base(game)
		{
			Transform = Matrix2D.Identity;
			Parent = null;

			return;
		}

		public virtual void Translate(float tx, float ty)
		{
			Transform = Transform.Translate(tx,ty);
			return;
		}

		public virtual void Translate(Vector2 t)
		{
			Transform = Transform.Translate(t);
			return;
		}

		public virtual void Rotate(float angle, bool righthanded_chirality = false)
		{
			Transform = Transform.Rotate(angle,righthanded_chirality);
			return;
		}

		public virtual void Rotate(float angle, Vector2 point, bool righthanded_chirality = false)
		{
			Transform = Transform.Rotate(angle,point,righthanded_chirality);
			return;
		}

		public virtual void Rotate(float angle, float x, float y, bool righthanded_chirality = false)
		{
			Transform = Transform.Rotate(angle,x,y,righthanded_chirality);
			return;
		}

		public virtual void Rotate90(int times, bool righthanded_chirality = false)
		{
			Transform = Transform.Rotate90(times,righthanded_chirality);
			return;
		}

		public virtual void Rotate90(int times, Vector2 point, bool righthanded_chirality = false)
		{
			Transform = Transform.Rotate90(times,point,righthanded_chirality);
			return;
		}

		public virtual void Rotate90(int times, float x, float y, bool righthanded_chirality = false)
		{
			Transform = Transform.Rotate90(times,x,y,righthanded_chirality);
			return;
		}

		public virtual void Scale(float s)
		{
			Transform = Transform.Scale(s);
			return;
		}

		public virtual void Scale(float s, float x, float y)
		{
			Transform = Transform.Scale(s,x,y);
			return;
		}

		public virtual void Scale(float s, Vector2 pos)
		{
			Transform = Transform.Scale(s,pos);
			return;
		}

		public virtual void Scale(float sx, float sy)
		{
			Transform = Transform.Scale(sx,sy);
			return;
		}

		public virtual void Scale(float sx, float sy, float x, float y)
		{
			Transform = Transform.Scale(sx,sy,x,y);
			return;
		}

		public virtual void Scale(float sx, float sy, Vector2 pos)
		{
			Transform = Transform.Scale(sx,sy,pos);
			return;
		}

		public virtual void Scale(Vector2 s)
		{
			Transform = Transform.Scale(s);
			return;
		}

		public virtual void Scale(Vector2 s, float x, float y)
		{
			Transform = Transform.Scale(s,x,y);
			return;
		}

		public virtual void Scale(Vector2 s, Vector2 pos)
		{
			Transform = Transform.Scale(s,pos);
			return;
		}

		public Matrix2D World
		{
			get
			{
				if(StaleWorld)
				{
					// We won't bother checking what's up with these and blindly assign them
					StaleParent = false;
					StaleTransform = false;

					_w = Parent is null ? Transform : Parent.World * Transform;
				}

				return _w;
			}
		}

		protected Matrix2D _w;
		
		public bool StaleWorld => StaleParent || StaleTransform || Parent is not null && Parent.StaleWorld;

		/// <summary>
		/// If true, then the transform has changed and marks the world matrix as stale.
		/// </summary>
		private bool StaleTransform
		{get; set;}

		public Matrix2D Transform
		{
			get => _t;

			set
			{
				// It's not worth checking if _t == value, so we'll just blindly mark these as stale
				StaleTransform = true;
				StaleInverse = true;

				_t = value;
				return;
			}
		}

		protected Matrix2D _t;

		public Matrix2D InverseWorld
		{
			get
			{
				if(StaleInverseWorld)
				{
					StaleParentInverse = false; // We won't bother checking what's up with this and blindly assign it
					_iw = Parent is null ? InverseTransform : InverseTransform * Parent.InverseWorld;
				}

				return _iw;
			}
		}

		protected Matrix2D _iw;

		public bool StaleInverseWorld => StaleInverse || StaleParentInverse || Parent is not null && Parent.StaleInverseWorld;

		/// <summary>
		/// If true, then the parent has changed and the world is now stale.
		/// </summary>
		private bool StaleParent
		{get; set;}

		/// <summary>
		/// If true, then the parent has changed and the inverse world is now stale.
		/// </summary>
		private bool StaleParentInverse
		{get; set;}

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
		/// If true, then the inverse transform is marked as stale.
		/// </summary>
		private bool StaleInverse
		{get; set;}

		public IAffineComponent? Parent
		{
			get => _p;
			
			set
			{
				StaleParent = true;
				StaleParentInverse = true;

				_p = value;
				return;
			}
		}

		protected IAffineComponent? _p;
	}
}
