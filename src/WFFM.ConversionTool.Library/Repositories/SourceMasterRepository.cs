using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFFM.ConversionTool.Library.Database.Master;
using WFFM.ConversionTool.Library.Models;

namespace WFFM.ConversionTool.Library.Repositories
{
	public class SourceMasterRepository : ISourceMasterRepository
	{
		private SourceMasterDb _sourceMasterDb;

		private readonly Guid WFFMFormTemplateId = new Guid("FFB1DA32-2764-47DB-83B0-95B843546A7E");

		public SourceMasterRepository(SourceMasterDb sourceMasterDb)
		{
			_sourceMasterDb = sourceMasterDb;
		}

		public List<SCItem> GetForms()
		{
			var forms = GetWFFMForms();
			List<SCItem> scItems = new List<SCItem>();
			foreach (var form in forms)
			{
				scItems.Add(GetSourceFormItemAndFields(form));
			}

			return scItems;
		}

		private SCItem GetSourceFormItemAndFields(Item formItem)
		{
			return new SCItem()
			{
				ID = formItem.ID,
				Name = formItem.Name,
				MasterID = formItem.MasterID,
				ParentID = formItem.ParentID,
				TemplateID = formItem.TemplateID,
				Created = formItem.Created,
				Updated = formItem.Updated,
				Fields = GetWFFMFormFields(formItem.ID),
			};
		}

		/// <summary>
		/// Get the list of existing WFFM forms in source master database
		/// </summary>
		/// <returns></returns>
		private List<Item> GetWFFMForms()
		{
			return _sourceMasterDb.Items.Where(item => item.TemplateID == WFFMFormTemplateId && item.Name != "__Standard Values").ToList();
		}

		/// <summary>
		/// Get list of fields of a WFFM form from the source master database
		/// </summary>
		private List<SCField> GetWFFMFormFields(Guid formItemId)
		{
			// fields from form template
			var formFields = _sourceMasterDb.SharedFields.Where(field => field.ItemId == formItemId)
				.Select(field => new SCField() { Id = field.Id, Value = field.Value, Created = field.Created, Updated = field.Updated, ItemId = field.ItemId, Type = FieldType.Shared, Language = null, Version = null, FieldId = field.FieldId })
				.Union(_sourceMasterDb.UnversionedFields.Where(field => field.ItemId == formItemId)
					.Select(field => new SCField() { Id = field.Id, Value = field.Value, Created = field.Created, Updated = field.Updated, ItemId = field.ItemId, Type = FieldType.Unversioned, Language = field.Language, Version = null, FieldId = field.FieldId }))
				.Union(_sourceMasterDb.VersionedFields.Where(field => field.ItemId == formItemId)
					.Select(field => new SCField() { Id = field.Id, Value = field.Value, Created = field.Created, Updated = field.Updated, ItemId = field.ItemId, Type = FieldType.Versioned, Language = field.Language, Version = field.Version, FieldId = field.FieldId }));

			return formFields.ToList();
		}
	}
}
