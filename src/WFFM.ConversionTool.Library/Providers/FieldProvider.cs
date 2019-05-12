using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFFM.ConversionTool.Library.Helpers;
using WFFM.ConversionTool.Library.Models.Sitecore;

namespace WFFM.ConversionTool.Library.Providers
{
	public class FieldProvider : IFieldProvider
	{
		public Dictionary<Tuple<string, int>, string> GetFieldValues(SCItem sourceItem, Guid sourceFieldId, string defaultValue, bool stripHtml = false)
		{
			var values = new Dictionary<Tuple<string, int>, string>();
			IEnumerable<Tuple<string, int>> langVersions = sourceItem.Fields.Where(f => f.Version != null && f.Language != null).Select(f => new Tuple<string, int>(f.Language, (int)f.Version)).Distinct();
			var languages = sourceItem.Fields.Where(f => f.Language != null).Select(f => f.Language).Distinct();
			foreach (var langVersion in langVersions)
			{
				var value = sourceItem.Fields.FirstOrDefault(f =>
					f.FieldId == sourceFieldId && f.Language == langVersion.Item1 && f.Version == langVersion.Item2)?.Value;
				if (stripHtml)
				{
					value = XmlHelper.StripHtml(value);
				}
				values.Add(langVersion, value ?? defaultValue);
			}

			return values;
		}
	}
}
