#pragma warning disable SYSLIB0011

using System.Runtime.Serialization.Formatters.Binary;

namespace GameEngine.Utility.ExtensionMethods.ClassExtensions
{
	/// <summary>
	/// Contains extension methods for the Random class.
	/// </summary>
	public static class RandomExtensions
	{
		/// <summary>
		/// Serializes <paramref name="rand"/> into a Random state so that it may be restored later via SeedRandom.
		/// </summary>
		/// <param name="rand">The Random to turn into a Random state.</param>
		/// <returns>Returns the byte array representing the Random state of <paramref name="rand"/>.</returns>
		[Obsolete("SaveRandom uses binary serialization, which is vulnerable to arbitrary code execution. While this is good in a game in some ways, it IS a security risk.")]
		public static byte[] SaveRandom(Random rand)
		{return new RandomState(rand).State;}

		/// <summary>
		/// Creates a Random from a Random state.
		/// </summary>
		/// <param name="state">The Random state of a Random.</param>
		/// <returns>Returns a Random initialized to the Random state specified by <paramref name="state"/>.</returns>
		[Obsolete("SeedRandom uses binary serialization, which is vulnerable to arbitrary code execution. While this is good in a game in some ways, it IS a security risk.")]
		public static Random SeedRandom(byte[] state)
		{return new RandomState(state).Rand;}

		/// <summary>
		/// Contains a Random and its Random state.
		/// </summary>
		private struct RandomState
		{
			/// <summary>
			/// Constructs a new Random state from a byte array of memory.
			/// </summary>
			/// <param name="state">The raw state of the Random.</param>
			public RandomState(byte[] state)
			{
				BinaryFormatter fmt = new BinaryFormatter();

				using(MemoryStream temp = new MemoryStream(state))
					Rand = (Random)fmt.Deserialize(temp);

				State = state;
				return;
			}

			/// <summary>
			/// Constructs a new Random state from <paramref name="rand"/>.
			/// </summary>
			/// <param name="rand">The Random to turn into a Random state.</param>
			public RandomState(Random rand)
			{
				BinaryFormatter fmt = new BinaryFormatter();

				using(MemoryStream temp = new MemoryStream())
				{
					fmt.Serialize(temp,rand);
					State = temp.ToArray();
				}

				Rand = rand;
				return;
			}

			/// <summary>
			/// The Random state in Random form.
			/// </summary>
			public Random Rand
			{get;}

			/// <summary>
			/// The state of the Random.
			/// </summary>
			public byte[] State
			{get;}
		}
	}
}