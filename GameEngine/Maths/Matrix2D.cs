using GameEngine.Utility.ExtensionMethods.PrimitiveExtensions;
using Microsoft.Xna.Framework;

namespace GameEngine.Maths
{
	/// <summary>
	/// A dedicated and optimized two dimensional matrix for computer graphics in a plane.
	/// </summary>
	public readonly struct Matrix2D : IEquatable<Matrix2D>
	{
		/// <summary>
		/// Constructs the identity matrix.
		/// </summary>
		public Matrix2D()
		{
			M00 = 1.0f;
			M01 = 0.0f;
			M02 = 0.0f;
			
			M10 = 0.0f;
			M11 = 1.0f;
			M12 = 0.0f;
			
			return;
		}

		/// <summary>
		/// Constructs a matrix with the first two rows specified in row-column order.
		/// The first three parameters are the first row.
		/// The second three parameters are the second row.
		/// </summary>
		private Matrix2D(float m00, float m01, float m02, float m10, float m11, float m12)
		{
			M00 = m00;
			M01 = m01;
			M02 = m02;
			
			M10 = m10;
			M11 = m11;
			M12 = m12;
			
			return;
		}

		/// <summary>
		/// Creates a shallow copy of the immutate matrix <paramref name="m"/>.
		/// </summary>
		private Matrix2D(Matrix2D m)
		{
			M00 = m[0,0];
			M01 = m[0,1];
			M02 = m[0,2];

			M10 = m[1,0];
			M11 = m[1,1];
			M12 = m[1,2];
			
			return;
		}

		/// <summary>
		/// Transforms this into Monogame's three dimensional matrix class.
		/// </summary>
		public static implicit operator Matrix(Matrix2D m)
		{
			// Monogame for some reason places the translations on the bottom row instead of the right hand column
			return new Matrix(m[0,0],m[0,1],0.0f,0.0f,m[1,0],m[1,1],0.0f,0.0f,0.0f,0.0f,1.0f,0.0f,m[0,2],m[1,2],0.0f,1.0f);
		}

		/// <summary>
		/// Decomposes this matrix into its translation, rotation, and scale.
		/// These values can be transformed into an equivalent matrix via the appropriate matrices for translation * scale * rotation.
		/// For a SpriteBatch's Draw call, these parameters correspond to position, -rotation (with origin = Vector2.Zero), and scale.
		/// </summary>
		/// <param name="translation">Where the translation values will be returned.</param>
		/// <param name="rotation">Where the rotation value will be returned.</param>
		/// <param name="scale">Where the <b>unsigned</b> scale values will be returned.</param>
		/// <returns>Returns true if the decomposition is possible and false otherwise. If this returns false, then none of <paramref name="translation"/>, <paramref name="rotation"/>, and <paramref name="scale"/> are valid.</returns>
		/// <remarks>Reflections are not propertly supported (i.e. negative scalings) via MonoGame's SpriteBatch, so use SpriteEffects to achieve reflections instead.</remarks>
		public bool Decompose(out Vector2 translation, out float rotation, out Vector2 scale)
		{
			Matrix m = this;
			bool b = m.Decompose(out Vector3 s,out Quaternion r, out Vector3 t);

			if(!b)
			{
				translation = Vector2.Zero;
				rotation = 0.0f;
				scale = Vector2.Zero;

				return false;
			}

			translation = new Vector2(t.X,t.Y);
			rotation = 2.0f * MathF.Acos(r.W) * MathF.Sign(r.Z);
			scale = new Vector2(s.X,s.Y);
			
			return true;
		}

		/// <summary>
		/// Creates a matrix from a position, rotation, and scale.
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
		public static Matrix2D FromPositionRotationScale(Vector2 position, float rotation, Vector2 scale, Vector2 origin = default(Vector2), bool righthanded_chirality = false)
		{return Matrix2D.Translation(position) * Matrix2D.Rotation(rotation,righthanded_chirality) * Matrix2D.Scaling(scale) * Matrix2D.Translation(-origin);}

		public static bool operator ==(Matrix2D lhs, Matrix2D rhs)
		{return lhs.Equals(rhs);}

		public static bool operator !=(Matrix2D lhs, Matrix2D rhs)
		{return !(lhs == rhs);}

		public override bool Equals(object? obj)
		{return Equals(obj as Matrix2D?);}

		public bool Equals(Matrix2D m)
		{return M00.CloseEnough(m.M00) && M01.CloseEnough(m.M01) && M02.CloseEnough(m.M02) && M10.CloseEnough(m.M10) && M11.CloseEnough(m.M11) && M12.CloseEnough(m.M12);}

		/// <summary>
		/// Calculates the matrix multiplication of <paramref name="lhs"/> times <paramref name="rhs"/> in that order.
		/// </summary>
		/// <param name="lhs">The left hand matrix.</param>
		/// <param name="rhs">The right hand matrix.</param>
		/// <returns>Returns a new matrix containing the result.</returns>
		public static Matrix2D operator *(Matrix2D lhs, Matrix2D rhs)
		{
			float r00 = lhs[0,0] * rhs[0,0] + lhs[0,1] * rhs[1,0];
			float r01 = lhs[0,0] * rhs[0,1] + lhs[0,1] * rhs[1,1];
			float r02 = lhs[0,0] * rhs[0,2] + lhs[0,1] * rhs[1,2] + lhs[0,2];

			float r10 = lhs[1,0] * rhs[0,0] + lhs[1,1] * rhs[1,0];
			float r11 = lhs[1,0] * rhs[0,1] + lhs[1,1] * rhs[1,1];
			float r12 = lhs[1,0] * rhs[0,2] + lhs[1,1] * rhs[1,2] + lhs[1,2];

			return new Matrix2D(r00,r01,r02,r10,r11,r12);
		}

		/// <summary>
		/// Calculates the multiplication of a matrix <paramref name="m"/> times a column vector <paramref name="v"/> in that order.
		/// </summary>
		/// <returns>Returns a new vector containing the multiplication.</returns>
		public static Vector2 operator *(Matrix2D m, Vector2 v)
		{
			float r0 = m[0,0] * v.X + m[0,1] * v.Y + m[0,2];
			float r1 = m[1,0] * v.X + m[1,1] * v.Y + m[1,2];

			return new Vector2(r0,r1);
		}

		/// <summary>
		/// Applies a translation to this matrix.
		/// This is equivalent to a left multiplication by a translation matrix.
		/// </summary>
		/// <param name="tx">The horizontal distance to translate by.</param>
		/// <param name="ty">The vertical distance to translate by.</param>
		/// <returns>Returns a new matrix containing the result.</returns>
		public Matrix2D Translate(float tx, float ty)
		{return Translation(tx,ty) * this;}

		/// <summary>
		/// Applies a translation to this matrix.
		/// This is equivalent to a left multiplication by a translation matrix.
		/// </summary>
		/// <param name="t">The distance to translate by.</param>
		/// <returns>Returns a new matrix containing the result.</returns>
		public Matrix2D Translate(Vector2 t)
		{return Translate(t.X,t.Y);}

		/// <summary>
		/// Creates a new translation matrix.
		/// </summary>
		/// <param name="tx">The horizontal distance to translate by.</param>
		/// <param name="ty">The vertical distance to translate by.</param>
		public static Matrix2D Translation(float tx, float ty)
		{return new Matrix2D(1.0f,0.0f,tx,0.0f,1.0f,ty);}

		/// <summary>
		/// Creates a new translation matrix.
		/// </summary>
		/// <param name="t">The distance to translate by.</param>
		public static Matrix2D Translation(Vector2 t)
		{return new Matrix2D().Translate(t);}

		/// <summary>
		/// Applies a rotation to this matrix.
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
		public Matrix2D Rotate(float angle, bool righthanded_chirality = false)
		{return Rotation(angle,righthanded_chirality) * this;}

		/// <summary>
		/// Applies a rotation to this matrix about a point.
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
		public Matrix2D Rotate(float angle, Vector2 point, bool righthanded_chirality = false)
		{return Translation(point) * Rotation(angle,righthanded_chirality) * Translation(-point) * this;}

		/// <summary>
		/// Applies a rotation to this matrix about a point.
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
		public Matrix2D Rotate(float angle, float x, float y, bool righthanded_chirality = false)
		{return Translation(x,y) * Rotation(angle,righthanded_chirality) * Translation(-x,-y) * this;}

		/// <summary>
		/// Creates a new rotation matrix.
		/// </summary>
		/// <param name="angle">The rotation angle (in radians).</param>
		/// <param name="righthanded_chirality">
		///	Positive rotation values result in a counterclockwise rotation about its axis.
		///	Monogame's SpriteBatch Draw (but not Begin) z-axis points outward (left-handed), resulting in screen-counterclockwise rotations with positive values.
		///	The rotation matrix this function generates follows this scheme when this value is false.
		///	If this is true, it will produce the opposite result (a right-handed rotation matrix with the z-axis pointing inward).
		/// </param>
		public static Matrix2D Rotation(float angle, bool righthanded_chirality = false)
		{
			float cos = MathF.Cos(angle);
			float sin = MathF.Sin(angle);

			if(righthanded_chirality)
				return new Matrix2D(cos,-sin,0.0f,sin,cos,0.0f);

			return new Matrix2D(cos,sin,0.0f,-sin,cos,0.0f);
		}

		/// <summary>
		/// Creates a new rotation matrix about a point.
		/// </summary>
		/// <param name="angle">The rotation angle (in radians).</param>
		/// <param name="point">The point to rotate around.</param>
		/// <param name="righthanded_chirality">
		///	Positive rotation values result in a counterclockwise rotation about its axis.
		///	Monogame's SpriteBatch Draw (but not Begin) z-axis points outward (left-handed), resulting in screen-counterclockwise rotations with positive values.
		///	The rotation matrix this function generates follows this scheme when this value is false.
		///	If this is true, it will produce the opposite result (a right-handed rotation matrix with the z-axis pointing inward).
		/// </param>
		public static Matrix2D Rotation(float angle, Vector2 point, bool righthanded_chirality = false)
		{return Translation(point) * Rotation(angle,righthanded_chirality) * Translation(-point);}

		/// <summary>
		/// Creates a new rotation matrix about a point.
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
		public static Matrix2D Rotation(float angle, float x, float y, bool righthanded_chirality = false)
		{return Translation(x,y) * Rotation(angle,righthanded_chirality) * Translation(-x,-y);}

		/// <summary>
		/// Applies a clean 90 degree rotation <paramref name="times"/> times to this matrix.
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
		public Matrix2D Rotate90(int times, bool righthanded_chirality = false)
		{return Rotation90(times,righthanded_chirality) * this;}

		/// <summary>
		/// Applies a clean 90 degree rotation <paramref name="times"/> times to this matrix about a point.
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
		public Matrix2D Rotate90(int times, Vector2 point, bool righthanded_chirality = false)
		{return Translation(point) * Rotation90(times,righthanded_chirality) * Translation(-point) * this;}

		/// <summary>
		/// Applies a clean 90 degree rotation <paramref name="times"/> times to this matrix about a point.
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
		/// <returns>Returns a new matrix containing the result.</returns>
		public Matrix2D Rotate90(int times, float x, float y, bool righthanded_chirality = false)
		{return Translation(x,y) * Rotation90(times,righthanded_chirality) * Translation(-x,-y) * this;}

		/// <summary>
		/// Creates a new rotation matrix.
		/// This will apply a clean 90 degree rotation <paramref name="times"/> times.
		/// This is done in a single multiplication.
		/// </summary>
		/// <param name="times">The number of 90 degree clockwise rotations to perform.</param>
		/// <param name="righthanded_chirality">
		///	Positive rotation values result in a counterclockwise rotation about its axis.
		///	Monogame's SpriteBatch Draw (but not Begin) z-axis points outward (left-handed), resulting in screen-counterclockwise rotations with positive values.
		///	The rotation matrix this function generates follows this scheme when this value is false.
		///	If this is true, it will produce the opposite result (a right-handed rotation matrix with the z-axis pointing inward).
		/// </param>
		public static Matrix2D Rotation90(int times, bool righthanded_chirality = false)
		{
			times &= 0b11; // We only need the last two bits (this also allows us to ignore sign)

			float cos = 0.0f;
			float sin = 0.0f;

			switch(times)
			{
			case 0:
				cos = 1.0f;
				sin = 0.0f;

				break;
			case 1:
				cos = 0.0f;
				sin = 1.0f;

				break;
			case 2:
				cos = -1.0f;
				sin = 0.0f;

				break;
			case 3:
				cos = 0.0f;
				sin = -1.0f;

				break;
			}

			if(righthanded_chirality)
				return new Matrix2D(cos,-sin,0.0f,sin,cos,0.0f);
			
			return new Matrix2D(cos,sin,0.0f,-sin,cos,0.0f);
		}

		/// <summary>
		/// Creates a new rotation matrix about a point.
		/// This will apply a clean 90 degree rotation <paramref name="times"/> times about a point.
		/// The rotation is done in a single multiplication.
		/// </summary>
		/// <param name="times">The number of 90 degree clockwise rotations to perform.</param>
		/// <param name="point">The point to rotate around.</param>
		/// <param name="righthanded_chirality">
		///	Positive rotation values result in a counterclockwise rotation about its axis.
		///	Monogame's SpriteBatch Draw (but not Begin) z-axis points outward (left-handed), resulting in screen-counterclockwise rotations with positive values.
		///	The rotation matrix this function generates follows this scheme when this value is false.
		///	If this is true, it will produce the opposite result (a right-handed rotation matrix with the z-axis pointing inward).
		/// </param>
		public static Matrix2D Rotation90(int times, Vector2 point, bool righthanded_chirality = false)
		{return Translation(point) * Rotation90(times,righthanded_chirality) * Translation(-point);}

		/// <summary>
		/// Creates a new rotation matrix about a point.
		/// This will apply a clean 90 degree rotation <paramref name="times"/> times about a point.
		/// The rotation is done in a single multiplication.
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
		public static Matrix2D Rotation90(int times, float x, float y, bool righthanded_chirality = false)
		{return Translation(x,y) * Rotation90(times,righthanded_chirality) * Translation(-x,-y);}

		/// <summary>
		/// Applies a scale factor to this matrix.
		/// This is equivalent to a left multiplication by a scale matrix.
		/// </summary>
		/// <param name="s">The uniform scale factor along both axes.</param>
		/// <returns>Returns a new matrix containing the result.</returns>
		/// <remarks>Reflections are not propertly supported (i.e. negative scalings) via MonoGame's SpriteBatch, so use SpriteEffects to achieve reflections instead. Negative scalings are not, however, prohibited, as there are other uses for matrices.</remarks>
		public Matrix2D Scale(float s)
		{return Scale(s,s);}

		/// <summary>
		/// Applies a scale factor to this matrix.
		/// This is equivalent to a left multiplication by a scale matrix.
		/// </summary>
		/// <param name="s">The uniform scale factor along both axes.</param>
		/// <param name="x">The x position to scale about.</param>
		/// <param name="y">The y position to scale about.</param>
		/// <returns>Returns a new matrix containing the result.</returns>
		/// <remarks>Reflections are not propertly supported (i.e. negative scalings) via MonoGame's SpriteBatch, so use SpriteEffects to achieve reflections instead. Negative scalings are not, however, prohibited, as there are other uses for matrices.</remarks>
		public Matrix2D Scale(float s, float x, float y)
		{return Scale(s,s,x,y);}

		/// <summary>
		/// Applies a scale factor to this matrix.
		/// This is equivalent to a left multiplication by a scale matrix.
		/// </summary>
		/// <param name="s">The uniform scale factor along both axes.</param>
		/// <param name="pos">The position to scale about.</param>
		/// <returns>Returns a new matrix containing the result.</returns>
		/// <remarks>Reflections are not propertly supported (i.e. negative scalings) via MonoGame's SpriteBatch, so use SpriteEffects to achieve reflections instead. Negative scalings are not, however, prohibited, as there are other uses for matrices.</remarks>
		public Matrix2D Scale(float s, Vector2 pos)
		{return Scale(s,s,pos.X,pos.Y);}

		/// <summary>
		/// Applies a scale factor to this matrix.
		/// This is equivalent to a left multiplication by a scale matrix.
		/// </summary>
		/// <param name="sx">The scale factor for the horizontal axis.</param>
		/// <param name="sx">The scale factor for the vertical axis.</param>
		/// <returns>Returns a new matrix containing the result.</returns>
		/// <remarks>Reflections are not propertly supported (i.e. negative scalings) via MonoGame's SpriteBatch, so use SpriteEffects to achieve reflections instead. Negative scalings are not, however, prohibited, as there are other uses for matrices.</remarks>
		public Matrix2D Scale(float sx, float sy)
		{return Scaling(sx,sy) * this;}

		/// <summary>
		/// Applies a scale factor to this matrix.
		/// This is equivalent to a left multiplication by a scale matrix.
		/// </summary>
		/// <param name="sx">The scale factor for the horizontal axis.</param>
		/// <param name="sx">The scale factor for the vertical axis.</param>
		/// <param name="x">The x position to scale about.</param>
		/// <param name="y">The y position to scale about.</param>
		/// <returns>Returns a new matrix containing the result.</returns>
		/// <remarks>Reflections are not propertly supported (i.e. negative scalings) via MonoGame's SpriteBatch, so use SpriteEffects to achieve reflections instead. Negative scalings are not, however, prohibited, as there are other uses for matrices.</remarks>
		public Matrix2D Scale(float sx, float sy, float x, float y)
		{return Translation(x,y) * Scaling(sx,sy) * Translation(-x,-y) * this;}

		/// <summary>
		/// Applies a scale factor to this matrix.
		/// This is equivalent to a left multiplication by a scale matrix.
		/// </summary>
		/// <param name="sx">The scale factor for the horizontal axis.</param>
		/// <param name="sx">The scale factor for the vertical axis.</param>
		/// <param name="pos">The position to scale about.</param>
		/// <returns>Returns a new matrix containing the result.</returns>
		/// <remarks>Reflections are not propertly supported (i.e. negative scalings) via MonoGame's SpriteBatch, so use SpriteEffects to achieve reflections instead. Negative scalings are not, however, prohibited, as there are other uses for matrices.</remarks>
		public Matrix2D Scale(float sx, float sy, Vector2 pos)
		{return Translation(pos.X,pos.Y) * Scaling(sx,sy) * Translation(-pos.X,-pos.Y) * this;}

		/// <summary>
		/// Applies a scale factor to this matrix.
		/// This is equivalent to a left multiplication by a scale matrix.
		/// </summary>
		/// <param name="s">The scale factor for each axis.</param>
		/// <returns>Returns a new matrix containing the result.</returns>
		/// <remarks>Reflections are not propertly supported (i.e. negative scalings) via MonoGame's SpriteBatch, so use SpriteEffects to achieve reflections instead. Negative scalings are not, however, prohibited, as there are other uses for matrices.</remarks>
		public Matrix2D Scale(Vector2 s)
		{return Scale(s.X,s.Y);}

		/// <summary>
		/// Applies a scale factor to this matrix.
		/// This is equivalent to a left multiplication by a scale matrix.
		/// </summary>
		/// <param name="s">The scale factor for each axis.</param>
		/// <param name="x">The x position to scale about.</param>
		/// <param name="y">The y position to scale about.</param>
		/// <returns>Returns a new matrix containing the result.</returns>
		/// <remarks>Reflections are not propertly supported (i.e. negative scalings) via MonoGame's SpriteBatch, so use SpriteEffects to achieve reflections instead. Negative scalings are not, however, prohibited, as there are other uses for matrices.</remarks>
		public Matrix2D Scale(Vector2 s, float x, float y)
		{return Scale(s.X,s.Y,x,y);}

		/// <summary>
		/// Applies a scale factor to this matrix.
		/// This is equivalent to a left multiplication by a scale matrix.
		/// </summary>
		/// <param name="s">The scale factor for each axis.</param>
		/// <param name="pos">The position to scale about.</param>
		/// <returns>Returns a new matrix containing the result.</returns>
		/// <remarks>Reflections are not propertly supported (i.e. negative scalings) via MonoGame's SpriteBatch, so use SpriteEffects to achieve reflections instead. Negative scalings are not, however, prohibited, as there are other uses for matrices.</remarks>
		public Matrix2D Scale(Vector2 s, Vector2 pos)
		{return Scale(s.X,s.Y,pos.X,pos.Y);}

		/// <summary>
		/// Creates a new scale matrix.
		/// </summary>
		/// <param name="s">The uniform scale factor along both axes.</param>
		/// <remarks>Reflections are not propertly supported (i.e. negative scalings) via MonoGame's SpriteBatch, so use SpriteEffects to achieve reflections instead. Negative scalings are not, however, prohibited, as there are other uses for matrices.</remarks>
		public static Matrix2D Scaling(float s)
		{return Scaling(s,s);}

		/// <summary>
		/// Creates a new scale matrix.
		/// </summary>
		/// <param name="s">The uniform scale factor along both axes.</param>
		/// <param name="x">The x position to scale about.</param>
		/// <param name="y">The y position to scale about.</param>
		/// <remarks>Reflections are not propertly supported (i.e. negative scalings) via MonoGame's SpriteBatch, so use SpriteEffects to achieve reflections instead. Negative scalings are not, however, prohibited, as there are other uses for matrices.</remarks>
		public static Matrix2D Scaling(float s, float x, float y)
		{return Scaling(s,s,x,y);}

		/// <summary>
		/// Creates a new scale matrix.
		/// </summary>
		/// <param name="s">The uniform scale factor along both axes.</param>
		/// <param name="pos">The position to scale about.</param>
		/// <remarks>Reflections are not propertly supported (i.e. negative scalings) via MonoGame's SpriteBatch, so use SpriteEffects to achieve reflections instead. Negative scalings are not, however, prohibited, as there are other uses for matrices.</remarks>
		public static Matrix2D Scaling(float s, Vector2 pos)
		{return Scaling(s,s,pos.X,pos.Y);}

		/// <summary>
		/// Creates a new scale matrix.
		/// </summary>
		/// <param name="sx">The scale factor for the horizontal axis.</param>
		/// <param name="sx">The scale factor for the vertical axis.</param>
		/// <remarks>Reflections are not propertly supported (i.e. negative scalings) via MonoGame's SpriteBatch, so use SpriteEffects to achieve reflections instead. Negative scalings are not, however, prohibited, as there are other uses for matrices.</remarks>
		public static Matrix2D Scaling(float sx, float sy)
		{return new Matrix2D(sx,0.0f,0.0f,0.0f,sy,0.0f);}

		/// <summary>
		/// Creates a new scale matrix.
		/// </summary>
		/// <param name="sx">The scale factor for the horizontal axis.</param>
		/// <param name="sx">The scale factor for the vertical axis.</param>
		/// <param name="x">The x position to scale about.</param>
		/// <param name="y">The y position to scale about.</param>
		/// <remarks>Reflections are not propertly supported (i.e. negative scalings) via MonoGame's SpriteBatch, so use SpriteEffects to achieve reflections instead. Negative scalings are not, however, prohibited, as there are other uses for matrices.</remarks>
		public static Matrix2D Scaling(float sx, float sy, float x, float y)
		{return Translation(x,y) * new Matrix2D(sx,0.0f,0.0f,0.0f,sy,0.0f) * Translation(-x,-y);}

		/// <summary>
		/// Creates a new scale matrix.
		/// </summary>
		/// <param name="sx">The scale factor for the horizontal axis.</param>
		/// <param name="sx">The scale factor for the vertical axis.</param>
		/// <param name="pos">The position to scale about.</param>
		/// <remarks>Reflections are not propertly supported (i.e. negative scalings) via MonoGame's SpriteBatch, so use SpriteEffects to achieve reflections instead. Negative scalings are not, however, prohibited, as there are other uses for matrices.</remarks>
		public static Matrix2D Scaling(float sx, float sy, Vector2 pos)
		{return Scaling(sx,sy,pos.X,pos.Y);}

		/// <summary>
		/// Creates a new scale matrix.
		/// </summary>
		/// <param name="s">The scale factor for each axis.</param>
		/// <remarks>Reflections are not propertly supported (i.e. negative scalings) via MonoGame's SpriteBatch, so use SpriteEffects to achieve reflections instead. Negative scalings are not, however, prohibited, as there are other uses for matrices.</remarks>
		public static Matrix2D Scaling(Vector2 s)
		{return Scaling(s.X,s.Y);}

		/// <summary>
		/// Creates a new scale matrix.
		/// </summary>
		/// <param name="s">The scale factor for each axis.</param>
		/// <param name="x">The x position to scale about.</param>
		/// <param name="y">The y position to scale about.</param>
		/// <remarks>Reflections are not propertly supported (i.e. negative scalings) via MonoGame's SpriteBatch, so use SpriteEffects to achieve reflections instead. Negative scalings are not, however, prohibited, as there are other uses for matrices.</remarks>
		public static Matrix2D Scaling(Vector2 s, float x, float y)
		{return Scaling(s.X,s.Y,x,y);}

		/// <summary>
		/// Creates a new scale matrix.
		/// </summary>
		/// <param name="s">The scale factor for each axis.</param>
		/// <param name="pos">The position to scale about.</param>
		/// <remarks>Reflections are not propertly supported (i.e. negative scalings) via MonoGame's SpriteBatch, so use SpriteEffects to achieve reflections instead. Negative scalings are not, however, prohibited, as there are other uses for matrices.</remarks>
		public static Matrix2D Scaling(Vector2 s, Vector2 pos)
		{return Scaling(s.X,s.Y,pos.X,pos.Y);}

		/// <summary>
		/// Applies a horizontal reflection to this matrix.
		/// This is equivalent to a left multiplication by a (-1,1) scale matrix.
		/// </summary>
		/// <returns>Returns a new matrix containing the result.</returns>
		/// <remarks>Reflections are not propertly supported (i.e. negative scalings) via MonoGame's SpriteBatch, so use SpriteEffects to achieve reflections instead. Reflections are not, however, prohibited, as there are other uses for matrices.</remarks>
		public Matrix2D ReflectHorizontal()
		{return HorizontalReflection() * this;}

		/// <summary>
		/// Creates a new horizontal reflection.
		/// </summary>
		/// <remarks>Reflections are not propertly supported (i.e. negative scalings) via MonoGame's SpriteBatch, so use SpriteEffects to achieve reflections instead. Reflections are not, however, prohibited, as there are other uses for matrices.</remarks>
		public static Matrix2D HorizontalReflection()
		{return Scaling(-1.0f,1.0f);}

		/// <summary>
		/// Applies a vertical reflection to this matrix.
		/// This is equivalent to a left multiplication by a (1,-1) scale matrix.
		/// </summary>
		/// <returns>Returns a new matrix containing the result.</returns>
		/// <remarks>Reflections are not propertly supported (i.e. negative scalings) via MonoGame's SpriteBatch, so use SpriteEffects to achieve reflections instead. Reflections are not, however, prohibited, as there are other uses for matrices.</remarks>
		public Matrix2D ReflectVertical()
		{return VerticalReflection() * this;}

		/// <summary>
		/// Creates a new vertical reflection.
		/// </summary>
		/// <remarks>Reflections are not propertly supported (i.e. negative scalings) via MonoGame's SpriteBatch, so use SpriteEffects to achieve reflections instead. Reflections are not, however, prohibited, as there are other uses for matrices.</remarks>
		public static Matrix2D VerticalReflection()
		{return Scaling(1.0f,-1.0f);}

		/// <summary>
		/// Calculates the inverse of this matrix.
		/// </summary>
		/// <returns>Returns a new matrix containing the result.</returns>
		public Matrix2D Invert()
		{
			float det = InnerDeterminant;
			return new Matrix2D(det * M11,-det * M01,det * (M01 * M12 - M02 * M11),-det * M10,det * M00,det * (M02 * M10 - M00 * M12));
		}

		/// <summary>
		/// Calculates the equivalent matrix of this with the opposite chirality.
		/// </summary>
		/// <returns>Returns a new matrix containing the result./returns>
		public Matrix2D SwapChirality()
		{
			if(!Decompose(out Vector2 t,out float r,out Vector2 s))
				throw new ArgumentException("Cannot swap chirality of a matrix that cannot be decomposed.");

			return Translation(t) * Scaling(s) * Rotation(r,true);
		}

		public override string ToString()
		{
			string m00 = M00.ToString();
			string m01 = M01.ToString();
			string m02 = M02.ToString();

			string m10 = M10.ToString();
			string m11 = M11.ToString();
			string m12 = M12.ToString();

			string m20 = "0";
			string m21 = "0";
			string m22 = "1";

			int longest0 = Math.Max(Math.Max(m00.Length,m10.Length),m20.Length);
			int longest1 = Math.Max(Math.Max(m01.Length,m11.Length),m21.Length);
			int longest2 = Math.Max(Math.Max(m02.Length,m12.Length),m22.Length);

			string blank0 = "";
			string blank1 = "";
			string blank2 = "";
			
			for(int i = 0;i < longest0;i++)
				blank0 += " ";
			
			for(int i = 0;i < longest1;i++)
				blank1 += " ";
			
			for(int i = 0;i < longest2;i++)
				blank2 += " ";
			
			string ret = " _ " + blank0 + " " + blank1 + " " + blank2 + " _ \n";
			ret += "|  " + Pad(m00,longest0) + " " + Pad(m01,longest1) + " " + Pad(m02,longest2) + "  |\n";
			ret += "|  " + Pad(m10,longest0) + " " + Pad(m11,longest1) + " " + Pad(m12,longest2) + "  |\n";
			ret += "|_ " + Pad(m20,longest0) + " " + Pad(m21,longest1) + " " + Pad(m22,longest2) + " _|\n";
			
			return ret;
		}

		public override int GetHashCode()
		{return HashCode.Combine(M00,M01,M02,M10,M11,M12);}
		
		/// <summary>
		/// Pads <paramref name="str"/> with front spaces until it has (at least) length <paramref name="len"/>.
		/// </summary>
		/// <param name="str">The string to pad.</param>
		/// <param name="len">The length to attain.</param>
		/// <returns>Returns <paramref name="str"/> padded with spaces until it has least at least <paramref name="len"/>.</returns>
		private string Pad(string str, int len)
		{
			while(str.Length < len)
				str = " " + str;
			
			return str;
		}

		/// <summary>
		/// Gets the entry of this matrix the specified row and column.
		/// </summary>
		/// <param name="row">The row to access. This value should be between 0 and 1 for meaningful values. The last row, 2, of any 3x3 matrix will always be (0,0,1).</param>
		/// <param name="column">The column to access. This value should be between 0 and 2.</param>
		/// <returns>Returns the row-column entry of this matrix.</returns>
		/// <exception cref="IndexOutOfRangeException">Thrown if <paramref name="row"/> or <paramref name="column"/> is out of bounds.</exception>
		public float this[int row, int column]
		{
			get
			{
				switch((row << 2) + column)
				{
				case 0:
					return M00;
				case 1:
					return M01;
				case 2:
					return M02;
				case 4:
					return M10;
				case 5:
					return M11;
				case 6:
					return M12;
				case 8:
				case 9:
					return 0.0f;
				case 10:
					return 1.0f;
				default:
					throw new IndexOutOfRangeException();
				}

				throw new IndexOutOfRangeException();
			}
		}

		/// <summary>
		/// A matrix entry.
		/// They are specified by row first and then column.
		/// </summary>
		private readonly float M00;

		/// <summary>
		/// A matrix entry.
		/// They are specified by row first and then column.
		/// </summary>
		private readonly float M01;

		/// <summary>
		/// A matrix entry.
		/// They are specified by row first and then column.
		/// </summary>
		private readonly float M02;

		/// <summary>
		/// A matrix entry.
		/// They are specified by row first and then column.
		/// </summary>
		private readonly float M10;

		/// <summary>
		/// A matrix entry.
		/// They are specified by row first and then column.
		/// </summary>
		private readonly float M11;

		/// <summary>
		/// A matrix entry.
		/// They are specified by row first and then column.
		/// </summary>
		private readonly float M12;

		/// <summary>
		/// The determinant of the inner 2x2 matrix.
		/// </summary>
		public float InnerDeterminant => 1.0f / (M00 * M11 - M01 * M10); // Affine transformations are always invertable (with the exception of dumb things like scaling by 0), so we generally don't need to worry about the determinant being 0

		/// <summary>
		/// The identity matrix.
		/// </summary>
		public static readonly Matrix2D Identity = new Matrix2D();
	}
}