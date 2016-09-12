using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Data.SqlClient;

namespace QueryFirst
{
    public class QueryParam : IQueryParam
    {
        public string Name { get; private set; }
        public string SqlTypeAndLength { get; private set; }
        public string CSType { get; private set; }
        public bool ExplicitlyDeclared { get; set; }
        public string SqlDbType { get; set; }
        public int Length { get; set; }

        private ITypeMapping map;

        public QueryParam(ITypeMapping _map)
        {
            map = _map;
        }
        public void Populate(string name, string sqlTypeAndLength, bool explicitlyDeclared)
        {
            Name = name;
            SqlTypeAndLength = sqlTypeAndLength;
            string typeOnly;
            if (sqlTypeAndLength.IndexOf('(') > 0)
            {
                typeOnly = sqlTypeAndLength.Substring(0, sqlTypeAndLength.IndexOf('('));
                Length = int.Parse(sqlTypeAndLength.Substring(sqlTypeAndLength.IndexOf('(') + 1, sqlTypeAndLength.Length - sqlTypeAndLength.IndexOf('(') - 2));
            }
            else
                typeOnly = sqlTypeAndLength;

            string sqlDbType;
            CSType = map.DBType2CSType(typeOnly, out sqlDbType);
            SqlDbType = sqlDbType;
        }
    }
}
