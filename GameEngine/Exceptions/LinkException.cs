namespace GameEngine.Exceptions
{
	/// <summary>
	/// An exception thrown when something goes wrong with asset linking.
	/// </summary>
	public class LinkException : Exception
	{
		/// <summary>
		/// Creates an empty link exception.
		/// </summary>
		public LinkException()
		{return;}

		/// <summary>
		/// Creates a link exception with the provided message.
		/// </summary>
		public LinkException(string? message) : base(message)
		{return;}

		/// <summary>
		/// Creates a link exception with the provided message and inner exception.
		/// </summary>
		public LinkException(string? message, Exception? innerException) : base(message,innerException)
		{return;}
	}
}
