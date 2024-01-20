using GameEngine.Framework;

namespace GameEngine.Utility.ExtensionMethods.PrimitiveExtensions
{
	/// <summary>
	/// Provides extension functions to floats.
	/// </summary>
	public static class FloatExtensions
	{
		/// <summary>
		/// Determines if two floats are close enough to be considered identical.
		/// </summary>
		/// <param name="a">The first float.</param>
		/// <param name="b">The second float.</param>
		/// <returns>Returns true if <paramref name="a"/> and <paramref name="b"/> are within EPSILON of each other and false otherwise.</returns>
		public static bool CloseEnough(this float a, float b)
		{return a.CloseEnough(b,GlobalConstants.EPSILON);}

		/// <summary>
		/// Determines if two floats are close enough to be considered identical.
		/// </summary>
		/// <param name="a">The first float.</param>
		/// <param name="b">The second float.</param>
		/// <param name="epsilon">The permissible margin of error.</param>
		/// <returns>Returns true if <paramref name="a"/> and <paramref name="b"/> are within <paramref name="epsilon"/> of each other and false otherwise.</returns>
		public static bool CloseEnough(this float a, float b, float epsilon)
		{return Math.Abs(a - b) < epsilon;}
	}
}
