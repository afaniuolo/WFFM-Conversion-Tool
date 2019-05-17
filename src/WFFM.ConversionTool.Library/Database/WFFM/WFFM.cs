using System.Data.Entity;

namespace WFFM.ConversionTool.Library.Database.WFFM
{
	public partial class WFFM : DbContext
	{
		public WFFM(string nameOrConnectionString)
			: base(nameOrConnectionString)
		{
		}

		public virtual DbSet<FieldData> FieldDatas { get; set; }
		public virtual DbSet<FormData> FormDatas { get; set; }

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
		}
	}

	public interface IContext
	{
	}
}
