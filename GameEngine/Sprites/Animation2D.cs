using GameEngine.Events;
using GameEngine.Exceptions;
using GameEngine.Framework;
using GameEngine.GameComponents;
using GameEngine.Maths;
using GameEngine.Time;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngine.Sprites
{
	/// <summary>
	/// Encapsulates sprite-based animation.
	/// </summary>
	public class Animation2D : BareGameComponent, IBlueprint, IObservable<TimeEvent>
	{
		/// <summary>
		/// Instantiates a new instance of <paramref name="a"/> as an Animation2D.
		/// </summary>
		/// <param name="a">The animation to turn into an Animation2D.</param>
		/// <exception cref="AnimationFormatException">Thrown if this animation does not represent an Animation2D.</exception>
		public Animation2D(Animation a)
		{
			if(!a.IsAnimation2D)
				throw new AnimationFormatException("The provided Animation did not represent an Animation2D");

			// The source sprite sheet should be shallow copied as it is immutable
			Source = a.Source!;

			// Load all of our frames
			Frames = new Animation2DFrame[a.FrameCount];

			IEnumerator<float> lens = a.FrameTimes!.GetEnumerator();
			IEnumerator<int> indices = a.FrameIndices!.GetEnumerator();
			IEnumerator<Matrix2D> transformations = a.FrameTransformations!.GetEnumerator();

			float start = 0.0f;

			for(int i = 0;i < Count;i++)
			{
				lens.MoveNext();
				indices.MoveNext();
				transformations.MoveNext();

				Frames[i] = new Animation2DFrame(i,start,lens.Current,indices.Current,transformations.Current);
				start += lens.Current;
			}

			// Set up the clock
			// The most important part about this construction process is that each animation instance gets its own clock
			Clock = new TimePartition(Frames.Skip(1).Select((value,index) => value.StartTime)); // We skip the first start time since 0 is always a start time
			Clock.SetMaximumTime(true,Frames[Count - 1].EndTime);
			
			// We set up the loop seperately so that we can ensure all three values are set without resorting to something gimmicky
			Clock.Loop(a.Loops);
			Clock.Loop(a.LoopStart,a.LoopEnd);

			return;
		}

		/// <summary>
		/// Creates a sufficiently deep copy of <paramref name="a"/> for them to operate independently while sharing resources.
		/// </summary>
		/// <param name="a">The animation to clone.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="a"/> is null.</exception>
		public Animation2D(Animation2D a)
		{
			if(a is null)
				throw new ArgumentNullException("The provided Animation was null");

			Source = a.Source; // Sprite sheets are immutable
			Frames = a.Frames; // Frames are immutable and so are arrays, so this is fine
			Clock = new TimePartition(a.Clock); // This will create a deep enough copy to get the job done
			
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
			if(singleton)
				if(BlueprintSingleton is null)
					return BlueprintSingleton = ToDrawable(game,renderer,false);
				else
					return BlueprintSingleton;
			
			return new AnimatedComponent(game,renderer,this);
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
			Clock.Dispose();
			
			base.Dispose(disposing);
			return;
		}

		public override string ToString()
		{return "Current Frame: " + CurrentFrame + " Current Time: " + CurrentTime;}

		public IDisposable Subscribe(IObserver<TimeEvent> eye)
		{return Clock.Subscribe(eye);}

		/// <summary>
		/// The clock running the animation.
		/// </summary>
		protected TimePartition Clock
		{get; set;}

		/// <summary>
		/// The sprite sheet we use for the animation frames.
		/// </summary>
		public SpriteSheet Source
		{get; init;}

		/// <summary>
		/// Obtains the source texture for this animation.
		/// </summary>
		public Texture2D SourceTexture
		{get => Source.Source;}

		/// <summary>
		/// Obtains the current source rectangle for this animation.
		/// </summary>
		public Rectangle CurrentSource
		{get => Source[this[CurrentFrame].Source];}

		/// <summary>
		/// The frame data of this animation.
		/// </summary>
		protected Animation2DFrame[] Frames
		{get; init;}

		/// <summary>
		/// Obtains the <paramref name="index"/>th frame of this animation.
		/// </summary>
		/// <param name="index">The index of the frame to obtain.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index"/> is negative or at least Count.</exception>
		public Animation2DFrame this[int index]
		{get => Frames[index];}

		/// <summary>
		/// The number of frames in this animation.
		/// </summary>
		public int Count
		{get => Frames.Length;}

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
			get => !Playing && CurrentTime == 0.0f; // This floating point assignment is exact when we are truly stopped, so this is fine
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

		/// <summary>
		/// The singleton instance of the drawable version of this object to return from ToDrawable.
		/// </summary>
		protected DrawableAffineComponent? BlueprintSingleton
		{get; set;}
	}

	/// <summary>
	/// Encapsulates frame information for an Animation2D.
	/// </summary>
	public class Animation2DFrame
	{
		/// <summary>
		/// Creates a new animation frame with the specified parameters.
		/// </summary>
		/// <param name="index">The index of the frame within its animation.</param>
		/// <param name="start">The start time of the frame within its animation.</param>
		/// <param name="len">The duration of the frame within its animation.</param>
		/// <param name="source">The index into the animation's sprite sheet for this frame's source texture.</param>
		/// <param name="transformation">The (affine) transformation to apply to this keyframe.</param>
		public Animation2DFrame(int index, float start, float len, int source, Matrix2D transformation)
		{
			Index = index;
			StartTime = start;
			Duration = len;
			Source = source;
			Transformation = transformation;

			return;
		}

		/// <summary>
		/// The index of this frame within its animation.
		/// </summary>
		public int Index
		{get; init;}

		/// <summary>
		/// The (inclusive) start time of this frame within its animation.
		/// </summary>
		public float StartTime
		{get; init;}

		/// <summary>
		/// The (exclusive) end time of this frame within its animation.
		/// </summary>
		public float EndTime
		{get => StartTime + Duration;}

		/// <summary>
		/// The duration of the frame in seconds.
		/// </summary>
		public float Duration
		{get; init;}

		/// <summary>
		/// The index into this frame's animation's sprite sheet for its source texture.
		/// </summary>
		public int Source
		{get; init;}

		/// <summary>
		/// The (affine) transformation to apply to this keyframe.
		/// </summary>
		public Matrix2D Transformation
		{get; init;}
	}
}
