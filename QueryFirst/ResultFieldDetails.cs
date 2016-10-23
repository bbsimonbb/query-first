using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using TinyIoC;

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
        public string DataType { get; set; }
        public bool AllowDBNull { get; set; }
        public int ProviderType { get; set; }
        public bool IsIdentity { get; set; }
        public bool IsAutoIncrement { get; set; }
        public bool IsRowVersion { get; set; }
        public bool IsLong { get; set; }
        public bool IsReadOnly { get; set; }
        public string ProviderSpecificDataType { get; set; }
        public string DataTypeName { get; set; }
        public string UdtAssemblyQualifiedName { get; set; }
        public int NewVersionedProviderType { get; set; }
        public bool IsColumnSet { get; set; }
        public string RawProperties { get; set; }
        public int NonVersionedProviderType { get; set; }

        //sby do I need this? Schema table has a .NET type.
        public string CSType
        {
            get
            {
                string notUsed;
                return TinyIoCContainer.Current.Resolve<ITypeMapping>().DBType2CSType(DataTypeName, out notUsed, AllowDBNull);
            }
        }
        private string csColumnName;
        public string CSColumnName
        {
            get
            {
                if (csColumnName == null)
                {
                    if (Regex.Match((ColumnName.Substring(0, 1)), "[0-9]").Success)
                        csColumnName = "_" + ColumnName;
                    else
                        csColumnName = ColumnName;
                    if (ColumnName.IndexOf('?') != -1)
                        csColumnName = csColumnName.Substring(0, ColumnName.IndexOf('?'));
                }
                return csColumnName;
            }
        }
        private Dictionary<string, string> columnOptions;
        private Dictionary<string, string> ColumnOptions
        {
            get
            {
                if(columnOptions == null)
                {
                    columnOptions = new Dictionary<string, string>();
                    var parts = ColumnName.Split('?');
                    if (parts.Length == 2)
                    {
                        var nameVals = parts[1].Split('&');
                        foreach (var option in nameVals)
                        {
                            var nameVal = option.Split('=');
                            columnOptions.Add(nameVal[0], nameVal.Length > 1 ? nameVal[1] : null);
                        }
                    }
                }
                return columnOptions;
            }
        }

    }

}
