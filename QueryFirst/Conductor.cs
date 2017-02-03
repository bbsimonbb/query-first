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
using TinyIoC;



namespace QueryFirst
{
    class Conductor
    {
        private CodeGenerationContext ctx;
        private TinyIoCContainer _tiny;

        public Conductor(Document queryDoc)
        {
            ctx = new CodeGenerationContext(queryDoc);
            _tiny = TinyIoCContainer.Current;

            // Test this! If I can get source control exclusions working, team members won't get the generated file.

            if (!File.Exists(ctx.GeneratedClassFullFilename))
                File.Create(ctx.GeneratedClassFullFilename);
            if (GetItemByFilename(queryDoc.ProjectItem.Collection, ctx.GeneratedClassFullFilename) != null)
                queryDoc.ProjectItem.Collection.AddFromFile(ctx.GeneratedClassFullFilename);
            // copy namespace of generated partial class from user partial class

        }
        public bool IsQFQuery()
        {
            return ctx.Query.IsQFQuery();
        }



        public void Process()
        {
            // backward compatible...
            var textDoc = ((TextDocument)ctx.QueryDoc.Object());
            textDoc.ReplacePattern("--designTime", "-- designTime");
            textDoc.ReplacePattern("--endDesignTime", "-- endDesignTime");
            try
            {
                if (!ctx.DesignTimeConnectionString.IsPresent)
                {
                    LogToVSOutputWindow(@"QueryFirst would like to help you, but you need to tell it where your DB is.
    You can specify the design time connection string in your app or web.config or directly in the query file.
    Add these lines to your app or web.config. providerName should be one of System.Data.SqlClient, Npgsql MySql.Data.MySqlClient.
    <connectionStrings>
        <add name=""QfDefaultConnection"" connectionString=""Data Source = localhost; Initial Catalog = NORTHWND; Integrated Security = SSPI; "" providerName=""System.Data.SqlClient"" />
    </ connectionStrings >
    or put --QfDefaultConnection=myConnectionString somewhere in your query file.
");
                    return; // nothing to be done

                }
                if (!ctx.DesignTimeConnectionString.IsProviderValid)
                {
                    LogToVSOutputWindow(string.Format(
@"No Implementation of IProvider for providerName {0}. 
The query {1} may not run and the wrapper has not been regenerated.",
                    ctx.DesignTimeConnectionString.v.ProviderName, ctx.BaseName
                    ));
                }
                // Use QueryFirst within QueryFirst !
                // ToDo, to make this work with Postgres, store as ConnectionStringSettings with provider name.
                QfRuntimeConnection.CurrentConnectionString = ctx.DesignTimeConnectionString.v.ConnectionString;

                var makeSelfTest = ctx.ProjectConfig?.AppSettings["QfMakeSelfTest"] != null && bool.Parse(ctx.ProjectConfig.AppSettings["QfMakeSelfTest"].Value);

                var matchInsert = Regex.Match(ctx.Query.Text, "^insert\\s+into\\s+(?<tableName>\\w+)\\.\\.\\.", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                var matchUpdate = Regex.Match(ctx.Query.Text, "^update\\s+(?<tableName>\\w+)\\.\\.\\.", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                if (matchInsert.Success)
                {
                    var statement = new ScaffoldInsert().ExecuteScalar(matchInsert.Groups["tableName"].Value);
                    var ep = textDoc.CreateEditPoint();
                    ep.ReplaceText(ctx.Query.Text.Length, statement, 0);
                    //ctx.QueryDoc.Save();
                }
                else if (matchUpdate.Success)
                {
                    var statement = new ScaffoldUpdate().ExecuteScalar(matchUpdate.Groups["tableName"].Value);
                    var ep = textDoc.CreateEditPoint();
                    ep.ReplaceText(ctx.Query.Text.Length, statement, 0);
                    //ctx.QueryDoc.Save();
                }
                else
                {

                    // Execute query
                    try
                    {
                        // also called in the bowels of schema fetching, for Postgres, because no notion of declarations.
                        try
                        {
                            var undeclared = ctx.Provider.FindUndeclaredParameters(ctx.Query.Text);
                            var newParamDeclarations = ctx.Provider.ConstructParameterDeclarations(undeclared);
                            if (!string.IsNullOrEmpty(newParamDeclarations))
                            {
                                ctx.Query.ReplacePattern("-- endDesignTime", newParamDeclarations + "-- endDesignTime");
                            }
                        }
                        catch (SqlException ex)
                        {
                            LogToVSOutputWindow("Unable to find undeclared parameters. You will have to do this yourself.");
                        }

                        ctx.ResultFields = ctx.Hlpr.GetFields(ctx.DesignTimeConnectionString.v, ctx.Query.Text);
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
                        File.AppendAllText(ctx.GeneratedClassFullFilename, bldr.ToString());
                        throw;
                    }
                    ctx.QueryHasRun = true;
                    StringBuilder Code = new StringBuilder();

                    var wrapper = _tiny.Resolve<IWrapperClassMaker>();
                    var results = _tiny.Resolve<IResultClassMaker>();

                    Code.Append(wrapper.StartNamespace(ctx));
                    Code.Append(wrapper.Usings(ctx));
                    if (makeSelfTest)
                        Code.Append(wrapper.SelfTestUsings(ctx));
                    if (ctx.ResultFields != null && ctx.ResultFields.Count > 0)
                        Code.Append(results.Usings());
                    Code.Append(wrapper.MakeInterface(ctx));
                    Code.Append(wrapper.StartClass(ctx));
                    Code.Append(wrapper.MakeExecuteNonQueryWithoutConn(ctx));
                    Code.Append(wrapper.MakeExecuteNonQueryWithConn(ctx));
                    Code.Append(wrapper.MakeGetCommandTextMethod(ctx));
                    Code.Append(ctx.Provider.MakeAddAParameter(ctx));

                    if (makeSelfTest)
                        Code.Append(wrapper.MakeSelfTestMethod(ctx));
                    if (ctx.ResultFields != null && ctx.ResultFields.Count > 0)
                    {
                        Code.Append(wrapper.MakeExecuteWithoutConn(ctx));
                        Code.Append(wrapper.MakeExecuteWithConn(ctx));
                        Code.Append(wrapper.MakeGetOneWithoutConn(ctx));
                        Code.Append(wrapper.MakeGetOneWithConn(ctx));
                        Code.Append(wrapper.MakeExecuteScalarWithoutConn(ctx));
                        Code.Append(wrapper.MakeExecuteScalarWithConn(ctx));

                        Code.Append(wrapper.MakeCreateMethod(ctx));
                        Code.Append(wrapper.MakeOtherMethods(ctx));
                        Code.Append(wrapper.CloseClass(ctx));
                        Code.Append(results.StartClass(ctx));
                        foreach (var fld in ctx.ResultFields)
                        {
                            Code.Append(results.MakeProperty(fld));
                        }
                    }
                    Code.Append(results.CloseClass()); // closes wrapper class if no results !
                    Code.Append(wrapper.CloseNamespace(ctx));
                    //File.WriteAllText(ctx.GeneratedClassFullFilename, Code.ToString());
                    ctx.PutCodeHere.WriteAndFormat(Code.ToString());
                    var partialClassFile = GetItemByFilename(ctx.QueryDoc.ProjectItem.ProjectItems, ctx.CurrDir + ctx.BaseName + "Results.cs");
                    new BackwardCompatibility().InjectPOCOFactory(ctx, partialClassFile);
                    LogToVSOutputWindow(Environment.NewLine + "QueryFirst generated wrapper class for " + ctx.BaseName + ".sql");
                }

            }
            catch (Exception ex)
            {
                LogToVSOutputWindow(ex.TellMeEverything());
            }
        }

        // Doesn't recurse into folders. Prefer items.Item("")
        public static ProjectItem GetItemByFilename(ProjectItems items, string filename)
        {
            foreach (ProjectItem item in items)
            {
                for (short i = 0; i < item.FileCount; i++)
                {
                    if (item.FileNames[i].Equals(filename))
                        return item as ProjectItem;
                }
            }
            return null;
        }
        public void LogToVSOutputWindow(string message)
        {
            Window window = ctx.Dte.Windows.Item(EnvDTE.Constants.vsWindowKindOutput);
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
}

