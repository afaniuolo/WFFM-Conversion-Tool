using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using log4net;
using log4net.Core;
using SimpleInjector;
using WFFM.ConversionTool.Library;
using WFFM.ConversionTool.Library.Database;
using WFFM.ConversionTool.Library.Logging;
using WFFM.ConversionTool.Library.Migrators;
using WFFM.ConversionTool.Library.Processors;
using WFFM.ConversionTool.Library.Validators;
using WFFM.ConversionTool.Library.Visualization;
using ILogger = WFFM.ConversionTool.Library.Logging.ILogger;

namespace WFFM.ConversionTool
{
	class Program
	{
		static readonly Container container;

		private static bool help = false;
		private static bool convert = false;
		private static bool onlydata = false;

		static Program()
		{
			container = IoC.Initialize();
		}

		static void Main(string[] args)
		{
			// Start watch
			var stopwatch = Stopwatch.StartNew();

			// Init Console App Parameters
			InitializeAppParameters(args);

			// Render Help message if needed
			if (help)
			{
				Console.WriteLine();
				Console.WriteLine("Available command parameters:");
				Console.WriteLine();
				Console.WriteLine("/convert                 to convert and migrate items and data in destination databases.");
				Console.WriteLine();
				Console.WriteLine("/convert /onlydata       to convert and migrate only data in destination database.");
				Console.WriteLine();
				return;
			}

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
			if (!metadataValidator.Validate()) return;

			// AppSettings Validation
			var appSettingsValidator = container.GetInstance<AppSettingsValidator>();
			if (!appSettingsValidator.Validate()) return;

			// Connection Strings Testing
			var dbConnectionStringValidator = container.GetInstance<DbConnectionStringValidator>();
			if (!dbConnectionStringValidator.Validate()) return;

			if (!onlydata)
			{
				// Read and analyze source data
				var formProcessor = container.GetInstance<FormProcessor>();
				formProcessor.ConvertForms();
			}

			// Convert & Migrate data
			var dataMigrator = container.GetInstance<DataMigrator>();
			dataMigrator.MigrateData();

			// Stop watch
			System.Console.WriteLine();
			System.Console.WriteLine($"Execution completed in {Math.Round(stopwatch.Elapsed.TotalMinutes, 2)} minutes.");
			System.Console.WriteLine();
		}

		private static void InitializeAppParameters(string[] args)
		{
			for (int i = 0; i < args.Length; i++)
			{
				string arg = args[i];

				switch (arg.ToLower())
				{
					case "/help":
						help = true;
						break;
					case "/convert":
						convert = true;
						break;
					case "/onlydata":
						onlydata = true;
						break;
				}
			}
		}
	}
}
