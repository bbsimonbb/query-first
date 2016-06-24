using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Data.SqlClient;

namespace QueryFirst
{
    public class QueryParam
    {
        public string Name { get; private set; }
        public string SqlTypeAndLength { get; private set; }
        public string CSType { get; private set; }
        public bool ExplicitlyDeclared { get; set; }

        private QueryParam(string name, string sqlTypeAndLength, bool explicitlyDeclared, IMap map)
        {
            Name = name;
            SqlTypeAndLength = sqlTypeAndLength;
            if (sqlTypeAndLength.IndexOf('(') > 0)
                CSType = map.DBType2CSType(sqlTypeAndLength.Substring(0, sqlTypeAndLength.IndexOf('('))); //param type, withouth length
            else
                CSType = map.DBType2CSType(sqlTypeAndLength);
        }
        public static QueryParam Create(string name, string sqlTypeAndLength, bool explicitlyDeclared, IMap map)
        {
            return new QueryParam(name, sqlTypeAndLength, explicitlyDeclared, map);
        }
    }
}
