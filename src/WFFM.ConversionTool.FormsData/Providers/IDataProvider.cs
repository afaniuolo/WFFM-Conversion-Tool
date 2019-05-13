using System;
using System.Collections.Generic;
using WFFM.ConversionTool.FormsData.Database.WFFM;

namespace WFFM.ConversionTool.FormsData.Providers
{
	public interface IDataProvider
	{
		List<FormData> GetFormDataRecords(Guid formItemId);
		List<FieldData> GetFieldDataRecords(Guid formDataId);
	}
}
