using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using WFFM.ConversionTool.Library.Models;
using WFFM.ConversionTool.Library.Models.Metadata;
using WFFM.ConversionTool.Library.Models.Sitecore;

namespace WFFM.ConversionTool.Library.Factories
{
	public class FieldFactory : IFieldFactory
	{
		public List<SCField> CreateFields(MetadataTemplate.MetadataFields.MetadataNewField metadataNewField, Guid itemId, IEnumerable<Tuple<string, int>> langVersions, IEnumerable<string> languages)
		{
			SCField destField = new SCField();
			List<SCField> destFields = new List<SCField>();

			var fieldValue = GetValue(metadataNewField.value, metadataNewField.valueType);

			switch (metadataNewField.fieldType)
			{
				case FieldType.Shared:
					destField = CreateSharedField(metadataNewField.destFieldId, itemId, fieldValue);
					if (destField != null)
					{
						destFields.Add(destField);
					}
					break;
				case FieldType.Versioned:
					foreach (var langVersion in langVersions)
					{
						if (metadataNewField.values != null)
						{
							fieldValue = metadataNewField.values[langVersion] ?? fieldValue;
						}
						destField = CreateVersionedField(metadataNewField.destFieldId, itemId, fieldValue, langVersion.Item2, langVersion.Item1);
						if (destField != null)
						{
							destFields.Add(destField);
						}
					}
					break;
				case FieldType.Unversioned:
					foreach (var language in languages)
					{
						var langVersion = new Tuple<string,int>(language, 1);
						if (metadataNewField.values != null)
						{
							fieldValue = metadataNewField.values[langVersion] ?? fieldValue;
						}
						destField = CreateUnversionedField(metadataNewField.destFieldId, itemId, fieldValue, language);
						if (destField != null)
						{
							destFields.Add(destField);
						}
					}
					break;
				//default:
				//throw new ArgumentOutOfRangeException(); TODO: To implement meanful error message
			}

			return destFields;
		}

		private SCField CreateSharedField(Guid fieldId, Guid itemID, string value)
		{
			return CreateField(fieldId, itemID, value, FieldType.Shared);
		}

		private SCField CreateUnversionedField(Guid fieldId, Guid itemID, string value, string language)
		{
			return CreateField(fieldId, itemID, value, FieldType.Unversioned, null, language);
		}

		private SCField CreateVersionedField(Guid fieldId, Guid itemID, string value, int version, string language)
		{
			return CreateField(fieldId, itemID, value, FieldType.Versioned, version, language);
		}

		private SCField CreateField(Guid fieldId, Guid itemID, string value, FieldType fieldType, int? version = null, string language = null)
		{
			return new SCField()
			{
				Id = Guid.NewGuid(),
				FieldId = fieldId,
				Created = DateTime.UtcNow,
				ItemId = itemID,
				Updated = DateTime.UtcNow,
				Type = fieldType,
				Value = value,
				Version = version,
				Language = language
			};
		}

		private string GetValue(string value, string valueType)
		{
			return value ?? GenerateValue(valueType);
		}

		private string GenerateValue(string valueType)
		{
			var value = string.Empty;
			switch (valueType.ToLower())
			{
				case "system.datetime":
					value = DateTime.UtcNow.ToString("yyyyMMddThhmmssZ");
					break;
				case "system.guid":
					value = Guid.NewGuid().ToString();
					break;
				case "system.guid.tostring":
					value = Guid.NewGuid().ToString("N").ToUpper();
					break;
				default:
					value = string.Empty;
					break;
			}

			return value;
		}
	}
}
