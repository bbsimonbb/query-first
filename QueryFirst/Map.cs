using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryFirst
{
    class Map : IMap
    {
        public string DBType2CSType(string p)
        {
            switch (p.ToLower())
            {
                case "bigint":
                    return "long";
                case "binary":
                    return "byte[]";
                case "bit":
                    return "bool";
                case "date":
                case "datetime":
                case "datetime2":
                    return "DateTime";
                case "datetimeoffset":
                    return "DateTimeOffset";
                case "decimal":
                case "money":
                    return "decimal";
                case "float":
                    return "double";
                case "image":
                case "timestamp":
                    return "byte[]";
                case "smallint":
                case "int":
                    return "int";
                case "char":
                case "nchar":
                case "ntext":
                case "nvarchar":
                case "varchar":
                case "text":
                case "xml":
                    return "string";
                case "sql_variant":
                    return "Object";
                default:
                    throw new Exception("type not matched : " + p);
                    // todo : keep going here. old method had a second switch on ResultFieldDetails.DataType to catch a bunch of never seen types

            }
        }
    }
}
