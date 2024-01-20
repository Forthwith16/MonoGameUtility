using Microsoft.Xna.Framework;

namespace ExampleDrawImage
{
	/// <summary>
	/// Static classes in C# are, unlike Java, just a collection of static things (methods, properties, variables, etc) and have no relation to inner/outer classes.
	/// In other words, it is exactly the same as a nonstatic class with the sole exception that it cannot be instantiated.
	/// There is only every the single copy of it in existence.
	/// </summary>
	public static class Program
	{
		/// <summary>
		/// The entry point into a C# program.
		/// Just like other languages, it is called Main (with a capital M) and takes an array of string command line arguments.
		/// </summary>
		/// <param name="args">The command line arguments.</param>
		public static void Main(string[] args)
		{
			// The using keyword in C# inside code can be used with IDisposable objects to automatically dispose of them when they fall out of scope
			using Game game = new ExampleDrawImageGame();

			// Games are runnable
			// You must call their Run method for them to do anything after construction
			game.Run();

			return;
		}
	}
}