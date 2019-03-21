using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFFM.ConversionTool.Library.Models.Metadata;
using WFFM.ConversionTool.Library.Models.Sitecore;
using WFFM.ConversionTool.Library.Readers;

namespace WFFM.ConversionTool.Library.Factories
{
	public class ItemFactory : IItemFactory
	{
		private MetadataTemplate _itemMetadataTemplate;
		private IMetadataReader _metadataReader;
		private IFieldFactory _fieldFactory;

		public ItemFactory(IMetadataReader metadataReader, IFieldFactory fieldFactory)
		{
			_metadataReader = metadataReader;
			_fieldFactory = fieldFactory;
		}

		public SCItem Create(Guid destTemplateId, SCItem parentItem)
		{
			_itemMetadataTemplate = _metadataReader.GetItemMetadata(destTemplateId);
			return CreateItem(destTemplateId, parentItem);
		}

		private SCItem CreateItem(Guid destTemplateId, SCItem parentItem)
		{
			var itemId = Guid.NewGuid();

			return new SCItem()
			{
				ID = itemId,
				Name = "Page",
				MasterID = Guid.Empty,
				ParentID = parentItem.ID,
				Created = DateTime.Now,
				Updated = DateTime.Now,
				TemplateID = _itemMetadataTemplate.destTemplateId,
				Fields = CreateFields(itemId, parentItem)
			};
		}

		private List<SCField> CreateFields(Guid itemId, SCItem parentItem)
		{
			var destFields = new List<SCField>();

			IEnumerable<Tuple<string, int>> langVersions = parentItem.Fields.Where(f => f.Version != null && f.Language != null).Select(f => new Tuple<string, int>(f.Language, (int)f.Version)).Distinct();
			var languages = parentItem.Fields.Where(f => f.Language != null).Select(f => f.Language).Distinct();

			foreach (var newField in _itemMetadataTemplate.fields.newFields)
			{
				SCField destField = null;

				var fieldValue = GetValue(newField.value, newField.valueType);

				switch (newField.fieldType)
				{
					case FieldType.Shared:
						destField = _fieldFactory.CreateSharedField(newField.fieldId, itemId, fieldValue);
						if (destField != null)
						{
							destFields.Add(destField);
						}
						break;
					case FieldType.Versioned:
						foreach (var langVersion in langVersions)
						{
							destField = _fieldFactory.CreateVersionedField(newField.fieldId, itemId, fieldValue, langVersion.Item2, langVersion.Item1);
							if (destField != null)
							{
								destFields.Add(destField);
							}
						}
						break;
					case FieldType.Unversioned:
						foreach (var language in languages)
						{
							destField = _fieldFactory.CreateUnversionedField(newField.fieldId, itemId, fieldValue, language);
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
					value = DateTime.Now.ToString();
					break;
				case "system.guid":
					value = Guid.NewGuid().ToString();
					break;
				case "system.string":
					value = Guid.NewGuid().ToString("N");
					break;
				default:
					value = string.Empty;
					break;
			}

			return value;
		}
	}
}
