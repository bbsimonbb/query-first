using System.Collections.Generic;
using EnvDTE;

namespace QueryFirst
{
    public interface ICodeGenerationContext
    {
        string BaseName { get; }
        string CallingArgs { get; }
        string CurrDir { get; }
        DTE Dte { get; }
        string GeneratedClassFullFilename { get; }
        ISchemaFetcher SchemaFetcher { get; }
        string MethodSignature { get; }
        string NameAndPathForManifestStream { get; }
        string Namespace { get; }
        string PathFromAppRoot { get; }
        IProvider Provider { get; }
        PutCodeHere PutCodeHere { get; }
        Query Query { get; }
        Document QueryDoc { get; }
        bool QueryHasRun { get; set; }
        string ResultClassName { get; }
        List<ResultFieldDetails> ResultFields { get; set; }
        QFConfigModel Config { get; }
        void InitForQuery(Document queryDoc);
        string ExecuteScalarReturnType { get; }
    }
}