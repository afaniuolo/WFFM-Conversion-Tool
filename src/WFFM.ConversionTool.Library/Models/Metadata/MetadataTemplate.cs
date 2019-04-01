using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFFM.ConversionTool.Library.Models.Sitecore;

namespace WFFM.ConversionTool.Library.Models.Metadata
{
	public class MetadataTemplate
	{
		public Guid sourceTemplateId { get; set; }
		public string sourceTemplateName { get; set; }
		public Guid destTemplateId { get; set; }
		public string destTemplateName { get; set; }
		public List<DestTemplateMapping> destTemplateMappings { get; set; }
		public MetadataFields fields { get; set; }
		public Guid sourceMappingFieldId { get; set; }

		public class DestTemplateMapping
		{
			public Guid sourceMappingFieldValue { get; set; }
			public Guid? destTemplateId { get; set; }
		}

		public class MetadataFields
		{
			public List<MetadataExistingField> existingFields { get; set; }
			public List<MetadataNewField> newFields { get; set; }
			public List<MetadataConvertedField> convertedFields { get; set; }

			public class MetadataExistingField
			{
				public Guid fieldId { get; set; }
			}

			public class MetadataNewField
			{
				public FieldType fieldType { get; set; }
				public Guid destFieldId { get; set; }
				public string value { get; set; }
				public string valueType { get; set; }
			}

			public class MetadataConvertedField
			{
				public string fieldConverter { get; set; }
				public Guid sourceFieldId { get; set; }
				public Guid? destFieldId { get; set; }
				public List<ValueXmlElementMapping> destFields { get; set; }

				public class ValueXmlElementMapping
				{
					public string sourceElementName { get; set; }
					public Guid? destFieldId { get; set; }
					public string fieldConverter { get; set; }
				}
			}
		}
	}
}
