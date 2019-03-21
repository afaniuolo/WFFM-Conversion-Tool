using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFFM.ConversionTool.Library.Models;
using WFFM.ConversionTool.Library.Models.Sitecore;

namespace WFFM.ConversionTool.Library.Factories
{
	public class FieldFactory : IFieldFactory
	{
		public SCField CreateSharedField(Guid fieldId, Guid itemID, string value)
		{
			return CreateField(fieldId, itemID, value, FieldType.Shared);
		}

		public SCField CreateUnversionedField(Guid fieldId, Guid itemID, string value, string language)
		{
			return CreateField(fieldId, itemID, value, FieldType.Unversioned, null, language);
		}

		public SCField CreateVersionedField(Guid fieldId, Guid itemID, string value, int version, string language)
		{
			return CreateField(fieldId, itemID, value, FieldType.Versioned, version, language);
		}

		private SCField CreateField(Guid fieldId, Guid itemID, string value, FieldType fieldType, int? version = null, string language = null)
		{
			return new SCField()
			{
				Id = Guid.NewGuid(),
				FieldId = fieldId,
				Created = DateTime.Now,
				ItemId = itemID,
				Updated = DateTime.Now,
				Type = fieldType,
				Value = value,
				Version = version,
				Language = language
			};
		}
	}
}
