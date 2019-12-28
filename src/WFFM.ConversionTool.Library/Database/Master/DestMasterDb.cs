namespace WFFM.ConversionTool.Library.Database.Master
{
	using System;
	using System.Data.Entity;
	using System.ComponentModel.DataAnnotations.Schema;
	using System.Linq;

	public partial class DestMasterDb : DbContext
	{
		public DestMasterDb(string nameOrConnectionString)
			: base(nameOrConnectionString)
		{
		}

		public virtual DbSet<Blob> Blobs { get; set; }
		public virtual DbSet<Item> Items { get; set; }
		public virtual DbSet<SharedField> SharedFields { get; set; }
		public virtual DbSet<UnversionedField> UnversionedFields { get; set; }
		public virtual DbSet<VersionedField> VersionedFields { get; set; }

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
		}
	}
}
