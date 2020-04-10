using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Configuration;
using Npgsql;
using TinyIoC;

namespace QueryFirst.Providers
{
    [RegistrationName("Npgsql")]
    public class Npgsql : IProvider
    {
        public IDbConnection GetConnection(string connectionString)
        {
            return new NpgsqlConnection(connectionString);
        }
        public List<QueryParamInfo> ParseDeclaredParameters(string queryText, string connectionString)
        {
            return FindUndeclaredParameters(queryText, connectionString, out string outputMessage);
        }
        public List<QueryParamInfo> FindUndeclaredParameters(string queryText, string connectionString, out string outputMessage)
        {
            var queryParams = new List<QueryParamInfo>();
            var matchParams = Regex.Matches(queryText, "(:|@)\\w*");
            if (matchParams.Count > 0)
            {
                var myParams = new List<QueryParamInfo>();
                foreach (Match foundOne in matchParams)
                {
                    DbType myDbType;
                    string name;
                    string UserDeclaredType = null;
                    var parts = foundOne.Value.Split('_');
                    if (parts.Length > 1)
                    {
                        UserDeclaredType = parts[parts.Length - 1];
                        // just to verify. Does this throw?
                        myDbType = (DbType)System.Enum.Parse(typeof(DbType), UserDeclaredType);
                        name = foundOne.Value.Substring(1, foundOne.Value.Length - UserDeclaredType.Length - 2); // strip type to form csName
                    }
                    else
                    {
                        name = foundOne.Value.Substring(1);
                        UserDeclaredType = "";
                    }
                    var qp = TinyIoC.TinyIoCContainer.Current.Resolve<QueryParamInfo>();
                    qp.CSNameCamel = char.ToLower(name.First()) + name.Substring(1);
                    qp.CSNamePascal = char.ToUpper(name.First()) + name.Substring(1);
                    qp.CSNamePrivate = "_" + qp.CSNameCamel;
                    qp.DbName = foundOne.Value;
                    qp.DbType = UserDeclaredType;
                    if (DBType2CSType.ContainsKey(UserDeclaredType))
                        qp.CSType = DBType2CSType[UserDeclaredType];
                    else
                        qp.CSType = "object"; //lots of convertibility, will do till I figure out how NpgsqlTypes works. Not an enum. Doesn't have half the values listed in the table.
                    queryParams.Add(qp);
                }
            }
            outputMessage = null;
            return queryParams;
        }
        public string ConstructParameterDeclarations(List<QueryParamInfo> foundParams)
        {
            // nothing to do here.
            return null;
        }
        public void PrepareParametersForSchemaFetching(IDbCommand cmd)
        {
            // no notion of declaring parameters in Postgres
            // refacto, will this work harvesting connection string from passed command !
            foreach (var queryParam in FindUndeclaredParameters(cmd.CommandText, cmd.Connection.ConnectionString, out string outputMessage))
            {
                var myParam = new global::Npgsql.NpgsqlParameter();
                myParam.ParameterName = queryParam.DbName;
                if (!string.IsNullOrEmpty(queryParam.DbType))
                {
                    myParam.DbType = (DbType)System.Enum.Parse(typeof(DbType), queryParam.DbType);
                }
                myParam.Value = DBNull.Value;
                cmd.Parameters.Add(myParam);
            }

        }
        public virtual string MakeAddAParameter(State state)
        {
            StringBuilder code = new StringBuilder();
            code.AppendLine("private void AddAParameter(IDbCommand Cmd, string DbType, string DbName, object Value, int Length, int Scale, int Precision)\n{");
            code.AppendLine("var myParam = new Npgsql.NpgsqlParameter();");
            code.AppendLine("myParam.ParameterName = DbName;");
            code.AppendLine("if(DbType != \"\")");
            code.AppendLine("myParam.DbType = (DbType)System.Enum.Parse(typeof(DbType), DbType);");
            code.AppendLine("myParam.Value = Value != null ? Value : DBNull.Value; ");
            code.AppendLine("Cmd.Parameters.Add(myParam);");
            code.AppendLine("}");
            return code.ToString();

        }

        private Dictionary<string, string> DBType2CSType = new Dictionary<string, string>
        {
            {"Boolean","bool" },
            {"Int16","short" },
            {"Int32","int" },
            {"Int64","long" },
            {"Single","float" },
            {"Double","double" },
            {"Decimal","decimal" },
            {"VarNumeric","decimal" },
            {"Currency","decimal" },
            {"String","string" },
            {"StringFixedLength","string" },
            {"AnsiString","string" },
            {"AnsiStringFixedLength","string" },
            {"Date","DateTime" },
            {"DateTime","DateTime" },
            {"DateTimeOffset","DateTime" },
            {"Time","Timespan" },
            {"Binary","byte[]" }
        };

        public string TypeMapDB2CS(string DBType, out string DBTypeNormalized, bool nullable = true)
        {
            // http://www.npgsql.org/doc/types.html
            switch (DBType.ToLower())
            {
                case "bool":
                case "boolean":
                    DBTypeNormalized = "Boolean";
                    return nullable ? "bool?" : "bool";
                case "int2":
                case "smallint":
                    DBTypeNormalized = "Smallint";
                    return nullable ? "short?" : "short";
                case "int4":
                case "integer":
                    DBTypeNormalized = "Integer";
                    return nullable ? "int?" : "int";
                case "int8":
                case "bigint":
                    DBTypeNormalized = "Bigint";
                    return nullable ? "long?" : "long";
                case "float4":
                case "real":
                    DBTypeNormalized = "Real";
                    return nullable ? "float?" : "float";
                case "float8":
                case "double":
                    DBTypeNormalized = "Double";
                    return nullable ? "double?" : "double";
                case "numeric":
                    DBTypeNormalized = "Numeric";
                    return nullable ? "decimal?" : "decimal";
                case "money":
                    DBTypeNormalized = "Money";
                    return nullable ? "decimal?" : "decimal";
                case "text":
                    DBTypeNormalized = "Text";
                    return "string";
                case "varchar":
                    DBTypeNormalized = "Varchar";
                    return "string";
                case "char":
                    DBTypeNormalized = "Char";
                    return "string";
                case "citext":
                    DBTypeNormalized = "Citext";
                    return "string";
                case "json":
                    DBTypeNormalized = "Json";
                    return "string";
                case "jsonb":
                    DBTypeNormalized = "Jsonb";
                    return "string";
                case "xml":
                    DBTypeNormalized = "Xml";
                    return "string";
                case "point":
                    DBTypeNormalized = "Point";
                    return "NpgsqlPoint";
                case "lseg":
                    DBTypeNormalized = "LSeg";
                    return "NpgsqlLSeg";
                case "path":
                    DBTypeNormalized = "Path";
                    return "NpgsqlPath";
                case "polygon":
                    DBTypeNormalized = "Polygon";
                    return "NpgsqlPolygon";
                default:
                    DBTypeNormalized = null;
                    return "object";
            }

            //case "":
            //    DBTypeNormalized = "";
            //    return nullable ? "?" : "";


            //Line line NpgsqlLine
            //Circle circle NpgsqlCircle
            //Box box NpgsqlBox
            //Bit bit BitArray or bool?
            //Varbit varbit BitArray or bool
            //Hstore hstore IDictionary< string,string>
            //Uuid uuid Guid
            //Cidr cidr IPAddress
            //Inet inet IPAddress
            //MacAddr macaddr PhysicalAddress
            //TsVector tsvector NpgsqlTsVector
            //Date date DateTime
            //Interval interval TimeSpan
            //Timestamp timestamp DateTime
            //TimestampTZ timestamptz DateTime
            //Time time TimeSpan
            //TimeTZ timetz DateTimeOffset
            //Bytea bytea byte[]
            //Oid oid uint
            //Xid xid uint
            //Cid cid uint
            //Oidvector oidvector uint[]
            //Name name string
            //InternalChar internalchar byte
            //Geometry geometry PostgisGeometry
        }
        /// <summary>
        /// No implementation for Postgres
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        public List<ResultFieldDetails> GetQuerySchema2ndAttempt(string sql, string connectionString)
        {
            return null;
        }

        public string HookUpForExecutionMessages()
        {
            return "";
        }
    }
}
