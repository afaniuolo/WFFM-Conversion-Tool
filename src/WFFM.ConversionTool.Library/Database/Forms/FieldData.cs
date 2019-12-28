namespace WFFM.ConversionTool.Library.Database.Forms
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("sitecore_forms_storage.FieldData")]
    public partial class FieldData
    {
        public Guid Id { get; set; }

        public Guid FormEntryId { get; set; }

        public Guid FieldDefinitionId { get; set; }

        [Required]
        [StringLength(256)]
        public string FieldName { get; set; }

        public string Value { get; set; }

        [StringLength(256)]
        public string ValueType { get; set; }

        public virtual FormEntry FormEntry { get; set; }
    }
}
