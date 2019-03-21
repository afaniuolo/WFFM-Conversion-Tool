using System;
using System.Linq;
using Newtonsoft.Json;
using WFFM.ConversionTool.Library.Models.Metadata;

namespace WFFM.ConversionTool.Library.Readers
{
	public class MetadataReader : IMetadataReader
	{
		private AppSettings _appSettings;

		public MetadataReader(AppSettings appSettings)
		{
			_appSettings = appSettings;
		}

		public MetadataTemplate GetItemMetadata(Guid templateId)
		{
			// Read json file
			var filePath = string.Format("{0}/{1}", _appSettings.metadataFolderRelativePath, _appSettings.metadataFiles.FirstOrDefault(m => m.sourceTemplateId == templateId || m.destTemplateId == templateId)?.metadataFileName);
			var itemMeta = System.IO.File.ReadAllText(filePath);
			// Deserialize Json to Object
			return JsonConvert.DeserializeObject<MetadataTemplate>(itemMeta);
		}
	}
}
