using EnvDTE;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using TinyIoC;



namespace QueryFirst
{
    class Conductor
    {
        private CodeGenerationContext ctx;
        private TinyIoCContainer _tiny;
        private VSOutputWindow _vsOutputWindow;

        public Conductor(VSOutputWindow vsOutpuWindow)
        {
            _vsOutputWindow = vsOutpuWindow;
        }




        public void ProcessOneQuery(Document queryDoc)
        {
            _tiny = TinyIoCContainer.Current;
            ctx = new CodeGenerationContext(queryDoc);

            // Test this! If I can get source control exclusions working, team members won't get the generated file.

            if (!File.Exists(ctx.GeneratedClassFullFilename))
                File.Create(ctx.GeneratedClassFullFilename);
            if (GetItemByFilename(queryDoc.ProjectItem.Collection, ctx.GeneratedClassFullFilename) != null)
                queryDoc.ProjectItem.Collection.AddFromFile(ctx.GeneratedClassFullFilename);
            // copy namespace of generated partial class from user partial class
            // backward compatible...
            var textDoc = ((TextDocument)ctx.QueryDoc.Object());
            textDoc.ReplacePattern("--designTime", "-- designTime");
            textDoc.ReplacePattern("--endDesignTime", "-- endDesignTime");
            try
            {
                if (!ctx.DesignTimeConnectionString.IsPresent)
                {
                    _vsOutputWindow.Write(@"QueryFirst would like to help you, but you need to tell it where your DB is.
Breaking change in 1.0.0: QueryFirst now has it's own config file. You need to create qfconfig.json beside or above your query 
or put --QfDefaultConnection=myConnectionString somewhere in your query file.
See the Readme section at https://marketplace.visualstudio.com/items?itemName=bbsimonbb.QueryFirst    
");
                    return; // nothing to be done

                }
                if (!ctx.DesignTimeConnectionString.IsProviderValid)
                {
                    _vsOutputWindow.Write(string.Format(
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
                            if (ex.Message.Contains("sp_describe_undeclared_parameters"))
                                _vsOutputWindow.Write("Unable to find undeclared parameters. You will have to do this yourself.\n");
                            else throw;
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
                    _vsOutputWindow.Write(Environment.NewLine + "QueryFirst generated wrapper class for " + ctx.BaseName + ".sql");
                }

            }
            catch (Exception ex)
            {
                _vsOutputWindow.Write(ex.TellMeEverything());
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
    }
}

