using System.Runtime.Serialization;

namespace GameEngine.Exceptions
{
	/// <summary>
	/// A time exception for when time is a little too timey-wimey.
	/// </summary>
	public class TimeException : Exception
	{
		/// <summary>
		/// Creates an empty time exception.
		/// </summary>
		public TimeException()
		{return;}

		/// <summary>
		/// Creates a time exception with the provided message.
		/// </summary>
		public TimeException(string? message) : base(message)
		{return;}

		/// <summary>
		/// Creates a time exception with the provided message and inner exception.
		/// </summary>
		public TimeException(string? message, Exception? innerException) : base(message,innerException)
		{return;}

		/// <summary>
		/// Creates a time expcetion with the given serialization info and streaming context.
		/// </summary>
		public TimeException(SerializationInfo info, StreamingContext context) : base(info,context)
		{return;}
	}
}
