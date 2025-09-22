using System.Text;

namespace GameEngine.Utility.ExtensionMethods.SerializationExtensions
{
	/// <summary>
	/// Extends the StringWriter class to allow for arbitrary encodings.
	/// </summary>
	public class EncodedStringWriter : StringWriter
	{
		/// <summary>
		/// Creates a new StringWriter with default behavior.
		/// </summary>
		public EncodedStringWriter() : base()
		{return;}

		/// <summary>
		/// Creates a new StringWriter with the specified encoding.
		/// </summary>
		/// <param name="encoding">The encoding to use.</param>
		public EncodedStringWriter(Encoding? encoding) : base()
		{
			_e = encoding;
			return;
		}

		public override Encoding Encoding => _e ?? base.Encoding;

		private readonly Encoding? _e;
	}
}
