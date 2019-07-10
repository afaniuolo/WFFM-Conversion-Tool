using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFFM.ConversionTool.Library.Models.Metadata;
using WFFM.ConversionTool.Library.Models.Reporting;
using WFFM.ConversionTool.Library.Models.Sitecore;

namespace WFFM.ConversionTool.Library.Reporting
{
	public interface IReporter
	{
		void AddUnmappedItemField(SCField field, Guid itemId);
		void AddUnmappedFormFieldItem(Guid itemId, string sourceMappingFieldValue);
		void AddUnmappedSaveAction(SCField field, Guid itemId, Guid saveActionId);

		void AddUnmappedValueElementSourceField(SCField field, Guid itemId, string sourceFieldValueElementName, string sourceFieldValueElementValue);
		void GenerateOutput();
	}
}
