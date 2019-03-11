using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFFM.ConversionTool.Library.Models;

namespace WFFM.ConversionTool.Library.Factories
{
	public interface IFieldFactory
	{
		SCField CreateSharedField(Guid fieldId, Guid itemID, string value);
		SCField CreateUnversionedField(Guid fieldId, Guid itemID, string value, string language);
		SCField CreateVersionedField(Guid fieldId, Guid itemID, string value, int version, string language);
	}
}
