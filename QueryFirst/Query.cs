using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;
using System.Data.SqlClient;
using EnvDTE;
using QueryFirst.TypeMappings;

namespace QueryFirst
{
    public class Query
    {
        private CodeGenerationContext ctx;
        private string text;
        private IProvider provider;
        private List<IQueryParamInfo> queryParams;
        /// <summary>
        /// List of param names and types, parsed from declare statements anywhere in the sql query 
        /// or recovered with sp_describe_undeclared_parameters.
        /// Names are as declared, with leading @. The portion following the @ must be a valid C# identifier.
        /// Types are C# types that map to the sql type in the declare statement.
        /// </summary>
        public List<IQueryParamInfo> QueryParams
        {
            get
            {
                if (queryParams == null)
                {
                    queryParams = ctx.Provider.ParseDeclaredParameters(Text);
                }
                return queryParams;
            }
        }
        public Query(CodeGenerationContext _ctx)
        {
            ctx = _ctx;
            var textDoc = ((TextDocument)ctx.QueryDoc.Object());
            var start = textDoc.StartPoint;
            text = start.CreateEditPoint().GetText(textDoc.EndPoint);
            provider = ctx.Provider;

        }
        public void ReplacePattern(string pattern, string replaceWith)
        {
            var textDoc = ((TextDocument)ctx.QueryDoc.Object());
            textDoc.ReplacePattern(pattern, replaceWith);
            var start = textDoc.StartPoint;
            text = start.CreateEditPoint().GetText(textDoc.EndPoint);
            queryParams = null;
        }
        public string Text {
            get { return text; }
            set
            {
                var textDoc = ((TextDocument)ctx.QueryDoc.Object());
                var ep = textDoc.CreateEditPoint();
                ep.ReplaceText(textDoc.EndPoint, value, 0);
                text = value;
            }
        }
        public bool IsQFQuery()
        {
            return Text.Contains("managed by QueryFirst");
        }
        public void ConvertForDesignDebug()
        {
            text = text.Replace("/*designTime", "-- designTime");
            text = text.Replace("endDesignTime*/", "-- endDesignTime");
        }
        public void ConvertForProductionBuild()
        {
            // backwards  compatible
            text = text.Replace("--designTime", "/*designTime");
            text = text.Replace("--endDesignTime", "endDesignTime*/");
            text = text.Replace("-- designTime", "/*designTime");
            text = text.Replace("-- endDesignTime", "endDesignTime*/");
        }
    }
}
