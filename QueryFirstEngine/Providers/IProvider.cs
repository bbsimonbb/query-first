using System.Configuration;
using System.Data;
using System.Collections.Generic;

namespace QueryFirst
{
    public interface IProvider
    {
        /// <summary>
        /// Harvests parameter declarations from the -- design time section. Different DBs have different
        /// syntaxes for local variable declaration, and this method needs to understand your DBs syntax. 
        /// A SQL Server declaration for instance looks like DECLARE @myLocalVariable [dataType]
        /// MySql has SET @myLocalVariable (no datatype).
        /// Postgres has no local variables, so parameters need to be inferred directly from the text of the query.
        /// </summary>
        /// <param name="queryText">The text of the query from which parameters are to be extracted.</param>
        /// <returns></returns>
        List<QueryParamInfo> ParseDeclaredParameters(string queryText, string connectionString);
        List<QueryParamInfo> FindUndeclaredParameters(string queryText, string connectionString, out string outputMessage);
        /// <summary>
        /// Find undeclared parameters and add them, either in the declarations section of the text (SqlServer, MySql)
        /// or as regular parameters on the command.
        /// </summary>
        /// <param name="cmd">The command to add parameters to. CommandText must already be assigned.</param>
        void PrepareParametersForSchemaFetching(IDbCommand cmd);
        string ConstructParameterDeclarations(List<QueryParamInfo> foundParams);

        /// <summary>
        /// Creates and returns a provider-specific connection instance.
        /// </summary>
        /// <param name="connectionString">Connection string for the connection.</param>
        /// <returns></returns>
        IDbConnection GetConnection(string connectionString);

        /// <summary>
        /// Returns the C# type to which the reader result can be safely cast, and from which a sql parameter
        /// can be safely created.
        /// </summary>
        /// <param name="DBType">The Transact SQL type name.</param>
        /// <param name="DBTypeNormalized">Outputs the supplied DBType with capitalization corrected.</param>
        /// <returns>The C# type name.</returns>
        string TypeMapDB2CS(string DBType, out string DBTypeNormalized, bool nullable = true);

        /// <summary>
        /// Generates the C# method that adds a parameter to the command. Called once for each parameter in the query.
        /// The method should have the signature...
        /// private void AddAParameter(IDbCommand Cmd, string DbType, string DbName, object Value, int Length)
        /// </summary>
        /// <param name="ctx">The code generation context</param>
        /// <returns></returns>
        string MakeAddAParameter(State state);

        /// <summary>
        /// The ADO way is not working for Sql Server table valued functions. Here we give providers a chance to 
        /// do better if the initial attempt returns no rows.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql">The query whose schema we are interested in</param>
        /// <returns></returns>
        List<ResultFieldDetails> GetQuerySchema2ndAttempt(string sql, string connectionString);
        /// <summary>
        /// Provider specific line in Execute methods to listen for execution messages.
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        string HookUpForExecutionMessages();
    }
}