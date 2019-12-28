using System;
using System.Collections.Generic;
using WFFM.ConversionTool.Library.Database.Forms;

namespace WFFM.ConversionTool.Library.Repositories
{
	public interface ISitecoreFormsDbRepository
	{
		void CreateOrUpdateFormData(FormEntry formEntry);
		void DeleteFieldDataByFormRecordId(Guid formRecordId);
		void CreateOrUpdateFileStorageFormRecord(FileStorage fileStorage);
	}
}
