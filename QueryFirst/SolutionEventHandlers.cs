using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using TinyIoC;

namespace QueryFirst
{
    class SolutionEventHandlers
    {
        // singleton
        private static SolutionEventHandlers _inst = null;
        private Window _lastConnectedSqlWindow;

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
        private bool spammed = false;
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

            myDocumentEvents.DocumentOpened += myDocumentEvents_DocumentOpened;
            myDocumentEvents.DocumentSaved += myDocumentEvents_DocumentSaved;
            CSharpProjectItemsEvents = (ProjectItemsEvents)dte.Events.GetObject("CSharpProjectItemsEvents");
            CSharpProjectItemsEvents.ItemRenamed += CSharpItemRenamed;
            myEvents.SolutionEvents.Opened += SolutionEvents_Opened;
            _VSOutputWindow = new VSOutputWindow(_dte2);
        }

        private void SolutionEvents_Opened()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (!spammed)
            {
                _VSOutputWindow.Write(
@"If you're using and enjoying QueryFirst, please leave a review!
https://marketplace.visualstudio.com/items?itemName=bbsimonbb.QueryFirst#review-details
"
                );
                spammed = true;
            }
            RegisterTypes.Instance.Register(_VSOutputWindow, true);

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
                            new Conductor(_VSOutputWindow, null, null).ProcessOneQuery(renamedQuery.Document);
                            if (rememberToClose1)
                                renamedQuery.Document.Close();
                            return; //2 files to rename, then we're finished.
                        }
                    }
                }
            }
        }
        private void myDocumentEvents_DocumentOpened(Document Document)
        {
            if (!connecting)
            {
                // we need to let the window initialise, otherwise intellisense is broken
                var t = new System.Threading.Thread(() => connectEditorWindow2DB(Document));
                t.Start();
            }
        }
        private bool connecting = false;
        private void connectEditorWindow2DB(Document Document)
        {
            try
            {
                connecting = true;
                // on my machine, delays less than 700ms hang the environment. Better not do that to folk.
                System.Threading.Thread.Sleep(1500);
                if (Document.FullName.EndsWith(".sql"))
                {
                    var textDoc = ((TextDocument)Document.Object());
                    var text = textDoc.CreateEditPoint().GetText(textDoc.EndPoint);
                    if (text.Contains("managed by QueryFirst"))
                    {
                        RegisterTypes.Instance.Register(_VSOutputWindow);
                        // get connection string
                        var state = new State();
                        new Conductor(_VSOutputWindow, null, null).ProcessUpToStep4(Document, ref state);

                        if (_lastConnectedSqlWindow != Document.ActiveWindow
                            && state._4Config.provider == "System.Data.SqlClient"
                            && state._4Config.connectEditor2DB
                            )
                        {
                            _lastConnectedSqlWindow = Document.ActiveWindow;

                            IVsPackage sqlEditorPackageInstance = null;
                            IVsShell val = Package.GetGlobalService(typeof(SVsShell)) as IVsShell;
                            if (val != null)
                            {
                                Guid guid = new Guid("fef13793-c947-4fb1-b864-c9f0be9d9cf6");
                                val.LoadPackage(ref guid, out sqlEditorPackageInstance);
                            }

                            var lastFocusedSqlEditor = sqlEditorPackageInstance.GetType().GetRuntimeProperties().Where(p => p.Name == "LastFocusedSqlEditor").FirstOrDefault().GetValue(sqlEditorPackageInstance);

                            if (lastFocusedSqlEditor != null)
                            {
                                var getAuxillaryDocDataMethodInfo = sqlEditorPackageInstance.GetType().GetRuntimeMethods().Where(m => m.Name == "GetAuxillaryDocData").FirstOrDefault();
                                var docData = lastFocusedSqlEditor.GetType().GetRuntimeProperties().Where(p => p.Name == "DocData").FirstOrDefault().GetValue(lastFocusedSqlEditor);
                                var auxiliaryDocData = getAuxillaryDocDataMethodInfo.Invoke(sqlEditorPackageInstance, new object[] { docData });

                                var isQueryWindowInfo = auxiliaryDocData.GetType().GetRuntimeProperties().Where(p => p.Name == "IsQueryWindow").FirstOrDefault();

                                var strategyInfo = Type.GetType("Microsoft.VisualStudio.Data.Tools.SqlEditor.DataModel.DefaultSqlEditorStrategy, Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=16.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
                                var ctors = strategyInfo.GetConstructors(BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance);
                                var strategyInstance = ctors[3].Invoke(new object[] { new SqlConnectionStringBuilder(state._4Config.defaultConnection), true });

                                var strategySetterInfo = auxiliaryDocData.GetType().GetRuntimeProperties().Where(p => p.Name == "Strategy").FirstOrDefault();
                                strategySetterInfo.SetValue(auxiliaryDocData, strategyInstance);

                                var queryExecutorInstance = auxiliaryDocData.GetType().GetRuntimeProperties().Where(p => p.Name == "QueryExecutor").FirstOrDefault().GetValue(auxiliaryDocData);
                                var connectionStrategyInstance = queryExecutorInstance.GetType().GetRuntimeProperties().Where(p => p.Name == "ConnectionStrategy").FirstOrDefault().GetValue(queryExecutorInstance);

                                var ensureConnectionMethodInfo = connectionStrategyInstance.GetType().GetRuntimeMethods().Where(m => m.Name == "EnsureConnection").FirstOrDefault();
                                ensureConnectionMethodInfo.Invoke(connectionStrategyInstance, new object[] { true });

                                isQueryWindowInfo.SetValue(auxiliaryDocData, true);
                            }
                        }
                    }
                }
                connecting = false;
            }
            catch(Exception ex)
            {
                // in space no-one can hear you scream.
            }
        }
        void myDocumentEvents_DocumentSaved(Document Document)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            //kludge
            if (!TinyIoCContainer.Current.CanResolve<IProvider>())
                RegisterTypes.Instance.Register(_VSOutputWindow, false);
            if (Document.FullName.EndsWith(".sql"))
                try
                {
                    var textDoc = ((TextDocument)Document.Object());
                    var text = textDoc.CreateEditPoint().GetText(textDoc.EndPoint);
                    if (text.Contains("managed by QueryFirst"))
                    {
                        if (CheckPrerequisites.HasPrerequites(_dte.Solution, Document.ProjectItem))
                        {
                            var cdctr = new Conductor(_VSOutputWindow, null, null);
                            cdctr.ProcessOneQuery(Document);
                        }
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
