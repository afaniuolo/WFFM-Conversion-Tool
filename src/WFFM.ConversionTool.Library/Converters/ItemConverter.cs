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
using WFFM.ConversionTool.Library.Readers;

namespace WFFM.ConversionTool.Library.Converters
{
	public class ItemConverter : IItemConverter
	{
		private IFieldFactory _fieldFactory;
		private MetadataTemplate _itemMetadataTemplate;
		private AppSettings _appSettings;
		private IMetadataReader _metadataReader;

		private readonly string _baseFieldConverterType = "WFFM.ConversionTool.Library.Converters.BaseFieldConverter, WFFM.ConversionTool.Library";

		public ItemConverter(IFieldFactory fieldFactory, AppSettings appSettings, IMetadataReader metadataReader)
		{
			_fieldFactory = fieldFactory;
			_appSettings = appSettings;
			_metadataReader = metadataReader;
		}

		public SCItem Convert(SCItem scItem, Guid destParentId)
		{
			_itemMetadataTemplate = _metadataReader.GetItemMetadata(scItem.TemplateID);
			return ConvertItemAndFields(scItem, destParentId);
		}

		private SCItem ConvertItemAndFields(SCItem sourceItem, Guid destParentId)
		{
			return new SCItem()
			{
				ID = sourceItem.ID,
				Name = sourceItem.Name,
				MasterID = Guid.Empty,
				ParentID = destParentId,
				Created = sourceItem.Created,
				Updated = sourceItem.Updated,
				TemplateID = _itemMetadataTemplate.destTemplateId,
				Fields = ConvertFields(sourceItem.Fields)
			};
		}

		private List<SCField> ConvertFields(List<SCField> fields)
		{
			var destFields = new List<SCField>();

			var itemId = fields.First().ItemId;

			IEnumerable<Tuple<string, int>> langVersions = fields.Where(f => f.Version != null && f.Language != null).Select(f => new Tuple<string, int>(f.Language, (int)f.Version)).Distinct();
			var languages = fields.Where(f => f.Language != null).Select(f => f.Language).Distinct();

			if (_itemMetadataTemplate.fields.existingFields != null)
			{
				var filteredExistingFields = fields.Where(f =>
					_itemMetadataTemplate.fields.existingFields.Select(mf => mf.fieldId).Contains(f.FieldId));

				foreach (var filteredExistingField in filteredExistingFields)
				{
					var existingField =
						_itemMetadataTemplate.fields.existingFields.FirstOrDefault(mf => mf.fieldId == filteredExistingField.FieldId);

					if (existingField != null)
					{
						destFields.Add(filteredExistingField);
					}
				}
			}

			if (_itemMetadataTemplate.fields.convertedFields != null)
			{
				var filteredConvertedFields = fields.Where(f =>
					_itemMetadataTemplate.fields.convertedFields.Select(mf => mf.sourceFieldId).Contains(f.FieldId));

				foreach (var filteredConvertedField in filteredConvertedFields)
				{
					var convertedField =
						_itemMetadataTemplate.fields.convertedFields.FirstOrDefault(mf =>
							mf.sourceFieldId == filteredConvertedField.FieldId);

					if (convertedField != null)
					{
						if (!string.IsNullOrEmpty(convertedField.fieldConverter))
						{
							if (convertedField.destFields != null && convertedField.destFields.Any())
							{
								var valueElements = GetXmlElementNames(filteredConvertedField.Value);
								var filteredValueElements =
									convertedField.destFields.Where(f => valueElements.Contains(f.sourceElementName.ToLower()) && f.destFieldId != null);

								foreach (var valueXmlElementMapping in filteredValueElements)
								{
									IFieldConverter converter = InitConverter(valueXmlElementMapping.fieldConverter);

									SCField destField = converter?.ConvertValueElement(filteredConvertedField, (Guid)valueXmlElementMapping.destFieldId, GetXmlElementValue(filteredConvertedField.Value, valueXmlElementMapping.sourceElementName));

									if (destField != null && destField.FieldId != Guid.Empty)
									{
										destFields.Add(destField);
									}
								}
							}
							else
							{
								IFieldConverter converter = InitConverter(convertedField.fieldConverter);
								SCField destField = converter?.ConvertField(filteredConvertedField, (Guid)convertedField.destFieldId);

								if (destField != null && destField.FieldId != Guid.Empty)
								{
									destFields.Add(destField);
								}
							}
						}
					}
				}
			}

			foreach (var newField in _itemMetadataTemplate.fields.newFields)
			{
				destFields.AddRange(_fieldFactory.CreateFields(newField, itemId, langVersions, languages));
			}

			return destFields;
		}

		private IFieldConverter InitConverter(string converterName)
		{
			var converterType = _baseFieldConverterType;
			if (converterName != null)
			{
				var metaConverter = _appSettings.converters.FirstOrDefault(c => c.name == converterName)?.converterType;
				if (!string.IsNullOrEmpty(metaConverter))
				{
					converterType = metaConverter;
				}
			}
			return ConverterInstantiator.CreateInstance(converterType);
		}

		private List<string> GetXmlElementNames(string fieldValue)
		{
			List<string> elementNames = new List<string>();
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(fieldValue);

			foreach (XmlNode childNode in xmlDocument.ChildNodes)
			{
				elementNames.Add(childNode.Name.ToLower());
			}

			return elementNames;
		}

		private string GetXmlElementValue(string fieldValue, string elementName)
		{
			string elementValue = string.Empty;
			if (!string.IsNullOrEmpty(fieldValue) && !string.IsNullOrEmpty(elementName))
			{
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(fieldValue);

				XmlNodeList elementsByTagName = xmlDocument.GetElementsByTagName(elementName);

				if (elementsByTagName.Count > 0)
				{
					var element = elementsByTagName.Item(0);
					return element?.Value;
				}
			}
			return string.Empty;


		}
	}

}
