using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
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
        public virtual List<QueryParamInfo> ParseDeclaredParameters(string queryText, string connectionString)
        {
            int i = 0;
            var queryParams = new List<QueryParamInfo>();
            // get design time section
            var dt = Regex.Match(queryText, "-- designTime(?<designTime>.*)-- endDesignTime", RegexOptions.Singleline).Value;
            // extract declared parameters
            string pattern = "declare[^;\n]*";
            Match m = Regex.Match(dt, pattern, RegexOptions.IgnoreCase);
            while (m.Success)
            {
                string[] parts = m.Value.Split(new[] { ' ', '	' }, StringSplitOptions.RemoveEmptyEntries);
                var qp = GetParamInfo(parts[1].Substring(1), parts[2]);
                queryParams.Add(qp);
                m = m.NextMatch();
                i++;
            }
            return queryParams;
        }
        QueryParamInfo GetParamInfo(string name, string sqlTypeAndLength)
        {
            var qp = new QueryParamInfo();
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
            return qp;
        }
        public virtual List<QueryParamInfo> FindUndeclaredParameters(string queryText, string connectionString, out string outputMessage)
        {
            outputMessage = null;
            var myParams = new List<QueryParamInfo>();
            // sp_describe_undeclared_parameters
            try
            {
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
            }
            catch (Exception ex)
            {
                outputMessage = "Unable to find undeclared parameters. Make sure your parameters are declared in the designTime section\n";
            }
            return myParams;
        }
        public virtual void PrepareParametersForSchemaFetching(IDbCommand cmd)
        {
            // nothing to do here.
        }

        public string ConstructParameterDeclarations(List<QueryParamInfo> foundParams)
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
                bldr.Append(";\r\n");
            }
            return bldr.ToString();
        }
        public virtual string MakeAddAParameter(State state)
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
                    return nullable ? "System.Int64?" : "System.Int64";
                case "binary":
                    DBTypeNormalized = "Binary";
                    return "System.Byte[]";
                case "image":
                    DBTypeNormalized = "Image";
                    return "System.Byte[]";
                case "timestamp":
                    DBTypeNormalized = "Timestamp";
                    return "System.Byte[]";
                case "varbinary":
                    DBTypeNormalized = "Varbinary";
                    return "System.Byte[]";
                case "bit":
                    DBTypeNormalized = "Bit";
                    return nullable ? "System.Boolean?" : "System.Boolean";
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
                    return nullable ? "System.Decimal?" : "System.Decimal";
                case "money":
                    DBTypeNormalized = "Money";
                    return nullable ? "System.Decimal?" : "System.Decimal";
                case "smallmoney":
                    DBTypeNormalized = "SmallMoney";
                    return nullable ? "System.Decimal?" : "System.Decimal";
                case "float":
                    DBTypeNormalized = "Float";
                    return nullable ? "System.Double?" : "System.Double";
                case "real":
                    DBTypeNormalized = "Real";
                    return nullable ? "System.Single?" : "System.Single";
                case "smallint":
                    DBTypeNormalized = "SmallInt";
                    return nullable ? "System.Single?" : "System.Single";
                case "tinyint":
                    DBTypeNormalized = "TinyInt";
                    return nullable ? "System.Byte?" : "System.Byte";
                case "int":
                    DBTypeNormalized = "Int";
                    return nullable ? "System.Int32?" : "System.Int32";
                case "char":
                    DBTypeNormalized = "Char";
                    return "System.String";
                case "nchar":
                    DBTypeNormalized = "NChar";
                    return "System.String";
                case "ntext":
                    DBTypeNormalized = "NText";
                    return "System.String";
                case "nvarchar":
                    DBTypeNormalized = "NVarChar";
                    return "System.String";
                case "varchar":
                    DBTypeNormalized = "VarChar";
                    return "System.String";
                case "text":
                    DBTypeNormalized = "Text";
                    return "System.String";
                case "xml":
                    DBTypeNormalized = "Xml";
                    return "System.String";
                case "sql_variant":
                    DBTypeNormalized = "Variant";
                    return "System.Object";
                case "variant":
                    DBTypeNormalized = "Variant";
                    return "System.Object";
                case "udt":
                    DBTypeNormalized = "Udt";
                    return "System.Object";
                case "structured":
                    DBTypeNormalized = "Structured";
                    return "DataTable";
                case "uniqueidentifier":
                    DBTypeNormalized = "UniqueIdentifier";
                    return nullable ? "Guid?" : "Guid";
                default:
                    throw new Exception("type not matched : " + DBType);
                    // todo : keep going here. old method had a second switch on ResultFieldDetails.DataType to catch a bunch of never seen types

            }
        }

        public List<ResultFieldDetails> GetQuerySchema2ndAttempt(string sql, string connectionString)
        {
            using (var conn = GetConnection(connectionString))
            {
                var command = conn.CreateCommand();
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "SP_DESCRIBE_FIRST_RESULT_SET";

                var tsql = new SqlParameter("@TSQL", sql);
                tsql.Direction = ParameterDirection.Input;
                command.Parameters.Add(tsql);
                var returnVal = new List<ResultFieldDetails>();
                using (DataTable dt = new DataTable())
                {
                    conn.Open();
                    var dr = command.ExecuteReader();
                    dt.Load(dr);
                    foreach (DataRow rec in dt.Rows)
                    {
                        string colName = rec.Field<string>("name");
                        string csColName;
                        if (Regex.Match((colName.Substring(0, 1)), "[0-9]").Success)
                            csColName = "_" + colName;
                        else
                            csColName = colName;
                        var qp = GetParamInfo("dontCare", rec.Field<string>("system_type_name"));
                        returnVal.Add(new ResultFieldDetails
                            {
                                ColumnName = colName,
                                AllowDBNull = rec.Field<bool>("is_nullable"),
                                BaseColumnName = rec.Field<string>("name"),
                                ColumnOrdinal = rec.Field<int>("column_ordinal"),
                                CSColumnName = csColName,
                                IsIdentity = rec.Field<bool>("is_identity_column"),
                                NumericPrecision = qp.Precision,
                                NumericScale = qp.Scale,
                                TypeCs = qp.CSType.TrimEnd(new char[] { '?' }) // qp may have the question mark on the end, but on the result side it's managed separately. Bit flaky.
                            }
                        );
                    }
                }
                return returnVal;
            }
        }
    }
}
