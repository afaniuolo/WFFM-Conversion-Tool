using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFFM.ConversionTool.Library.Models.Metadata;
using WFFM.ConversionTool.Library.Models.Sitecore;
using WFFM.ConversionTool.Library.Providers;

namespace WFFM.ConversionTool.Library.Factories
{
	public class ItemFactory : IItemFactory
	{
		private MetadataTemplate _itemMetadataTemplate;
		private IMetadataProvider _metadataProvider;
		private IFieldFactory _fieldFactory;

		public ItemFactory(IMetadataProvider metadataProvider, IFieldFactory fieldFactory)
		{
			_metadataProvider = metadataProvider;
			_fieldFactory = fieldFactory;
		}

		public SCItem Create(Guid destTemplateId, SCItem parentItem, string itemName)
		{
			_itemMetadataTemplate = _metadataProvider.GetItemMetadataByTemplateId(destTemplateId);
			return CreateItem(parentItem, itemName);
		}

		private SCItem CreateItem(SCItem parentItem, string itemName)
		{
			var itemId = Guid.NewGuid();

			return new SCItem()
			{
				ID = itemId,
				Name = itemName,
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
				destFields.AddRange(_fieldFactory.CreateFields(newField, itemId, langVersions, languages));
			}

			return destFields;
		}
	}
}
