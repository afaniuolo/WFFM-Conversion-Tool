using WFFM.ConversionTool.Library.Database.Forms;

namespace WFFM.ConversionTool.Library.Repositories
{
	public interface ISitecoreFormsDbRepository
	{
		void CreateOrUpdateFormData(FormEntry formEntry);
	}
}
