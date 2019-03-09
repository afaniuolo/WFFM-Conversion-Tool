using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WFFM.ConversionTool.Library.Models
{
	public class SCItem
	{
		public Guid ID { get; set; }
		public string Name { get; set; }
		public Guid TemplateID { get; set; }
		public Guid MasterID { get; set; }
		public Guid ParentID { get; set; }
		public DateTime Created { get; set; }
		public DateTime Updated { get; set; }
		public List<SCField> Fields { get; set; }
		
	}
}
