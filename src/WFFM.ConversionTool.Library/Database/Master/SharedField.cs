namespace WFFM.ConversionTool.Library.Database.Master
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class SharedField
    {
        [Key]
        [Column(Order = 0)]
        public Guid Id { get; set; }

        [Key]
        [Column(Order = 1)]
        public Guid ItemId { get; set; }

        [Key]
        [Column(Order = 2)]
        public Guid FieldId { get; set; }

        [Key]
        [Column(Order = 3)]
        public string Value { get; set; }

        [Key]
        [Column(Order = 4)]
        public DateTime Created { get; set; }

        [Key]
        [Column(Order = 5)]
        public DateTime Updated { get; set; }
    }
}
