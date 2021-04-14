using Newtonsoft.Json;
using System;
using System.Text.RegularExpressions;
using WFFM.ConversionTool.Library.Converters;
using WFFM.ConversionTool.Library.Helpers;
using WFFM.ConversionTool.Library.Logging;
using WFFM.ConversionTool.Library.Repositories;

namespace WFFM.ConversionTool.Extensions.SitecoreFormsSendEmailSubmitAction.Converters
{
	public class SendEmailConverter : BaseFieldConverter
	{
		private IDestMasterRepository _destMasterRepository;
		private ILogger _logger;

		public SendEmailConverter(IDestMasterRepository destMasterRepository, ILogger logger)
		{
			_destMasterRepository = destMasterRepository;
			_logger = logger;
		}

		public override string ConvertValue(string sourceValue)
		{
			// example of sourceValue
			// <host>example.host</host><from>example@mail.net</from><isbodyhtml>true</isbodyhtml><to>to@example.com</to><cc>cc@example.com</cc><bcc>bcc@example.com</bcc><localfrom>example@mail.net</localfrom><subject>This is the subject of the email.</subject><mail><p>This is the body of the email.</p><p>[<label id="{CFA55E36-3018-41A4-9F4D-2EA1293D5902}">Single-Line Text</label>]</p></mail>
			var host = XmlHelper.GetXmlElementValue(sourceValue, "host", true);
			var from = XmlHelper.GetXmlElementValue(sourceValue, "from", true);
			var isbodyhtml = XmlHelper.GetXmlElementValue(sourceValue, "isbodyhtml", true);
			var to = XmlHelper.GetXmlElementValue(sourceValue, "to", true);
			var cc = XmlHelper.GetXmlElementValue(sourceValue, "cc", true);
			var bcc = XmlHelper.GetXmlElementValue(sourceValue, "bcc", true);
			var localfrom = XmlHelper.GetXmlElementValue(sourceValue, "localfrom", true);
			var subject = ConvertFieldTokens(XmlHelper.GetXmlElementValue(sourceValue, "subject", true));
			var mail = ConvertFieldTokens(XmlHelper.GetXmlElementValue(sourceValue, "mail", true));

			var fromValue = !string.IsNullOrEmpty(from) ? from : localfrom;

			return JsonConvert.SerializeObject(new { 
				from = fromValue,
				to = to,
				cc = cc,
				bcc = bcc,
				subject = subject,
				message = mail,
				isHtml = isbodyhtml,
				customSmtpConfig = $"<Host>{host}</Host>"
			});
		}

		private string ConvertFieldTokens(string fieldText)
		{
			// Find all tokens
			var matches = Regex.Matches(fieldText, @"\[(.*?)\]", RegexOptions.IgnoreCase);

			foreach (Match match in matches)
			{
				Guid fieldId;
				var fieldName = string.Empty;
				var matchValue = match.Value.Replace("[", "").Replace("]", "");
				if (Guid.TryParse(matchValue, out fieldId)) // case of token in subject field
				{
					// find field name
					fieldName = _destMasterRepository.GetSitecoreItem(fieldId)?.Name;

				}
				else // case of token in message field
				{
					// get field label value
					fieldName = XmlHelper.GetXmlElementValue(matchValue, "label", true);
				}

				if (!string.IsNullOrEmpty(fieldName))
				{
					// replace token with label value
					fieldText = fieldText.Replace(matchValue, fieldName);
				}
			}

			return fieldText;
		}
	}
}
