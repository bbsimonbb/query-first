using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;
using TinyIoC;

namespace QueryFirst.Providers
{
    [RegistrationName("MySql.Data.MySqlClient")]
    class MySqlClient : SqlClient, IProvider
    {

        public override IDbConnection GetConnection(ConnectionStringSettings connectionString)
        {
            return new MySqlConnection(connectionString.ConnectionString);
        }
        
        public override List<IQueryParamInfo> ParseDeclaredParameters(string queryText)
        {
            int i = 0;
            var queryParams = new List<IQueryParamInfo>();
            // get design time section
            var dt = Regex.Match(queryText, "-- designTime(?<designTime>.*)-- endDesignTime", RegexOptions.Singleline).Value;
            // extract declared parameters
            string pattern = "\\bset\\s+(?<param>@\\w+)";
            var myParams = Regex.Matches(dt, pattern, RegexOptions.IgnoreCase);
            if(myParams.Count > 0)
            {
                foreach(Match m in myParams)
                {
                    queryParams.Add(new QueryParamInfo()
                    {
                        DbName = m.Groups["param"].Value,
                        CSName = m.Groups["param"].Value.Substring(1),
                        CSType = "object",
                        DbType = "Object"
                    });
                }
            }

            return queryParams;
        }
        public override string MakeAddAParameter(ICodeGenerationContext ctx)
        {
            StringBuilder code = new StringBuilder();
            code.AppendLine("private void AddAParameter(IDbCommand Cmd, string DbType, string DbName, object Value, int Length, int Scale, int Precision)\n{");
            code.AppendLine("((MySql.Data.MySqlClient.MySqlCommand)Cmd).Parameters.AddWithValue(DbName, Value);");
            code.AppendLine("}");
            return code.ToString();
        }
        public override List<IQueryParamInfo> FindUndeclaredParameters(string queryText)
        {
            return new List<IQueryParamInfo>();
        }
    }
}
