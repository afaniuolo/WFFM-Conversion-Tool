using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFFM.ConversionTool.Library.Helpers;
using WFFM.ConversionTool.Library.Models.Sitecore;

namespace WFFM.ConversionTool.Library.Converters.FieldConverters
{
	public class SelectedValueConverter : BaseFieldConverter
	{
		public override SCField ConvertValueElement(SCField scField, Guid destFieldId, string elementValue, List<SCItem> destItems = null)
		{
			if (destItems == null || !destItems.Any())
			{
				elementValue = string.Empty;
				return base.ConvertValueElement(scField, destFieldId, elementValue, destItems);
			}

			var itemElements = XmlHelper.GetXmlElementNodeList(elementValue, "item");
			if (itemElements == null || itemElements.Count == 0)
			{
				elementValue = string.Empty;
				return base.ConvertValueElement(scField, destFieldId, elementValue, destItems);
			}

			var firstSelectedItemValue = itemElements.Item(0)?.InnerXml ?? string.Empty;

			var selectedItemId = destItems.FirstOrDefault(i =>
				                     i.TemplateID == new Guid("{B3BDFE59-6667-4432-B261-05D0E3F7FDF6}") // Item is Extendend List Item
				                     && string.Equals(
					                     i.Fields.FirstOrDefault(f => f.FieldId == new Guid("{3A07C171-9BCA-464D-8670-C5703C6D3F11}"))?.Value, // Select Field Value
					                     firstSelectedItemValue, StringComparison.InvariantCultureIgnoreCase))?.ID.ToString("B").ToUpper() ??
			                     string.Empty; 

			return CreateFieldFromElement(scField, destFieldId, selectedItemId);
		}
	}
}
