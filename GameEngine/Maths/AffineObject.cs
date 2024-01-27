using GameEngine.GameComponents;
using Microsoft.Xna.Framework;

namespace GameEngine.Maths
{
	/// <summary>
	/// Represents an affine object.
	/// </summary>
	public class AffineObject : IAffineComponent
	{
		/// <summary>
		/// Creates a blank affine object.
		/// </summary>
		public AffineObject()
		{
			Transform = Matrix2D.Identity;
			Parent = null;

			return;
		}

		/// <summary>
		/// Creates an affine child.
		/// </summary>
		/// <param name="parent">The parent of this object.</param>
		public AffineObject(IAffineComponent parent)
		{
			Transform = Matrix2D.Identity;
			Parent = parent;

			return;
		}

		/// <summary>
		/// Creates an affine object with a transform.
		/// </summary>
		/// <param name="transform">The initial transform.</param>
		public AffineObject(Matrix2D transform)
		{
			Transform = transform;
			Parent = null;

			return;
		}

		/// <summary>
		/// Creates an affine object.
		/// </summary>
		/// <param name="transform">The initial transform.</param>
		/// <param name="parent">The parent of this object.</param>
		public AffineObject(IAffineComponent parent, Matrix2D transform)
		{
			Transform = transform;
			Parent = parent;

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

		public override string ToString()
		{return "Parent: " + Parent + "\n" + Transform.ToString();}

		public Matrix2D World => Parent is null ? Transform : Parent.World * Transform;

		public Matrix2D Transform
		{get; set;}

		public IAffineComponent? Parent
		{get; set;}
	}
}
