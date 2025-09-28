using GameEngine.Maths;
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
		/// <param name="renderer">The means by which we will draw the polygon.</param>
		/// <param name="tint">The color tint to draw the polygon texture with.</param>
		/// <param name="thickness">The thickness of the polygon.</param>
		/// <param name="layer_depth">The layer depth to draw at. This value should lie in the interval [0,1], where smaller values are considered the 'back' and larger values the 'front'.</param>
		/// <param name="points">The points of the polygon in clockwise or counterclockwise order. An extra line from the last to the first will be drawn to complete the polygon.</param>
		public static void DrawPolygon(this SpriteBatch renderer, Color tint, float thickness, float layer_depth, params Vector2[] points)
		{
			renderer.DrawPolygon(renderer.GetPixel(),tint,thickness,layer_depth,(IReadOnlyList<Vector2>)points);
			return;
		}

		/// <summary>
		/// Draws a polygon.
		/// </summary>
		/// <param name="renderer">The means by which we will draw the polygon.</param>
		/// <param name="tint">The color tint to draw the polygon texture with.</param>
		/// <param name="thickness">The thickness of the polygon.</param>
		/// <param name="layer_depth">The layer depth to draw at. This value should lie in the interval [0,1], where smaller values are considered the 'back' and larger values the 'front'.</param>
		/// <param name="points">The points of the polygon in clockwise or counterclockwise order. An extra line from the last to the first will be drawn to complete the polygon.</param>
		public static void DrawPolygon(this SpriteBatch renderer, Color tint, float thickness, float layer_depth, IReadOnlyList<Vector2> points)
		{
			renderer.DrawPolygon(renderer.GetPixel(),tint,thickness,layer_depth,points);
			return;
		}

		/// <summary>
		/// Draws a polygon.
		/// </summary>
		/// <param name="renderer">The means by which we will draw the polygon.</param>
		/// <param name="line_texture">The texture to draw the polygon with. Often, users will want this to be a single pixel, in which case omit this parameter.</param>
		/// <param name="tint">The color tint to draw the polygon texture with.</param>
		/// <param name="thickness">The thickness of the polygon.</param>
		/// <param name="layer_depth">The layer depth to draw at. This value should lie in the interval [0,1], where smaller values are considered the 'back' and larger values the 'front'.</param>
		/// <param name="points">The points of the polygon in clockwise or counterclockwise order. An extra line from the last to the first will be drawn to complete the polygon.</param>
		public static void DrawPolygon(this SpriteBatch renderer, Texture2D line_texture, Color tint, float thickness, float layer_depth, params Vector2[] points)
		{
			renderer.DrawPolygon(line_texture,tint,thickness,layer_depth,(IReadOnlyList<Vector2>)points);
			return;
		}

		/// <summary>
		/// Draws a polygon.
		/// </summary>
		/// <param name="renderer">The means by which we will draw the polygon.</param>
		/// <param name="line_texture">The texture to draw the polygon with. Often, users will want this to be a single pixel, in which case omit this parameter.</param>
		/// <param name="tint">The color tint to draw the polygon texture with.</param>
		/// <param name="thickness">The thickness of the polygon.</param>
		/// <param name="layer_depth">The layer depth to draw at. This value should lie in the interval [0,1], where smaller values are considered the 'back' and larger values the 'front'.</param>
		/// <param name="points">The points of the polygon in clockwise or counterclockwise order. An extra line from the last to the first will be drawn to complete the polygon.</param>
		public static void DrawPolygon(this SpriteBatch renderer, Texture2D line_texture, Color tint, float thickness, float layer_depth, IReadOnlyList<Vector2> points)
		{
			// If we don't have a polygon or even a line, do nothing
			if(points.Count < 2)
				return;

			// Draw each side of the polygon
			if(points.Count > 2)
				for(int i = 1;i < points.Count;i++)
					renderer.DrawLine(line_texture,points[i - 1],points[i],tint,thickness,layer_depth);

			// Special case the most annoying case last to complete the polygon
			renderer.DrawLine(line_texture,points[points.Count - 1],points[0],tint,thickness,layer_depth);

			return;
		}

		/// <summary>
		/// Draws a regular n-gon.
		/// </summary>
		/// <param name="renderer">The means by which we will draw the polygon.</param>
		/// <param name="center">The center of the polygon</param>
		/// <param name="radius">The radius of circumcircle of the polygon.</param>
		/// <param name="angle">The angle (in radians) to place the first point of the regular n-gon at with respect to the center. The angle is measured with respect to the usual right-handed coordinate plane.</param>
		/// <param name="tint">The color tint to draw the polygon texture with.</param>
		/// <param name="thickness">The thickness of the polygon.</param>
		/// <param name="layer_depth">The layer depth to draw at. This value should lie in the interval [0,1], where smaller values are considered the 'back' and larger values the 'front'.</param>
		/// <param name="n">The number of sides to the n-gon.</param>
		public static void DrawRegularNGon(this SpriteBatch renderer, Vector2 center, float radius, float angle, Color tint, float thickness, float layer_depth, int n)
		{
			renderer.DrawRegularNGon(renderer.GetPixel(),center,radius,angle,tint,thickness,layer_depth,n);
			return;
		}

		/// <summary>
		/// Draws a regular n-gon.
		/// </summary>
		/// <param name="renderer">The means by which we will draw the polygon.</param>
		/// <param name="line_texture">The texture to draw the polygon with. Often, users will want this to be a single pixel, in which case omit this parameter.</param>
		/// <param name="center">The center of the polygon</param>
		/// <param name="radius">The radius of circumcircle of the polygon.</param>
		/// <param name="angle">The angle (in radians) to place the first point of the regular n-gon at with respect to the center. The angle is measured with respect to the usual right-handed coordinate plane.</param>
		/// <param name="tint">The color tint to draw the polygon texture with.</param>
		/// <param name="thickness">The thickness of the polygon.</param>
		/// <param name="layer_depth">The layer depth to draw at. This value should lie in the interval [0,1], where smaller values are considered the 'back' and larger values the 'front'.</param>
		/// <param name="n">The number of sides to the n-gon.</param>
		public static void DrawRegularNGon(this SpriteBatch renderer, Texture2D line_texture, Vector2 center, float radius, float angle, Color tint, float thickness, float layer_depth, int n)
		{
			// I don't know why you would want this, but just no
			if(n < 3)
				return;

			Matrix2D rot = Matrix2D.Rotation(MathF.Tau / n);
			Vector2 prev = Matrix2D.Rotation(angle) * rot * new Vector2(radius,0.0f);
			Vector2 cur = rot * prev;
			
			for(int i = 0;i < n;i++)
			{
				renderer.DrawLine(line_texture,center + prev,center + cur,tint,thickness,layer_depth);

				prev = cur;
				cur = rot * cur;
			}

			return;
		}

		/// <summary>
		/// Draws a circle by approximating it with a sequence of drawn lines.
		/// </summary>
		/// <param name="renderer">The means by which we will draw the circle.</param>
		/// <param name="center">The center of the circle</param>
		/// <param name="radius">The radius of the circle.</param>
		/// <param name="tint">The color tint to draw the circle texture with.</param>
		/// <param name="thickness">The thickness of the circle.</param>
		/// <param name="layer_depth">The layer depth to draw at. This value should lie in the interval [0,1], where smaller values are considered the 'back' and larger values the 'front'.</param>
		/// <param name="approximation_points">The number of points to draw the circle with. In practice, we draw a regular <paramref name="approximation_points"/>-gon to approximate the circle.</param>
		public static void DrawCircle(this SpriteBatch renderer, Vector2 center, float radius, Color tint, float thickness, float layer_depth, int approximation_points = 20)
		{
			renderer.DrawCircle(renderer.GetPixel(),center,radius,tint,thickness,layer_depth,approximation_points);
			return;
		}

		/// <summary>
		/// Draws a circle by approximating it with a sequence of drawn lines.
		/// </summary>
		/// <param name="renderer">The means by which we will draw the circle.</param>
		/// <param name="line_texture">The texture to draw the circle with. Often, users will want this to be a single pixel, in which case omit this parameter.</param>
		/// <param name="center">The center of the circle</param>
		/// <param name="radius">The radius of the circle.</param>
		/// <param name="tint">The color tint to draw the circle texture with.</param>
		/// <param name="thickness">The thickness of the circle.</param>
		/// <param name="layer_depth">The layer depth to draw at. This value should lie in the interval [0,1], where smaller values are considered the 'back' and larger values the 'front'.</param>
		/// <param name="approximation_points">The number of points to draw the circle with. In practice, we draw a regular <paramref name="approximation_points"/>-gon to approximate the circle.</param>
		public static void DrawCircle(this SpriteBatch renderer, Texture2D line_texture, Vector2 center, float radius, Color tint, float thickness, float layer_depth, int approximation_points = 20)
		{
			// I don't know why you would want this, but just no
			if(approximation_points < 3)
				return;

			Matrix2D rot = Matrix2D.Rotation(MathF.Tau / approximation_points);
			Vector2 cur = rot * new Vector2(0.0f,radius);
			Vector2 prev = new Vector2(0.0f,radius);
			
			for(int i = 0;i < approximation_points;i++)
			{
				renderer.DrawLine(line_texture,center + prev,center + cur,tint,thickness,layer_depth);

				prev = cur;
				cur = rot * cur;
			}

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
