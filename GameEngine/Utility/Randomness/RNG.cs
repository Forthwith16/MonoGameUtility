using GameEngine.Utility.Parsing;
using System.Buffers.Binary;

namespace GameEngine.Utility.Randomness
{
	/// <summary>
	/// A custom random number generator exclusively built so that we can save it's bloody state. Looking at you, C#.
	/// <para/>
	/// Yeah, I'm calling you out. Come on. It's not that hard.
	/// I literally copy/pasted your code from the source and modified it so I can actually use it as an RNG for a game.
	/// </summary>
	public class RNG : ISaveable
	{
		/// <summary>
		/// Constructs a new RNG with a seed based on the current system time.
		/// </summary>
		public RNG() : this(Environment.TickCount)
		{return;}

		/// <summary>
		/// Constructs a new RNG with the seed <paramref name="seed"/>.
		/// </summary>
		/// <param name="seed">The seed for the RNG.</param>
		public RNG(int seed)
		{
			int ii;
			int mj, mk;
			
			int subtraction = seed == int.MinValue ? int.MaxValue : Math.Abs(seed);
			mj = MSEED - subtraction;
			_sa[55] = mj;
			mk = 1;

			for(int i = 1;i < 55;i++)
			{
				ii = 21 * i % 55;
				_sa[ii] = mk;
				mk = mj - mk;

				if(mk < 0)
					mk += MBIG;
				
				mj = _sa[ii];
			}

			for(int k = 1;k < 5;k++)
				for(int i = 1;i < 56;i++)
				{
					_sa[i] -= _sa[1 + (i + 30) % 55];
					
					if(_sa[i] < 0)
						_sa[i] += MBIG;
				}

			INext = 0;
			INext_p = 21;

			return;
		}

		/// <summary>
		/// Constructs a new RNG using the provided state parameters.
		/// Each matches an internal state variable.
		/// </summary>
		/// <param name="_inext">Used for the next random number.</param>
		/// <param name="_indext_p">Used for the next random number.</param>
		/// <param name="seed_array">An array of length 56 for the seed parameters.</param>
		/// <exception cref="ArgumentException">Thrown if <paramref name="seed_array"/> is not of length 56.</exception>
		public RNG(int _inext, int _indext_p, int[] seed_array)
		{
			if(seed_array.Length != 56)
				throw new ArgumentException("seed_array","The seed array must contain exactly 56 elements.");

			INext = _inext;
			INext_p = _indext_p;

			seed_array.CopyTo(_sa,0);
			return;
		}

		/// <summary>
		/// Creates a copy of an RNG with the same random state.
		/// </summary>
		/// <param name="rand">The RNG to copy.</param>
		public RNG(RNG rand)
		{
			INext = rand.INext;
			INext_p = rand.INext_p;

			rand._sa.CopyTo(_sa,0);
			return;
		}

		/// <summary>
		/// Reads in an RNG state from <paramref name="sin"/>.
		/// </summary>
		/// <param name="sin">The stream to read the RNG state from.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="sout"/> is null.</exception>
		/// <exception cref="NotSupportedException">Thrown if <paramref name="sout"/> does not support reading.</exception>
		/// <exception cref="ObjectDisposedException">Thrown if the stream was already closed.</exception>
		/// <exception cref="IOException">Thrown when an IO exception occurs for mysterious reasons.</exception>
		/// <remarks>The format should provide INext, INext_p, and then the 56 elements of the seed array stored in little endian order with no data in between each value.</remarks>
		public RNG(Stream sin)
		{
			if(sin is null)
				throw new ArgumentNullException("sin","The stream must not be null");

			if(!sin.CanRead)
				throw new NotSupportedException("The provided stream cannot be read");

			// One read is more efficient, so let's skip the nice extension functions
			Span<byte> buffer = new Span<byte>(new byte[sizeof(int) * 58]);
			
			if(sin.Read(buffer) != buffer.Length)
				throw new IOException("The provided stream ran out of data to read");

			INext = BinaryPrimitives.ReadInt32LittleEndian(buffer);
			INext_p = BinaryPrimitives.ReadInt32LittleEndian(buffer.Slice(sizeof(int))); // Slice keeps the same backing buffer

			for(int i = 0; i < _sa.Length;i++)
				_sa[i] = BinaryPrimitives.ReadInt32LittleEndian(buffer.Slice(sizeof(int) * (2 + i)));
			
			return;
		}

		public virtual void WriteToStream(Stream sout)
		{
			if(sout is null)
				throw new ArgumentNullException("sout","The stream cannot be null");

			if(!sout.CanWrite)
				throw new NotSupportedException("The provided stream cannot be written to");

			// One write is more efficient, so let's skip the nice extension functions
			Span<byte> buffer = new Span<byte>(new byte[sizeof(int) * 58]);

			BinaryPrimitives.WriteInt32LittleEndian(buffer,INext);
			BinaryPrimitives.WriteInt32LittleEndian(buffer.Slice(sizeof(int),sizeof(int)),INext_p);

			for(int i = 0;i < _sa.Length;i++)
				BinaryPrimitives.WriteInt32LittleEndian(buffer.Slice(sizeof(int) * (2 + i),sizeof(int)),_sa[i]);

			sout.Write(buffer);
			return;
		}

		/// <summary>
		/// Returns a random floating-point number between 0.0 and 1.0.
		/// </summary>
		/// <returns>A double-precision floating point number that is greater than or equal to 0.0, and less than 1.0.</returns>
		protected virtual double Sample()
		{return InternalSample() * (1.0 / MBIG);} // Including this division at the end gives us significantly improved random number distribution

		protected int InternalSample()
		{
			int retVal;
			int locINext = INext;
			int locINextp = INext_p;

			if(++locINext >= 56)
				locINext = 1;

			if(++locINextp >= 56)
				locINextp = 1;

			retVal = _sa[locINext] - _sa[locINextp];

			if(retVal == MBIG)
				retVal--;

			if(retVal < 0)
				retVal += MBIG;

			_sa[locINext] = retVal;

			INext = locINext;
			INext_p = locINextp;

			return retVal;
		}

		/// <summary>
		/// Returns a non-negative random integer.
		/// </summary>
		/// <returns>A 32-bit signed integer that is greater than or equal to 0 and less than Int32.MaxValue.</returns>
		public virtual int Next()
		{return InternalSample();}

		/// <summary>
		/// The distribution of double value returned by Sample is not distributed well enough for a large range.
		/// If we use Sample for a range [Int32.MinValue..Int32.MaxValue) we will end up getting even numbers only.
		/// </summary>
		protected double GetSampleForLargeRange()
		{
			int result = InternalSample();
			
			// Note we can't use addition here
			// The distribution will be bad if we do that
			bool negative = InternalSample() % 2 == 0 ? true : false; // decide the sign based on second sample
			
			if(negative)
				result = -result;

			double d = result;
			d += int.MaxValue - 1; // get a number in range [0 .. 2 * Int32MaxValue - 1)
			d /= 2 * (uint)int.MaxValue - 1;

			return d;
		}

		/// <summary>
		/// Returns a random integer that is within a specified range.
		/// </summary>
		/// <param name="min_value">The inclusive lower bound of the random number returned.</param>
		/// <param name="max_value">The exclusive upper bound of the random number returned. <paramref name="max_value"/> must be greater than or equal to <paramref name="min_value"/>.</param>
		/// <returns>A 32-bit signed integer greater than or equal to <paramref name="min_value"/> and less than <paramref name="max_value"/>; that is, the range of return values includes <paramref name="min_value"/> but not <paramref name="max_value"/>. If <paramref name="min_value"/> equals <paramref name="max_value"/>, <paramref name="min_value"/> is returned.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="min_value"/> is greater than <paramref name="max_value"/>.</exception>
		public virtual int Next(int min_value, int max_value)
		{
			if(min_value > max_value)
				throw new ArgumentOutOfRangeException("min_value","The minimum value must not be greater than the maximum value");

			long range = (long)max_value - min_value;
			
			if(range <= int.MaxValue)
				return (int)(Sample() * range) + min_value;
			
			return (int)((long)(GetSampleForLargeRange() * range) + min_value);
		}

		/// <summary>
		/// Returns a non-negative random integer that is less than the specified maximum.
		/// </summary>
		/// <param name="max_value">The exclusive upper bound of the random number to be generated. <paramref name="max_value"/> must be greater than or equal to 0.</param>
		/// <returns>A 32-bit signed integer that is greater than or equal to 0, and less than <paramref name="max_value"/>; that is, the range of return values ordinarily includes 0 but not <paramref name="max_value"/>. However, if maxValue equals 0, <paramref name="max_value"/> is returned.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="max_value"/> is less neagtive.</exception>
		public virtual int Next(int max_value)
		{
			if(max_value < 0)
				throw new ArgumentOutOfRangeException("max_value","Cannot generate a number at least 0 and less than 0");
			
			return (int)(Sample() * max_value);
		}

		/// <summary>
		/// Returns a random floating-point number that is greater than or equal to 0.0, and less than 1.0.
		/// </summary>
		/// <returns>Returns a random floating-point number that is greater than or equal to 0.0, and less than 1.0.</returns>
		public virtual double NextDouble()
		{return Sample();}

		/// <summary>
		/// Fills the elements of a specified array of bytes with random numbers.
		/// </summary>
		/// <param name="buffer">The array to be filled with random numbers.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="buffer"/> is null.</exception>
		public virtual void NextBytes(byte[] buffer)
		{
			if(buffer == null)
				throw new ArgumentNullException("buffer cannot be null");
			
			for(int i = 0;i < buffer.Length;i++)
				buffer[i] = (byte)(InternalSample() % (byte.MaxValue + 1));

			return;
		}

		/// <summary>
		/// Used to generate the next random number.
		/// </summary>
		public int INext
		{get; protected set;}

		/// <summary>
		/// Used to generate the next random number.
		/// </summary>
		public int INext_p
		{get; protected set;}
		
		/// <summary>
		/// The seed array of this RNG.
		/// </summary>
		public int[] SeedArray => (int[])_sa.Clone();
		
		protected int[] _sa = new int[56];
		
		protected const int MBIG = int.MaxValue;
		protected const int MSEED = 161803398;
		protected const int MZ = 0;
	}
}