using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFFM.ConversionTool.Library.Models;

namespace WFFM.ConversionTool.Library.Repositories
{
	public interface IMasterRepository
	{
		void AddOrUpdateForm(SCItem scItem);
		void AddOrUpdateSharedField(SCField scField);
		void AddOrUpdateUnversionedField(SCField scField);
		void AddOrUpdateVersionedField(SCField scField);
	}
}
