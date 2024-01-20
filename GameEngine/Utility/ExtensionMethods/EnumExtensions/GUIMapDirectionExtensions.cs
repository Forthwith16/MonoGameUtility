using GameEngine.GUI.Map;

namespace GameEngine.Utility.ExtensionMethods.EnumExtensions
{
	/// <summary>
	/// A collection of extension methods for the GUIMapDirection enum.
	/// </summary>
	public static class GUIMapDirectionExtensions
	{
		/// <summary>
		/// Reflects a GUIMapDirection so that up<-> down and left<-> right.
		/// </summary>
		/// <param name="dir">The direction to reflect.</param>
		/// <returns>Returns the opposite direction to the direction provided.</returns>
		public static GUIMapDirection Reflect(this GUIMapDirection dir)
		{
			switch(dir)
			{
			case GUIMapDirection.UP:
				return GUIMapDirection.DOWN;
			case GUIMapDirection.DOWN:
				return GUIMapDirection.UP;
			case GUIMapDirection.LEFT:
				return GUIMapDirection.RIGHT;
			case GUIMapDirection.RIGHT:
				return GUIMapDirection.LEFT;
			}

			return GUIMapDirection.NONE;
		}
	}
}
