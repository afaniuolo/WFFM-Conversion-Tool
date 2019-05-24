using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WFFM.ConversionTool.Library.Database.MongoDB
{
	public class FormData
	{
		[BsonId]
		public Guid Id { get; set; }
		[BsonElement("FormID")]
		public Guid FormId { get; set; }
		[BsonElement("ContactId")]
		public Guid ContactId { get; set; }
		[BsonElement("InteractionId")]
		public Guid InteractionId { get; set; }
		[BsonElement("Timestamp")]
		public DateTime Timestamp { get; set; }
		[BsonElement("Fields")]
		public List<FieldData> FieldDatas { get; set; }
	}
}
