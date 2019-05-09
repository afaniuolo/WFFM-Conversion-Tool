using System;
using System.Collections.Generic;
using System.Data;
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

		private const string FormTrackingFieldId = "{B0A67B2A-8B07-4E0B-8809-69F751709806}";
		private const string SubmitActionFieldId = "{ABC57B6D-5542-4AB9-A889-106225A032E6}";
		private const string SortOrderFieldId = "{BA3F86A2-4A1C-4D78-B63D-91C2779C1B5E}";
		private const string DisplayNameFieldId = "{B5E02AD9-D56F-4C41-A065-A133DB87BDEB}";
		private const string FormSaveToDatabaseFieldId = "{5B891B54-4FCE-489E-A7E6-841D92E9859A}";
		private const string FormSubmitModeFieldId = "{D755DE87-9C62-4EB3-83DC-2293784A3A3F}";
		private const string FormSuccessPageFieldId = "{9C4C1994-5140-49D3-BAD7-DB997816F816}";
		private const string ParametersFieldId = "{5C796924-3F06-4D1F-8510-8AD9A4244477}";
		private const string ButtonNavigationFieldId = "{D842AF43-E220-48D7-9714-6EB2381D2B0C}";
		private const string FormSaveActionFieldId = "{A7F779B9-5FCF-45CC-866B-7C973F5C4FAC}";
		private const string FormSuccessMessageFieldId = "{4E2DC894-59A2-49BB-A49C-562F611169A2}";
		private const string FormSubmitNameFieldId = "{B71296B6-32B9-4703-A8CB-FB7437271103}";

		private const string FormSubmitModeField_RedirectModeValue = "{F4D50806-6B89-4F2D-89FE-F77FC0A07D48}";
		private const string FormSubmitModeField_ShowMessageValue = "{3B8369A0-CC1A-4E9A-A3DB-7B086379C53B}";
		
		private const string FormSaveAction_RegisterCampaignValue = "{AD26FE98-EED1-45C8-95AE-F2714EE33C62}";

		private const string SubmitActionField_SaveActionValue = "{0C61EAB3-A61E-47B8-AE0B-B6EBA0D6EB1B}";
		private const string SubmitActionField_RedirectToPageActionValue = "{3F3E2093-9DEA-4199-86CA-44FC69EF624D}";
		private const string SubmitActionField_TriggerGoalActionValue = "{106587B9-1B9C-4DDB-AE96-BAC8416C21B5}";
		private const string SubmitActionField_TriggerCampaignActivityActionValue = "{4A937D74-7986-4E19-9D8E-EC14675B17F0}";

		private const string ButtonTitleFIeldId = "{71FFD7B2-8B09-4F7B-8A66-1E4CEF653E8D}";

		private const string TextFieldId = "{9666782B-21BB-40CE-B38F-8F6C53FA5070}";

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
			string tracking = form.Fields.First(field => field.FieldId == new Guid(FormTrackingFieldId))?.Value;

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
			saveDataValues.Add(new Guid(SubmitActionFieldId), SubmitActionField_SaveActionValue);
			saveDataValues.Add(new Guid(SortOrderFieldId), "0");
			ConvertSourceFieldToSubmitActionItem(form, "SubmitActionDefinition", new Guid(FormSaveToDatabaseFieldId), "1", "Save Data", saveDataValues, buttonItem);
		}

		private void ConvertSubmitMode(SCItem form, SCItem buttonItem)
		{
			var submitMode = form.Fields.FirstOrDefault(field => field.FieldId == new Guid(FormSubmitModeFieldId));
			if (submitMode != null && submitMode.Value == FormSubmitModeField_RedirectModeValue)
			{
				var successPage = form.Fields.FirstOrDefault(field => field.FieldId == new Guid(FormSuccessPageFieldId));
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
							redirectToPageValues.Add(new Guid(SubmitActionFieldId), SubmitActionField_RedirectToPageActionValue);
							redirectToPageValues.Add(new Guid(ParametersFieldId), string.Format("{{\"referenceId\":\"{0}\"}}", successPageId));
							redirectToPageValues.Add(new Guid(SortOrderFieldId), "5000");
							ConvertFieldsToSubmitActionItem("Redirect to Page", redirectToPageValues, buttonItem);
						}
					}
				}
			}
			else if (submitMode == null || submitMode.Value == FormSubmitModeField_ShowMessageValue) // Show Message Mode
			{
				// Create success page
				var successPageItemName = "Success Page";
				var pageMetadata = _metadataProvider.GetItemMetadataByTemplateName("Page");
				SCItem successPageItem = _destMasterRepository.GetSitecoreChildrenItems(pageMetadata.destTemplateId, form.ID).FirstOrDefault(item => string.Equals(item.Name, successPageItemName, StringComparison.InvariantCultureIgnoreCase));
				Guid successPageId;
				if (successPageItem == null)
				{
					successPageId = WriteNewItem(pageMetadata.destTemplateId, form, successPageItemName);
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
				buttonItem.Fields.First(field => field.FieldId == new Guid(ButtonNavigationFieldId)).Value = "1";
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
						triggerGoalValues.Add(new Guid(SubmitActionFieldId), SubmitActionField_TriggerGoalActionValue); // Submit Action field
						triggerGoalValues.Add(new Guid(ParametersFieldId), string.Format("{{\"referenceId\":\"{0}\"}}", goalId)); // Parameters field
						ConvertFieldsToSubmitActionItem("Trigger Goal", triggerGoalValues, buttonItem);
					}
				}
			}
		}

		private void ConvertSaveActions(SCItem form, string tracking, SCItem buttonItem)
		{
			var formSaveActions = form.Fields
				.FirstOrDefault(field => field.FieldId == new Guid(FormSaveActionFieldId))?.Value;
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
						if (saveActionItem.Key == FormSaveAction_RegisterCampaignValue)
						{
							var trackingCampaign = XmlHelper.GetXmlElementNodeList(tracking, "campaign");
							var trackingCampaignId = trackingCampaign.Count > 0 ? trackingCampaign[0].Attributes["id"]?.Value : null;
							if (trackingCampaignId != null)
							{
								// Create Trigger Campaign Activity Save Action
								var triggerCampaignActivityValues = new Dictionary<Guid, string>();
								triggerCampaignActivityValues.Add(new Guid(SubmitActionFieldId), SubmitActionField_TriggerCampaignActivityActionValue);
								triggerCampaignActivityValues.Add(new Guid(ParametersFieldId), string.Format("{{\"referenceId\":\"{0}\"}}", trackingCampaignId));
								ConvertFieldsToSubmitActionItem("Trigger Campaign Activity", triggerCampaignActivityValues, buttonItem);
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

			var fieldValues = GetFieldValues(form, new Guid(FormSuccessMessageFieldId),
				"Thank you for filling in the form.");

			// Set text field
			textMetadata.fields.newFields.First(field => field.destFieldId == new Guid(TextFieldId)).values = fieldValues;

			return WriteNewItem(textMetadata.destTemplateId, parentItem, "Success Message", textMetadata);
		}

		private Guid ConvertSubmitFields(SCItem form, Guid parentId)
		{
			var parentItem = _destMasterRepository.GetSitecoreItem(parentId);
			var buttonMetadata = _metadataProvider.GetItemMetadataByTemplateName("Button");

			var fieldValues = GetFieldValues(form, new Guid(FormSubmitNameFieldId), "Submit");

			// Set button title
			buttonMetadata.fields.newFields
				.First(field => field.destFieldId == new Guid(ButtonTitleFIeldId)).values = fieldValues;
			// Set display name
			buttonMetadata.fields.newFields
				.First(field => field.destFieldId == new Guid(DisplayNameFieldId)).values = fieldValues;

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

		private void CreateOrUpdateItem(string metadataTemplateName, string destItemName, Dictionary<Guid, string> destFieldValues, SCItem buttonItem)
		{
			var metadataTemplate = _metadataProvider.GetItemMetadataByTemplateName(metadataTemplateName);

			// Get Submit Action Folder
			var submitActionsFolder =
				_destMasterRepository.GetSitecoreChildrenItems(_metadataProvider.GetItemMetadataByTemplateName("Folder").destTemplateId,
					buttonItem.ID).FirstOrDefault();

			var existingConvertedItem = _destMasterRepository
				.GetSitecoreChildrenItems(metadataTemplate.destTemplateId, submitActionsFolder.ID)
				.FirstOrDefault(item => string.Equals(item.Name, destItemName, StringComparison.InvariantCultureIgnoreCase));
			if (existingConvertedItem == null)
			{
				foreach (var destFieldValue in destFieldValues)
				{
					metadataTemplate.fields.newFields.First(field => field.destFieldId == destFieldValue.Key).value = destFieldValue.Value;
				}
				// Create item
				WriteNewItem(metadataTemplate.destTemplateId, submitActionsFolder, destItemName, metadataTemplate);
				
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

		private void ConvertSourceFieldToSubmitActionItem(SCItem form, string metadataTemplateName, Guid sourceFieldId,
			string sourceFieldValue, string destItemName, Dictionary<Guid, string> destFieldValues, SCItem buttonItem)
		{
			SCField sourceFieldToConvert = null;
			if (form != null && sourceFieldId != Guid.Empty)
			{
				sourceFieldToConvert = form.Fields.FirstOrDefault(field => field.FieldId == sourceFieldId);
			}

			if (sourceFieldToConvert == null || sourceFieldToConvert.Value == sourceFieldValue)
			{
				CreateOrUpdateItem("SubmitActionDefinition", destItemName, destFieldValues, buttonItem);
			}
		}

		private void ConvertFieldsToSubmitActionItem(string destItemName, Dictionary<Guid, string> destFieldValues, SCItem buttonItem)
		{
			CreateOrUpdateItem("SubmitActionDefinition", destItemName, destFieldValues, buttonItem);
		}
	}
}
