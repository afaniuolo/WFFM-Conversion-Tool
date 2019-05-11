using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

		private const string FormShowTitleFieldId = "{A42052F7-6C18-424A-A2A1-7A72054CB1A6}";
		private const string FormTitleFieldId = "{86D6A401-5A20-49C9-A2F8-0312398EE946}";
		private const string FormTitleTagFieldId = "{6739FA82-A9B4-40F1-AEFE-3D67AE3E8DFC}";

		private const string FormShowIntroductionFieldId = "{46FB324A-9741-43BD-A0C9-5081A39D40AA}";
		private const string FormIntroductionFieldId = "{0F2414BD-A95D-419C-A238-A6637A3FAB76}";

		private const string FormShowFooterFieldId = "{FAEACD39-B059-4F46-BEC4-74BA468A97B8}";
		private const string FormFooterFieldId = "{95968A5A-DB52-4F50-B471-C6C7DA81F7E8}";

		private const string FormTitleTagStandardValue = "H1";

		private const string TextFieldId = "{9666782B-21BB-40CE-B38F-8F6C53FA5070}";
		private const string TextHtmlTagFieldId = "{C6CAA979-C3AC-4FFC-861E-2961F5FC3C48}";

		private const string SortOrderFieldId = "{BA3F86A2-4A1C-4D78-B63D-91C2779C1B5E}";

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

			var showTitle = form.Fields.FirstOrDefault(field => field.FieldId == new Guid(FormShowTitleFieldId));
			if (showTitle == null || showTitle.Value == "1")
			{
				// Create Text Item with text in Title field using Title Tag HTML element
				var title = form.Fields.FirstOrDefault(field => field.FieldId == new Guid(FormTitleFieldId))?.Value;
				if (!string.IsNullOrEmpty(title))
				{
					var titleTagField = form.Fields.FirstOrDefault(field => field.FieldId == new Guid(FormTitleTagFieldId));
					var titleTag = titleTagField != null ? titleTagField.Value : FormTitleTagStandardValue;

					// Create Text item
					var parentItem = _destMasterRepository.GetSitecoreItem(pageItem.ID);
					var textMetadata = _metadataProvider.GetItemMetadataByTemplateName("Text");

					var fieldValues = GetFieldValues(form, new Guid(FormTitleFieldId), titleItemName);

					// Set text field
					textMetadata.fields.newFields.First(field => field.destFieldId == new Guid(TextFieldId)).values = fieldValues;
					// Set Html Tag field
					textMetadata.fields.newFields.First(field => field.destFieldId == new Guid(TextHtmlTagFieldId)).value =
						ConvertTitleTag(titleTag);
					// Set __Sortorder field
					textMetadata.fields.newFields.First(field => field.destFieldId == new Guid(SortOrderFieldId)).value = "-100"; // First item in the page

					WriteNewItem(textMetadata.destTemplateId, parentItem, titleItemName, textMetadata);
				}
			}
		}

		public void ConvertIntroduction(SCItem form, SCItem pageItem)
		{
			var introductionItemName = "Introduction";

			DeleteExistingTextItem(pageItem.ID, introductionItemName);

			var showIntroduction = form.Fields.FirstOrDefault(field => field.FieldId == new Guid(FormShowIntroductionFieldId));
			if (showIntroduction != null && showIntroduction.Value == "1")
			{
				// Create Text Item with text in Introduction field
				var introduction = form.Fields.FirstOrDefault(field => field.FieldId == new Guid(FormIntroductionFieldId))?.Value;
				if (!string.IsNullOrEmpty(introduction))
				{
					// Create Text item
					var parentItem = _destMasterRepository.GetSitecoreItem(pageItem.ID);
					var textMetadata = _metadataProvider.GetItemMetadataByTemplateName("Text");
					
					var fieldValues = GetFieldValues(form, new Guid(FormIntroductionFieldId), string.Empty, true);

					// Set text field
					textMetadata.fields.newFields.First(field => field.destFieldId == new Guid(TextFieldId)).values = fieldValues;
					// Set __Sortorder field
					textMetadata.fields.newFields.First(field => field.destFieldId == new Guid(SortOrderFieldId)).value = "-50"; // Second item in the page, after title

					WriteNewItem(textMetadata.destTemplateId, parentItem, introductionItemName, textMetadata);
				}
			}
		}

		public void ConvertFooter(SCItem form, SCItem pageItem)
		{
			var footerItemName = "Footer";

			DeleteExistingTextItem(pageItem.ID, footerItemName);

			var showFooter = form.Fields.FirstOrDefault(field => field.FieldId == new Guid(FormShowFooterFieldId));
			if (showFooter != null && showFooter.Value == "1")
			{
				// Create Text Item with text in Introduction field
				var footer = form.Fields.FirstOrDefault(field => field.FieldId == new Guid(FormFooterFieldId))?.Value;
				if (!string.IsNullOrEmpty(footer))
				{
					// Create Text item
					var parentItem = _destMasterRepository.GetSitecoreItem(pageItem.ID);
					var textMetadata = _metadataProvider.GetItemMetadataByTemplateName("Text");

					var fieldValues = GetFieldValues(form, new Guid(FormFooterFieldId), string.Empty, true);

					// Set text field
					textMetadata.fields.newFields.First(field => field.destFieldId == new Guid(TextFieldId)).values = fieldValues;
					// Set __Sortorder field
					textMetadata.fields.newFields.First(field => field.destFieldId == new Guid(SortOrderFieldId)).value = "4000"; // Second item in the page, after title

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