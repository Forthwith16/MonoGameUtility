using GameEngine.Maths;
using Microsoft.Xna.Framework;

namespace GameEngine.GameComponents
{
	/// <summary>
	/// Defines the bare bones of what it means to be an affine component.
	/// Affine components may be translated, rotated, or scaled.
	/// </summary>
	public interface IAffineComponent
	{
		public void jd()
		{return;}

		/// <summary>
		/// Applies a translation to this component.
		/// This is equivalent to a left multiplication by a translation matrix.
		/// </summary>
		/// <param name="tx">The horizontal distance to translate by.</param>
		/// <param name="ty">The vertical distance to translate by.</param>
		/// <returns>Returns a new matrix containing the result.</returns>
		public void Translate(float tx, float ty);

		/// <summary>
		/// Applies a translation to this component.
		/// This is equivalent to a left multiplication by a translation matrix.
		/// </summary>
		/// <param name="t">The distance to translate by.</param>
		/// <returns>Returns a new matrix containing the result.</returns>
		public void Translate(Vector2 t);

		/// <summary>
		/// Applies a rotation to this component.
		/// This is equivalent to a left multiplication by a rotation matrix.
		/// </summary>
		/// <param name="angle">The rotation angle (in radians).</param>
		/// <param name="righthanded_chirality">
		///	Positive rotation values result in a counterclockwise rotation about its axis.
		///	Monogame's SpriteBatch Draw (but not Begin) z-axis points outward (left-handed), resulting in screen-counterclockwise rotations with positive values.
		///	The rotation matrix this function generates follows this scheme when this value is false.
		///	If this is true, it will produce the opposite result (a right-handed rotation matrix with the z-axis pointing inward).
		/// </param>
		/// <returns>Returns a new matrix containing the result.</returns>
		public void Rotate(float angle, bool righthanded_chirality = false);

		/// <summary>
		/// Applies a rotation to this component about a point.
		/// This is equivalent to a left multiplication by a rotation matrix about a point.
		/// </summary>
		/// <param name="angle">The rotation angle (in radians).</param>
		/// <param name="point">The point to rotate around.</param>
		/// <param name="righthanded_chirality">
		///	Positive rotation values result in a counterclockwise rotation about its axis.
		///	Monogame's SpriteBatch Draw (but not Begin) z-axis points outward (left-handed), resulting in screen-counterclockwise rotations with positive values.
		///	The rotation matrix this function generates follows this scheme when this value is false.
		///	If this is true, it will produce the opposite result (a right-handed rotation matrix with the z-axis pointing inward).
		/// </param>
		/// <returns>Returns a new matrix containing the result.</returns>
		public void Rotate(float angle, Vector2 point, bool righthanded_chirality = false);

		/// <summary>
		/// Applies a rotation to this component about a point.
		/// This is equivalent to a left multiplication by a rotation matrix about a point.
		/// </summary>
		/// <param name="angle">The rotation angle (in radians).</param>
		/// <param name="x">The x coordinate to rotate around.</param>
		/// <param name="y">The y coordinate to rotate around.</param>
		/// <param name="righthanded_chirality">
		///	Positive rotation values result in a counterclockwise rotation about its axis.
		///	Monogame's SpriteBatch Draw (but not Begin) z-axis points outward (left-handed), resulting in screen-counterclockwise rotations with positive values.
		///	The rotation matrix this function generates follows this scheme when this value is false.
		///	If this is true, it will produce the opposite result (a right-handed rotation matrix with the z-axis pointing inward).
		/// </param>
		/// <returns>Returns a new matrix containing the result.</returns>
		public void Rotate(float angle, float x, float y, bool righthanded_chirality = false);

		/// <summary>
		/// Applies a clean 90 degree rotation <paramref name="times"/> times to this component.
		/// This is done in a single multiplication.
		/// This is equivalent to a left multiplication by a rotation matrix.
		/// </summary>
		/// <param name="times">The number of 90 degree clockwise rotations to perform.</param>
		/// <param name="righthanded_chirality">
		///	Positive rotation values result in a counterclockwise rotation about its axis.
		///	Monogame's SpriteBatch Draw (but not Begin) z-axis points outward (left-handed), resulting in screen-counterclockwise rotations with positive values.
		///	The rotation matrix this function generates follows this scheme when this value is false.
		///	If this is true, it will produce the opposite result (a right-handed rotation matrix with the z-axis pointing inward).
		/// </param>
		/// <returns>Returns a new matrix containing the result.</returns>
		public void Rotate90(int times, bool righthanded_chirality = false);

		/// <summary>
		/// Applies a clean 90 degree rotation <paramref name="times"/> times to this component about a point.
		/// The rotation is done in a single multiplication.
		/// This is equivalent to a left multiplication by a rotation matrix about a point.
		/// </summary>
		/// <param name="times">The number of 90 degree clockwise rotations to perform.</param>
		/// <param name="point">The point to rotate around.</param>
		/// <param name="righthanded_chirality">
		///	Positive rotation values result in a counterclockwise rotation about its axis.
		///	Monogame's SpriteBatch Draw (but not Begin) z-axis points outward (left-handed), resulting in screen-counterclockwise rotations with positive values.
		///	The rotation matrix this function generates follows this scheme when this value is false.
		///	If this is true, it will produce the opposite result (a right-handed rotation matrix with the z-axis pointing inward).
		/// </param>
		/// <returns>Returns a new matrix containing the result.</returns>
		public void Rotate90(int times, Vector2 point, bool righthanded_chirality = false);

		/// <summary>
		/// Applies a clean 90 degree rotation <paramref name="times"/> times to this component about a point.
		/// The rotation is done in a single multiplication.
		/// This is equivalent to a left multiplication by a rotation matrix about a point.
		/// </summary>
		/// <param name="times">The number of 90 degree clockwise rotations to perform.</param>
		/// <param name="x">The x coordinate to rotate around.</param>
		/// <param name="y">The y coordinate to rotate around.</param>
		/// <param name="righthanded_chirality">
		///	Positive rotation values result in a counterclockwise rotation about its axis.
		///	Monogame's SpriteBatch Draw (but not Begin) z-axis points outward (left-handed), resulting in screen-counterclockwise rotations with positive values.
		///	The rotation matrix this function generates follows this scheme when this value is false.
		///	If this is true, it will produce the opposite result (a right-handed rotation matrix with the z-axis pointing inward).
		/// </param>
		public void Rotate90(int times, float x, float y, bool righthanded_chirality = false);

		/// <summary>
		/// Applies a scale factor to this component.
		/// This is equivalent to a left multiplication by a scale matrix.
		/// </summary>
		/// <param name="s">The uniform scale factor along both axes.</param>
		/// <remarks>Reflections are not propertly supported (i.e. negative scalings) via MonoGame's SpriteBatch, so use SpriteEffects to achieve reflections instead. Negative scalings are not, however, prohibited, as there are other uses for matrices.</remarks>
		public void Scale(float s);

		/// <summary>
		/// Applies a scale factor to this component.
		/// This is equivalent to a left multiplication by a scale matrix.
		/// </summary>
		/// <param name="s">The uniform scale factor along both axes.</param>
		/// <param name="x">The x position to scale about.</param>
		/// <param name="y">The y position to scale about.</param>
		/// <remarks>Reflections are not propertly supported (i.e. negative scalings) via MonoGame's SpriteBatch, so use SpriteEffects to achieve reflections instead. Negative scalings are not, however, prohibited, as there are other uses for matrices.</remarks>
		public void Scale(float s, float x, float y);

		/// <summary>
		/// Applies a scale factor to this component.
		/// This is equivalent to a left multiplication by a scale matrix.
		/// </summary>
		/// <param name="s">The uniform scale factor along both axes.</param>
		/// <param name="pos">The position to scale about.</param>
		/// <remarks>Reflections are not propertly supported (i.e. negative scalings) via MonoGame's SpriteBatch, so use SpriteEffects to achieve reflections instead. Negative scalings are not, however, prohibited, as there are other uses for matrices.</remarks>
		public void Scale(float s, Vector2 pos);

		/// <summary>
		/// Applies a scale factor to this component.
		/// This is equivalent to a left multiplication by a scale matrix.
		/// </summary>
		/// <param name="sx">The scale factor for the horizontal axis.</param>
		/// <param name="sx">The scale factor for the vertical axis.</param>
		/// <remarks>Reflections are not propertly supported (i.e. negative scalings) via MonoGame's SpriteBatch, so use SpriteEffects to achieve reflections instead. Negative scalings are not, however, prohibited, as there are other uses for matrices.</remarks>
		public void Scale(float sx, float sy);

		/// <summary>
		/// Applies a scale factor to this component.
		/// This is equivalent to a left multiplication by a scale matrix.
		/// </summary>
		/// <param name="sx">The scale factor for the horizontal axis.</param>
		/// <param name="sx">The scale factor for the vertical axis.</param>
		/// <param name="x">The x position to scale about.</param>
		/// <param name="y">The y position to scale about.</param>
		/// <remarks>Reflections are not propertly supported (i.e. negative scalings) via MonoGame's SpriteBatch, so use SpriteEffects to achieve reflections instead. Negative scalings are not, however, prohibited, as there are other uses for matrices.</remarks>
		public void Scale(float sx, float sy, float x, float y);

		/// <summary>
		/// Applies a scale factor to this component.
		/// This is equivalent to a left multiplication by a scale matrix.
		/// </summary>
		/// <param name="sx">The scale factor for the horizontal axis.</param>
		/// <param name="sx">The scale factor for the vertical axis.</param>
		/// <param name="pos">The position to scale about.</param>
		/// <remarks>Reflections are not propertly supported (i.e. negative scalings) via MonoGame's SpriteBatch, so use SpriteEffects to achieve reflections instead. Negative scalings are not, however, prohibited, as there are other uses for matrices.</remarks>
		public void Scale(float sx, float sy, Vector2 pos);

		/// <summary>
		/// Applies a scale factor to this component.
		/// This is equivalent to a left multiplication by a scale matrix.
		/// </summary>
		/// <param name="s">The scale factor for each axis.</param>
		/// <remarks>Reflections are not propertly supported (i.e. negative scalings) via MonoGame's SpriteBatch, so use SpriteEffects to achieve reflections instead. Negative scalings are not, however, prohibited, as there are other uses for matrices.</remarks>
		public void Scale(Vector2 s);

		/// <summary>
		/// Applies a scale factor to this component.
		/// This is equivalent to a left multiplication by a scale matrix.
		/// </summary>
		/// <param name="s">The scale factor for each axis.</param>
		/// <param name="x">The x position to scale about.</param>
		/// <param name="y">The y position to scale about.</param>
		/// <remarks>Reflections are not propertly supported (i.e. negative scalings) via MonoGame's SpriteBatch, so use SpriteEffects to achieve reflections instead. Negative scalings are not, however, prohibited, as there are other uses for matrices.</remarks>
		public void Scale(Vector2 s, float x, float y);

		/// <summary>
		/// Applies a scale factor to this component.
		/// This is equivalent to a left multiplication by a scale matrix.
		/// </summary>
		/// <param name="s">The scale factor for each axis.</param>
		/// <param name="pos">The position to scale about.</param>
		/// <remarks>Reflections are not propertly supported (i.e. negative scalings) via MonoGame's SpriteBatch, so use SpriteEffects to achieve reflections instead. Negative scalings are not, however, prohibited, as there are other uses for matrices.</remarks>
		public void Scale(Vector2 s, Vector2 pos);

		/// <summary>
		/// Obtains the world position of this affine component (this includes the parent transform).
		/// </summary>
		public Vector2 Position
		{get => World * Vector2.Zero;}

		/// <summary>
		/// The transformation matrix of this component.
		/// If this component has a parent, this includes the parent transform.
		/// The parent transform is applied second when it exists with this component's transform being applied first (i.e. Transform is left multiplied by Parent.Transform).
		/// <para/>
		/// For the raw transformation matrix, use Transform instead.
		/// </summary>
		public Matrix2D World
		{get;}

		/// <summary>
		/// The transformation matrix of this component absent its parent transform (if it has one).
		/// <para/>
		/// For the transformation matrix containing the parent (if present), use World instead.
		/// </summary>
		public Matrix2D Transform
		{get; set;}

		/// <summary>
		/// The parent of this component.
		/// Transforming the parent will transform this object in turn.
		/// </summary>
		public IAffineComponent? Parent
		{get; set;}
	}
}
