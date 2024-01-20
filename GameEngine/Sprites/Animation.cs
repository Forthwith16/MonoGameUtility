using GameEngine.Exceptions;
using GameEngine.Framework;
using GameEngine.GameComponents;
using GameEngine.Maths;
using GameEngine.Utility.ExtensionMethods.InterfaceFunctions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngine.Sprites
{
	/// <summary>
	/// Encapsulates an animation.
	/// <para/>
	/// Although this class <i>can</i> be added to a Game, it is generally best for an object utilitizing it to manage it manually.
	/// This will include calling its Initialize, Update, and Dispose functions.
	/// </summary>
	public class Animation : IBlueprint
	{
		/// <summary>
		/// Creates an Animation that represents an Animation2D.
		/// </summary>
		/// <param name="src">The sprite sheet holding the texture information for this animation.</param>
		/// <param name="frame_lens">The durations (in seconds) of each frame.</param>
		/// <param name="tex_index">The indices into <paramref name="src"/> for each frame's texture information.</param>
		/// <param name="transformations">The (affine) transformations of each frame.</param>
		/// <param name="loop">If true, this animation will loop. If false, it will not.</param>
		/// <param name="loop_start">This is the start of the loop. Even if <paramref name="loop"/> is false, this must be less than <paramref name="loop_end"/>.</param>
		/// <param name="loop_end">This is the end of the loop. Even if <paramref name="loop"/> is false, this must be greater than <paramref name="loop_start"/>.</param>
		/// <exception cref="AnimationFormatException">Thrown if the provided information does not fully define an Animation2D.</exception>
		public Animation(SpriteSheet src, IEnumerable<float> frame_lens, IEnumerable<int> tex_index, IEnumerable<Matrix2D> transformations, bool loop = false, float loop_start = 0.0f, float loop_end = float.MaxValue)
		{
			// We don't want someone to be able to stealth change this animation on us, so we create our own enumerables
			List<float> f = new List<float>(frame_lens);
			List<int> t = new List<int>(tex_index);
			List<Matrix2D> m = new List<Matrix2D>(transformations);

			// Check that the frame counts match
			if(f.Count != t.Count || f.Count != m.Count || f.IsEmpty())
				throw new AnimationFormatException("The number of frames provided must be positive and must match the number of source images and transformations provided");

			// Check that the source textures all exist
			foreach(int index in t)
				if(index < 0 || index >= src.Count)
					throw new AnimationFormatException("The animation required a texture source that did not exist");
			
			// Check that the durations are all nontrivial
			float end = 0.0f;

			foreach(float d in f)
			{
				end += d;

				if(d <= 0.0f)
					throw new AnimationFormatException("The animation specified a frame of nonpositive duration");
			}

			// Check that the loop values are sane
			if(loop_end <= loop_start || loop_start < 0.0f || loop_end > end)
				throw new AnimationFormatException("Loops must end after they start and must fall within the time domain");

			// The animation is valid, so we can go ahead and assign our variables
			Source = src;
			FrameTimes = f;
			FrameIndices = t;
			FrameTransformations = m;
			FrameCount = f.Count;

			Loops = loop;
			LoopStart = loop_start;
			LoopEnd = loop_end;

			IsAnimation2D = true;
			return;
		}

		public DrawableAffineComponent ToDrawable()
		{
			if(BlueprintSingleton is null)
				throw new InvalidOperationException("The singleton drawable instance has not yet been created");

			return BlueprintSingleton;
		}

		public DrawableAffineComponent ToDrawable(Game game, SpriteBatch? renderer, bool singleton = false)
		{
			if(IsAnimation2D)
			{
				if(singleton)
					if(BlueprintSingleton is null)
						return BlueprintSingleton = ToDrawable(game,renderer,false);
					else
						return BlueprintSingleton;

				return new AnimatedComponent(game,renderer,new Animation2D(this));
			}

			throw new AnimationFormatException("This animation is invalid.");
		}

		/// <summary>
		/// If true, this Animation represents an Animation2D and is fully valid.
		/// </summary>
		public bool IsAnimation2D
		{get; init;}

		/// <summary>
		/// The sprite sheet we use for the animation frames.
		/// <para/>
		/// This is only defined when this represents an Animation2D.
		/// </summary>
		public SpriteSheet? Source
		{get; init;}

		/// <summary>
		/// The duration (in seconds) of each animation frame.
		/// <para/>
		/// This is only defined when this represents an Animation2D.
		/// </summary>
		public IEnumerable<float>? FrameTimes
		{get; init;}

		/// <summary>
		/// The indices of the frames' source texture data within Source.
		/// <para/>
		/// This is only defined when this represents an Animation2D.
		/// </summary>
		public IEnumerable<int>? FrameIndices
		{get; init;}

		/// <summary>
		/// The (affine) transformations for each of the frames.
		/// <para/>
		/// This is only define when this represents an Animation2D.
		/// </summary>
		public IEnumerable<Matrix2D>? FrameTransformations
		{get; init;}

		/// <summary>
		/// The number of frames.
		/// </summary>
		public int FrameCount
		{get; init;}

		/// <summary>
		/// If true, this animation loops.
		/// If false, this animation plays only once.
		/// </summary>
		public bool Loops
		{get; init;}

		/// <summary>
		/// The time when the loop starts.
		/// <para/>
		/// This value has meaning even if Loops is false as it permits a default loop start if looping is enabled.
		/// </summary>
		public float LoopStart
		{get; init;}

		/// <summary>
		/// The time when the loop ends.
		/// <para/>
		/// This value has meaning even if Loops is false as it permits a default loop end if looping is enabled.
		/// </summary>
		public float LoopEnd
		{get; init;}

		/// <summary>
		/// The singleton instance of the drawable version of this object to return from ToDrawable.
		/// </summary>
		protected DrawableAffineComponent? BlueprintSingleton
		{get; set;}
	}
}
