using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;
using TinyIoC;

namespace QueryFirst.Providers
{
    [RegistrationName("MySql.Data.MySqlClient")]
    class MySqlClient : SqlClient, IProvider
    {

        public override IDbConnection GetConnection(string connectionString)
        {
            return new MySqlConnection(connectionString);
        }

        public override List<QueryParamInfo> ParseDeclaredParameters(string queryText, string connectionString)
        {
            var queryParams = new List<QueryParamInfo>();
            // get design time section
            var dt = Regex.Match(queryText, "-- designTime(?<designTime>.*)-- endDesignTime", RegexOptions.Singleline).Value;
            // extract declared parameters
            string pattern = "\\bset\\s+(?<param>@\\w+)";
            var myParams = Regex.Matches(dt, pattern, RegexOptions.IgnoreCase);
            if (myParams.Count > 0)
            {
                foreach (Match m in myParams)
                {
                    var name = m.Groups["param"].Value.Substring(1);
                    queryParams.Add(new QueryParamInfo()
                    {
                        DbName = m.Groups["param"].Value,
                        CSNameCamel = char.ToLower(name.First()) + name.Substring(1),
                        CSNamePascal = char.ToUpper(name.First()) + name.Substring(1),
                        CSNamePrivate = "_" + char.ToLower(name.First()) + name.Substring(1),
                        CSType = "object",
                        DbType = "Object"
                    });
                }
            }

            return queryParams;
        }
        public override string MakeAddAParameter(State state)
        {
            StringBuilder code = new StringBuilder();
            code.AppendLine("private void AddAParameter(IDbCommand Cmd, string DbType, string DbName, object Value, int Length, int Scale, int Precision)\n{");
            code.AppendLine("((MySql.Data.MySqlClient.MySqlCommand)Cmd).Parameters.AddWithValue(DbName, Value);");
            code.AppendLine("}");
            return code.ToString();
        }
        public override List<QueryParamInfo> FindUndeclaredParameters(string queryText, string connectionString, out string outputMessage)
        {
            outputMessage = null;
            return new List<QueryParamInfo>();
        }
        public override string HookUpForExecutionMessages()
        {
            return "";
        }
    }
}
