using GameEngine.GameComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngine.Framework
{
	/// <summary>
	/// Represents a pure data object that can become a drawable object with additional information.
	/// </summary>
	public interface IBlueprint
	{
		/// <summary>
		/// Produces a drawable version of this pure data object.
		/// Changes made to this may be reflected in the drawn object.
		/// This version of ToDrawable only works if the singleton instance has already been created via ToDrawable(Game,SpriteBatch?,bool).
		/// </summary>
		/// <returns>Returns a drawable game component that draws this pure data object.</returns>
		/// <exception cref="InvalidOperationException">Thrown if the singleton instance has not been created yet (see ToDrawable(Game,SpriteBatch?,bool)).</exception>
		public DrawableAffineComponent ToDrawable();

		/// <summary>
		/// Produces a drawable version of this pure data object.
		/// Changes made to this may be reflected in the drawn object.
		/// </summary>
		/// <param name="game">The game the drawable object will belong to.</param>
		/// <param name="renderer">The renderer used to draw the drawable object (it can usually be changed later).</param>
		/// <param name="singleton">
		///	If true, then this will return a singleton instance of the drawable version of this object every time.
		///	If false, this returns a new instance of the drawable version of this object.
		/// </param>
		/// <returns>Returns a drawable game component that draws this pure data object.</returns>
		public DrawableAffineComponent ToDrawable(Game game, SpriteBatch? renderer, bool singleton = false);
	}
}
