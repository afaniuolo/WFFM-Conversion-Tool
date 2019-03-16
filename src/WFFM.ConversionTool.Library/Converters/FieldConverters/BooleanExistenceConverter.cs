using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFFM.ConversionTool.Library.Models;
using WFFM.ConversionTool.Library.Models.Sitecore;

namespace WFFM.ConversionTool.Library.Converters.FieldConverters
{
	public class BooleanExistenceConverter : IFieldConverter
	{
		public SCField Convert(SCField scField)
		{
			return new SCField()
			{
				Created = DateTime.Now,
				Updated = DateTime.Now,
				ItemId = scField.ItemId,
				Language = scField.Language,
				Version = scField.Version,
				Type = scField.Type,
				Value = scField.Value != string.Empty ? "1" : "0",
				FieldId = scField.FieldId,
				Id = Guid.NewGuid()
			};
		}
	}
}
