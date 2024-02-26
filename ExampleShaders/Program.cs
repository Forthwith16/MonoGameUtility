using Microsoft.Xna.Framework;

namespace ExampleShaders
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			using Game game = new ExampleShadersGame();
			game.Run();

			return;
		}
	}
}