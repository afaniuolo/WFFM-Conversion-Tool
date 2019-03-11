using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFFM.ConversionTool.Library.Factories;
using WFFM.ConversionTool.Library.Models;

namespace WFFM.ConversionTool.Library.Converters
{
	public class FormConverter : IItemConverter
	{
		private IFieldFactory _fieldFactory;

		private readonly Guid DestFormsFolderID = new Guid("B701850A-CB8A-4943-B2BC-DDDB1238C103");
		private readonly Guid SitecoreFormsTemplateId = new Guid("6ABEE1F2-4AB4-47F0-AD8B-BDB36F37F64C");

		public FormConverter(IFieldFactory fieldFactory)
		{
			_fieldFactory = fieldFactory;
		}

		public SCItem Convert(SCItem scItem)
		{
			return ConvertFormItemAndFields(scItem);
		}

		private SCItem ConvertFormItemAndFields(SCItem sourceFormItem)
		{
			return new SCItem()
			{
				ID = sourceFormItem.ID,
				Name = sourceFormItem.Name,
				MasterID = Guid.Empty,
				ParentID = DestFormsFolderID,
				Created = sourceFormItem.Created,
				Updated = sourceFormItem.Updated,
				TemplateID = SitecoreFormsTemplateId,
				Fields = ConvertFields(sourceFormItem.Fields)
			};
		}

		private List<SCField> ConvertFields(List<SCField> fields)
		{
			var destFields = new List<SCField>();

			var itemId = fields.First().ItemId;

			// Map source fields to dest fields
			foreach (SCField field in fields)
			{
				SCField destField = new SCField();
				switch (field.FieldId.ToString().ToUpper())
				{
					case "B0A67B2A-8B07-4E0B-8809-69F751709806":
						var converter = ConverterInstantiator.CreateInstance("WFFM.ConversionTool.Library.Converters.FieldConverters.BooleanExistenceConverter, WFFM.ConversionTool.Library");
						destField = converter.Convert(field);
						break;
					case "5DD74568-4D4B-44C1-B513-0AF5F4CDA34F":
					case "BADD9CF9-53E0-4D0C-BCC0-2D784C282F6A":
					case "8CDC337E-A112-42FB-BBB4-4143751E123F":
					case "BA3F86A2-4A1C-4D78-B63D-91C2779C1B5E":
					case "25BED78C-4957-4165-998A-CA1B52F67497":
					case "52807595-0F8F-4B20-8D2A-CB71D28C6103":
					case "D9CF14B1-FA16-4BA6-9288-E8A174D4D522":
					case "B5E02AD9-D56F-4C41-A065-A133DB87BDEB":
						destField = field;
						break;
				}

				if (destField.FieldId != Guid.Empty)
				{
					destFields.Add(destField);
				}
			}

			// Add new fields
			destFields.Add(_fieldFactory.CreateSharedField(new Guid("589A7ADE-81D4-4A60-B9E2-7EAF6AE8A563"), itemId, "{3A4DF9C0-7C82-4415-90C3-25440257756D}")); // Field Type
			destFields.Add(_fieldFactory.CreateSharedField(new Guid("9EB86F19-2E5D-48DB-9795-0EE4868EFF11"), itemId, "1")); // Is Ajax
			destFields.Add(_fieldFactory.CreateSharedField(new Guid("FCE03B41-4CBB-4F7F-920F-FF04F12FA896"), itemId, @"jquery-2.1.3.min.js|jquery.validate.min.js|jquery.validate.unobtrusive.min.js|jquery.unobtrusive-ajax.min.js|form.validate.js|form.tracking.js|form.conditions.js")); // Scripts

			return destFields;
		}
	}
}
