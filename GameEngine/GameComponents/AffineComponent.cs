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

		public Matrix2D World => Parent is null ? Transform : Parent.World * Transform;

		public Matrix2D Transform
		{get; set;}

		public IAffineComponent? Parent
		{get; set;}
	}
}
