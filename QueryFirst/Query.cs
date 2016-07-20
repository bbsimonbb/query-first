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

namespace QueryFirst
{
    public class Query
    {
        private CodeGenerationContext ctx;
        private string text;
        private IMap map;
        private List<QueryParam> queryParams;
        /// <summary>
        /// List of param names and types, parsed from declare statements anywhere in the sql query 
        /// or recovered with sp_describe_undeclared_parameters.
        /// Names are as declared, with leading @. The portion following the @ must be a valid C# identifier.
        /// Types are C# types that map to the sql type in the declare statement.
        /// </summary>
        public List<QueryParam> QueryParams
        {
            get
            {
                if (queryParams == null)
                {
                    int i = 0;
                    queryParams = new List<QueryParam>();
                    // extract declared parameters
                    string pattern = "declare[^;]*";
                    Match m = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
                    while (m.Success)
                    {
                        string[] parts = m.Value.Split(' ');
                        queryParams.Add(QueryParam.Create(parts[1].Substring(1), parts[2], true, map));
                        m = m.NextMatch();
                        i++;
                    }
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
            map = ctx.Map;

        }

        public string Text { get { return text; } }
        public bool IsQFQuery()
        {
            return Text.Contains("managed by QueryFirst");
        }
        public void ConvertForDesignDebug()
        {
            text = text.Replace("/*designTime", "--designTime");
            text = text.Replace("endDesignTime*/", "--endDesignTime");
        }
        public void ConvertForProductionBuild()
        {
            text = text.Replace("--designTime", "/*designTime");
            text = text.Replace("--endDesignTime", "endDesignTime*/");
        }
        public void DiscoverParams()
        {
            // sp_describe_undeclared_parameters
            using (SqlConnection conn = new SqlConnection(ctx.DesignTimeConnectionString))
            {
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "sp_describe_undeclared_parameters @tsql";
                var tsql = new SqlParameter("@tsql", System.Data.SqlDbType.NChar);
                tsql.Value = text;
                cmd.Parameters.Add(tsql);
                StringBuilder bldr = new StringBuilder();

                conn.Open();
                var rdr = cmd.ExecuteReader();
                bldr.Length = 0; // reuse
                while (rdr.Read())
                {
                    // ignore global variables
                    if (rdr.GetString(1).Substring(0, 2) != "@@")
                    {
                        // build declaration.
                        bldr.AppendLine("declare " + rdr.GetString(1) + " " + rdr.GetString(3) + ";");
                        queryParams = null; // reset the list, they will be re-read from the updated text.
                    }


                }
                //inject discovered params
                if (bldr.Length > 0)
                {
                    int insertHere = text.IndexOf("endDesignTime") - 2;
                    text = text.Substring(0, insertHere) + bldr.ToString() + text.Substring(insertHere);
                    //File.WriteAllText(ctx.QueryDoc.FullName, text);
                    var textDoc = ((TextDocument)ctx.QueryDoc.Object());
                    textDoc.ReplacePattern("--endDesignTime", bldr.ToString() + "--endDesignTime");
                }

            }
        }
    }
}
