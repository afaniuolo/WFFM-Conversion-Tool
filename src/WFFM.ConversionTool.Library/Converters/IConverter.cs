using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WFFM.ConversionTool.Library.Converters
{
	public interface IConverter
	{
		bool CanConvert(dynamic fieldType);
		dynamic Convert(dynamic source);
	}
}
