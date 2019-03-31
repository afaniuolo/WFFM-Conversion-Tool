using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFFM.ConversionTool.Library.Models;
using WFFM.ConversionTool.Library.Models.Sitecore;

namespace WFFM.ConversionTool.Library.Converters.FieldConverters
{
	public class TrackingConverter : BaseFieldConverter
	{
		public override string ConvertValue(string sourceValue)
		{
			return sourceValue.Replace(" ","").Contains("<tracking></tracking>") ? string.Empty : "1";
		}
	}
}
