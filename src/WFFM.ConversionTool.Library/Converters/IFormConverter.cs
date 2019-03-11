using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFFM.ConversionTool.Library.Models;

namespace WFFM.ConversionTool.Library.Converters
{
	public interface IFormConverter
	{
		SCItem Convert(SCItem scItem);
	}
}
