using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryFirst.TypeMappings
{
    class SqlServer2CS : IDB2CS
    {
        public string Map(string p, out string SqlDbType, bool nullable=true)
        {
            switch (p.ToLower())
            {
                case "bigint":
                    SqlDbType = "BigInt";
                    return nullable ? "long?" : "long";
                case "binary":
                    SqlDbType = "Binary";
                    return "byte[]";
                case "image":
                    SqlDbType = "Image";
                    return "byte[]";
                case "timestamp":
                    SqlDbType = "Timestamp";
                    return "byte[]";
                case "varbinary":
                    SqlDbType = "Varbinary";
                    return "byte[]";
                case "bit":
                    SqlDbType = "Bit";
                    return nullable ? "bool?" : "bool";
                case "date":
                    SqlDbType = "Date";
                    return nullable ? "DateTime?" : "DateTime";
                case "datetime":
                    SqlDbType = "DateTime";
                    return nullable ? "DateTime?" : "DateTime";
                case "datetime2":
                    SqlDbType = "DateTime2";
                    return nullable ? "DateTime?" : "DateTime";
                case "smalldatetime":
                    SqlDbType = "SmallDateTime";
                    return nullable ? "DateTime?" : "DateTime";
                case "time":
                    SqlDbType = "Time";
                    return nullable ? "DateTime?" : "DateTime";
                case "datetimeoffset":
                    SqlDbType = "DateTimeOffset";
                    return nullable ? "DateTimeOffset?" : "DateTimeOffset";
                case "decimal":
                    SqlDbType = "Decimal";
                    return nullable ? "decimal?" : "decimal";
                case "money":
                    SqlDbType = "Money";
                    return nullable ? "decimal?" : "decimal";
                case "smallmoney":
                    SqlDbType = "SmallMoney";
                    return nullable ? "decimal?" : "decimal";
                case "float":
                    SqlDbType = "Float";
                    return nullable ? "double?" : "double";
                case "real":
                    SqlDbType = "Real";
                    return nullable ? "float?" : "float";
                case "smallint":
                    SqlDbType = "SmallInt";
                    return nullable ? "short?" : "short";
                case "tinyint":
                    SqlDbType = "TinyInt";
                    return nullable ? "byte?" : "byte";
                case "int":
                    SqlDbType = "Int";
                    return nullable ? "int?" : "int";
                case "char":
                    SqlDbType = "Char";
                    return "string";
                case "nchar":
                    SqlDbType = "NChar";
                    return "string";
                case "ntext":
                    SqlDbType = "NText";
                    return "string";
                case "nvarchar":
                    SqlDbType = "NVarChar";
                    return "string";
                case "varchar":
                    SqlDbType = "VarChar";
                    return "string";
                case "text":
                    SqlDbType = "Text";
                    return "string";
                case "xml":
                    SqlDbType = "Xml";
                    return "string";
                case "sql_variant":
                    SqlDbType = "Variant";
                    return "object";
                case "variant":
                    SqlDbType = "Variant";
                    return "object";
                case "udt":
                    SqlDbType = "Udt";
                    return "object";
                case "structured":
                    SqlDbType = "Structured";
                    return "DataTable";
                case "uniqueidentifier":
                    SqlDbType = "UniqueIdentifier";
                    return "Guid";
                default:
                    throw new Exception("type not matched : " + p);
                    // todo : keep going here. old method had a second switch on ResultFieldDetails.DataType to catch a bunch of never seen types

            }
        }
    }
}
