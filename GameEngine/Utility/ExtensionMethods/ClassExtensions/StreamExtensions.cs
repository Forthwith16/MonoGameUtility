using System.Buffers.Binary;
using System.Text;

namespace GameEngine.Utility.ExtensionMethods.ClassExtensions
{
	/// <summary>
	/// Provides extension methods to Streams.
	/// </summary>
	public static class StreamExtensions
	{
		#region BOOLEANS
		/// <summary>
		/// Reads a bool from <paramref name="sin"/>.
		/// It is assumed that <paramref name="sin"/> is valid and ready to go.
		/// </summary>
		/// <param name="sin">The stream to read from.</param>
		/// <returns>Returns the bool read from <paramref name="sin"/>.</returns>
		/// <exception cref="IOException">Thrown if <paramref name="sin"/> runs out of input early or for other mysterious reasons.</exception>
		public static bool ReadBoolean(this Stream sin)
		{return ReadByte(sin) != 0;}

		/// <summary>
		/// Writes a bool to <paramref name="sout"/>.
		/// It is assumed that <paramref name="sout"/> is valid and ready to go.
		/// </summary>
		/// <param name="sout">The stream to write to.</param>
		/// <param name="b">The bool to write.</param>
		/// <exception cref="IOException">Thrown when an IO exception occurs for mysterious reasons.</exception>
		public static void WriteBoolean(this Stream sout, bool b)
		{
			WriteByte(sout,b ? (byte)1 : (byte)0);
			return;
		}
		#endregion

		#region PACKED BOOLEANS
		/// <summary>
		/// Reads up to eight bools from <paramref name="sin"/>.
		/// It is assumed that <paramref name="sin"/> is valid and ready to go.
		/// </summary>
		/// <param name="sin">The stream to read from.</param>
		/// <returns>Returns the bools read from <paramref name="sin"/>.</returns>
		/// <exception cref="IOException">Thrown if <paramref name="sin"/> runs out of input early or for other mysterious reasons.</exception>
		public static PackedBooleanBits ReadBooleans(this Stream sin)
		{return (PackedBooleanBits)ReadByte(sin);}

		/// <summary>
		/// Reads a bool from a packed set of bools from <paramref name="sin"/>.
		/// It is assumed that <paramref name="sin"/> is valid and ready to go.
		/// </summary>
		/// <param name="sin">The stream to read from.</param>
		/// <param name="flag">The boolean bit we want to access. The algorithm skips error checking and immediately checks if (flag & read) != NO_BITS. Use/abuse this at your own peril.</param>
		/// <returns>Returns the bool read from <paramref name="sin"/>.</returns>
		/// <exception cref="IOException">Thrown if <paramref name="sin"/> runs out of input early or for other mysterious reasons.</exception>
		public static bool ReadBoolean(this Stream sin, PackedBooleanBits flag)
		{return ((PackedBooleanBits)ReadByte(sin) & flag) != PackedBooleanBits.NO_BITS;}

		#region UTILITY READS
		/// <summary>
		/// Reads up to eight bools from <paramref name="sin"/>.
		/// It is assumed that <paramref name="sin"/> is valid and ready to go.
		/// </summary>
		/// <param name="sin">The stream to read from.</param>
		/// <param name="b0">The 0th bool in the packed booleans.</param>
		/// <exception cref="IOException">Thrown if <paramref name="sin"/> runs out of input early or for other mysterious reasons.</exception>
		public static void ReadBooleans(this Stream sin, out bool b0)
		{
			PackedBooleanBits val = (PackedBooleanBits)ReadByte(sin);

			b0 = (val & PackedBooleanBits.BIT0_BOOLEAN) != PackedBooleanBits.NO_BITS;

			return;
		}

		/// <summary>
		/// Reads up to eight bools from <paramref name="sin"/>.
		/// It is assumed that <paramref name="sin"/> is valid and ready to go.
		/// </summary>
		/// <param name="sin">The stream to read from.</param>
		/// <param name="b0">The 0th bool in the packed booleans.</param>
		/// <param name="b1">The 1st bool in the packed booleans.</param>
		/// <exception cref="IOException">Thrown if <paramref name="sin"/> runs out of input early or for other mysterious reasons.</exception>
		public static void ReadBooleans(this Stream sin, out bool b0, out bool b1)
		{
			PackedBooleanBits val = (PackedBooleanBits)ReadByte(sin);

			b0 = (val & PackedBooleanBits.BIT0_BOOLEAN) != PackedBooleanBits.NO_BITS;
			b1 = (val & PackedBooleanBits.BIT1_BOOLEAN) != PackedBooleanBits.NO_BITS;

			return;
		}

		/// <summary>
		/// Reads up to eight bools from <paramref name="sin"/>.
		/// It is assumed that <paramref name="sin"/> is valid and ready to go.
		/// </summary>
		/// <param name="sin">The stream to read from.</param>
		/// <param name="b0">The 0th bool in the packed booleans.</param>
		/// <param name="b1">The 1st bool in the packed booleans.</param>
		/// <param name="b2">The 2nd bool in the packed booleans.</param>
		/// <exception cref="IOException">Thrown if <paramref name="sin"/> runs out of input early or for other mysterious reasons.</exception>
		public static void ReadBooleans(this Stream sin, out bool b0, out bool b1, out bool b2)
		{
			PackedBooleanBits val = (PackedBooleanBits)ReadByte(sin);

			b0 = (val & PackedBooleanBits.BIT0_BOOLEAN) != PackedBooleanBits.NO_BITS;
			b1 = (val & PackedBooleanBits.BIT1_BOOLEAN) != PackedBooleanBits.NO_BITS;
			b2 = (val & PackedBooleanBits.BIT2_BOOLEAN) != PackedBooleanBits.NO_BITS;

			return;
		}

		/// <summary>
		/// Reads up to eight bools from <paramref name="sin"/>.
		/// It is assumed that <paramref name="sin"/> is valid and ready to go.
		/// </summary>
		/// <param name="sin">The stream to read from.</param>
		/// <param name="b0">The 0th bool in the packed booleans.</param>
		/// <param name="b1">The 1st bool in the packed booleans.</param>
		/// <param name="b2">The 2nd bool in the packed booleans.</param>
		/// <param name="b3">The 3rd bool in the packed booleans.</param>
		/// <exception cref="IOException">Thrown if <paramref name="sin"/> runs out of input early or for other mysterious reasons.</exception>
		public static void ReadBooleans(this Stream sin, out bool b0, out bool b1, out bool b2, out bool b3)
		{
			PackedBooleanBits val = (PackedBooleanBits)ReadByte(sin);

			b0 = (val & PackedBooleanBits.BIT0_BOOLEAN) != PackedBooleanBits.NO_BITS;
			b1 = (val & PackedBooleanBits.BIT1_BOOLEAN) != PackedBooleanBits.NO_BITS;
			b2 = (val & PackedBooleanBits.BIT2_BOOLEAN) != PackedBooleanBits.NO_BITS;
			b3 = (val & PackedBooleanBits.BIT3_BOOLEAN) != PackedBooleanBits.NO_BITS;

			return;
		}

		/// <summary>
		/// Reads up to eight bools from <paramref name="sin"/>.
		/// It is assumed that <paramref name="sin"/> is valid and ready to go.
		/// </summary>
		/// <param name="sin">The stream to read from.</param>
		/// <param name="b0">The 0th bool in the packed booleans.</param>
		/// <param name="b1">The 1st bool in the packed booleans.</param>
		/// <param name="b2">The 2nd bool in the packed booleans.</param>
		/// <param name="b3">The 3rd bool in the packed booleans.</param>
		/// <param name="b4">The 4th bool in the packed booleans.</param>
		/// <exception cref="IOException">Thrown if <paramref name="sin"/> runs out of input early or for other mysterious reasons.</exception>
		public static void ReadBooleans(this Stream sin, out bool b0, out bool b1, out bool b2, out bool b3, out bool b4)
		{
			PackedBooleanBits val = (PackedBooleanBits)ReadByte(sin);

			b0 = (val & PackedBooleanBits.BIT0_BOOLEAN) != PackedBooleanBits.NO_BITS;
			b1 = (val & PackedBooleanBits.BIT1_BOOLEAN) != PackedBooleanBits.NO_BITS;
			b2 = (val & PackedBooleanBits.BIT2_BOOLEAN) != PackedBooleanBits.NO_BITS;
			b3 = (val & PackedBooleanBits.BIT3_BOOLEAN) != PackedBooleanBits.NO_BITS;
			b4 = (val & PackedBooleanBits.BIT4_BOOLEAN) != PackedBooleanBits.NO_BITS;

			return;
		}

		/// <summary>
		/// Reads up to eight bools from <paramref name="sin"/>.
		/// It is assumed that <paramref name="sin"/> is valid and ready to go.
		/// </summary>
		/// <param name="sin">The stream to read from.</param>
		/// <param name="b0">The 0th bool in the packed booleans.</param>
		/// <param name="b1">The 1st bool in the packed booleans.</param>
		/// <param name="b2">The 2nd bool in the packed booleans.</param>
		/// <param name="b3">The 3rd bool in the packed booleans.</param>
		/// <param name="b4">The 4th bool in the packed booleans.</param>
		/// <param name="b5">The 5th bool in the packed booleans.</param>
		/// <exception cref="IOException">Thrown if <paramref name="sin"/> runs out of input early or for other mysterious reasons.</exception>
		public static void ReadBooleans(this Stream sin, out bool b0, out bool b1, out bool b2, out bool b3, out bool b4, out bool b5)
		{
			PackedBooleanBits val = (PackedBooleanBits)ReadByte(sin);

			b0 = (val & PackedBooleanBits.BIT0_BOOLEAN) != PackedBooleanBits.NO_BITS;
			b1 = (val & PackedBooleanBits.BIT1_BOOLEAN) != PackedBooleanBits.NO_BITS;
			b2 = (val & PackedBooleanBits.BIT2_BOOLEAN) != PackedBooleanBits.NO_BITS;
			b3 = (val & PackedBooleanBits.BIT3_BOOLEAN) != PackedBooleanBits.NO_BITS;
			b4 = (val & PackedBooleanBits.BIT4_BOOLEAN) != PackedBooleanBits.NO_BITS;
			b5 = (val & PackedBooleanBits.BIT5_BOOLEAN) != PackedBooleanBits.NO_BITS;

			return;
		}

		/// <summary>
		/// Reads up to eight bools from <paramref name="sin"/>.
		/// It is assumed that <paramref name="sin"/> is valid and ready to go.
		/// </summary>
		/// <param name="sin">The stream to read from.</param>
		/// <param name="b0">The 0th bool in the packed booleans.</param>
		/// <param name="b1">The 1st bool in the packed booleans.</param>
		/// <param name="b2">The 2nd bool in the packed booleans.</param>
		/// <param name="b3">The 3rd bool in the packed booleans.</param>
		/// <param name="b4">The 4th bool in the packed booleans.</param>
		/// <param name="b5">The 5th bool in the packed booleans.</param>
		/// <param name="b6">The 6th bool in the packed booleans.</param>
		/// <exception cref="IOException">Thrown if <paramref name="sin"/> runs out of input early or for other mysterious reasons.</exception>
		public static void ReadBooleans(this Stream sin, out bool b0, out bool b1, out bool b2, out bool b3, out bool b4, out bool b5, out bool b6)
		{
			PackedBooleanBits val = (PackedBooleanBits)ReadByte(sin);

			b0 = (val & PackedBooleanBits.BIT0_BOOLEAN) != PackedBooleanBits.NO_BITS;
			b1 = (val & PackedBooleanBits.BIT1_BOOLEAN) != PackedBooleanBits.NO_BITS;
			b2 = (val & PackedBooleanBits.BIT2_BOOLEAN) != PackedBooleanBits.NO_BITS;
			b3 = (val & PackedBooleanBits.BIT3_BOOLEAN) != PackedBooleanBits.NO_BITS;
			b4 = (val & PackedBooleanBits.BIT4_BOOLEAN) != PackedBooleanBits.NO_BITS;
			b5 = (val & PackedBooleanBits.BIT5_BOOLEAN) != PackedBooleanBits.NO_BITS;
			b6 = (val & PackedBooleanBits.BIT6_BOOLEAN) != PackedBooleanBits.NO_BITS;

			return;
		}

		/// <summary>
		/// Reads up to eight bools from <paramref name="sin"/>.
		/// It is assumed that <paramref name="sin"/> is valid and ready to go.
		/// </summary>
		/// <param name="sin">The stream to read from.</param>
		/// <param name="b0">The 0th bool in the packed booleans.</param>
		/// <param name="b1">The 1st bool in the packed booleans.</param>
		/// <param name="b2">The 2nd bool in the packed booleans.</param>
		/// <param name="b3">The 3rd bool in the packed booleans.</param>
		/// <param name="b4">The 4th bool in the packed booleans.</param>
		/// <param name="b5">The 5th bool in the packed booleans.</param>
		/// <param name="b6">The 6th bool in the packed booleans.</param>
		/// <param name="b7">The 7th bool in the packed booleans.</param>
		/// <exception cref="IOException">Thrown if <paramref name="sin"/> runs out of input early or for other mysterious reasons.</exception>
		public static void ReadBooleans(this Stream sin, out bool b0, out bool b1, out bool b2, out bool b3, out bool b4, out bool b5, out bool b6, out bool b7)
		{
			PackedBooleanBits val = (PackedBooleanBits)ReadByte(sin);

			b0 = (val & PackedBooleanBits.BIT0_BOOLEAN) != PackedBooleanBits.NO_BITS;
			b1 = (val & PackedBooleanBits.BIT1_BOOLEAN) != PackedBooleanBits.NO_BITS;
			b2 = (val & PackedBooleanBits.BIT2_BOOLEAN) != PackedBooleanBits.NO_BITS;
			b3 = (val & PackedBooleanBits.BIT3_BOOLEAN) != PackedBooleanBits.NO_BITS;
			b4 = (val & PackedBooleanBits.BIT4_BOOLEAN) != PackedBooleanBits.NO_BITS;
			b5 = (val & PackedBooleanBits.BIT5_BOOLEAN) != PackedBooleanBits.NO_BITS;
			b6 = (val & PackedBooleanBits.BIT6_BOOLEAN) != PackedBooleanBits.NO_BITS;
			b7 = (val & PackedBooleanBits.BIT7_BOOLEAN) != PackedBooleanBits.NO_BITS;

			return;
		}
		#endregion

		/// <summary>
		/// Writes up to eight bools to <paramref name="sout"/>.
		/// It is assumed that <paramref name="sout"/> is valid and ready to go.
		/// </summary>
		/// <param name="sout">The stream to write to.</param>
		/// <param name="flags">The bools to write.</param>
		/// <exception cref="IOException">Thrown when an IO exception occurs for mysterious reasons.</exception>
		public static void WriteBooleans(this Stream sout, PackedBooleanBits flags)
		{
			WriteByte(sout,(byte)flags);
			return;
		}

		#region UTILITY WRITES
		/// <summary>
		/// Writes up to eight bools to <paramref name="sout"/>.
		/// It is assumed that <paramref name="sout"/> is valid and ready to go.
		/// </summary>
		/// <param name="src">The ISaveable object wanting to write up to eight bools.</param>
		/// <param name="sout">The stream to write to.</param>
		/// <exception cref="IOException">Thrown when an IO exception occurs for mysterious reasons.</exception>
		public static void WriteBooleans(this Stream sout)
		{
			sout.WriteBooleans(PackedBooleanBits.NO_BITS);
			return;
		}

		/// <summary>
		/// Writes up to eight bools to <paramref name="sout"/>.
		/// It is assumed that <paramref name="sout"/> is valid and ready to go.
		/// </summary>
		/// <param name="src">The ISaveable object wanting to write up to eight bools.</param>
		/// <param name="sout">The stream to write to.</param>
		/// <param name="b0">The boolean to write at bit 0.</param>
		/// <exception cref="IOException">Thrown when an IO exception occurs for mysterious reasons.</exception>
		public static void WriteBooleans(this Stream sout, bool b0)
		{
			sout.WriteBooleans(b0 ? PackedBooleanBits.BIT0_BOOLEAN : PackedBooleanBits.NO_BITS);
			return;
		}

		/// <summary>
		/// Writes up to eight bools to <paramref name="sout"/>.
		/// It is assumed that <paramref name="sout"/> is valid and ready to go.
		/// </summary>
		/// <param name="src">The ISaveable object wanting to write up to eight bools.</param>
		/// <param name="sout">The stream to write to.</param>
		/// <param name="b0">The boolean to write at bit 0.</param>
		/// <param name="b1">The boolean to write at bit 1.</param>
		/// <exception cref="IOException">Thrown when an IO exception occurs for mysterious reasons.</exception>
		public static void WriteBooleans(this Stream sout, bool b0, bool b1)
		{
			sout.WriteBooleans((b0 ? PackedBooleanBits.BIT0_BOOLEAN : PackedBooleanBits.NO_BITS) |
							(b1 ? PackedBooleanBits.BIT1_BOOLEAN : PackedBooleanBits.NO_BITS));

			return;
		}

		/// <summary>
		/// Writes up to eight bools to <paramref name="sout"/>.
		/// It is assumed that <paramref name="sout"/> is valid and ready to go.
		/// </summary>
		/// <param name="src">The ISaveable object wanting to write up to eight bools.</param>
		/// <param name="sout">The stream to write to.</param>
		/// <param name="b0">The boolean to write at bit 0.</param>
		/// <param name="b1">The boolean to write at bit 1.</param>
		/// <param name="b2">The boolean to write at bit 2.</param>
		/// <exception cref="IOException">Thrown when an IO exception occurs for mysterious reasons.</exception>
		public static void WriteBooleans(this Stream sout, bool b0, bool b1, bool b2)
		{
			sout.WriteBooleans((b0 ? PackedBooleanBits.BIT0_BOOLEAN : PackedBooleanBits.NO_BITS) |
							(b1 ? PackedBooleanBits.BIT1_BOOLEAN : PackedBooleanBits.NO_BITS) |
							(b2 ? PackedBooleanBits.BIT2_BOOLEAN : PackedBooleanBits.NO_BITS));

			return;
		}

		/// <summary>
		/// Writes up to eight bools to <paramref name="sout"/>.
		/// It is assumed that <paramref name="sout"/> is valid and ready to go.
		/// </summary>
		/// <param name="src">The ISaveable object wanting to write up to eight bools.</param>
		/// <param name="sout">The stream to write to.</param>
		/// <param name="b0">The boolean to write at bit 0.</param>
		/// <param name="b1">The boolean to write at bit 1.</param>
		/// <param name="b2">The boolean to write at bit 2.</param>
		/// <param name="b3">The boolean to write at bit 3.</param>
		/// <exception cref="IOException">Thrown when an IO exception occurs for mysterious reasons.</exception>
		public static void WriteBooleans(this Stream sout, bool b0, bool b1, bool b2, bool b3)
		{
			sout.WriteBooleans((b0 ? PackedBooleanBits.BIT0_BOOLEAN : PackedBooleanBits.NO_BITS) |
							(b1 ? PackedBooleanBits.BIT1_BOOLEAN : PackedBooleanBits.NO_BITS) |
							(b2 ? PackedBooleanBits.BIT2_BOOLEAN : PackedBooleanBits.NO_BITS) |
							(b3 ? PackedBooleanBits.BIT3_BOOLEAN : PackedBooleanBits.NO_BITS));

			return;
		}

		/// <summary>
		/// Writes up to eight bools to <paramref name="sout"/>.
		/// It is assumed that <paramref name="sout"/> is valid and ready to go.
		/// </summary>
		/// <param name="src">The ISaveable object wanting to write up to eight bools.</param>
		/// <param name="sout">The stream to write to.</param>
		/// <param name="b0">The boolean to write at bit 0.</param>
		/// <param name="b1">The boolean to write at bit 1.</param>
		/// <param name="b2">The boolean to write at bit 2.</param>
		/// <param name="b3">The boolean to write at bit 3.</param>
		/// <param name="b4">The boolean to write at bit 4.</param>
		/// <exception cref="IOException">Thrown when an IO exception occurs for mysterious reasons.</exception>
		public static void WriteBooleans(this Stream sout, bool b0, bool b1, bool b2, bool b3, bool b4)
		{
			sout.WriteBooleans((b0 ? PackedBooleanBits.BIT0_BOOLEAN : PackedBooleanBits.NO_BITS) |
							(b1 ? PackedBooleanBits.BIT1_BOOLEAN : PackedBooleanBits.NO_BITS) |
							(b2 ? PackedBooleanBits.BIT2_BOOLEAN : PackedBooleanBits.NO_BITS) |
							(b3 ? PackedBooleanBits.BIT3_BOOLEAN : PackedBooleanBits.NO_BITS) |
							(b4 ? PackedBooleanBits.BIT4_BOOLEAN : PackedBooleanBits.NO_BITS));

			return;
		}

		/// <summary>
		/// Writes up to eight bools to <paramref name="sout"/>.
		/// It is assumed that <paramref name="sout"/> is valid and ready to go.
		/// </summary>
		/// <param name="src">The ISaveable object wanting to write up to eight bools.</param>
		/// <param name="sout">The stream to write to.</param>
		/// <param name="b0">The boolean to write at bit 0.</param>
		/// <param name="b1">The boolean to write at bit 1.</param>
		/// <param name="b2">The boolean to write at bit 2.</param>
		/// <param name="b3">The boolean to write at bit 3.</param>
		/// <param name="b4">The boolean to write at bit 4.</param>
		/// <param name="b5">The boolean to write at bit 5.</param>
		/// <exception cref="IOException">Thrown when an IO exception occurs for mysterious reasons.</exception>
		public static void WriteBooleans(this Stream sout, bool b0, bool b1, bool b2, bool b3, bool b4, bool b5)
		{
			sout.WriteBooleans((b0 ? PackedBooleanBits.BIT0_BOOLEAN : PackedBooleanBits.NO_BITS) |
							(b1 ? PackedBooleanBits.BIT1_BOOLEAN : PackedBooleanBits.NO_BITS) |
							(b2 ? PackedBooleanBits.BIT2_BOOLEAN : PackedBooleanBits.NO_BITS) |
							(b3 ? PackedBooleanBits.BIT3_BOOLEAN : PackedBooleanBits.NO_BITS) |
							(b4 ? PackedBooleanBits.BIT4_BOOLEAN : PackedBooleanBits.NO_BITS) |
							(b5 ? PackedBooleanBits.BIT5_BOOLEAN : PackedBooleanBits.NO_BITS));

			return;
		}

		/// <summary>
		/// Writes up to eight bools to <paramref name="sout"/>.
		/// It is assumed that <paramref name="sout"/> is valid and ready to go.
		/// </summary>
		/// <param name="src">The ISaveable object wanting to write up to eight bools.</param>
		/// <param name="sout">The stream to write to.</param>
		/// <param name="b0">The boolean to write at bit 0.</param>
		/// <param name="b1">The boolean to write at bit 1.</param>
		/// <param name="b2">The boolean to write at bit 2.</param>
		/// <param name="b3">The boolean to write at bit 3.</param>
		/// <param name="b4">The boolean to write at bit 4.</param>
		/// <param name="b5">The boolean to write at bit 5.</param>
		/// <param name="b6">The boolean to write at bit 6.</param>
		/// <exception cref="IOException">Thrown when an IO exception occurs for mysterious reasons.</exception>
		public static void WriteBooleans(this Stream sout, bool b0, bool b1, bool b2, bool b3, bool b4, bool b5, bool b6)
		{
			sout.WriteBooleans((b0 ? PackedBooleanBits.BIT0_BOOLEAN : PackedBooleanBits.NO_BITS) |
							(b1 ? PackedBooleanBits.BIT1_BOOLEAN : PackedBooleanBits.NO_BITS) |
							(b2 ? PackedBooleanBits.BIT2_BOOLEAN : PackedBooleanBits.NO_BITS) |
							(b3 ? PackedBooleanBits.BIT3_BOOLEAN : PackedBooleanBits.NO_BITS) |
							(b4 ? PackedBooleanBits.BIT4_BOOLEAN : PackedBooleanBits.NO_BITS) |
							(b5 ? PackedBooleanBits.BIT5_BOOLEAN : PackedBooleanBits.NO_BITS) |
							(b6 ? PackedBooleanBits.BIT6_BOOLEAN : PackedBooleanBits.NO_BITS));

			return;
		}

		/// <summary>
		/// Writes up to eight bools to <paramref name="sout"/>.
		/// It is assumed that <paramref name="sout"/> is valid and ready to go.
		/// </summary>
		/// <param name="src">The ISaveable object wanting to write up to eight bools.</param>
		/// <param name="sout">The stream to write to.</param>
		/// <param name="b0">The boolean to write at bit 0.</param>
		/// <param name="b1">The boolean to write at bit 1.</param>
		/// <param name="b2">The boolean to write at bit 2.</param>
		/// <param name="b3">The boolean to write at bit 3.</param>
		/// <param name="b4">The boolean to write at bit 4.</param>
		/// <param name="b5">The boolean to write at bit 5.</param>
		/// <param name="b6">The boolean to write at bit 6.</param>
		/// <param name="b7">The boolean to write at bit 7.</param>
		/// <exception cref="IOException">Thrown when an IO exception occurs for mysterious reasons.</exception>
		public static void WriteBooleans(this Stream sout, bool b0, bool b1, bool b2, bool b3, bool b4, bool b5, bool b6, bool b7)
		{
			sout.WriteBooleans((b0 ? PackedBooleanBits.BIT0_BOOLEAN : PackedBooleanBits.NO_BITS) |
							(b1 ? PackedBooleanBits.BIT1_BOOLEAN : PackedBooleanBits.NO_BITS) |
							(b2 ? PackedBooleanBits.BIT2_BOOLEAN : PackedBooleanBits.NO_BITS) |
							(b3 ? PackedBooleanBits.BIT3_BOOLEAN : PackedBooleanBits.NO_BITS) |
							(b4 ? PackedBooleanBits.BIT4_BOOLEAN : PackedBooleanBits.NO_BITS) |
							(b5 ? PackedBooleanBits.BIT5_BOOLEAN : PackedBooleanBits.NO_BITS) |
							(b6 ? PackedBooleanBits.BIT6_BOOLEAN : PackedBooleanBits.NO_BITS) |
							(b7 ? PackedBooleanBits.BIT7_BOOLEAN : PackedBooleanBits.NO_BITS));

			return;
		}
		#endregion

		/// <summary>
		/// Allows us to pack booleans into bit flags.
		/// </summary>
		[Flags]
		public enum PackedBooleanBits
		{
			NO_BITS	   = 0b00000000,
			BIT0_BOOLEAN = 0b00000001,
			BIT1_BOOLEAN = 0b00000010,
			BIT2_BOOLEAN = 0b00000100,
			BIT3_BOOLEAN = 0b00001000,
			BIT4_BOOLEAN = 0b00010000,
			BIT5_BOOLEAN = 0b00100000,
			BIT6_BOOLEAN = 0b01000000,
			BIT7_BOOLEAN = 0b10000000
		}
		#endregion

		#region BYTES
		/// <summary>
		/// Reads a byte from <paramref name="sin"/>.
		/// It is assumed that <paramref name="sin"/> is valid and ready to go.
		/// </summary>
		/// <param name="sin">The stream to read from.</param>
		/// <returns>Returns the byte read from <paramref name="sin"/>.</returns>
		/// <exception cref="IOException">Thrown if <paramref name="sin"/> runs out of input early or for other mysterious reasons.</exception>
		public static byte ReadByte(this Stream sin)
		{
			int ret = sin.ReadByte();

			if(ret == -1)
				throw new IOException("The provided stream ran out of data to read");

			return (byte)ret;
		}

		/// <summary>
		/// Writes a byte to <paramref name="sout"/>.
		/// It is assumed that <paramref name="sout"/> is valid and ready to go.
		/// </summary>
		/// <param name="sout">The stream to write to.</param>
		/// <param name="b">The byte to write.</param>
		/// <exception cref="IOException">Thrown when an IO exception occurs for mysterious reasons.</exception>
		public static void WriteByte(this Stream sout, byte b)
		{
			sout.WriteByte(b);
			return;
		}
		#endregion

		#region SHORTS
		/// <summary>
		/// Reads a short from <paramref name="sin"/>.
		/// The input format should be little endian.
		/// It is assumed that <paramref name="sin"/> is valid and ready to go.
		/// </summary>
		/// <param name="sin">The stream to read from.</param>
		/// <returns>Returns the short read from <paramref name="sin"/>.</returns>
		/// <exception cref="IOException">Thrown if <paramref name="sin"/> runs out of input early or for other mysterious reasons.</exception>
		public static short ReadShort(this Stream sin)
		{
			Span<byte> buffer = new Span<byte>(new byte[sizeof(short)]);

			if(sin.Read(buffer) != buffer.Length)
				throw new IOException("The provided stream ran out of data to read");

			return BinaryPrimitives.ReadInt16LittleEndian(buffer);
		}
		
		/// <summary>
		/// Writes a short to <paramref name="sout"/>.
		/// The output format is in little endian.
		/// It is assumed that <paramref name="sout"/> is valid and ready to go.
		/// </summary>
		/// <param name="sout">The stream to write to.</param>
		/// <param name="s">The short to write.</param>
		/// <exception cref="IOException">Thrown when an IO exception occurs for mysterious reasons.</exception>
		public static void WriteShort(this Stream sout, short s)
		{
			Span<byte> buffer = new Span<byte>(new byte[sizeof(short)]);
			BinaryPrimitives.WriteInt16LittleEndian(buffer,s);
			sout.Write(buffer);

			return;
		}
		#endregion

		#region INTS
		/// <summary>
		/// Reads an int from <paramref name="sin"/>.
		/// The input format should be little endian.
		/// It is assumed that <paramref name="sin"/> is valid and ready to go.
		/// </summary>
		/// <param name="sin">The stream to read from.</param>
		/// <returns>Returns the int read from <paramref name="sin"/>.</returns>
		/// <exception cref="IOException">Thrown if <paramref name="sin"/> runs out of input early or for other mysterious reasons.</exception>
		public static int ReadInt(this Stream sin)
		{
			Span<byte> buffer = new Span<byte>(new byte[sizeof(int)]);

			if(sin.Read(buffer) != buffer.Length)
				throw new IOException("The provided stream ran out of data to read");

			return BinaryPrimitives.ReadInt32LittleEndian(buffer);
		}

		/// <summary>
		/// Writes an int to <paramref name="sout"/>.
		/// The output format is in little endian.
		/// It is assumed that <paramref name="sout"/> is valid and ready to go.
		/// </summary>
		/// <param name="sout">The stream to write to.</param>
		/// <param name="i">The integer to write.</param>
		/// <exception cref="IOException">Thrown when an IO exception occurs for mysterious reasons.</exception>
		public static void WriteInt(this Stream sout, int i)
		{
			Span<byte> buffer = new Span<byte>(new byte[sizeof(int)]);
			BinaryPrimitives.WriteInt32LittleEndian(buffer,i);
			sout.Write(buffer);
			
			return;
		}
		#endregion

		#region LONGS
		/// <summary>
		/// Reads a long from <paramref name="sin"/>.
		/// The input format should be little endian.
		/// It is assumed that <paramref name="sin"/> is valid and ready to go.
		/// </summary>
		/// <param name="sin">The stream to read from.</param>
		/// <returns>Returns the long read from <paramref name="sin"/>.</returns>
		/// <exception cref="IOException">Thrown if <paramref name="sin"/> runs out of input early or for other mysterious reasons.</exception>
		public static long ReadLong(this Stream sin)
		{
			Span<byte> buffer = new Span<byte>(new byte[sizeof(long)]);

			if(sin.Read(buffer) != buffer.Length)
				throw new IOException("The provided stream ran out of data to read");

			return BinaryPrimitives.ReadInt64LittleEndian(buffer);
		}

		/// <summary>
		/// Writes a long to <paramref name="sout"/>.
		/// The output format is in little endian.
		/// It is assumed that <paramref name="sout"/> is valid and ready to go.
		/// </summary>
		/// <param name="sout">The stream to write to.</param>
		/// <param name="l">The long to write.</param>
		/// <exception cref="IOException">Thrown when an IO exception occurs for mysterious reasons.</exception>
		public static void WriteLong(this Stream sout, long l)
		{
			Span<byte> buffer = new Span<byte>(new byte[sizeof(long)]);
			BinaryPrimitives.WriteInt64LittleEndian(buffer,l);
			sout.Write(buffer);

			return;
		}
		#endregion

		#region UNSIGNED SHORTS
		/// <summary>
		/// Reads a ushort from <paramref name="sin"/>.
		/// The input format should be little endian.
		/// It is assumed that <paramref name="sin"/> is valid and ready to go.
		/// </summary>
		/// <param name="sin">The stream to read from.</param>
		/// <returns>Returns the ushort read from <paramref name="sin"/>.</returns>
		/// <exception cref="IOException">Thrown if <paramref name="sin"/> runs out of input early or for other mysterious reasons.</exception>
		public static ushort ReadUnsignedShort(this Stream sin)
		{
			Span<byte> buffer = new Span<byte>(new byte[sizeof(ushort)]);

			if(sin.Read(buffer) != buffer.Length)
				throw new IOException("The provided stream ran out of data to read");

			return BinaryPrimitives.ReadUInt16LittleEndian(buffer);
		}

		/// <summary>
		/// Writes a ushort to <paramref name="sout"/>.
		/// The output format is in little endian.
		/// It is assumed that <paramref name="sout"/> is valid and ready to go.
		/// </summary>
		/// <param name="sout">The stream to write to.</param>
		/// <param name="s">The short to write.</param>
		/// <exception cref="IOException">Thrown when an IO exception occurs for mysterious reasons.</exception>
		public static void WriteUnsignedShort(this Stream sout, ushort s)
		{
			Span<byte> buffer = new Span<byte>(new byte[sizeof(ushort)]);
			BinaryPrimitives.WriteUInt16LittleEndian(buffer,s);
			sout.Write(buffer);

			return;
		}
		#endregion

		#region UNSIGNED INTS
		/// <summary>
		/// Reads an unsigned int from <paramref name="sin"/>.
		/// The input format should be little endian.
		/// It is assumed that <paramref name="sin"/> is valid and ready to go.
		/// </summary>
		/// <param name="sin">The stream to read from.</param>
		/// <returns>Returns the unsigned int read from <paramref name="sin"/>.</returns>
		/// <exception cref="IOException">Thrown if <paramref name="sin"/> runs out of input early or for other mysterious reasons.</exception>
		public static uint ReadUnsignedInt(this Stream sin)
		{
			Span<byte> buffer = new Span<byte>(new byte[sizeof(uint)]);

			if(sin.Read(buffer) != buffer.Length)
				throw new IOException("The provided stream ran out of data to read");

			return BinaryPrimitives.ReadUInt32LittleEndian(buffer);
		}

		/// <summary>
		/// Writes an unsigned int to <paramref name="sout"/>.
		/// The output format is in little endian.
		/// It is assumed that <paramref name="sout"/> is valid and ready to go.
		/// </summary>
		/// <param name="sout">The stream to write to.</param>
		/// <param name="i">The unsigned integer to write.</param>
		/// <exception cref="IOException">Thrown when an IO exception occurs for mysterious reasons.</exception>
		public static void WriteUnsignedInt(this Stream sout, uint i)
		{
			Span<byte> buffer = new Span<byte>(new byte[sizeof(uint)]);
			BinaryPrimitives.WriteUInt32LittleEndian(buffer,i);
			sout.Write(buffer);
			
			return;
		}
		#endregion

		#region UNSIGNED LONGS
		/// <summary>
		/// Reads a ulong from <paramref name="sin"/>.
		/// The input format should be little endian.
		/// It is assumed that <paramref name="sin"/> is valid and ready to go.
		/// </summary>
		/// <param name="sin">The stream to read from.</param>
		/// <returns>Returns the ulong read from <paramref name="sin"/>.</returns>
		/// <exception cref="IOException">Thrown if <paramref name="sin"/> runs out of input early or for other mysterious reasons.</exception>
		public static ulong ReadUnsignedLong(this Stream sin)
		{
			Span<byte> buffer = new Span<byte>(new byte[sizeof(ulong)]);

			if(sin.Read(buffer) != buffer.Length)
				throw new IOException("The provided stream ran out of data to read");

			return BinaryPrimitives.ReadUInt64LittleEndian(buffer);
		}

		/// <summary>
		/// Writes a ulong to <paramref name="sout"/>.
		/// The output format is in little endian.
		/// It is assumed that <paramref name="sout"/> is valid and ready to go.
		/// </summary>
		/// <param name="sout">The stream to write to.</param>
		/// <param name="l">The ulong to write.</param>
		/// <exception cref="IOException">Thrown when an IO exception occurs for mysterious reasons.</exception>
		public static void WriteUnsignedLong(this Stream sout, ulong l)
		{
			Span<byte> buffer = new Span<byte>(new byte[sizeof(ulong)]);
			BinaryPrimitives.WriteUInt64LittleEndian(buffer,l);
			sout.Write(buffer);

			return;
		}
		#endregion

		#region FLOATS
		/// <summary>
		/// Reads a float from <paramref name="sin"/>.
		/// The input format should be little endian.
		/// It is assumed that <paramref name="sin"/> is valid and ready to go.
		/// </summary>
		/// <param name="sin">The stream to read from.</param>
		/// <returns>Returns the float read from <paramref name="sin"/>.</returns>
		/// <exception cref="IOException">Thrown if <paramref name="sin"/> runs out of input early or for other mysterious reasons.</exception>
		public static float ReadFloat(this Stream sin)
		{return BitConverter.Int32BitsToSingle(sin.ReadInt());}

		/// <summary>
		/// Writes a float to <paramref name="sout"/>.
		/// The output format is in little endian.
		/// It is assumed that <paramref name="sout"/> is valid and ready to go.
		/// </summary>
		/// <param name="sout">The stream to write to.</param>
		/// <param name="f">The float to write.</param>
		/// <exception cref="IOException">Thrown when an IO exception occurs for mysterious reasons.</exception>
		public static void WriteFloat(this Stream sout, float f)
		{
			sout.WriteInt(BitConverter.SingleToInt32Bits(f));
			return;
		}
		#endregion

		#region DOUBLES
		/// <summary>
		/// Reads a double from <paramref name="sin"/>.
		/// The input format should be little endian.
		/// It is assumed that <paramref name="sin"/> is valid and ready to go.
		/// </summary>
		/// <param name="sin">The stream to read from.</param>
		/// <returns>Returns the double read from <paramref name="sin"/>.</returns>
		/// <exception cref="IOException">Thrown if <paramref name="sin"/> runs out of input early or for other mysterious reasons.</exception>
		public static double ReadDouble(this Stream sin)
		{return BitConverter.Int64BitsToDouble(sin.ReadLong());}

		/// <summary>
		/// Writes a double to <paramref name="sout"/>.
		/// The output format is in little endian.
		/// It is assumed that <paramref name="sout"/> is valid and ready to go.
		/// </summary>
		/// <param name="sout">The stream to write to.</param>
		/// <param name="d">The double to write.</param>
		/// <exception cref="IOException">Thrown when an IO exception occurs for mysterious reasons.</exception>
		public static void WriteDouble(this Stream sout, double d)
		{
			sout.WriteLong(BitConverter.DoubleToInt64Bits(d));
			return;
		}
		#endregion

		#region STRINGS
		/// <summary>
		/// Reads a string from <paramref name="sin"/>.
		/// The input format should be an integer specifying the length of the string followed by the string itself.
		/// It is assumed that <paramref name="sin"/> is valid and ready to go.
		/// </summary>
		/// <param name="sin">The stream to read from.</param>
		/// <returns>Returns the string read or null if the proposed length of the string is less than zero.</returns>
		/// <exception cref="IOException">Thrown if <paramref name="sin"/> runs out of input early or for other mysterious reasons.</exception>
		public static string? ReadString(this Stream sin)
		{
			int len = sin.ReadInt();

			if(len < 0)
				return null;

			Span<byte> buffer = new Span<byte>(new byte[len * sizeof(char)]);

			if(sin.Read(buffer) != buffer.Length)
				throw new IOException("The provided stream ran out of data to read");

			Span<char> raw = new Span<char>(new char[len]);
			
			if(Encoding.Unicode.GetDecoder().GetChars(buffer,raw,true) != len)
				throw new IOException("Something went wrong when decoding a string");

			return new string(raw);
		}

		/// <summary>
		/// Writes a string to <paramref name="sout"/>.
		/// The output format is an integer specifying the length of the string followed by the string itself.
		/// It is assumed that <paramref name="sout"/> is valid and ready to go.
		/// </summary>
		/// <param name="sout">The stream to write to.</param>
		/// <param name="str">The string to write.</param>
		/// <exception cref="IOException">Thrown when an IO exception occurs for mysterious reasons.</exception>
		public static void WriteString(this Stream sout, string str)
		{
			if(str == null)
			{
				sout.WriteInt(-1);
				return;
			}

			sout.WriteInt(str.Length);

			Span<byte> buffer = new Span<byte>(new byte[str.Length * sizeof(char)]);
			
			for(int i = 0;i < str.Length;i++)
				BinaryPrimitives.WriteUInt16LittleEndian(buffer.Slice(i * sizeof(char),sizeof(char)),str[i]);

			sout.Write(buffer);
			return;
		}
		#endregion
	}
}