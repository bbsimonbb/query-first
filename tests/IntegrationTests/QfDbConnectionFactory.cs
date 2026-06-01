using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using QueryFirst;

namespace QueryFirst.IntegrationTests
{


    public class TestDB : QueryFirstConnectionFactory
    {
        //public IConfiguration _config { get; set; }
        //public QfDbConnectionFactory(IConfiguration config)
        //{
        //    _config = config;
        //}

        public override IDbConnection CreateConnection()
        {
            //return new SqlConnection(_config.GetConnectionString("QueryFirstTestDB"));
            return new SqlConnection("Data Source=s-dev4,1434;Database=QueryFirstTestDB;Trusted_Connection=True;");
        }
    }

    public class MasterDB : QueryFirstConnectionFactory
    {
        public override IDbConnection CreateConnection()
        {
            //return new SqlConnection(_config.GetConnectionString("QueryFirstTestDB"));
            return new SqlConnection("Data Source=s-dev4,1434;Database=master;Trusted_Connection=True;");
        }
    }
}