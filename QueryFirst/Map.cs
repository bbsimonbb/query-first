using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryFirst
{
    class Map : IMap
    {
        public string DBType2CSType(string p, bool nullable=true)
        {
            switch (p.ToLower())
            {
                case "bigint":
                    return nullable ? "long?" : "long";
                case "binary":
                case "image":
                case "timestamp":
                case "varbinary":
                    return "byte[]";
                case "bit":
                    return nullable ? "bool?" : "bool";
                case "date":
                case "datetime":
                case "datetime2":
                case "smalldatetime":
                case "time":
                    return nullable ? "DateTime?" : "DateTime";
                case "datetimeoffset":
                    return nullable ? "DateTimeOffset?" : "DateTimeOffset";
                case "decimal":
                case "money":
                case "smallmoney":
                    return nullable ? "decimal?" : "decimal";
                case "float":
                    return nullable ? "double?" : "double";
                case "real":
                    return nullable ? "float?" : "float";
                case "smallint":
                    return nullable ? "short?" : "short";
                case "tinyint":
                    return nullable ? "byte?" : "byte";
                case "int":
                    return nullable ? "int?" : "int";
                case "char":
                case "nchar":
                case "ntext":
                case "nvarchar":
                case "varchar":
                case "text":
                case "xml":
                    return "string";
                case "sql_variant":
                case "variant":
                case "udt":
                    return "object";
                case "structured":
                    return "DataTable";
                default:
                    throw new Exception("type not matched : " + p);
                    // todo : keep going here. old method had a second switch on ResultFieldDetails.DataType to catch a bunch of never seen types

            }
        }
    }
}
