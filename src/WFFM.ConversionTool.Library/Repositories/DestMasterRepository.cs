using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFFM.ConversionTool.Library.Database.Master;
using WFFM.ConversionTool.Library.Logging;
using WFFM.ConversionTool.Library.Models;

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

		public void AddOrUpdateForm(SCItem scItem)
		{
			var dbFormItem = new Item()
			{
				ID = scItem.ID,
				Name = scItem.Name,
				MasterID = scItem.MasterID,
				ParentID = scItem.ParentID,
				Created = scItem.Created,
				Updated = scItem.Updated,
				TemplateID = scItem.TemplateID
			};
			_destMasterDb.Items.AddOrUpdate(dbFormItem);
			_destMasterDb.SaveChanges();
		}

		public void AddOrUpdateSharedField(SCField scField)
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

		public void AddOrUpdateUnversionedField(SCField scField)
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

		public void AddOrUpdateVersionedField(SCField scField)
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
