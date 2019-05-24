using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace WFFM.ConversionTool.Library.Database.MongoDB
{
	public class MongoAnalytics
	{
		private IMongoClient _client;
		private IMongoDatabase _database;
		private IMongoCollection<FormData> _formDataCollection;

		public MongoAnalytics(string connectionString)
		{

			MongoDefaults.GuidRepresentation = GuidRepresentation.CSharpLegacy;
			var _databaseName = MongoUrl.Create(connectionString).DatabaseName;
			var _client = new MongoClient(connectionString).GetDatabase(_databaseName);

			_formDataCollection = _client.GetCollection<FormData>("FormData");
		}

		public List<FormData> GetFormDataByFormItemId(Guid formItemId)
		{
			var result = _formDataCollection.Find(d => d.FormId == formItemId).ToList();
			return result;
		}

		public FormData GetFormDataByFormRecordId(Guid formRecordId)
		{
			var result = _formDataCollection.Find(d => d.Id == formRecordId).FirstOrDefault();
			return result;
		}
	}
}
