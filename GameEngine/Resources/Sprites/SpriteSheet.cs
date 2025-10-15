using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngine.Resources.Sprites
{
	/// <summary>
	/// Encapsulates a sprite sheet drawn from a single texture.
	/// <para/>
	/// This class should be created via a ContentManager.
	/// </summary>
	public class SpriteSheet : IResource
	{
		/// <summary>
		/// Creates a new sprite sheet with <paramref name="source"/> as the source texture and <paramref name="sprites"/> specifying the source rectangle for each sprite in the sprite sheet.
		/// </summary>
		/// <param name="name">The name of the sprite sheet. This should usually be the path, relative to a content root, to the file specifying this sprite sheet (or where one would like the file to be if saved).</param>
		/// <param name="source">The sprite sheet's source texture.</param>
		/// <param name="sprites">The list of sprites specified by their source rectangle in <paramref name="source"/>. The contents of this will be copied and this variable discarded.</param>
		public SpriteSheet(string name, Texture2D source, IEnumerable<Rectangle> sprites)
		{
			ResourceName = name;
			Source = source;
			
			_sprites = new List<Rectangle>(sprites);
			return;
		}

		/// <summary>
		/// The source texture of the sprites.
		/// </summary>
		public Texture2D Source
		{get;}

		/// <summary>
		/// Obtains the source rectangle of Source for the <paramref name="index"/>th sprite.
		/// </summary>
		/// <param name="index">The index of the sprite source rectangle to fetch.</param>
		/// <exception cref="ArgumentOutOfRangeException">Throw if <paramref name="index"/> is negative or at least Count.</exception>
		/// <remarks>The Rectangle returned is a deep copy of its source value. Modifying it will not modify the sprite sheet's copy.</remarks>
		public Rectangle this[int index]
		{get => _sprites[index];} // Rectangles are structs, and structs are value types, so there's no need to deep copy this return value

		protected readonly List<Rectangle> _sprites;

		/// <summary>
		/// The number of sprites in this sprite sheet.
		/// </summary>
		public int Count => _sprites.Count;

		public string ResourceName
		{get;}
	}
}
