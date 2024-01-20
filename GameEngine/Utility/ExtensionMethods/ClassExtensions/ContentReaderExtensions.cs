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
		/// Read a rectangle from the current stream.
		/// </summary>
		/// <param name="cin">The current context to read from.</param>
		public static Rectangle ReadRectangle(this ContentReader cin)
		{return new Rectangle(cin.ReadInt32(),cin.ReadInt32(),cin.ReadInt32(),cin.ReadInt32());}

		/// <summary>
		/// Reads a matrix from the current stream.
		/// The matrix must be given as a translation (Vector2/two floats), scale (Vector2/two floats), and rotation (float) in that order.
		/// The resulting matrix will be given as three respective matrices multiplied together also in that order.
		/// </summary>
		/// <param name="cin">The current context to read from.</param>
		public static Matrix2D ReadMatrix2D(this ContentReader cin)
		{return Matrix2D.Translation(cin.ReadSingle(),cin.ReadSingle()) * Matrix2D.Scaling(cin.ReadSingle(),cin.ReadSingle()) * Matrix2D.Rotation(cin.ReadSingle());}
	}
}
