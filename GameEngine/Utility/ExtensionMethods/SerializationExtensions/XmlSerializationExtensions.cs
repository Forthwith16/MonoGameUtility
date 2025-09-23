using GameEngine.Utility.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace GameEngine.Utility.ExtensionMethods.SerializationExtensions
{
	/// <summary>
	/// Adds extension methods to the XML Serialization.
	/// </summary>
	public static class XmlSerializationExtensions
	{
		/// <summary>
		/// Serializes an object to a <paramref name="stream"/> in XML format.
		/// </summary>
		/// <typeparam name="T">The type to serialize.</typeparam>
		/// <param name="value">The value to serialize.</param>
		/// <param name="stream">The stream to write to.</param>
		/// <param name="remove_default_xml_namespace">If true, we will remove the default XML namespaces from the output.</param>
		/// <param name="omit_xml_declaration">If true, we will omit the XML declartion from the output.</param>
		public static void SerializeXml<T>(this T value, Stream stream, bool remove_default_xml_namespace = true, bool omit_xml_declaration = true)
		{
			XmlSerializerNamespaces? xmlns = remove_default_xml_namespace ? new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty }) : null;

			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Indent = true;
			settings.OmitXmlDeclaration = omit_xml_declaration;
			settings.CheckCharacters = false;
			
			using(XmlWriter xout = XmlWriter.Create(stream,settings))
				new XmlSerializer(typeof(T)).Serialize(xout,value,xmlns);

			return;
		}

		/// <summary>
		/// Serializes an object to a <paramref name="stream"/> in XML format.
		/// </summary>
		/// <typeparam name="T">The type to serialize.</typeparam>
		/// <param name="value">The value to serialize.</param>
		/// <param name="path">The file to write to.</param>
		/// <param name="remove_default_xml_namespace">If true, we will remove the default XML namespaces from the output.</param>
		/// <param name="omit_xml_declaration">If true, we will omit the XML declartion from the output.</param>
		public static void SerializeXml<T>(this T value, string path, bool remove_default_xml_namespace = true, bool omit_xml_declaration = true)
		{
			using(FileStream fout = File.Create(path))
				value.SerializeXml(fout,remove_default_xml_namespace,omit_xml_declaration);

			return;
		}

		/// <summary>
		/// Serializes an object to a <paramref name="stream"/> in XML format.
		/// </summary>
		/// <typeparam name="T">The type to serialize.</typeparam>
		/// <param name="value">The value to serialize.</param>
		/// <param name="sout">The StringWriter to write to.</param>
		/// <param name="remove_default_xml_namespace">If true, we will remove the default XML namespaces from the output.</param>
		/// <param name="omit_xml_declaration">If true, we will omit the XML declartion from the output.</param>
		public static void SerializeXml<T>(this T value, StringWriter sout, bool remove_default_xml_namespace = true, bool omit_xml_declaration = true)
		{
			XmlSerializerNamespaces? xmlns = remove_default_xml_namespace ? new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty }) : null;

			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Indent = true;
			settings.OmitXmlDeclaration = omit_xml_declaration;
			settings.CheckCharacters = false;

			using(XmlWriter xout = XmlWriter.Create(sout,settings))
				new XmlSerializer(typeof(T)).Serialize(xout,value,xmlns);

			return;
		}

		/// <summary>
		/// Serializes an object to a string in XML format.
		/// </summary>
		/// <typeparam name="T">The type to serialize.</typeparam>
		/// <param name="value">The value to serialize.</param>
		/// <param name="remove_default_xml_namespace">If true, we will remove the default XML namespaces from the output.</param>
		/// <param name="omit_xml_declaration">If true, we will omit the XML declartion from the output.</param>
		/// <param name="encoding">The character encoding to utilize in the output.</param>
		public static string ToXmlString<T>(this T value, bool remove_default_xml_namespace = true, bool omit_xml_declaration = true, Encoding? encoding = null)
		{
			using(EncodedStringWriter sout = new EncodedStringWriter(encoding))
			{
				value.SerializeXml(sout,remove_default_xml_namespace,omit_xml_declaration);
				return sout.ToString();
			}
		}
	}
}
