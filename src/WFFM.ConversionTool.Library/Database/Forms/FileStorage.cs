namespace WFFM.ConversionTool.Library.Database.Forms
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("sitecore_forms_filestorage.FileStorage")]
    public partial class FileStorage
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(256)]
        public string FileName { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime Created { get; set; }

        public bool Committed { get; set; }

        [Required]
        public byte[] FileContent { get; set; }
    }
}
