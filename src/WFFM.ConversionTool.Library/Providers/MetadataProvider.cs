using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using WFFM.ConversionTool.Library.Models.Metadata;

namespace WFFM.ConversionTool.Library.Providers
{
	public class MetadataProvider : IMetadataProvider
	{
		private AppSettings _appSettings;
		private List<MetadataTemplate> _metadataTemplates = new List<MetadataTemplate>();
		private string[] _metadataFiles;

		public MetadataProvider(AppSettings appSettings)
		{
			_appSettings = appSettings;

			_metadataFiles = GetMetadataFileList();

			foreach (string filePath in _metadataFiles)
			{
				_metadataTemplates.Add(GetItemMetadataByFilePath(filePath));
			}
		}

		public MetadataTemplate GetItemMetadataByTemplateId(Guid templateId)
		{
			return _metadataTemplates.FirstOrDefault(m => m.sourceTemplateId == templateId || m.destTemplateId == templateId);
		}

		public MetadataTemplate GetItemMetadataByTemplateName(string templateName)
		{
			return _metadataTemplates.FirstOrDefault(m => string.Equals(m.sourceTemplateName, templateName, StringComparison.InvariantCultureIgnoreCase) 
			                                              || string.Equals(m.destTemplateName, templateName, StringComparison.InvariantCultureIgnoreCase));
		}

		public MetadataTemplate GetItemMetadataBySourceMappingFieldValue(string mappingValue)
		{
			return _metadataTemplates.FirstOrDefault(m => string.Equals(m.sourceMappingFieldValue, mappingValue, StringComparison.InvariantCultureIgnoreCase));
		}
		
		private string[] GetMetadataFileList()
		{
			var metadataDirPath = _appSettings.metadataFolderRelativePath;
			return Directory.GetFiles(metadataDirPath, "*.json", SearchOption.AllDirectories);
		}

		private MetadataTemplate GetItemMetadataByFilePath(string filePath)
		{
			// Read json file
			var itemMeta = System.IO.File.ReadAllText(filePath);
			// Deserialize Json to Object
			MetadataTemplate metadataTemplate = JsonConvert.DeserializeObject<MetadataTemplate>(itemMeta);

			if (string.IsNullOrEmpty(metadataTemplate.baseTemplateMetadataFileName)) return metadataTemplate;
			var baseTemplateMetadataFilePath = _metadataFiles.FirstOrDefault(f => GetFileName(f).Equals(metadataTemplate.baseTemplateMetadataFileName, StringComparison.InvariantCultureIgnoreCase));
			var fullMetadataTemplate = MergeBaseMetadataTemplate(metadataTemplate, baseTemplateMetadataFilePath);

			return fullMetadataTemplate;
		}

		private string GetFileName(string filePath)
		{
			var filename = filePath.Split('\\').Last();
			return filename;
		}

		private MetadataTemplate MergeBaseMetadataTemplate(MetadataTemplate metadataTemplate, string baseTemplateMetadataFilePath)
		{
			if (string.IsNullOrEmpty(baseTemplateMetadataFilePath)) return metadataTemplate;

			MetadataTemplate baseTemplateMeta = GetItemMetadataByFilePath(baseTemplateMetadataFilePath);

			if (baseTemplateMeta == null) return metadataTemplate;

			// Iterate merging if baseTemplate filename is not null
			if (!string.IsNullOrEmpty(baseTemplateMeta.baseTemplateMetadataFileName))
			{
				var filePath = _metadataFiles.FirstOrDefault(f => f.IndexOf(baseTemplateMeta.baseTemplateMetadataFileName, StringComparison.InvariantCultureIgnoreCase) > -1);
				baseTemplateMeta = MergeBaseMetadataTemplate(baseTemplateMeta, filePath);
			}

			// Merge Fields
			metadataTemplate.fields = MergeFields(baseTemplateMeta.fields, metadataTemplate.fields);

			// Merge Descendant Items
			metadataTemplate.descendantItems =
				MergeDescendantItems(baseTemplateMeta.descendantItems, metadataTemplate.descendantItems);

			return metadataTemplate;
		}

		private List<MetadataTemplate.DescendantItem> MergeDescendantItems(
			List<MetadataTemplate.DescendantItem> baseDescendantItems, List<MetadataTemplate.DescendantItem> metaDescendantItems)
		{
			if (baseDescendantItems != null)
			{
				if (metaDescendantItems != null)
				{
					foreach (var baseDescendantItem in baseDescendantItems)
					{
						if (!metaDescendantItems.Any(f =>
							string.Equals(f.itemName, baseDescendantItem.itemName, StringComparison.InvariantCultureIgnoreCase)
							&& string.Equals(f.destTemplateName, baseDescendantItem.destTemplateName,
								StringComparison.InvariantCultureIgnoreCase)
							&& string.Equals(f.parentItemName, baseDescendantItem.parentItemName)
							&& f.isParentChild == baseDescendantItem.isParentChild))
						{
							metaDescendantItems.Add(baseDescendantItem);
						}
					}
				}
				else
				{
					return baseDescendantItems;
				}
			}

			return metaDescendantItems;
		}

		private MetadataTemplate.MetadataFields MergeFields(MetadataTemplate.MetadataFields baseFields,
			MetadataTemplate.MetadataFields metaFields)
		{
			// Add base fields
			if (baseFields.newFields != null)
			{
				if (metaFields.newFields != null)
				{
					foreach (var newField in baseFields.newFields)
					{
						if (metaFields.newFields.All(f => f.destFieldId != newField.destFieldId))
						{
							metaFields.newFields.Add(newField);
						}
					}
				}
				else
				{
					metaFields.newFields = baseFields.newFields;
				}
			}
			if (baseFields.convertedFields != null)
			{
				if (metaFields.convertedFields != null)
				{
					foreach (var convertedField in baseFields.convertedFields)
					{
						// Check if metadataTemplate contains it already
						var metaConvertedField = metaFields.convertedFields.FirstOrDefault(cf => cf.sourceFieldId == convertedField.sourceFieldId);
						if (metaConvertedField != null)
						{
							if (metaConvertedField.destFields != null && metaConvertedField.destFields.Any())
							{
								foreach (var convertedFieldDestField in convertedField.destFields)
								{
									if (metaConvertedField.destFields.All(df => !string.Equals(df.sourceElementName, convertedFieldDestField.sourceElementName, StringComparison.InvariantCultureIgnoreCase)))
									{
										metaConvertedField.destFields.Add(convertedFieldDestField);
									}
								}
							}
							else if (metaConvertedField.destFieldId == null)
							{
								metaConvertedField.destFields = convertedField.destFields;
							}
						}
						else
						{
							metaFields.convertedFields.Add(convertedField);
						}
					}
				}
				else
				{
					metaFields.convertedFields = baseFields.convertedFields;
				}
			}
			if (baseFields.existingFields != null)
			{
				if (metaFields.existingFields != null)
				{
					foreach (var newField in baseFields.existingFields)
					{
						if (metaFields.existingFields.All(f => f.fieldId != newField.fieldId))
						{
							metaFields.existingFields.Add(newField);
						}
					}
				}
				else
				{
					metaFields.existingFields = baseFields.existingFields;
				}
			}

			return metaFields;
		}
	}
}
