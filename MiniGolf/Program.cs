using Microsoft.Xna.Framework;

namespace MiniGolf
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			using Game game = new MiniGolfGame();
			game.Run();

			return;
		}
	}
}