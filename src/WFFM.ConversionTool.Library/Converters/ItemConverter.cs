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

			IEnumerable<Tuple<string,int>> langVersions = fields.Where(f => f.Version != null && f.Language != null).Select(f => new Tuple<string,int>(f.Language, (int)f.Version)).Distinct();
			var languages = fields.Where(f => f.Language != null).Select(f => f.Language).Distinct();

			var filteredSourceFields = fields.Where(f =>
				_itemMetadataTemplate.fields.existingFields.Select(mf => mf.fieldId).Contains(f.FieldId));

			foreach (var filteredSourceField in filteredSourceFields)
			{
				SCField destField = null;
				IFieldConverter converter;
				var existingField =
					_itemMetadataTemplate.fields.existingFields.FirstOrDefault(mf => mf.fieldId == filteredSourceField.FieldId);

				if (existingField == null)
					continue;

				if(!string.IsNullOrEmpty(existingField.fieldConverter))
				{
					converter = ConverterInstantiator.CreateInstance(_appSettings.converters.FirstOrDefault(c => c.name == existingField.fieldConverter)?.converterType);
					destField = converter?.Convert(filteredSourceField);
				}
				else
				{
					destField = filteredSourceField;
				}

				if (destField != null && destField.FieldId != Guid.Empty)
				{
					destFields.Add(destField);
				}
			}

			foreach (var newField in _itemMetadataTemplate.fields.newFields)
			{
				destFields.AddRange(_fieldFactory.CreateFields(newField, itemId, langVersions, languages));
			}
			
			return destFields;
		}

		
	}
}
