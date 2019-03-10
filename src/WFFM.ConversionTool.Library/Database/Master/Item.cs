namespace WFFM.ConversionTool.Library.Database.Master
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Item
    {
        [Key]
        [Column(Order = 0)]
        public Guid ID { get; set; }

        [Column(Order = 1)]
        [StringLength(256)]
        public string Name { get; set; }

        [Column(Order = 2)]
        public Guid TemplateID { get; set; }

        [Column(Order = 3)]
        public Guid MasterID { get; set; }

        [Column(Order = 4)]
        public Guid ParentID { get; set; }

        [Column(Order = 5)]
        public DateTime Created { get; set; }

        [Column(Order = 6)]
        public DateTime Updated { get; set; }
    }
}
