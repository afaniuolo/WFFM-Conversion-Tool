using System.Data.Entity;

namespace WFFM.ConversionTool.FormsData.Database.Forms
{
	public partial class SitecoreForms : DbContext
	{
		public SitecoreForms(string nameOrConnectionString)
			: base(nameOrConnectionString)
		{
		}

		public virtual DbSet<FieldData> FieldDatas { get; set; }
		public virtual DbSet<FormEntry> FormEntries { get; set; }

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
		}
	}
}
