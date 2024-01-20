using GameEngine.Utility.ExtensionMethods.ClassExtensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using TRead = GameEngine.Sprites.SpriteSheet;

namespace GameEngine.Readers
{
	/// <summary>
	/// Transforms content into its game component form.
	/// </summary>
	public sealed class SpriteSheetReader : ContentTypeReader<TRead>
	{
		/// <summary>
		/// Reads a sprite sheet into the game.
		/// </summary>
		/// <param name="input">The source of content data.</param>
		/// <param name="existingInstance">The existence instance of this content (if any).</param>
		/// <returns>Returns the sprite sheet specified by <paramref name="input"/> or <paramref name="existingInstance"/> if we've already created an instance of this source asset.</returns>
		protected override TRead Read(ContentReader input, TRead existingInstance)
		{
			// We insist that sprite sheets are unique for each source
			if(existingInstance is not null)
				return existingInstance;

			// First get the source texture
			Texture2D source = input.ReadExternalReference<Texture2D>();

			// Now check if we're tiling or not
			bool tile = input.ReadBoolean();

			// We'll need this either way
			List<Rectangle> sprites = new List<Rectangle>();

			if(tile)
			{
				// Read in the tiling data first
				bool row_first = input.ReadBoolean();
				int vcount = input.ReadInt32();
				int hcount = input.ReadInt32();
				int count = input.ReadInt32();
				int width = input.ReadInt32();
				int height = input.ReadInt32();

				if(row_first)
					for(int y = 0,c = 0;c < count;y++)
						for(int x = 0;x < hcount;x++,c++)
							sprites.Add(new Rectangle(x * width,y * height,width,height));
				else
					for(int x = 0,c = 0;c < count;x++)
						for(int y = 0;y < vcount;y++,c++)
							sprites.Add(new Rectangle(x * width,y * height,width,height));
			}
			else
			{
				// Read how many rectangles we have
				int num = input.ReadInt32();

				// Now read in each rectangle
				for(int i = 0;i < num; i++)
					sprites.Add(input.ReadRectangle());
			}

			return new TRead(source,sprites);
		}
	}
}
