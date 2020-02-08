using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QueryFirst;

namespace QueryFirstTests
{
    public class FakeProvider : IProvider
    {
        public string ConstructParameterDeclarations(List<QueryParamInfo> foundParams)
        {
            throw new NotImplementedException();
        }
        public bool FindUndeclaredParameters_WasCalled = false;
        public List<QueryParamInfo> FindUndeclaredParameters(string queryText, string connectionString, out string outputMessage)
        {
            FindUndeclaredParameters_WasCalled = true;
            outputMessage = "";
            return new List<QueryParamInfo>()
            {
                { new QueryParamInfo
                    {
                        CSName = "custNum",
                        CSType = "int",
                        DbName = "CustNum",
                        Length = 20,
                        Precision = 4            
                    } 
                }
            };
        }

        public IDbConnection GetConnection(string connectionString)
        {
            throw new NotImplementedException();
        }

        public string MakeAddAParameter(State state)
        {
            throw new NotImplementedException();
        }

        public List<QueryParamInfo> ParseDeclaredParameters(string queryText, string connectionString)
        {
            throw new NotImplementedException();
        }

        public void PrepareParametersForSchemaFetching(IDbCommand cmd)
        {
            throw new NotImplementedException();
        }

        public string TypeMapDB2CS(string DBType, out string DBTypeNormalized, bool nullable = true)
        {
            throw new NotImplementedException();
        }

        public List<ResultFieldDetails> GetQuerySchema2ndAttempt(string sql, string connectionString)
        {
            return null;
        }
    }
}
