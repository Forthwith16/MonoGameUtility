using GameEngine.Sprites;
using Microsoft.Xna.Framework.Content;

using TRead = GameEngine.Sprites.Animation2DCollection;

namespace GameEngine.Readers
{
	/// <summary>
	/// Transforms content into its game component form.
	/// </summary>
	public class Animation2DCollectionReader : ContentTypeReader<TRead>
	{
		/// <summary>
		/// Reads an Animation2DCollection into the game.
		/// </summary>
		/// <param name="input">The source of content data.</param>
		/// <param name="existingInstance">The existence instance of this content (if any).</param>
		/// <returns>Returns the Animation2DCollection specified by <paramref name="input"/> or <paramref name="existingInstance"/> if we've already created an instance of this source asset.</returns>
		/// <exception cref="AnimationFormatException">Thrown if the animation specification is invalid.</exception>
		/// <exception cref="ContentLoadException">Thrown if the content pipeline did not contain the expected data.</exception>
		protected override TRead Read(ContentReader input, TRead existingInstance)
		{
			if(existingInstance is not null)
				return existingInstance;

			// First read in how many animations we have
			int n = input.ReadInt32();

			// Now read in each animation and its name
			LinkedList<string> names = new LinkedList<string>();
			LinkedList<Animation> animations = new LinkedList<Animation>();

			for(int i = 0;i < n;i++)
			{
				names.AddLast(input.ReadString());
				animations.AddLast(input.ReadExternalReference<Animation>());
			}

			// Pick up the idle animation name
			string idle = input.ReadString();

			// We can make our return value now
			TRead ret = new TRead(animations,names,idle);

			// Lastly, pick up the reset setting even though it's probably always going to be true
			ret.ResetOnSwitch = input.ReadBoolean();

			// Now make the collection and be done
			return ret;
		}
	}
}
