using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFFM.ConversionTool.Library.Models;

namespace WFFM.ConversionTool.Library.Factories
{
	public class FieldFactory : IFieldFactory
	{
		public SCField CreateSharedField(Guid fieldId, Guid itemID, string value)
		{
			return CreateField(fieldId, itemID, value);
		}

		public SCField CreateUnversionedField(Guid fieldId, Guid itemID, string value, string language)
		{
			return CreateField(fieldId, itemID, value, null, language);
		}

		public SCField CreateVersionedField(Guid fieldId, Guid itemID, string value, int version, string language)
		{
			return CreateField(fieldId, itemID, value, version, language);
		}

		private SCField CreateField(Guid fieldId, Guid itemID, string value, int? version = null, string language = null)
		{
			return new SCField()
			{
				Id = Guid.NewGuid(),
				FieldId = fieldId,
				Created = DateTime.Now,
				ItemId = itemID,
				Updated = DateTime.Now,
				Type = FieldType.Shared,
				Value = value,
				Version = version,
				Language = language
			};
		}
	}
}
