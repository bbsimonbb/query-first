using System.Collections.Generic;
using System.Text;
using TinyIoC;
using EnvDTE;
using System.IO;
using System.Text.RegularExpressions;
using System;

namespace QueryFirst
{
    public class CodeGenerationContext : ICodeGenerationContext
    {
        protected TinyIoCContainer tiny;
        private PutCodeHere _putCodeHere;
        public PutCodeHere PutCodeHere { get { return _putCodeHere; } }
        protected DTE dte;
        public DTE Dte { get { return dte; } }
        protected Document queryDoc;
        public Document QueryDoc { get { return queryDoc; } }
        protected IProvider provider;
        public IProvider Provider { get { return provider; } }
        protected Query query;
        private IConfigResolver _configResolver;
        private QFConfigModel _config;
        protected ISchemaFetcher _schemaFetcher;


        // 
        public CodeGenerationContext(IConfigResolver configResolver, ISchemaFetcher schemaFetcher)
        {
            _configResolver = configResolver;
            _schemaFetcher = schemaFetcher;
        }
        public void InitForQuery(Document queryDoc)
        {
            tiny = TinyIoCContainer.Current;
            queryHasRun = false;
            this.queryDoc = queryDoc;
            dte = queryDoc.DTE;
            query = new Query(this);
            _config = _configResolver.GetConfig( queryDoc.FullName, query.Text );
            if (string.IsNullOrEmpty(_config.DefaultConnection))
            {
                return; // absence will be picked up in conductor. Not fabulous.
            }
            provider = tiny.Resolve<IProvider>(DesignTimeConnectionString.v.ProviderName);
            provider.Initialize(DesignTimeConnectionString.v);
            // resolving the target project item for code generation. We know the file name, we loop through child items of the query til we find it.
            var target = Conductor.GetItemByFilename(queryDoc.ProjectItem.ProjectItems, GeneratedClassFullFilename);
            if(target == null)
            {
                // .net core has a little problem with nested items.
                target = Conductor.GetItemByFilename(queryDoc.ProjectItem.ContainingProject.ProjectItems, GeneratedClassFullFilename);
            }
            _putCodeHere = new PutCodeHere(target);

            string currDir = Path.GetDirectoryName(queryDoc.FullName);
        }
        public QFConfigModel Config { get { return _config; } }
        public Query Query { get { return query; } }
        protected string baseName;
        /// <summary>
        /// The name of the query file, without extension. Used to infer the filenames of code classes, and to generate the wrapper class name.
        /// </summary>
        public string BaseName
        {
            get
            {
                if (baseName == null)
                    baseName = Path.GetFileNameWithoutExtension((string)queryDoc.ProjectItem.Properties.Item("FullPath").Value);
                return baseName;
            }
        }
        /// <summary>
        /// The directory containing the 3 files for this query, with trailing slash
        /// </summary>
        public string CurrDir { get { return Path.GetDirectoryName((string)queryDoc.ProjectItem.Properties.Item("FullPath").Value) + "\\"; } }
        /// <summary>
        /// The query filename, extension and path relative to approot. Used for generating the call to GetManifestStream()
        /// </summary>
        public string PathFromAppRoot
        {
            get
            {
                string fullNameAndPath = (string)queryDoc.ProjectItem.Properties.Item("FullPath").Value;
                return fullNameAndPath.Substring(queryDoc.ProjectItem.ContainingProject.Properties.Item("FullPath").Value.ToString().Length);
            }
        }
        /// <summary>
        /// DefaultNamespace.Path.From.Approot.QueryFileName.sql
        /// </summary>
        public string NameAndPathForManifestStream
        {
            get
            {
                EnvDTE.Project vsProject = queryDoc.ProjectItem.ContainingProject;
                return vsProject.Properties.Item("DefaultNamespace").Value.ToString() + '.' + PathFromAppRoot.Replace('\\', '.');
            }
        }
        /// <summary>
        /// Path and filename of the generated code file.
        /// </summary>
        public string GeneratedClassFullFilename
        {
            get
            {
                return CurrDir + BaseName + ".gen.cs";
            }
        }
        protected string userPartialClass;
        protected string resultClassName;
        /// <summary>
        /// Result class name, read from the user's half of the partial class, written to the generated half.
        /// </summary>
        public virtual string ResultClassName
        {
            get
            {
                if (string.IsNullOrEmpty(userPartialClass))
                    userPartialClass = File.ReadAllText(CurrDir + BaseName + "Results.cs");
                if (resultClassName == null)
                    resultClassName = Regex.Match(userPartialClass, "(?im)partial class (\\S+)").Groups[1].Value;
                return resultClassName;

            }
        }
        /// <summary>
        /// The query namespace, read from the user's half of the result class, used for the generated code file.
        /// </summary>
        public virtual string Namespace
        {
            get
            {
                if (string.IsNullOrEmpty(userPartialClass))
                    userPartialClass = File.ReadAllText(CurrDir + BaseName + "Results.cs");
                return Regex.Match(userPartialClass, "(?im)^namespace (\\S+)").Groups[1].Value;

            }
        }
        protected DesignTimeConnectionString _dtcs;
        public DesignTimeConnectionString DesignTimeConnectionString
        {
            get
            {
                return _dtcs ?? (_dtcs = new DesignTimeConnectionString(this));
            }
        }

        protected string methodSignature;
        /// <summary>
        /// Parameter types and names, with trailing comma.
        /// </summary>
        public virtual string MethodSignature
        {
            // todo this should be a stringtemplate
            get
            {
                if (string.IsNullOrEmpty(methodSignature))
                {
                    StringBuilder sig = new StringBuilder();
                    int i = 0;
                    foreach (var qp in Query.QueryParams)
                    {
                        sig.Append(qp.CSType + ' ' + qp.CSName + ", ");
                        i++;
                    }
                    //signature trailing comma trimmed in place if not needed. 
                    methodSignature = sig.ToString();
                }
                return methodSignature;
            }
        }
        //taken out of constructor, we don't need this anymore????
        //                ((ISignatureMaker)TinyIoCContainer.Current.Resolve(typeof(ISignatureMaker)))
        //.MakeMethodAndCallingSignatures(ctx.Query.QueryParams, out methodSignature, out callingArgs);
        protected string callingArgs;
        /// <summary>
        /// Parameter names, if any, withOUT trailing "conn". String used by connectionless methods to call their connectionful overloads.
        /// </summary>
        public string CallingArgs
        {
            get
            {
                if (string.IsNullOrEmpty(callingArgs))
                {
                    StringBuilder sig = new StringBuilder();
                    StringBuilder call = new StringBuilder();
                    foreach (var qp in Query.QueryParams)
                    {
                        sig.Append(qp.CSType + ' ' + qp.CSName + ", ");
                        call.Append(qp.CSName + ", ");
                    }
                    //signature trailing comma trimmed in place if needed. 
                    //call.Append("conn"); // calling args always used to call overload with connection
                    callingArgs = call.ToString();
                }
                return callingArgs;
            }
        }
        protected List<ResultFieldDetails> resultFields;
        /// <summary>
        /// The schema table returned from the dummy run of the query.
        /// </summary>
        public List<ResultFieldDetails> ResultFields
        {
            get { return resultFields; }
            set { resultFields = value; }
        }

        protected bool queryHasRun;
        public bool QueryHasRun
        {
            get { return queryHasRun; }
            set { queryHasRun = value; }
        }
        /// <summary>
        /// The class that runs the query and returns the schema table
        /// </summary>
        public ISchemaFetcher SchemaFetcher { get { return _schemaFetcher; } }
        /// <summary>
        /// Execute scalar return type should always be nullable, even when the underlying column is not nullable.
        /// </summary>
        public string ExecuteScalarReturnType {
            get {
                if (IsNullable(Type.GetType(ResultFields[0].TypeCs, false)) || ResultFields[0].TypeCs == "System.String")
                {
                    return ResultFields[0].TypeCs;
                }
                else
                {
                    return ResultFields[0].TypeCs + "?";
                }

            }
        }
        bool IsNullable(Type type) => Nullable.GetUnderlyingType(type) != null;
        protected ProjectItem resultsClass;
    }
}
