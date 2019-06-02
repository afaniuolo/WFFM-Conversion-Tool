using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using WFFM.ConversionTool.Library.Constants;
using WFFM.ConversionTool.Library.Models.Metadata;
using WFFM.ConversionTool.Library.Models.Reporting;
using WFFM.ConversionTool.Library.Models.Sitecore;
using WFFM.ConversionTool.Library.Repositories;

namespace WFFM.ConversionTool.Library.Reporting
{
	public class AnalysisReporter : IReporter
	{
		private List<ReportingRecord> _reportingRecords = new List<ReportingRecord>();
	
		private ISourceMasterRepository _sourceMasterRepository;
		private AppSettings _appSettings;

		private List<string> _convertedFieldIds = new List<string>()
		{
			FormConstants.FormTitleFieldId,
			FormConstants.FormFooterFieldId,
			FormConstants.FormIntroductionFieldId,
			FormConstants.FormShowFooterFieldId,
			FormConstants.FormShowTitleFieldId,
			FormConstants.FormShowIntroductionFieldId,
			FormConstants.FormSaveToDatabaseFieldId,
			FormConstants.FormSubmitModeFieldId,
			FormConstants.FormSubmitNameFieldId,
			FormConstants.FormSuccessMessageFieldId,
			FormConstants.FormSuccessPageFieldId,
			FormConstants.FormTitleTagFieldId
		};

		public AnalysisReporter(ISourceMasterRepository sourceMasterRepository, AppSettings appSettings)
		{
			_sourceMasterRepository = sourceMasterRepository;
			_appSettings = appSettings;
		}

		public void AddUnmappedItemField(SCField field, Guid itemId)
		{
			AddReportingRecord(new ReportingRecord()
			{
				ItemId = itemId.ToString("B").ToUpper(),
				ItemName = _sourceMasterRepository.GetSitecoreItemName(itemId),
				ItemPath = _sourceMasterRepository.GetItemPath(itemId),
				ItemVersion = field.Version,
				ItemLanguage = field.Language,
				ItemTemplateId = _sourceMasterRepository.GetItemTemplateId(itemId).ToString("B").ToUpper(),
				ItemTemplateName = _sourceMasterRepository.GetSitecoreItemName(_sourceMasterRepository.GetItemTemplateId(itemId)),
				FieldId = field.FieldId.ToString("B").ToUpper(),
				FieldName = _sourceMasterRepository.GetSitecoreItemName(field.FieldId),
				FieldType = field.Type.ToString(),
				Message = "Source Field Not Mapped"
			});
		}

		public void AddUnmappedFormFieldItem(Guid itemId, string sourceMappingFieldValue)
		{
			AddReportingRecord(new ReportingRecord()
			{
				ItemId = itemId.ToString("B").ToUpper(),
				ItemName = _sourceMasterRepository.GetSitecoreItemName(itemId),
				ItemPath = _sourceMasterRepository.GetItemPath(itemId),
				ItemTemplateId = _sourceMasterRepository.GetItemTemplateId(itemId).ToString("B").ToUpper(),
				ItemTemplateName = _sourceMasterRepository.GetSitecoreItemName(_sourceMasterRepository.GetItemTemplateId(itemId)),
				Message = $"Form Field Item Not Mapped - Form Field Type Name = {_sourceMasterRepository.GetSitecoreItemName(Guid.Parse(sourceMappingFieldValue))}"
			});
		}

		private void AddReportingRecord(ReportingRecord reportingRecord)
		{
			_reportingRecords.Add(reportingRecord);
		}

		public void GenerateOutput()
		{
			// Filter out fields converted in ad-hoc converters
			_reportingRecords = _reportingRecords.Where(r => !_convertedFieldIds.Contains(r.FieldId)).ToList();

			// Filter out base standard fields if analysis_ExcludeBaseStandardFields is set to true
			if (_appSettings.analysis_ExcludeBaseStandardFields)
			{
				_reportingRecords = _reportingRecords.Where(r => string.IsNullOrEmpty(r.FieldName) || !r.FieldName.StartsWith("__")).ToList();
			}

			// Convert to CSV file
			var filePath = $"Analysis\\AnalysisReport.{DateTime.Now.ToString("yyyyMMdd.hhmmss")}.csv";
			using (var writer = new StreamWriter(filePath))
			using (var csv = new CsvWriter(writer))
			{
				csv.WriteRecords(_reportingRecords);
			}

			Console.WriteLine();
			Console.WriteLine("  Conversion analysis report can be reviewed here: " + filePath);
			Console.WriteLine();
		}
	}
}
