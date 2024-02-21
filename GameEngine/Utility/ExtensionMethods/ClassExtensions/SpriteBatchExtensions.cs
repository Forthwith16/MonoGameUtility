using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngine.Utility.ExtensionMethods.ClassExtensions
{
	/// <summary>
	/// Primitive drawing extensions for the SpriteBatch class.
	/// </summary>
	public static class SpriteBatchExtensions
	{
		/// <summary>
		/// Draws a line from point <paramref name="a"/> to point <paramref name="b"/>.
		/// </summary>
		/// <param name="renderer">The means by which we will draw the line.</param>
		/// <param name="a">The point to start drawing the line from.</param>
		/// <param name="b">The point to end drawing the line at.</param>
		/// <param name="tint">The color tint to draw the line texture with.</param>
		/// <param name="thickness">The thickness of the line.</param>
		/// <param name="layer_depth">The layer depth to draw at. This value should lie in the interval [0,1], where smaller values are considered the 'back' and larger values the 'front'.</param>
		public static void DrawLine(this SpriteBatch renderer, Vector2 a, Vector2 b, Color tint, float thickness, float layer_depth)
		{
			// Remember that the y-axis points down in screen coordinates, so to get the right (oppositely signed) angle, we swap b.Y - a.Y to a.Y - b.Y
			renderer.DrawLine(renderer.GetPixel(),a,(b - a).Length(),MathF.Atan2(a.Y - b.Y,b.X - a.X),tint,thickness,layer_depth);

			return;
		}

		/// <summary>
		/// Draws a line from point <paramref name="a"/> at angle <paramref name="angle"/> a distance of <paramref name="length"/>.
		/// </summary>
		/// <param name="renderer">The means by which we will draw the line.</param>
		/// <param name="p">The point to start drawing the line from.</param>
		/// <param name="angle">The angle to draw the line from <paramref name="p"/> at. This angle a counterclockwise angle made with the x-axis in the usual right-handed coordinate system.</param>
		/// <param name="length">The length of the line to draw.</param>
		/// <param name="tint">The color tint to draw the line texture with.</param>
		/// <param name="thickness">The thickness of the line.</param>
		/// <param name="layer_depth">The layer depth to draw at. This value should lie in the interval [0,1], where smaller values are considered the 'back' and larger values the 'front'.</param>
		public static void DrawLine(this SpriteBatch renderer, Vector2 p, float length, float angle, Color tint, float thickness, float layer_depth)
		{
			renderer.DrawLine(renderer.GetPixel(),p,length,angle,tint,thickness,layer_depth);
			return;
		}

		/// <summary>
		/// Draws a line from point <paramref name="a"/> to point <paramref name="b"/>.
		/// </summary>
		/// <param name="renderer">The means by which we will draw the line.</param>
		/// <param name="line_texture">The texture to draw the line with. Often, users will want this to be a single pixel, in which case omit this parameter.</param>
		/// <param name="a">The point to start drawing the line from.</param>
		/// <param name="b">The point to end drawing the line at.</param>
		/// <param name="tint">The color tint to draw the line texture with.</param>
		/// <param name="thickness">The thickness of the line.</param>
		/// <param name="layer_depth">The layer depth to draw at. This value should lie in the interval [0,1], where smaller values are considered the 'back' and larger values the 'front'.</param>
		public static void DrawLine(this SpriteBatch renderer, Texture2D line_texture, Vector2 a, Vector2 b, Color tint, float thickness, float layer_depth)
		{
			// Remember that the y-axis points down in screen coordinates, so to get the right (oppositely signed) angle, we swap b.Y - a.Y to a.Y - b.Y
			renderer.DrawLine(line_texture,a,(b - a).Length(),MathF.Atan2(a.Y - b.Y,b.X - a.X),tint,thickness,layer_depth);

			return;
		}

		/// <summary>
		/// Draws a line from point <paramref name="a"/> at angle <paramref name="angle"/> a distance of <paramref name="length"/>.
		/// </summary>
		/// <param name="renderer">The means by which we will draw the line.</param>
		/// <param name="line_texture">The texture to draw the line with. Often, users will want this to be a single pixel, in which case omit this parameter.</param>
		/// <param name="p">The point to start drawing the line from.</param>
		/// <param name="angle">The angle to draw the line from <paramref name="p"/> at. This angle a counterclockwise angle made with the x-axis in the usual right-handed coordinate system.</param>
		/// <param name="length">The length of the line to draw.</param>
		/// <param name="tint">The color tint to draw the line texture with.</param>
		/// <param name="thickness">The thickness of the line.</param>
		/// <param name="layer_depth">The layer depth to draw at. This value should lie in the interval [0,1], where smaller values are considered the 'back' and larger values the 'front'.</param>
		public static void DrawLine(this SpriteBatch renderer, Texture2D line_texture, Vector2 p, float length, float angle, Color tint, float thickness, float layer_depth)
		{
			// Recall that angles are oppositely signed when it comes to screen coordinates in 2D, unfortunately
			// Also, we are drawing the entire texture, so the scale factor needs to be scaled down by the side of the texture to get the correct dimensions
			renderer.Draw(line_texture,p,null,tint,-angle,new Vector2(0.0f,0.5f),new Vector2(length / line_texture.Width,thickness / line_texture.Height),SpriteEffects.None,layer_depth);

			return;
		}

		/// <summary>
		/// Draws a polygon.
		/// </summary>
		/// <param name="renderer">The means by which we will draw the line.</param>
		/// <param name="tint">The color tint to draw the line texture with.</param>
		/// <param name="thickness">The thickness of the line.</param>
		/// <param name="layer_depth">The layer depth to draw at. This value should lie in the interval [0,1], where smaller values are considered the 'back' and larger values the 'front'.</param>
		/// <param name="points">The points of the polygon in clockwise or counterclockwise order. An extra line from the last to the first will be drawn to complete the polygon.</param>
		public static void DrawPolygon(this SpriteBatch renderer, Color tint, float thickness, float layer_depth, params Vector2[] points)
		{
			renderer.DrawPolygon(renderer.GetPixel(),tint,thickness,layer_depth,points);
			return;
		}

		/// <summary>
		/// Draws a polygon.
		/// </summary>
		/// <param name="renderer">The means by which we will draw the line.</param>
		/// <param name="line_texture">The texture to draw the line with. Often, users will want this to be a single pixel, in which case omit this parameter.</param>
		/// <param name="tint">The color tint to draw the line texture with.</param>
		/// <param name="thickness">The thickness of the line.</param>
		/// <param name="layer_depth">The layer depth to draw at. This value should lie in the interval [0,1], where smaller values are considered the 'back' and larger values the 'front'.</param>
		/// <param name="points">The points of the polygon in clockwise or counterclockwise order. An extra line from the last to the first will be drawn to complete the polygon.</param>
		public static void DrawPolygon(this SpriteBatch renderer, Texture2D line_texture, Color tint, float thickness, float layer_depth, params Vector2[] points)
		{
			// If we don't have a polygon or even a line, do nothing
			if(points.Length < 2)
				return;

			// Draw each side of the polygon
			if(points.Length > 2)
				for(int i = 1;i < points.Length; i++)
					renderer.DrawLine(line_texture,points[i - 1],points[i],tint,thickness,layer_depth);

			// Special case the most annoying case last to complete the polygon
			renderer.DrawLine(line_texture,points[points.Length - 1],points[0],tint,thickness,layer_depth);

			return;
		}

		/// <summary>
		/// Produces a (singleton) single pixel used to draw stuff.
		/// </summary>
		private static Texture2D GetPixel(this SpriteBatch renderer)
		{
			if(_dp is null)
			{
				_dp = new Texture2D(renderer.GraphicsDevice,1,1,false,SurfaceFormat.Color);
				_dp.SetData(new Color[] {Color.White});
			}

			return _dp;
		}

		private static Texture2D? _dp;
	}
}
