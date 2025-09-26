using GameEngine.Texture;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngine.GameObjects
{
	/// <summary>
	/// A game object that generates a texture and draws it as a rectangle.
	/// </summary>
	public class RectangleGameObject : ImageGameObject
	{
		/// <summary>
		/// Creates a rectangle game object with a solid color.
		/// </summary>
		/// <param name="renderer">The image renderer to draw with (can be changed later).</param>
		/// <param name="w">The width of the image generated.</param>
		/// <param name="h">The height of the image generated.</param>
		/// <param name="c">The color to draw with.</param>
		public RectangleGameObject(SpriteBatch? renderer, int w, int h, Color c) : this(renderer,w,h,ColorFunctions.SolidColor(c))
		{return;}
		
		/// <summary>
		/// Creates a rectangle game object with a textured generated according to <paramref name="func"/>.
		/// </summary>
		/// <param name="renderer">The image renderer to draw with (can be changed later).</param>
		/// <param name="w">The width of the image generated.</param>
		/// <param name="h">The height of the image generated.</param>
		/// <param name="func">The color generating function.</param>
		public RectangleGameObject(SpriteBatch? renderer, int w, int h, ColorFunction func) : base(renderer)
		{
			Generator = func;

			IntendedWidth = w;
			IntendedHeight = h;

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
		/// <remarks>
		///	This will generate another rectangle texture with the same data as <paramref name="other"/>.
		///	It may be best to use a regulare ImageGameObject using the same source, although this will still dispose of the source when it is disposed of.
		/// </remarks>
		public RectangleGameObject(RectangleGameObject other) : base(other)
		{
			Generator = other.Generator;

			IntendedWidth = other.IntendedWidth;
			IntendedHeight = other.IntendedHeight;

			return;
		}

		protected override void LoadContent()
		{
			Source = ColorFunctions.GenerateTexture(Game!,IntendedWidth,IntendedHeight,Generator);
			return;
		}

		protected override void UnloadContent()
		{
			Source!.Dispose();
			return;
		}

		/// <summary>
		/// The generating color function.
		/// </summary>
		protected ColorFunction Generator
		{get;}

		/// <summary>
		/// The intended width.
		/// </summary>
		protected int IntendedWidth
		{get;}

		/// <summary>
		/// The intended height.
		/// </summary>
		protected int IntendedHeight
		{get;}
	}
}
