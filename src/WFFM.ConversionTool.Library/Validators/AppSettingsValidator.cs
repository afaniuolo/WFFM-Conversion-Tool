using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace WFFM.ConversionTool.Library.Validators
{
	public class AppSettingsValidator : IValidator
	{
		private readonly string AppSettingsFilePath = "AppSettings.json";

		public bool Validate()
		{
			bool isValid = true;

			// Load Schema
			JsonSchema appSettingsSchema = LoadAppSettingsSchema();

			// Validate
			// Read json file
			var appSettingsFileContent = System.IO.File.ReadAllText(AppSettingsFilePath);
			var appSettingsFileJson = JObject.Parse(appSettingsFileContent);

			// Break with message in Console if a json file is not valid
			if (!appSettingsFileJson.IsValid(appSettingsSchema))
			{
				Console.WriteLine();
				Console.WriteLine("Execution aborted!");
				Console.WriteLine(string.Format("The following file doesn't contain a valid JSON object: {0}", AppSettingsFilePath));
				Console.WriteLine();
				return false;
			}

			return isValid;
		}

		private JsonSchema LoadAppSettingsSchema()
		{
			var appSettingsSchemaFile = File.ReadAllText(@"Schemas\appsettings-schema.json");
			return JsonSchema.Parse(appSettingsSchemaFile);
		}
	}
}
