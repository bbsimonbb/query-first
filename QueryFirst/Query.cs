//using EnvDTE;
//using Microsoft.VisualStudio.Shell;
//using System.Collections.Generic;

//namespace QueryFirst
//{
//    public class Query
//    {
//        private ICodeGenerationContext ctx;
//        private string text;
//        private IProvider provider;
//        private List<QueryParamInfo> queryParams;
//        /// <summary>
//        /// List of param names and types, parsed from declare statements anywhere in the sql query 
//        /// or recovered with sp_describe_undeclared_parameters.
//        /// Names are as declared, with leading @. The portion following the @ must be a valid C# identifier.
//        /// Types are C# types that map to the sql type in the declare statement.
//        /// </summary>
//        public List<QueryParamInfo> QueryParams
//        {
//            get
//            {
//                if (queryParams == null)
//                {
//                    queryParams = ctx.Provider.ParseDeclaredParameters(Text, "ctx.Config.DefaultConnection");
//                }
//                return queryParams;
//            }
//        }
//        public Query(ICodeGenerationContext _ctx)
//        {
//            ThreadHelper.ThrowIfNotOnUIThread();
//            ctx = _ctx;
//            var textDoc = ((TextDocument)ctx.QueryDoc.Object());
//            var start = textDoc.StartPoint;
//            text = start.CreateEditPoint().GetText(textDoc.EndPoint);
//            provider = ctx.Provider;

//        }

//        public string Text {
//            get { return text; }
//            //set
//            //{
//            //    var textDoc = ((TextDocument)ctx.QueryDoc.Object());
//            //    var ep = textDoc.CreateEditPoint();
//            //    ep.ReplaceText(textDoc.EndPoint, value, 0);
//            //    text = value;
//            //}
//        }
//        public string FinalTextForCode { get {
//                return text
//                    .Replace("-- designTime", "/*designTime")
//                    .Replace("-- endDesignTime", "endDesignTime*/")
//                    // for inclusion in a verbatim string, only modif required is to double double quotes
//                    .Replace("\"","\"\"");
//            }
//        }
//    }
//}
