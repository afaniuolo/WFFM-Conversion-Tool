using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFFM.ConversionTool.Library.Models.Metadata;
using WFFM.ConversionTool.Library.Models.Sitecore;

namespace WFFM.ConversionTool.Library.Converters.FieldConverters
{
	public class FieldTypeConverter : BaseFieldConverter
	{
		public override string ConvertValue(string sourceValue)
		{
			var container = IoC.Initialize();
			var appSettings = container.GetInstance<AppSettings>();

			var fieldTypeMapping = appSettings.inputTypesMapping.Where(m => m.destFieldTypeId != null).FirstOrDefault(f => string.Equals(f.sourceFieldLinkId.ToString("B"),sourceValue, StringComparison.InvariantCultureIgnoreCase));
			return fieldTypeMapping?.destFieldTypeId.ToString();
		}
	}
}
