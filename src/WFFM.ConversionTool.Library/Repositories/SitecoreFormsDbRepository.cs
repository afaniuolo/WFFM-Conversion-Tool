using System;
using System.Data.Entity.Migrations;
using System.Linq;
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

			foreach (FieldData fieldData in formEntry.FieldDatas)
			{
				_sitecoreFormsDb.FieldDatas.AddOrUpdate(fieldData);
			}
			_sitecoreFormsDb.SaveChanges();
		}

		public void DeleteFieldDataByFormRecordId(Guid formRecordId)
		{
			_sitecoreFormsDb.FieldDatas.RemoveRange(_sitecoreFormsDb.FieldDatas.Where(f => f.FormEntryID == formRecordId));
		}
	}
}
