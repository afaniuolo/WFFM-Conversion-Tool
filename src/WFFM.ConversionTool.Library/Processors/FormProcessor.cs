using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFFM.ConversionTool.Library.Database.Forms;
using WFFM.ConversionTool.Library.Database.Master;
using WFFM.ConversionTool.Library.Database.WFFM;
using WFFM.ConversionTool.Library.Logging;
using WFFM.ConversionTool.Library.Models;

namespace WFFM.ConversionTool.Library.Processors
{
	public class FormProcessor : IFormProcessor
	{
		private ILogger logger;
		private Database.WFFM.WFFM _wffmContext;
		private SitecoreForms _sitecoreFormsContext;
		private SourceMasterDb _sourceMasterDb;
		private DestMasterDb _destMasterDb;

		private readonly Guid WFFMFormTemplateId = new Guid("FFB1DA32-2764-47DB-83B0-95B843546A7E");
		private readonly Guid DestFormsFolderID = new Guid("B701850A-CB8A-4943-B2BC-DDDB1238C103");
		private readonly Guid SitecoreFormsTemplateId = new Guid("6ABEE1F2-4AB4-47F0-AD8B-BDB36F37F64C");

		public FormProcessor(ILogger iLogger, Database.WFFM.WFFM wffmContext, SitecoreForms sitecoreFormsContext, SourceMasterDb sourceMasterDb, DestMasterDb destMasterDb)
		{
			logger = iLogger;
			_wffmContext = wffmContext;
			_sitecoreFormsContext = sitecoreFormsContext;
			_sourceMasterDb = sourceMasterDb;
			_destMasterDb = destMasterDb;
		}

		public void ConvertForms()
		{
			var forms = GetWFFMForms();
			List<FormEntry> sitecoreFormsEntries = new List<FormEntry>();
			foreach (var form in forms)
			{
				ConvertForm(form);
			}

		}

		private void ConvertForm(Item formItem)
		{
			logger.Log(new LogEntry(LoggingEventType.Debug, string.Format("FormID={0}", formItem.ID)));

			// Get form children items
			var childrenItems = _sourceMasterDb.Items.Where(item => item.ParentID == formItem.ID);
			logger.Log(new LogEntry(LoggingEventType.Debug, string.Format("Number of child items={0}", childrenItems.Count())));


			var sourceFormItem = new SCItem()
			{
				ID = formItem.ID,
				Name = formItem.Name,
				MasterID = formItem.MasterID,
				ParentID = formItem.ParentID,
				TemplateID = formItem.TemplateID,
				Created = formItem.Created,
				Updated = formItem.Updated,
				Fields = GetWFFMFormFields(formItem.ID),
			};

			// Map
			var destFormItem = new SCItem()
			{
				ID = sourceFormItem.ID,
				Name = sourceFormItem.Name,
				MasterID = Guid.Empty,
				ParentID = DestFormsFolderID,
				Created = sourceFormItem.Created,
				Updated = sourceFormItem.Updated,
				TemplateID = SitecoreFormsTemplateId,
				Fields = ConvertFields(sourceFormItem.Fields)
			};

			// Create item
			var dbFormItem = new Item()
			{
				ID = destFormItem.ID,
				Name = destFormItem.Name,
				MasterID = destFormItem.MasterID,
				ParentID = destFormItem.ParentID,
				Created = destFormItem.Created,
				Updated = destFormItem.Updated,
				TemplateID = destFormItem.TemplateID
			};
			_destMasterDb.Items.AddOrUpdate(dbFormItem);

			_destMasterDb.SaveChanges();

			// Create fields
			foreach (SCField scField in destFormItem.Fields)
			{
				if (scField.Type == FieldType.Shared)
				{
					_destMasterDb.SharedFields.AddOrUpdate(new SharedField()
					{
						Created = scField.Created,
						Updated = scField.Updated,
						FieldId = scField.FieldId,
						ItemId = scField.ItemId,
						Value = scField.Value,
						Id = scField.Id
					});
				}
				else if (scField.Type == FieldType.Unversioned)
				{

					_destMasterDb.UnversionedFields.AddOrUpdate(new UnversionedField()
					{
						Created = scField.Created,
						Updated = scField.Updated,
						FieldId = scField.FieldId,
						ItemId = scField.ItemId,
						Value = scField.Value,
						Id = scField.Id,
						Language = scField.Language
					});
				}
				else if (scField.Type == FieldType.Versioned)
				{
					_destMasterDb.VersionedFields.AddOrUpdate(new VersionedField()
					{
						Created = scField.Created,
						Updated = scField.Updated,
						FieldId = scField.FieldId,
						ItemId = scField.ItemId,
						Value = scField.Value,
						Id = scField.Id,
						Language = scField.Language,
						Version = scField.Version ?? 1
					});
				}
			}

			_destMasterDb.SaveChanges();

		}

		private List<SCField> ConvertFields(List<SCField> fields)
		{
			var destFields = new List<SCField>();

			var itemId = fields.First().ItemId;

			// Map source fields to dest fields
			foreach (SCField field in fields)
			{
				SCField destField = new SCField();
				switch (field.FieldId.ToString().ToUpper())
				{
					case "B0A67B2A-8B07-4E0B-8809-69F751709806":
						destField = new SCField()
						{
							Created = DateTime.Now,
							Updated = DateTime.Now,
							ItemId = field.ItemId,
							Language = field.Language,
							Version = field.Version,
							Type = field.Type,
							Value = field.Value != string.Empty ? "1" : "0",
							FieldId = field.FieldId,
							Id = Guid.NewGuid()
						};
						break;
					case "5DD74568-4D4B-44C1-B513-0AF5F4CDA34F":
					case "BADD9CF9-53E0-4D0C-BCC0-2D784C282F6A":
					case "8CDC337E-A112-42FB-BBB4-4143751E123F":
					case "BA3F86A2-4A1C-4D78-B63D-91C2779C1B5E":
					case "25BED78C-4957-4165-998A-CA1B52F67497":
					case "52807595-0F8F-4B20-8D2A-CB71D28C6103":
					case "D9CF14B1-FA16-4BA6-9288-E8A174D4D522":
					case "B5E02AD9-D56F-4C41-A065-A133DB87BDEB":
						destField = field;
						break;
				}

				if (destField.FieldId != Guid.Empty)
				{
					destFields.Add(destField);
				}
			}

			// Add new fields
			destFields.Add(CreateSharedField(new Guid("589A7ADE-81D4-4A60-B9E2-7EAF6AE8A563"), itemId, "{3A4DF9C0-7C82-4415-90C3-25440257756D}")); // Field Type
			destFields.Add(CreateSharedField(new Guid("9EB86F19-2E5D-48DB-9795-0EE4868EFF11"), itemId, "1")); // Is Ajax
			destFields.Add(CreateSharedField(new Guid("FCE03B41-4CBB-4F7F-920F-FF04F12FA896"), itemId, @"jquery-2.1.3.min.js|jquery.validate.min.js|jquery.validate.unobtrusive.min.js|jquery.unobtrusive-ajax.min.js|form.validate.js|form.tracking.js|form.conditions.js")); // Scripts

			return destFields;
		}

		private SCField CreateSharedField(Guid fieldId, Guid itemID, string value)
		{
			return new SCField()
			{
				Id = Guid.NewGuid(),
				FieldId = fieldId,
				Created = DateTime.Now,
				ItemId = itemID,
				Updated = DateTime.Now,
				Type = FieldType.Shared,
				Value = value,
				Version = null,
				Language = null
			};
		}

		/// <summary>
		/// Get the list of existing WFFM forms in source master database
		/// </summary>
		/// <returns></returns>
		private List<Item> GetWFFMForms()
		{
			return _sourceMasterDb.Items.Where(item => item.TemplateID == WFFMFormTemplateId && item.Name != "__Standard Values").ToList();
		}

		/// <summary>
		/// Get list of fields of a WFFM form from the source master database
		/// </summary>
		private List<SCField> GetWFFMFormFields(Guid formItemId)
		{
			// fields from form template
			var formFields = _sourceMasterDb.SharedFields.Where(field => field.ItemId == formItemId)
				.Select(field => new SCField() { Id = field.Id, Value = field.Value, Created = field.Created, Updated = field.Updated, ItemId = field.ItemId, Type = FieldType.Shared, Language = null, Version = null, FieldId = field.FieldId })
				.Union(_sourceMasterDb.UnversionedFields.Where(field => field.ItemId == formItemId)
					.Select(field => new SCField() { Id = field.Id, Value = field.Value, Created = field.Created, Updated = field.Updated, ItemId = field.ItemId, Type = FieldType.Unversioned, Language = field.Language, Version = null, FieldId = field.FieldId }))
				.Union(_sourceMasterDb.VersionedFields.Where(field => field.ItemId == formItemId)
					.Select(field => new SCField() { Id = field.Id, Value = field.Value, Created = field.Created, Updated = field.Updated, ItemId = field.ItemId, Type = FieldType.Versioned, Language = field.Language, Version = field.Version, FieldId = field.FieldId }));

			return formFields.ToList();

			var formStandardValuesItem =
				_sourceMasterDb.Items.FirstOrDefault(
					item => item.TemplateID == WFFMFormTemplateId && item.Name == "__Standard values");

			// fields inherited and set in the standard values item
			var formStandardValuesFields = _sourceMasterDb.SharedFields.Where(field => field.ItemId == formStandardValuesItem.ID)
				.Select(field => new SCField() { Id = field.FieldId, Value = field.Value, Created = field.Created, Updated = field.Updated, ItemId = field.ItemId, Type = FieldType.Shared })
				.Union(_sourceMasterDb.UnversionedFields.Where(field => field.ItemId == formStandardValuesItem.ID)
					.Select(field => new SCField() { Id = field.FieldId, Value = field.Value, Created = field.Created, Updated = field.Updated, ItemId = field.ItemId, Type = FieldType.Unversioned, Language = field.Language }))
				.Union(_sourceMasterDb.VersionedFields.Where(field => field.ItemId == formStandardValuesItem.ID)
					.Select(field => new SCField() { Id = field.FieldId, Value = field.Value, Created = field.Created, Updated = field.Updated, ItemId = field.ItemId, Type = FieldType.Versioned, Language = field.Language, Version = field.Version })).Where(f => !formFields.Select(field => field.Id).Contains(f.Id));

			return formFields.Union(formStandardValuesFields).ToList();
		}
	}
}
