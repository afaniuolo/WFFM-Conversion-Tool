using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFFM.ConversionTool.Library.Models.Sitecore;

namespace WFFM.ConversionTool.Library.Processors
{
	public interface IItemProcessor
	{
		void ConvertAndWriteItem(SCItem sourceItem, Guid parentId);
		Guid WriteNewItem(Guid destTemplateId, SCItem parentItem, string itemName);
	}
}
