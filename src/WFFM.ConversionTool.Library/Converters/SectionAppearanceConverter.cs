using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFFM.ConversionTool.Library.Constants;
using WFFM.ConversionTool.Library.Factories;
using WFFM.ConversionTool.Library.Helpers;
using WFFM.ConversionTool.Library.Models.Metadata;
using WFFM.ConversionTool.Library.Models.Sitecore;
using WFFM.ConversionTool.Library.Processors;
using WFFM.ConversionTool.Library.Providers;
using WFFM.ConversionTool.Library.Repositories;

namespace WFFM.ConversionTool.Library.Converters
{
	public class SectionAppearanceConverter : ItemProcessor
	{
		private IDestMasterRepository _destMasterRepository;
		private IMetadataProvider _metadataProvider;
		private IFieldProvider _fieldProvider;
		private AppSettings _appSettings;

		public SectionAppearanceConverter(IMetadataProvider metadataProvider, IDestMasterRepository destMasterRepository, IItemConverter itemConverter, IItemFactory itemFactory, IFieldProvider fieldProvider, AppSettings appSettings)
			: base(destMasterRepository, itemConverter, itemFactory, appSettings)
		{
			_destMasterRepository = destMasterRepository;
			_metadataProvider = metadataProvider;
			_fieldProvider = fieldProvider;
			_appSettings = appSettings;
		}

		public void ConvertTitle(SCItem sectionItem)
		{
			var titleItemName = "Title";
			var textMetadata = _metadataProvider.GetItemMetadataByTemplateName("Text");

			DeleteItem(sectionItem.ID, titleItemName, textMetadata);

			var parameters = sectionItem.Fields.FirstOrDefault(field => field.FieldId == new Guid(SectionConstants.SectionParametersFieldId));
			var showLegend = XmlHelper.GetXmlElementValue(parameters.Value, SectionConstants.SectionShowLegendElementName);
			if (showLegend == null || string.Equals(showLegend, "Yes", StringComparison.InvariantCultureIgnoreCase))
			{
				// Create Text Item with text in Title field using Title Tag HTML element
				var title = sectionItem.Fields.FirstOrDefault(field => field.FieldId == new Guid(SectionConstants.SectionTitleFieldId))?.Value;
				if (!string.IsNullOrEmpty(title))
				{
					// Create Text item
					var fieldValues = _fieldProvider.GetFieldValues(sectionItem, new Guid(SectionConstants.SectionTitleFieldId), titleItemName);

					// Set text field
					textMetadata.fields.newFields.First(field => field.destFieldId == new Guid(TextConstants.TextFieldId)).values = fieldValues;
					// Set Html Tag field
					textMetadata.fields.newFields.First(field => field.destFieldId == new Guid(TextConstants.TextHtmlTagFieldId))
						.value = "h2";
					// Set __Sortorder field
					textMetadata.fields.newFields.First(field => field.destFieldId == new Guid(BaseTemplateConstants.SortOrderFieldId)).value = "-100"; // First item in the section

					WriteNewItem(textMetadata.destTemplateId, sectionItem, titleItemName, textMetadata);
				}
			}
		}

		public void ConvertInformation(SCItem sectionItem)
		{
			var informationItemName = "Information";
			var textMetadata = _metadataProvider.GetItemMetadataByTemplateName("Text");

			DeleteItem(sectionItem.ID, informationItemName, textMetadata);

			// Create Text Item with text in Information parameter
			var localizedParameters = sectionItem.Fields.FirstOrDefault(field => field.FieldId == new Guid(SectionConstants.SectionLocalizedParametersFieldId))?.Value;
			var information = XmlHelper.GetXmlElementValue(localizedParameters, SectionConstants.SectionInformationElementName);
			if (!string.IsNullOrEmpty(information))
			{
				// Create Text item
				var fieldValues = _fieldProvider.GetFieldValues(sectionItem, new Guid(SectionConstants.SectionLocalizedParametersFieldId), string.Empty, false);
				var informationFieldValues = new Dictionary<Tuple<string, int>, string>();
				foreach (var fieldValue in fieldValues)
				{
					informationFieldValues.Add(fieldValue.Key, XmlHelper.GetXmlElementValue(fieldValue.Value, SectionConstants.SectionInformationElementName));
				}

				// Set text field
				textMetadata.fields.newFields.First(field => field.destFieldId == new Guid(TextConstants.TextFieldId)).values = informationFieldValues;
				// Set __Sortorder field
				textMetadata.fields.newFields.First(field => field.destFieldId == new Guid(BaseTemplateConstants.SortOrderFieldId)).value = "-50"; // Second item in the page, after title

				WriteNewItem(textMetadata.destTemplateId, sectionItem, informationItemName, textMetadata);
			}
		}
	}
}