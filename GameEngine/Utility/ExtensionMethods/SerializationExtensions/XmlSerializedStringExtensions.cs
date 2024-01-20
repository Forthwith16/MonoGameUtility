using System.Xml.Serialization;

namespace GameEngine.Utility.ExtensionMethods.SerializationExtensions
{
	/// <summary>
	/// Provides XML related extensions to strings.
	/// </summary>
	public static class XmlSerializedStringExtensions
	{
		/// <summary>
		/// Converts an XML string into a <typeparamref name="T"/> type.
		/// </summary>
		/// <typeparam name="T">The type to convert to.</typeparam>
		/// <param name="sxml">The XML specification of a <typeparamref name="T"/> type.</param>
		/// <returns>Returns the converted object or null if <paramref name="sxml"/> was not a valid specification of a <typeparamref name="T"/> type.</returns>
		public static T? DeserializeXml<T>(this string sxml)
		{
			using(StringReader sin = new StringReader(sxml))
				return (T?)new XmlSerializer(typeof(T)).Deserialize(sin);
		}

		/// <summary>
		/// Converts an XML file specified by <paramref name="path"/> into a <typeparamref name="T"/> type.
		/// </summary>
		/// <typeparam name="T">The type to convert to.</typeparam>
		/// <param name="path">The path to an XML file specifying a <typeparamref name="T"/> type.</param>
		/// <returns>Returns the converted object or null if <paramref name="path"/> was not a valid file specifying a <typeparamref name="T"/> type.</returns>
		public static T? DeserializeXmlFile<T>(this string path)
		{
			using(FileStream sin = File.OpenRead(path))
				return (T?)new XmlSerializer(typeof(T)).Deserialize(sin);
		}
	}
}
