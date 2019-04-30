using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using WFFM.ConversionTool.Library.Models.Metadata;
using WFFM.ConversionTool.Library.Models.Sitecore;
using WFFM.ConversionTool.Library.Providers;
using WFFM.ConversionTool.Library.Repositories;

namespace WFFM.ConversionTool.Library.Factories
{
	public class ItemFactory : IItemFactory
	{
		private MetadataTemplate _itemMetadataTemplate;
		private IMetadataProvider _metadataProvider;
		private IFieldFactory _fieldFactory;
		private AppSettings _appSettings;
		private IDestMasterRepository _destMasterRepository;

		public ItemFactory(IMetadataProvider metadataProvider, IFieldFactory fieldFactory, AppSettings appSettings, IDestMasterRepository destMasterRepository)
		{
			_metadataProvider = metadataProvider;
			_fieldFactory = fieldFactory;
			_appSettings = appSettings;
			_destMasterRepository = destMasterRepository;
		}

		public SCItem Create(Guid destTemplateId, SCItem parentItem, string itemName, MetadataTemplate metadataTemplate = null)
		{
			if (metadataTemplate != null)
			{
				_itemMetadataTemplate = metadataTemplate;
			}
			else
			{
				_itemMetadataTemplate = _metadataProvider.GetItemMetadataByTemplateId(destTemplateId);
			}

			itemName = RemoveInvalidChars(itemName);

			return CreateItem(parentItem, itemName);
		}

		public List<SCItem> CreateDescendantItems(MetadataTemplate metadataTemplate, SCItem parentItem)
		{
			var destItems = new List<SCItem>();
			if (metadataTemplate.descendantItems != null)
			{
				foreach (var descendantItem in metadataTemplate.descendantItems)
				{
					if (descendantItem.isParentChild)
					{
						var destDescItem = CreateDescendantItem(descendantItem, parentItem);
						if (destDescItem != null) destItems.Add(destDescItem);
					}
					else
					{
						var destParentItem = destItems.FirstOrDefault(d =>
							string.Equals(d.Name, descendantItem.parentItemName, StringComparison.InvariantCultureIgnoreCase));
						if (destParentItem != null)
						{
							var destDescItem = CreateDescendantItem(descendantItem, destParentItem);
							if (destDescItem != null) destItems.Add(destDescItem);
						}
					}
				}
			}

			return destItems;
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

		private string RemoveInvalidChars(string itemName)
		{
			string invalidItemNameChars = _appSettings.invalidItemNameChars;
			if (string.IsNullOrEmpty(invalidItemNameChars)) return itemName;

			var invalidItemNameCharsDecoded = HttpUtility.HtmlDecode(invalidItemNameChars);
			var invalidItemNameCharsEscaped = invalidItemNameCharsDecoded.Replace("[", @"\[").Replace("]", @"\]");
			var replaceRegex = string.Format("[" + invalidItemNameCharsEscaped + "]");

			return Regex.Replace(itemName, replaceRegex, "");
		}

		private SCItem CreateDescendantItem(MetadataTemplate.DescendantItem descendantItem, SCItem destParentItem)
		{
			var descendantItemMetadataTemplate =
				_metadataProvider.GetItemMetadataByTemplateName(descendantItem.destTemplateName);
			var children = _destMasterRepository.GetSitecoreChildrenItems(descendantItemMetadataTemplate.destTemplateId,
				destParentItem.ID);
			if (children != null && children.Any(i =>
				    string.Equals(i.Name, descendantItem.itemName, StringComparison.InvariantCultureIgnoreCase)))
			{
				return children.FirstOrDefault(i =>
					string.Equals(i.Name, descendantItem.itemName, StringComparison.InvariantCultureIgnoreCase));
			}
			return Create(descendantItemMetadataTemplate.destTemplateId, destParentItem, descendantItem.itemName);
		}
	}
}
