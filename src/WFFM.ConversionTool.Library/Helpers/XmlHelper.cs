using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace WFFM.ConversionTool.Library.Helpers
{
	public static class XmlHelper
	{
		public static List<string> GetXmlElementNames(string fieldValue)
		{
			List<string> elementNames = new List<string>();
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(AddParentNodeAndEncodeElementValue(fieldValue));

			foreach (XmlNode childNode in xmlDocument.ChildNodes.Item(0).ChildNodes)
			{
				elementNames.Add(childNode.Name.ToLower());
			}

			return elementNames;
		}

		public static string GetXmlElementValue(string fieldValue, string elementName)
		{
			if (!string.IsNullOrEmpty(fieldValue) && !string.IsNullOrEmpty(elementName))
			{
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(AddParentNodeAndEncodeElementValue(fieldValue));

				XmlNodeList elementsByTagName = xmlDocument.GetElementsByTagName(elementName);

				if (elementsByTagName.Count > 0)
				{
					var element = elementsByTagName.Item(0);
					return element?.InnerXml;
				}
			}
			return string.Empty;
		}

		public static XmlNode GetXmlElementNode(string fieldValue, string elementName)
		{
			if (!string.IsNullOrEmpty(fieldValue) && !string.IsNullOrEmpty(elementName))
			{
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(AddParentNodeAndEncodeElementValue(fieldValue));

				XmlNodeList elementsByTagName = xmlDocument.GetElementsByTagName(elementName);

				if (elementsByTagName.Count > 0)
				{
					return elementsByTagName.Item(0);
				}
			}
			return null;
		}

		public static XmlNodeList GetXmlElementNodeList(string fieldValue, string elementName)
		{
			if (!string.IsNullOrEmpty(fieldValue) && !string.IsNullOrEmpty(elementName))
			{
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(AddParentNodeAndEncodeElementValue(fieldValue));

				XmlNodeList elementsByTagName = xmlDocument.GetElementsByTagName(elementName);

				if (elementsByTagName.Count > 0)
				{
					return elementsByTagName;
				}
			}
			return null;
		}


		private static string AddParentNodeAndEncodeElementValue(string fieldValue)
		{
			if (!fieldValue.StartsWith("<?xml", StringComparison.InvariantCultureIgnoreCase))
			{
				// Add parent xml element to value
				fieldValue = string.Format("<ParentNode>{0}</ParentNode>", fieldValue);
				// Escape special chars in text value
				fieldValue = fieldValue.Replace("&", "&amp;");
			}

			return fieldValue;
		}
	}
}
