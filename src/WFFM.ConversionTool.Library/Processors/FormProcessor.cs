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
				Guid buttonItemId;
				SCItem buttonItem;
				var buttonMetadata = _metadataProvider.GetItemMetadataByTemplateName("Button");
				if (!_destMasterRepository.ItemHasChildrenOfTemplate((Guid) destButtonTemplateId, pageItem))
				{					
					buttonItemId = ConvertSubmitFields(form, pageId);
					buttonItem = _destMasterRepository.GetSitecoreItem(buttonItemId);
					CreateDescendantItems(buttonItem, buttonMetadata);
				}			
				else
				{
					buttonItem = _destMasterRepository.GetSitecoreDescendantsItems(buttonMetadata.destTemplateId, pageItem.ID).FirstOrDefault();
				}

				var submitActionsFolder =
					_destMasterRepository.GetSitecoreChildrenItems(_metadataProvider.GetItemMetadataByTemplateName("Folder").destTemplateId,
						buttonItem.ID).FirstOrDefault();

				// Save Data Action
				var submitActionDefMetadata = _metadataProvider.GetItemMetadataByTemplateName("SubmitActionDefinition");
				var saveFormDataToStorage =
					form.Fields.FirstOrDefault(field => field.FieldId == new Guid("{5B891B54-4FCE-489E-A7E6-841D92E9859A}")); // Save Form Data To Storage field
				var saveDataItem = _destMasterRepository
					.GetSitecoreChildrenItems(submitActionDefMetadata.destTemplateId, submitActionsFolder.ID)
					.FirstOrDefault(item => string.Equals(item.Name, "Save Data", StringComparison.InvariantCultureIgnoreCase));
				if (saveDataItem == null)
				{
					if (saveFormDataToStorage == null || saveFormDataToStorage.Value == "1")
					{
						// Create Save Data submit action					
						// Set Submit Action field
						submitActionDefMetadata.fields.newFields
								.First(field => field.destFieldId == new Guid("{ABC57B6D-5542-4AB9-A889-106225A032E6}")).value =
							"{0C61EAB3-A61E-47B8-AE0B-B6EBA0D6EB1B}";
						WriteNewItem(submitActionDefMetadata.destTemplateId, submitActionsFolder, "Save Data", submitActionDefMetadata);
					}
				}
				else
				{
					saveDataItem.Fields.First(field => field.FieldId == new Guid("{ABC57B6D-5542-4AB9-A889-106225A032E6}")).Value =
						"{0C61EAB3-A61E-47B8-AE0B-B6EBA0D6EB1B}";
					_destMasterRepository.AddOrUpdateSitecoreItem(saveDataItem);
				}

				// Convert Submit Mode

				// Convert Other Save Actions

				// Migrate Data

			}
		}

		private void CreateDescendantItems(SCItem parentItem, MetadataTemplate metadataTemplate)
		{
			// Create descendant items
			if (metadataTemplate.descendantItems != null)
			{
				foreach (var descendantItem in metadataTemplate.descendantItems)
				{
					if (descendantItem.isParentChild)
					{
						var destDescItemId = CreateDescendantItem(descendantItem, parentItem, metadataTemplate);
					}
					else
					{
						var destTemplateId = _metadataProvider.GetItemMetadataByTemplateName(descendantItem.destTemplateName)
							.destTemplateId;
						var parentItemChildren = _destMasterRepository.GetSitecoreDescendantsItems(destTemplateId, parentItem.ID);
						var destParentItem = parentItemChildren.FirstOrDefault(d =>
							string.Equals(d.Name, descendantItem.parentItemName, StringComparison.InvariantCultureIgnoreCase));
						if (destParentItem != null)
						{
							var destDescItem = CreateDescendantItem(descendantItem, destParentItem, metadataTemplate);
						}
					}
				}
			}
		}

		private Guid CreateDescendantItem(MetadataTemplate.DescendantItem descendantItem, SCItem destParentItem, MetadataTemplate metadataTemplate)
		{
			var _descendantItemMetadataTemplate =
				_metadataProvider.GetItemMetadataByTemplateName(descendantItem.destTemplateName);
			var children = _destMasterRepository.GetSitecoreChildrenItems(_descendantItemMetadataTemplate.destTemplateId,
				destParentItem.ID);
			if (children != null && children.Any(i =>
				    string.Equals(i.Name, descendantItem.itemName, StringComparison.InvariantCultureIgnoreCase)))
			{
				var child = children.FirstOrDefault(i =>
					string.Equals(i.Name, descendantItem.itemName, StringComparison.InvariantCultureIgnoreCase));

				return child != null ? child.ID : Guid.Empty;
			}
			return WriteNewItem(_descendantItemMetadataTemplate.destTemplateId, destParentItem, descendantItem.itemName);
		}

		private Guid ConvertSubmitFields(SCItem form, Guid parentId)
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

			return WriteNewItem(_metadataProvider.GetItemMetadataByTemplateName("Button").destTemplateId, parentItem, "Submit", buttonMetadata);
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
