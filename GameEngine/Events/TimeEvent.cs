namespace GameEngine.Events
{
	/// <summary>
	/// Represents something happening in the timeline.
	/// </summary>
	public class TimeEvent : EventArgs
	{
		/// <summary>
		/// Creates an end of time event.
		/// </summary>
		public TimeEvent()
		{
			Type = EventType.EOT;
			
			NewTimeSegement = 0;
			OldTimeSegement = 0;
			
			JumpTo = 0.0f;
			JumpFrom = 0.0f;
			TimeRemaining = 0.0f;
			
			return;
		}

		/// <summary>
		/// Creates a time event for passing into a new time segment.
		/// </summary>
		/// <param name="segment">The time segment advanced into.</param>
		/// <param name="old">The time segment advanced out of.</param>
		public TimeEvent(int segment, int old)
		{
			NewTimeSegement = segment;
			OldTimeSegement = old;
			Type = EventType.SEGMENT_CHANGE;
			
			JumpTo = 0.0f;
			JumpFrom = 0.0f;
			TimeRemaining = 0.0f;
			
			return;
		}

		/// <summary>
		/// Creates a time event for when a non-time loop jump is made.
		/// </summary>
		/// <param name="to">The time jumped to.</param>
		/// <param name="from">The time jumped from.</param>
		public TimeEvent(float to, float from)
		{
			JumpTo = to;
			JumpFrom = from;
			Type = EventType.JUMP;
			
			NewTimeSegement = 0;
			OldTimeSegement = 0;

			TimeRemaining = 0.0f;
			return;
		}

		/// <summary>
		/// Creates a time event for a time loop jump or the end of time.
		/// </summary>
		/// <param name="remaining">The remaining time left to advance after the time jump has been made.</param>
		public TimeEvent(float remaining)
		{
			TimeRemaining = remaining;
			Type = EventType.LOOP;
			
			NewTimeSegement = 0;
			OldTimeSegement = 0;

			JumpTo = 0.0f;
			JumpFrom = 0.0f;
			
			return;
		}

		/// <summary>
		/// Creates a time event for time starting, pausing, or stopping.
		/// </summary>
		/// <param name="play">If true, time started.</param>
		/// <param name="pause">If true, time paused.</param>
		/// <param name="stop">If true, then time stopped.</param>
		public TimeEvent(bool play, bool pause, bool stop)
		{
			if(play && !pause && !stop)
				Type = EventType.PLAY;
			else if(!play && pause && !stop)
				Type = EventType.PAUSE;
			else if(!play && !pause && stop)
				Type = EventType.STOP;
			else
				Type = EventType.INVALID;
			
			NewTimeSegement = 0;
			OldTimeSegement = 0;

			JumpTo = 0.0f;
			JumpFrom = 0.0f;
			TimeRemaining = 0.0f;
			
			return;
		}
		
		public override string ToString()
		{
			string ret;
			
			switch(Type)
			{
			case EventType.PLAY:
				ret = "This is a play event.";
				break;
			case EventType.PAUSE:
				ret = "This is a pause event.";
				break;
			case EventType.STOP:
				ret = "This is a stop event.";
				break;
			case EventType.LOOP:
				ret = "This is a time loop event with " + TimeRemaining + " time remaining to process.";
				break;
			case EventType.JUMP:
				ret = "This is a time jump event from time " + JumpFrom + " to time " + JumpTo + ".";
				break;
			case EventType.SEGMENT_CHANGE:
				ret = "This is a time segment change event from segment " + OldTimeSegement + " to segment " + NewTimeSegement + ".";
				break;
			case EventType.EOT:
				ret = "This is an end of time event.";
				break;
			default:
				ret = "This is an invalid time event.";
				break;
			}
			
			return ret;
		}
		
		/// <summary>
		/// True if this is a play event and false otherwise.
		/// </summary>
		public bool IsPlayEvent => Type == EventType.PLAY;
		
		/// <summary>
		/// True if this is a pause event and false otherwise.
		/// </summary>
		public bool IsPauseEvent => Type == EventType.PAUSE;
		
		/// <summary>
		/// True if this is a play event and false otherwise.
		/// </summary>
		public bool IsStopEvent => Type == EventType.STOP;
		
		/// <summary>
		/// True if this is a time loop event and false otherwise.
		/// </summary>
		public bool IsTimeLoop => Type == EventType.LOOP;
		
		/// <summary>
		/// True if this is a time jump (not loop) event and false otherwise.
		/// </summary>
		public bool IsTimeJump => Type == EventType.JUMP;
		
		/// <summary>
		/// True if this is a time segment change event and false otherwise.
		/// </summary>
		public bool IsSegmentChange => Type == EventType.SEGMENT_CHANGE;
		
		/// <summary>
		/// True if this is an end of time event and false otherwise.
		/// </summary>
		public bool IsEndOfTime => Type == EventType.EOT;

		/// <summary>
		/// The time segment time is now in during a segement change event.
		/// This valud is undefined otherwise.
		/// </summary>
		public int NewTimeSegement
		{get;}

		/// <summary>
		/// The time segment time was in during a segement change event.
		/// This valud is undefined otherwise.
		/// </summary>
		public int OldTimeSegement
		{get;}

		/// <summary>
		/// The time jumped to in a time jump (not loop) event.
		/// This value is undefined otherwise.
		/// </summary>
		public float JumpTo
		{get;}

		/// <summary>
		/// The time left in a time jump (not loop) event.
		/// This value is undefined otherwise.
		//// </summary>
		public float JumpFrom
		{get;}

		/// <summary>
		/// The time remaining to process in a time loop event.
		/// This value is undefined otherwise.
		/// </summary>
		public float TimeRemaining
		{get;}

		/// <summary>
		/// The type of event this represents.
		/// </summary>
		public EventType Type
		{get;}

		/// <summary>
		/// Species what kind of event a TimeEvent is.
		/// </summary>
		public enum EventType
		{
			INVALID,
			PLAY,
			PAUSE,
			STOP,
			LOOP,
			JUMP,
			SEGMENT_CHANGE,
			EOT
		}
	}
}
