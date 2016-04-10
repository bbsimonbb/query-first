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
            CSharpProjectItemsEvents = (ProjectItemsEvents)dte.Events.GetObject("CSharpProjectItemsEvents");
            CSharpProjectItemsEvents.ItemRenamed += CSharpItemRenamed;
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
                        if (item.Name == OldName.Replace(".sql", ".cs"))
                        {
                            item.Name = renamedQuery.Name.Replace(".sql", ".cs");
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
            if (Document.FullName.EndsWith(".sql"))
                try
                {
                    new QueryProcessor(Document).Process();
                }
                catch (Exception ex) { }
        }

        #endregion
    }
}
