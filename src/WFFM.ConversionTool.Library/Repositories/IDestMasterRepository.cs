using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFFM.ConversionTool.Library.Models;
using WFFM.ConversionTool.Library.Models.Sitecore;

namespace WFFM.ConversionTool.Library.Repositories
{
	public interface IDestMasterRepository
	{
		void AddOrUpdateForm(SCItem scItem);
		void AddOrUpdateSharedField(SCField scField);
		void AddOrUpdateUnversionedField(SCField scField);
		void AddOrUpdateVersionedField(SCField scField);
	}
}
