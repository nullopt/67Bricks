using System;
using System.IO;
using System.Xml.Serialization;

namespace _68Buns.Handlers
{
	public static class XmlReader
	{
		public static T ReadXml<T>(string filePath)
		{
			try
			{
				var serializer = new XmlSerializer(typeof(T));

				using var reader = new StreamReader(filePath);

				return (T)serializer.Deserialize(reader);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}

			return default;
		}
	}
}
