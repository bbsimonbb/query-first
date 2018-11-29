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
        private TinyIoCContainer _tiny;
        private VSOutputWindow _vsOutputWindow;
        private ICodeGenerationContext _ctx;

        public Conductor(VSOutputWindow vsOutpuWindow,ICodeGenerationContext ctx)
        {
            _vsOutputWindow = vsOutpuWindow;
            _ctx = ctx;
        }




        public void ProcessOneQuery(Document queryDoc)
        {
            _tiny = TinyIoCContainer.Current;
            _ctx.InitForQuery(queryDoc);
            // Test this! If I can get source control exclusions working, team members won't get the generated file.

            if (!File.Exists(_ctx.GeneratedClassFullFilename))
                File.Create(_ctx.GeneratedClassFullFilename);
            if (GetItemByFilename(queryDoc.ProjectItem.Collection, _ctx.GeneratedClassFullFilename) != null)
                queryDoc.ProjectItem.Collection.AddFromFile(_ctx.GeneratedClassFullFilename);
            // copy namespace of generated partial class from user partial class
            // backward compatible...
            var textDoc = ((TextDocument)_ctx.QueryDoc.Object());
            textDoc.ReplacePattern("--designTime", "-- designTime");
            textDoc.ReplacePattern("--endDesignTime", "-- endDesignTime");
            try
            {
                if (string.IsNullOrEmpty(_ctx.Config.DefaultConnection))
                {
                    _vsOutputWindow.Write(@"QueryFirst would like to help you, but you need to tell it where your DB is.
Breaking change in 1.0.0: QueryFirst now has it's own config file. You need to create qfconfig.json beside or above your query 
or put --QfDefaultConnection=myConnectionString somewhere in your query file.
See the Readme section at https://marketplace.visualstudio.com/items?itemName=bbsimonbb.QueryFirst    
");
                    return; // nothing to be done

                }
                if (!_tiny.CanResolve<IProvider>(_ctx.Config.Provider))
                {
                    _vsOutputWindow.Write(string.Format(
@"No Implementation of IProvider for providerName {0}. 
The query {1} may not run and the wrapper has not been regenerated.",
                    _ctx.Config.Provider, _ctx.BaseName
                    ));
                    return;
                }
                // Use QueryFirst within QueryFirst !
                // ToDo, to make this work with Postgres, store as ConnectionStringSettings with provider name.
                QfRuntimeConnection.CurrentConnectionString = _ctx.Config.DefaultConnection;


                var matchInsert = Regex.Match(_ctx.Query.Text, "^insert\\s+into\\s+(?<tableName>\\w+)\\.\\.\\.", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                var matchUpdate = Regex.Match(_ctx.Query.Text, "^update\\s+(?<tableName>\\w+)\\.\\.\\.", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                if (matchInsert.Success)
                {
                    var statement = new ScaffoldInsert().ExecuteScalar(matchInsert.Groups["tableName"].Value);
                    if (string.IsNullOrEmpty(statement))
                    {
                        _vsOutputWindow.Write("Unknown problem generating insert.\n");

                    }
                    else
                    {
                        var ep = textDoc.CreateEditPoint();
                        ep.ReplaceText(_ctx.Query.Text.Length, statement, 0);
                        //ctx.QueryDoc.Save();
                    }

                }
                else if (matchUpdate.Success)
                {
                    var statement = new ScaffoldUpdate().ExecuteScalar(matchUpdate.Groups["tableName"].Value);
                    if (string.IsNullOrEmpty(statement))
                    {
                        _vsOutputWindow.Write("Unknown problem generating update.\n");

                    }
                    else
                    {
                        var ep = textDoc.CreateEditPoint();
                        ep.ReplaceText(_ctx.Query.Text.Length, statement, 0);
                        //ctx.QueryDoc.Save();
                    }
                }
                else
                {

                    // Execute query
                    try
                    {
                        // also called in the bowels of schema fetching, for Postgres, because no notion of declarations.
                        try
                        {
                            var undeclared = _ctx.Provider.FindUndeclaredParameters(_ctx.Query.Text, _ctx.Config.DefaultConnection);
                            var newParamDeclarations = _ctx.Provider.ConstructParameterDeclarations(undeclared);
                            if (!string.IsNullOrEmpty(newParamDeclarations))
                            {
                                _ctx.Query.ReplacePattern("-- endDesignTime", newParamDeclarations + "-- endDesignTime");
                            }
                        }
                        catch (SqlException ex)
                        {
                            if (ex.Message.Contains("sp_describe_undeclared_parameters"))
                                _vsOutputWindow.Write("Unable to find undeclared parameters. You will have to do this yourself.\n");
                            else throw;
                        }

                        _ctx.ResultFields = _ctx.SchemaFetcher.GetFields(_ctx.Config.DefaultConnection, _ctx.Config.Provider, _ctx.Query.Text);
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
                        File.AppendAllText(_ctx.GeneratedClassFullFilename, bldr.ToString());
                        throw;
                    }
                    _ctx.QueryHasRun = true;
                    StringBuilder Code = new StringBuilder();

                    var wrapper = _tiny.Resolve<IWrapperClassMaker>();
                    var results = _tiny.Resolve<IResultClassMaker>();

                    Code.Append(wrapper.StartNamespace(_ctx));
                    Code.Append(wrapper.Usings(_ctx));
                    if (_ctx.Config.MakeSelfTest)
                        Code.Append(wrapper.SelfTestUsings(_ctx));
                    if (_ctx.ResultFields != null && _ctx.ResultFields.Count > 0)
                        Code.Append(results.Usings());
                    Code.Append(wrapper.MakeInterface(_ctx));
                    Code.Append(wrapper.StartClass(_ctx));
                    Code.Append(wrapper.MakeExecuteNonQueryWithoutConn(_ctx));
                    Code.Append(wrapper.MakeExecuteNonQueryWithConn(_ctx));
                    Code.Append(wrapper.MakeGetCommandTextMethod(_ctx));
                    Code.Append(_ctx.Provider.MakeAddAParameter(_ctx));

                    if (_ctx.Config.MakeSelfTest)
                        Code.Append(wrapper.MakeSelfTestMethod(_ctx));
                    if (_ctx.ResultFields != null && _ctx.ResultFields.Count > 0)
                    {
                        Code.Append(wrapper.MakeExecuteWithoutConn(_ctx));
                        Code.Append(wrapper.MakeExecuteWithConn(_ctx));
                        Code.Append(wrapper.MakeGetOneWithoutConn(_ctx));
                        Code.Append(wrapper.MakeGetOneWithConn(_ctx));
                        Code.Append(wrapper.MakeExecuteScalarWithoutConn(_ctx));
                        Code.Append(wrapper.MakeExecuteScalarWithConn(_ctx));

                        Code.Append(wrapper.MakeCreateMethod(_ctx));
                        Code.Append(wrapper.MakeOtherMethods(_ctx));
                        Code.Append(wrapper.CloseClass(_ctx));
                        Code.Append(results.StartClass(_ctx));
                        foreach (var fld in _ctx.ResultFields)
                        {
                            Code.Append(results.MakeProperty(fld));
                        }
                    }
                    Code.Append(results.CloseClass()); // closes wrapper class if no results !
                    Code.Append(wrapper.CloseNamespace(_ctx));
                    //File.WriteAllText(ctx.GeneratedClassFullFilename, Code.ToString());
                    _ctx.PutCodeHere.WriteAndFormat(Code.ToString());
                    var partialClassFile = GetItemByFilename(_ctx.QueryDoc.ProjectItem.ProjectItems, _ctx.CurrDir + _ctx.BaseName + "Results.cs");
                    if (partialClassFile == null)
                    {
                        // .net core has a little problem with nested items.
                        partialClassFile = GetItemByFilename(_ctx.QueryDoc.ProjectItem.ContainingProject.ProjectItems, _ctx.CurrDir + _ctx.BaseName + "Results.cs");
                    }
                    new BackwardCompatibility().InjectPOCOFactory(_ctx, partialClassFile);
                    _vsOutputWindow.Write(Environment.NewLine + "QueryFirst generated wrapper class for " + _ctx.BaseName + ".sql");
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

