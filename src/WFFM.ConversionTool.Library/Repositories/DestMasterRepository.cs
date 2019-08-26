using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFFM.ConversionTool.Library.Database.Master;
using WFFM.ConversionTool.Library.Logging;
using WFFM.ConversionTool.Library.Models;
using WFFM.ConversionTool.Library.Models.Metadata;
using WFFM.ConversionTool.Library.Models.Sitecore;

namespace WFFM.ConversionTool.Library.Repositories
{
	public class DestMasterRepository : IDestMasterRepository
	{
		private ILogger logger;
		private DestMasterDb _destMasterDb;
		private AppSettings _appSettings;

		public DestMasterRepository(ILogger iLogger, DestMasterDb destMasterDb, AppSettings appSettings)
		{
			logger = iLogger;
			_destMasterDb = destMasterDb;
			_appSettings = appSettings;
		}

		public void AddOrUpdateSitecoreItem(SCItem destItem)
		{
			AddOrUpdateItem(destItem);

			foreach (SCField scField in destItem.Fields)
			{
				switch (scField.Type)
				{
					case FieldType.Shared:
						AddOrUpdateSharedField(scField);
						break;
					case FieldType.Unversioned:
						AddOrUpdateUnversionedField(scField);
						break;
					case FieldType.Versioned:
						AddOrUpdateVersionedField(scField);
						break;
				}
			}
		}

		public void DeleteSitecoreItem(SCItem scItem)
		{
			DeleteItem(scItem);

			foreach (SCField scField in scItem.Fields)
			{
				switch (scField.Type)
				{
					case FieldType.Shared:
						DeleteSharedField(scField);
						break;
					case FieldType.Unversioned:
						DeleteUnversionedField(scField);
						break;
					case FieldType.Versioned:
						DeleteVersionedField(scField);
						break;
				}
			}
		}

		public bool ItemHasChildrenOfTemplate(Guid templateId, SCItem scItem)
		{
			if (scItem == null) return false;
			return _destMasterDb.Items.Any(item => item.TemplateID == templateId && item.ParentID == scItem.ID);
		}

		public List<SCItem> GetSitecoreChildrenItems(Guid templateId, Guid parentId)
		{
			List<SCItem> scItems = new List<SCItem>();
			var childrenItems = GetChildrenItems(templateId, parentId);
			
			foreach (var item in childrenItems)
			{
				scItems.Add(GetSourceItemAndFields(item));
			}

			return scItems;
		}

		public List<SCItem> GetSitecoreDescendantsItems(Guid templateId, Guid parentId)
		{
			var descendantsItems = GetSitecoreDescendantsItems(parentId);			
			return descendantsItems.Where(item => item.TemplateID == templateId).ToList();
		}

		public List<SCItem> GetSitecoreDescendantsItems(Guid parentId)
		{
			var scItems = new List<SCItem>();
			var childrenItems = GetChildrenItems(parentId);
			if (childrenItems.Any())
			{
				foreach (var item in childrenItems)
				{
					scItems.Add(GetSourceItemAndFields(item));
				}

				foreach (var childrenItem in childrenItems)
				{
					scItems.AddRange(GetSitecoreDescendantsItems(childrenItem.ID));
				}
			}

			return scItems;
		}

		public SCItem GetSitecoreItem(Guid itemId)
		{
			var item = _destMasterDb.Items.FirstOrDefault(i => i.ID == itemId);
			if (item == null) return null;
			return GetSourceItemAndFields(item);
		}

		public bool ItemExists(Guid itemId)
		{
			return _destMasterDb.Items.Any(i => i.ID == itemId);
		}

		private void AddOrUpdateItem(SCItem scItem)
		{
			var dbItem = new Item()
			{
				ID = scItem.ID,
				Name = scItem.Name,
				MasterID = scItem.MasterID,
				ParentID = scItem.ParentID,
				Created = scItem.Created,
				Updated = scItem.Updated,
				TemplateID = scItem.TemplateID
			};
			_destMasterDb.Items.AddOrUpdate(dbItem);
			if (!_appSettings.enableOnlyAnalysisByDefault) _destMasterDb.SaveChanges();
		}

		private void AddOrUpdateSharedField(SCField scField)
		{
			var fieldCheck = _destMasterDb.SharedFields.FirstOrDefault(field =>
				field.FieldId == scField.FieldId && field.ItemId == scField.ItemId);
			_destMasterDb.SharedFields.AddOrUpdate(new SharedField()
			{
				Created = scField.Created,
				Updated = scField.Updated,
				FieldId = scField.FieldId,
				ItemId = scField.ItemId,
				Value = scField.Value,
				Id = fieldCheck?.Id ?? scField.Id
			});
			if (!_appSettings.enableOnlyAnalysisByDefault) _destMasterDb.SaveChanges();
		}

		private void AddOrUpdateUnversionedField(SCField scField)
		{
			var fieldCheck = _destMasterDb.UnversionedFields.FirstOrDefault(field =>
				field.FieldId == scField.FieldId && field.ItemId == scField.ItemId && field.Language == scField.Language);
			_destMasterDb.UnversionedFields.AddOrUpdate(new UnversionedField()
			{
				Created = scField.Created,
				Updated = scField.Updated,
				FieldId = scField.FieldId,
				ItemId = scField.ItemId,
				Value = scField.Value,
				Id = fieldCheck?.Id ?? scField.Id,
				Language = scField.Language
			});
			if (!_appSettings.enableOnlyAnalysisByDefault) _destMasterDb.SaveChanges();
		}

		private void AddOrUpdateVersionedField(SCField scField)
		{
			var fieldCheck = _destMasterDb.VersionedFields.FirstOrDefault(field =>
				field.FieldId == scField.FieldId && field.ItemId == scField.ItemId && field.Language == scField.Language && field.Version == scField.Version);
			_destMasterDb.VersionedFields.AddOrUpdate(new VersionedField()
			{
				Created = scField.Created,
				Updated = scField.Updated,
				FieldId = scField.FieldId,
				ItemId = scField.ItemId,
				Value = scField.Value,
				Id = fieldCheck?.Id ?? scField.Id,
				Language = scField.Language,
				Version = scField.Version ?? 1
			});
			if (!_appSettings.enableOnlyAnalysisByDefault) _destMasterDb.SaveChanges();
		}

		private void DeleteItem(SCItem scItem)
		{
			var dbItem = _destMasterDb.Items.FirstOrDefault(i => i.ID == scItem.ID);
			if (dbItem != null)
			{
				_destMasterDb.Items.Remove(dbItem);
				if (!_appSettings.enableOnlyAnalysisByDefault) _destMasterDb.SaveChanges();
			}
		}

		private void DeleteSharedField(SCField scField)
		{
			var dbField = _destMasterDb.SharedFields.FirstOrDefault(i => i.Id == scField.Id);
			if (dbField != null)
			{
				_destMasterDb.SharedFields.Remove(dbField);
				if (!_appSettings.enableOnlyAnalysisByDefault) _destMasterDb.SaveChanges();
			}
		}

		private void DeleteUnversionedField(SCField scField)
		{
			var dbField = _destMasterDb.UnversionedFields.FirstOrDefault(i => i.Id == scField.Id);
			if (dbField != null)
			{
				_destMasterDb.UnversionedFields.Remove(dbField);
				if (!_appSettings.enableOnlyAnalysisByDefault) _destMasterDb.SaveChanges();
			}
		}

		private void DeleteVersionedField(SCField scField)
		{
			var dbField = _destMasterDb.VersionedFields.FirstOrDefault(i => i.Id == scField.Id);
			if (dbField != null)
			{
				_destMasterDb.VersionedFields.Remove(dbField);
				if (!_appSettings.enableOnlyAnalysisByDefault) _destMasterDb.SaveChanges();
			}
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
		/// Get the list of existing items by templateId in master database
		/// </summary>
		/// <returns></returns>
		private List<Item> GetItems(Guid templateId)
		{
			return _destMasterDb.Items.Where(item => item.TemplateID == templateId && item.Name != "__Standard Values").ToList();
		}

		/// <summary>
		/// Get the list of existing children items of a specific template of a parent item in master database
		/// </summary>
		/// <param name="templateId"></param>
		/// <param name="parentId"></param>
		/// <returns></returns>
		private List<Item> GetChildrenItems(Guid templateId, Guid parentId)
		{
			return _destMasterDb.Items.Where(item => item.TemplateID == templateId && item.Name != "__Standard Values" && item.ParentID == parentId).ToList();
		}

		/// <summary>
		/// Get the list of all existing children items of a parent item in the master database
		/// </summary>
		/// <param name="templateId"></param>
		/// <param name="parentId"></param>
		/// <returns></returns>
		private List<Item> GetChildrenItems(Guid parentId)
		{
			return _destMasterDb.Items.Where(item => item.Name != "__Standard Values" && item.ParentID == parentId).ToList();
		}

		/// <summary>
		/// Get list of fields of a Sitecore item from the master database
		/// </summary>
		private List<SCField> GetItemFields(Guid itemId)
		{
			// fields from item template
			var fields = _destMasterDb.SharedFields.Where(field => field.ItemId == itemId)
				.Select(field => new SCField() { Id = field.Id, Value = field.Value, Created = field.Created, Updated = field.Updated, ItemId = field.ItemId, Type = FieldType.Shared, Language = null, Version = null, FieldId = field.FieldId })
				.Union(_destMasterDb.UnversionedFields.Where(field => field.ItemId == itemId)
					.Select(field => new SCField() { Id = field.Id, Value = field.Value, Created = field.Created, Updated = field.Updated, ItemId = field.ItemId, Type = FieldType.Unversioned, Language = field.Language, Version = null, FieldId = field.FieldId }))
				.Union(_destMasterDb.VersionedFields.Where(field => field.ItemId == itemId)
					.Select(field => new SCField() { Id = field.Id, Value = field.Value, Created = field.Created, Updated = field.Updated, ItemId = field.ItemId, Type = FieldType.Versioned, Language = field.Language, Version = field.Version, FieldId = field.FieldId }));

			return fields.ToList();
		}
	}
}
