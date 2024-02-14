using GameEngine.GameComponents;
using GameEngine.Maths;
using Microsoft.Xna.Framework;

namespace GameEngine.Utility.ExtensionMethods.InterfaceFunctions
{
	/// <summary>
	/// Extensions for common code to the IAffineComponet interface.
	/// These provide default implementations for methods that should be consistent across all affine objects.
	/// </summary>
	public static class AffineExtensions
	{
		/// <summary>
		/// Sets this affine object's Transform to a matrix built from a position, rotation, and scale.
		/// It is constructed by translating by -<paramref name="origin"/>, then scaling by <paramref name="scale"/>, then rotating by <paramref name="rotation"/> (about the z axis), and then translating by <paramref name="position"/>.
		/// This will place <b><u><paramref name="origin"/></u></b> (the rotational/scaling center) at <paramref name="position"/>.
		/// </summary>
		/// <param name="position">The translation to apply to this object.</param>
		/// <param name="rotation">The rotation (in radians about the z axis) to apply to this object.</param>
		/// <param name="scale">The scale to apply to this object.</param>
		/// <param name="righthanded_chirality">
		/// Positive rotation values result in a counterclockwise rotation about its axis.
		/// Monogame's SpriteBatch Draw (but not Begin) z-axis points outward (left-handed), resulting in screen-counterclockwise rotations with positive values.
		/// The rotation matrix this function generates follows this scheme when this value is false.
		/// If this is true, it will produce the opposite result (a right-handed rotation matrix with the z-axis pointing inward).
		/// </param>
		public static void TransformToPositionRotationScale(this IAffineComponent2D me, Vector2 position, float rotation, Vector2 scale, Vector2 origin = default(Vector2), bool righthanded_chirality = false)
		{
			me.Transform = Matrix2D.FromPositionRotationScale(position,rotation,scale,origin,righthanded_chirality);
			return;
		}

		/// <summary>
		/// Applies a translation to this component.
		/// This is equivalent to a left multiplication by a translation matrix.
		/// </summary>
		/// <param name="tx">The horizontal distance to translate by.</param>
		/// <param name="ty">The vertical distance to translate by.</param>
		/// <returns>Returns a new matrix containing the result.</returns>
		public static void Translate(this IAffineComponent2D me, float tx, float ty)
		{
			me.Transform = me.Transform.Translate(tx,ty);
			return;
		}

		/// <summary>
		/// Applies a translation to this component.
		/// This is equivalent to a left multiplication by a translation matrix.
		/// </summary>
		/// <param name="t">The distance to translate by.</param>
		/// <returns>Returns a new matrix containing the result.</returns>
		public static void Translate(this IAffineComponent2D me, Vector2 t)
		{
			me.Transform = me.Transform.Translate(t);
			return;
		}

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
		public static void Rotate(this IAffineComponent2D me, float angle, bool righthanded_chirality = false)
		{
			me.Transform = me.Transform.Rotate(angle,righthanded_chirality);
			return;
		}

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
		public static void Rotate(this IAffineComponent2D me, float angle, Vector2 point, bool righthanded_chirality = false)
		{
			me.Transform = me.Transform.Rotate(angle,point,righthanded_chirality);
			return;
		}

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
		public static void Rotate(this IAffineComponent2D me, float angle, float x, float y, bool righthanded_chirality = false)
		{
			me.Transform = me.Transform.Rotate(angle,x,y,righthanded_chirality);
			return;
		}

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
		public static void Rotate90(this IAffineComponent2D me, int times, bool righthanded_chirality = false)
		{
			me.Transform = me.Transform.Rotate90(times,righthanded_chirality);
			return;
		}

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
		public static void Rotate90(this IAffineComponent2D me, int times, Vector2 point, bool righthanded_chirality = false)
		{
			me.Transform = me.Transform.Rotate90(times,point,righthanded_chirality);
			return;
		}

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
		public static void Rotate90(this IAffineComponent2D me, int times, float x, float y, bool righthanded_chirality = false)
		{
			me.Transform = me.Transform.Rotate90(times,x,y,righthanded_chirality);
			return;
		}

		/// <summary>
		/// Applies a scale factor to this component.
		/// This is equivalent to a left multiplication by a scale matrix.
		/// </summary>
		/// <param name="s">The uniform scale factor along both axes.</param>
		/// <remarks>Reflections are not propertly supported (i.e. negative scalings) via MonoGame's SpriteBatch, so use SpriteEffects to achieve reflections instead. Negative scalings are not, however, prohibited, as there are other uses for matrices.</remarks>
		public static void Scale(this IAffineComponent2D me, float s)
		{
			me.Transform = me.Transform.Scale(s);
			return;
		}

		/// <summary>
		/// Applies a scale factor to this component.
		/// This is equivalent to a left multiplication by a scale matrix.
		/// </summary>
		/// <param name="s">The uniform scale factor along both axes.</param>
		/// <param name="x">The x position to scale about.</param>
		/// <param name="y">The y position to scale about.</param>
		/// <remarks>Reflections are not propertly supported (i.e. negative scalings) via MonoGame's SpriteBatch, so use SpriteEffects to achieve reflections instead. Negative scalings are not, however, prohibited, as there are other uses for matrices.</remarks>
		public static void Scale(this IAffineComponent2D me, float s, float x, float y)
		{
			me.Transform = me.Transform.Scale(s,x,y);
			return;
		}

		/// <summary>
		/// Applies a scale factor to this component.
		/// This is equivalent to a left multiplication by a scale matrix.
		/// </summary>
		/// <param name="s">The uniform scale factor along both axes.</param>
		/// <param name="pos">The position to scale about.</param>
		/// <remarks>Reflections are not propertly supported (i.e. negative scalings) via MonoGame's SpriteBatch, so use SpriteEffects to achieve reflections instead. Negative scalings are not, however, prohibited, as there are other uses for matrices.</remarks>
		public static void Scale(this IAffineComponent2D me, float s, Vector2 pos)
		{
			me.Transform = me.Transform.Scale(s,pos);
			return;
		}

		/// <summary>
		/// Applies a scale factor to this component.
		/// This is equivalent to a left multiplication by a scale matrix.
		/// </summary>
		/// <param name="sx">The scale factor for the horizontal axis.</param>
		/// <param name="sx">The scale factor for the vertical axis.</param>
		/// <remarks>Reflections are not propertly supported (i.e. negative scalings) via MonoGame's SpriteBatch, so use SpriteEffects to achieve reflections instead. Negative scalings are not, however, prohibited, as there are other uses for matrices.</remarks>
		public static void Scale(this IAffineComponent2D me, float sx, float sy)
		{
			me.Transform = me.Transform.Scale(sx,sy);
			return;
		}

		/// <summary>
		/// Applies a scale factor to this component.
		/// This is equivalent to a left multiplication by a scale matrix.
		/// </summary>
		/// <param name="sx">The scale factor for the horizontal axis.</param>
		/// <param name="sx">The scale factor for the vertical axis.</param>
		/// <param name="x">The x position to scale about.</param>
		/// <param name="y">The y position to scale about.</param>
		/// <remarks>Reflections are not propertly supported (i.e. negative scalings) via MonoGame's SpriteBatch, so use SpriteEffects to achieve reflections instead. Negative scalings are not, however, prohibited, as there are other uses for matrices.</remarks>
		public static void Scale(this IAffineComponent2D me, float sx, float sy, float x, float y)
		{
			me.Transform = me.Transform.Scale(sx,sy,x,y);
			return;
		}

		/// <summary>
		/// Applies a scale factor to this component.
		/// This is equivalent to a left multiplication by a scale matrix.
		/// </summary>
		/// <param name="sx">The scale factor for the horizontal axis.</param>
		/// <param name="sx">The scale factor for the vertical axis.</param>
		/// <param name="pos">The position to scale about.</param>
		/// <remarks>Reflections are not propertly supported (i.e. negative scalings) via MonoGame's SpriteBatch, so use SpriteEffects to achieve reflections instead. Negative scalings are not, however, prohibited, as there are other uses for matrices.</remarks>
		public static void Scale(this IAffineComponent2D me, float sx, float sy, Vector2 pos)
		{
			me.Transform = me.Transform.Scale(sx,sy,pos);
			return;
		}

		/// <summary>
		/// Applies a scale factor to this component.
		/// This is equivalent to a left multiplication by a scale matrix.
		/// </summary>
		/// <param name="s">The scale factor for each axis.</param>
		/// <remarks>Reflections are not propertly supported (i.e. negative scalings) via MonoGame's SpriteBatch, so use SpriteEffects to achieve reflections instead. Negative scalings are not, however, prohibited, as there are other uses for matrices.</remarks>
		public static void Scale(this IAffineComponent2D me, Vector2 s)
		{
			me.Transform = me.Transform.Scale(s);
			return;
		}

		/// <summary>
		/// Applies a scale factor to this component.
		/// This is equivalent to a left multiplication by a scale matrix.
		/// </summary>
		/// <param name="s">The scale factor for each axis.</param>
		/// <param name="x">The x position to scale about.</param>
		/// <param name="y">The y position to scale about.</param>
		/// <remarks>Reflections are not propertly supported (i.e. negative scalings) via MonoGame's SpriteBatch, so use SpriteEffects to achieve reflections instead. Negative scalings are not, however, prohibited, as there are other uses for matrices.</remarks>
		public static void Scale(this IAffineComponent2D me, Vector2 s, float x, float y)
		{
			me.Transform = me.Transform.Scale(s,x,y);
			return;
		}

		/// <summary>
		/// Applies a scale factor to this component.
		/// This is equivalent to a left multiplication by a scale matrix.
		/// </summary>
		/// <param name="s">The scale factor for each axis.</param>
		/// <param name="pos">The position to scale about.</param>
		/// <remarks>Reflections are not propertly supported (i.e. negative scalings) via MonoGame's SpriteBatch, so use SpriteEffects to achieve reflections instead. Negative scalings are not, however, prohibited, as there are other uses for matrices.</remarks>
		public static void Scale(this IAffineComponent2D me, Vector2 s, Vector2 pos)
		{
			me.Transform = me.Transform.Scale(s,pos);
			return;
		}

		/// <summary>
		/// Determines what position (0,0) ends up at after being transformed by this affine object's World transform.
		/// </summary>
		/// <param name="me">This affine object.</param>
		/// <returns>Returns <paramref name="me"/>.World * (0,0).</returns>
		public static Vector2 GetAffinePosition(this IAffineComponent2D me) => me.World * Vector2.Zero;
	}
}
