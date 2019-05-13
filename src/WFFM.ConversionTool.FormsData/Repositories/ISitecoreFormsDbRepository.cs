using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFFM.ConversionTool.FormsData.Database.Forms;

namespace WFFM.ConversionTool.FormsData.Repositories
{
	public interface ISitecoreFormsDbRepository
	{
		void CreateOrUpdateFormData(FormEntry formEntry);
	}
}
