using GameEngine.Utility.ExtensionMethods.PrimitiveExtensions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameEngine.Utility.Randomness
{
	/// <summary>
	/// A unified implementation of random number generation
	/// </summary>
	[JsonConverter(typeof(MyRandomConverter))]
	public class Random
	{
		/// <summary>
		/// Creates a random number generator seeded with the current tick time.
		/// </summary>
		public Random() : this((ulong)DateTime.Now.Ticks)
		{return;}

		/// <summary>
		/// Creates a random number generator seeded with <paramref name="seed"/>.
		/// </summary>
		/// <param name="seed">The seed to use.</param>
		public Random(ulong seed)
		{
			Seed = seed;
			return;
		}

		/// <summary>
		/// Creates a random number generator in the middle of its prime.
		/// </summary>
		/// <param name="seed">The seed value on record.</param>
		/// <param name="next">The next value.</param>
		public Random(ulong seed, ulong next)
		{
			_s = seed;
			Next = next;

			return;
		}

		/// <summary>
		/// Obtains a random signed integer.
		/// </summary>
		public uint NextUInt32()
		{
			uint ret;

			unchecked
			{ret = (uint)(NextUInt64() / (ulong.MaxValue / uint.MaxValue));}

			return ret;
		}

		/// <summary>
		/// Obtains a random signed integer.
		/// </summary>
		public int NextInt32() => (int)NextUInt32();

		/// <summary>
		/// Obtains a random integer within [<paramref name="min"/>,<paramref name="max"/>).
		/// </summary>
		/// <param name="min">The inclusive lower bound.</param>
		/// <param name="max">The exclusive upper bound.</param>
		/// <exception cref="ArgumentException">Thrown if <paramref name="max"/> is at most <paramref name="min"/>.</exception>
		public int NextInt32(int min, int max)
		{
			if(max <= min)
				throw new ArgumentException();
			
			int diff = max - min;
			int rng = NextInt32() % diff;

			if(rng < 0)
				rng += diff;

			return min + rng;
		}

		/// <summary>
		/// Obtains a random unsigned long.
		/// </summary>
		/// <remarks>This is the base randomness call from which all other calls derive.</remarks>
		public ulong NextUInt64()
		{
			unchecked
			{Next = Next * 1103515245Lu + 12345Lu;}
			
			return Next;
		}

		/// <summary>
		/// The next seed value.
		/// </summary>
		public ulong Next
		{get; protected set;}

		/// <summary>
		/// The seed value of this random number generator.
		/// </summary>
		public ulong Seed
		{
			get => _s;
			
			set
			{
				_s = value;
				Next = _s;

				return;
			}
		}

		private ulong _s;
	}

	/// <summary>
	/// Converts a randomness itself to/from a JSON format.
	/// </summary>
	public class MyRandomConverter : JsonConverter<Random>
	{
		public override Random Read(ref Utf8JsonReader reader, Type type_to_convert, JsonSerializerOptions ops)
		{
			// We start with the object opening
			if(!reader.HasNextObjectStart())
				throw new JsonException();
			
			reader.Read();

			// We'll need to track what properties we've already done, though this is not strictly necessary
			HashSet<string> processed = new HashSet<string>();

			// Create a place to store the stuff we read
			ulong seed = 0Lu;
			ulong next = 0Lu;

			// Loop until we reach the end of the object
			while(!reader.HasNextObjectEnd())
			{
				if(!reader.HasNextProperty())
					throw new JsonException();

				string property_name = reader.GetString()!;
				reader.Read();

				if(processed.Contains(property_name))
					throw new JsonException();

				switch(property_name)
				{
				case "Next":
					if(!reader.HasNextNumber())
						throw new JsonException();

					next = reader.GetUInt64();
					break;
				case "Seed":
					if(!reader.HasNextNumber())
						throw new JsonException();

					seed = reader.GetUInt64();
					break;
				default:
					throw new JsonException();
				}

				processed.Add(property_name);
				reader.Read();
			}

			// Check that we read everything we need
			if(processed.Count != 2)
				throw new JsonException();

			return new Random(seed,next);
		}

		public override void Write(Utf8JsonWriter writer, Random value, JsonSerializerOptions ops)
		{
			writer.WriteStartObject();

			writer.WriteNumber("Next",value.Next);
			writer.WriteNumber("Seed",value.Seed);

			writer.WriteEndObject();

			return;
		}
	}
}
