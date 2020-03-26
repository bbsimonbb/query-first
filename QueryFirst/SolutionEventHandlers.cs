using System;
using System.Collections.Generic;
using EnvDTE;
using EnvDTE80;
using System.IO;
using System.Reflection;
using TinyIoC;
using Microsoft.VisualStudio.Shell;

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
            ThreadHelper.ThrowIfNotOnUIThread();
            _dte = dte;
            _dte2 = dte2;
            myEvents = dte.Events;
            myDocumentEvents = dte.Events.DocumentEvents;
            myDocumentEvents.DocumentSaved += myDocumentEvents_DocumentSaved;
            CSharpProjectItemsEvents = (ProjectItemsEvents)dte.Events.GetObject("CSharpProjectItemsEvents");
            CSharpProjectItemsEvents.ItemRenamed += CSharpItemRenamed;
            myEvents.SolutionEvents.Opened += SolutionEvents_Opened;
            _VSOutputWindow = new VSOutputWindow(_dte2);
        }

        private void SolutionEvents_Opened()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            RegisterTypes(true);
        }
        private void RegisterTypes(bool force = false)
        {
            try
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                var ctr = TinyIoCContainer.Current;
                _VSOutputWindow.Write(@"If you're using and enjoying QueryFirst, please leave a review!
https://marketplace.visualstudio.com/items?itemName=bbsimonbb.QueryFirst#review-details
");
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

                List<Assembly> assemblies = new List<Assembly>();
                if (helperAssembly != null && !string.IsNullOrEmpty(helperAssembly.Value))
                {
                    assemblies.Add(Assembly.LoadFrom(helperAssembly.Value));
                }
                assemblies.Add(Assembly.GetExecutingAssembly());
                TinyIoCContainer.Current.AutoRegister(assemblies, DuplicateImplementationActions.RegisterSingle);
                // IProvider, for instance, has multiple implementations. To resolve we use the provider name on the connection string, 
                // which must correspond to the fully qualified name of the implementation. ie. QueryFirst.Providers.SqlClient for SqlServer

                //_VSOutputWindow.Write("Registered types...\n");
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
            ThreadHelper.ThrowIfNotOnUIThread();
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
                            bool rememberToClose = false;
                            if (!item.IsOpen)
                            {
                                item.Open();
                                rememberToClose = true;
                            }                            
                            var userFile = ((TextDocument)item.Document.Object());
                            userFile.ReplacePattern(oldBaseName, newBaseName);
                            item.Document.Save();

                            if (rememberToClose)
                                item.Document.Close();
                            fuxed++;
                        }
                        if (fuxed == 2)
                        {
                            // regenerate query in new location.
                            var rememberToClose1 = false;
                            if (!renamedQuery.IsOpen)
                            {
                                renamedQuery.Open();
                                rememberToClose1 = true;
                            }
                            new Conductor(_VSOutputWindow, null,null).ProcessOneQuery(renamedQuery.Document);
                            if (rememberToClose1)
                                renamedQuery.Document.Close();
                            return; //2 files to rename, then we're finished.
                        }
                    }
                }
            }
        }
        void myDocumentEvents_DocumentSaved(Document Document)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            //kludge
            if (!TinyIoCContainer.Current.CanResolve<IProvider>())
                RegisterTypes();
            if (Document.FullName.EndsWith(".sql"))
                try
                {
                    var textDoc = ((TextDocument)Document.Object());
                    var text = textDoc.CreateEditPoint().GetText(textDoc.EndPoint);
                    if (text.Contains("managed by QueryFirst"))
                    {
                        var cdctr = new Conductor(_VSOutputWindow, null, null);
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
