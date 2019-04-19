using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using WFFM.ConversionTool.Library.Factories;
using WFFM.ConversionTool.Library.Helpers;
using WFFM.ConversionTool.Library.Models.Metadata;
using WFFM.ConversionTool.Library.Models.Sitecore;

namespace WFFM.ConversionTool.Library.Converters.FieldConverters
{
	public class DatasourceConverter : BaseFieldConverter
	{
		private IItemFactory _itemFactory;

		public DatasourceConverter(IItemFactory itemFactory)
		{
			_itemFactory = itemFactory;
		}

		public override List<SCItem> ConvertValueElementToItems(SCField scField, string elementValue, MetadataTemplate metadataTemplate, SCItem sourceItem)
		{
			List<SCItem> convertedItems = new List<SCItem>();

			var decodedElementValue = Uri.UnescapeDataString(elementValue).Replace("+", " ");

			var queryElements = XmlHelper.GetXmlElementNodeList(decodedElementValue, "query");
			if (queryElements != null && queryElements.Count > 0)
			{
				foreach (XmlNode queryElement in queryElements)
				{
					if (queryElement.Attributes != null && queryElement.Attributes["t"] != null)
					{
						var queryType = queryElement.Attributes["t"].Value;
						if (string.Equals(queryType, "default", StringComparison.InvariantCultureIgnoreCase))
						{
							var value = XmlHelper.GetXmlElementValue(queryElement.InnerXml, "value");
							if (!string.IsNullOrEmpty(value))
							{
								// Set item value
								metadataTemplate.fields.newFields
									.First(field => field.destFieldId == new Guid("{3A07C171-9BCA-464D-8670-C5703C6D3F11}")).value = value;
								SCItem convertedItem = _itemFactory.Create(metadataTemplate.destTemplateId, sourceItem, value, metadataTemplate);
								convertedItems.Add(convertedItem);							
							}
						}
					}
				}				
			}

			return convertedItems;
		}

		public override List<SCField> ConvertValueElementToFields(SCField scField, string elementValue)
		{
			List<SCField> convertedFields = new List<SCField>();

			var decodedElementValue = Uri.UnescapeDataString(elementValue).Replace("+"," ");

			var queryElement = XmlHelper.GetXmlElementNode(decodedElementValue, "query");
			if (queryElement != null)
			{
				if (queryElement.Attributes != null && queryElement.Attributes["t"] != null)
				{
					var queryType = queryElement.Attributes["t"].Value;
					if (string.Equals(queryType, "root", StringComparison.InvariantCultureIgnoreCase))
					{
						string rootItemId = XmlHelper.GetXmlElementValue(queryElement.InnerXml, "value");
						string textFieldValue = queryElement.Attributes["tf"].Value;
						string valueFieldValue = queryElement.Attributes["vf"].Value;

						// Create fields
						var datasourceField = CreateFieldFromElement(scField, new Guid("{5BE76442-950F-4C1F-A797-BEBD71101ABB}"), rootItemId);
						if (datasourceField != null) convertedFields.Add(datasourceField);

						var displayField = CreateFieldFromElement(scField, new Guid("{492361E0-72D8-4847-82BA-EBFC235CF57B}"), textFieldValue);
						if (displayField != null) convertedFields.Add(displayField);

						var valueField = CreateFieldFromElement(scField, new Guid("{78778432-6327-4CEA-A28B-E190E3541D28}"), valueFieldValue);
						if (valueField != null) convertedFields.Add(valueField);

						var isDynamicField = CreateFieldFromElement(scField, new Guid("{54424E06-0E7A-47A7-8CB2-7383D700472F}"), "1");
						if (isDynamicField != null) convertedFields.Add(isDynamicField);
					}
				}
			}

			return convertedFields;
		}
	}
}
