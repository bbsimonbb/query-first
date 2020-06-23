using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using TinyIoC;

//[assembly: InternalsVisibleTo("QueryFirstTests")]


namespace QueryFirst
{
    public class Conductor
    {
        private State _state;
        private TinyIoCContainer _tiny;
        private VSOutputWindow _vsOutputWindow;
        private Document _queryDoc;
        private ProjectItem _item;
        private IProvider _provider;
        private IWrapperClassMaker _wrapper;
        private IResultClassMaker _results;

        public Conductor(VSOutputWindow vSOutputWindow, IWrapperClassMaker wrapperClassMaker, IResultClassMaker resultClassMaker)
        {
            _tiny = TinyIoCContainer.Current;
            _wrapper = wrapperClassMaker ?? _tiny.Resolve<IWrapperClassMaker>();
            _results = resultClassMaker ?? _tiny.Resolve<IResultClassMaker>();

            _vsOutputWindow = vSOutputWindow;
        }

        public void ProcessOneQuery(Document queryDoc)
        {
            try
            {
                ThreadHelper.ThrowIfNotOnUIThread();

                _state = new State();
                _queryDoc = queryDoc;
                _item = queryDoc.ProjectItem;

                ProcessUpToStep4(queryDoc, ref _state);

                // Test this! If I can get source control exclusions working, team members won't get the generated file.
                if (!File.Exists(_state._1GeneratedClassFullFilename))
                {
                    var _ = File.Create(_state._1GeneratedClassFullFilename);
                    _.Dispose();
                }
                if (GetItemByFilename(queryDoc.ProjectItem, _state._1GeneratedClassFullFilename) != null)
                    queryDoc.ProjectItem.Collection.AddFromFile(_state._1GeneratedClassFullFilename);

                // We have the config, we can instantiate our provider...
                if (_tiny.CanResolve<IProvider>(_state._4Config.provider))
                    _provider = _tiny.Resolve<IProvider>(_state._4Config.provider);
                else
                    _vsOutputWindow.Write(@"After resolving the config, we have no provider\n");



                if (string.IsNullOrEmpty(_state._4Config.defaultConnection))
                {
                    _vsOutputWindow.Write(@"No design time connection string. You need to create qfconfig.json beside or above your query 
or put --QfDefaultConnection=myConnectionString somewhere in your query file.
See the Readme section at https://marketplace.visualstudio.com/items?itemName=bbsimonbb.QueryFirst    
");
                    return; // nothing to be done

                }
                if (!_tiny.CanResolve<IProvider>(_state._4Config.provider))
                {
                    _vsOutputWindow.Write(string.Format(
@"No Implementation of IProvider for providerName {0}. 
The query {1} may not run and the wrapper has not been regenerated.\n",
                    _state._4Config.provider, _state._1BaseName
                    ));
                    return;
                }
                // Scaffold inserts and updates
                _tiny.Resolve<_5ScaffoldUpdateOrInsert>().Go(ref _state);

                if (_state._3InitialQueryText != _state._5QueryAfterScaffolding)
                {
                    var textDoc = ((TextDocument)queryDoc.Object());
                    var ep = textDoc.CreateEditPoint();
                    ep.ReplaceText(_state._3InitialQueryText.Length, _state._5QueryAfterScaffolding, 0);
                }


                // Execute query
                try
                {
                    new _6FindUndeclaredParameters(_provider).Go(ref _state, out string outputMessage);
                    // if message returned, write it to output.
                    if (!string.IsNullOrEmpty(outputMessage))
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

                // dump state for reproducing issues
#if DEBUG
                using (var ms = new MemoryStream())
                {
                    var ser = new DataContractJsonSerializer(typeof(State));
                    ser.WriteObject(ms, _state);
                    byte[] json = ms.ToArray();
                    ms.Close();
                    File.WriteAllText(_state._1CurrDir + "qfDumpState.json", Encoding.UTF8.GetString(json, 0, json.Length));
                }
#endif

                var code = GenerateCode(_state);
                //File.WriteAllText(ctx.GeneratedClassFullFilename, Code.ToString());
                var genFile = GetItemByFilename(queryDoc.ProjectItem, _state._1GeneratedClassFullFilename);
                WriteAndFormat(genFile, code);
                // what was this for ????
                //var partialClassFile = GetItemByFilename(_ctx.QueryDoc.ProjectItem, _state._1CurrDir + _state._1BaseName + "Results.cs");
                _vsOutputWindow.Write("QueryFirst generated wrapper class for " + _state._1BaseName + ".sql" + Environment.NewLine);

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
        /// <summary>
        /// Now we can connect the editor window, we need to recover the connection string when we open a query.
        /// This method is called on open and on save.
        /// </summary>
        /// <param name="queryDoc"></param>
        /// <param name="state"></param>
        internal void ProcessUpToStep4(Document queryDoc, ref State state)
        {
            // todo: if a .sql is not in the project, this throws null exception. What should it do?
            new _1ProcessQueryPath().Go(state, (string)queryDoc.ProjectItem.Properties.Item("FullPath").Value);

            // copy namespace of generated partial class from user partial class
            var userPartialClass = File.ReadAllText(state._1UserPartialClassFullFilename);
            new _2ExtractNamesFromUserPartialClass().Go(state, userPartialClass);

            var textDoc = ((TextDocument)queryDoc.Object());
            var start = textDoc.StartPoint;
            var text = start.CreateEditPoint().GetText(textDoc.EndPoint);
            new _3ReadQuery().Go(state, text);
            var _4 = (_4ResolveConfig)_tiny.Resolve(typeof(_4ResolveConfig));
            _4.Go(state);
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

        public string GenerateCode(State state)
        {
            StringBuilder Code = new StringBuilder();

            Code.Append(_wrapper.StartNamespace(state));
            Code.Append(_wrapper.Usings(state));
            if (state._4Config.makeSelfTest)
                Code.Append(_wrapper.SelfTestUsings(state));
            if (state._7ResultFields != null && state._7ResultFields.Count > 0)
                Code.Append(_results.Usings());
            Code.Append(_wrapper.MakeInterface(state));
            Code.Append(_wrapper.StartClass(state));
            Code.Append(_wrapper.MakeExecuteNonQueryWithoutConn(state));
            Code.Append(_wrapper.MakeExecuteNonQueryWithConn(state));
            Code.Append(_wrapper.MakeGetCommandTextMethod(state));
            //Code.Append(_provider.MakeAddAParameter(state));
            Code.Append(_wrapper.MakeTvpPocos(state));

            if (state._4Config.makeSelfTest)
                Code.Append(_wrapper.MakeSelfTestMethod(state));
            if (state._7ResultFields != null && state._7ResultFields.Count > 0)
            {
                Code.Append(_wrapper.MakeExecuteWithoutConn(state));
                Code.Append(_wrapper.MakeExecuteWithConn(state));
                Code.Append(_wrapper.MakeGetOneWithoutConn(state));
                Code.Append(_wrapper.MakeGetOneWithConn(state));
                Code.Append(_wrapper.MakeExecuteScalarWithoutConn(state));
                Code.Append(_wrapper.MakeExecuteScalarWithConn(state));

                Code.Append(_wrapper.MakeCreateMethod(state));
                Code.Append(_wrapper.MakeOtherMethods(state));
                Code.Append(_wrapper.CloseClass(state));
                Code.Append(_results.StartClass(state));
                foreach (var fld in state._7ResultFields)
                {
                    Code.Append(_results.MakeProperty(fld));
                }
            }
            Code.Append(_results.CloseClass()); // closes wrapper class if no results !
            Code.Append(_wrapper.CloseNamespace(state));

            return Code.ToString();
        }
    }
}

