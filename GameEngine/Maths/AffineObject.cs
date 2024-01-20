using GameEngine.GameComponents;

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

		public override string ToString()
		{return "Parent: " + Parent + "\n" + Transform.ToString();}

		public Matrix2D World => Parent is null ? Transform : Parent.World * Transform;

		public Matrix2D Transform
		{get; set;}

		public IAffineComponent? Parent
		{get; set;}
	}
}
