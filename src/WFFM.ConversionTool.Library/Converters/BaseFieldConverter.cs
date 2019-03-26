using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFFM.ConversionTool.Library.Models.Sitecore;

namespace WFFM.ConversionTool.Library.Converters
{
	public abstract class BaseFieldConverter : IFieldConverter
	{
		public virtual SCField Convert(SCField scField, Guid destFieldId)
		{
			return ConvertField(scField, destFieldId, scField.Value);
		}

		public virtual SCField ConvertField(SCField scField, Guid destFieldId, string convertedValue)
		{
			return new SCField()
			{
				Created = DateTime.Now,
				Updated = DateTime.Now,
				ItemId = scField.ItemId,
				Language = scField.Language,
				Version = scField.Version,
				Type = scField.Type,
				Value = convertedValue,
				FieldId = destFieldId,
				Id = scField.Id
			};
		}
	}
}
