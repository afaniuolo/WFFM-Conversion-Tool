using System.Data.Entity;

namespace WFFM.ConversionTool.Library.Database.Forms
{
	public partial class SitecoreForms : DbContext
	{
		public SitecoreForms()
			: base("name=SitecoreForms")
		{
		}

		public virtual DbSet<FieldData> FieldDatas { get; set; }
		public virtual DbSet<FormEntry> FormEntries { get; set; }

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
		}
	}
}
