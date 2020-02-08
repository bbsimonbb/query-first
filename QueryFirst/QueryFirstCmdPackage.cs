//------------------------------------------------------------------------------
// <copyright file="QueryFirstCmdPackage.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
// sby
using EnvDTE;
using EnvDTE80;
using System.Threading;

namespace QueryFirst
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)] // sby
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(QueryFirstCmdPackage.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    public sealed class QueryFirstCmdPackage : AsyncPackage, IVsShellPropertyEvents
    {
        /// <summary>
        /// QueryFirstCmdPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "3a63d8f4-e8a0-44b4-b3dc-d7670d3b55a6";
        // sby : knickers in the breeze
        public DTE dte;
        public DTE2 dte2;
        uint cookie;
        SolutionEventHandlers qfRoot;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryFirstCmd"/> class.
        /// </summary>
        public QueryFirstCmdPackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override async System.Threading.Tasks.Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            QueryFirstCmd.Initialize(this);
            await base.InitializeAsync(cancellationToken, progress);

            // sby : set an eventlistener for shell property changes
            IVsShell shellService = await GetServiceAsync(typeof(SVsShell)) as IVsShell;
            if (shellService != null)
                ErrorHandler.ThrowOnFailure(shellService.AdviseShellPropertyChanges(this, out cookie));
            // do it anyway
            zombieProofInitialization();
        }

        #endregion
        public int OnShellPropertyChange(int propid, object var)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            // when zombie state changes to false, finish package initialization
            if ((int)__VSSPROPID.VSSPROPID_Zombie == propid)
            {
                if ((bool)var == false)
                {
                    if (zombieProofInitialization())
                    {
                        // eventlistener no longer needed
                        IVsShell shellService = GetService(typeof(SVsShell)) as IVsShell;
                        if (shellService != null)
                            ErrorHandler.ThrowOnFailure(shellService.UnadviseShellPropertyChanges(this.cookie));
                        this.cookie = 0;
                    }

                }
            }
            return VSConstants.S_OK;
        }
        private bool zombieProofInitialization()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            dte = GetService(typeof(SDTE)) as DTE;
            DTE2 dte2 = GetService(typeof(EnvDTE.DTE)) as EnvDTE80.DTE2;//Package.GetGlobalService(typeof(DTE)) as DTE2;
            if (dte != null && dte2 != null)
            {
                qfRoot = SolutionEventHandlers.Inst(dte, dte2);
                return true;
            }
            else
                return false;
        }
    }
}
