using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinyIoC;
using EnvDTE;
using System.IO;
using System.Text.RegularExpressions;

namespace QueryFirst
{
    public class CodeGenerationContext
    {
        protected TinyIoCContainer tiny;
        protected DTE dte;
        public DTE Dte { get { return dte; } }
        protected Document queryDoc;
        public Document QueryDoc { get { return queryDoc; } }
        protected ITypeMapping map;
        public ITypeMapping Map { get { return map; } }
        protected Query query;
        public Query Query { get { return query; } }
        protected string baseName;
        private ConfigurationAccessor config;
        public ConfigurationAccessor ProjectConfig
        {
            get
            {
                if (config == null)
                    config = new ConfigurationAccessor(dte, null);
                return config;
            }
        }
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
        protected string designTimeConnectionString;
        /// <summary>
        /// For recuperating the query schema at design time.
        /// </summary>
        public virtual string DesignTimeConnectionString
        {
            get
            {
                if (string.IsNullOrEmpty(designTimeConnectionString))
                {
                    designTimeConnectionString = ProjectConfig.ConnectionStrings["QfDefaultConnection"].ConnectionString;
                }
                return designTimeConnectionString;
            }
        }

        protected string methodSignature;
        /// <summary>
        /// Parameter types and names, extracted from sql, with trailing comma.
        /// </summary>
        public virtual string MethodSignature
        {
            get
            {
                if (string.IsNullOrEmpty(methodSignature))
                {
                    StringBuilder sig = new StringBuilder();
                    int i = 0;
                    foreach(var qp in Query.QueryParams)
                    {
                        sig.Append(qp.CSType + ' ' + qp.Name + ", ");
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
        /// Parameter names, if any, with trailing "conn". String used by connectionless methods to call their connectionful overloads.
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
                        sig.Append(qp.CSType + ' ' + qp.Name + ", ");
                        call.Append(qp.Name + ", ");
                    }
                    //signature trailing comma trimmed in place if needed. 
                    call.Append("conn"); // calling args always used to call overload with connection
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
        protected ADOHelper hlpr;
        /// <summary>
        /// The class that runs the query and returns the schema table
        /// </summary>
        public ADOHelper Hlpr { get { return hlpr; } }
        protected ProjectItem resultsClass;

        // constructor
        public CodeGenerationContext(Document queryDoc)
        {
            tiny = TinyIoCContainer.Current;
            map = tiny.Resolve<ITypeMapping>();
            queryHasRun = false;
            this.queryDoc = queryDoc;
            dte = queryDoc.DTE;
            query = new Query(this);


            string currDir = Path.GetDirectoryName(queryDoc.FullName);
            //WriteToOutput("\nprocessing " + queryDoc.FullName );
            // class name and namespace read from user's half of partial class.
            //QfClassName = Regex.Match(query, "(?im)^--QfClassName\\s*=\\s*(\\S+)").Groups[1].Value;
            //QfNamespace = Regex.Match(query, "(?im)^--QfNamespace\\s*=\\s*(\\S+)").Groups[1].Value;
            // doc.fullname started being lowercase ??
            //namespaceAndClassNames = GetNamespaceAndClassNames(resultsClass);

            hlpr = new ADOHelper();
        }
        //public Tuple<string, string, string> GetNamespaceAndClassNames(ProjectItem userPartialClass)
        //{
        //    string _namespace = null;
        //    string _loaderClass = null;
        //    string _resultsClass = null;
        //    FileCodeModel model = userPartialClass.FileCodeModel;
        //    foreach (CodeElement element in model.CodeElements)
        //    {
        //        if (element.Kind == vsCMElement.vsCMElementNamespace)
        //        {
        //            _namespace = element.FullName;
        //            foreach (CodeElement child in element.Children)
        //            {
        //                if (child.Kind == vsCMElement.vsCMElementClass)
        //                {
        //                    // a bit complicated but here we go. If partial class name finishes with "Results",
        //                    // derive the loader class name by trimming "Results". Else derive the loader class name
        //                    // by adding "Request".
        //                    _resultsClass = child.Name;
        //                    if (_resultsClass.Length > 7 && _resultsClass.Substring(_resultsClass.Length - 7) == "Results")
        //                        _loaderClass = _resultsClass.Substring(0, _resultsClass.Length - 7);
        //                    else
        //                        _loaderClass = _resultsClass + "Request";
        //                    return new Tuple<string, string, string>(_namespace, _loaderClass, _resultsClass);
        //                }
        //            }
        //        }
        //    }
        //    return null;
        //}

        //static string GetAssemblyPath(EnvDTE.Project vsProject)
        //{
        //    string fullPath = vsProject.Properties.Item("FullPath").Value.ToString();
        //    string outputPath = vsProject.ConfigurationManager.ActiveConfiguration.Properties.Item("OutputPath").Value.ToString();
        //    string outputDir = Path.Combine(fullPath, outputPath);
        //    string outputFileName = vsProject.Properties.Item("OutputFileName").Value.ToString();
        //    string assemblyPath = Path.Combine(outputDir, outputFileName);
        //    return assemblyPath;
        //}


    }
}
