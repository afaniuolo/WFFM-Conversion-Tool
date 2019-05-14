using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using WFFM.ConversionTool.Library.Constants;
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
		private IFieldProvider _fieldProvider;
		private AppSettings _appSettings;
		
		public SubmitConverter(IMetadataProvider metadataProvider, IDestMasterRepository destMasterRepository, IItemConverter itemConverter, IItemFactory itemFactory, IFieldProvider fieldProvider, AppSettings appSettings)
			: base(destMasterRepository, itemConverter, itemFactory)
		{
			_destMasterRepository = destMasterRepository;
			_metadataProvider = metadataProvider;
			_fieldProvider = fieldProvider;
			_appSettings = appSettings;
		}

		public void Convert(SCItem form, SCItem pageItem)
		{
			// Create Submit Button
			SCItem buttonItem = ConvertSubmitButton(form, pageItem);

			// Get tracking field
			string tracking = form.Fields.First(field => field.FieldId == new Guid(FormConstants.FormTrackingFieldId))?.Value;

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
			saveDataValues.Add(new Guid(SubmitActionConstants.SubmitActionFieldId), SubmitActionConstants.SubmitActionField_SaveActionValue);
			saveDataValues.Add(new Guid(BaseTemplateConstants.SortOrderFieldId), "0");
			ConvertSourceFieldToSubmitActionItem(form, new Guid(FormConstants.FormSaveToDatabaseFieldId), "1", "Save Data", saveDataValues, buttonItem);
		}

		private void ConvertSubmitMode(SCItem form, SCItem buttonItem)
		{
			var submitMode = form.Fields.FirstOrDefault(field => field.FieldId == new Guid(FormConstants.FormSubmitModeFieldId));
			if (submitMode != null && submitMode.Value == FormConstants.FormSubmitModeField_RedirectModeValue)
			{
				var successPage = form.Fields.FirstOrDefault(field => field.FieldId == new Guid(FormConstants.FormSuccessPageFieldId));
				if (successPage != null && !string.IsNullOrEmpty(successPage.Value))
				{
					var successPageLink = XmlHelper.GetXmlElementNode(successPage.Value, "link");
					if (successPageLink != null)
					{
						var successPageId = successPageLink.Attributes["id"]?.Value;
						if (successPageId != null)
						{
							if (!_appSettings.enableReferencedItemCheck || _destMasterRepository.ItemExists(new Guid(successPageId)))
							{
								// Redirect To Page Action
								var redirectToPageValues = new Dictionary<Guid, string>();
								redirectToPageValues.Add(new Guid(SubmitActionConstants.SubmitActionFieldId),
									SubmitActionConstants.SubmitActionField_RedirectToPageActionValue);
								redirectToPageValues.Add(new Guid(SubmitActionConstants.ParametersFieldId),
									string.Format("{{\"referenceId\":\"{0}\"}}", successPageId));
								redirectToPageValues.Add(new Guid(BaseTemplateConstants.SortOrderFieldId), "5000");
								ConvertFieldsToSubmitActionItem("Redirect to Page", redirectToPageValues, buttonItem);
							}
						}
					}
				}
			}
			else if (submitMode == null || submitMode.Value == FormConstants.FormSubmitModeField_ShowMessageValue) // Show Message Mode
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
				buttonItem.Fields.First(field => field.FieldId == new Guid(ButtonConstants.ButtonNavigationFieldId)).Value = "1";
				UpdateItem(buttonItem);
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
						if (!_appSettings.enableReferencedItemCheck || _destMasterRepository.ItemExists(new Guid(goalId)))
						{
							// Create Trigger Goal Save Action
							var triggerGoalValues = new Dictionary<Guid, string>();
							triggerGoalValues.Add(new Guid(SubmitActionConstants.SubmitActionFieldId),
								SubmitActionConstants.SubmitActionField_TriggerGoalActionValue); // Submit Action field
							triggerGoalValues.Add(new Guid(SubmitActionConstants.ParametersFieldId),
								string.Format("{{\"referenceId\":\"{0}\"}}", goalId)); // Parameters field
							ConvertFieldsToSubmitActionItem("Trigger Goal", triggerGoalValues, buttonItem);
						}
					}
				}
			}
		}

		private void ConvertSaveActions(SCItem form, string tracking, SCItem buttonItem)
		{
			var formSaveActions = form.Fields
				.FirstOrDefault(field => field.FieldId == new Guid(FormConstants.FormSaveActionFieldId))?.Value;
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
						if (saveActionItem.Key == FormConstants.FormSaveAction_RegisterCampaignValue)
						{
							var trackingCampaign = XmlHelper.GetXmlElementNodeList(tracking, "campaign");
							var trackingCampaignId = trackingCampaign.Count > 0 ? trackingCampaign[0].Attributes["id"]?.Value : null;
							if (trackingCampaignId != null)
							{
								if (!_appSettings.enableReferencedItemCheck || _destMasterRepository.ItemExists(new Guid(trackingCampaignId)))
								{
									// Create Trigger Campaign Activity Save Action
									var triggerCampaignActivityValues = new Dictionary<Guid, string>();
									triggerCampaignActivityValues.Add(new Guid(SubmitActionConstants.SubmitActionFieldId),
										SubmitActionConstants.SubmitActionField_TriggerCampaignActivityActionValue);
									triggerCampaignActivityValues.Add(new Guid(SubmitActionConstants.ParametersFieldId),
										string.Format("{{\"referenceId\":\"{0}\"}}", trackingCampaignId));
									ConvertFieldsToSubmitActionItem("Trigger Campaign Activity", triggerCampaignActivityValues, buttonItem);
								}
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

			var fieldValues = _fieldProvider.GetFieldValues(form, new Guid(FormConstants.FormSuccessMessageFieldId),
				"Thank you for filling in the form.");

			// Set text field
			textMetadata.fields.newFields.First(field => field.destFieldId == new Guid(TextConstants.TextFieldId)).values = fieldValues;

			return WriteNewItem(textMetadata.destTemplateId, parentItem, "Success Message", textMetadata);
		}

		private Guid ConvertSubmitFields(SCItem form, Guid parentId)
		{
			var parentItem = _destMasterRepository.GetSitecoreItem(parentId);
			var buttonMetadata = _metadataProvider.GetItemMetadataByTemplateName("Button");

			var fieldValues = _fieldProvider.GetFieldValues(form, new Guid(FormConstants.FormSubmitNameFieldId), "Submit");

			// Set button title
			buttonMetadata.fields.newFields
				.First(field => field.destFieldId == new Guid(ButtonConstants.ButtonTitleFIeldId)).values = fieldValues;
			// Set display name
			buttonMetadata.fields.newFields
				.First(field => field.destFieldId == new Guid(BaseTemplateConstants.DisplayNameFieldId)).values = fieldValues;

			return WriteNewItem(_metadataProvider.GetItemMetadataByTemplateName("Button").destTemplateId, parentItem, "Submit", buttonMetadata);
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

		private void ConvertSourceFieldToSubmitActionItem(SCItem form, Guid sourceFieldId,
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
