using System;

namespace WFFM.ConversionTool.FormsData.Migrators
{
	public interface IDataMigrator
	{
		void MigrateData(Guid formId);
	}
}
