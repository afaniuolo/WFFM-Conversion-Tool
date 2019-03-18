using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFFM.ConversionTool.Library.Converters;
using WFFM.ConversionTool.Library.Database.Forms;
using WFFM.ConversionTool.Library.Database.Master;
using WFFM.ConversionTool.Library.Database.WFFM;
using WFFM.ConversionTool.Library.Logging;
using WFFM.ConversionTool.Library.Models;
using WFFM.ConversionTool.Library.Models.Metadata;
using WFFM.ConversionTool.Library.Models.Sitecore;
using WFFM.ConversionTool.Library.Repositories;

namespace WFFM.ConversionTool.Library.Processors
{
	public class FormProcessor : ItemProcessor, IFormProcessor
	{
		private ILogger logger;
		private ISourceMasterRepository _sourceMasterRepository;
		private AppSettings _appSettings;

		public FormProcessor(ILogger iLogger, ISourceMasterRepository sourceMasterRepository, AppSettings appSettings, IDestMasterRepository destMasterRepository, IItemConverter itemConverter) : base(destMasterRepository, itemConverter)
		{
			logger = iLogger;
			_sourceMasterRepository = sourceMasterRepository;
			_appSettings = appSettings;
		}

		public void ConvertForms()
		{
			var sourceFormTemplateId =
				_appSettings.metadataFiles.FirstOrDefault(m => m.templateName.ToLower() == "form")?.sourceTemplateId;

			if (sourceFormTemplateId == null)
				return;

			var forms = _sourceMasterRepository.GetSitecoreItems((Guid)sourceFormTemplateId);
			foreach (var form in forms)
			{
				// Convert and Migrate items
				ConvertAndWriteItem(form, _appSettings.itemReferences["destFormFolderId"]);

				// Migrate Data

			}
		}
	}
}
