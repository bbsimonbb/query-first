using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryFirst
{
    public static class QfRuntimeConnection
    {
        public static string CurrentConnectionString { get; set; }
        public static string GetConnectionString()
        {
            return CurrentConnectionString;
        }
    }
}
