using GameEngine.Maths;
using Microsoft.Xna.Framework;

namespace GameEngine.GameComponents
{
	/// <summary>
	/// Defines the bare bones of what it means to be an affine component.
	/// Affine components may be translated, rotated, or scaled.
	/// </summary>
	public interface IAffineComponent
	{
		/// <summary>
		/// Obtains the world position of this affine component (this includes the parent transform).
		/// </summary>
		public Vector2 Position
		{get => World * Vector2.Zero;}

		/// <summary>
		/// The transformation matrix of this component.
		/// If this component has a parent, this includes the parent transform.
		/// The parent transform is applied second when it exists with this component's transform being applied first (i.e. Transform is left multiplied by Parent.Transform).
		/// <para/>
		/// For the raw transformation matrix, use Transform instead.
		/// </summary>
		public Matrix2D World
		{get;}

		/// <summary>
		/// The transformation matrix of this component absent its parent transform (if it has one).
		/// <para/>
		/// For the transformation matrix containing the parent (if present), use World instead.
		/// </summary>
		public Matrix2D Transform
		{get; set;}

		/// <summary>
		/// The parent of this component.
		/// Transforming the parent will transform this object in turn.
		/// </summary>
		public IAffineComponent? Parent
		{get; set;}
	}
}
