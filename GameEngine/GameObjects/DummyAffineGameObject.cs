using GameEngine.Framework;

namespace GameEngine.GameObjects
{
	/// <summary>
	/// This is a dummy affine game object meant to do nothing but fill space.
	/// </summary>
	/// <remarks>This does not care what Game it belongs to.</remarks>
	public sealed class DummyAffineGameObject : DrawableAffineObject
	{
		/// <summary>
		/// Creates a dummy game object.
		/// </summary>
		/// <param name="w">The width of this dummy game object.</param>
		/// <param name="w">The height of this dummy game object.</param>
		public DummyAffineGameObject(int w = 0, int h = 0) : base()
		{
			DummyWidth = w;
			DummyHeight = h;

			return;
		}

		/// <summary>
		/// Makes a (sorta) deep copy of <paramref name="other"/>.
		/// <list type="bullet">
		///	<item>This will have a fresh ID.</item>
		///	<item>This will have the same parent as <paramref name="other"/>, but it will leave Children unpopulated.</item>
		///	<item>This will not match the initialization/disposal state of <paramref name="other"/>. It will be uninitialized.</item>
		///	<item>This will not copy event handlers.</item>
		/// </list>
		/// Note that this will not initialize, dispose, or otherwise modify <paramref name="other"/>.
		/// </summary>
		public DummyAffineGameObject(DummyAffineGameObject other) : base(other)
		{
			DummyWidth = other.DummyWidth;
			DummyHeight = other.DummyHeight;

			return;
		}

		public override int Width => DummyWidth;
		
		/// <summary>
		/// The dummy width of this dummy game object.
		/// </summary>
		public int DummyWidth
		{get; set;}

		public override int Height => DummyHeight;
		
		/// <summary>
		/// The dummy height of this dummy game object.
		/// </summary>
		public int DummyHeight
		{get; set;}
	}
}
