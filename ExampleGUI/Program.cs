using Microsoft.Xna.Framework;

namespace ExampleGUI
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			using Game game = new ExampleGUIGame();
			game.Run();

			return;
		}
	}
}