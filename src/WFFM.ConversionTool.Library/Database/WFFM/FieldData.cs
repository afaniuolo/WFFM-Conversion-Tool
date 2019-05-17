using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace WFFM.ConversionTool.Library.Database.WFFM
{
	[Table("FieldData")]
    public partial class FieldData
    {
        public Guid Id { get; set; }

        public Guid FormId { get; set; }

        public Guid FieldItemId { get; set; }

        public string FieldName { get; set; }

        public string Value { get; set; }

        public string Data { get; set; }
    }
}
