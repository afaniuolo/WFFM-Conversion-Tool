using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFFM.ConversionTool.Library.Models;
using WFFM.ConversionTool.Library.Models.Metadata;
using WFFM.ConversionTool.Library.Models.Sitecore;

namespace WFFM.ConversionTool.Library.Converters
{
	public interface IFieldConverter
	{
		string ConvertValue(string sourceValue);
		SCField ConvertField(SCField scField, Guid destFieldId);
		SCField ConvertValueElement(SCField scField, Guid destFieldId, string elementValue);
		List<SCItem> ConvertValueElementToItems(SCField scField, string elementValue);
		List<SCField> ConvertValueElementToFields(SCField scField, string elementValue);
	}
}
