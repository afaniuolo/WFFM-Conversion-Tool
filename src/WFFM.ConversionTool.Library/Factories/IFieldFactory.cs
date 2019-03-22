using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFFM.ConversionTool.Library.Models;
using WFFM.ConversionTool.Library.Models.Metadata;
using WFFM.ConversionTool.Library.Models.Sitecore;

namespace WFFM.ConversionTool.Library.Factories
{
	public interface IFieldFactory
	{
		List<SCField> CreateFields(MetadataTemplate.MetadataFields.MetadataNewField metadataNewField, Guid itemId, IEnumerable<Tuple<string, int>> langVersions, IEnumerable<string> languages);
	}
}
