using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFFM.ConversionTool.Library.Database;
using WFFM.ConversionTool.Library.Models.Metadata;

namespace WFFM.ConversionTool.Library.Validators
{
	public class DbConnectionStringValidator : IValidator
	{
		private AppSettings _appSettings;

		public DbConnectionStringValidator(AppSettings appSettings)
		{
			_appSettings = appSettings;
		}

		public bool Validate()
		{
			bool isSqlFormsDataProvider = string.Equals(_appSettings.formsDataProvider, "sqlFormsDataProvider", StringComparison.InvariantCultureIgnoreCase);

			return ValidateConnectionString("SitecoreForms") && ValidateConnectionString("SourceMasterDb") && ValidateConnectionString("DestMasterDb") &&
				(isSqlFormsDataProvider ? ValidateConnectionString("WFFM") : ValidateConnectionString("mongodb_analytics"));
		}

		private bool ValidateConnectionString(string connectionStringName)
		{
			var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;

			var connectionIsValid = string.IsNullOrEmpty(connectionString) || ConnectionTester.IsServerConnected(connectionString);
			if (!connectionIsValid)
			{
				Console.WriteLine();
				Console.WriteLine("Execution aborted!");
				Console.WriteLine($"The {connectionStringName} connection string is not valid.");
				Console.WriteLine();
			}

			return connectionIsValid;
		}
	}
}
