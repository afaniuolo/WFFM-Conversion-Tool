using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WFFM.ConversionTool.Library.Converters.FieldConverters
{
	public class SelectionModeConverter : BaseFieldConverter
	{
		public override string ConvertValue(string sourceValue)
		{
			return sourceValue.ToLower() == "multiple" ? "1" : "";
		}
	}
}
