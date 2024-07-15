using GameEngine.Framework;
using GameEngine.Utility.ExtensionMethods.PrimitiveExtensions;
using Microsoft.Xna.Framework;

namespace GameEngine.GUI.Components
{
	/// <summary>
	/// A shield is a GUI element with no purpose other than to soak up clicks that do nothing.
	/// This allows the user to shield GUI elements underneath it from meaningful clicks.
	/// </summary>
	public class Shield : GUIBase
	{
		/// <summary>
		/// Creates a new shield of no dimensions.
		/// </summary>
		/// <param name="game">The game this shield will belong to.</param>
		/// <param name="name">The name of this GUI component.</param>
		public Shield(RenderTargetFriendlyGame game, string name) : base(game,name)
		{
			_width = 0;
			_height = 0;

			return;
		}

		/// <summary>
		/// Creates a new shield of no dimensions.
		/// </summary>
		/// <param name="game">The game this shield will belong to.</param>
		/// <param name="name">The name of this GUI component.</param>
		/// <param name="w">The width of the shield.</param>
		/// <param name="h">The height of the shield.</param>
		public Shield(RenderTargetFriendlyGame game, string name, int w, int h) : base(game,name)
		{
			_width = Width;
			_height = Height;

			return;
		}

		public override bool Contains(Vector2 pos, out IGUI? component, bool include_children = true)
		{
			// Transform pos into the local coordinate system
			pos = InverseWorld * pos;

			// We need to calculate containment within an arbitrarily rotated, translated, and scaled rectangle
			// The obvious choice for doing this is to just make sure we're all on the 'inside' of the lines defined by the four corners of the untransformed boundary
			int W = Width; // Who knows how long these take to calculate
			int H = Height;

			Vector2 TL = new Vector2(0,0); // Screen coorinates (and thus world coordinates) put the origin at the top left
			Vector2 TR = new Vector2(W,0);
			Vector2 BL = new Vector2(0,H);
			Vector2 BR = new Vector2(W,H);

			// To check that we're on the 'inside', we need only check that the cross product points up clockwise (screen coordinates are left-handed)
			if((TR - TL).Cross(pos - TL) >= 0.0f && (BR - TR).Cross(pos - TR) >= 0.0f && (BL - BR).Cross(pos - BR) >= 0.0f && (TL - BL).Cross(pos - BL) >= 0.0f)
			{
				component = this;
				return true;
			}
			
			component = null;
			return false;
		}

		protected override void DrawAddendum(GameTime delta)
		{return;}

		protected override void LoadAdditionalContent()
		{return;}

		protected override void UpdateAddendum(GameTime delta)
		{return;}

		/// <summary>
		/// Sets the dimensions of the shield.
		/// </summary>
		/// <param name="width">The new width. If this is not at least 0, the width will not change.</param>
		/// <param name="height">The new height. If this is not at least 0, the height will not change.</param>
		public void SetDimensions(int width, int height)
		{
			if(width >= 0)
				_width = width;

			if(height >= 0)
				_height = height;

			return;
		}

		public override int Width => _width;
		protected int _width;

		public override int Height => _height;
		protected int _height;

		public override Rectangle Bounds
		{
			get
			{
				// We need to know where the four corners end up as the most extreme points
				// Then we can pick the min and max values to get a bounding rectangle
				int W = Width; // Who knows how long these take to calculate
				int H = Height;

				Vector2 TL = World * new Vector2(0,0); // Screen coorinates (and thus world coordinates) put the origin at the top left
				Vector2 TR = World * new Vector2(W,0);
				Vector2 BL = World * new Vector2(0,H);
				Vector2 BR = World * new Vector2(W,H);

				float left = MathF.Min(MathF.Min(TL.X,TR.X),MathF.Min(BL.X,BR.X));
				float right = MathF.Max(MathF.Max(TL.X,TR.X),MathF.Max(BL.X,BR.X));
				float top = MathF.Min(MathF.Min(TL.Y,TR.Y),MathF.Min(BL.Y,BR.Y));
				float bottom = MathF.Max(MathF.Max(TL.Y,TR.Y),MathF.Max(BL.Y,BR.Y));

				return new Rectangle((int)left,(int)top,(int)MathF.Ceiling(right - left),(int)MathF.Ceiling(bottom - top));
			}
		}
	}
}
