using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFFM.ConversionTool.Library.Models.Sitecore;

namespace WFFM.ConversionTool.Library.Models.Metadata
{
	[Serializable]
	public class MetadataTemplate
	{
		public Guid sourceTemplateId { get; set; }
		public string sourceTemplateName { get; set; }
		public Guid destTemplateId { get; set; }
		public string destTemplateName { get; set; }
		public string baseTemplateMetadataFileName { get; set; }
		public MetadataFields fields { get; set; }
		public Guid sourceMappingFieldId { get; set; }
		public string sourceMappingFieldValue { get; set; }
		public List<DescendantItem> descendantItems { get; set; }

		[Serializable]
		public class DescendantItem
		{
			public string itemName { get; set; }
			public string destTemplateName { get; set; }
			public bool isParentChild { get; set; }
			public string parentItemName { get; set; }
		}

		[Serializable]
		public class MetadataFields
		{
			public List<MetadataExistingField> existingFields { get; set; }
			public List<MetadataNewField> newFields { get; set; }
			public List<MetadataConvertedField> convertedFields { get; set; }

			[Serializable]
			public class MetadataExistingField
			{
				public Guid fieldId { get; set; }
			}

			[Serializable]
			public class MetadataNewField
			{
				public FieldType fieldType { get; set; }
				public Guid destFieldId { get; set; }
				public string valueType { get; set; }
				public string value { get; set; }
				public Dictionary<Tuple<string, int>, string> values { get; set; }
			}

			[Serializable]
			public class MetadataConvertedField
			{
				public string fieldConverter { get; set; }
				public Guid sourceFieldId { get; set; }
				public Guid? destFieldId { get; set; }
				public List<ValueXmlElementMapping> destFields { get; set; }

				[Serializable]
				public class ValueXmlElementMapping
				{
					public string sourceElementName { get; set; }
					public Guid? destFieldId { get; set; }
					public string fieldConverter { get; set; }
					public List<ValueXmlElementMapping> destFields { get; set; }
				}
			}
		}
	}
}
