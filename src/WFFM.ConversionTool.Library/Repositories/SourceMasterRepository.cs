using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFFM.ConversionTool.Library.Database.Master;
using WFFM.ConversionTool.Library.Models;
using WFFM.ConversionTool.Library.Models.Sitecore;

namespace WFFM.ConversionTool.Library.Repositories
{
	public class SourceMasterRepository : ISourceMasterRepository
	{
		private SourceMasterDb _sourceMasterDb;

		public SourceMasterRepository(SourceMasterDb sourceMasterDb)
		{
			_sourceMasterDb = sourceMasterDb;
		}

		public List<SCItem> GetSitecoreItems(Guid templateId)
		{
			var items = GetItems(templateId);
			List<SCItem> scItems = new List<SCItem>();
			foreach (var item in items)
			{
				scItems.Add(GetSourceItemAndFields(item));
			}

			return scItems;
		}

		private SCItem GetSourceItemAndFields(Item sourceItem)
		{
			return new SCItem()
			{
				ID = sourceItem.ID,
				Name = sourceItem.Name,
				MasterID = sourceItem.MasterID,
				ParentID = sourceItem.ParentID,
				TemplateID = sourceItem.TemplateID,
				Created = sourceItem.Created,
				Updated = sourceItem.Updated,
				Fields = GetItemFields(sourceItem.ID),
			};
		}

		/// <summary>
		/// Get the list of existing items by templateId in source master database
		/// </summary>
		/// <returns></returns>
		private List<Item> GetItems(Guid templateId)
		{
			return _sourceMasterDb.Items.Where(item => item.TemplateID == templateId && item.Name != "__Standard Values").ToList();
		}

		/// <summary>
		/// Get list of fields of a Sitecore item from the source master database
		/// </summary>
		private List<SCField> GetItemFields(Guid itemId)
		{
			// fields from item template
			var fields = _sourceMasterDb.SharedFields.Where(field => field.ItemId == itemId)
				.Select(field => new SCField() { Id = field.Id, Value = field.Value, Created = field.Created, Updated = field.Updated, ItemId = field.ItemId, Type = FieldType.Shared, Language = null, Version = null, FieldId = field.FieldId })
				.Union(_sourceMasterDb.UnversionedFields.Where(field => field.ItemId == itemId)
					.Select(field => new SCField() { Id = field.Id, Value = field.Value, Created = field.Created, Updated = field.Updated, ItemId = field.ItemId, Type = FieldType.Unversioned, Language = field.Language, Version = null, FieldId = field.FieldId }))
				.Union(_sourceMasterDb.VersionedFields.Where(field => field.ItemId == itemId)
					.Select(field => new SCField() { Id = field.Id, Value = field.Value, Created = field.Created, Updated = field.Updated, ItemId = field.ItemId, Type = FieldType.Versioned, Language = field.Language, Version = field.Version, FieldId = field.FieldId }));

			return fields.ToList();
		}
	}
}
