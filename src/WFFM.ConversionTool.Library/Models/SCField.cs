using System;

namespace WFFM.ConversionTool.Library.Models
{
	public class SCField
	{
		public string Value { get; set; }
		public Guid Id { get; set; }
		public Guid ItemId { get; set; }
		public DateTime Created { get; set; }
		public DateTime Updated { get; set; }
		public FieldType Type { get; set; }
		public int? Version { get; set; }
		public string Language { get; set; }
	}

	public enum FieldType
	{
		Shared,
		Versioned,
		Unversioned
	}
}