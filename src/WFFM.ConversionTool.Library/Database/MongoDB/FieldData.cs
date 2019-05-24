using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;

namespace WFFM.ConversionTool.Library.Database.MongoDB
{
	public class FieldData
	{
		[BsonElement("Data")]
		public string Data { get; set; }
		[BsonElement("FieldId")]
		public Guid FieldId { get; set; }
		[BsonElement("FieldName")]
		public string FieldName { get; set; }
		[BsonElement("Value")]
		public string Value { get; set; }
	}
}
