using System;
using System.Collections.Generic;
using WFFM.ConversionTool.Library.Database.MongoDB;
using FieldData = WFFM.ConversionTool.Library.Database.WFFM.FieldData;
using FormData = WFFM.ConversionTool.Library.Database.WFFM.FormData;

namespace WFFM.ConversionTool.Library.Providers.FormsData
{
	public class MongoDbDataProvider : IDataProvider
	{
		private MongoAnalytics _mongoAnalytics;

		public MongoDbDataProvider(MongoAnalytics mongoAnalytics)
		{
			_mongoAnalytics = mongoAnalytics;
		}

		public List<FormData> GetFormDataRecords(Guid formItemId)
		{
			var mongoDbFormData = _mongoAnalytics.GetFormDataByFormItemId(formItemId);
			var formData = new List<FormData>();

			foreach (var data in mongoDbFormData)
			{
				formData.Add(new FormData()
				{
					Id = data.Id,
					ContactId = data.ContactId,
					Data = string.Empty,
					FormItemId = data.FormId,
					InteractionId = data.InteractionId,
					TimeStamp = data.Timestamp
				});
			}

			return formData;
		}

		public List<FieldData> GetFieldDataRecords(Guid formRecordId)
		{
			var mongoDbFormData = _mongoAnalytics.GetFormDataByFormRecordId(formRecordId);
			var fieldDatas = new List<FieldData>();

			foreach (var fieldData in mongoDbFormData.FieldDatas)
			{
				fieldDatas.Add(new FieldData()
				{
					Id = Guid.NewGuid(),
					FieldItemId = fieldData.FieldId,
					Data = fieldData.Data,
					Value = fieldData.Value,
					FieldName = fieldData.FieldName,
					FormId = formRecordId
				});
			}

			return fieldDatas;
		}
	}
}