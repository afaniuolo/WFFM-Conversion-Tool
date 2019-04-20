using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.UI;
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
		private AppSettings _appSettings;

		public ItemFactory(IMetadataProvider metadataProvider, IFieldFactory fieldFactory, AppSettings appSettings)
		{
			_metadataProvider = metadataProvider;
			_fieldFactory = fieldFactory;
			_appSettings = appSettings;
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

			var invalidItemNameCharsDecoded = Uri.UnescapeDataString(invalidItemNameChars);
			var invalidItemNameCharsEscaped = invalidItemNameCharsDecoded.Replace("[", @"\[").Replace("]", @"\]");
			var replaceRegex = string.Format("[" + invalidItemNameCharsEscaped + "]");

			return Regex.Replace(itemName, replaceRegex, "");
		}
	}
}
