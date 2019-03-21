using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WFFM.ConversionTool.Library.Converters;
using WFFM.ConversionTool.Library.Database.Forms;
using WFFM.ConversionTool.Library.Database.Master;
using WFFM.ConversionTool.Library.Database.WFFM;
using WFFM.ConversionTool.Library.Factories;
using WFFM.ConversionTool.Library.Logging;
using WFFM.ConversionTool.Library.Models;
using WFFM.ConversionTool.Library.Models.Metadata;
using WFFM.ConversionTool.Library.Models.Sitecore;
using WFFM.ConversionTool.Library.Readers;
using WFFM.ConversionTool.Library.Repositories;

namespace WFFM.ConversionTool.Library.Processors
{
	public class FormProcessor : ItemProcessor, IFormProcessor
	{
		private ILogger logger;
		private ISourceMasterRepository _sourceMasterRepository;
		private AppSettings _appSettings;

		private readonly string FormTemplateName = "form";
		private readonly string PageTemplateName = "page";

		public FormProcessor(ILogger iLogger, ISourceMasterRepository sourceMasterRepository, AppSettings appSettings, IDestMasterRepository destMasterRepository, IItemConverter itemConverter, IItemFactory itemFactory) 
			: base(destMasterRepository, itemConverter, itemFactory)
		{
			logger = iLogger;
			_sourceMasterRepository = sourceMasterRepository;
			_appSettings = appSettings;
		}

		public void ConvertForms()
		{
			var sourceFormTemplateId =
				_appSettings.metadataFiles.FirstOrDefault(m => m.templateName.ToLower() == FormTemplateName)?.sourceTemplateId;

			if (sourceFormTemplateId == null)
				return;

			var destPageTemplateId = _appSettings.metadataFiles.FirstOrDefault(m => m.templateName.ToLower() == PageTemplateName)
				?.destTemplateId;

			if (destPageTemplateId == null)
				return;

			var forms = _sourceMasterRepository.GetSitecoreItems((Guid)sourceFormTemplateId);
			foreach (var form in forms)
			{
				// Convert and Migrate Form items
				ConvertAndWriteItem(form, _appSettings.itemReferences["destFormFolderId"]);
				// Create Page items for each form
				WriteNewItem((Guid)destPageTemplateId,form);

				// Migrate Data

			}
		}
	}
}
