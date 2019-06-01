using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver.Core.WireProtocol.Messages;
using WFFM.ConversionTool.Library.Models.Sitecore;

namespace WFFM.ConversionTool.Library.Models.Reporting
{
	public class ReportingRecord
	{
		public string ItemId { get; set; }
		public string ItemName { get; set; }
		public string ItemPath { get; set; }
		public int? ItemVersion { get; set; }
		public string ItemLanguage { get; set; }
		public string ItemTemplateId { get; set; }
		public string ItemTemplateName { get; set; }
		public string FieldId { get; set; }
		public string FieldName { get; set; }
		public FieldType FieldType { get; set; }
		public string ElementName { get; set; }
		public string ReferencedItemId { get; set; }
		public string ReferencedItemName { get; set; }
		public string Message { get; set; }
	}
}
