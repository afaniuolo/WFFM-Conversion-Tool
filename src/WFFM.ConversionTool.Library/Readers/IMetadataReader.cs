using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFFM.ConversionTool.Library.Models.Metadata;

namespace WFFM.ConversionTool.Library.Readers
{
	public interface IMetadataReader
	{
		MetadataTemplate GetItemMetadata(Guid templateId);
	}
}
