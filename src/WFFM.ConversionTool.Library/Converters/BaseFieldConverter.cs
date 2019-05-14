using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFFM.ConversionTool.Library.Models.Metadata;
using WFFM.ConversionTool.Library.Models.Sitecore;

namespace WFFM.ConversionTool.Library.Converters
{
	public class BaseFieldConverter : IFieldConverter
	{
		public virtual string ConvertValue(string sourceValue)
		{
			return sourceValue;
		}

		public SCField ConvertField(SCField scField, Guid destFieldId)
		{
			var convertedValue = ConvertValue(scField.Value);
			if (convertedValue == null) return null;
			return CreateField(scField, destFieldId, convertedValue);
		}

		public virtual SCField ConvertValueElement(SCField scField, Guid destFieldId, string elementValue, List<SCItem> destItems = null)
		{
			var convertedValue = ConvertValue(elementValue);
			if (convertedValue == null) return null;
			return CreateFieldFromElement(scField, destFieldId, convertedValue);
		}

		public virtual List<SCItem> ConvertValueElementToItems(SCField scField, string elementValue, MetadataTemplate metadataTemplate, SCItem sourceItem)
		{
			return new List<SCItem>();
		}

		public virtual List<SCField> ConvertValueElementToFields(SCField scField, string elementValue)
		{
			return new List<SCField>();
		}

		public SCField CreateFieldFromElement(SCField scField, Guid destFieldId, string convertedValue)
		{
			return CreateFieldFromElement(scField, destFieldId, convertedValue, scField.Type);
		}

		public SCField CreateFieldFromElement(SCField scField, Guid destFieldId, string convertedValue, FieldType fieldType)
		{
			return new SCField()
			{
				Created = DateTime.UtcNow,
				Updated = DateTime.UtcNow,
				ItemId = scField.ItemId,
				Language = scField.Language,
				Version = scField.Version,
				Type = fieldType,
				Value = convertedValue,
				FieldId = destFieldId,
				Id = Guid.NewGuid()
			};
		}

		public SCField CreateField(SCField scField, Guid destFieldId, string convertedValue)
		{
			return new SCField()
			{
				Created = DateTime.Now,
				Updated = DateTime.Now,
				ItemId = scField.ItemId,
				Language = scField.Language,
				Version = scField.Version,
				Type = scField.Type,
				Value = convertedValue,
				FieldId = destFieldId,
				Id = scField.Id
			};
		}
	}
}
