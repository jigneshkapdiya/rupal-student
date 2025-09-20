using Microsoft.Data.SqlClient;
using System.Data;

namespace RupalStudentCore8App.Server.Class
{
    public class DbFactory
    {
        public DbFactory(string connectionString)
        {
            db = new SqlConnection(connectionString);
        }

        public IDbConnection db { get; }
    }
}
