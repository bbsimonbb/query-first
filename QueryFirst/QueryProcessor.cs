using System.Text;
using System.IO;
using System;
using System.Data;
using System.Data.SqlClient;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.CommandBars;
using System.Text.RegularExpressions;
using System.Resources;
using System.Reflection;
using System.Globalization;



namespace QueryFirst
{
    class QueryProcessor
    {
        DTE dte;
        Document QueryDoc;
        string query;
        string BaseFilename;
        string genCsPathAndFilename;
        Tuple<string, string, string> NamespaceAndClassNames;
        string DBConnectionString;
        string[] codeLines;
        int codeLinesI;
        string[,] paramNamesAndTypes;
        string methodSignature;
        string callingArgs;
        System.Collections.Generic.List<QueryField> Columns;
        bool queryHasRun;
        ADOHelper hlpr;

        public QueryProcessor(Document queryDoc)
        {
            queryHasRun = false;
            QueryDoc = queryDoc;
            dte = queryDoc.DTE;
            query = File.ReadAllText(queryDoc.FullName);
            query = query.Replace("/*designTime", "--designTime");
            query = query.Replace("endDesignTime*/", "--endDesignTime");
            //WriteToOutput("\nprocessing " + queryDoc.FullName );
            // class name and namespace read from user's half of partial class.
            //QfClassName = Regex.Match(query, "(?im)^--QfClassName\\s*=\\s*(\\S+)").Groups[1].Value;
            //QfNamespace = Regex.Match(query, "(?im)^--QfNamespace\\s*=\\s*(\\S+)").Groups[1].Value;
            // doc.fullname started being lowercase ??
            BaseFilename = Path.GetFileNameWithoutExtension((string)queryDoc.ProjectItem.Properties.Item("FullPath").Value);
            string currDir = Path.GetDirectoryName(queryDoc.FullName);


            // leaving for now, but the template should have taken care of this.
            genCsPathAndFilename = currDir + "\\" + BaseFilename + ".gen.cs";
            if (!File.Exists(genCsPathAndFilename))
                File.Create(genCsPathAndFilename);
            if (GetItemByFilename(queryDoc.ProjectItem.Collection, genCsPathAndFilename) != null)
                queryDoc.ProjectItem.Collection.AddFromFile(genCsPathAndFilename);
            // copy namespace of generated partial class from user partial class
            var resultsClass = queryDoc.ProjectItem.ProjectItems.Item(BaseFilename + "Results.cs");
            NamespaceAndClassNames = GetNamespaceAndClassNames(resultsClass);
            hlpr = new ADOHelper();
            DBConnectionString = GetConnectionString();
        }

        static Tuple<string, string, string> GetNamespaceAndClassNames(ProjectItem item)
        {
            string _namespace = null;
            string _loaderClass = null;
            string _resultsClass = null;
            FileCodeModel model = item.FileCodeModel;
            foreach (CodeElement element in model.CodeElements)
            {
                if (element.Kind == vsCMElement.vsCMElementNamespace)
                {
                    _namespace = element.FullName;
                    foreach (CodeElement child in element.Children)
                    {
                        if (child.Kind == vsCMElement.vsCMElementClass)
                        {
                            // a bit complicated but here we go. If partial class name finishes with "Results",
                            // derive the loader class name by trimming "Results". Else derive the loader class name
                            // by adding "Request".
                            _resultsClass = child.Name;
                            if (_resultsClass.Length > 7 && _resultsClass.Substring(_resultsClass.Length - 7) == "Results")
                                _loaderClass = _resultsClass.Substring(0, _resultsClass.Length - 7);
                            else
                                _loaderClass = _resultsClass + "Request";
                            return new Tuple<string, string, string>(_namespace, _loaderClass, _resultsClass);
                        }
                    }
                }
            }
            return null;

        }
        string GetNameAndPathForManifestStream(Document doc)
        {
            string fullNameAndPath = (string)doc.ProjectItem.Properties.Item("FullPath").Value;
            EnvDTE.Project vsProject = doc.ProjectItem.ContainingProject;
            string QueryFilePath = fullNameAndPath.Substring(vsProject.Properties.Item("FullPath").Value.ToString().Length);
            return vsProject.Properties.Item("DefaultNamespace").Value.ToString() + '.' + QueryFilePath.Replace('\\', '.');
        }
        static string GetAssemblyPath(EnvDTE.Project vsProject)
        {
            string fullPath = vsProject.Properties.Item("FullPath").Value.ToString();
            string outputPath = vsProject.ConfigurationManager.ActiveConfiguration.Properties.Item("OutputPath").Value.ToString();
            string outputDir = Path.Combine(fullPath, outputPath);
            string outputFileName = vsProject.Properties.Item("OutputFileName").Value.ToString();
            string assemblyPath = Path.Combine(outputDir, outputFileName);
            return assemblyPath;
        }

        public void Process()
        {
            try
            {
                if (DBConnectionString == null)
                    return; // nothing to be done
                var SchemaTable = new System.Data.DataTable();
                // Execute query
                try
                {
                    Columns = hlpr.GetFields(DBConnectionString, query, ref SchemaTable);
                }
                catch (Exception ex)
                {
                    StringBuilder bldr = new StringBuilder();
                    bldr.AppendLine("Error running query.");
                    bldr.AppendLine();
                    bldr.AppendLine("/*The last attempt to run this query failed with the following error. This class is no longer synced with the query");
                    bldr.AppendLine("You can compile the class by deleting this error information, but it will likely generate runtime errors.");
                    bldr.AppendLine("-----------------------------------------------------------");
                    bldr.AppendLine(ex.Message);
                    bldr.AppendLine("-----------------------------------------------------------");
                    bldr.AppendLine(ex.StackTrace);
                    bldr.AppendLine("*/");
                    File.AppendAllText(genCsPathAndFilename, bldr.ToString());
                    throw;
                }
                queryHasRun = true;
                if (Columns != null && Columns.Count > 0)
                {
                    string Code;
                    //generate the class
                    Code = GenerateClass();
                    File.WriteAllText(genCsPathAndFilename, Code);
                }
                WriteToOutput(Environment.NewLine + "QueryFirst generated wrapper class for " + BaseFilename + ".sql");
            }
            catch (Exception ex)
            {
                WriteToOutput(Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

        private string GetConnectionString()
        {
            ConfigurationAccessor config = new ConfigurationAccessor(dte, null);
            //return config.AppSettings["DBStrConnect"].Value;
            return config.ConnectionStrings["QfDefaultConnection"].ConnectionString;
        }
        private string GenerateClass()
        {
            string[] properties = hlpr.GenerateCodeCS(ref Columns, NamespaceAndClassNames.Item3);

            //codeLines = new string[properties.Length + linesAbove + linesBelow];
            codeLines = new string[1000];
            codeLines[codeLinesI++] = "using System;";
            codeLines[codeLinesI++] = "using System.Data;";
            codeLines[codeLinesI++] = "using System.Data.SqlClient;";
            codeLines[codeLinesI++] = "using System.IO;";
            codeLines[codeLinesI++] = "using System.Collections.Generic;";
            codeLines[codeLinesI++] = "using System.Configuration;";
            codeLines[codeLinesI++] = "using System.Linq;";
            if (!string.IsNullOrEmpty(NamespaceAndClassNames.Item1))
            {
                codeLines[codeLinesI++] = "namespace " + NamespaceAndClassNames.Item1 + "{";
            }
            else codeLines[codeLinesI++] = "";
            codeLines[codeLinesI++] = "public class " + NamespaceAndClassNames.Item2 + "{";
            paramNamesAndTypes = extractParamNamesAndTypes(query);
            signatureAndCallingArgs(paramNamesAndTypes);
            // GetOne method definition
            WriteMethods();
            //WriteGetAllMethod();
            codeLines[codeLinesI++] = "}"; // Close query class

            //Copy in properties
            for (int j = 0; j < properties.Length; j++)
            {
                codeLines[codeLinesI++] = properties[j];
            }
            if (!string.IsNullOrEmpty(NamespaceAndClassNames.Item1))
                codeLines[codeLinesI++] = "}"; // Close namespace
            string Code = hlpr.StringArrayToText(codeLines);
            return Code;
        }


        private void WriteMethods()
        {
            char[] spaceComma = new char[] { ',', ' ' };
            // Execute method, without connection
            codeLines[codeLinesI++] = "public static List<" + NamespaceAndClassNames.Item3 + "> Execute(" + methodSignature.Trim(spaceComma) + "){";
            codeLines[codeLinesI++] = "using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings[\"QfDefaultConnection\"].ConnectionString))";
            codeLines[codeLinesI++] = "{";
            codeLines[codeLinesI++] = "conn.Open();";
            codeLines[codeLinesI++] = "return Execute(" + callingArgs + ").ToList();";
            codeLines[codeLinesI++] = "}";
            codeLines[codeLinesI++] = "}";
            // Execute method with connection
            codeLines[codeLinesI++] = "public static IEnumerable<" + NamespaceAndClassNames.Item3 + "> Execute(" + methodSignature + "SqlConnection conn){";
            codeLines[codeLinesI++] = "SqlCommand cmd = conn.CreateCommand();";
            codeLines[codeLinesI++] = "loadCommandText(cmd);";
            //string[,] paramNamesAndTypes = { { "fraisId", "int" }, { "myString", "string" } };
            int i = 0;
            while (!string.IsNullOrEmpty(paramNamesAndTypes[i, 0]))
            {
                codeLines[codeLinesI++] = "cmd.Parameters.AddWithValue(\"@" + paramNamesAndTypes[i, 0] + "\", " + paramNamesAndTypes[i, 0] + ");";
                i++;
            }
            codeLines[codeLinesI++] = "using (var reader = cmd.ExecuteReader())";
            codeLines[codeLinesI++] = "{";
            codeLines[codeLinesI++] = "while (reader.Read())";
            codeLines[codeLinesI++] = "{";
            codeLines[codeLinesI++] = "yield return Create(reader);";
            codeLines[codeLinesI++] = "}";
            codeLines[codeLinesI++] = "}";
            codeLines[codeLinesI++] = "}"; //close Execute() method
                                           // GetOne without connection
            codeLines[codeLinesI++] = "public static " + NamespaceAndClassNames.Item3 + " GetOne(" + methodSignature.Trim(spaceComma) + "){";
            codeLines[codeLinesI++] = "using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings[\"QfDefaultConnection\"].ConnectionString))";
            codeLines[codeLinesI++] = "{";
            codeLines[codeLinesI++] = "conn.Open();";
            codeLines[codeLinesI++] = "return GetOne(" + callingArgs + ");";
            codeLines[codeLinesI++] = "}";
            codeLines[codeLinesI++] = "}";
            // GetOne() with connection
            codeLines[codeLinesI++] = "public static " + NamespaceAndClassNames.Item3 + " GetOne(" + methodSignature + "SqlConnection conn)";
            codeLines[codeLinesI++] = "{";
            codeLines[codeLinesI++] = "var all = Execute(" + callingArgs + ");";
            codeLines[codeLinesI++] = "using (IEnumerator<" + NamespaceAndClassNames.Item3 + "> iter = all.GetEnumerator())";
            codeLines[codeLinesI++] = "{";
            codeLines[codeLinesI++] = "iter.MoveNext();";
            codeLines[codeLinesI++] = "return iter.Current;";
            codeLines[codeLinesI++] = "}";
            codeLines[codeLinesI++] = "}"; // close GetOne() method
                                           //ExecuteScalar without connection
            codeLines[codeLinesI++] = "public static " + Columns[0].DataType + " ExecuteScalar(" + methodSignature.Trim(spaceComma) + "){";
            codeLines[codeLinesI++] = "using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings[\"QfDefaultConnection\"].ConnectionString))";
            codeLines[codeLinesI++] = "{";
            codeLines[codeLinesI++] = "conn.Open();";
            codeLines[codeLinesI++] = "return ExecuteScalar(" + callingArgs + ");";
            codeLines[codeLinesI++] = "}";
            codeLines[codeLinesI++] = "}";
            // ExecuteScalar() with connection
            codeLines[codeLinesI++] = "public static " + Columns[0].DataType + " ExecuteScalar(" + methodSignature + "SqlConnection conn){";
            codeLines[codeLinesI++] = "SqlCommand cmd = conn.CreateCommand();";
            codeLines[codeLinesI++] = "loadCommandText(cmd);";
            codeLines[codeLinesI++] = "return (" + Columns[0].DataType + ")cmd.ExecuteScalar();";
            codeLines[codeLinesI++] = "}";
            // close ExecuteScalar()
            // Create() method
            codeLines[codeLinesI++] = "public static " + NamespaceAndClassNames.Item3 + " Create(IDataRecord record)";
            codeLines[codeLinesI++] = "{";
            codeLines[codeLinesI++] = "var returnVal = new " + NamespaceAndClassNames.Item3 + "();";
            int j = 0;
            foreach (var col in Columns)
            {
                codeLines[codeLinesI++] = "if(! (record[" + j + "] == null) && !( record[" + j + "] == DBNull.Value ))";
                codeLines[codeLinesI++] = "returnVal." + col.ColumnName + " =  (" + col.DataType + ")record[" + j++ + "];";
            }
            // call OnLoad method in user's half of partial class
            codeLines[codeLinesI++] = "returnVal.OnLoad();";
            codeLines[codeLinesI++] = "return returnVal;";

            codeLines[codeLinesI++] = "}"; // close method;
                                           // private load command text
            codeLines[codeLinesI++] = "private static void loadCommandText(SqlCommand cmd){";
            codeLines[codeLinesI++] = "Stream strm = typeof(" + NamespaceAndClassNames.Item3 + ").Assembly.GetManifestResourceStream(\"" + GetNameAndPathForManifestStream(QueryDoc) + "\");";
            codeLines[codeLinesI++] = "string queryText = new StreamReader(strm).ReadToEnd();";
            codeLines[codeLinesI++] = "cmd.CommandText = queryText;";
            codeLines[codeLinesI++] = "}"; // close method;

        }

        private void signatureAndCallingArgs(string[,] namesAndTypes)
        {
            StringBuilder sig = new StringBuilder();
            StringBuilder call = new StringBuilder();
            int i = 0;
            while (!string.IsNullOrEmpty(namesAndTypes[i, 0]))
            {
                sig.Append(namesAndTypes[i, 1] + ' ' + namesAndTypes[i, 0] + ", ");
                call.Append(namesAndTypes[i, 0] + ", ");
                i++;
            }
            //signature trailing comma trimmed in place if needed. 
            call.Append("conn"); // calling args always used to call overload with connection
            methodSignature = sig.ToString();
            callingArgs = call.ToString();
        }

        private static string[,] extractParamNamesAndTypes(string query)
        {
            if (Regex.IsMatch(query, "^\\s*select", RegexOptions.IgnoreCase | RegexOptions.Multiline))
            {
                int i = 0;
                string pattern = "declare[^;]*";
                string[,] returnVal = new string[10, 2];

                Match m = Regex.Match(query, pattern, RegexOptions.IgnoreCase);
                while (m.Success)
                {
                    string[] parts = m.Value.Split(' ');
                    returnVal[i, 0] = parts[1].Substring(1); //param name
                    if (parts[2].IndexOf('(') > 0)
                        returnVal[i, 1] = DBType2CSType(parts[2].Substring(0, parts[2].IndexOf('('))); //param type, withouth length
                    else
                        returnVal[i, 1] = DBType2CSType(parts[2]);
                    m = m.NextMatch();
                    i++;
                }
                return returnVal;
            }
            else if (Regex.IsMatch(query, "^\\s*exec", RegexOptions.IgnoreCase | RegexOptions.Multiline))
            {
                throw new NotImplementedException();
            }
            else
                throw new Exception("Unable to determine query or procedure. A line must start with SELECT or EXEC");
        }

        private static string DBType2CSType(string p)
        {
            switch (p.ToLower())
            {
                case "bigint":
                    return "long";
                case "binary":
                    return "byte[]";
                case "bit":
                    return "bool";
                case "date":
                case "datetime":
                case "datetime2":
                    return "DateTime";
                case "datetimeoffset":
                    return "DateTimeOffset";
                case "decimal":
                case "money":
                    return "decimal";
                case "float":
                    return "double";
                case "image":
                case "timestamp":
                    return "byte[]";
                case "int":
                    return "int";
                case "char":
                case "nchar":
                case "ntext":
                case "nvarchar":
                case "varchar":
                case "text":
                case "xml":
                    return "string";
                default:
                    throw new Exception("type not matched : " + p);

            }
        }
        // Doesn't recurse into folders. Prefer items.Item("")
        private static ProjectItem GetItemByFilename(ProjectItems items, string filename)
        {
            foreach (ProjectItem item in items)
            {
                for (short i = 0; i < item.FileCount; i++)
                {
                    if (item.FileNames[i].Equals(filename))
                        return item;
                }
            }
            return null;
        }
        public void WriteToOutput(string message)
        {
            Window window = dte.Windows.Item(EnvDTE.Constants.vsWindowKindOutput);
            OutputWindow outputWindow = (OutputWindow)window.Object;
            OutputWindowPane outputWindowPane = null;

            for (uint i = 1; i <= outputWindow.OutputWindowPanes.Count; i++)
            {
                if (outputWindow.OutputWindowPanes.Item(i).Name.Equals("QueryFirst", StringComparison.CurrentCultureIgnoreCase))
                {
                    outputWindowPane = outputWindow.OutputWindowPanes.Item(i);
                    break;
                }
            }

            if (outputWindowPane == null)
                outputWindowPane = outputWindow.OutputWindowPanes.Add("QueryFirst");

            outputWindowPane.OutputString(message);
        }
    }

    class AllQueries
    {

    }
}

