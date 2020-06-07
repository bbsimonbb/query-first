using QueryFirst.TypeMappings;
using System.Collections.Generic;

namespace QueryFirst
{
    public class ResultFieldDetails
    {
        public string ColumnName { get; set; }
        public int ColumnOrdinal { get; set; }
        public int ColumnSize { get; set; }
        public int NumericPrecision { get; set; }
        public int NumericScale { get; set; }
        public bool IsUnique { get; set; }
        public string BaseColumnName { get; set; }
        public string BaseTableName { get; set; }
        public bool AllowDBNull { get; set; }
        public int ProviderType { get; set; }
        public bool IsIdentity { get; set; }
        public bool IsAutoIncrement { get; set; }
        public bool IsRowVersion { get; set; }
        public bool IsLong { get; set; }
        public bool IsReadOnly { get; set; }
        public string ProviderSpecificDataType { get; set; }
        public string TypeDb { get; set; }
        public string TypeCs { get; set; }
        public string TypeCsShort { get { return System2Alias.Map(TypeCs, AllowDBNull); } }
        public string UdtAssemblyQualifiedName { get; set; }
        public int NewVersionedProviderType { get; set; }
        public bool IsColumnSet { get; set; }
        public string RawProperties { get; set; }
        public int NonVersionedProviderType { get; set; }
        public string CSColumnName { get; set; }
        public Dictionary<string, string> ColumnOptions { get; set; }
    }
}
