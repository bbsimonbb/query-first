using System.Configuration;
using System.Data;
using System.Collections.Generic;

namespace QueryFirst
{
    public interface IProvider
    {
        List<IQueryParamInfo> ParseDeclaredParameters(string queryText);
        List<IQueryParamInfo> FindUndeclaredParameters(string queryText);
        /// <summary>
        /// Find undeclared parameters and add them, either in the declarations section of the text (SqlServer, MySql)
        /// or as regular parameters on the command.
        /// </summary>
        /// <param name="cmd">The command to add parameters to. CommandText must already be assigned.</param>
        void PrepareParametersForSchemaFetching(IDbCommand cmd);
        string ConstructParameterDeclarations(List<IQueryParamInfo> foundParams);

        /// <summary>
        /// Creates and returns a provider-specific connection instance.
        /// </summary>
        /// <param name="connectionString">Connection string for the connection.</param>
        /// <returns></returns>
        IDbConnection GetConnection(ConnectionStringSettings connectionString);

        /// <summary>
        /// Returns the C# type to which the reader result can be safely cast, and from which a sql parameter
        /// can be safely created.
        /// </summary>
        /// <param name="DBType">The Transact SQL type name.</param>
        /// <param name="DBTypeNormalized">Outputs the supplied DBType with capitalization corrected.</param>
        /// <returns>The C# type name.</returns>
        string TypeMapDB2CS(string DBType, out string DBTypeNormalized, bool nullable = true);

        string MakeAddAParameter(CodeGenerationContext ctx);
        void Initialize(ConnectionStringSettings designTimeConnectionString);
    }
}