using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WFFM.ConversionTool.Library.Converters.FieldConverters
{
	public class DateConverter : BaseFieldConverter
	{
		public override string ConvertValue(string sourceValue)
		{
			DateTime sourceDate;
			if (DateTime.TryParseExact(sourceValue, "yyyyMMddThhmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out sourceDate))
			{
				return sourceDate.ToUniversalTime().ToString("yyyyMMddThhmmssZ");
			}
			return sourceValue;
		}
	}
}
