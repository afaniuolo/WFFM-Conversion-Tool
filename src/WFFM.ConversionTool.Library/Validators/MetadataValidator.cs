using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using WFFM.ConversionTool.Library.Models.Metadata;
using WFFM.ConversionTool.Library.Providers;

namespace WFFM.ConversionTool.Library.Validators
{
	public class MetadataValidator : IValidator
	{
		private IMetadataProvider _metadataProvider;

		public MetadataValidator(IMetadataProvider metadataProvider)
		{
			_metadataProvider = metadataProvider;
		}

		public bool Validate()
		{
			bool isValid = true;

			// Load Schema
			JsonSchema metadataSchema = LoadMetadataSchema();

			// Load all metadata files
			var metadataFiles = _metadataProvider.GetAllMetadataFiles();

			// Validate each file
			foreach (string metadataFile in metadataFiles)
			{
				// Read json file
				var metadataFileContent = System.IO.File.ReadAllText(metadataFile);
				var metadataFileJson = JObject.Parse(metadataFileContent);

				// Break with message in Console if a json file is not valid
				if (!metadataFileJson.IsValid(metadataSchema))
				{
					Console.WriteLine();
					Console.WriteLine("Execution aborted!");
					Console.WriteLine(string.Format("The following metadata file doesn't contain a valid JSON object: {0}", metadataFile));
					Console.WriteLine();
					return false;
				}
			}

			return isValid;
		}

		private JsonSchema LoadMetadataSchema()
		{
			var metadataSchemaFile = File.ReadAllText(@"Schemas\metadata-schema.json");
			return JsonSchema.Parse(metadataSchemaFile);
		}
	}
}
