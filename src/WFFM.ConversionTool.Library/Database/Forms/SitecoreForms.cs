namespace WFFM.ConversionTool.Library.Database.Forms
{
	using System;
	using System.Data.Entity;
	using System.ComponentModel.DataAnnotations.Schema;
	using System.Linq;

	public partial class SitecoreForms : DbContext
	{
		public SitecoreForms(string nameOrConnectionString)
			: base(nameOrConnectionString)
		{
		}

		public virtual DbSet<FileStorage> FileStorages { get; set; }
		public virtual DbSet<FieldData> FieldDatas { get; set; }
		public virtual DbSet<FormEntry> FormEntries { get; set; }

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			modelBuilder.Entity<FileStorage>()
				.Property(e => e.Created)
				.HasPrecision(3);

			modelBuilder.Entity<FormEntry>()
				.Property(e => e.Created)
				.HasPrecision(3);

			modelBuilder.Entity<FormEntry>()
				.HasMany(e => e.FieldDatas)
				.WithRequired(e => e.FormEntry)
				.WillCascadeOnDelete(false);
		}
	}
}
