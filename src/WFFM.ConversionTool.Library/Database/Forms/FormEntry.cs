namespace WFFM.ConversionTool.Library.Database.Forms
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("sitecore_forms_storage.FormEntries")]
    public partial class FormEntry
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public FormEntry()
        {
            FieldDatas = new HashSet<FieldData>();
        }

        public Guid Id { get; set; }

        public Guid FormDefinitionId { get; set; }

        public Guid? ContactId { get; set; }

        public bool IsRedacted { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime Created { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<FieldData> FieldDatas { get; set; }
    }
}
