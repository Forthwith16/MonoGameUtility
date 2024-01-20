using System.Text;

namespace GameEngine.Utility.Streams
{
	/// <summary>
	/// Extends the StringWriter class to allow for arbitrary encodings.
	/// </summary>
	public class StringWriterWithEncoding : StringWriter
	{
		/// <summary>
		/// Creates a new StringWriter with default behavior.
		/// </summary>
		public StringWriterWithEncoding() : base()
		{return;}

		/// <summary>
		/// Creates a new StringWriter with the specified encoding.
		/// </summary>
		/// <param name="encoding">The encoding to use.</param>
		public StringWriterWithEncoding(Encoding? encoding) : base()
		{
			_e = encoding;
			return;
		}

		public override Encoding Encoding => _e ?? base.Encoding;

		private readonly Encoding? _e;
	}
}
