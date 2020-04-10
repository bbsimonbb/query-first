using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;

namespace QueryFirst
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class QueryFirstCmd
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("565bc905-9d2a-4269-bd5a-4b34b04fe9f8");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package _package;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryFirstCmd"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private QueryFirstCmd(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this._package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static QueryFirstCmd Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get
            {
                return this._package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new QueryFirstCmd(package);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            DTE2 dte2 = ServiceProvider.GetService(typeof(EnvDTE.DTE)) as EnvDTE80.DTE2;//Package.GetGlobalService(typeof(DTE)) as DTE2;
            var vsOutputWindow = new VSOutputWindow(dte2);
            RegisterTypes.Instance.Register(vsOutputWindow,false);

            foreach (Project proj in ((QueryFirstCmdPackage)_package).dte.Solution.Projects)
            {
                ProcessAllItems(proj.ProjectItems, vsOutputWindow);
            }
            return;
        }
        private void ProcessAllItems(ProjectItems items, VSOutputWindow vsOutputWindow)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (items != null)
            {

                foreach (ProjectItem item in items)
                {
                    try
                    {
                        if (item.FileNames[1].EndsWith(".sql"))
                        {
                            item.Open();
                            var textDoc = ((TextDocument)item.Document.Object());
                            var text = textDoc.CreateEditPoint().GetText(textDoc.EndPoint);
                            if (text.Contains("managed by QueryFirst"))
                            {
                                new Conductor(vsOutputWindow, null, null).ProcessOneQuery(item.Document);
                            }
                            
                        }
                        if (item.Kind == "{6BB5F8EF-4483-11D3-8BCF-00C04F8EC28C}") //folder
                            ProcessAllItems(item.ProjectItems, vsOutputWindow);
                    }
                    catch(Exception ex)
                    {
                        vsOutputWindow.Write(ex.ToString());
                    }
                }
            }
        }
    }
}
