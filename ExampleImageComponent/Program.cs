using Microsoft.Xna.Framework;

namespace ExampleImageComponent
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			using Game game = new ExampleImageComponentGame();
			game.Run();

			return;
		}
	}
}