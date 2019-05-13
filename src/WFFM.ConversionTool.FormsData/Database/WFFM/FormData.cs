using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace WFFM.ConversionTool.FormsData.Database.WFFM
{
	[Table("FormData")]
    public partial class FormData
    {
        public Guid Id { get; set; }

        public Guid FormItemId { get; set; }

        public Guid ContactId { get; set; }

        public Guid InteractionId { get; set; }

        public DateTime TimeStamp { get; set; }

        public string Data { get; set; }
    }
}
