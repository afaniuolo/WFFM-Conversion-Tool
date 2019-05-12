using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFFM.ConversionTool.Library.Constants;
using WFFM.ConversionTool.Library.Factories;
using WFFM.ConversionTool.Library.Helpers;
using WFFM.ConversionTool.Library.Models.Sitecore;
using WFFM.ConversionTool.Library.Processors;
using WFFM.ConversionTool.Library.Providers;
using WFFM.ConversionTool.Library.Repositories;

namespace WFFM.ConversionTool.Library.Converters
{
	public class AppearanceConverter : ItemProcessor
	{
		private IDestMasterRepository _destMasterRepository;
		private IMetadataProvider _metadataProvider;

		public AppearanceConverter(IMetadataProvider metadataProvider, IDestMasterRepository destMasterRepository, IItemConverter itemConverter, IItemFactory itemFactory)
			: base(destMasterRepository, itemConverter, itemFactory)
		{
			_destMasterRepository = destMasterRepository;
			_metadataProvider = metadataProvider;
		}

		public void ConvertTitle(SCItem form, SCItem pageItem)
		{
			var titleItemName = "Title";

			DeleteExistingTextItem(pageItem.ID, titleItemName);

			var showTitle = form.Fields.FirstOrDefault(field => field.FieldId == new Guid(FormConstants.FormShowTitleFieldId));
			if (showTitle == null || showTitle.Value == "1")
			{
				// Create Text Item with text in Title field using Title Tag HTML element
				var title = form.Fields.FirstOrDefault(field => field.FieldId == new Guid(FormConstants.FormTitleFieldId))?.Value;
				if (!string.IsNullOrEmpty(title))
				{
					var titleTagField = form.Fields.FirstOrDefault(field => field.FieldId == new Guid(FormConstants.FormTitleTagFieldId));
					var titleTag = titleTagField != null ? titleTagField.Value : FormConstants.FormTitleTagStandardValue;

					// Create Text item
					var parentItem = _destMasterRepository.GetSitecoreItem(pageItem.ID);
					var textMetadata = _metadataProvider.GetItemMetadataByTemplateName("Text");

					var fieldValues = GetFieldValues(form, new Guid(FormConstants.FormTitleFieldId), titleItemName);

					// Set text field
					textMetadata.fields.newFields.First(field => field.destFieldId == new Guid(TextConstants.TextFieldId)).values = fieldValues;
					// Set Html Tag field
					textMetadata.fields.newFields.First(field => field.destFieldId == new Guid(TextConstants.TextHtmlTagFieldId)).value =
						ConvertTitleTag(titleTag);
					// Set __Sortorder field
					textMetadata.fields.newFields.First(field => field.destFieldId == new Guid(BaseTemplateConstants.SortOrderFieldId)).value = "-100"; // First item in the page

					WriteNewItem(textMetadata.destTemplateId, parentItem, titleItemName, textMetadata);
				}
			}
		}

		public void ConvertIntroduction(SCItem form, SCItem pageItem)
		{
			var introductionItemName = "Introduction";

			DeleteExistingTextItem(pageItem.ID, introductionItemName);

			var showIntroduction = form.Fields.FirstOrDefault(field => field.FieldId == new Guid(FormConstants.FormShowIntroductionFieldId));
			if (showIntroduction != null && showIntroduction.Value == "1")
			{
				// Create Text Item with text in Introduction field
				var introduction = form.Fields.FirstOrDefault(field => field.FieldId == new Guid(FormConstants.FormIntroductionFieldId))?.Value;
				if (!string.IsNullOrEmpty(introduction))
				{
					// Create Text item
					var parentItem = _destMasterRepository.GetSitecoreItem(pageItem.ID);
					var textMetadata = _metadataProvider.GetItemMetadataByTemplateName("Text");
					
					var fieldValues = GetFieldValues(form, new Guid(FormConstants.FormIntroductionFieldId), string.Empty, true);

					// Set text field
					textMetadata.fields.newFields.First(field => field.destFieldId == new Guid(TextConstants.TextFieldId)).values = fieldValues;
					// Set __Sortorder field
					textMetadata.fields.newFields.First(field => field.destFieldId == new Guid(BaseTemplateConstants.SortOrderFieldId)).value = "-50"; // Second item in the page, after title

					WriteNewItem(textMetadata.destTemplateId, parentItem, introductionItemName, textMetadata);
				}
			}
		}

		public void ConvertFooter(SCItem form, SCItem pageItem)
		{
			var footerItemName = "Footer";

			DeleteExistingTextItem(pageItem.ID, footerItemName);

			var showFooter = form.Fields.FirstOrDefault(field => field.FieldId == new Guid(FormConstants.FormShowFooterFieldId));
			if (showFooter != null && showFooter.Value == "1")
			{
				// Create Text Item with text in Introduction field
				var footer = form.Fields.FirstOrDefault(field => field.FieldId == new Guid(FormConstants.FormFooterFieldId))?.Value;
				if (!string.IsNullOrEmpty(footer))
				{
					// Create Text item
					var parentItem = _destMasterRepository.GetSitecoreItem(pageItem.ID);
					var textMetadata = _metadataProvider.GetItemMetadataByTemplateName("Text");

					var fieldValues = GetFieldValues(form, new Guid(FormConstants.FormFooterFieldId), string.Empty, true);

					// Set text field
					textMetadata.fields.newFields.First(field => field.destFieldId == new Guid(TextConstants.TextFieldId)).values = fieldValues;
					// Set __Sortorder field
					textMetadata.fields.newFields.First(field => field.destFieldId == new Guid(BaseTemplateConstants.SortOrderFieldId)).value = "4000"; // Second item in the page, after title

					WriteNewItem(textMetadata.destTemplateId, parentItem, footerItemName, textMetadata);
				}
			}
		}

		private string ConvertTitleTag(string sourceTitleTagValue)
		{
			var tagValue = sourceTitleTagValue.ToLower();
			switch (tagValue)
			{
				case "a":
				case "b":
				case "strong":
					return "h1";
				default:
					return tagValue;
			}
		}

		private Dictionary<Tuple<string, int>, string> GetFieldValues(SCItem sourceItem, Guid sourceFieldId, string defaultValue, bool stripHtml = false)
		{
			var values = new Dictionary<Tuple<string, int>, string>();
			IEnumerable<Tuple<string, int>> langVersions = sourceItem.Fields.Where(f => f.Version != null && f.Language != null).Select(f => new Tuple<string, int>(f.Language, (int)f.Version)).Distinct();
			var languages = sourceItem.Fields.Where(f => f.Language != null).Select(f => f.Language).Distinct();
			foreach (var langVersion in langVersions)
			{
				var value = sourceItem.Fields.FirstOrDefault(f =>
					f.FieldId == sourceFieldId && f.Language == langVersion.Item1 && f.Version == langVersion.Item2)?.Value;
				if (stripHtml)
				{
					value = XmlHelper.StripHtml(value);
				}
				values.Add(langVersion, value ?? defaultValue);
			}

			return values;
		}

		private void DeleteExistingTextItem(Guid parentId, string itemName)
		{
			var textMetadata = _metadataProvider.GetItemMetadataByTemplateName("Text");
			var textItem = _destMasterRepository.GetSitecoreChildrenItems(textMetadata.destTemplateId, parentId)
				.FirstOrDefault(item => string.Equals(item.Name, itemName, StringComparison.InvariantCultureIgnoreCase));
			if (textItem != null)
			{
				_destMasterRepository.DeleteSitecoreItem(textItem);
			}
		}
	}
}