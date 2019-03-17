using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using WFFM.ConversionTool.Library.Factories;
using WFFM.ConversionTool.Library.Models;
using WFFM.ConversionTool.Library.Models.Metadata;
using WFFM.ConversionTool.Library.Models.Sitecore;

namespace WFFM.ConversionTool.Library.Converters
{
	public class FormConverter : IItemConverter
	{
		private IFieldFactory _fieldFactory;
		private MetadataTemplate _formMetadataTemplate;
		private AppSettings _appSettings;

		private readonly Guid DestFormsFolderID = new Guid("B701850A-CB8A-4943-B2BC-DDDB1238C103");

		public FormConverter(IFieldFactory fieldFactory, AppSettings appSettings)
		{
			_fieldFactory = fieldFactory;
			_appSettings = appSettings;
		}

		public SCItem Convert(SCItem scItem)
		{
			ReadFormMetadata(scItem.TemplateID);
			return ConvertFormItemAndFields(scItem);
		}

		private void ReadFormMetadata(Guid sourceTemplateId)
		{
			// Read json file
			var filePath = string.Format("{0}/{1}", _appSettings.metadataFolderRelativePath, _appSettings.metadataFiles.FirstOrDefault(m => m.sourceTemplateId == sourceTemplateId)?.metadataFileName);
			var formMeta = System.IO.File.ReadAllText(filePath);
			// Deserialize Json to Object
			_formMetadataTemplate = JsonConvert.DeserializeObject<MetadataTemplate>(formMeta);
		}

		private SCItem ConvertFormItemAndFields(SCItem sourceFormItem)
		{
			return new SCItem()
			{
				ID = sourceFormItem.ID,
				Name = sourceFormItem.Name,
				MasterID = Guid.Empty,
				ParentID = DestFormsFolderID,
				Created = sourceFormItem.Created,
				Updated = sourceFormItem.Updated,
				TemplateID = _formMetadataTemplate.templateId,
				Fields = ConvertFields(sourceFormItem.Fields)
			};
		}

		private List<SCField> ConvertFields(List<SCField> fields)
		{
			var destFields = new List<SCField>();

			var itemId = fields.First().ItemId;

			IEnumerable<Tuple<string,int>> langVersions = fields.Where(f => f.Version != null && f.Language != null).Select(f => new Tuple<string,int>(f.Language, (int)f.Version)).Distinct();
			var languages = fields.Where(f => f.Language != null).Select(f => f.Language).Distinct();

			foreach (var existingField in _formMetadataTemplate.fields.existingFields)
			{
				SCField destField = null;
				IFieldConverter converter;
				var sourceField = fields.FirstOrDefault(f => f.FieldId == existingField.fieldId);
				if(!string.IsNullOrEmpty(existingField.fieldConverter))
				{
					converter = ConverterInstantiator.CreateInstance(_appSettings.converters.FirstOrDefault(c => c.name == existingField.fieldConverter)?.converterType);
					destField = converter?.Convert(sourceField);
				}
				else
				{
					destField = sourceField;
				}

				if (destField != null && destField.FieldId != Guid.Empty)
				{
					destFields.Add(destField);
				}
			}

			foreach (var newField in _formMetadataTemplate.fields.newFields)
			{
				SCField destField = null;

				switch (newField.fieldType)
				{
					case FieldType.Shared:
						destField = _fieldFactory.CreateSharedField(newField.fieldId, itemId, newField.value);
						if (destField != null)
						{
							destFields.Add(destField);
						}
						break;
					case FieldType.Versioned:
						foreach (var langVersion in langVersions)
						{
							destField = _fieldFactory.CreateVersionedField(newField.fieldId, itemId, newField.value, langVersion.Item2, langVersion.Item1);
							if (destField != null)
							{
								destFields.Add(destField);
							}
						}
						break;
					case FieldType.Unversioned:
						foreach (var language in languages)
						{
							destField = _fieldFactory.CreateUnversionedField(newField.fieldId, itemId, newField.value, language);
							if (destField != null)
							{
								destFields.Add(destField);
							}
						}
						break;
					//default:
						//throw new ArgumentOutOfRangeException(); TODO: To implement meanful error message
				}
			}
			
			return destFields;
		}
	}
}
