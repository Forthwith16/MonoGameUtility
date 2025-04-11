using System.Runtime.Serialization;

namespace GameEngine.Exceptions
{
	/// <summary>
	/// An exception thrown when a vertex does not exist.
	/// </summary>
	public class NoSuchVertexException : Exception
	{
		/// <summary>
		/// Creates a NoSuchVertexException.
		/// </summary>
		public NoSuchVertexException()
		{return;}

		/// <summary>
		/// Creates a NoSuchVertexException with the provided message.
		/// </summary>
		public NoSuchVertexException(string? message) : base(message)
		{return;}

		/// <summary>
		/// Creates a NoSuchVertexException with the provided message and inner exception.
		/// </summary>
		public NoSuchVertexException(string? message, Exception? innerException) : base(message,innerException)
		{return;}

		/// <summary>
		/// Creates a NoSuchVertexException with the given serialization info and streaming context.
		/// </summary>
		public NoSuchVertexException(SerializationInfo info, StreamingContext context) : base(info,context)
		{return;}
	}

	/// <summary>
	/// An exception thrown when an edge does not exist.
	/// </summary>
	public class NoSuchEdgeException : Exception
	{
		/// <summary>
		/// Creates a NoSuchEdgeException.
		/// </summary>
		public NoSuchEdgeException()
		{return;}

		/// <summary>
		/// Creates a NoSuchEdgeException with the provided message.
		/// </summary>
		public NoSuchEdgeException(string? message) : base(message)
		{return;}

		/// <summary>
		/// Creates a NoSuchEdgeException with the provided message and inner exception.
		/// </summary>
		public NoSuchEdgeException(string? message, Exception? innerException) : base(message,innerException)
		{return;}

		/// <summary>
		/// Creates a NoSuchEdgeException with the given serialization info and streaming context.
		/// </summary>
		public NoSuchEdgeException(SerializationInfo info, StreamingContext context) : base(info,context)
		{return;}
	}
}
