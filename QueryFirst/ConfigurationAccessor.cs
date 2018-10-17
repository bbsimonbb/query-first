//using EnvDTE;
//using System;
//using System.Configuration;
//using System.Text.RegularExpressions;

//namespace QueryFirst
//{

//    /// <summary>
//    /// Provides strongly typed access to the hosting EnvDTE.Project and app.config/web.config
//    /// configuration file, if present.
//    ///
//    /// Typical usage from T4 template:
//    /// <code>ConfigurationAccessor config = new ConfigurationAccessor((IServiceProvider)this.Host);</code>
//    ///
//    /// </summary>
//    /// <author>Sky Sanders [sky.sanders@gmail.com, http://skysanders.net/subtext]</author>
//    /// <date>01-23-10</date>
//    /// <copyright>The contents of this file are a Public Domain Dedication.</copyright>
//    ///
//    /// TODO: determine behaviour of ProjectItem.FileNames when referred to a linked file.
//    ///
//    public class ConfigurationAccessor
//    {

//        /// <summary>
//        /// Typical usage from T4 template:
//        /// <code>ConfigurationAccessor config = new ConfigurationAccessor((IServiceProvider)this.Host);</code>
//        /// </summary>
//        //public ConfigurationAccessor(IServiceProvider host)
//        //	: this(host, null)
//        //{ }

//        /// <summary>
//        /// Same as default constructor but it looks for a web.config/app.config in the passed config
//        /// project location and not in the first startup project it finds. The configProjectLocation
//        /// passed should be relative to the solution file.
//        /// </summary>
//        //public ConfigurationAccessor(IServiceProvider host, string configProjectLocation)
//        public ConfigurationAccessor(EnvDTE.DTE env, string configProjectLocation)
//        {
//            // Get the instance of Visual Studio that is hosting the calling file
//            // SBY : not in a T4, no IServiceProvider and we already have env.
//            //EnvDTE.DTE env = (EnvDTE.DTE)host.GetService(typeof(EnvDTE.DTE));

//            // Initialize configuration filename
//            string configurationFilename = null;

//            _project = GetConfigProject(env);

//            // Try and find the configuration file in the active solution project
//            configurationFilename = FindConfigurationFilename(_project);

//            // If we didn't find the configuration file, check the startup project or passed config
//            // project location in the constructor
//            if (configurationFilename == null)
//            {
//                // We are going to get the first *STARTUP* project in the solution
//                // Our startup projects should also have a valid web.config/app.config, however,
//                // if for some reason we have more than one startup project in the solution, this
//                // will just grab the first one it finds
//                //
//                // We can also supply a config project location to look for in the constructor. This solves
//                // the problem in the case where we have multiple startup projects and this file is not
//                // in the first, or the config file is not in either the startup project or the active solution
//                // project.
//                if (!string.IsNullOrEmpty(configProjectLocation))
//                {
//                    _project = (EnvDTE.Project)env.Solution.Projects.Item(configProjectLocation);
//                }
//                else
//                {
//                    foreach (String s in (Array)env.Solution.SolutionBuild.StartupProjects)
//                    {
//                        _project = (EnvDTE.Project)env.Solution.Projects.Item(s);
//                        break;
//                    }
//                }

//                // Try and find the configuration file in one of the projects we found
//                configurationFilename = FindConfigurationFilename(_project);
//            }

//            // Return the configuration object if we have a configuration file name
//            // If we do not have a configuration file name, throw an exception
//            if (!string.IsNullOrEmpty(configurationFilename))
//            {
//                // found it, map it and expose salient members as properties
//                ExeConfigurationFileMap configFile = null;
//                configFile = new ExeConfigurationFileMap();
//                configFile.ExeConfigFilename = configurationFilename;
//                _configuration = System.Configuration.ConfigurationManager.OpenMappedExeConfiguration(configFile, ConfigurationUserLevel.None);
//            }
//            else
//            {
//                throw new ArgumentException("Unable to find a configuration file (web.config/app.config). If the config file is located in a different project, you must mark that project as either the Startup Project or pass the project location of the config file relative to the solution file.");
//            }
//        }

//        private Project GetConfigProject(DTE env)
//        {
//            // Gets an array of currently selected projects. Since you are either in this file saving it or
//            // right-clicking the item in solution explorer to invoke the context menu it stands to reason
//            // that there is 1 ActiveSolutionProject and that it is the parent of this file....
//            //_project = (EnvDTE.Project)((Array)env.ActiveSolutionProjects).GetValue(0);
//            // SBY can fail when called from toolbar. Startup project as fallback
//            Array asp = (Array)env.ActiveSolutionProjects;
//            if (asp.Length > 0)
//                return (EnvDTE.Project)asp.GetValue(0); // original code
//            else
//            {
//                foreach (Project projInSolution in env.Solution.Projects)
//                {
//                    foreach (string startupProj in (Array)env.Solution.SolutionBuild.StartupProjects)
//                    {
//                        if (projInSolution.FullName.EndsWith(startupProj))
//                            return projInSolution; //first startup project found.
//                    }
//                }
//            }
//            return null;
//        }

//        /// <summary>
//        /// Finds a web.config/app.config file in the passed project and returns the file name.
//        /// If none are found, returns null.
//        /// </summary>
//        private string FindConfigurationFilename(EnvDTE.Project project)
//        {
//            // examine each project item's filename looking for app.config or web.config
//            foreach (EnvDTE.ProjectItem item in project.ProjectItems)
//            {
//                if (Regex.IsMatch(item.Name, "(app|web).config", RegexOptions.IgnoreCase))
//                {
//                    // TODO: try this with linked files. is the filename pointing to the source?
//                    return item.get_FileNames(0);
//                }
//            }

//            // not found, return null
//            return null;
//        }

//        private EnvDTE.Project _project;
//        private System.Configuration.Configuration _configuration;

//        /// <summary>
//        /// Provides access to the host project.
//        /// </summary>
//        /// <remarks>see http://msdn.microsoft.com/en-us/library/envdte.project.aspx</remarks>
//        public EnvDTE.Project Project
//        {
//            get { return _project; }
//        }

//        /// <summary>
//        /// Convenience getter for Project.Properties.
//        /// Examples:
//        /// <code>string thisAssemblyName = config.Properties.Item("AssemblyName").Value.ToString();</code>
//        /// <code>string thisAssemblyName = config.Properties.Item("AssemblyName").Value.ToString();</code>
//        /// </summary>
//        /// <remarks>see http://msdn.microsoft.com/en-us/library/envdte.project_properties.aspx</remarks>
//        public EnvDTE.Properties Properties
//        {
//            get { return _project.Properties; }
//        }

//        /// <summary>
//        /// Provides access to the application/web configuration file.
//        /// </summary>
//        /// <remarks>see http://msdn.microsoft.com/en-us/library/system.configuration.configuration.aspx</remarks>
//        public System.Configuration.Configuration Configuration
//        {
//            get { return _configuration; }
//        }

//        /// <summary>
//        /// Provides access to the appSettings section of the configuration file.
//        /// Behavior differs from typical AppSettings usage in that the indexed
//        /// item's .Value must be explicitly addressed.
//        /// <code>string setting = config.AppSettings["MyAppSetting"].Value;</code>
//        /// </summary>
//        /// <remarks>see http://msdn.microsoft.com/en-us/library/system.configuration.configuration.appsettings.aspx</remarks>
//        public KeyValueConfigurationCollection AppSettings
//        {
//            get { return _configuration.AppSettings.Settings; }
//        }

//        /// <summary>
//        /// Provides access to the connectionStrings section of the configuration file.
//        /// Behavior is as expected; items are accessed by string key or integer index.
//        /// <code>string northwindProvider = config.ConnectionStrings["northwind"].ProviderName;</code>
//        /// </summary>
//        /// <remarks>see http://msdn.microsoft.com/en-us/library/system.configuration.configuration.connectionstrings.aspx</remarks>
//        public ConnectionStringSettingsCollection ConnectionStrings
//        {
//            get { return _configuration.ConnectionStrings.ConnectionStrings; }
//        }
//    }
//}
