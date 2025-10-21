using GameEngine.Utility.ExtensionMethods.PrimitiveExtensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngine.Texture
{
	/// <summary>
	/// A collection of common color functions.
	/// These are of particular use when generating textures from code.
	/// </summary>
	public static class ColorFunctions
	{
		/// <summary>
		/// Geneartes a texture according to <paramref name="func"/> of dimensions <paramref name="w"/> by <paramref name="h"/>.
		/// </summary>
		/// <param name="g">The GraphicsDevice to use to generate the texture.</param>
		/// <param name="w">The width of the texture to generate.</param>
		/// <param name="h">The height of the texture to generate.</param>
		/// <param name="func">The color generation function.</param>
		public static Texture2D GenerateTexture(GraphicsDevice g, int w, int h, ColorFunction func)
		{
			Texture2D ret = new Texture2D(g,w,h);
			Color[] data = new Color[w * h];
			
			for(int x = 0;x < w;x++)
				for(int y = 0;y < h;y++)
					data[y * w + x] = func(x,y);
			
			ret.SetData(data);
			return ret;
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

		/// <summary>
		/// Creates a ColorFunction that returns a constant color <paramref name="c"/>.
		/// </summary>
		public static ColorFunction SolidColor(Color c)
		{return (x,y) => c;}

		/// <summary>
		/// Creates a ColorFunction that makes a wireframe box.
		/// </summary>
		/// <param name="w">The width of the box.</param>
		/// <param name="h">The height of the box.</param>
		/// <param name="thickness">The thickness of the wireframe.</param>
		/// <param name="c">The color of the wireframe.</param>
		public static ColorFunction Wireframe(int w, int h, int thickness, Color c)
		{return (x,y) => x < thickness || x >= w - thickness || y < thickness || y >= h - thickness ? c : Color.Transparent;}

		/// <summary>
		/// Creates an elliptical ColorFunction that returns a constant color <paramref name="c"/> inside an ellipse whose horizontal and vertical radii center the ellipse in a <paramref name="w"/> by <paramref name="h"/> box.
		/// </summary>
		/// <param name="w">The horizontal diameter.</param>
		/// <param name="h">The vertical diameter.</param>
		/// <param name="c">The color of the ellipse.</param>
		public static ColorFunction Ellipse(int w, int h, Color c)
		{
			// We precompute these values so that we don't have to do so over and over
			float cx = w / 2.0f;
			float cy = h / 2.0f;

			float rx2 = cx * cx;
			float ry2 = cy * cy;

			return (x,y) =>
			{
				float x2 = x - cx;
				float y2 = y - cy;
				
				return x2 * x2 / rx2 + y2 * y2 / ry2 <= 1 ? c : Color.Transparent;
			};
		}

		/// <summary>
		/// Creates an elliptical wireframe ColorFunction with horizontal and vertical radii centered in a <paramref name="w"/> by <paramref name="h"/> box.
		/// </summary>
		/// <param name="w">The horizontal diameter.</param>
		/// <param name="h">The vertical diameter.</param>
		/// <param name="thickness">The thickness of the wireframe.</param>
		/// <param name="c">The color of the ellipse.</param>
		public static ColorFunction WireframeEllipse(int w, int h, int thickness, Color c)
		{
			// We precompute these values so that we don't have to do so over and over
			float cxo = w / 2.0f;
			float cyo = h / 2.0f;

			float rxo2 = cxo * cxo;
			float ryo2 = cyo * cyo;

			float cxi = w / 2.0f;
			float cyi = h / 2.0f;

			float rxi2 = (cxi - (thickness << 1)) * (cxi - (thickness << 1));
			float ryi2 = (cyi - (thickness << 1)) * (cyi - (thickness << 1));

			return (x,y) =>
			{
				float x2 = x - cxo;
				float y2 = y - cyo;
				
				if(x2 * x2 / rxo2 + y2 * y2 / ryo2 > 1)
					return Color.Transparent;

				return x2 * x2 / rxi2 + y2 * y2 / ryi2 < 1 ? Color.Transparent : c;
			};
		}

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

		/// <summary>
		/// Creates a ColorFunction that sums two ColorFunctions together.
		/// Colors clamp between black and white if overflowing or (somehow) underflowing.
		/// </summary>
		/// <param name="f1">The first color function.</param>
		/// <param name="f2">The second color function.</param>
		/// <param name="average_alpha">If true, then the average alpha value will be used isntead of the sum. Otherwise, the sum will be used for the alpha channel.</param>
		/// <returns>Returns a color function that computes <paramref name="f1"/> + <paramref name="f2"/>.</returns>
		public static ColorFunction Sum(ColorFunction f1, ColorFunction f2, bool average_alpha = true) => Mix(f1,f2,(a,b) => new Color(a.R + b.R,a.G + b.G,a.B + b.B,average_alpha ? (a.A + b.A) >> 1 : a.A + b.A));

		/// <summary>
		/// Creates a compound ColorFunction that places the <paramref name="f1"/> colors over the <paramref name="f2"/> colors.
		/// </summary>
		/// <param name="f1">The top colors.</param>
		/// <param name="f2">The bottom colors.</param>
		/// <param name="premultiplied">If true, <paramref name="f1"/> and <paramref name="f2"/> are treated as premultiplied color outputs, i.e. emmision and occlusion. If false, then they produce true RGBA values.</param>
		/// <returns>Returns the new blended ColorFunction.</returns>
		public static ColorFunction AlphaMultiplyOver(ColorFunction f1, ColorFunction f2, bool premultiplied = false)
		{
			if(premultiplied)
				return Mix(f1,f2,(a,b) =>
				{
					Vector4 va = a.ToVector4();
					return new Color(va + b.ToVector4() * (1.0f - va.W));
				});

			return Mix(f1,f2,(a,b) =>
			{
				Vector4 va = a.ToVector4();
				Vector4 vb = b.ToVector4();
				
				float am1 = 1.0f - va.W;
				float a0 = va.W + vb.W * am1;

				return new Color(new Vector4((va.Shed() * va.W + vb.W * am1 * vb.Shed()) / a0,a0));
			});
		}

		/// <summary>
		/// Creates a ColorFunction that mixes two ColorFunctions together.
		/// </summary>
		/// <param name="f1">The first color function.</param>
		/// <param name="f2">The second color function.</param>
		/// <param name="mixer">The means by which we mix colors.</param>
		/// <returns>Returns a color function that mixes <paramref name="f1"/> and <paramref name="f2"/>.</returns>
		public static ColorFunction Mix(ColorFunction f1, ColorFunction f2, ColorMixer mixer) => (x,y) => mixer(f1(x,y),f2(x,y));
	}

	/// <summary>
	/// Transforms a position (<paramref name="x"/>,<paramref name="y"/>) into a color.
	/// </summary>
	public delegate Color ColorFunction(int x, int y);

	/// <summary>
	/// Mixes two colors together.
	/// </summary>
	/// <param name="a">The first color.</param>
	/// <param name="b">The second color.</param>
	/// <returns>Returns a mixture of <paramref name="a"/> and <paramref name="b"/>.</returns>
	public delegate Color ColorMixer(Color a, Color b);
}
