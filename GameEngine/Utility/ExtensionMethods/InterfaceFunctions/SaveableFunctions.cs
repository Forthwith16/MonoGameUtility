using GameEngine.Utility.Parsing;
using System.Buffers.Binary;
using System.Text;

namespace GameEngine.Utility.ExtensionMethods.InterfaceFunctions
{
	/// <summary>
	/// Provides extension functions to ISaveable objects.
	/// </summary>
	public static class SaveableFunctions
	{
		#region BOOLEANS
		/// <summary>
		/// Reads a bool from <paramref name="sin"/>.
		/// It is assumed that <paramref name="sin"/> is valid and ready to go.
		/// </summary>
		/// <param name="src">The ISaveable object wanting to read a bool.</param>
		/// <param name="sin">The stream to read from.</param>
		/// <returns>Returns the bool read from <paramref name="sin"/>.</returns>
		/// <exception cref="IOException">Thrown if <paramref name="sin"/> runs out of input early or for other mysterious reasons.</exception>
		public static bool ReadBoolean(this ISaveable src, Stream sin)
		{return src.ReadByte(sin) != 0;}

		/// <summary>
		/// Reads a bool from <paramref name="sin"/>.
		/// It is assumed that <paramref name="sin"/> is valid and ready to go.
		/// </summary>
		/// <param name="sin">The stream to read from.</param>
		/// <returns>Returns the bool read from <paramref name="sin"/>.</returns>
		/// <exception cref="IOException">Thrown if <paramref name="sin"/> runs out of input early or for other mysterious reasons.</exception>
		public static bool ReadBoolean(Stream sin)
		{return ReadByte(sin) != 0;}

		/// <summary>
		/// Writes a bool to <paramref name="sout"/>.
		/// It is assumed that <paramref name="sout"/> is valid and ready to go.
		/// </summary>
		/// <param name="src">The ISaveable object wanting to write a bool.</param>
		/// <param name="sout">The stream to write to.</param>
		/// <param name="b">The bool to write.</param>
		/// <exception cref="IOException">Thrown when an IO exception occurs for mysterious reasons.</exception>
		public static void WriteBoolean(this ISaveable src, Stream sout, bool b)
		{
			src.WriteByte(sout,b ? (byte)1 : (byte)0);
			return;
		}

		/// <summary>
		/// Writes a bool to <paramref name="sout"/>.
		/// It is assumed that <paramref name="sout"/> is valid and ready to go.
		/// </summary>
		/// <param name="sout">The stream to write to.</param>
		/// <param name="b">The bool to write.</param>
		/// <exception cref="IOException">Thrown when an IO exception occurs for mysterious reasons.</exception>
		public static void WriteBoolean(Stream sout, bool b)
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
		/// <param name="src">The ISaveable object wanting to read up to eight bools.</param>
		/// <param name="sin">The stream to read from.</param>
		/// <returns>Returns the bools read from <paramref name="sin"/>.</returns>
		/// <exception cref="IOException">Thrown if <paramref name="sin"/> runs out of input early or for other mysterious reasons.</exception>
		public static PackedBooleanBits ReadBooleans(this ISaveable src, Stream sin)
		{return (PackedBooleanBits)src.ReadByte(sin);}

		/// <summary>
		/// Reads up to eight bools from <paramref name="sin"/>.
		/// It is assumed that <paramref name="sin"/> is valid and ready to go.
		/// </summary>
		/// <param name="sin">The stream to read from.</param>
		/// <returns>Returns the bools read from <paramref name="sin"/>.</returns>
		/// <exception cref="IOException">Thrown if <paramref name="sin"/> runs out of input early or for other mysterious reasons.</exception>
		public static PackedBooleanBits ReadBooleans(Stream sin)
		{return (PackedBooleanBits)ReadByte(sin);}

		/// <summary>
		/// Reads a bool from a packed set of bools from <paramref name="sin"/>.
		/// It is assumed that <paramref name="sin"/> is valid and ready to go.
		/// </summary>
		/// <param name="src">The ISaveable object wanting to read a packed bool value.</param>
		/// <param name="sin">The stream to read from.</param>
		/// <param name="flag">The boolean bit we want to access. The algorithm skips error checking and immediately checks if (flag & read) != NO_BITS. Use/abuse this at your own peril.</param>
		/// <returns>Returns the bool read from <paramref name="sin"/>.</returns>
		/// <exception cref="IOException">Thrown if <paramref name="sin"/> runs out of input early or for other mysterious reasons.</exception>
		public static bool ReadBoolean(this ISaveable src, Stream sin, PackedBooleanBits flag)
		{return ((PackedBooleanBits)src.ReadByte(sin) & flag) != PackedBooleanBits.NO_BITS;}

		/// <summary>
		/// Reads a bool from a packed set of bools from <paramref name="sin"/>.
		/// It is assumed that <paramref name="sin"/> is valid and ready to go.
		/// </summary>
		/// <param name="sin">The stream to read from.</param>
		/// <param name="flag">The boolean bit we want to access. The algorithm skips error checking and immediately checks if (flag & read) != NO_BITS. Use/abuse this at your own peril.</param>
		/// <returns>Returns the bool read from <paramref name="sin"/>.</returns>
		/// <exception cref="IOException">Thrown if <paramref name="sin"/> runs out of input early or for other mysterious reasons.</exception>
		public static bool ReadBoolean(Stream sin, PackedBooleanBits flag)
		{return ((PackedBooleanBits)ReadByte(sin) & flag) != PackedBooleanBits.NO_BITS;}

		#region UTILITY READS
		/// <summary>
		/// Reads up to eight bools from <paramref name="sin"/>.
		/// It is assumed that <paramref name="sin"/> is valid and ready to go.
		/// </summary>
		/// <param name="src">The ISaveable object wanting to read up to eight bools.</param>
		/// <param name="sin">The stream to read from.</param>
		/// <param name="b0">The 0th bool in the packed booleans.</param>
		/// <exception cref="IOException">Thrown if <paramref name="sin"/> runs out of input early or for other mysterious reasons.</exception>
		public static void ReadBooleans(this ISaveable src, Stream sin, out bool b0)
		{
			PackedBooleanBits val = (PackedBooleanBits)src.ReadByte(sin);

			b0 = (val & PackedBooleanBits.BIT0_BOOLEAN) != PackedBooleanBits.NO_BITS;

			return;
		}

		/// <summary>
		/// Reads up to eight bools from <paramref name="sin"/>.
		/// It is assumed that <paramref name="sin"/> is valid and ready to go.
		/// </summary>
		/// <param name="sin">The stream to read from.</param>
		/// <param name="b0">The 0th bool in the packed booleans.</param>
		/// <exception cref="IOException">Thrown if <paramref name="sin"/> runs out of input early or for other mysterious reasons.</exception>
		public static void ReadBooleans(Stream sin, out bool b0)
		{
			PackedBooleanBits val = (PackedBooleanBits)ReadByte(sin);

			b0 = (val & PackedBooleanBits.BIT0_BOOLEAN) != PackedBooleanBits.NO_BITS;

			return;
		}

		/// <summary>
		/// Reads up to eight bools from <paramref name="sin"/>.
		/// It is assumed that <paramref name="sin"/> is valid and ready to go.
		/// </summary>
		/// <param name="src">The ISaveable object wanting to read up to eight bools.</param>
		/// <param name="sin">The stream to read from.</param>
		/// <param name="b0">The 0th bool in the packed booleans.</param>
		/// <param name="b1">The 1st bool in the packed booleans.</param>
		/// <exception cref="IOException">Thrown if <paramref name="sin"/> runs out of input early or for other mysterious reasons.</exception>
		public static void ReadBooleans(this ISaveable src, Stream sin, out bool b0, out bool b1)
		{
			PackedBooleanBits val = (PackedBooleanBits)src.ReadByte(sin);

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
		/// <exception cref="IOException">Thrown if <paramref name="sin"/> runs out of input early or for other mysterious reasons.</exception>
		public static void ReadBooleans(Stream sin, out bool b0, out bool b1)
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
		/// <param name="src">The ISaveable object wanting to read up to eight bools.</param>
		/// <param name="sin">The stream to read from.</param>
		/// <param name="b0">The 0th bool in the packed booleans.</param>
		/// <param name="b1">The 1st bool in the packed booleans.</param>
		/// <param name="b2">The 2nd bool in the packed booleans.</param>
		/// <exception cref="IOException">Thrown if <paramref name="sin"/> runs out of input early or for other mysterious reasons.</exception>
		public static void ReadBooleans(this ISaveable src, Stream sin, out bool b0, out bool b1, out bool b2)
		{
			PackedBooleanBits val = (PackedBooleanBits)src.ReadByte(sin);

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
		/// <exception cref="IOException">Thrown if <paramref name="sin"/> runs out of input early or for other mysterious reasons.</exception>
		public static void ReadBooleans(Stream sin, out bool b0, out bool b1, out bool b2)
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
		/// <param name="src">The ISaveable object wanting to read up to eight bools.</param>
		/// <param name="sin">The stream to read from.</param>
		/// <param name="b0">The 0th bool in the packed booleans.</param>
		/// <param name="b1">The 1st bool in the packed booleans.</param>
		/// <param name="b2">The 2nd bool in the packed booleans.</param>
		/// <param name="b3">The 3rd bool in the packed booleans.</param>
		/// <exception cref="IOException">Thrown if <paramref name="sin"/> runs out of input early or for other mysterious reasons.</exception>
		public static void ReadBooleans(this ISaveable src, Stream sin, out bool b0, out bool b1, out bool b2, out bool b3)
		{
			PackedBooleanBits val = (PackedBooleanBits)src.ReadByte(sin);

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
		/// <exception cref="IOException">Thrown if <paramref name="sin"/> runs out of input early or for other mysterious reasons.</exception>
		public static void ReadBooleans(Stream sin, out bool b0, out bool b1, out bool b2, out bool b3)
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
		/// <param name="src">The ISaveable object wanting to read up to eight bools.</param>
		/// <param name="sin">The stream to read from.</param>
		/// <param name="b0">The 0th bool in the packed booleans.</param>
		/// <param name="b1">The 1st bool in the packed booleans.</param>
		/// <param name="b2">The 2nd bool in the packed booleans.</param>
		/// <param name="b3">The 3rd bool in the packed booleans.</param>
		/// <param name="b4">The 4th bool in the packed booleans.</param>
		/// <exception cref="IOException">Thrown if <paramref name="sin"/> runs out of input early or for other mysterious reasons.</exception>
		public static void ReadBooleans(this ISaveable src, Stream sin, out bool b0, out bool b1, out bool b2, out bool b3, out bool b4)
		{
			PackedBooleanBits val = (PackedBooleanBits)src.ReadByte(sin);

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
		/// <exception cref="IOException">Thrown if <paramref name="sin"/> runs out of input early or for other mysterious reasons.</exception>
		public static void ReadBooleans(Stream sin, out bool b0, out bool b1, out bool b2, out bool b3, out bool b4)
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
		/// <param name="src">The ISaveable object wanting to read up to eight bools.</param>
		/// <param name="sin">The stream to read from.</param>
		/// <param name="b0">The 0th bool in the packed booleans.</param>
		/// <param name="b1">The 1st bool in the packed booleans.</param>
		/// <param name="b2">The 2nd bool in the packed booleans.</param>
		/// <param name="b3">The 3rd bool in the packed booleans.</param>
		/// <param name="b4">The 4th bool in the packed booleans.</param>
		/// <param name="b5">The 5th bool in the packed booleans.</param>
		/// <exception cref="IOException">Thrown if <paramref name="sin"/> runs out of input early or for other mysterious reasons.</exception>
		public static void ReadBooleans(this ISaveable src, Stream sin, out bool b0, out bool b1, out bool b2, out bool b3, out bool b4, out bool b5)
		{
			PackedBooleanBits val = (PackedBooleanBits)src.ReadByte(sin);

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
		/// <exception cref="IOException">Thrown if <paramref name="sin"/> runs out of input early or for other mysterious reasons.</exception>
		public static void ReadBooleans(Stream sin, out bool b0, out bool b1, out bool b2, out bool b3, out bool b4, out bool b5)
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
		/// <param name="src">The ISaveable object wanting to read up to eight bools.</param>
		/// <param name="sin">The stream to read from.</param>
		/// <param name="b0">The 0th bool in the packed booleans.</param>
		/// <param name="b1">The 1st bool in the packed booleans.</param>
		/// <param name="b2">The 2nd bool in the packed booleans.</param>
		/// <param name="b3">The 3rd bool in the packed booleans.</param>
		/// <param name="b4">The 4th bool in the packed booleans.</param>
		/// <param name="b5">The 5th bool in the packed booleans.</param>
		/// <param name="b6">The 6th bool in the packed booleans.</param>
		/// <exception cref="IOException">Thrown if <paramref name="sin"/> runs out of input early or for other mysterious reasons.</exception>
		public static void ReadBooleans(this ISaveable src, Stream sin, out bool b0, out bool b1, out bool b2, out bool b3, out bool b4, out bool b5, out bool b6)
		{
			PackedBooleanBits val = (PackedBooleanBits)src.ReadByte(sin);

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
		/// <exception cref="IOException">Thrown if <paramref name="sin"/> runs out of input early or for other mysterious reasons.</exception>
		public static void ReadBooleans(Stream sin, out bool b0, out bool b1, out bool b2, out bool b3, out bool b4, out bool b5, out bool b6)
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
		/// <param name="src">The ISaveable object wanting to read up to eight bools.</param>
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
		public static void ReadBooleans(this ISaveable src, Stream sin, out bool b0, out bool b1, out bool b2, out bool b3, out bool b4, out bool b5, out bool b6, out bool b7)
		{
			PackedBooleanBits val = (PackedBooleanBits)src.ReadByte(sin);

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
		public static void ReadBooleans(Stream sin, out bool b0, out bool b1, out bool b2, out bool b3, out bool b4, out bool b5, out bool b6, out bool b7)
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
		/// <param name="src">The ISaveable object wanting to write up to eight bools.</param>
		/// <param name="sout">The stream to write to.</param>
		/// <param name="flags">The bools to write.</param>
		/// <exception cref="IOException">Thrown when an IO exception occurs for mysterious reasons.</exception>
		public static void WriteBooleans(this ISaveable src, Stream sout, PackedBooleanBits flags)
		{
			src.WriteByte(sout,(byte)flags);
			return;
		}

		/// <summary>
		/// Writes up to eight bools to <paramref name="sout"/>.
		/// It is assumed that <paramref name="sout"/> is valid and ready to go.
		/// </summary>
		/// <param name="sout">The stream to write to.</param>
		/// <param name="flags">The bools to write.</param>
		/// <exception cref="IOException">Thrown when an IO exception occurs for mysterious reasons.</exception>
		public static void WriteBooleans(Stream sout, PackedBooleanBits flags)
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
		public static void WriteBooleans(this ISaveable src, Stream sout)
		{
			src.WriteBooleans(sout,PackedBooleanBits.NO_BITS);
			return;
		}

		/// <summary>
		/// Writes up to eight bools to <paramref name="sout"/>.
		/// It is assumed that <paramref name="sout"/> is valid and ready to go.
		/// </summary>
		/// <param name="src">The ISaveable object wanting to write up to eight bools.</param>
		/// <param name="sout">The stream to write to.</param>
		/// <exception cref="IOException">Thrown when an IO exception occurs for mysterious reasons.</exception>
		public static void WriteBooleans(Stream sout)
		{
			WriteBooleans(sout,PackedBooleanBits.NO_BITS);
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
		public static void WriteBooleans(this ISaveable src, Stream sout, bool b0)
		{
			src.WriteBooleans(sout,b0 ? PackedBooleanBits.BIT0_BOOLEAN : PackedBooleanBits.NO_BITS);
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
		public static void WriteBooleans(Stream sout, bool b0)
		{
			WriteBooleans(sout,b0 ? PackedBooleanBits.BIT0_BOOLEAN : PackedBooleanBits.NO_BITS);
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
		public static void WriteBooleans(this ISaveable src, Stream sout, bool b0, bool b1)
		{
			src.WriteBooleans(sout,  (b0 ? PackedBooleanBits.BIT0_BOOLEAN : PackedBooleanBits.NO_BITS) |
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
		/// <exception cref="IOException">Thrown when an IO exception occurs for mysterious reasons.</exception>
		public static void WriteBooleans(Stream sout, bool b0, bool b1)
		{
			WriteBooleans(sout, (b0 ? PackedBooleanBits.BIT0_BOOLEAN : PackedBooleanBits.NO_BITS) |
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
		public static void WriteBooleans(this ISaveable src, Stream sout, bool b0, bool b1, bool b2)
		{
			src.WriteBooleans(sout,  (b0 ? PackedBooleanBits.BIT0_BOOLEAN : PackedBooleanBits.NO_BITS) |
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
		/// <exception cref="IOException">Thrown when an IO exception occurs for mysterious reasons.</exception>
		public static void WriteBooleans(Stream sout, bool b0, bool b1, bool b2)
		{
			WriteBooleans(sout, (b0 ? PackedBooleanBits.BIT0_BOOLEAN : PackedBooleanBits.NO_BITS) |
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
		public static void WriteBooleans(this ISaveable src, Stream sout, bool b0, bool b1, bool b2, bool b3)
		{
			src.WriteBooleans(sout,  (b0 ? PackedBooleanBits.BIT0_BOOLEAN : PackedBooleanBits.NO_BITS) |
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
		/// <exception cref="IOException">Thrown when an IO exception occurs for mysterious reasons.</exception>
		public static void WriteBooleans(Stream sout, bool b0, bool b1, bool b2, bool b3)
		{
			WriteBooleans(sout, (b0 ? PackedBooleanBits.BIT0_BOOLEAN : PackedBooleanBits.NO_BITS) |
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
		public static void WriteBooleans(this ISaveable src, Stream sout, bool b0, bool b1, bool b2, bool b3, bool b4)
		{
			src.WriteBooleans(sout,  (b0 ? PackedBooleanBits.BIT0_BOOLEAN : PackedBooleanBits.NO_BITS) |
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
		/// <exception cref="IOException">Thrown when an IO exception occurs for mysterious reasons.</exception>
		public static void WriteBooleans(Stream sout, bool b0, bool b1, bool b2, bool b3, bool b4)
		{
			WriteBooleans(sout, (b0 ? PackedBooleanBits.BIT0_BOOLEAN : PackedBooleanBits.NO_BITS) |
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
		public static void WriteBooleans(this ISaveable src, Stream sout, bool b0, bool b1, bool b2, bool b3, bool b4, bool b5)
		{
			src.WriteBooleans(sout,  (b0 ? PackedBooleanBits.BIT0_BOOLEAN : PackedBooleanBits.NO_BITS) |
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
		/// <exception cref="IOException">Thrown when an IO exception occurs for mysterious reasons.</exception>
		public static void WriteBooleans(Stream sout, bool b0, bool b1, bool b2, bool b3, bool b4, bool b5)
		{
			WriteBooleans(sout, (b0 ? PackedBooleanBits.BIT0_BOOLEAN : PackedBooleanBits.NO_BITS) |
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
		public static void WriteBooleans(this ISaveable src, Stream sout, bool b0, bool b1, bool b2, bool b3, bool b4, bool b5, bool b6)
		{
			src.WriteBooleans(sout,  (b0 ? PackedBooleanBits.BIT0_BOOLEAN : PackedBooleanBits.NO_BITS) |
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
		/// <exception cref="IOException">Thrown when an IO exception occurs for mysterious reasons.</exception>
		public static void WriteBooleans(Stream sout, bool b0, bool b1, bool b2, bool b3, bool b4, bool b5, bool b6)
		{
			WriteBooleans(sout, (b0 ? PackedBooleanBits.BIT0_BOOLEAN : PackedBooleanBits.NO_BITS) |
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
		public static void WriteBooleans(this ISaveable src, Stream sout, bool b0, bool b1, bool b2, bool b3, bool b4, bool b5, bool b6, bool b7)
		{
			src.WriteBooleans(sout,  (b0 ? PackedBooleanBits.BIT0_BOOLEAN : PackedBooleanBits.NO_BITS) |
								(b1 ? PackedBooleanBits.BIT1_BOOLEAN : PackedBooleanBits.NO_BITS) |
								(b2 ? PackedBooleanBits.BIT2_BOOLEAN : PackedBooleanBits.NO_BITS) |
								(b3 ? PackedBooleanBits.BIT3_BOOLEAN : PackedBooleanBits.NO_BITS) |
								(b4 ? PackedBooleanBits.BIT4_BOOLEAN : PackedBooleanBits.NO_BITS) |
								(b5 ? PackedBooleanBits.BIT5_BOOLEAN : PackedBooleanBits.NO_BITS) |
								(b6 ? PackedBooleanBits.BIT6_BOOLEAN : PackedBooleanBits.NO_BITS) |
								(b7 ? PackedBooleanBits.BIT7_BOOLEAN : PackedBooleanBits.NO_BITS));

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
		public static void WriteBooleans(Stream sout, bool b0, bool b1, bool b2, bool b3, bool b4, bool b5, bool b6, bool b7)
		{
			WriteBooleans(sout, (b0 ? PackedBooleanBits.BIT0_BOOLEAN : PackedBooleanBits.NO_BITS) |
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
		/// <param name="src">The ISaveable object wanting to read a byte.</param>
		/// <param name="sin">The stream to read from.</param>
		/// <returns>Returns the byte read from <paramref name="sin"/>.</returns>
		/// <exception cref="IOException">Thrown if <paramref name="sin"/> runs out of input early or for other mysterious reasons.</exception>
		public static byte ReadByte(this ISaveable src, Stream sin)
		{
			int ret = sin.ReadByte();

			if(ret == -1)
				throw new IOException("The provided stream ran out of data to read");

			return (byte)ret;
		}

		/// <summary>
		/// Reads a byte from <paramref name="sin"/>.
		/// It is assumed that <paramref name="sin"/> is valid and ready to go.
		/// </summary>
		/// <param name="sin">The stream to read from.</param>
		/// <returns>Returns the byte read from <paramref name="sin"/>.</returns>
		/// <exception cref="IOException">Thrown if <paramref name="sin"/> runs out of input early or for other mysterious reasons.</exception>
		public static byte ReadByte(Stream sin)
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
		/// <param name="src">The ISaveable object wanting to write a byte.</param>
		/// <param name="sout">The stream to write to.</param>
		/// <param name="b">The byte to write.</param>
		/// <exception cref="IOException">Thrown when an IO exception occurs for mysterious reasons.</exception>
		public static void WriteByte(this ISaveable src, Stream sout, byte b)
		{
			sout.WriteByte(b);
			return;
		}

		/// <summary>
		/// Writes a byte to <paramref name="sout"/>.
		/// It is assumed that <paramref name="sout"/> is valid and ready to go.
		/// </summary>
		/// <param name="sout">The stream to write to.</param>
		/// <param name="b">The byte to write.</param>
		/// <exception cref="IOException">Thrown when an IO exception occurs for mysterious reasons.</exception>
		public static void WriteByte(Stream sout, byte b)
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
		/// <param name="src">The ISaveable object wanting to read a short.</param>
		/// <param name="sin">The stream to read from.</param>
		/// <returns>Returns the short read from <paramref name="sin"/>.</returns>
		/// <exception cref="IOException">Thrown if <paramref name="sin"/> runs out of input early or for other mysterious reasons.</exception>
		public static short ReadShort(this ISaveable src, Stream sin)
		{
			Span<byte> buffer = new Span<byte>(new byte[sizeof(short)]);

			if(sin.Read(buffer) != buffer.Length)
				throw new IOException("The provided stream ran out of data to read");

			return BinaryPrimitives.ReadInt16LittleEndian(buffer);
		}

		/// <summary>
		/// Reads a short from <paramref name="sin"/>.
		/// The input format should be little endian.
		/// It is assumed that <paramref name="sin"/> is valid and ready to go.
		/// </summary>
		/// <param name="sin">The stream to read from.</param>
		/// <returns>Returns the short read from <paramref name="sin"/>.</returns>
		/// <exception cref="IOException">Thrown if <paramref name="sin"/> runs out of input early or for other mysterious reasons.</exception>
		public static short ReadShort(Stream sin)
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
		/// <param name="src">The ISaveable object wanting to write a short.</param>
		/// <param name="sout">The stream to write to.</param>
		/// <param name="s">The short to write.</param>
		/// <exception cref="IOException">Thrown when an IO exception occurs for mysterious reasons.</exception>
		public static void WriteShort(this ISaveable src, Stream sout, short s)
		{
			Span<byte> buffer = new Span<byte>(new byte[sizeof(short)]);
			BinaryPrimitives.WriteInt16LittleEndian(buffer,s);
			sout.Write(buffer);

			return;
		}

		/// <summary>
		/// Writes a short to <paramref name="sout"/>.
		/// The output format is in little endian.
		/// It is assumed that <paramref name="sout"/> is valid and ready to go.
		/// </summary>
		/// <param name="sout">The stream to write to.</param>
		/// <param name="s">The short to write.</param>
		/// <exception cref="IOException">Thrown when an IO exception occurs for mysterious reasons.</exception>
		public static void WriteShort(Stream sout, short s)
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
		/// <param name="src">The ISaveable object wanting to read an int.</param>
		/// <param name="sin">The stream to read from.</param>
		/// <returns>Returns the int read from <paramref name="sin"/>.</returns>
		/// <exception cref="IOException">Thrown if <paramref name="sin"/> runs out of input early or for other mysterious reasons.</exception>
		public static int ReadInt(this ISaveable src, Stream sin)
		{
			Span<byte> buffer = new Span<byte>(new byte[sizeof(int)]);

			if(sin.Read(buffer) != buffer.Length)
				throw new IOException("The provided stream ran out of data to read");

			return BinaryPrimitives.ReadInt32LittleEndian(buffer);
		}

		/// <summary>
		/// Reads an int from <paramref name="sin"/>.
		/// The input format should be little endian.
		/// It is assumed that <paramref name="sin"/> is valid and ready to go.
		/// </summary>
		/// <param name="sin">The stream to read from.</param>
		/// <returns>Returns the int read from <paramref name="sin"/>.</returns>
		/// <exception cref="IOException">Thrown if <paramref name="sin"/> runs out of input early or for other mysterious reasons.</exception>
		public static int ReadInt(Stream sin)
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
		/// <param name="src">The ISaveable object wanting to write an int.</param>
		/// <param name="sout">The stream to write to.</param>
		/// <param name="i">The integer to write.</param>
		/// <exception cref="IOException">Thrown when an IO exception occurs for mysterious reasons.</exception>
		public static void WriteInt(this ISaveable src, Stream sout, int i)
		{
			Span<byte> buffer = new Span<byte>(new byte[sizeof(int)]);
			BinaryPrimitives.WriteInt32LittleEndian(buffer,i);
			sout.Write(buffer);

			return;
		}

		/// <summary>
		/// Writes an int to <paramref name="sout"/>.
		/// The output format is in little endian.
		/// It is assumed that <paramref name="sout"/> is valid and ready to go.
		/// </summary>
		/// <param name="sout">The stream to write to.</param>
		/// <param name="i">The integer to write.</param>
		/// <exception cref="IOException">Thrown when an IO exception occurs for mysterious reasons.</exception>
		public static void WriteInt(Stream sout, int i)
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
		/// <param name="src">The ISaveable object wanting to read a long.</param>
		/// <param name="sin">The stream to read from.</param>
		/// <returns>Returns the long read from <paramref name="sin"/>.</returns>
		/// <exception cref="IOException">Thrown if <paramref name="sin"/> runs out of input early or for other mysterious reasons.</exception>
		public static long ReadLong(this ISaveable src, Stream sin)
		{
			Span<byte> buffer = new Span<byte>(new byte[sizeof(long)]);

			if(sin.Read(buffer) != buffer.Length)
				throw new IOException("The provided stream ran out of data to read");

			return BinaryPrimitives.ReadInt64LittleEndian(buffer);
		}

		/// <summary>
		/// Reads a long from <paramref name="sin"/>.
		/// The input format should be little endian.
		/// It is assumed that <paramref name="sin"/> is valid and ready to go.
		/// </summary>
		/// <param name="sin">The stream to read from.</param>
		/// <returns>Returns the long read from <paramref name="sin"/>.</returns>
		/// <exception cref="IOException">Thrown if <paramref name="sin"/> runs out of input early or for other mysterious reasons.</exception>
		public static long ReadLong(Stream sin)
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
		/// <param name="src">The ISaveable object wanting to write a long.</param>
		/// <param name="sout">The stream to write to.</param>
		/// <param name="l">The long to write.</param>
		/// <exception cref="IOException">Thrown when an IO exception occurs for mysterious reasons.</exception>
		public static void WriteLong(this ISaveable src, Stream sout, long l)
		{
			Span<byte> buffer = new Span<byte>(new byte[sizeof(long)]);
			BinaryPrimitives.WriteInt64LittleEndian(buffer,l);
			sout.Write(buffer);

			return;
		}

		/// <summary>
		/// Writes a long to <paramref name="sout"/>.
		/// The output format is in little endian.
		/// It is assumed that <paramref name="sout"/> is valid and ready to go.
		/// </summary>
		/// <param name="sout">The stream to write to.</param>
		/// <param name="l">The long to write.</param>
		/// <exception cref="IOException">Thrown when an IO exception occurs for mysterious reasons.</exception>
		public static void WriteLong(Stream sout, long l)
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
		/// <param name="src">The ISaveable object wanting to read a ushort.</param>
		/// <param name="sin">The stream to read from.</param>
		/// <returns>Returns the ushort read from <paramref name="sin"/>.</returns>
		/// <exception cref="IOException">Thrown if <paramref name="sin"/> runs out of input early or for other mysterious reasons.</exception>
		public static ushort ReadUnsignedShort(this ISaveable src, Stream sin)
		{
			Span<byte> buffer = new Span<byte>(new byte[sizeof(ushort)]);

			if(sin.Read(buffer) != buffer.Length)
				throw new IOException("The provided stream ran out of data to read");

			return BinaryPrimitives.ReadUInt16LittleEndian(buffer);
		}

		/// <summary>
		/// Reads a ushort from <paramref name="sin"/>.
		/// The input format should be little endian.
		/// It is assumed that <paramref name="sin"/> is valid and ready to go.
		/// </summary>
		/// <param name="sin">The stream to read from.</param>
		/// <returns>Returns the ushort read from <paramref name="sin"/>.</returns>
		/// <exception cref="IOException">Thrown if <paramref name="sin"/> runs out of input early or for other mysterious reasons.</exception>
		public static ushort ReadUnsignedShort(Stream sin)
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
		/// <param name="src">The ISaveable object wanting to write an short.</param>
		/// <param name="sout">The stream to write to.</param>
		/// <param name="s">The short to write.</param>
		/// <exception cref="IOException">Thrown when an IO exception occurs for mysterious reasons.</exception>
		public static void WriteUnsignedShort(this ISaveable src, Stream sout, ushort s)
		{
			Span<byte> buffer = new Span<byte>(new byte[sizeof(ushort)]);
			BinaryPrimitives.WriteUInt16LittleEndian(buffer,s);
			sout.Write(buffer);

			return;
		}

		/// <summary>
		/// Writes a ushort to <paramref name="sout"/>.
		/// The output format is in little endian.
		/// It is assumed that <paramref name="sout"/> is valid and ready to go.
		/// </summary>
		/// <param name="sout">The stream to write to.</param>
		/// <param name="s">The short to write.</param>
		/// <exception cref="IOException">Thrown when an IO exception occurs for mysterious reasons.</exception>
		public static void WriteUnsignedShort(Stream sout, ushort s)
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
		/// <param name="src">The ISaveable object wanting to read an unsigned int.</param>
		/// <param name="sin">The stream to read from.</param>
		/// <returns>Returns the unsigned int read from <paramref name="sin"/>.</returns>
		/// <exception cref="IOException">Thrown if <paramref name="sin"/> runs out of input early or for other mysterious reasons.</exception>
		public static uint ReadUnsignedInt(this ISaveable src, Stream sin)
		{
			Span<byte> buffer = new Span<byte>(new byte[sizeof(uint)]);

			if(sin.Read(buffer) != buffer.Length)
				throw new IOException("The provided stream ran out of data to read");

			return BinaryPrimitives.ReadUInt32LittleEndian(buffer);
		}

		/// <summary>
		/// Reads an unsigned int from <paramref name="sin"/>.
		/// The input format should be little endian.
		/// It is assumed that <paramref name="sin"/> is valid and ready to go.
		/// </summary>
		/// <param name="sin">The stream to read from.</param>
		/// <returns>Returns the unsigned int read from <paramref name="sin"/>.</returns>
		/// <exception cref="IOException">Thrown if <paramref name="sin"/> runs out of input early or for other mysterious reasons.</exception>
		public static uint ReadUnsignedInt(Stream sin)
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
		/// <param name="src">The ISaveable object wanting to write an unsigned int.</param>
		/// <param name="sout">The stream to write to.</param>
		/// <param name="i">The unsigned integer to write.</param>
		/// <exception cref="IOException">Thrown when an IO exception occurs for mysterious reasons.</exception>
		public static void WriteUnsignedInt(this ISaveable src, Stream sout, uint i)
		{
			Span<byte> buffer = new Span<byte>(new byte[sizeof(uint)]);
			BinaryPrimitives.WriteUInt32LittleEndian(buffer,i);
			sout.Write(buffer);

			return;
		}

		/// <summary>
		/// Writes an unsigned int to <paramref name="sout"/>.
		/// The output format is in little endian.
		/// It is assumed that <paramref name="sout"/> is valid and ready to go.
		/// </summary>
		/// <param name="sout">The stream to write to.</param>
		/// <param name="i">The unsigned integer to write.</param>
		/// <exception cref="IOException">Thrown when an IO exception occurs for mysterious reasons.</exception>
		public static void WriteUnsignedInt(Stream sout, uint i)
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
		/// <param name="src">The ISaveable object wanting to read a ulong.</param>
		/// <param name="sin">The stream to read from.</param>
		/// <returns>Returns the ulong read from <paramref name="sin"/>.</returns>
		/// <exception cref="IOException">Thrown if <paramref name="sin"/> runs out of input early or for other mysterious reasons.</exception>
		public static ulong ReadUnsignedLong(this ISaveable src, Stream sin)
		{
			Span<byte> buffer = new Span<byte>(new byte[sizeof(ulong)]);

			if(sin.Read(buffer) != buffer.Length)
				throw new IOException("The provided stream ran out of data to read");

			return BinaryPrimitives.ReadUInt64LittleEndian(buffer);
		}

		/// <summary>
		/// Reads a ulong from <paramref name="sin"/>.
		/// The input format should be little endian.
		/// It is assumed that <paramref name="sin"/> is valid and ready to go.
		/// </summary>
		/// <param name="sin">The stream to read from.</param>
		/// <returns>Returns the ulong read from <paramref name="sin"/>.</returns>
		/// <exception cref="IOException">Thrown if <paramref name="sin"/> runs out of input early or for other mysterious reasons.</exception>
		public static ulong ReadUnsignedLong(Stream sin)
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
		/// <param name="src">The ISaveable object wanting to write a ulong.</param>
		/// <param name="sout">The stream to write to.</param>
		/// <param name="l">The ulong to write.</param>
		/// <exception cref="IOException">Thrown when an IO exception occurs for mysterious reasons.</exception>
		public static void WriteUnsignedLong(this ISaveable src, Stream sout, ulong l)
		{
			Span<byte> buffer = new Span<byte>(new byte[sizeof(ulong)]);
			BinaryPrimitives.WriteUInt64LittleEndian(buffer,l);
			sout.Write(buffer);

			return;
		}

		/// <summary>
		/// Writes a ulong to <paramref name="sout"/>.
		/// The output format is in little endian.
		/// It is assumed that <paramref name="sout"/> is valid and ready to go.
		/// </summary>
		/// <param name="sout">The stream to write to.</param>
		/// <param name="l">The ulong to write.</param>
		/// <exception cref="IOException">Thrown when an IO exception occurs for mysterious reasons.</exception>
		public static void WriteUnsignedLong(Stream sout, ulong l)
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
		/// <param name="src">The ISaveable object wanting to read a float.</param>
		/// <param name="sin">The stream to read from.</param>
		/// <returns>Returns the float read from <paramref name="sin"/>.</returns>
		/// <exception cref="IOException">Thrown if <paramref name="sin"/> runs out of input early or for other mysterious reasons.</exception>
		public static float ReadFloat(this ISaveable src, Stream sin)
		{return BitConverter.Int32BitsToSingle(src.ReadInt(sin));}

		/// <summary>
		/// Reads a float from <paramref name="sin"/>.
		/// The input format should be little endian.
		/// It is assumed that <paramref name="sin"/> is valid and ready to go.
		/// </summary>
		/// <param name="sin">The stream to read from.</param>
		/// <returns>Returns the float read from <paramref name="sin"/>.</returns>
		/// <exception cref="IOException">Thrown if <paramref name="sin"/> runs out of input early or for other mysterious reasons.</exception>
		public static float ReadFloat(Stream sin)
		{return BitConverter.Int32BitsToSingle(ReadInt(sin));}

		/// <summary>
		/// Writes a float to <paramref name="sout"/>.
		/// The output format is in little endian.
		/// It is assumed that <paramref name="sout"/> is valid and ready to go.
		/// </summary>
		/// <param name="src">The ISaveable object wanting to write a float.</param>
		/// <param name="sout">The stream to write to.</param>
		/// <param name="f">The float to write.</param>
		/// <exception cref="IOException">Thrown when an IO exception occurs for mysterious reasons.</exception>
		public static void WriteFloat(this ISaveable src, Stream sout, float f)
		{
			src.WriteInt(sout,BitConverter.SingleToInt32Bits(f));
			return;
		}

		/// <summary>
		/// Writes a float to <paramref name="sout"/>.
		/// The output format is in little endian.
		/// It is assumed that <paramref name="sout"/> is valid and ready to go.
		/// </summary>
		/// <param name="sout">The stream to write to.</param>
		/// <param name="f">The float to write.</param>
		/// <exception cref="IOException">Thrown when an IO exception occurs for mysterious reasons.</exception>
		public static void WriteFloat(Stream sout, float f)
		{
			WriteInt(sout,BitConverter.SingleToInt32Bits(f));
			return;
		}
		#endregion

		#region DOUBLES
		/// <summary>
		/// Reads a double from <paramref name="sin"/>.
		/// The input format should be little endian.
		/// It is assumed that <paramref name="sin"/> is valid and ready to go.
		/// </summary>
		/// <param name="src">The ISaveable object wanting to read a double.</param>
		/// <param name="sin">The stream to read from.</param>
		/// <returns>Returns the double read from <paramref name="sin"/>.</returns>
		/// <exception cref="IOException">Thrown if <paramref name="sin"/> runs out of input early or for other mysterious reasons.</exception>
		public static double ReadDouble(this ISaveable src, Stream sin)
		{return BitConverter.Int64BitsToDouble(src.ReadLong(sin));}

		/// <summary>
		/// Reads a double from <paramref name="sin"/>.
		/// The input format should be little endian.
		/// It is assumed that <paramref name="sin"/> is valid and ready to go.
		/// </summary>
		/// <param name="sin">The stream to read from.</param>
		/// <returns>Returns the double read from <paramref name="sin"/>.</returns>
		/// <exception cref="IOException">Thrown if <paramref name="sin"/> runs out of input early or for other mysterious reasons.</exception>
		public static double ReadDouble(Stream sin)
		{return BitConverter.Int64BitsToDouble(ReadLong(sin));}

		/// <summary>
		/// Writes a double to <paramref name="sout"/>.
		/// The output format is in little endian.
		/// It is assumed that <paramref name="sout"/> is valid and ready to go.
		/// </summary>
		/// <param name="src">The ISaveable object wanting to write a double.</param>
		/// <param name="sout">The stream to write to.</param>
		/// <param name="d">The double to write.</param>
		/// <exception cref="IOException">Thrown when an IO exception occurs for mysterious reasons.</exception>
		public static void WriteDouble(this ISaveable src, Stream sout, double d)
		{
			src.WriteLong(sout,BitConverter.DoubleToInt64Bits(d));
			return;
		}

		/// <summary>
		/// Writes a double to <paramref name="sout"/>.
		/// The output format is in little endian.
		/// It is assumed that <paramref name="sout"/> is valid and ready to go.
		/// </summary>
		/// <param name="sout">The stream to write to.</param>
		/// <param name="d">The double to write.</param>
		/// <exception cref="IOException">Thrown when an IO exception occurs for mysterious reasons.</exception>
		public static void WriteDouble(Stream sout, double d)
		{
			WriteLong(sout,BitConverter.DoubleToInt64Bits(d));
			return;
		}
		#endregion

		#region STRINGS
		/// <summary>
		/// Reads a string from <paramref name="sin"/>.
		/// The input format should be an integer specifying the length of the string followed by the string itself.
		/// It is assumed that <paramref name="sin"/> is valid and ready to go.
		/// </summary>
		/// <param name="src">The ISaveable object wanting to read a string.</param>
		/// <param name="sin">The stream to read from.</param>
		/// <returns>Returns the string read or null if the proposed length of the string is less than zero.</returns>
		/// <exception cref="IOException">Thrown if <paramref name="sin"/> runs out of input early or for other mysterious reasons.</exception>
		public static string? ReadString(this ISaveable src, Stream sin)
		{
			int len = src.ReadInt(sin);

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
		/// Reads a string from <paramref name="sin"/>.
		/// The input format should be an integer specifying the length of the string followed by the string itself.
		/// It is assumed that <paramref name="sin"/> is valid and ready to go.
		/// </summary>
		/// <param name="sin">The stream to read from.</param>
		/// <returns>Returns the string read or null if the proposed length of the string is less than zero.</returns>
		/// <exception cref="IOException">Thrown if <paramref name="sin"/> runs out of input early or for other mysterious reasons.</exception>
		public static string? ReadString(Stream sin)
		{
			int len = ReadInt(sin);

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
		/// <param name="src">The ISaveable object wanting to write a string.</param>
		/// <param name="sout">The stream to write to.</param>
		/// <param name="str">The string to write.</param>
		/// <exception cref="IOException">Thrown when an IO exception occurs for mysterious reasons.</exception>
		public static void WriteString(this ISaveable src, Stream sout, string str)
		{
			if(str == null)
			{
				src.WriteInt(sout,-1);
				return;
			}

			src.WriteInt(sout,str.Length);

			Span<byte> buffer = new Span<byte>(new byte[str.Length * sizeof(char)]);
			
			for(int i = 0;i < str.Length;i++)
				BinaryPrimitives.WriteUInt16LittleEndian(buffer.Slice(i * sizeof(char),sizeof(char)),str[i]);

			sout.Write(buffer);
			return;
		}

		/// <summary>
		/// Writes a string to <paramref name="sout"/>.
		/// The output format is an integer specifying the length of the string followed by the string itself.
		/// It is assumed that <paramref name="sout"/> is valid and ready to go.
		/// </summary>
		/// <param name="sout">The stream to write to.</param>
		/// <param name="str">The string to write.</param>
		/// <exception cref="IOException">Thrown when an IO exception occurs for mysterious reasons.</exception>
		public static void WriteString(Stream sout, string str)
		{
			if(str == null)
			{
				WriteInt(sout,-1);
				return;
			}

			WriteInt(sout,str.Length);

			Span<byte> buffer = new Span<byte>(new byte[str.Length * sizeof(char)]);
			
			for(int i = 0;i < str.Length;i++)
				BinaryPrimitives.WriteUInt16LittleEndian(buffer.Slice(i * sizeof(char),sizeof(char)),str[i]);

			sout.Write(buffer);
			return;
		}
		#endregion
	}
}