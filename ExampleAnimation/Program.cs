using Microsoft.Xna.Framework;

namespace ExampleAnimation
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			using Game game = new ExampleAnimationGame();
			game.Run();

			return;
		}
	}
}