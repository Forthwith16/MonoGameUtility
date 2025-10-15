using GameEngine.Utility.ExtensionMethods.ClassExtensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using TRead = GameEngine.Resources.Sprites.SpriteSheet;

namespace GameEnginePipeline.Readers.Sprites
{
	/// <summary>
	/// Transforms content into its game object form.
	/// </summary>
	public sealed class SpriteSheetReader : ContentTypeReader<TRead>
	{
		/// <summary>
		/// Reads a sprite sheet into the game.
		/// </summary>
		/// <param name="cin">The source of content data.</param>
		/// <param name="existingInstance">The existence instance of this content (if any).</param>
		/// <returns>Returns the sprite sheet specified by <paramref name="cin"/> or <paramref name="existingInstance"/> if we've already created an instance of this source asset.</returns>
		protected override TRead Read(ContentReader cin, TRead? existingInstance)
		{
			// We insist that sprite sheets are unique for each source
			if(existingInstance is not null)
				return existingInstance;

			// First read the sprite sheet's name
			string name = cin.ReadString();

			// Next get the source texture
			Texture2D source = cin.ReadExternalReference<Texture2D>();

			// We append the source's extension if it doesn't already have it from a Load elsewhere
			string extension = cin.ReadString();
			
			if(!Path.HasExtension(source.Name))
				source.Name += extension;

			// Now check if we're tiling or not
			bool tile = cin.ReadBoolean();

			// We'll need this either way
			List<Rectangle> sprites = new List<Rectangle>();

			if(tile)
			{
				// Read in the tiling data first
				bool row_first = cin.ReadBoolean();
				int vcount = cin.ReadInt32();
				int hcount = cin.ReadInt32();
				int count = cin.ReadInt32();
				int width = cin.ReadInt32();
				int height = cin.ReadInt32();

				if(row_first)
					for(int y = 0,c = 0;c < count;y++)
						for(int x = 0;x < hcount && c < count;x++,c++)
							sprites.Add(new Rectangle(x * width,y * height,width,height));
				else
					for(int x = 0,c = 0;c < count;x++)
						for(int y = 0;y < vcount && c < count;y++,c++)
							sprites.Add(new Rectangle(x * width,y * height,width,height));
			}
			else
			{
				// Read how many rectangles we have
				int num = cin.ReadInt32();

				// Now read in each rectangle
				for(int i = 0;i < num; i++)
					sprites.Add(cin.ReadRectangle());
			}

			return new TRead(name,source,sprites);
		}
	}
}
