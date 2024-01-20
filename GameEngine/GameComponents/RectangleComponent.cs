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
		public RectangleComponent(Game game, SpriteBatch? renderer, int w, int h, Color c) : base(game,renderer,GenerateTexture(game,w,h,ColorFunctions.SolidColor(c)))
		{return;}
		
		/// <summary>
		/// Creates a rectangle component with a textured generated according to <paramref name="func"/>.
		/// </summary>
		/// <param name="game">The game this component will belong to.</param>
		/// <param name="renderer">The image renderer to draw with (can be changed later).</param>
		/// <param name="w">The width of the image generated.</param>
		/// <param name="h">The height of the image generated.</param>
		/// <param name="func">The color generating function.</param>
		public RectangleComponent(Game game, SpriteBatch? renderer, int w, int h, ColorFunction func) : base(game,renderer,GenerateTexture(game,w,h,func))
		{return;}

		protected override void UnloadContent()
		{
			Source!.Dispose();
			return;
		}

		/// <summary>
		/// Geneartes a texture according to <paramref name="func"/> of dimensions <paramref name="w"/> by <paramref name="h"/>.
		/// </summary>
		/// <param name="game">The game with the GraphicsDevice to use to create the texture.</param>
		/// <param name="w">The width of the texture to generate.</param>
		/// <param name="h">The height of the texture to generate.</param>
		/// <param name="func">The color generation function.</param>
		public static Texture2D GenerateTexture(Game game, int w, int h, ColorFunction func)
		{
			Texture2D ret = new Texture2D(game.GraphicsDevice,w,h);
			Color[] data = new Color[w * h];
			
			for(int x = 0;x < w;x++)
				for(int y = 0;y < h;y++)
					data[y * w + x] = func(x,y);
			
			ret.SetData(data);
			return ret;
		}
	}

	/// <summary>
	/// Transforms a position (<paramref name="x"/>,<paramref name="y"/>) into a color.
	/// </summary>
	public delegate Color ColorFunction(int x, int y);

	/// <summary>
	/// A collection of common color functions.
	/// </summary>
	public static class ColorFunctions
	{
		/// <summary>
		/// Creates a ColorFunction that returns a constant color <paramref name="c"/>.
		/// </summary>
		public static ColorFunction SolidColor(Color c)
		{return (x,y) => c;}

		/// <summary>
		/// Creates a ColorFunction that generates a horizontal gradient.
		/// </summary>
		/// <param name="w">The width of the gradient. If this is smaller than the width of image being generated, extra color values will clamp at the boundaries.</param>
		/// <param name="l">The leftmost color.</param>
		/// <param name="r">The rightmost color.</param>
		public static ColorFunction HorizontalGradient(int w, Color l, Color r)
		{
			// We precompute this value so that we don't have to do it over and over
			float fw = w - 1.0f;

			return (x,y) => Color.Lerp(l,r,x / fw);
		}

		/// <summary>
		/// Creates a ColorFunction that generates a horizontal gradient.
		/// </summary>
		/// <param name="w">The width of the gradient. If this is smaller than the width of image being generated, extra color values will clamp at the boundaries.</param>
		/// <param name="t">The topmost color.</param>
		/// <param name="b">The bottommost color.</param>
		public static ColorFunction VerticalGradient(int h, Color t, Color b)
		{
			// We precompute this value so that we don't have to do it over and over
			float fh = h - 1.0f;

			return (x,y) => Color.Lerp(t,b,y / fh);
		}

		/// <summary>
		/// Creates a ColorFunction that generates a checkerboard pattern.
		/// </summary>
		/// <param name="w">The width of each tile.</param>
		/// <param name="h">The height of each tile.</param>
		/// <param name="odd">The color of 'odd' tiles, that is those whose x and y indices sum to an odd number.</param>
		/// <param name="even">The color of 'even' tiles, that is those whose x and y indices sum to an even number.</param>
		/// <returns></returns>
		public static ColorFunction Checkerboard(int w, int h, Color odd, Color even)
		{return (x,y) => ((x / w + y / h) & 0x1) == 0 ? even : odd;}

		/// <summary>
		/// Transforms a ColorFunction into graysacle.
		/// This will preserve alpha values.
		/// </summary>
		/// <param name="func">The function to make grayscale.</param>
		public static ColorFunction Grayscale(ColorFunction func)
		{
			return (x,y) =>
			{
				Vector4 c = func(x,y).ToVector4();
				float v = 0.299f * c.X + 0.587f * c.Y + 0.114f * c.Z;

				return new Color(v,v,v,c.W);
			};
		}
	}
}
