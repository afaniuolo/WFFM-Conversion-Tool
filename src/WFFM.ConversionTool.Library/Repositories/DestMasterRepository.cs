using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFFM.ConversionTool.Library.Database.Master;
using WFFM.ConversionTool.Library.Logging;
using WFFM.ConversionTool.Library.Models;
using WFFM.ConversionTool.Library.Models.Sitecore;

namespace WFFM.ConversionTool.Library.Repositories
{
	public class DestMasterRepository : IDestMasterRepository
	{
		private ILogger logger;
		private DestMasterDb _destMasterDb;

		public DestMasterRepository(ILogger iLogger, DestMasterDb destMasterDb)
		{
			logger = iLogger;
			_destMasterDb = destMasterDb;
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
			_destMasterDb.SaveChanges();
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
			_destMasterDb.SaveChanges();
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
			_destMasterDb.SaveChanges();
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
			_destMasterDb.SaveChanges();
		}
	}
}
