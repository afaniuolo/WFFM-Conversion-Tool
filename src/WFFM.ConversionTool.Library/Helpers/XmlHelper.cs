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
			fieldValue = SanitizeFieldValue(fieldValue);
			try
			{
				xmlDocument.LoadXml(AddParentNodeAndEncodeElementValue(fieldValue));

				foreach (XmlNode childNode in xmlDocument.ChildNodes.Item(0).ChildNodes)
				{
					elementNames.Add(childNode.Name);
				}
			}
			catch (Exception e)
			{
				Console.WriteLine();
				Console.WriteLine("XmlHelper - GetXmlElementNames - Failed to parse Xml value - Value = " + fieldValue);
				Console.WriteLine(e);
				Console.WriteLine();
			}

			return elementNames;
		}

		public static string GetXmlElementValue(string fieldValue, string elementName)
		{
			if (!string.IsNullOrEmpty(fieldValue) && !string.IsNullOrEmpty(elementName))
			{
				XmlDocument xmlDocument = new XmlDocument();
				fieldValue = SanitizeFieldValue(fieldValue);
				try
				{
					xmlDocument.LoadXml(AddParentNodeAndEncodeElementValue(fieldValue));

					XmlNodeList elementsByTagName = xmlDocument.GetElementsByTagName(elementName);

					if (elementsByTagName.Count > 0)
					{
						var element = elementsByTagName.Item(0);
						return element?.InnerXml;
					}
				}
				catch (Exception e)
				{
					Console.WriteLine();
					Console.WriteLine("XmlHelper - GetXmlElementValue - Failed to parse Xml value - Value = " + fieldValue);
					Console.WriteLine(e);
					Console.WriteLine();
				}			
			}
			return string.Empty;
		}

		public static XmlNode GetXmlElementNode(string fieldValue, string elementName)
		{
			if (!string.IsNullOrEmpty(fieldValue) && !string.IsNullOrEmpty(elementName))
			{
				XmlDocument xmlDocument = new XmlDocument();
				fieldValue = SanitizeFieldValue(fieldValue);
				try
				{
					xmlDocument.LoadXml(AddParentNodeAndEncodeElementValue(fieldValue));

					XmlNodeList elementsByTagName = xmlDocument.GetElementsByTagName(elementName);

					if (elementsByTagName.Count > 0)
					{
						return elementsByTagName.Item(0);
					}
				}
				catch (Exception e)
				{
					Console.WriteLine();
					Console.WriteLine("XmlHelper - GetXmlElementNode - Failed to parse Xml value - Value = " + fieldValue);
					Console.WriteLine(e);
					Console.WriteLine();
				}			
			}
			return null;
		}

		public static XmlNodeList GetXmlElementNodeList(string fieldValue, string elementName)
		{
			if (!string.IsNullOrEmpty(fieldValue) && !string.IsNullOrEmpty(elementName))
			{
				XmlDocument xmlDocument = new XmlDocument();
				fieldValue = SanitizeFieldValue(fieldValue);
				try
				{
					xmlDocument.LoadXml(AddParentNodeAndEncodeElementValue(fieldValue));

					XmlNodeList elementsByTagName = xmlDocument.GetElementsByTagName(elementName);

					if (elementsByTagName.Count > 0)
					{
						return elementsByTagName;
					}
				}
				catch (Exception e)
				{
					Console.WriteLine();
					Console.WriteLine("XmlHelper - GetXmlElementNodeList - Failed to parse Xml value - Value = " + fieldValue);
					Console.WriteLine(e);
					Console.WriteLine();
				}				
			}
			return null;
		}

		public static string StripHtml(string fieldValue)
		{
			if (!string.IsNullOrEmpty(fieldValue))
			{
				XmlDocument xmlDocument = new XmlDocument();
				fieldValue = SanitizeFieldValue(fieldValue);
				try
				{
					xmlDocument.LoadXml(AddParentNodeAndEncodeElementValue(fieldValue));
					return xmlDocument.InnerText;
				}
				catch (Exception e)
				{
					Console.WriteLine();
					Console.WriteLine("XmlHelper - StripHtml - Failed to parse Xml value - Value = " + fieldValue);
					Console.WriteLine(e);
					Console.WriteLine();
				}
				
			}
			return fieldValue;
		}


		private static string AddParentNodeAndEncodeElementValue(string fieldValue)
		{
			if (!fieldValue.StartsWith("<?xml", StringComparison.InvariantCultureIgnoreCase))
			{
				// Add parent xml element to value
				fieldValue = string.Format("<ParentNode>{0}</ParentNode>", fieldValue);
				// Escape special chars in text value
				fieldValue = fieldValue.Replace(" & ", " &amp; ").Replace(" &<"," &amp;<");
			}

			return fieldValue;
		}

		private static string SanitizeFieldValue(string fieldValue)
		{
			return fieldValue.Replace("<br>", "<br/>")
				.Replace("</em<","</em><")
				.Replace("</b<","</b><")
				.Replace("</a<","</a><")
				.Replace("</strong<","</strong><")
				.Replace("</i<","</i><")
				.Replace("&rsquo;", "’")
				.Replace("&lsquo;", "‘")
				.Replace("&nbsp;"," ");
		}
	}
}
