using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFFM.ConversionTool.Library.Database;

namespace WFFM.ConversionTool.Library.Validators
{
	public class DbConnectionStringValidator : IValidator
	{
		public bool Validate()
		{
			return ValidateConnectionString("WFFM") && ValidateConnectionString("SitecoreForms") &&
			       ValidateConnectionString("SourceMasterDb") && ValidateConnectionString("DestMasterDb");
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
