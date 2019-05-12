using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFFM.ConversionTool.Library.Models.Sitecore;

namespace WFFM.ConversionTool.Library.Providers
{
	public interface IFieldProvider
	{
		Dictionary<Tuple<string, int>, string> GetFieldValues(SCItem sourceItem, Guid sourceFieldId, string defaultValue,
			bool stripHtml = false);
	}
}
