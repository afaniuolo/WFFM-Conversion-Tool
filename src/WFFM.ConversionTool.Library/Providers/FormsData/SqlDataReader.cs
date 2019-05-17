using System;
using System.Collections.Generic;
using System.Linq;
using WFFM.ConversionTool.Library.Database.WFFM;

namespace WFFM.ConversionTool.Library.Providers.FormsData
{
	public class SqlDataProvider : IDataProvider
	{
		private Database.WFFM.WFFM _wffmDatabase;

		public SqlDataProvider(Database.WFFM.WFFM wffmDatabase)
		{
			_wffmDatabase = wffmDatabase;
		}

		public List<FormData> GetFormDataRecords(Guid formItemId)
		{
			return _wffmDatabase.FormDatas.Where(x => x.FormItemId == formItemId).ToList();
		}

		public List<FieldData> GetFieldDataRecords(Guid formDataId)
		{
			return _wffmDatabase.FieldDatas.Where(f => f.FormId == formDataId).ToList();
		}
	}
}
