using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WFFM.ConversionTool.Library.Models.Metadata
{
	public class AppSettings
	{
		public Dictionary<string, Guid> itemReferences { get; set; }
		public List<Converter> converters { get; set; }
		public string metadataFolderRelativePath { get; set; }
		public List<MetadataFile> metadataFiles { get; set; }
		public List<InputTypeMapping> inputTypesMapping { get; set; }

		public class Converter
		{
			public string name { get; set; }
			public string converterType { get; set; }
		}

		public class MetadataFile
		{
			public Guid sourceTemplateId { get; set; }
			public Guid destTemplateId { get; set; }
			public string templateName { get; set; }
			public string metadataFileName { get; set; }
		}

		public class InputTypeMapping
		{
			public Guid sourceFieldLinkId { get; set; }
			public Guid? destFieldTypeId { get; set; }
		}
	}
}
