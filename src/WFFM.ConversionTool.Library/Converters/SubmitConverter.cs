using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using WFFM.ConversionTool.Library.Factories;
using WFFM.ConversionTool.Library.Helpers;
using WFFM.ConversionTool.Library.Logging;
using WFFM.ConversionTool.Library.Models.Metadata;
using WFFM.ConversionTool.Library.Models.Sitecore;
using WFFM.ConversionTool.Library.Processors;
using WFFM.ConversionTool.Library.Providers;
using WFFM.ConversionTool.Library.Repositories;

namespace WFFM.ConversionTool.Library.Converters
{
	public class SubmitConverter : ItemProcessor
	{
		private IDestMasterRepository _destMasterRepository;
		private IMetadataProvider _metadataProvider;

		public SubmitConverter(IMetadataProvider metadataProvider, IDestMasterRepository destMasterRepository, IItemConverter itemConverter, IItemFactory itemFactory)
			: base(destMasterRepository, itemConverter, itemFactory)
		{
			_destMasterRepository = destMasterRepository;
			_metadataProvider = metadataProvider;
		}

		public void Convert(SCItem form, SCItem pageItem)
		{
			// Create Submit Button
			SCItem buttonItem = ConvertSubmitButton(form, pageItem);

			// Get tracking field
			string tracking = form.Fields.First(field => field.FieldId == new Guid("{B0A67B2A-8B07-4E0B-8809-69F751709806}"))?.Value;

			// Save Data Action
			ConvertSaveDataAction(form, buttonItem);

			// Convert Submit Mode
			ConvertSubmitMode(form, buttonItem);

			// Trigger Goal
			ConvertTriggerGoal(form, tracking, buttonItem);

			// Convert Other Save Actions
			ConvertSaveActions(form, tracking, buttonItem);
		}

		private SCItem ConvertSubmitButton(SCItem form, SCItem pageItem)
		{
			var buttonMetadata = _metadataProvider.GetItemMetadataByTemplateName("Button");
			SCItem buttonItem;
			if (!_destMasterRepository.ItemHasChildrenOfTemplate(buttonMetadata.destTemplateId, pageItem))
			{
				var buttonItemId = ConvertSubmitFields(form, pageItem.ID);
				buttonItem = _destMasterRepository.GetSitecoreItem(buttonItemId);
				WriteDescendentItems(buttonMetadata, buttonItem);
			}
			else
			{
				buttonItem = _destMasterRepository.GetSitecoreDescendantsItems(buttonMetadata.destTemplateId, pageItem.ID).FirstOrDefault();
			}

			return buttonItem;
		}

		private void ConvertSaveDataAction(SCItem form, SCItem buttonItem)
		{
			var saveDataValues = new Dictionary<Guid, string>();
			saveDataValues.Add(new Guid("{ABC57B6D-5542-4AB9-A889-106225A032E6}"), "{0C61EAB3-A61E-47B8-AE0B-B6EBA0D6EB1B}"); // Submit Action field
			saveDataValues.Add(new Guid("{BA3F86A2-4A1C-4D78-B63D-91C2779C1B5E}"), "0"); // __Sort order field
			ConvertFieldsIfSourceFieldValue(form, "SubmitActionDefinition", new Guid("{5B891B54-4FCE-489E-A7E6-841D92E9859A}"), "1", "Save Data", saveDataValues, buttonItem);
		}

		private void ConvertSubmitMode(SCItem form, SCItem buttonItem)
		{
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
							var redirectToPageValues = new Dictionary<Guid, string>();
							redirectToPageValues.Add(new Guid("{ABC57B6D-5542-4AB9-A889-106225A032E6}"), "{3F3E2093-9DEA-4199-86CA-44FC69EF624D}"); // Submit Action field
							redirectToPageValues.Add(new Guid("{5C796924-3F06-4D1F-8510-8AD9A4244477}"), string.Format("{{\"referenceId\":\"{0}\"}}", successPageId)); // Parameters field
							redirectToPageValues.Add(new Guid("{BA3F86A2-4A1C-4D78-B63D-91C2779C1B5E}"), "5000"); // Parameters field
							ConvertFieldsIfSourceFieldValue(form, "SubmitActionDefinition", new Guid("{ABC57B6D-5542-4AB9-A889-106225A032E6}"), "", "Redirect to Page", redirectToPageValues, buttonItem);
						}
					}
				}
			}
			else if (submitMode == null || submitMode.Value == "{3B8369A0-CC1A-4E9A-A3DB-7B086379C53B}") // Show Message Mode
			{
				// Create success page
				var pageMetadata = _metadataProvider.GetItemMetadataByTemplateName("Page");
				SCItem successPageItem = _destMasterRepository.GetSitecoreChildrenItems(pageMetadata.destTemplateId, form.ID).FirstOrDefault(item => string.Equals(item.Name, "Success Page", StringComparison.InvariantCultureIgnoreCase));
				Guid successPageId;
				if (successPageItem == null)
				{
					successPageId = WriteNewItem(pageMetadata.destTemplateId, form, "Success Page");
					successPageItem = _destMasterRepository.GetSitecoreItem(successPageId);
				}
				else
				{
					successPageId = successPageItem.ID;
				}

				// Create Text item with message stored in Success Message field
				var textMetadata = _metadataProvider.GetItemMetadataByTemplateName("Text");
				var successMessageItem = _destMasterRepository.GetSitecoreChildrenItems(textMetadata.destTemplateId, successPageId)
					.FirstOrDefault(item => string.Equals(item.Name, "Success Message", StringComparison.InvariantCultureIgnoreCase));
				if (successMessageItem != null)
				{
					_destMasterRepository.DeleteSitecoreItem(successMessageItem);
				}
				ConvertTextField(form, successPageId);

				// Configure Navigation field in Submit button to go to next page
				buttonItem.Fields.First(field => field.FieldId == new Guid("{D842AF43-E220-48D7-9714-6EB2381D2B0C}")).Value = "1";
			}
		}

		private void ConvertTriggerGoal(SCItem form, string tracking, SCItem buttonItem)
		{
			var trackingEvents = XmlHelper.GetXmlElementNodeList(tracking, "event");
			if (trackingEvents.Count > 0)
			{
				var trackingGoalEvent = trackingEvents[trackingEvents.Count - 1];
				if (trackingGoalEvent != null)
				{
					var goalId = trackingGoalEvent.Attributes["id"]?.Value;
					if (!string.IsNullOrEmpty(goalId))
					{
						// Create Trigger Goal Save Action
						var triggerGoalValues = new Dictionary<Guid, string>();
						triggerGoalValues.Add(new Guid("{ABC57B6D-5542-4AB9-A889-106225A032E6}"), "{106587B9-1B9C-4DDB-AE96-BAC8416C21B5}"); // Submit Action field
						triggerGoalValues.Add(new Guid("{5C796924-3F06-4D1F-8510-8AD9A4244477}"), string.Format("{{\"referenceId\":\"{0}\"}}", goalId)); // Parameters field
						ConvertFieldsToItem("SubmitActionDefinition", "Trigger Goal", triggerGoalValues, buttonItem);
					}
				}
			}
		}

		private void ConvertSaveActions(SCItem form, string tracking, SCItem buttonItem)
		{
			var formSaveActions = form.Fields
				.FirstOrDefault(field => field.FieldId == new Guid("{A7F779B9-5FCF-45CC-866B-7C973F5C4FAC}"))?.Value;
			if (!string.IsNullOrEmpty(formSaveActions))
			{
				var saveActionElements = XmlHelper.GetXmlElementNodeList(XmlHelper.GetXmlElementNode(formSaveActions, "g").InnerXml, "li");
				Dictionary<string, string> saveActionItems = new Dictionary<string, string>();
				if (saveActionElements != null)
				{
					foreach (XmlNode saveActionElement in saveActionElements)
					{
						saveActionItems.Add(saveActionElement.Attributes["id"].Value,
							XmlHelper.GetXmlElementValue(saveActionElement.Value, "parameters"));
					}

					foreach (var saveActionItem in saveActionItems)
					{
						if (saveActionItem.Key == "{AD26FE98-EED1-45C8-95AE-F2714EE33C62}") // Register a Campaign
						{
							var trackingCampaign = XmlHelper.GetXmlElementNodeList(tracking, "campaign");
							var trackingCampaignId = trackingCampaign.Count > 0 ? trackingCampaign[0].Attributes["id"]?.Value : null;
							if (trackingCampaignId != null)
							{
								// Create Trigger Campaign Activity Save Action
								var triggerCampaignActivityValues = new Dictionary<Guid, string>();
								triggerCampaignActivityValues.Add(new Guid("{ABC57B6D-5542-4AB9-A889-106225A032E6}"), "{4A937D74-7986-4E19-9D8E-EC14675B17F0}"); // Submit Action field
								triggerCampaignActivityValues.Add(new Guid("{5C796924-3F06-4D1F-8510-8AD9A4244477}"), string.Format("{{\"referenceId\":\"{0}\"}}", trackingCampaignId)); // Parameters field
								ConvertFieldsToItem("SubmitActionDefinition", "Trigger Campaign Activity", triggerCampaignActivityValues, buttonItem);
							}
						}
						else
						{
							// TODO: logic to flag save actions not mapped
						}
					}
				}
			}
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

		private void ConvertFieldsIfSourceFieldValue(SCItem form, string metadataTemplateName, Guid sourceFieldId, 
			string sourceFieldValue, string destItemName, Dictionary<Guid, string> destFieldValues, SCItem buttonItem)
		{
			var metadataTemplate = _metadataProvider.GetItemMetadataByTemplateName(metadataTemplateName);

			// Get Submit Action Folder
			var submitActionsFolder =
				_destMasterRepository.GetSitecoreChildrenItems(_metadataProvider.GetItemMetadataByTemplateName("Folder").destTemplateId,
					buttonItem.ID).FirstOrDefault();

			SCField sourceFieldToConvert = null;
			if (form != null && sourceFieldId != Guid.Empty)
			{
				sourceFieldToConvert = form.Fields.FirstOrDefault(field => field.FieldId == sourceFieldId);
			}

			var existingConvertedItem = _destMasterRepository
				.GetSitecoreChildrenItems(metadataTemplate.destTemplateId, submitActionsFolder.ID)
				.FirstOrDefault(item => string.Equals(item.Name, destItemName, StringComparison.InvariantCultureIgnoreCase));
			if (existingConvertedItem == null)
			{
				if (sourceFieldToConvert == null || sourceFieldToConvert.Value == sourceFieldValue)
				{
					foreach (var destFieldValue in destFieldValues)
					{
						metadataTemplate.fields.newFields.First(field => field.destFieldId == destFieldValue.Key).value = destFieldValue.Value;
					}
					// Create item
					WriteNewItem(metadataTemplate.destTemplateId, submitActionsFolder, destItemName, metadataTemplate);
				}
			}
			else
			{
				foreach (var destFieldValue in destFieldValues)
				{
					existingConvertedItem.Fields.First(field => field.FieldId == destFieldValue.Key).Value = destFieldValue.Value;
				}

				_destMasterRepository.AddOrUpdateSitecoreItem(existingConvertedItem);
			}
		}

		private void ConvertFieldsToItem(string metadataTemplateName, string destItemName, Dictionary<Guid, string> destFieldValues, SCItem buttonItem)
		{
			ConvertFieldsIfSourceFieldValue(null, metadataTemplateName, Guid.Empty, string.Empty, destItemName, destFieldValues, buttonItem);
		}
	}
}
