using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WFFM.ConversionTool.Library.Converters;
using WFFM.ConversionTool.Library.Database.Forms;
using WFFM.ConversionTool.Library.Database.Master;
using WFFM.ConversionTool.Library.Database.WFFM;
using WFFM.ConversionTool.Library.Factories;
using WFFM.ConversionTool.Library.Logging;
using WFFM.ConversionTool.Library.Models;
using WFFM.ConversionTool.Library.Models.Metadata;
using WFFM.ConversionTool.Library.Models.Sitecore;
using WFFM.ConversionTool.Library.Providers;
using WFFM.ConversionTool.Library.Repositories;

namespace WFFM.ConversionTool.Library.Processors
{
	public class FormProcessor : ItemProcessor, IFormProcessor
	{
		private ILogger logger;
		private ISourceMasterRepository _sourceMasterRepository;
		private IDestMasterRepository _destMasterRepository;
		private AppSettings _appSettings;
		private IMetadataProvider _metadataProvider;

		private readonly string FormTemplateName = "form";
		private readonly string PageTemplateName = "page";
		private readonly string SectionTemplateName = "section";
		private readonly string InputTemplateName = "field";
		private readonly string ButtonTemplateName = "button";

		public FormProcessor(ILogger iLogger, ISourceMasterRepository sourceMasterRepository, AppSettings appSettings, IMetadataProvider metadataProvider,
			IDestMasterRepository destMasterRepository, IItemConverter itemConverter, IItemFactory itemFactory)
			: base(destMasterRepository, itemConverter, itemFactory)
		{
			logger = iLogger;
			_sourceMasterRepository = sourceMasterRepository;
			_destMasterRepository = destMasterRepository;
			_appSettings = appSettings;
			_metadataProvider = metadataProvider;
		}

		public void ConvertForms()
		{
			var sourceFormTemplateId = _metadataProvider.GetItemMetadataByTemplateName(FormTemplateName)?.sourceTemplateId;

			if (sourceFormTemplateId == null)
				return;

			var destPageTemplateId = _metadataProvider.GetItemMetadataByTemplateName(PageTemplateName)?.destTemplateId;

			if (destPageTemplateId == null)
				return;

			var sourceSectionTemplateId = _metadataProvider.GetItemMetadataByTemplateName(SectionTemplateName)?.sourceTemplateId;

			if (sourceSectionTemplateId == null)
				return;

			var sourceFieldTemplateId = _metadataProvider.GetItemMetadataByTemplateName(InputTemplateName)?.sourceTemplateId;

			if (sourceFieldTemplateId == null)
				return;

			var destButtonTemplateId = _metadataProvider.GetItemMetadataByTemplateName(ButtonTemplateName)?.destTemplateId;

			if (destButtonTemplateId == null)
				return;


			var forms = _sourceMasterRepository.GetSitecoreItems((Guid) sourceFormTemplateId);
			foreach (var form in forms)
			{
				// Convert and Migrate Form items
				ConvertAndWriteItem(form, _appSettings.itemReferences["destFormFolderId"]);

				var pageId = Guid.Empty;
				var pageItem = new SCItem();
				if (!_destMasterRepository.ItemHasChildrenOfTemplate((Guid) destPageTemplateId, form))
				{
					// Create Page items for each form (only once)
					pageId = WriteNewItem((Guid) destPageTemplateId, form, "Page");
				}
				else
				{
					// Get Page Item Id
					pageItem = _destMasterRepository.GetSitecoreChildrenItems((Guid) destPageTemplateId, form.ID).FirstOrDefault();
					pageId = pageItem?.ID ?? form.ID;
				}

				// Convert and Migrate Section items
				var sections = _sourceMasterRepository.GetSitecoreChildrenItems((Guid) sourceSectionTemplateId, form.ID);
				foreach (var section in sections)
				{
					ConvertAndWriteItem(section, pageId);
				}

				// Convert and Migrate Form Field items
				List<SCItem> formFields = new List<SCItem>();
				formFields.AddRange(_sourceMasterRepository.GetSitecoreChildrenItems((Guid) sourceFieldTemplateId, form.ID));
				foreach (var section in sections)
				{
					formFields.AddRange(_sourceMasterRepository.GetSitecoreChildrenItems((Guid) sourceFieldTemplateId, section.ID));
				}

				foreach (var formField in formFields)
				{
					var parentItem = _sourceMasterRepository.GetSitecoreItem(formField.ParentID);
					var destParentId = parentItem.TemplateID == sourceFormTemplateId ? pageId : parentItem.ID;
					ConvertAndWriteItem(formField, destParentId);
				}

				// Create Submit Button
				if (!_destMasterRepository.ItemHasChildrenOfTemplate((Guid) destButtonTemplateId, pageItem))
				{
					ConvertSubmitFields(form, pageId);
				}

				// Convert Submit Mode

				// Convert Other Save Actions

				// Migrate Data

			}
		}

		private void ConvertSubmitFields(SCItem form, Guid parentId)
		{
			var submitNameField =
				form.Fields.FirstOrDefault(f => f.FieldId == new Guid("{B71296B6-32B9-4703-A8CB-FB7437271103}")); // Submit - Name field
			var submitName = submitNameField != null ? submitNameField.Value : "Submit";
			var parentItem = _destMasterRepository.GetSitecoreItem(parentId);
			var buttonMetadata = _metadataProvider.GetItemMetadataByTemplateName("Button");

			var fieldValues = GetFieldValues(form, new Guid("{B71296B6-32B9-4703-A8CB-FB7437271103}"), "Submit");

			// Set button title
			buttonMetadata.fields.newFields
				.First(field => field.destFieldId == new Guid("{71FFD7B2-8B09-4F7B-8A66-1E4CEF653E8D}")).values = fieldValues;
			// Set display name
			buttonMetadata.fields.newFields
				.First(field => field.destFieldId == new Guid("{B5E02AD9-D56F-4C41-A065-A133DB87BDEB}")).values = fieldValues;

			var submitItemId = WriteNewItem(_metadataProvider.GetItemMetadataByTemplateName("Button").destTemplateId, parentItem, "Submit", buttonMetadata);
		}

		private Dictionary<Tuple<string, int>, string> GetFieldValues(SCItem sourceItem, Guid sourceFieldId, string defaultValue)
		{
			var values = new Dictionary<Tuple<string, int>, string>();
			IEnumerable<Tuple<string, int>> langVersions = sourceItem.Fields.Where(f => f.Version != null && f.Language != null).Select(f => new Tuple<string, int>(f.Language, (int)f.Version)).Distinct();
			var languages = sourceItem.Fields.Where(f => f.Language != null).Select(f => f.Language).Distinct();
			foreach (var langVersion in langVersions)
			{
				var value = sourceItem.Fields.FirstOrDefault(f =>
					f.FieldId == sourceFieldId && f.Language == langVersion.Item1 && f.Version == langVersion.Item2)?.Value;
				values.Add(langVersion, value ?? defaultValue);
			}

			return values;
		}
	}
}
