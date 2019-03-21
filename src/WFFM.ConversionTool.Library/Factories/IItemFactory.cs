using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFFM.ConversionTool.Library.Models.Sitecore;

namespace WFFM.ConversionTool.Library.Factories
{
	public interface IItemFactory
	{
		SCItem Create(Guid destTemplateId, SCItem parentItem);
	}
}
