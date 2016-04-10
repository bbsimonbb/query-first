using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

namespace QueryFirstRuntime
{
    public class RuntimeLoader
    {
        public IEnumerable<T> GetData<T>(IDataReader reader, Func<IDataRecord, T> BuildObject)
        {
            try
            {
                while (reader.Read())
                {
                    yield return BuildObject(reader);
                }
            }
            finally
            {
                reader.Dispose();
            }
        }
    }
}