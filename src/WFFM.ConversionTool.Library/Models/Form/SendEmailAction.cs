using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WFFM.ConversionTool.Library.Models.Form
{
	public class SendEmailAction
	{
		public string from { get; set; }
		public string to { get; set; }
		public string cc { get; set; }
		public string bcc { get; set; }
		public string subject { get; set; }
		public string body { get; set; }
	}
}
