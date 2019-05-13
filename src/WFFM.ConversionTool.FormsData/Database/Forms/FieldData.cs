using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WFFM.ConversionTool.FormsData.Database.Forms
{
	[Table("FieldData")]
    public partial class FieldData
    {
        public Guid ID { get; set; }

        public Guid FormEntryID { get; set; }

        public Guid FieldItemID { get; set; }

        [Required]
        [StringLength(256)]
        public string FieldName { get; set; }

        public string Value { get; set; }

        [StringLength(256)]
        public string ValueType { get; set; }

        public virtual FormEntry FormEntry { get; set; }
    }
}
