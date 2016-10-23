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
            try
            {
                if (ctx.DesignTimeConnectionString == null)
                {
                    LogToVSOutputWindow(@"QueryFirst would like to help you, but you need to tell it where your DB is.
    Add these lines to your app or web.config. The name QfDefaultConnection and SqlClient are important. The rest is up to you.
    <connectionStrings>
        <add name=""QfDefaultConnection"" connectionString=""Data Source = localhost; Initial Catalog = NORTHWND; Integrated Security = SSPI; "" providerName=""System.Data.SqlClient"" />
    </ connectionStrings >
");
                    return; // nothing to be done

                }
                var makeSelfTest = ctx.ProjectConfig.AppSettings["QfMakeSelfTest"] != null && bool.Parse(ctx.ProjectConfig.AppSettings["QfMakeSelfTest"].Value);
                if (!makeSelfTest)
                    LogToVSOutputWindow(@"If you would like QueryFirst to generate SelfTest() methods for your queries, add the following in your app settings...
    <add key=""QfMakeSelfTest"" value=""true"" />
    You will also need to add project references for QfSchemaTools and Xunit.
");

                // Execute query
                try
                {
                    ctx.Query.DiscoverParams();

                    ctx.ResultFields = ctx.Hlpr.GetFields(ctx.DesignTimeConnectionString, ctx.Query.Text);
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
                File.WriteAllText(ctx.GeneratedClassFullFilename, Code.ToString());
                LogToVSOutputWindow(Environment.NewLine + "QueryFirst generated wrapper class for " + ctx.BaseName + ".sql");
            }
            catch (Exception ex)
            {
                LogToVSOutputWindow(Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace);
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

