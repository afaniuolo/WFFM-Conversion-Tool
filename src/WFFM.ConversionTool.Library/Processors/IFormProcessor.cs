using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WFFM.ConversionTool.Library.Processors
{
	public interface IFormProcessor
	{
		List<Guid> ConvertForms();
	}
}
