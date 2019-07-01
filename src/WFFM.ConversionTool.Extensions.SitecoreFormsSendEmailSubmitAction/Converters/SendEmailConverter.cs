using WFFM.ConversionTool.Library.Converters;
using WFFM.ConversionTool.Library.Helpers;

namespace WFFM.ConversionTool.Extensions.SitecoreFormsSendEmailSubmitAction.Converters
{
	public class SendEmailConverter : BaseFieldConverter
	{
		public override string ConvertValue(string sourceValue)
		{
			// example of sourceValue
			// <host>example.host</host><from>example@mail.net</from><isbodyhtml>true</isbodyhtml><to>to@example.com</to><cc>cc@example.com</cc><bcc>bcc@example.com</bcc><localfrom>example@mail.net</localfrom><subject>This is the subject of the email.</subject><mail><p>This is the body of the email.</p><p>[<label id="{CFA55E36-3018-41A4-9F4D-2EA1293D5902}">Single-Line Text</label>]</p></mail>
			var host = XmlHelper.GetXmlElementValue(sourceValue, "host");
			var from = XmlHelper.GetXmlElementValue(sourceValue, "from");
			var isbodyhtml = XmlHelper.GetXmlElementValue(sourceValue, "isbodyhtml");
			var to = XmlHelper.GetXmlElementValue(sourceValue, "to");
			var cc = XmlHelper.GetXmlElementValue(sourceValue, "cc");
			var bcc = XmlHelper.GetXmlElementValue(sourceValue, "bcc");
			var localfrom = XmlHelper.GetXmlElementValue(sourceValue, "localfrom");
			var subject = XmlHelper.GetXmlElementValue(sourceValue, "subject");
			var mail = XmlHelper.GetXmlElementValue(sourceValue, "mail");

			var formValue = !string.IsNullOrEmpty(from) ? from : localfrom;

			return $"{{\"from\":\"{formValue}\",\"to\":\"{to}\",\"cc\":\"{cc}\",\"bcc\":\"{bcc}\",\"subject\":\"{subject}\",\"message\":\"{mail}\",\"isHtml\":{isbodyhtml},\"customSmtpConfig\":\"<Host>{host}</Host>\"}}";
		}
	}
}
