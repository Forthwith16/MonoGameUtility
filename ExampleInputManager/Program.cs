using Microsoft.Xna.Framework;

namespace ExampleInputManager
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			using Game game = new ExampleInputManagerGame();
			game.Run();

			return;
		}
	}
}