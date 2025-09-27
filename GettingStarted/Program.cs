using Microsoft.Xna.Framework;

namespace GettingStarted
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			using Game game = new BounceGame();
			game.Run();

			return;
		}
	}
}