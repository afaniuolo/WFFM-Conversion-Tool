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
			//var formProcessor = container.GetInstance<FormProcessor>();
			//var convertedForms = formProcessor.ConvertForms();

			// Convert & Migrate data
			var dataMigrator = container.GetInstance<DataMigrator>();
			dataMigrator.MigrateData();
		}

		
	}

}
