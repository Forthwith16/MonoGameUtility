using GameEngine.Events;
using GameEngine.Exceptions;
using GameEngine.Framework;
using GameEngine.Maths;
using GameEngine.Time;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngine.Assets.Sprites
{
	/// <summary>
	/// Encapsulates sprite-based animation.
	/// </summary>
	/// <remarks>
	/// Note that neither this nor anything it controls ever needs to know what Game it belongs to.
	/// </remarks>
	public class Animation2D : GameObject, IAsset, IObservable<TimeEvent>
	{
		/// <summary>
		/// Instantiates a new instance of <paramref name="a"/> as an Animation2D.
		/// </summary>
		/// <param name="a">The animation to turn into an Animation2D.</param>
		/// <exception cref="AnimationFormatException">Thrown if this animation does not represent an Animation2D.</exception>
		public Animation2D(Animation a) : base()
		{
			if(!a.IsAnimation2D)
				throw new AnimationFormatException("The provided Animation did not represent an Animation2D");

			// Grab the asset name
			AssetName = a.AssetName;

			// Remmber where we come from
			SourceData = a;
			Source = SourceData.Source!; // If this is null, then a lied to us, so don't error check

			// Load the start time
			StartTime = a.Start;

			// Set up the clock
			// The most important part about this construction process is that each animation instance gets its own clock
			Clock = new TimePartition(SegmentTimes()); // We skip the first start time since 0 is always a start time
			Clock.SetMaximumTime(true,this[Count - 1].EndTime);
			Clock.Seek(StartTime);
			
			// We set up the loop seperately so that we can ensure all three values are set without resorting to something gimmicky
			Clock.Loop(a.Loops);
			Clock.Loop(a.LoopStart,a.LoopEnd);

			return;
		}

		/// <summary>
		/// Enumerates the segment start times for each frame.
		/// </summary>
		private IEnumerable<float> SegmentTimes()
		{
			for(int i = 1;i < Count;i++) // Skip the first frame's start time since it's always 0, which is always a time segment
				yield return SourceData.Get2DFrame(i)!.StartTime;
		}

		/// <summary>
		/// Creates a sufficiently deep copy of <paramref name="a"/> for this to operate independently while sharing resources.
		/// </summary>
		/// <param name="a">The animation to clone.</param>
		public Animation2D(Animation2D a) : base(a)
		{
			AssetName = a.AssetName;
			SourceData = a.SourceData;
			Source = a.Source;

			StartTime = a.StartTime;
			
			Clock = new TimePartition(a.Clock); // This will create a deep enough copy to get the job done
			Clock.Seek(StartTime);

			return;
		}

		public override void Initialize()
		{
			if(Initialized)
				return;

			Playing = true;

			base.Initialize();
			return;
		}

		public void Reset()
		{
			Stopped = true;
			Playing = true;

			return;
		}

		public override void Update(GameTime delta)
		{
			Clock.Update(delta);
			return;
		}

		protected override void Dispose(bool disposing)
		{
			if(Disposed)
				return;

			Clock.Dispose();
			
			base.Dispose(disposing);
			return;
		}

		public override string ToString()
		{return "Current Frame: " + CurrentFrame + " Current Time: " + CurrentTime;}

		public IDisposable Subscribe(IObserver<TimeEvent> eye)
		{return Clock.Subscribe(eye);}

		/// <summary>
		/// Obtains the <paramref name="index"/>th frame of this animation.
		/// </summary>
		/// <param name="index">The index of the frame to obtain.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index"/> is negative or at least Count.</exception>
		public Animation2DFrame this[int index] => SourceData.Get2DFrame(index)!; // If this is null, then our source lied to us

		/// <summary>
		/// The source data for the animation.
		/// </summary>
		public Animation SourceData
		{get;}

		/// <summary>
		/// The clock running the animation.
		/// </summary>
		protected TimePartition Clock
		{get;}

		/// <summary>
		/// The sprite sheet we use for the animation frames.
		/// </summary>
		public SpriteSheet Source
		{get;}

		/// <summary>
		/// Obtains the source texture for this animation.
		/// </summary>
		public Texture2D SourceTexture => Source.Source;

		/// <summary>
		/// Obtains the current source rectangle for this animation.
		/// </summary>
		public Rectangle CurrentSource
		{get => Source[this[CurrentFrame].SpriteIndex];}

		/// <summary>
		/// The number of frames in this animation.
		/// </summary>
		public int Count => SourceData.FrameCount;

		/// <summary>
		/// This is the current frame within the animation's timeline.
		/// The current time lies somewhere within this, and setting the current frame causes the current time to jump to the frame's beginning.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="segment"/> is negative or at least Count</exception>
		public int CurrentFrame
		{
			get => Clock.CurrentSegment;
			set => Clock.Seek(value);
		}

		/// <summary>
		/// The total length of the animation.
		/// </summary>
		public float AnimationLength
		{get => Clock.EndOfTime;}

		/// <summary>
		/// The initial start time of the animation.
		/// </summary>
		protected float StartTime
		{get;}

		/// <summary>
		/// This is the current time within the animation's timeline.
		/// </summary>
		/// <exception cref="ArgumentException">
		///	Thrown if attempting to set the time to a negative time or a time past the end of the animation.
		///	Note that although the ending time of the animation is technically exclusive, it is valid to assign this to that time.
		/// </exception>
		public float CurrentTime
		{
			get => Clock.CurrentTime;
			set => Clock.Seek(value);
		}

		/// <summary>
		/// The total time this animation has been playing.
		/// </summary>
		public float ElapsedTime
		{get => Clock.ElapsedTime;}

		/// <summary>
		/// If true, then the animation is playing.
		/// If false, then the animation is paused.
		/// </summary>
		public bool Playing
		{
			get => Clock.Playing;
			
			set
			{
				if(value)
					Clock.Play();
				else
					Clock.Pause();
				
				return;
			}
		}

		/// <summary>
		/// If true, then the animation is paused.
		/// If false, then the animation is playing.
		/// </summary>
		public bool Paused
		{
			get => !Playing;
			set => Playing = !value;
		}

		/// <summary>
		/// If true, then the animation has stopped (meaning it is paused a time 0).
		/// If false, then the animation is either playing or paused.
		/// <para/>
		/// This will also return true when merely paused at time 0, as there is no difference between that and being more truly stopped.
		/// </summary>
		public bool Stopped
		{
			get => !Playing && CurrentTime == StartTime; // This floating point assignment is exact when we are truly stopped, so this is fine
			set => Clock.Stop();
		}

		/// <summary>
		/// This is true if the animation loops and false if it does not.
		/// </summary>
		/// <remarks>We do not expose the animation LoopStart and LoopEnd here as we expect an animation to not change <i>too much</i> over the course of its lifetime.</remarks>
		public bool Loop
		{
			get => Clock.Loops;
			set => Clock.Loop(value);
		}

		public string AssetName
		{get;}
	}

	/// <summary>
	/// Encapsulates frame information for an Animation2D.
	/// </summary>
	public class Animation2DFrame
	{
		/// <summary>
		/// Creates a new animation frame with the specified parameters.
		/// </summary>
		/// <param name="frame">The index of the frame within its animation.</param>
		/// <param name="start">The start time of the frame within its animation.</param>
		/// <param name="len">The duration of the frame within its animation.</param>
		/// <param name="sprite">The index into the animation's sprite sheet for this frame's source texture.</param>
		/// <param name="transformation">The (affine) transformation to apply to this keyframe.</param>
		public Animation2DFrame(int frame, float start, float len, int sprite, Matrix2D transformation)
		{
			FrameIndex = frame;
			StartTime = start;
			Duration = len;
			SpriteIndex = sprite;
			Transformation = transformation;

			return;
		}

		/// <summary>
		/// The index of this frame within its animation.
		/// </summary>
		public int FrameIndex
		{get;}

		/// <summary>
		/// The (inclusive) start time of this frame within its animation.
		/// </summary>
		public float StartTime
		{get;}

		/// <summary>
		/// The (exclusive) end time of this frame within its animation.
		/// </summary>
		public float EndTime
		{get => StartTime + Duration;}

		/// <summary>
		/// The duration of the frame in seconds.
		/// </summary>
		public float Duration
		{get;}

		/// <summary>
		/// The index into this frame's animation's sprite sheet for its source texture.
		/// </summary>
		public int SpriteIndex
		{get;}

		/// <summary>
		/// The (affine) transformation to apply to this keyframe.
		/// </summary>
		public Matrix2D Transformation
		{get;}
	}
}
