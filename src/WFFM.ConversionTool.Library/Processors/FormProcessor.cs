using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFFM.ConversionTool.Library.Database.Forms;
using WFFM.ConversionTool.Library.Database.Master;
using WFFM.ConversionTool.Library.Database.WFFM;
using WFFM.ConversionTool.Library.Logging;

namespace WFFM.ConversionTool.Library.Processors
{
	public class FormProcessor : IFormProcessor
	{
		private ILogger logger;
		private Database.WFFM.WFFM _wffmContext;
		private SitecoreForms _sitecoreFormsContext;
		private SourceMasterDb _sourceMasterDb;
		private DestMasterDb _destMasterDb;

		public FormProcessor(ILogger iLogger, Database.WFFM.WFFM wffmContext, SitecoreForms sitecoreFormsContext, SourceMasterDb sourceMasterDb, DestMasterDb destMasterDb)
		{
			logger = iLogger;
			_wffmContext = wffmContext;
			_sitecoreFormsContext = sitecoreFormsContext;
			_sourceMasterDb = sourceMasterDb;
			_destMasterDb = destMasterDb;
		}

		public void ConvertForms()
		{
			var forms = _sourceMasterDb.Items.Where(item => item.TemplateID.ToString() == "FFB1DA32-2764-47DB-83B0-95B843546A7E");
			List<FormEntry> sitecoreFormsEntries = new List<FormEntry>();
			foreach (var form in forms)
			{
				logger.Log(new LogEntry(LoggingEventType.Debug, string.Format("FormID={0}", form.ID)));
				// Get all forms field

				var formFields = _sourceMasterDb.SharedFields.Where(field => field.ItemId == form.ID).Select(field => new Field(){ID = field.FieldId, Value = field.Value})
					.Union(_sourceMasterDb.UnversionedFields.Where(field => field.ItemId == form.ID).Select(field => new Field() { ID = field.FieldId, Value = field.Value }))
					.Union(_sourceMasterDb.VersionedFields.Where(field => field.ItemId == form.ID).Select(field => new Field() { ID = field.FieldId, Value = field.Value }));

				logger.Log(new LogEntry(LoggingEventType.Debug, string.Format("FormID={0} - NumberOfFields={1}", form.ID, formFields.Count())));
				// Map
			}
		}

		public class Field
		{
			public string Value { get; set; }
			public Guid ID { get; set; }
		}
	}
}
