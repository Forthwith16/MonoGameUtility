using Microsoft.Xna.Framework;

namespace GameEngine.Utility.ExtensionMethods.PrimitiveExtensions
{
	/// <summary>
	/// Extensions for the Point class.
	/// </summary>
	public static class PointExtensions
	{
		/// <summary>
		/// Determines the taxicab distance between two points.
		/// </summary>
		/// <param name="a">The first point.</param>
		/// <param name="b">The second point. If this is null, it defaults to the origin (0,0).</param>
		/// <returns>Returns the taxicab distance between points <paramref name="a"/> and <paramref name="b"/>.</returns>
		public static int TaxicabDistance(this Point a, Point? b = null)
		{
			if(b is null)
				return Math.Abs(a.X) + Math.Abs(a.Y);
			
			return Math.Abs(b.Value.X - a.X) + Math.Abs(b.Value.Y - a.Y);
		}

		/// <summary>
		/// Creates a new point that is the additive inverse of this point.
		/// </summary>
		/// <param name="p">The point to negate.</param>
		/// <returns>Returns -p.</returns>
		public static Point UnaryMinus(this Point p)
		{return new Point(-p.X,-p.Y);}
	}
}
