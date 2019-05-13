using System;
using System.Collections.Generic;
using WFFM.ConversionTool.FormsData.Database.WFFM;

namespace WFFM.ConversionTool.FormsData.Providers
{
	public class MongoDbDataProvider : IDataProvider
	{
		public List<FormData> GetFormDataRecords(Guid formItemId)
		{
			throw new NotImplementedException();
		}

		public List<FieldData> GetFieldDataRecords(Guid formDataId)
		{
			throw new NotImplementedException();
		}
	}
}
