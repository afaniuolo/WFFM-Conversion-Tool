using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WFFM.ConversionTool.Library.Converters.FieldValueConverters
{
	public class BooleanConverter : BaseFieldConverter
	{
		public override string ConvertValue(string sourceValue)
		{
			return string.Equals(sourceValue, "1", StringComparison.InvariantCultureIgnoreCase) ? "True" : "False";
		}
	}
}
