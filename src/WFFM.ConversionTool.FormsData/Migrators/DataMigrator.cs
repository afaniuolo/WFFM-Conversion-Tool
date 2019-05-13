using System;
using System.Collections.Generic;
using System.Linq;
using WFFM.ConversionTool.FormsData.Database.Forms;
using WFFM.ConversionTool.FormsData.Database.WFFM;
using WFFM.ConversionTool.FormsData.Providers;
using WFFM.ConversionTool.FormsData.Repositories;
using FieldData = WFFM.ConversionTool.FormsData.Database.Forms.FieldData;

namespace WFFM.ConversionTool.FormsData.Migrators
{
	public class DataMigrator : IDataMigrator
	{
		private IDataProvider _dataProvider;
		private ISitecoreFormsDbRepository _sitecoreFormsDbRepository;

		public DataMigrator(IDataProvider dataProvider, ISitecoreFormsDbRepository sitecoreFormsDbRepository)
		{
			_dataProvider = dataProvider;
			_sitecoreFormsDbRepository = sitecoreFormsDbRepository;
		}

		public void MigrateData(Guid formId)
		{
			var formDataRecords = _dataProvider.GetFormDataRecords(formId);

			foreach (FormData formDataRecord in formDataRecords)
			{
				var fieldDataRecords = _dataProvider.GetFieldDataRecords(formDataRecord.Id);

				List<FieldData> fieldDataFormsRecords = fieldDataRecords.Select(ConvertFieldData).ToList();

				FormEntry formEntry = new FormEntry()
				{
					ID = formDataRecord.Id,
					FormItemID = formDataRecord.FormItemId,
					Created = formDataRecord.TimeStamp,
					FieldDatas = fieldDataFormsRecords
				};

				_sitecoreFormsDbRepository.CreateOrUpdateFormData(formEntry);
			}
		}

		private FieldData ConvertFieldData(Database.WFFM.FieldData wffmFieldData)
		{
			return new FieldData()
			{
				FieldItemID = wffmFieldData.FieldItemId,
				FieldName = wffmFieldData.FieldName,
				FormEntryID = wffmFieldData.FormId,
				ID = wffmFieldData.Id,
				Value = wffmFieldData.Value,
				ValueType = SetFieldDataValueType(wffmFieldData.Value)
			};
		}

		private string SetFieldDataValueType(string value)
		{
			return "System.String";
		}
	}
}
