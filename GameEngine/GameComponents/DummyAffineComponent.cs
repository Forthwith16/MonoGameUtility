using Microsoft.Xna.Framework;

namespace GameEngine.GameComponents
{
	/// <summary>
	/// This is a dummy affine component meant to do nothing but fill space.
	/// </summary>
	public sealed class DummyAffineComponent : DrawableAffineComponent
	{
		/// <summary>
		/// Creates a dummy game component.
		/// </summary>
		/// <param name="game">The game that this will be part of, though anything not null will do here.</param>
		/// <param name="w">The width of this dummy affine component.</param>
		/// <param name="w">The height of this dummy affine component.</param>
		public DummyAffineComponent(Game game, int w = 0, int h = 0) : base(game,null)
		{
			DummyWidth = w;
			DummyHeight = h;

			return;
		}

		public override int Width => DummyWidth;
		
		/// <summary>
		/// The dummy width of this dummy component.
		/// </summary>
		public int DummyWidth
		{get; set;}

		public override int Height => DummyHeight;
		
		/// <summary>
		/// The dummy height of this dummy component.
		/// </summary>
		public int DummyHeight
		{get; set;}
	}
}
