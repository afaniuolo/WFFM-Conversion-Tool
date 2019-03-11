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
using WFFM.ConversionTool.Library.Repositories;

namespace WFFM.ConversionTool.Library.Processors
{
	public class FormProcessor : IFormProcessor
	{
		private ILogger logger;
		private IDestMasterRepository _masterRepository;
		private ISourceMasterRepository _sourceMasterRepository;
		private IItemConverter _formConverter;

		public FormProcessor(ILogger iLogger, IDestMasterRepository masterRepository, IItemConverter formConverter, ISourceMasterRepository sourceMasterRepository)
		{
			logger = iLogger;
			_masterRepository = masterRepository;
			_sourceMasterRepository = sourceMasterRepository;
			_formConverter = formConverter;
		}

		public void ConvertForms()
		{
			var forms = _sourceMasterRepository.GetForms();
			foreach (var form in forms)
			{
				// Convert and Migrate items
				ConvertAndWriteForm(form);

				// Migrate Data
			}
		}
		
		private void ConvertAndWriteForm(SCItem formItem)
		{
			logger.Log(new LogEntry(LoggingEventType.Debug, string.Format("FormID={0}", formItem.ID)));

			// Convert
			var destFormItem = _formConverter.Convert(formItem);

			// Write to dest
			WriteSitecoreForm(destFormItem);
		}

		

		private void WriteSitecoreForm(SCItem destFormItem)
		{
			_masterRepository.AddOrUpdateForm(destFormItem);

			// Create fields
			foreach (SCField scField in destFormItem.Fields)
			{
				switch (scField.Type)
				{
					case FieldType.Shared:
						_masterRepository.AddOrUpdateSharedField(scField);
						break;
					case FieldType.Unversioned:
						_masterRepository.AddOrUpdateUnversionedField(scField);
						break;
					case FieldType.Versioned:
						_masterRepository.AddOrUpdateVersionedField(scField);
						break;
				}
			}
		}
	}
}
