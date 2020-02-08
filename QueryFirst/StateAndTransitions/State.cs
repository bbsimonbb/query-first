using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// CodeGenerationContext had a bunch of properties that were set on first use. It's simple stuff,
/// but regexes and string manip were mixed with EnvDTE references and file access. It wasn't testable
/// and it couldn't be used outside of VS. 
/// 
/// Here, we want to build up state being while mastering the order in which
/// things happen, and totally freeing ourselves from EnvDTE and other dependencies.
/// 
/// Properties are numbered so from any given prop you can depend on props lower in the order.
/// </summary>
namespace QueryFirst
{
    public class State
    {
        /// <summary>
        /// The name of the query file, without extension. Used to infer the filenames of code classes, and to generate the wrapper class name.
        /// </summary>
        public string _1BaseName { get; set; }
        /// <summary>
        /// The directory containing the 3 files for this query, with trailing slash
        /// </summary>
        public string _1CurrDir { get; set; }
        /// <summary>
        /// Path and filename of the generated code file.
        /// </summary>
        public string _1GeneratedClassFullFilename { get; set; }
        public string _1UserPartialClassFullFilename { get; set; }
        public string _2ResultClassName { get; set; }
        public string _2ResultInterfaceName { get; set; }
        public string _2Namespace { get; set; }
        /// <summary>
        /// The unmodified text of the SQL query. The full contents of the .sql file when the user saves.
        /// </summary>
        public string _3InitialQueryText { get; set; }
        public QFConfigModel _4Config { get; set; }
        public string _6NewParamDeclarations { get; set; }
        public string _6QueryWithParamsAdded { get; set; }
        public string _6FinalQueryTextForCode { get; set; }
        public List<ResultFieldDetails> _7ResultFields {get;set;}

        /// <summary>
        /// Execute scalar return type should always be nullable, even when the underlying column is not nullable.
        /// </summary>
        public string _7ExecuteScalarReturnType { get; set; }

        /// <summary>
        /// Query params from declarations in the design time section.
        /// </summary>
        public List<QueryParamInfo> _8QueryParams { get; set; }
        public string _8MethodSignature { get; set; }
        public string _8CallingArgs { get; set; }
    }
}
