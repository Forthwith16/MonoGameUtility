namespace GameEngine.Utility.Parsing
{
	/// <summary>
	/// Defines a saveable object.
	/// </summary>
	/// <remarks>Although this interface cannot require it, every <b>constructable</b> ISaveable object should define an analogous (not necessarily public) constructor which takes a Stream as a single parameter.</remarks>
	public interface ISaveable
	{
		/// <summary>
		/// Writes this object to the stream.
		/// </summary>
		/// <param name="sout">The stream to write the event to.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="sout"/> is null.</exception>
		/// <exception cref="NotSupportedException">Thrown if <paramref name="sout"/> does not support writing.</exception>
		/// <exception cref="ObjectDisposedException">Thrown if the stream was already closed.</exception>
		/// <exception cref="IOException">Thrown when an IO exception occurs for mysterious reasons.</exception>
		public void WriteToStream(Stream sout);

		/// <summary>
		/// Writes this object to the end of a file.
		/// </summary>
		/// <param name="path">The path to the file to write to.</param>
		/// <param name="mode">The writing mode. By default, this is append so that this object is written to the end of the file.</param>
		public void WriteToFile(string path, FileMode mode = FileMode.Append)
		{
			using(Stream sout = File.Open(path,mode))
				WriteToStream(sout);
			
			return;
		}
	}
}