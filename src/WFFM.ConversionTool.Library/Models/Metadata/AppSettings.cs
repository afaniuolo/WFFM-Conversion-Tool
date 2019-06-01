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
		public List<SubmitAction> submitActions { get; set; }

		public string metadataFolderRelativePath { get; set; }
		public string invalidItemNameChars { get; set; }

		public bool enableReferencedItemCheck { get; set; }
		public string formsDataProvider { get; set; }
		public bool analysis_ExcludeBaseStandardFields { get; set; }

		public class Converter
		{
			public string name { get; set; }
			public string converterType { get; set; }
		}

		public class SubmitAction
		{
			public Guid sourceSaveActionId { get; set; }
			public string destSubmitActionItemName { get; set; }
			public string destSubmitActionFieldValue { get; set; }
			public string destParametersConverterType { get; set; }
		}
	}
}
