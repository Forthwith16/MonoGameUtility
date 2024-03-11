using GameEngine.Texture;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngine.GameComponents
{
	/// <summary>
	/// A component that generates a texture and draws it as a rectangle.
	/// </summary>
	public class RectangleComponent : ImageComponent
	{
		/// <summary>
		/// Creates a rectangle component with a solid color.
		/// </summary>
		/// <param name="game">The game this component will belong to.</param>
		/// <param name="renderer">The image renderer to draw with (can be changed later).</param>
		/// <param name="w">The width of the image generated.</param>
		/// <param name="h">The height of the image generated.</param>
		/// <param name="c">The color to draw with.</param>
		public RectangleComponent(Game game, SpriteBatch? renderer, int w, int h, Color c) : base(game,renderer,ColorFunctions.GenerateTexture(game,w,h,ColorFunctions.SolidColor(c)))
		{return;}
		
		/// <summary>
		/// Creates a rectangle component with a textured generated according to <paramref name="func"/>.
		/// </summary>
		/// <param name="game">The game this component will belong to.</param>
		/// <param name="renderer">The image renderer to draw with (can be changed later).</param>
		/// <param name="w">The width of the image generated.</param>
		/// <param name="h">The height of the image generated.</param>
		/// <param name="func">The color generating function.</param>
		public RectangleComponent(Game game, SpriteBatch? renderer, int w, int h, ColorFunction func) : base(game,renderer,ColorFunctions.GenerateTexture(game,w,h,func))
		{return;}

		protected override void UnloadContent()
		{
			Source!.Dispose();
			return;
		}
	}
}
