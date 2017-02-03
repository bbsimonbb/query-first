using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using QueryFirst.TypeMappings;

namespace QueryFirst
{
    public class QueryParamInfo : IQueryParamInfo
    {
        public string CSName { get; set; }
        //public string SqlTypeAndLength { get; private set; }
        public string CSType { get; set; }
        //public bool ExplicitlyDeclared { get; set; }
        public string DbName { get; set; }
        public string DbType { get; set; }
        public int Length { get; set; }
        public int Precision { get; set; }
        public int Scale { get; set; }
    }
}
