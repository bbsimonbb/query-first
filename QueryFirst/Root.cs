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

namespace QueryFirst
{
    class Root
    {
        // singleton
        private static Root instance = null;
        public static Root Get(DTE dte)
        {
            if (instance == null)
            {
                instance = new Root(dte);
            }
            return instance;
        }
        #region instance members
        private DTE _dte;
        private EnvDTE.Events myEvents;
        private EnvDTE.DocumentEvents myDocumentEvents;
        ProjectItemsEvents CSharpProjectItemsEvents;
        #endregion
        // constructor
        private Root(DTE dte)
        {
            _dte = dte;
            myEvents = dte.Events;
            myDocumentEvents = dte.Events.DocumentEvents;
            myDocumentEvents.DocumentSaved += myDocumentEvents_DocumentSaved;
            myDocumentEvents.DocumentOpened += MyDocumentEvents_DocumentOpened;
            CSharpProjectItemsEvents = (ProjectItemsEvents)dte.Events.GetObject("CSharpProjectItemsEvents");
            CSharpProjectItemsEvents.ItemRenamed += CSharpItemRenamed;
            myEvents.SolutionEvents.Opened += SolutionEvents_Opened;
            myEvents.BuildEvents.OnBuildBegin += BuildEvents_OnBuildBegin;

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
                            queryText = queryText.Replace("/*designTime", "--designTime");
                            queryText = queryText.Replace("endDesignTime*/", "--endDesignTime");
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
                textDoc.ReplacePattern("/*designTime", "--designTime");
                textDoc.ReplacePattern("endDesignTime*/", "--endDesignTime");

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
            if (Document.FullName.EndsWith(".gen.cs"))
            {
                var textDoc = ((TextDocument)Document.Object());
                // format the whole document !
                textDoc.StartPoint.CreateEditPoint().SmartFormat(textDoc.EndPoint);
            }


        }
        private void HandleTimer(Object source, System.Timers.ElapsedEventArgs e)
        {
            //try
            //{
            //    if(_dte.Commands.Item("SQL.TSqlEditorNewQueryConnection") != null && _dte.Commands.Item("SQL.TSqlEditorNewQueryConnection").IsAvailable)
            //        _dte.ExecuteCommand("SQL.TSqlEditorNewQueryConnection", "Data Source=not-mobility;Initial Catalog=NORTHWND;Integrated Security=SSPI;");
            //}
            //catch (Exception ex) { }           
        }
        private void SolutionEvents_Opened()
        {
            RegisterTypes();
        }
        private void RegisterTypes()
        {
            LogToVSOutputWindow("Registering types...\n");
            //kludge
            if (TinyIoCContainer.Current.CanResolve<IWrapperClassMaker>())
            {
                LogToVSOutputWindow("Already registered\n");
                return;
            }
            ConfigurationAccessor config = new ConfigurationAccessor(_dte, null);
            var helperAssembly = config.AppSettings["QfHelperAssembly"];
            if (helperAssembly != null && !string.IsNullOrEmpty(helperAssembly.Value))
            {
                IEnumerable<Assembly> assemblies = new Assembly[]
                {Assembly.LoadFrom(helperAssembly.Value), Assembly.GetExecutingAssembly()};
                //IEnumerable<Assembly> assemblies = new Assembly[] { Assembly.GetExecutingAssembly(), Assembly.LoadFrom(helperAssembly.Value) };

                // Don't use AutoRegister(), it registers thousands of types and we only use four.
                //TinyIoCContainer.Current.AutoRegister(assemblies);
                TinyIoCContainer.Current.Register<IMap>(new Map());
                TinyIoCContainer.Current.Register(typeof(IWrapperClassMaker), typeof(WrapperClassMaker));
                TinyIoCContainer.Current.Register(typeof(ISignatureMaker), typeof(SignatureMaker));
                TinyIoCContainer.Current.Register(typeof(IResultClassMaker), typeof(ResultClassMaker));
            }
            else
            {
                // Don't use AutoRegister(), it registers thousands of types and we only use four.
                //TinyIoCContainer.Current.AutoRegister();
                TinyIoCContainer.Current.Register<IMap>(new Map());
                TinyIoCContainer.Current.Register(typeof(IWrapperClassMaker), typeof(WrapperClassMaker));
                TinyIoCContainer.Current.Register(typeof(ISignatureMaker), typeof(SignatureMaker));
                TinyIoCContainer.Current.Register(typeof(IResultClassMaker), typeof(ResultClassMaker));
            }
            LogToVSOutputWindow("Registered types...\n");
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
                            fuxed++;
                        }
                        if (fuxed == 2)
                            return; //2 files to rename, then we're finished.
                    }
                }
            }
        }
        void myDocumentEvents_DocumentSaved(Document Document)
        {
            //kludge
            if (!TinyIoCContainer.Current.CanResolve<IWrapperClassMaker>())
                RegisterTypes();
            if (Document.FullName.EndsWith(".sql"))
                try
                {
                    var cdctr = new Conductor(Document);
                    if (cdctr.IsQFQuery())
                        cdctr.Process();

                }
                catch (Exception ex)
                {
                    LogToVSOutputWindow(ex.Message + ex.StackTrace);
                }
        }
        public void LogToVSOutputWindow(string message)
        {
            Window window = _dte.Windows.Item(EnvDTE.Constants.vsWindowKindOutput);
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
        #endregion
    }
}
