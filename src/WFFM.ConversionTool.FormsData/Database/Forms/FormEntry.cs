using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace WFFM.ConversionTool.FormsData.Database.Forms
{
	[Table("FormEntry")]
    public partial class FormEntry
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public FormEntry()
        {
            FieldDatas = new HashSet<FieldData>();
        }

        public Guid ID { get; set; }

        public Guid FormItemID { get; set; }

        public DateTime Created { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<FieldData> FieldDatas { get; set; }
    }
}
