using GameEngine.Maths;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

namespace GameEnginePipeline.Writers
{
	/// <summary>
	/// Adds additional write abilities to the ContentWriter.
	/// </summary>
	public static class ContentWriterExtensions
	{
		/// <summary>
		/// Writes a rectangle to the stream.
		/// </summary>
		/// <param name="cout">The current context to write to.</param>
		/// <param name="rect">The rectangle to write.</param>
		public static void Write(this ContentWriter cout, Rectangle rect)
		{
			cout.Write(rect.X);
			cout.Write(rect.Y);
			cout.Write(rect.Width);
			cout.Write(rect.Height);
			
			return;
		}

		/// <summary>
		/// Writes a matrix to the stream.
		/// </summary>
		/// <param name="cout">The current context to write to.</param>
		/// <param name="m">The matrix to write.</param>
		public static void Write(this ContentWriter cout, Matrix2D m)
		{
			if(!m.Decompose(out Vector2 t,out float r,out Vector2 s))
				throw new ArgumentException(nameof(m));

			cout.Write(t);
			cout.Write(s);
			cout.Write(r);

			return;
		}

		/// <summary>
		/// Writes a matrix to the stream in component form.
		/// The matrix is given such that it can be recreated via a translation * scale * rotation operation.
		/// </summary>
		/// <param name="cout">The current context to write to.</param>
		/// <param name="translation">The translation component of the matrix.</param>
		/// <param name="rotation">The rotation component of the matrix.</param>
		/// <param name="scale">The scale component of the matrix.</param>
		/// <param name="origin">The origin component of the matrix.</param>
		public static void Write(this ContentWriter cout, Vector2 translation, float rotation, Vector2 scale, Vector2 origin)
		{
			cout.Write(translation);
			cout.Write(rotation);
			cout.Write(scale);
			cout.Write(origin);

			return;
		}

		/// <summary>
		/// Writes an enum to the stream.
		/// </summary>
		/// <typeparam name="T">The enum type to write.</typeparam>
		/// <param name="cout">The current context to write to.</param>
		/// <param name="value">The value ot write.</param>
		public static void Write<T>(this ContentWriter cout, T value) where T : Enum
		{
			cout.Write(value.ToString());
			return;
		}
	}
}
