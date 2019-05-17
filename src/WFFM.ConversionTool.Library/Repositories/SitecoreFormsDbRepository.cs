using System.Data.Entity.Migrations;
using WFFM.ConversionTool.Library.Database.Forms;

namespace WFFM.ConversionTool.Library.Repositories
{
	public class SitecoreFormsDbRepository : ISitecoreFormsDbRepository
	{
		private SitecoreForms _sitecoreFormsDb;

		public SitecoreFormsDbRepository(SitecoreForms sitecoreFormsDb)
		{
			_sitecoreFormsDb = sitecoreFormsDb;
		}

		public void CreateOrUpdateFormData(FormEntry formEntry)
		{
			_sitecoreFormsDb.FormEntries.AddOrUpdate(formEntry);
			_sitecoreFormsDb.SaveChanges();
		}
	}
}
