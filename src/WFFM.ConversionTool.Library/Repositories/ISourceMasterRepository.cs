using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFFM.ConversionTool.Library.Models;
using WFFM.ConversionTool.Library.Models.Sitecore;

namespace WFFM.ConversionTool.Library.Repositories
{
	public interface ISourceMasterRepository
	{
		SCItem GetSitecoreItem(Guid itemId);
		List<SCItem> GetSitecoreItems(Guid templateId);
		bool ItemHasChildrenOfTemplate(Guid templateId, SCItem scItem);
		List<SCItem> GetSitecoreChildrenItems(Guid templateId, Guid parentId);
	}
}
