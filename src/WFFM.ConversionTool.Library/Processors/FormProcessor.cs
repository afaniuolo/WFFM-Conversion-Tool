using System;
using System.Collections.Generic;
using System.Data.Entity;
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

		}

		private List<SCField> ConvertFields(List<SCField> fields)
		{
			var destFields = new List<SCField>();
			foreach (SCField field in fields)
			{
				SCField destField = new SCField();
				switch (field.Id.ToString())
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
							Value = field.Value != string.Empty ? "1" : "0"
						};
						break;
					case "":

						break;
					default:
						destField = field;
						break;
				}
				destFields.Add(destField);
			}

			return destFields;
		}

		/// <summary>
		/// Get the list of existing WFFM forms in source master database
		/// </summary>
		/// <returns></returns>
		private List<Item> GetWFFMForms()
		{
			return _sourceMasterDb.Items.Where(item => item.TemplateID == WFFMFormTemplateId).ToList();
		}

		/// <summary>
		/// Get list of fields of a WFFM form from the source master database
		/// </summary>
		private List<SCField> GetWFFMFormFields(Guid formItemId)
		{
			// fields from form template
			var formFields = _sourceMasterDb.SharedFields.Where(field => field.ItemId == formItemId)
				.Select(field => new SCField() { Id = field.FieldId, Value = field.Value, Created = field.Created, Updated = field.Updated, ItemId = field.ItemId, Type = FieldType.Shared })
				.Union(_sourceMasterDb.UnversionedFields.Where(field => field.ItemId == formItemId)
					.Select(field => new SCField() { Id = field.FieldId, Value = field.Value, Created = field.Created, Updated = field.Updated, ItemId = field.ItemId, Type = FieldType.Unversioned, Language = field.Language}))
				.Union(_sourceMasterDb.VersionedFields.Where(field => field.ItemId == formItemId)
					.Select(field => new SCField() { Id = field.FieldId, Value = field.Value, Created = field.Created, Updated = field.Updated, ItemId = field.ItemId, Type = FieldType.Versioned, Language = field.Language, Version = field.Version}));

			var formStandardValuesItem =
				_sourceMasterDb.Items.FirstOrDefault(
					item => item.TemplateID == WFFMFormTemplateId && item.Name == "__Standard values");

			// fields inherited and set in the standard values item
			var formStandardValuesFields = _sourceMasterDb.SharedFields.Where(field => field.ItemId == formStandardValuesItem.ID)
				.Select(field => new SCField() { Id = field.FieldId, Value = field.Value, Created = field.Created, Updated = field.Updated, ItemId = field.ItemId, Type = FieldType.Shared })
				.Union(_sourceMasterDb.UnversionedFields.Where(field => field.ItemId == formStandardValuesItem.ID)
					.Select(field => new SCField() { Id = field.FieldId, Value = field.Value, Created = field.Created, Updated = field.Updated, ItemId = field.ItemId, Type = FieldType.Unversioned, Language = field.Language}))
				.Union(_sourceMasterDb.VersionedFields.Where(field => field.ItemId == formStandardValuesItem.ID)
					.Select(field => new SCField() { Id = field.FieldId, Value = field.Value, Created = field.Created, Updated = field.Updated, ItemId = field.ItemId, Type = FieldType.Versioned, Language = field.Language, Version = field.Version})).Where(f => !formFields.Select(field => field.Id).Contains(f.Id));

			return formFields.Union(formStandardValuesFields).ToList();
		}
	}
}
