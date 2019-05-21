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
using WFFM.ConversionTool.Library;
using WFFM.ConversionTool.Library.Logging;
using WFFM.ConversionTool.Library.Migrators;
using WFFM.ConversionTool.Library.Processors;
using WFFM.ConversionTool.Library.Validators;
using WFFM.ConversionTool.Library.Visualization;
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
			// Start watch
			var stopwatch = Stopwatch.StartNew();
			
			// Init Console output
			System.Console.WriteLine();
			System.Console.WriteLine(" ***********************************************************************");
			System.Console.WriteLine(" *                                                                     *");
			System.Console.WriteLine(" *                 WFFM Conversion Tool - v1.0.0                       *");
			System.Console.WriteLine(" *                                                                     *");
			System.Console.WriteLine(" ***********************************************************************");
			System.Console.WriteLine();

			// Metadata Validation
			var metadataValidator = container.GetInstance<MetadataValidator>();
			if (!metadataValidator.Validate())
			{
				System.Console.ReadLine();
				return;
			}

			// AppSettings Validation
			var appSettingsValidator = container.GetInstance<AppSettingsValidator>();
			if (!appSettingsValidator.Validate())
			{
				System.Console.ReadLine();
				return;
			}

			// Connection Strings

			// Read and analyze source data
			var formProcessor = container.GetInstance<FormProcessor>();
			formProcessor.ConvertForms();

			// Convert & Migrate data
			var dataMigrator = container.GetInstance<DataMigrator>();
			dataMigrator.MigrateData();

			// Stop watch
			System.Console.WriteLine();
			System.Console.WriteLine($"Execution completed in {Math.Round(stopwatch.Elapsed.TotalMinutes, 2)} minutes.");
			System.Console.WriteLine();
		}

		
	}

}
