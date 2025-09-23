using GameEngine.Utility.ExtensionMethods.PrimitiveExtensions;
using GameEngine.Utility.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameEngine.Utility.Randomness
{
	/// <summary>
	/// A unified implementation of random number generation
	/// </summary>
	[JsonConverter(typeof(JsonRandomConverter))]
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
	/// Converts randomness itself to/from a JSON format.
	/// </summary>
	public class JsonRandomConverter : JsonBaseConverter<Random>
	{
		protected override object? ReadProperty(ref Utf8JsonReader reader, string property, JsonSerializerOptions ops)
		{
			// We only have number properties, so just check it
			if(!reader.HasNextNumber())
				throw new JsonException();

			switch(property)
			{
			case "Next":
			case "Seed":
				return reader.GetUInt64();
			default:
				throw new JsonException();
			}
		}

		protected override Random ConstructT(Dictionary<string,object?> properties)
		{
			if(properties.Count != 2)
				throw new JsonException();

			return new Random((ulong)properties["Seed"]!,(ulong)properties["Next"]!);
		}

		protected override void WriteProperties(Utf8JsonWriter writer, Random value, JsonSerializerOptions ops)
		{
			writer.WriteNumber("Next",value.Next);
			writer.WriteNumber("Seed",value.Seed);

			return;
		}
	}
}
