using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Core;
using SimpleInjector;
using WFFM.ConversionTool.FormsData.Migrators;
using WFFM.ConversionTool.Library;
using WFFM.ConversionTool.Library.Logging;
using WFFM.ConversionTool.Library.Processors;
using WFFM.ConversionTool.Utility;
using ILogger = WFFM.ConversionTool.Library.Logging.ILogger;

namespace WFFM.ConversionTool.Console
{
	class Program
	{
		static readonly Container container;

		static Program()
		{
			container = IoC.Initialize();
		}

		static void Main(string[] args)
		{
			// Init Console output
			System.Console.WriteLine();
			System.Console.WriteLine(" ***********************************************************************");
			System.Console.WriteLine(" *                                                                     *");
			System.Console.WriteLine(" *                 WFFM Conversion Tool - v1.0.0                       *");
			System.Console.WriteLine(" *                                                                     *");
			System.Console.WriteLine(" ***********************************************************************");
			System.Console.WriteLine();

			// Configure connection strings

			// Read and analyze source data
			var formProcessor = container.GetInstance<FormProcessor>();
			var convertedForms = formProcessor.ConvertForms();

			// Convert & Migrate data
			System.Console.WriteLine("Started forms data migration...");
			System.Console.WriteLine();

			var dataMigrator = container.GetInstance<DataMigrator>();
			int formsCounter = 0;
			foreach (Guid convertedFormId in convertedForms)
			{
				dataMigrator.MigrateData(convertedFormId);
				formsCounter++;
				ProgressBar.DrawTextProgressBar(formsCounter, convertedForms.Count, "forms data migrated");
			}

			System.Console.WriteLine();
			System.Console.WriteLine("Finished forms data migration.");
			System.Console.WriteLine();
		}

		
	}

}
