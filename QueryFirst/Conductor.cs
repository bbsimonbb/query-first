using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using TinyIoC;

//[assembly: InternalsVisibleTo("QueryFirstTests")]


namespace QueryFirst
{
    
    class Conductor
    {
        private State _state;
        private TinyIoCContainer _tiny;
        private VSOutputWindow _vsOutputWindow;
        private Document _queryDoc;
        private ProjectItem _item;
        private IProvider _provider;

        public Conductor(VSOutputWindow vsOutpuWindow)
        {
            _vsOutputWindow = vsOutpuWindow;
        }




        public void ProcessOneQuery(Document queryDoc)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            _tiny = TinyIoCContainer.Current;
            _state = new State();
            _queryDoc = queryDoc;
            _item = queryDoc.ProjectItem;


            new _1ProcessQueryPath().Go(_state, (string)queryDoc.ProjectItem.Properties.Item("FullPath").Value);

            // Test this! If I can get source control exclusions working, team members won't get the generated file.
            if (!File.Exists(_state._1GeneratedClassFullFilename))
            {
                var _ = File.Create(_state._1GeneratedClassFullFilename);
                _.Dispose();
            }                
            if (GetItemByFilename(queryDoc.ProjectItem, _state._1GeneratedClassFullFilename) != null)
                queryDoc.ProjectItem.Collection.AddFromFile(_state._1GeneratedClassFullFilename);

            // copy namespace of generated partial class from user partial class
            var userPartialClass = File.ReadAllText(_state._1UserPartialClassFullFilename);
            new _2ExtractNamesFromUserPartialClass().Go(_state, userPartialClass);

            var textDoc = ((TextDocument)queryDoc.Object());
            var start = textDoc.StartPoint;
            var text = start.CreateEditPoint().GetText(textDoc.EndPoint);
            new _3ReadQuery().Go(_state, text);
            var _4 = (_4ResolveConfig)_tiny.Resolve(typeof(_4ResolveConfig));
            _4.Go(_state);


            // We have the config, we can instantiate our provider...
            if (_tiny.CanResolve<IProvider>(_state._4Config.Provider))
                _provider = _tiny.Resolve<IProvider>(_state._4Config.Provider);
            else
                _vsOutputWindow.Write(@"After resolving the config, we have no provider\n");


            try
            {
                if (string.IsNullOrEmpty(_state._4Config.DefaultConnection))
                {
                    _vsOutputWindow.Write(@"No design time connection string. You need to create qfconfig.json beside or above your query 
or put --QfDefaultConnection=myConnectionString somewhere in your query file.
See the Readme section at https://marketplace.visualstudio.com/items?itemName=bbsimonbb.QueryFirst    
");
                    return; // nothing to be done

                }
                if (!_tiny.CanResolve<IProvider>(_state._4Config.Provider))
                {
                    _vsOutputWindow.Write(string.Format(
@"No Implementation of IProvider for providerName {0}. 
The query {1} may not run and the wrapper has not been regenerated.\n",
                    _state._4Config.Provider, _state._1BaseName
                    ));
                    return;
                }
                // Use QueryFirst within QueryFirst !
                // ToDo, to make this work with Postgres, store as ConnectionStringSettings with provider name.
                QfRuntimeConnection.CurrentConnectionString = _state._4Config.DefaultConnection;


                var matchInsert = Regex.Match(_state._3InitialQueryText, "^insert\\s+into\\s+(?<tableName>\\w+)\\.\\.\\.", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                var matchUpdate = Regex.Match(_state._3InitialQueryText, "^update\\s+(?<tableName>\\w+)\\.\\.\\.", RegexOptions.IgnoreCase | RegexOptions.Multiline);
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
                        ep.ReplaceText(_state._3InitialQueryText.Length, statement, 0);
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
                        ep.ReplaceText(_state._3InitialQueryText.Length, statement, 0);
                        //ctx.QueryDoc.Save();
                    }
                }
                else
                {

                    // Execute query
                    try
                    {
                        new _6FindUndeclaredParameters(_provider).Go(ref _state, out string outputMessage);
                        // if message returned, write it to output.
                        if(!string.IsNullOrEmpty(outputMessage))
                            _vsOutputWindow.Write(outputMessage);
                        // if undeclared params were found, add them to the .sql
                        if (!string.IsNullOrEmpty(_state._6NewParamDeclarations))
                        {
                            ReplacePattern("-- endDesignTime", _state._6NewParamDeclarations + "-- endDesignTime");
                        }

                        new _7RunQueryAndGetResultSchema(new AdoSchemaFetcher(), _provider).Go(ref _state);
                        new _8ParseOrFindDeclaredParams(_provider).Go(ref _state);
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
                        File.AppendAllText(_state._1GeneratedClassFullFilename, bldr.ToString());
                        throw;
                    }
                    StringBuilder Code = new StringBuilder();

                    var wrapper = _tiny.Resolve<IWrapperClassMaker>();
                    var results = _tiny.Resolve<IResultClassMaker>();

                    Code.Append(wrapper.StartNamespace(_state));
                    Code.Append(wrapper.Usings(_state));
                    if (_state._4Config.MakeSelfTest)
                        Code.Append(wrapper.SelfTestUsings(_state));
                    if (_state._7ResultFields != null && _state._7ResultFields.Count > 0)
                        Code.Append(results.Usings());
                    Code.Append(wrapper.MakeInterface(_state));
                    Code.Append(wrapper.StartClass(_state));
                    Code.Append(wrapper.MakeExecuteNonQueryWithoutConn(_state));
                    Code.Append(wrapper.MakeExecuteNonQueryWithConn(_state));
                    Code.Append(wrapper.MakeGetCommandTextMethod(_state));
                    Code.Append(_provider.MakeAddAParameter(_state));

                    if (_state._4Config.MakeSelfTest)
                        Code.Append(wrapper.MakeSelfTestMethod(_state));
                    if (_state._7ResultFields != null && _state._7ResultFields.Count > 0)
                    {
                        Code.Append(wrapper.MakeExecuteWithoutConn(_state));
                        Code.Append(wrapper.MakeExecuteWithConn(_state));
                        Code.Append(wrapper.MakeGetOneWithoutConn(_state));
                        Code.Append(wrapper.MakeGetOneWithConn(_state));
                        Code.Append(wrapper.MakeExecuteScalarWithoutConn(_state));
                        Code.Append(wrapper.MakeExecuteScalarWithConn(_state));

                        Code.Append(wrapper.MakeCreateMethod(_state));
                        Code.Append(wrapper.MakeOtherMethods(_state));
                        Code.Append(wrapper.CloseClass(_state));
                        Code.Append(results.StartClass(_state));
                        foreach (var fld in _state._7ResultFields)
                        {
                            Code.Append(results.MakeProperty(fld));
                        }
                    }
                    Code.Append(results.CloseClass()); // closes wrapper class if no results !
                    Code.Append(wrapper.CloseNamespace(_state));
                    //File.WriteAllText(ctx.GeneratedClassFullFilename, Code.ToString());
                    var genFile = GetItemByFilename(queryDoc.ProjectItem, _state._1GeneratedClassFullFilename);
                    WriteAndFormat(genFile, Code.ToString());
                    // what was this for ????
                    //var partialClassFile = GetItemByFilename(_ctx.QueryDoc.ProjectItem, _state._1CurrDir + _state._1BaseName + "Results.cs");
                    _vsOutputWindow.Write("QueryFirst generated wrapper class for " + _state._1BaseName + ".sql" + Environment.NewLine);
                }

            }
            catch (Exception ex)
            {
                _vsOutputWindow.Write(ex.TellMeEverything());
            }
        }
        private void WriteAndFormat(ProjectItem genFile, string code)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            bool rememberToClose = false;
            if (!genFile.IsOpen)
            {
                genFile.Open();
                rememberToClose = true;
            }
            var textDoc = ((TextDocument)genFile.Document.Object());
            var ep = textDoc.CreateEditPoint();
            ep.ReplaceText(textDoc.EndPoint, code, 0);
            ep.SmartFormat(textDoc.EndPoint);
            genFile.Save();
            if (rememberToClose)
            {
                genFile.Document.Close();
            }
        }

        // Doesn't recurse into folders. Prefer items.Item("")
        public static ProjectItem GetItemByFilename(ProjectItem item, string filename)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            foreach (ProjectItem childItem in item.ProjectItems)
            {
                for (short i = 0; i < childItem.FileCount; i++)
                {
                    if (childItem.FileNames[i].Equals(filename))
                        return childItem as ProjectItem;
                }
            }

            // .net core has a little problem with nested items.
            foreach (ProjectItem childItem in item.Collection)
            {
                if (childItem.FileNames[0] == filename)
                {
                    return childItem as ProjectItem;
                }
            }
            return null;
        }
        private void ReplacePattern(string pattern, string replaceWith)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var textDoc = ((TextDocument)_queryDoc.Object());
            textDoc.ReplacePattern(pattern, replaceWith);
        }
    }
}

