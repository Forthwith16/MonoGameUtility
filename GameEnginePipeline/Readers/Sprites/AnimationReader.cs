using GameEngine.Assets.Sprites;
using GameEngine.Maths;
using GameEngine.Utility.ExtensionMethods.ClassExtensions;
using Microsoft.Xna.Framework.Content;

using TRead = GameEngine.Assets.Sprites.Animation;

namespace GameEnginePipeline.Readers.Sprites
{
	/// <summary>
	/// Transforms content into its game object form.
	/// </summary>
	public sealed class AnimationReader : ContentTypeReader<TRead>
	{
		/// <summary>
		/// Reads an Animation into the game.
		/// </summary>
		/// <param name="cin">The source of content data.</param>
		/// <param name="existingInstance">The existence instance of this content (if any).</param>
		/// <returns>Returns the Animation specified by <paramref name="cin"/> or <paramref name="existingInstance"/> if we've already created an instance of this source asset.</returns>
		/// <exception cref="AnimationFormatException">Thrown if the animation specification is invalid.</exception>
		/// <exception cref="ContentLoadException">Thrown if the content pipeline did not contain the expected data.</exception>
		protected override TRead Read(ContentReader cin, TRead? existingInstance)
		{
			// We insist that Animations are unique for each source (which will then be instantiated for real later)
			if(existingInstance is not null)
				return existingInstance;

			// First determine what type of animation we're reading
			switch((AnimationType)cin.ReadInt32())
			{
			case AnimationType.Animation2D:
				return ReadAnimation2D(cin);
			}

			throw new ContentLoadException("An unsupported animation type was provided");
		}

		/// <summary>
		/// Loads an Animation specifying an Animation2D.
		/// </summary>
		/// <param name="cin">The source of the content data.</param>
		/// <returns>Returns the ANimaiton specified by <paramref name="input"/>.</returns>
		/// <exception cref="AnimationFormatException">Thrown if the animation specification is invalid.</exception>
		/// <exception cref="ContentLoadException">Thrown if the content pipeline did not contain the expected data.</exception>
		private TRead ReadAnimation2D(ContentReader cin)
		{
			// First get the source sprite sheet
			SpriteSheet source = cin.ReadExternalReference<SpriteSheet>();

			// Now find out how many frames we have
			int fcount = cin.ReadInt32();

			// We read in frame lengths, frame indices, and frame transformations interleaved with each other
			int[] indices = new int[fcount];
			float[] lens = new float[fcount];
			Matrix2D[] trans = new Matrix2D[fcount];
			
			for(int i = 0; i < fcount;i++)
			{
				indices[i] = cin.ReadInt32();
				lens[i] = cin.ReadSingle();
				trans[i] = cin.ReadMatrix2DComponents();
			}

			// Grab the start time
			float start = cin.ReadSingle();

			// Now that we have all of our frame data, we just need loop data
			bool loops = cin.ReadBoolean();
			float loop_start = cin.ReadSingle();
			float loop_end = cin.ReadSingle();

			// We now have everything so we'll call it a day
			return new TRead(source,lens,indices,trans,start,loops,loop_start,loop_end);
		}
	}

	/// <summary>
	/// The types of animation that we can load.
	/// </summary>
	public enum AnimationType
	{
		Animation2D
	}
}
