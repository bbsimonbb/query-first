using System.Collections.Generic;

namespace QueryFirst
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

        public static string Map(string csType, bool nullable)
        {
            if (_map.ContainsKey(csType))
            {
                var qm = "?";
                if (!nullable || csType == "System.String" || csType == "System.Object")
                    qm = "";
                return _map[csType] + qm;
            }
            else
            {
                return csType;
            }
        }
    }
}
