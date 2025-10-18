using GameEngine.Assets;
using GameEngine.Assets.Sprites;
using GameEngine.Exceptions;
using GameEngine.Maths;
using System.Collections;

namespace GameEngine.Resources.Sprites
{
	/// <summary>
	/// Encapsulates an animation.
	/// </summary>
	public class Animation : IResource
	{
		/// <summary>
		/// Creates an Animation that represents an Animation2D.
		/// </summary>
		/// <param name="name">The asset name.</param>
		/// <param name="src">The sprite sheet holding the texture information for this animation.</param>
		/// <param name="frame_lens">The durations (in seconds) of each frame.</param>
		/// <param name="tex_index">The indices into <paramref name="src"/> for each frame's texture information.</param>
		/// <param name="transformations">The (affine) transformations of each frame.</param>
		/// <param name="start">The start time of the animation.</param>
		/// <param name="loop">If true, this animation will loop. If false, it will not.</param>
		/// <param name="loop_start">This is the start of the loop. Even if <paramref name="loop"/> is false, this must be less than <paramref name="loop_end"/>.</param>
		/// <param name="loop_end">This is the end of the loop. Even if <paramref name="loop"/> is false, this must be greater than <paramref name="loop_start"/>.</param>
		/// <exception cref="AnimationFormatException">Thrown if the provided information does not fully define an Animation2D.</exception>
		public Animation(string name, SpriteSheet src, IEnumerable<float> frame_lens, IEnumerable<int> tex_index, IEnumerable<Matrix2D> transformations, float start = 0.0f, bool loop = false, float loop_start = 0.0f, float loop_end = float.MaxValue)
		{
			ResourceName = name;
			Source = src;
			IsAnimation2D = true;

			// We need to load our frames and error check in unison to be efficient
			IEnumerator<float> lens = frame_lens.GetEnumerator();
			IEnumerator<int> indices = tex_index.GetEnumerator();
			IEnumerator<Matrix2D> trans = transformations.GetEnumerator();

			List<Animation2DFrame> frames = new List<Animation2DFrame>();
			
			float end = 0.0f;
			int max_frame = 0;
			int moved;

			while((moved = MoveThreeNext(lens,indices,trans)) == 3)
			{
				frames.Add(new Animation2DFrame(frames.Count,end,lens.Current,indices.Current,trans.Current)); // end is currently the start of the frame
				
				// Check that the frame source index makes sense
				if(indices.Current < 0)
					throw new AnimationFormatException("A frame required a texture source that did not exist: " + indices.Current);
				else if(max_frame < indices.Current)
					max_frame = indices.Current;

				// Check that the frame duration is sensible
				if(lens.Current <= 0.0f)
					throw new AnimationFormatException("A frame specified a nonpositive duration: " + lens.Current);
				else
					end += lens.Current;
			}

			// If we have no frames, get mad
			if(frames.Count == 0)
				throw new AnimationFormatException("Animations must have at least one frame");

			// We need to everything to finish at the same time
			if(moved != 0)
				throw new AnimationFormatException("The number of frames provided was staggered across each data set, which is not allowed");

			// If a frame requires a source index that does not exist, get mad
			if(max_frame >= frames.Count)
				throw new AnimationFormatException("A frame required a texture source that did not exist: " + max_frame);

			// Check that the start and loop values are sane
			if(start < 0.0f || start > end)
				throw new AnimationFormatException("Animations cannot start before 0 or after their end");

			if(loop_end <= loop_start || loop_start < 0.0f || loop_end > end)
				throw new AnimationFormatException("Loops must end after they start and must fall within the time domain");

			// The animation is valid, so we can go ahead and assign our animation data
			// First copy the frame data into a fixed size array of the perfect length
			FrameCount = frames.Count;
			Frames = new Animation2DFrame[FrameCount];
			
			frames.CopyTo(Frames);

			// Now assign the timing info
			Start = start;

			Loops = loop;
			LoopStart = loop_start;
			LoopEnd = loop_end;
			
			return;
		}

		/// <summary>
		/// Advances three enumerators at once.
		/// Any enumerator failing to advance will not block the others from advancing.
		/// </summary>
		/// <returns>Returns the number of enumerators advanced.</returns>
		private int MoveThreeNext(IEnumerator a, IEnumerator b, IEnumerator c)
		{
			int ret = 0;

			if(a.MoveNext())
				ret++;

			if(b.MoveNext())
				ret++;
			
			if(c.MoveNext())
				ret++;

			return ret;
		}

		AssetBase? IResource.ToAsset() => new Animation2DAsset(this);

		#region Animation2D Methods
		/// <summary>
		/// Obtains the <paramref name="index"/>th frame of an Animation2D.
		/// <para/>
		/// This is only defined when this represents an Animation2D.
		/// </summary>
		/// <param name="index">The index of the frame to obtain.</param>
		/// <returns>Returns the 2D animation frame.</returns>
		/// <exception cref="NullReferenceException">Thrown if this is not an Animation2D.</exception>
		/// <exception cref="IndexOutOfRangeException">Thrown if <paramref name="index"/> is less than 0 or at least <see cref="FrameCount"/>.</exception>
		public Animation2DFrame Get2DFrame(int index) => Frames![index];
		#endregion

		#region Animation Type Identification
		/// <summary>
		/// If true, this Animation represents an Animation2D and is fully valid.
		/// </summary>
		public bool IsAnimation2D
		{get;}
		#endregion

		/// <summary>
		/// The number of frames in the animation.
		/// </summary>
		public int FrameCount
		{get;}

		#region Animation2D Exclusive Data
		/// <summary>
		/// The sprite sheet we use for the animation frames.
		/// <para/>
		/// This is only defined when this represents an Animation2D.
		/// </summary>
		public SpriteSheet? Source
		{get;}

		/// <summary>
		/// The frame data of this animation.
		/// </summary>
		protected Animation2DFrame[]? Frames
		{get;}
		#endregion

		#region Timing Data
		/// <summary>
		/// The animation start time.
		/// </summary>
		public float Start
		{get;}

		/// <summary>
		/// If true, this animation loops.
		/// If false, this animation plays only once.
		/// </summary>
		public bool Loops
		{get;}

		/// <summary>
		/// The time when the loop starts.
		/// <para/>
		/// This value has meaning even if Loops is false as it permits a default loop start if looping is enabled.
		/// </summary>
		public float LoopStart
		{get;}

		/// <summary>
		/// The time when the loop ends.
		/// <para/>
		/// This value has meaning even if Loops is false as it permits a default loop end if looping is enabled.
		/// </summary>
		public float LoopEnd
		{get;}
		#endregion

		public string ResourceName
		{get;}
	}
}
