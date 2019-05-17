using System;
using System.Collections.Generic;
using WFFM.ConversionTool.Library.Database.WFFM;

namespace WFFM.ConversionTool.Library.Providers.FormsData
{
	public interface IDataProvider
	{
		List<FormData> GetFormDataRecords(Guid formItemId);
		List<FieldData> GetFieldDataRecords(Guid formDataId);
	}
}
