using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WFFM.ConversionTool.Library.Database
{
	public static class ConnectionTester
	{
		public static bool IsServerConnected(string connectionString)
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
