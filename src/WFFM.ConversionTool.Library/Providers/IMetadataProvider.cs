using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Server;
using WFFM.ConversionTool.Library.Models.Metadata;

namespace WFFM.ConversionTool.Library.Providers
{
	public interface IMetadataProvider
	{
		MetadataTemplate GetItemMetadataByTemplateId(Guid templateId);
		MetadataTemplate GetItemMetadataByTemplateName(string templateName);
		MetadataTemplate GetItemMetadataBySourceMappingFieldValue(string mappingValue);
	}
}
