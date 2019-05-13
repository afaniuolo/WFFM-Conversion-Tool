using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFFM.ConversionTool.FormsData.Database.Forms;

namespace WFFM.ConversionTool.FormsData.Repositories
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
