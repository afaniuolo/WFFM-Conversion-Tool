using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WFFM.ConversionTool.Library.Converters.FieldConverters
{
	public class CheckedConverter : BaseFieldConverter
	{
		public override string ConvertValue(string sourceValue)
		{
			return sourceValue.ToLower() == "yes" ? "1" : "";
		}
	}
}
