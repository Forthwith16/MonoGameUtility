using GameEngine.Events;
using GameEngine.GameComponents;
using Microsoft.Xna.Framework;

namespace GameEngine.Time
{
	/// <summary>
	/// Keeps track of time like a clock and also partitions time into disjoint, consecutive segements.
	/// For example, if we want to animate a sequence of 10 images such that each image lasts .1 second, we would want to know when we should switch frames.
	/// We could set up time to send us a notification when the time passes the 0 ms mark, 100 ms mark, 200 ms mark, etc.
	/// If we also want that animation to loop, we could set this partition to loop when it reaches 1000 ms and jump back to 0 ms.
	/// <para/>
	/// Although this class <i>can</i> be added to a Game, it is generally best for an object utilitizing it to manage it manually.
	/// This will include calling its Initialize, Update, and Dispose functions.
	/// </summary>
	public class TimePartition : BareGameComponent, IObservable<TimeEvent>
	{
		/// <summary>
		/// Creates an empty time partition.
		/// It has no maximum time and only one time segment that goes unto infinity.
		/// </summary>
		/// <param name="game">The game this object </param>
		public TimePartition()
		{
			Playing = false;
			Loops = false;
			LoopStart = 0.0f;
			LoopEnd = float.MaxValue;
			
			TimeEnds = false;
			EndOfTime = float.MaxValue;
			
			ElapsedTime = 0.0f;
			CurrentTime = 0.0f;
			
			Segmentations = new List<float>();
			
			Observers = new LinkedList<IObserver<TimeEvent>>();
			return;
		}

		/// <summary>
		/// Creates a time partition with segmentations given by <paramref name="segment_starts"/>.
		/// </summary>
		/// <param name="segment_starts">
		/// These are the times when new segments start.
		/// The initial segment always begins at 0 (a value of 0 will be ignored), and the final segment extends forever.
		/// Duplicate values will be ignored.
		/// Best practice is to provide the segmented values as sorted in advance as possible to reduce sorting time.
		/// </param>
		public TimePartition(IEnumerable<float> segment_starts)
		{
			Playing = false;
			Loops = false;
			LoopStart = 0.0f;
			LoopEnd = float.MaxValue;
			
			TimeEnds = false;
			EndOfTime = float.MaxValue;
			
			ElapsedTime = 0.0f;
			CurrentTime = 0.0f;
			
			Segmentations = new List<float>(segment_starts.Where(t => t.CompareTo(0.0f) > 0).Distinct());
			Segmentations.Sort();

			Observers = new LinkedList<IObserver<TimeEvent>>();
			return;
		}

		/// <summary>
		/// Creates a duplicate time partition.
		/// The new time partition does not retain time data but does keep miscellaneous state changes such as loop settings.
		/// It also does not copy the old clock's observers.
		/// The new time partition must also be initialized.
		/// </summary>
		/// <param name="clock">The time partition to duplicate.</param>
		public TimePartition(TimePartition clock)
		{
			Playing = false;
			Loops = clock.Loops;
			LoopStart = clock.LoopStart;
			LoopEnd = clock.LoopEnd;
			
			TimeEnds = clock.TimeEnds;
			EndOfTime = clock.EndOfTime;
			
			ElapsedTime = 0.0f;
			CurrentTime = 0.0f;
			
			Segmentations = new List<float>(clock.Segmentations);
			
			Observers = new LinkedList<IObserver<TimeEvent>>();
			return;
		}
		
		/// <summary>
		/// Initializes this timeline.
		/// </summary>
		public override void Initialize()
		{
			base.Initialize();

			Play();
			return;
		}
		
		/// <summary>
		/// Updates this timeline.
		/// </summary>
		/// <param name="delta">The elapsed gametime since the last update.</param>
		public override void Update(GameTime delta)
		{
			// Elapsed time is easy
			// It just counts total time experienced
			ElapsedTime += (float)delta.ElapsedGameTime.TotalSeconds;
			
			// Current time has to factor in a possible time loop or a maximum time
			float PreviousTime = CurrentTime;
			CurrentTime += (float)delta.ElapsedGameTime.TotalSeconds;

			// If we're in a time loop, ensure that we time travel only if we pass through the barrier 
			if(Loops && CurrentTime >= LoopEnd && PreviousTime < LoopEnd && (!TimeEnds || EndOfTime >= LoopEnd))
			{
				float over = CurrentTime - LoopEnd;
				
				while(over >= 0.0f) // If we went to or past the time loop's end, we need to time travel
				{
					CurrentTime -= LoopLength; // We time travel immediately upon hitting the end of the loop, so we retain extra time after the time travel event to progress forward again
					over -= LoopLength;
					
					Notify(new TimeEvent(over >= 0.0f ? over : CurrentTime - LoopStart)); // Once we're back inside the time loop, we advance forward some amount past the time loop's start
				}
			}
			
			// If time ends, make sure we don't go sailing off into the void of a timeless realm
			if(TimeEnds && CurrentTime >= EndOfTime)
			{
				CurrentTime = EndOfTime;
				Pause();
				
				Notify(new TimeEvent());
			}
			
			// We can now handle segment change notifications
			int now = CurrentSegment;
			int old = Segment(PreviousTime);
			
			if(now != old)
				Notify(new TimeEvent(now,old));
			
			return;
		}
		
		/// <summary>
		/// Performs the actual disposal logic.
		/// </summary>
		/// <param name="disposing">If true, we are disposing of this. If false, then this has already been disposed and is in a finalizer call.</param>
		protected override void Dispose(bool disposing)
		{
			foreach(IObserver<TimeEvent> eye in Observers)
				eye.OnCompleted();
			
			base.Dispose(disposing);
			return;
		}

		/// <summary>
		/// Determines the time segment containing <paramref name="time"/>.
		/// </summary>
		/// <param name="time">The time to look for its containing time segment.</param>
		/// <returns>
		///	Returns the time segment containing <paramref name="time"/> or -1 if <paramref name="time"/> does not belong to any time segment.
		///	This can happen when <paramref name="time"/> is negative or when it is greater that the EndOfTime if TimeEnds.
		/// </returns>
		public int Segment(float time)
		{return BinarySearch(time);}
		
		/// <summary>
		/// Performs a binary search for <paramref name="time"/> in the time segments to determine to which segment it belongs.
		/// </summary>
		/// <param name="time">The time to search for.</param>
		/// <returns>Returns -1 if <paramref name="time"/> is negative and the time segment it belongs to otherwise.</returns>
		protected int BinarySearch(float time)
		{
			// Binary Search will always return a segment unless time is negative since we cover the entire nonnegative ray
			if(time < 0)
				return -1;
			
			// We'll do two special cases here so that writing the binary search doesn't require a weird special case
			if(Segmentations.Count == 0 || time < Segmentations[0])
				return 0;
			
			if(time >= Segmentations[Segmentations.Count - 1])
				if(TimeEnds)
					if(time <= EndOfTime)
						return Segmentations.Count;
					else
						return -1;
				else
					return Segmentations.Count;
			
			// Set up the l and r pointers
			int l = 0;
			int r = Segmentations.Count - 2;
			
			while(l <= r)
			{
				int m = (l + r) >> 1;
				
				if(time >= Segmentations[m + 1])
					l = m + 1;
				else if(time < Segmentations[m])
					r = m - 1;
				else
					return m + 1;
			}

			return -1;
		}
		
		/// <summary>
		/// Determines if the time segement <paramref name="segment"/> contains the time <paramref name="time"/>.
		/// </summary>
		/// <param name="segment">The segment to look into.</param>
		/// <param name="time">The time to check segment <paramref name="segment"/> for containment.</param>
		/// <returns>Returns true if segment <paramref name="segment"/> contains time <paramref name="time"/>. Returns false otherwise.</returns>
		public bool SegmentContainsTime(int segment, long time)
		{
			if(time < 0)
				return false;
			
			int size = Segmentations.Count; // This is intentionally NOT SegmentCount

			if(segment == 0)
				return size == 0 || time < Segmentations[0];
			
			if(segment < 0 || segment > size)
				return false;
			
			if(segment == size)
				return time >= Segmentations[segment - 1];
			
			return time >= Segmentations[segment - 1] && time < Segmentations[segment];
		}

		/// <summary>
		/// Partitions an existing time segment into two.
		/// One begins at the previous beginning time.
		/// The other begins at <paramref name="time"/>.
		/// </summary>
		/// <param name="time">The time for the new time segment to begin. This value must be unique and nonnegative to have an effect.</param>
		/// <returns>Returns true if the time segment was added and false otherwise.</returns>
		public bool AddSegment(float time)
		{
			if(time <= 0.0f)
				return false;
			
			int containing_segment = Segment(time);
			
			if(containing_segment == 0 && time != 0)
			{
				Segmentations.Insert(0,time);
				return true;
			}
			
			if(Segmentations[containing_segment - 1] == time)
				return false;
			
			Segmentations.Insert(containing_segment,time);
			return true;
		}
		
		/// <summary>
		/// Gets the beginning time of the <paramref name="index"/>th segment.
		/// </summary>
		/// <param name="index">The segment to fetch the beginning time of.</param>
		/// <returns>Returns the beginning time of the <paramref name="index"/>th segment.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index"/> is negative or at least SegmentCount.</exception>
		public float GetSegment(int index)
		{
			if(index == 0)
				return 0.0f;
			
			return Segmentations[index - 1];
		}

		/// <summary>
		/// Gets the length of time the <paramref name="index"/>th segment lasts.
		/// </summary>
		/// <param name="index">The zero-indexed segment to fetch the duration of.</param>
		/// <returns>Returns the length of the segment. If the segment is unbounded, then float.PositiveInfinity is returned instead.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index"/> is negative or at least SegmentCount.</exception>
		public float GetSegmentLength(int index)
		{
			if(index == SegmentCount - 1)
				return TimeEnds ? EndOfTime - GetSegment(index) : float.PositiveInfinity;

			return GetSegment(index + 1) - GetSegment(index);
		}

		/// <summary>
		/// Removes the segment beginning at time <paramref name="time"/>.
		/// Note that the first segment beginning a time 0 can never be removed.
		/// </summary>
		/// <param name="time">The time at which the segment to remove begins.</param>
		/// <returns>Returns true if a segment was removed and false otherwise.</returns>
		public bool RemoveSegment(float time)
		{
			int seg = Segment(time);
			
			if(seg == 0)
				return false;
			
			if(Segmentations[seg - 1] != time)
				return false;
			
			Segmentations.RemoveAt(seg - 1);
			return true;
		}
		
		/// <summary>
		/// Removes a time segment.
		/// </summary>
		/// <param name="index">The time segment to remove.</param>
		/// <returns>Returns the time when the segment began.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index"/> is less than 1 (the first segment always beings at 0) or if <paramref name="index"/> is at least SegmentCount.</exception>
		public float RemoveSegment(int index)
		{
			float ret = Segmentations[index];
			Segmentations.RemoveAt(index - 1);

			return ret;
		}
		
		/// <summary>
		/// Causes time to be experienced starting at whatever the current time is.
		/// </summary>
		public void Play()
		{
			bool ptemp = Playing;
			Playing = true;
			
			if(!ptemp)
				Notify(new TimeEvent(true,false,false));
			
			return;
		}

		/// <summary>
		/// Sets the current time to the beginning of time segement <paramref name="segment"/> without affecting whether or not time is experienced.
		/// </summary>
		/// <param name="segment">
		///	The time segment to jump to.
		///	Setting the time this way does not cause time loops or time clamps to immediately occur.
		///	The normal course of time advancement will see to these events if they occur.
		/// </param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="segment"/> is negative or at least SegementCount.</exception>
		public void Seek(int segment)
		{
			Seek(GetSegment(segment));
			return;
		}

		/// <summary>
		/// Sets the current time to <paramref name="start"/> without affecting whether or not time is experienced.
		/// </summary>
		/// <param name="start">
		///	The time to jump to.
		///	Setting the time this way does not cause time loops or time clamps to immediately occur.
		///	The normal course of time advancement will see to these events if they occur.
		/// </param>
		/// <exception cref="ArgumentException">Thrown if <paramref name="start"/> is negative or greater than EndOfTime if TimeEnds.</exception>
		public void Seek(float start)
		{
			if(start < 0.0f || TimeEnds && start > EndOfTime)
				throw new ArgumentException();
			
			float ttemp = CurrentTime;
			CurrentTime = start;
			
			if(ttemp != start)
				Notify(new TimeEvent(start,ttemp));
			
			int now = CurrentSegment;
			int old = Segment(ttemp);
			
			if(now != old)
				Notify(new TimeEvent(now,old));
			
			return;
		}
		
		/// <summary>
		/// Causes time to no longer be experienced but keeps the current time fixed.
		/// </summary>
		public void Pause()
		{
			bool ptemp = Playing;
			Playing = false;
			
			if(ptemp)
				Notify(new TimeEvent(false,true,false));
			
			return;
		}
		
		/// <summary>
		/// Causes time to no longer be experienced and sets the current time to 0.
		/// </summary>
		public void Stop()
		{
			bool ptemp = Playing;
			float ttemp = CurrentTime;
			
			Playing = false;
			CurrentTime = 0.0f;
			
			if(ptemp)
				Notify(new TimeEvent(false,false,true));
			
			if(ttemp != 0.0f)
				Notify(new TimeEvent(0.0f,ttemp));
			
			int now = CurrentSegment;
			int old = Segment(ttemp);
			
			if(now != old)
				Notify(new TimeEvent(now,old));
			
			return;
		}

		/// <summary>
		/// Sets if time loops.
		/// </summary>
		/// <param name="loop">If true, then time loops. If false, then time does not loop.</param>
		public void Loop(bool loop)
		{
			Loops = loop;
			return;
		}

		/// <summary>
		/// Sets the interval over which time loops (even if time is not currently looping).
		/// </summary>
		/// <param name="start">The (inclusive) start of the time loop.</param>
		/// <param name="end">
		///	The (exclusive) end of the time loop.
		///	Picking a loop end before the current time does not cause time to immediately loop.
		///	Time only loops if time advances through the end of the time loop in its ordinary course of motion.
		/// </param>
		/// <exception cref="ArgumentException">Thrown if <paramref name="start"/> or <paramref name="end"/> is negative or if <paramref name="start"/> >= <paramref name="end"/>.</exception>
		public void Loop(float start, float end)
		{
			if(start < 0.0f || end < 0.0f || start >= end)
				throw new ArgumentException();

			LoopStart = start;
			LoopEnd = end;

			return;
		}

		/// <summary>
		/// Sets if time loops.
		/// </summary>
		/// <param name="loop">If true, then time loops. If false, then time does not loop.</param>
		/// <param name="start">The (inclusive) start of the time loop. If <paramref name="loop"/> is false, then this value is ignored.</param>
		/// <param name="end">
		/// The (exclusive) end of the time loop.
		/// If <paramref name="loop"/> is false, then this value is ignored.
		/// Picking a loop end before the current time does not cause time to immediately loop.
		/// Time only loops if time advances through the end of the time loop in its ordinary course of motion.
		/// </param>
		/// <exception cref="ArgumentException">Thrown if <paramref name="start"/> or <paramref name="end"/> is negative or if <paramref name="start"/> >= <paramref name="end"/> when <paramref name="loop"/> is true.</exception>
		public void Loop(bool loop, float start, float end)
		{
			if(loop && (start < 0.0f || end < 0.0f || start >= end))
				throw new ArgumentException();
			
			Loops = loop;
			
			if(Loops)
			{
				LoopStart = start;
				LoopEnd = end;
			}
			
			return;
		}

		/// <summary>
		/// Sets if there is a maximum attainable time.
		/// If the maximum time is equal to the loop end, the time loop takes prioirty.
		/// </summary>
		/// <param name="time_ends">If true, then there is a maximum attainable time value. If false, time goes on forever.</param>
		/// <param name="EoT">The (inclusive) maximum attainable time value. This value is ignored if <paramref name="time_ends"/> is false.</param>
		/// <exception cref="ArgumentException">Thrown if <paramref name="EoT"/> is negative when <paramref name="time_ends"/> is true.</exception>
		public void SetMaximumTime(bool time_ends, float EoT)
		{
			if(time_ends && EoT < 0.0f)
				throw new ArgumentException();
			
			TimeEnds = time_ends;
			
			if(TimeEnds)
			{
				EndOfTime = EoT;
				
				if(CurrentTime >= EndOfTime)
				{
					CurrentTime = EndOfTime;
					Pause();
					
					Notify(new TimeEvent());
				}
			}
		
			return;
		}
		
		public IDisposable Subscribe(IObserver<TimeEvent> eye)
		{
			Observers.AddLast(eye);
			return new Unsubscriber(this,eye);
		}

		/// <summary>
		/// Removes <paramref name="eye"/> from the observation list.
		/// </summary>
		protected void Unsubscribe(IObserver<TimeEvent> eye)
		{
			Observers.Remove(eye);
			return;
		}
		
		/// <summary>
		/// Notifies all observers about the event <paramref name="e"/>.
		/// </summary>
		protected void Notify(TimeEvent e)
		{
			foreach(IObserver<TimeEvent> eye in Observers)
				eye.OnNext(e);
			
			return;
		}
		
		public override string ToString()
		{
			string ret = "{0";
			
			foreach(float f in Segmentations)
				ret += ", " + f;
			
			return ret + "}";
		}

		/// <summary>
		/// If true, then time is experienced.
		/// If false, then time is not experienced.
		/// </summary>
		public bool Playing
		{get; protected set;}
		
		/// <summary>
		/// Determines if time is not being experienced but can be resumed from where it stopped.
		/// </summary>
		public bool Paused => !Playing;
		
		/// <summary>
		/// Determines if time is not being experienced and has been reset to its beginning.
		/// </summary>
		public bool Stopped => Paused && CurrentTime == 0.0f; // We want the exact assignment, not close enough

		/// <summary>
		/// The current time.
		/// </summary>
		public float CurrentTime
		{get; protected set;}

		/// <summary>
		/// Determines the current time segment.
		/// <para/>
		/// Note that although this is a log n operation, n is the number of time segments.
		/// As such, log n should always be tiny.
		/// </summary>
		public int CurrentSegment => Segment(CurrentTime);

		/// <summary>
		/// Obtains the segment index containing the time <paramref name="time"/>.
		/// </summary>
		/// <param name="time">The time to query.</param>
		/// <returns>
		///	Returns the index of the time segment containing the <paramref name="time"/>.
		///	If no time segment contains <paramref name="time"/>, such as when <paramref name="time"/> is negative or greater than EndOfTime if TimeEnds, then -1 is returned instead.
		/// </returns>
		public float this[float time]
		{get => Segment(time);}

		/// <summary>
		/// The total elapsed time experienced including loops and resets.
		/// </summary>
		public float ElapsedTime
		{get; protected set;}
		
		/// <summary>
		/// If true, then time loops.
		/// </summary>
		public bool Loops
		{get; protected set;}
		
		/// <summary>
		/// If time loops, this is when the time loop starts.
		/// </summary>
		public float LoopStart
		{get; protected set;}
		
		/// <summary>
		/// If time loops, this is when the time loop ends.
		/// This value is exclusive.
		/// </summary>
		public float LoopEnd
		{get; protected set;}

		/// <summary>
		/// Determines the length of the time loop.
		/// If there is no loop, then positive infinity is returned instead.
		/// </summary>
		public float LoopLength => Loops ? LoopEnd - LoopStart : float.PositiveInfinity;
		
		/// <summary>
		/// If true, then there is a maximum time attainable.
		/// </summary>
		public bool TimeEnds
		{get; protected set;}
		
		/// <summary>
		/// If time ends, then this is the maximum time attainable.
		/// This value is inclusive.
		/// </summary>
		public float EndOfTime
		{get; protected set;}

		/// <summary>
		/// The times when a new time segment beings.
		/// The last time segement carries on to infinity.
		/// </summary>
		protected List<float> Segmentations;

		/// <summary>
		/// Determines the number of time segments in the time partition.
		/// </summary>
		public int SegmentCount => 1 + Segmentations.Count;

		/// <summary>
		/// The observers of this timeline.
		/// </summary>
		protected LinkedList<IObserver<TimeEvent>> Observers;

		/// <summary>
		/// Allows an observer to unsubscribe from this partition.
		/// </summary>
		protected class Unsubscriber : IDisposable
		{
			public Unsubscriber(TimePartition p, IObserver<TimeEvent> eye)
			{
				Partition = p;
				Eye = eye;
				
				Disposed = false;
				return;
			}

			public void Dispose()
			{
				// You can only unsubscribe once (even if you've subscribed multiple times) per unsubscriber
				if(Disposed)
					return;

				Partition.Unsubscribe(Eye);
				Disposed = true;

				return;
			}

			protected TimePartition Partition
			{get;}

			protected IObserver<TimeEvent> Eye
			{get;}

			public bool Disposed
			{get; protected set;}
		}
	}
}