using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryFirst.TypeMappings
{
    public static class System2Alias
    {
        private static Dictionary<string, string> _map = new Dictionary<string, string>()
            {
                {"System.Int32","int" },
                {"System.String","string" },
                {"System.Boolean","bool" },
                {"System.Byte","byte" },
                {"System.SByte","sbyte" },
                {"System.Char","char" },
                {"System.Decimal","decimal" },
                {"System.Double","double" },
                {"System.Single","float" },
                {"System.UInt32","uint" },
                {"System.Int64","long" },
                {"System.UInt64","ulong" },
                {"System.Object","object" },
                {"System.Int16","short" },
                {"System.UInt16","ushort" },
                {"System.Guid","Guid" },
                {"System.DateTime","DateTime" }
            };

        public static string Map(string CSType, bool AllowDBNull)
        {
            if (_map.ContainsKey(CSType))
            {
                var nullable = "?";
                if (!AllowDBNull || CSType == "System.String" || CSType == "System.Object")
                    nullable = "";
                return _map[CSType] + nullable;
            }
            else
            {
                return CSType;
            }
        }
    }
}
