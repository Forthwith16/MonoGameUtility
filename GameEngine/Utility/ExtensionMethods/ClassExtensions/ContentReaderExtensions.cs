using GameEngine.Maths;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace GameEngine.Utility.ExtensionMethods.ClassExtensions
{
	/// <summary>
	/// Adds additional read abilities to the ContentReader.
	/// </summary>
	public static class ContentReaderExtensions
	{
		/// <summary>
		/// Read a rectangle from the stream.
		/// </summary>
		/// <param name="cin">The current context to read from.</param>
		public static Rectangle ReadRectangle(this ContentReader cin)
		{return new Rectangle(cin.ReadInt32(),cin.ReadInt32(),cin.ReadInt32(),cin.ReadInt32());}

		/// <summary>
		/// Reads a matrix from the stream.
		/// The matrix must be given as a translation (Vector2/two floats), scale (Vector2/two floats), and rotation (float) in that order.
		/// The resulting matrix will be given as three respective matrices multiplied together also in that order.
		/// </summary>
		/// <param name="cin">The current context to read from.</param>
		public static Matrix2D ReadMatrix2DComponents(this ContentReader cin)
		{
			Vector2 t = cin.ReadVector2();
			float r = cin.ReadSingle();
			Vector2 s = cin.ReadVector2();
			Vector2 o = cin.ReadVector2();

			return Matrix2D.FromPositionRotationScaleOrigin(t,r,s,o);
		}

		/// <summary>
		/// Reads an enum from the stream.
		/// </summary>
		/// <typeparam name="T">The enum type to read.</typeparam>
		/// <param name="cin">The context to read from.</param>
		/// <returns>Returns the enum read.</returns>
		public static T ReadEnum<T>(this ContentReader cin) where T : struct, Enum => Enum.Parse<T>(cin.ReadString());
	}
}
