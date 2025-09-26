using Microsoft.Xna.Framework;

namespace GameEngine.GUI.Components
{
	/// <summary>
	/// A dummy GUI component with no purpose other than to exist.
	/// </summary>
	/// <remarks>This does not care what its Game is.</remarks>
	public class DummyGUI : GUIBase
	{
		/// <summary>
		/// Creates a dummy GUI component with the given name.
		/// </summary>
		/// <param name="name">The name of the component.</param>
		public DummyGUI(string name) : base(name)
		{return;}

		public override bool Contains(Vector2 pos, out IGUI? component, bool include_children = true)
		{
			component = null;
			return false;
		}

		protected override void LoadAdditionalContent()
		{return;}

		protected override void UpdateAddendum(GameTime delta)
		{return;}

		protected override void DrawAddendum(GameTime delta)
		{return;}

		public override int Width => 0;
		public override int Height => 0;

		public override Rectangle Bounds => Rectangle.Empty;
	}
}
