using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
using System.IO;
using Microsoft.VisualStudio.CommandBars;
using System.Resources;
using System.Reflection;
using System.Globalization;
using System.Drawing;
using TinyIoC;
using System.Timers;
using QueryFirst.TypeMappings;

namespace QueryFirst
{
    class SolutionEventHandlers
    {
        // singleton
        private static SolutionEventHandlers _inst = null;
        public static SolutionEventHandlers Inst(DTE dte, DTE2 dte2)
        {
            if (_inst == null)
            {
                _inst = new SolutionEventHandlers(dte, dte2);
            }
            return _inst;
        }
        #region instance members
        private DTE _dte;
        private DTE2 _dte2;
        private EnvDTE.Events myEvents;
        private EnvDTE.DocumentEvents myDocumentEvents;
        private VSOutputWindow _VSOutputWindow;
        ProjectItemsEvents CSharpProjectItemsEvents;
        #endregion
        // constructor
        private SolutionEventHandlers(DTE dte, DTE2 dte2)
        {
            _dte = dte;
            _dte2 = dte2;
            myEvents = dte.Events;
            myDocumentEvents = dte.Events.DocumentEvents;
            myDocumentEvents.DocumentSaved += myDocumentEvents_DocumentSaved;
            myDocumentEvents.DocumentOpened += MyDocumentEvents_DocumentOpened;
            CSharpProjectItemsEvents = (ProjectItemsEvents)dte.Events.GetObject("CSharpProjectItemsEvents");
            CSharpProjectItemsEvents.ItemRenamed += CSharpItemRenamed;
            myEvents.SolutionEvents.Opened += SolutionEvents_Opened;
            myEvents.BuildEvents.OnBuildBegin += BuildEvents_OnBuildBegin;
            _VSOutputWindow = new VSOutputWindow(_dte2);

        }

        private void BuildEvents_OnBuildBegin(vsBuildScope Scope, vsBuildAction Action)
        {
            if (!_dte.Solution.SolutionBuild.ActiveConfiguration.Name.Contains("Debug"))
            {
                foreach (Project proj in _dte.Solution.Projects)
                {
                    // Comments !
                    // On opening a query to edit, we "open" the design time comments.
                    // In debug builds, the comment may be compiled "open", and closed by the generated code prior to running the query.
                    // In production builds, to save this step, we verify and "close" all comments section before the build, and 
                    // the generated code runs the query as found.
                    SetCommentsForProd(proj.ProjectItems);
                }
            }
        }
        private void SetCommentsForProd(ProjectItems items)
        {
            foreach (ProjectItem item in items)
            {
                try
                {
                    if (item.FileNames[1].EndsWith(".sql"))
                    {
                        var queryText = File.ReadAllText(item.FileNames[1]);
                        if (queryText.IndexOf("-- designTime") >= 0)
                        {
                            queryText = queryText.Replace("-- designTime", "/*designTime");
                            queryText = queryText.Replace("-- endDesignTime", "endDesignTime*/");
                            File.WriteAllText(item.FileNames[1], queryText);
                        }
                        // backwards compatible
                        if (queryText.IndexOf("--designTime") >= 0)
                        {
                            queryText = queryText.Replace("--designTime", "/*designTime");
                            queryText = queryText.Replace("--endDesignTime", "endDesignTime*/");
                            File.WriteAllText(item.FileNames[1], queryText);
                        }

                    }
                    if (item.Kind == "{6BB5F8EF-4483-11D3-8BCF-00C04F8EC28C}") //folder
                        SetCommentsForProd(item.ProjectItems);
                }
                catch (Exception ex) { }
            }
        }

        private void SetCommentsForDebug(ProjectItems items)
        {
            foreach (ProjectItem item in items)
            {
                try
                {
                    if (item.FileNames[1].EndsWith(".sql"))
                    {
                        var queryText = File.ReadAllText(item.FileNames[1]);
                        if (queryText.IndexOf("/*designTime") >= 0)
                        {
                            queryText = queryText.Replace("/*designTime", "-- designTime");
                            queryText = queryText.Replace("endDesignTime*/", "-- endDesignTime");
                            File.WriteAllText(item.FileNames[1], queryText);
                        }

                    }
                    if (item.Kind == "{6BB5F8EF-4483-11D3-8BCF-00C04F8EC28C}") //folder
                        SetCommentsForDebug(item.ProjectItems);
                }
                catch (Exception ex) { }
            }
        }

        private void MyDocumentEvents_DocumentOpened(Document Document)
        {
            if (Document.FullName.EndsWith(".sql"))
            {
                var textDoc = ((TextDocument)Document.Object());
                textDoc.ReplacePattern("/*designTime", "-- designTime");
                textDoc.ReplacePattern("endDesignTime*/", "-- endDesignTime");
                // backward compatibility
                textDoc.ReplacePattern("--designTime", "-- designTime");
                textDoc.ReplacePattern("--endDesignTime", "-- endDesignTime");


                // never got close to working. cost me 50 points on stack. just saw it open the window????
                //try
                //{
                //    if (_dte.Commands.Item("SQL.TSqlEditorConnect") != null && _dte.Commands.Item("SQL.TSqlEditorConnect").IsAvailable)
                //    {
                //        _dte.ExecuteCommand("SQL.TSqlEditorConnect");
                //    }
                //    //_dte.ExecuteCommand("SQL.TSqlEditorConnect", "Data Source=not-mobility;Initial Catalog=NORTHWND;Integrated Security=SSPI;");
                //}
                //catch (Exception ex) { }


            }
        }
        private void SolutionEvents_Opened()
        {
            RegisterTypes(true);
        }
        private void RegisterTypes(bool force = false)
        {
            try
            {
                var ctr = TinyIoCContainer.Current;
                _VSOutputWindow.Write("Registering types...\n");
                //kludge
                if (force == true)
                {
                    ctr.Dispose();
                }
                else if (TinyIoCContainer.Current.CanResolve<IWrapperClassMaker>())
                {
                    _VSOutputWindow.Write("Already registered\n");
                    return;
                }
                System.Configuration.KeyValueConfigurationElement helperAssembly = null;
                try
                {
                    //var config = new ConfigResolver().GetConfig()
                    //helperAssembly = config.AppSettings["QfHelperAssembly"];
                }
                catch (Exception ex)
                {//nobody cares
                }
                List<Assembly> assemblies = new List<Assembly>();
                if (helperAssembly != null && !string.IsNullOrEmpty(helperAssembly.Value))
                {
                    assemblies.Add(Assembly.LoadFrom(helperAssembly.Value));
                }
                assemblies.Add(Assembly.GetExecutingAssembly());
                TinyIoCContainer.Current.AutoRegister(assemblies, DuplicateImplementationActions.RegisterSingle);
                // IProvider, for instance, has multiple implementations. To resolve we use the provider name on the connection string, 
                // which must correspond to the fully qualified name of the implementation. ie. QueryFirst.Providers.SqlClient for SqlServer

                _VSOutputWindow.Write("Registered types...\n");
            }
            catch (Exception ex)
            {
                _VSOutputWindow.Write(ex.Message + '\n' + ex.StackTrace);
            }
        }

        #region methods
        // SBY composite items. Rename wrapper class if query name changes...
        void CSharpItemRenamed(ProjectItem renamedQuery, string OldName)

        {
            if (OldName.EndsWith(".sql"))
            {
                int fuxed = 0;
                foreach (ProjectItem item in renamedQuery.ProjectItems)
                {
                    string folder = Path.GetDirectoryName((string)renamedQuery.Properties.Item("FullPath").Value);
                    if (((string)item.Properties.Item("FullPath").Value).StartsWith(folder))
                    {
                        if (item.Name == OldName.Replace(".sql", ".gen.cs"))
                        {
                            item.Name = renamedQuery.Name.Replace(".sql", ".gen.cs");

                            fuxed++;
                        }
                        if (item.Name == OldName.Replace(".sql", "Results.cs"))
                        {
                            item.Name = renamedQuery.Name.Replace(".sql", "Results.cs");
                            var oldBaseName = OldName.Replace(".sql", "");
                            var newBaseName = renamedQuery.Name.Replace(".sql", "");
                            var userFile = ((TextDocument)item.Document.Object());
                            if (!item.IsOpen)
                                item.Open();
                            userFile.ReplacePattern(oldBaseName, newBaseName);
                            item.Document.Save();

                            fuxed++;
                        }
                        if (fuxed == 2)
                        {
                            // regenerate query in new location to get new path to manifest stream.
                            var ctx = TinyIoC.TinyIoCContainer.Current.Resolve<ICodeGenerationContext>();
                            new Conductor(_VSOutputWindow, ctx).ProcessOneQuery(renamedQuery.Document);
                            return; //2 files to rename, then we're finished.
                        }
                    }
                }
            }
        }
        void myDocumentEvents_DocumentSaved(Document Document)
        {
            //kludge
            if (!TinyIoCContainer.Current.CanResolve<ICodeGenerationContext>())
                RegisterTypes();
            if (Document.FullName.EndsWith(".sql"))
                try
                {
                    var textDoc = ((TextDocument)Document.Object());
                    var text = textDoc.CreateEditPoint().GetText(textDoc.EndPoint);
                    if (text.Contains("managed by QueryFirst"))
                    {
                        var ctx = TinyIoCContainer.Current.Resolve<ICodeGenerationContext>();
                        var cdctr = new Conductor(_VSOutputWindow, ctx);
                        cdctr.ProcessOneQuery(Document);
                    }

                }
                catch (Exception ex)
                {
                    _VSOutputWindow.Write(ex.Message + ex.StackTrace);
                }
        }
        #endregion
    }
}
