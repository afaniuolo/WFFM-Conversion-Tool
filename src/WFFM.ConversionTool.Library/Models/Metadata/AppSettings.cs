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
		public string invalidItemNameChars { get; set; }

		public class Converter
		{
			public string name { get; set; }
			public string converterType { get; set; }
		}
	}
}
