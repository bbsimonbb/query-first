using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using TinyIoC;

namespace QueryFirst.Providers
{
    [RegistrationName("System.Data.SqlClient")]
    class SqlClient : IProvider
    {
        public virtual IDbConnection GetConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }
        public virtual List<IQueryParamInfo> ParseDeclaredParameters(string queryText, string connectionString)
        {
            int i = 0;
            var queryParams = new List<IQueryParamInfo>();
            // get design time section
            var dt = Regex.Match(queryText, "-- designTime(?<designTime>.*)-- endDesignTime", RegexOptions.Singleline).Value;
            // extract declared parameters
            string pattern = "declare[^;\n]*";
            Match m = Regex.Match(dt, pattern, RegexOptions.IgnoreCase);
            while (m.Success)
            {
                string[] parts = m.Value.Split(new[] { ' ', '	' }, StringSplitOptions.RemoveEmptyEntries);
                var qp = TinyIoC.TinyIoCContainer.Current.Resolve<IQueryParamInfo>();
                FillParamInfo(qp, parts[1].Substring(1), parts[2]);
                queryParams.Add(qp);
                m = m.NextMatch();
                i++;
            }
            return queryParams;
        }
        private List<string> typesWithLength = new List<string>() { "char", "varchar", "nchar", "nvarchar" };
        private void FillParamInfo(IQueryParamInfo qp, string name, string sqlTypeAndLength)
        {
            var m = Regex.Match(sqlTypeAndLength, @"(?'type'^\w*)\(?(?'firstNum'\d*),?(?'secondNum'\d*)");
            var typeOnly = m.Groups["type"].Value;
            int.TryParse(m.Groups["firstNum"].Value, out int firstNum);
            int.TryParse(m.Groups["secondNum"].Value, out int secondNum);
            if (secondNum != 0)
            {
                qp.Precision = firstNum;
                qp.Scale = secondNum;
            }
            else if (typeOnly.ToLower() == "datetime2")
            {
                qp.Precision = firstNum;
            }
            else if (firstNum > 0)
            {
                qp.Length = firstNum;
            }
            string normalizedType;
            var csType = TypeMapDB2CS(typeOnly, out normalizedType);

            qp.CSType = csType;
            qp.DbType = normalizedType;
            qp.CSName = name;
            qp.DbName = '@' + name;
        }
        public virtual List<IQueryParamInfo> FindUndeclaredParameters(string queryText, string connectionString)
        {
            var myParams = new List<IQueryParamInfo>();
            // sp_describe_undeclared_parameters
            using (IDbConnection conn = GetConnection(connectionString))
            {
                IDbCommand cmd = conn.CreateCommand();
                cmd.CommandText = "sp_describe_undeclared_parameters @tsql";
                var tsql = new SqlParameter("@tsql", System.Data.SqlDbType.NChar);
                tsql.Value = queryText;
                cmd.Parameters.Add(tsql);

                conn.Open();
                var rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    // ignore global variables
                    if (rdr.GetString(1).Substring(0, 2) != "@@")
                    {
                        // build declaration.
                        myParams.Add(new QueryParamInfo()
                        {
                            DbName = rdr.GetString(1),
                            DbType = rdr.GetString(3),
                            Length = rdr.GetInt16(4)
                        }
                            );
                    }
                }
            }
            return myParams;
        }
        public virtual void PrepareParametersForSchemaFetching(IDbCommand cmd)
        {
            // nothing to do here.
        }

        public string ConstructParameterDeclarations(List<IQueryParamInfo> foundParams)
        {
            StringBuilder bldr = new StringBuilder();

            foreach (var qp in foundParams)
            {
                // build declaration.
                bldr.Append("declare " + qp.DbName + " " + qp.DbType);
                //if (qp.Length != 0)
                //{
                //    bldr.Append("(" + qp.Length + ")");
                //}
                bldr.Append(";\n");
            }
            return bldr.ToString();
        }
        public virtual string MakeAddAParameter(ICodeGenerationContext ctx)
        {
            StringBuilder code = new StringBuilder();
            code.AppendLine("private void AddAParameter(IDbCommand Cmd, string DbType, string DbName, object Value, int Length, byte Scale, byte Precision)\n{");
            code.AppendLine("var dbType = (SqlDbType)System.Enum.Parse(typeof(SqlDbType), DbType);");
            code.AppendLine("SqlParameter myParam;");
            code.AppendLine("if(Length != 0){");
            code.AppendLine("myParam = new SqlParameter(DbName, dbType, Length);");
            code.AppendLine("}else{");
            code.AppendLine("myParam = new SqlParameter(DbName, dbType);");
            code.AppendLine("}");
            code.AppendLine("myParam.Value = Value != null ? Value : DBNull.Value;");
            code.AppendLine("myParam.Scale = Scale;");
            code.AppendLine("myParam.Precision = Precision;");
            code.AppendLine("Cmd.Parameters.Add( myParam);");
            code.AppendLine("}");

            return code.ToString();

        }
        public virtual string TypeMapDB2CS(string DBType, out string DBTypeNormalized, bool nullable = true)
        {
            switch (DBType.ToLower())
            {
                case "bigint":
                    DBTypeNormalized = "BigInt";
                    return nullable ? "long?" : "long";
                case "binary":
                    DBTypeNormalized = "Binary";
                    return "byte[]";
                case "image":
                    DBTypeNormalized = "Image";
                    return "byte[]";
                case "timestamp":
                    DBTypeNormalized = "Timestamp";
                    return "byte[]";
                case "varbinary":
                    DBTypeNormalized = "Varbinary";
                    return "byte[]";
                case "bit":
                    DBTypeNormalized = "Bit";
                    return nullable ? "bool?" : "bool";
                case "date":
                    DBTypeNormalized = "Date";
                    return nullable ? "DateTime?" : "DateTime";
                case "datetime":
                    DBTypeNormalized = "DateTime";
                    return nullable ? "DateTime?" : "DateTime";
                case "datetime2":
                    DBTypeNormalized = "DateTime2";
                    return nullable ? "DateTime?" : "DateTime";
                case "smalldatetime":
                    DBTypeNormalized = "SmallDateTime";
                    return nullable ? "DateTime?" : "DateTime";
                case "time":
                    DBTypeNormalized = "Time";
                    return nullable ? "TimeSpan?" : "TimeSpan";
                case "datetimeoffset":
                    DBTypeNormalized = "DateTimeOffset";
                    return nullable ? "DateTimeOffset?" : "DateTimeOffset";
                case "decimal":
                    DBTypeNormalized = "Decimal";
                    return nullable ? "decimal?" : "decimal";
                case "money":
                    DBTypeNormalized = "Money";
                    return nullable ? "decimal?" : "decimal";
                case "smallmoney":
                    DBTypeNormalized = "SmallMoney";
                    return nullable ? "decimal?" : "decimal";
                case "float":
                    DBTypeNormalized = "Float";
                    return nullable ? "double?" : "double";
                case "real":
                    DBTypeNormalized = "Real";
                    return nullable ? "float?" : "float";
                case "smallint":
                    DBTypeNormalized = "SmallInt";
                    return nullable ? "short?" : "short";
                case "tinyint":
                    DBTypeNormalized = "TinyInt";
                    return nullable ? "byte?" : "byte";
                case "int":
                    DBTypeNormalized = "Int";
                    return nullable ? "int?" : "int";
                case "char":
                    DBTypeNormalized = "Char";
                    return "string";
                case "nchar":
                    DBTypeNormalized = "NChar";
                    return "string";
                case "ntext":
                    DBTypeNormalized = "NText";
                    return "string";
                case "nvarchar":
                    DBTypeNormalized = "NVarChar";
                    return "string";
                case "varchar":
                    DBTypeNormalized = "VarChar";
                    return "string";
                case "text":
                    DBTypeNormalized = "Text";
                    return "string";
                case "xml":
                    DBTypeNormalized = "Xml";
                    return "string";
                case "sql_variant":
                    DBTypeNormalized = "Variant";
                    return "object";
                case "variant":
                    DBTypeNormalized = "Variant";
                    return "object";
                case "udt":
                    DBTypeNormalized = "Udt";
                    return "object";
                case "structured":
                    DBTypeNormalized = "Structured";
                    return "DataTable";
                case "uniqueidentifier":
                    DBTypeNormalized = "UniqueIdentifier";
                    return "Guid";
                default:
                    throw new Exception("type not matched : " + DBType);
                    // todo : keep going here. old method had a second switch on ResultFieldDetails.DataType to catch a bunch of never seen types

            }
        }
    }
}
