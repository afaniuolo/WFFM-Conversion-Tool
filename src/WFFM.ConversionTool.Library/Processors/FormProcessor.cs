using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using WFFM.ConversionTool.Library.Converters;
using WFFM.ConversionTool.Library.Database.Forms;
using WFFM.ConversionTool.Library.Database.Master;
using WFFM.ConversionTool.Library.Database.WFFM;
using WFFM.ConversionTool.Library.Factories;
using WFFM.ConversionTool.Library.Helpers;
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


			var forms = _sourceMasterRepository.GetSitecoreItems((Guid)sourceFormTemplateId);
			foreach (var form in forms)
			{
				// Convert and Migrate Form items
				ConvertAndWriteItem(form, _appSettings.itemReferences["destFormFolderId"]);

				var pageId = Guid.Empty;
				var pageItem = new SCItem();
				if (!_destMasterRepository.ItemHasChildrenOfTemplate((Guid)destPageTemplateId, form))
				{
					// Create Page items for each form (only once)
					pageId = WriteNewItem((Guid)destPageTemplateId, form, "Page");
				}
				else
				{
					// Get Page Item Id
					pageItem = _destMasterRepository.GetSitecoreChildrenItems((Guid)destPageTemplateId, form.ID).FirstOrDefault(item => string.Equals(item.Name, "Page", StringComparison.InvariantCultureIgnoreCase));
					pageId = pageItem?.ID ?? form.ID;
				}

				// Convert and Migrate Section items
				var sections = _sourceMasterRepository.GetSitecoreChildrenItems((Guid)sourceSectionTemplateId, form.ID);
				foreach (var section in sections)
				{
					ConvertAndWriteItem(section, pageId);
				}

				// Convert and Migrate Form Field items
				List<SCItem> formFields = new List<SCItem>();
				formFields.AddRange(_sourceMasterRepository.GetSitecoreChildrenItems((Guid)sourceFieldTemplateId, form.ID));
				foreach (var section in sections)
				{
					formFields.AddRange(_sourceMasterRepository.GetSitecoreChildrenItems((Guid)sourceFieldTemplateId, section.ID));
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
				if (!_destMasterRepository.ItemHasChildrenOfTemplate((Guid)destButtonTemplateId, pageItem))
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
						// Set __Sort order field
						submitActionDefMetadata.fields.newFields
							.First(field => field.destFieldId == new Guid("{BA3F86A2-4A1C-4D78-B63D-91C2779C1B5E}")).value = "0";
						WriteNewItem(submitActionDefMetadata.destTemplateId, submitActionsFolder, "Save Data", submitActionDefMetadata);
					}
				}
				else
				{
					// Set Submit Action field
					saveDataItem.Fields.First(field => field.FieldId == new Guid("{ABC57B6D-5542-4AB9-A889-106225A032E6}")).Value =
						"{0C61EAB3-A61E-47B8-AE0B-B6EBA0D6EB1B}";
					// Set __Sort order field
					submitActionDefMetadata.fields.newFields
						.First(field => field.destFieldId == new Guid("{BA3F86A2-4A1C-4D78-B63D-91C2779C1B5E}")).value = "0";
					_destMasterRepository.AddOrUpdateSitecoreItem(saveDataItem);
				}

				// Convert Submit Mode
				var submitMode = form.Fields.FirstOrDefault(field => field.FieldId == new Guid("{D755DE87-9C62-4EB3-83DC-2293784A3A3F}")); // Submit Mode field
				if (submitMode != null && submitMode.Value == "{F4D50806-6B89-4F2D-89FE-F77FC0A07D48}") // Redirect Mode
				{
					var successPage = form.Fields.FirstOrDefault(field => field.FieldId == new Guid("{9C4C1994-5140-49D3-BAD7-DB997816F816}")); // Success Page field
					if (successPage != null && !string.IsNullOrEmpty(successPage.Value))
					{
						var successPageLink = XmlHelper.GetXmlElementNode(successPage.Value, "link");
						if (successPageLink != null)
						{
							var successPageId = successPageLink.Attributes["id"]?.Value;
							if (successPageId != null)
							{
								// Redirect To Page Action
								var redirectToPageSubmitAction = _metadataProvider.GetItemMetadataByTemplateName("SubmitActionDefinition");
								var redirectToPageItem = _destMasterRepository
									.GetSitecoreChildrenItems(redirectToPageSubmitAction.destTemplateId, submitActionsFolder.ID)
									.FirstOrDefault(item => string.Equals(item.Name, "Redirect to Page", StringComparison.InvariantCultureIgnoreCase));
								if (redirectToPageItem == null)
								{
									// Set Submit Action field
									redirectToPageSubmitAction.fields.newFields
											.First(field => field.destFieldId == new Guid("{ABC57B6D-5542-4AB9-A889-106225A032E6}")).value =
										"{3F3E2093-9DEA-4199-86CA-44FC69EF624D}";
									// Set Parameters field
									redirectToPageSubmitAction.fields.newFields
											.First(field => field.destFieldId == new Guid("{5C796924-3F06-4D1F-8510-8AD9A4244477}")).value =
										string.Format("{{\"referenceId\":\"{0}\"}}", successPageId);
									// Set __Sort order field
									redirectToPageSubmitAction.fields.newFields
										.First(field => field.destFieldId == new Guid("{BA3F86A2-4A1C-4D78-B63D-91C2779C1B5E}")).value = "5000";

									WriteNewItem(redirectToPageSubmitAction.destTemplateId, submitActionsFolder, "Redirect to Page",
										redirectToPageSubmitAction);

								}
								else
								{
									// Set Submit Action field
									redirectToPageItem.Fields.First(field => field.FieldId == new Guid("{ABC57B6D-5542-4AB9-A889-106225A032E6}")).Value =
										"{3F3E2093-9DEA-4199-86CA-44FC69EF624D}";
									// Set Parameters field
									redirectToPageItem.Fields.First(field => field.FieldId == new Guid("{5C796924-3F06-4D1F-8510-8AD9A4244477}")).Value =
										string.Format("{{\"referenceId\":\"{0}\"}}", successPageId);
									// Set __Sort order field
									redirectToPageItem.Fields.First(field => field.FieldId == new Guid("{BA3F86A2-4A1C-4D78-B63D-91C2779C1B5E}")).Value = "5000";

									_destMasterRepository.AddOrUpdateSitecoreItem(redirectToPageItem);
								}
							}
						}
					}
				}
				else if (submitMode == null || submitMode.Value == "{3B8369A0-CC1A-4E9A-A3DB-7B086379C53B}") // Show Message Mode
				{
					// Create success page
					SCItem successPageItem = _destMasterRepository.GetSitecoreChildrenItems((Guid)destPageTemplateId, form.ID).FirstOrDefault(item => string.Equals(item.Name, "Success Page", StringComparison.InvariantCultureIgnoreCase));
					Guid successPageId;
					if (successPageItem == null)
					{
						successPageId = WriteNewItem((Guid)destPageTemplateId, form, "Success Page");
						successPageItem = _destMasterRepository.GetSitecoreItem(successPageId);
					}
					else
					{
						successPageId = successPageItem.ID;
					}

					// Create Text item with message stored in Success Message field
					Guid textItemId;
					SCItem textItem;
					var textMetadata = _metadataProvider.GetItemMetadataByTemplateName("Text");
					if (!_destMasterRepository.ItemHasChildrenOfTemplate(textMetadata.destTemplateId, successPageItem))
					{
						textItemId = ConvertTextField(form, successPageId);
					}
					else
					{
						var fieldValues = GetFieldValues(form, new Guid("{4E2DC894-59A2-49BB-A49C-562F611169A2}"),
							"Thank you for filling in the form.");

						// Set text field
						textMetadata.fields.newFields.First(field => field.destFieldId == new Guid("{9666782B-21BB-40CE-B38F-8F6C53FA5070}")).values = fieldValues;
						textItemId = WriteNewItem(textMetadata.destTemplateId, successPageItem, "Success Message", textMetadata);
					}
					// Configure Navigation field in Submit button to go to next page
					buttonItem.Fields.First(field => field.FieldId == new Guid("{D842AF43-E220-48D7-9714-6EB2381D2B0C}")).Value = "1";
				}

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

		private Guid ConvertTextField(SCItem form, Guid parentId)
		{
			var parentItem = _destMasterRepository.GetSitecoreItem(parentId);
			var textMetadata = _metadataProvider.GetItemMetadataByTemplateName("Text");

			var fieldValues = GetFieldValues(form, new Guid("{4E2DC894-59A2-49BB-A49C-562F611169A2}"),
				"Thank you for filling in the form.");

			// Set text field
			textMetadata.fields.newFields.First(field => field.destFieldId == new Guid("{9666782B-21BB-40CE-B38F-8F6C53FA5070}")).values = fieldValues;

			return WriteNewItem(textMetadata.destTemplateId, parentItem, "Success Message", textMetadata);
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
