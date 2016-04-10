using System.Text;
using System.IO;
using System;
using System.Data;
using System.Data.SqlClient;
//using Extensibility;
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
        string classFilename;
        string QfClassName;
        string QfNamespace;
        string myResultsClass;
        string myNamespace;
        string codeFile;
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
            WriteToOutput("processing " + queryDoc.FullName + "\n");
            QfClassName = Regex.Match(query, "(?im)^--QfClassName\\s*=\\s*(\\S+)").Groups[1].Value;
            QfNamespace = Regex.Match(query, "(?im)^--QfNamespace\\s*=\\s*(\\S+)").Groups[1].Value;
            classFilename = Path.GetFileNameWithoutExtension(queryDoc.FullName);
            string currDir = Path.GetDirectoryName(queryDoc.FullName);

            // leaving for now, but the template should have taken care of this.
            codeFile = currDir + "\\" + classFilename + ".cs";
            if (!File.Exists(codeFile))
                File.Create(codeFile);
            if (GetItemByFilename(queryDoc.ProjectItem.Collection, codeFile) != null)
                queryDoc.ProjectItem.Collection.AddFromFile(codeFile);
            // copy namespace of generated partial class from user partial class
            var resultsClass = queryDoc.ProjectItem.ProjectItems.Item(classFilename + "Results.cs");
            myNamespace = GetTopLevelNamespace(resultsClass).FullName;
            hlpr = new ADOHelper();
            DBConnectionString = GetConnectionString();
        }


        public CodeElement GetTopLevelNamespace(ProjectItem item)
        {
            FileCodeModel model = item.FileCodeModel;
            foreach (CodeElement element in model.CodeElements)
            {
                if (element.Kind == vsCMElement.vsCMElementNamespace)
                {
                    return element;
                }
            }
            return null;
        }
        string GetPathForManifestStream(Document doc)
        {
            EnvDTE.Project vsProject = doc.ProjectItem.ContainingProject;
            string QueryFilePath = doc.Path.Substring(vsProject.Properties.Item("FullPath").Value.ToString().Length);
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
                    File.AppendAllText(codeFile, bldr.ToString());
                    throw;
                }
                queryHasRun = true;
                if (Columns != null && Columns.Count > 0)
                {
                    string Code;
                    //generate the class
                    Code = GenerateClass();
                    File.WriteAllText(codeFile, Code);
                }
                WriteToOutput(Environment.NewLine + "QueryFirst generated wrapper class for " + classFilename + ".sql");
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
            myResultsClass = QfClassName + "Results";
            string[] properties = hlpr.GenerateCodeCS(ref Columns, myResultsClass);

            //codeLines = new string[properties.Length + linesAbove + linesBelow];
            codeLines = new string[1000];
            codeLines[codeLinesI++] = "using System;";
            codeLines[codeLinesI++] = "using System.Data;";
            codeLines[codeLinesI++] = "using System.Data.SqlClient;";
            codeLines[codeLinesI++] = "using System.IO;";
            codeLines[codeLinesI++] = "using System.Reflection;";
            codeLines[codeLinesI++] = "using System.Collections.Generic;";
            codeLines[codeLinesI++] = "using System.Configuration;";
            if (!string.IsNullOrEmpty(myNamespace))
            {
                codeLines[codeLinesI++] = "namespace " + myNamespace + "{";
            }
            else codeLines[codeLinesI++] = "";
            codeLines[codeLinesI++] = "public class " + QfClassName + "{";
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

            codeLines[codeLinesI++] = "}"; // Close namespace
            string Code = hlpr.StringArrayToText(codeLines);
            return Code;
        }


        private void WriteMethods()
        {
            char[] spaceComma = new char[] { ',', ' ' };
            // Execute method, without connection
            codeLines[codeLinesI++] = "public static IEnumerable<" + myResultsClass + "> Execute(" + methodSignature.Trim(spaceComma) + "){";
            codeLines[codeLinesI++] = "using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings[\"QfDefaultConnection\"].ConnectionString))";
            codeLines[codeLinesI++] = "{";
            codeLines[codeLinesI++] = "conn.Open();";
            codeLines[codeLinesI++] = "return Execute(" + callingArgs + ");";
            codeLines[codeLinesI++] = "}";
            codeLines[codeLinesI++] = "}";
            // Execute method with connection
            codeLines[codeLinesI++] = "public static IEnumerable<" + myResultsClass + "> Execute(" + methodSignature + "SqlConnection conn){";
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
            codeLines[codeLinesI++] = "public static " + myResultsClass + " GetOne(" + methodSignature.Trim(spaceComma) + "){";
            codeLines[codeLinesI++] = "using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings[\"QfDefaultConnection\"].ConnectionString))";
            codeLines[codeLinesI++] = "{";
            codeLines[codeLinesI++] = "conn.Open();";
            codeLines[codeLinesI++] = "return GetOne(" + callingArgs + ");";
            codeLines[codeLinesI++] = "}";
            codeLines[codeLinesI++] = "}";
            // GetOne() with connection
            codeLines[codeLinesI++] = "public static " + myResultsClass + " GetOne(" + methodSignature + "SqlConnection conn)";
            codeLines[codeLinesI++] = "{";
            codeLines[codeLinesI++] = "var all = Execute(" + callingArgs + ");";
            codeLines[codeLinesI++] = "using (IEnumerator<" + myResultsClass + "> iter = all.GetEnumerator())";
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
            // Create() method called by RuntimeLoader
            codeLines[codeLinesI++] = "public static " + myResultsClass + " Create(IDataRecord record)";
            codeLines[codeLinesI++] = "{";
            codeLines[codeLinesI++] = "var returnVal = new " + myResultsClass + "();";
            int j = 0;
            foreach (var col in Columns)
            {
                codeLines[codeLinesI++] = "if(! (record[" + j + "] == null) && !( record[" + j + "] == DBNull.Value ))";
                codeLines[codeLinesI++] = "returnVal." + col.ColumnName + " =  (" + col.DataType + ")record[" + j++ + "];";
            }
            codeLines[codeLinesI++] = "return returnVal;";

            codeLines[codeLinesI++] = "}"; // close method;
            // private load command text
            codeLines[codeLinesI++] = "private static void loadCommandText(SqlCommand cmd){";
            //codeLines[codeLinesI++] = "Stream strm = Assembly.GetExecutingAssembly().GetManifestResourceStream(MethodBase.GetCurrentMethod().DeclaringType.Namespace + \"." + classFilename + ".sql\");";
            codeLines[codeLinesI++] = "Stream strm = Assembly.GetExecutingAssembly().GetManifestResourceStream(\"" + GetPathForManifestStream(QueryDoc) + classFilename + ".sql\");";
            codeLines[codeLinesI++] = "string queryText = new StreamReader(strm).ReadToEnd();";
            codeLines[codeLinesI++] = "queryText = queryText.Replace(\"--designTime\", \"/*designTime\");";
            codeLines[codeLinesI++] = "queryText = queryText.Replace(\"--endDesignTime\", \"endDesignTime*/\");";
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
        //private static string buildExecuteCall(string[,] namesAndTypes)
        //{
        //    StringBuilder bldr = new StringBuilder("conn, ");
        //    int i = 0;
        //    while (!string.IsNullOrEmpty(namesAndTypes[i, 0]))
        //    {
        //        bldr.Append(namesAndTypes[i, 0] + ", ");
        //        i++;
        //    }
        //    bldr.Length = bldr.Length - 2; // trim trailing comma and space.
        //    return bldr.ToString();
        //}
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

