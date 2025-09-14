namespace GameEngine.Exceptions
{
	/// <summary>
	/// An animation format exception for when animation data does not match the format required or when animation data components are missing.
	/// </summary>
	public class AnimationFormatException : Exception
	{
		/// <summary>
		/// Creates an AnimationFormatException.
		/// </summary>
		public AnimationFormatException()
		{return;}

		/// <summary>
		/// Creates an AnimationFormatException with the provided message.
		/// </summary>
		public AnimationFormatException(string? message) : base(message)
		{return;}

		/// <summary>
		/// Creates an AnimationFormatException with the provided message and inner exception.
		/// </summary>
		public AnimationFormatException(string? message, Exception? innerException) : base(message,innerException)
		{return;}
	}
}
