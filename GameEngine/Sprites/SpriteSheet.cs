using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngine.Sprites
{
	/// <summary>
	/// Encapsulates a sprite sheet drawn from a single texture.
	/// <para/>
	/// This class should be created via a ContentManager.
	/// </summary>
	public class SpriteSheet
	{
		/// <summary>
		/// Creates a new sprite sheet with <paramref name="source"/> as the source texture and <paramref name="sprites"/> specifying the source rectangle for each sprite in the sprite sheet.
		/// </summary>
		/// <param name="source">The sprite sheet's source texture.</param>
		/// <param name="sprites">The list of sprites specified by their source rectangle in <paramref name="source"/>. The contents of this will be copied and this variable discarded.</param>
		public SpriteSheet(Texture2D source, IEnumerable<Rectangle> sprites)
		{
			Source = source;
			SourceSource = null;

			_sprites = new List<Rectangle>(sprites);
			return;
		}

		/// <summary>
		/// Creates a new sprite sheet with <paramref name="source"/> as the source texture and <paramref name="sprites"/> specifying the source rectangle for each sprite in the sprite sheet.
		/// </summary>
		/// <param name="source">The sprite sheet's source texture.</param>
		/// <param name="source_path">The path to the source image that produced <paramref name="source"/>. If no such source exists, use the constructor without this instead.</param>
		/// <param name="sprites">The list of sprites specified by their source rectangle in <paramref name="source"/>. The contents of this will be copied and this variable discarded.</param>
		public SpriteSheet(Texture2D source, string source_path, IEnumerable<Rectangle> sprites)
		{
			Source = source;
			SourceSource = source_path;

			_sprites = new List<Rectangle>(sprites);
			return;
		}

		/// <summary>
		/// The source texture of the sprites.
		/// </summary>
		public Texture2D Source
		{get;}

		/// <summary>
		/// The source for <see cref="Source"/>.
		/// If null, then <see cref="Source"/> has no source file.
		/// </summary>
		public string? SourceSource
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
	}
}
