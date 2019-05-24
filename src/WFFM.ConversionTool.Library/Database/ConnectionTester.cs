using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using WFFM.ConversionTool.Library.Database.MongoDB;

namespace WFFM.ConversionTool.Library.Database
{
	public static class ConnectionTester
	{
		public static bool IsServerConnected(string connectionString)
		{
			if (connectionString.ToLower().StartsWith("mongodb"))
			{
				try
				{
					var databaseName = MongoUrl.Create(connectionString).DatabaseName;
					var client = new MongoClient(connectionString).GetDatabase(databaseName);
					var collection = client.GetCollection<FormData>("FormData").CountDocuments(new BsonDocument());
					return true;
				}
				catch (Exception ex)
				{
					return false;
				}
			}
			else
			{
				using (SqlConnection connection = new SqlConnection(connectionString))
				{
					try
					{
						connection.Open();
						return true;
					}
					catch (SqlException)
					{
						return false;
					}
				}
			}
		}
	}
}
