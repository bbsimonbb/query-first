using Mono.Options;
using System;
using System.Collections.Generic;
using System.IO;

namespace QueryFirst.CommandLine
{
    public class Program
    {
        static bool keepOpen;
        public static void Main(string[] args)
        {
            // parse args
            var startupOptions = DefineAndParseOptions(args);

            // check we have a file (should never be empty because if nothing provided we take the current directory.)
            if (!string.IsNullOrEmpty(startupOptions.SourcePath) && !File.Exists(startupOptions.SourcePath) && !Directory.Exists(startupOptions.SourcePath))
            {
                QfConsole.WriteLine($@"The file or directory {startupOptions.SourcePath} does not exist. Exiting...");
                return;
            }
            // fetch config query/project/install
            var configFileReader = new ConfigFileReader();
            QfConfigModel outerConfig = configFileReader.ReadAndResolveProjectAndInstallConfigs(startupOptions, configFileReader);

            // register types
            RegisterTypes.Register(outerConfig.HelperAssemblies);

            // New Config
            if (startupOptions.CreateConfig.GetValueOrDefault())
            {
                var fileToCreate = Path.Join(Environment.CurrentDirectory, "qfconfig.json");
                if (File.Exists(fileToCreate))
                {
                    Console.WriteLine($"QueryFirst.CommandLine: {fileToCreate} exists already. Skipping.");
                }
                File.WriteAllText(fileToCreate,
$@"{{
  ""defaultConnection"": ""Server = localhost\\SQLEXPRESS; Database = NORTHWND; Trusted_Connection = True; "",
  ""provider"": ""Microsoft.Data.SqlClient"",
  ""namespace"": ""MyNamespaceForCodeGeneration""
}}
            "
                );
            }
            // New Runtime Connection
            if (startupOptions.CreateRuntimeConnection.GetValueOrDefault())
            {
                var fileToCreate = Path.Join(Environment.CurrentDirectory, "QfDbConnectionFactory.cs");
                if (File.Exists(fileToCreate))
                {
                    Console.WriteLine($"QueryFirst.CommandLine: {fileToCreate} exists already. Skipping.");
                }
                File.WriteAllText(fileToCreate,
$@"using System.Data;

namespace QueryFirst.CommandLine
{{
    /// <summary>
    /// If you're already referencing QueryFirst.CommandLine.CoreLib for self tests, you've already got this interface and you can delete this copy.
    /// </summary>
    public interface IQfDbConnectionFactory
    {{
        IDbConnection CreateConnection();
    }}
}}

namespace MyProjectNamespace
{{
    using Microsoft.Data.SqlClient;
    using QueryFirst.CommandLine;

    /// <summary>
    /// QueryFirst.CommandLine NEEDS YOU to implement its connection factory. You will need to customise this
    /// obviously for your environment/provider. The generated repo needs a connection factory instance, and will call CreateConnection()
    /// for every query execution. If you want something else, there are overloads where you supply the connection.
    /// </summary>
    public class QfDbConnectionFactory : IQfDbConnectionFactory
    {{
        public IDbConnection CreateConnection() => new SqlConnection(""Server = localhost\\SQLEXPRESS; Database = NORTHWND; Trusted_Connection = True;"");
    }}
}}
"
                );
            }
            // New query
            if (!string.IsNullOrEmpty(startupOptions.NewQueryName))
            {
                if (!startupOptions.NewQueryName.EndsWith(".sql"))
                    startupOptions.NewQueryName = startupOptions.NewQueryName + ".sql";
                if (File.Exists(startupOptions.NewQueryName))
                {
                    Console.WriteLine($"QueryFirst.CommandLine: {startupOptions.NewQueryName} exists already. Exiting");
                    return;
                }
                File.WriteAllText(startupOptions.NewQueryName,
@"/* .sql query managed by QueryFirst.CommandLine */
-- designTime - put parameter declarations and design time initialization here
-- endDesignTime"
                );
                return;
            }

            // C'est pas beau. If we've created a file or shown help, we'll do nothing else...
            if (startupOptions.CreateConfig.GetValueOrDefault()
                || startupOptions.CreateRuntimeConnection.GetValueOrDefault()
                || !string.IsNullOrEmpty(startupOptions.NewQueryName)
                || startupOptions.DidShowHelp
            )
            {
                return;
            }

            // Process one file
            if (File.Exists(startupOptions.SourcePath))
            {
                if (startupOptions.Watch)
                {
                    // watch for changes or renaming of the specified file. (VS renames a temp file after deleting the original file)
                    using (FileSystemWatcher watcher = new FileSystemWatcher(Path.GetDirectoryName(startupOptions.SourcePath), Path.GetFileName(startupOptions.SourcePath)))
                    {
                        watcher.NotifyFilter = NotifyFilters.LastAccess
                      | NotifyFilters.LastWrite
                      | NotifyFilters.FileName
                      | NotifyFilters.DirectoryName;


                        watcher.Changed += (source, e) =>
                        {
                            try
                            {
                                var conductor = new CommandLineConductor().BuildUp();
                                conductor.ProcessOneQuery(startupOptions.SourcePath, outerConfig);
                            }
                            catch (Exception ex)
                            {
                                Console.Write(ex.TellMeEverything());
                            }

                        };
                        //watcher.Deleted += OnChanged;
                        watcher.Renamed += (source, e) =>
                        {
                            try
                            {
                                var conductor = new CommandLineConductor().BuildUp();
                                conductor.ProcessOneQuery(startupOptions.SourcePath, outerConfig);
                            }
                            catch (Exception ex)
                            {
                                Console.Write(ex.TellMeEverything());
                            }

                        };
                        watcher.EnableRaisingEvents = true;
                        Console.WriteLine("Press 'q' to stop watching.");
                        while (Console.Read() != 'q') ;
                    }
                }
                else
                {
                    var conductor = new CommandLineConductor().BuildUp();
                    conductor.ProcessOneQuery(startupOptions.SourcePath, outerConfig);
                }

            }
            // Process a folder
            if (Directory.Exists(startupOptions.SourcePath))
            {
                if (startupOptions.Watch)
                {
                    // watch for changes or renaming of .sql files in the specified folder.
                    using (FileSystemWatcher watcher = new FileSystemWatcher(startupOptions.SourcePath, "*.sql"))
                    {
                        watcher.NotifyFilter = NotifyFilters.LastAccess
                      | NotifyFilters.LastWrite
                      | NotifyFilters.FileName
                      | NotifyFilters.DirectoryName;

                        watcher.IncludeSubdirectories = true;


                        watcher.Changed += (source, e) =>
                        {
                            try
                            {
                                if (e.FullPath.ToLower().EndsWith(".sql"))
                                {
                                    var conductor = new CommandLineConductor().BuildUp();
                                    conductor.ProcessOneQuery(e.FullPath, outerConfig);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.Write(ex.TellMeEverything());
                            }

                        };
                        //watcher.Deleted += OnChanged;
                        watcher.Renamed += (source, e) =>
                        {
                            try
                            {
                                if (e.FullPath.ToLower().EndsWith(".sql"))
                                {
                                    var conductor = new CommandLineConductor().BuildUp();
                                    conductor.ProcessOneQuery(e.FullPath, outerConfig);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.Write(ex.TellMeEverything());
                            }

                        };
                        watcher.EnableRaisingEvents = true;
                        Console.WriteLine($"Press 'q' to stop watching in ${startupOptions.SourcePath}");
                        while (Console.Read() != 'q') ;
                    }
                }
                else
                {
                    ProcessDirectory(startupOptions.SourcePath, outerConfig);
                }

                if (keepOpen)
                {
                    Console.ReadKey();
                }



            }
        }

        // Process all files in the directory passed in, recurse on any directories
        // that are found, and process the files they contain.
        public static void ProcessDirectory(string targetDirectory, QfConfigModel outerConfig)
        {
            // Process the list of files found in the directory.
            string[] fileEntries = Directory.GetFiles(targetDirectory, "*.sql");
            foreach (string fileName in fileEntries)
                ProcessFile(fileName, outerConfig);

            // Recurse into subdirectories of this directory.
            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
                ProcessDirectory(subdirectory, outerConfig);
        }

        public static void ProcessFile(string path, QfConfigModel outerConfig)
        {
            var conductor = new CommandLineConductor().BuildUp();
            conductor.ProcessOneQuery(path, outerConfig);
            Console.WriteLine("Processed file '{0}'.", path);
        }
        /// <summary>
        /// Defines the command line switches. See
        /// https://github.com/xamarin/XamarinComponents/tree/master/XPlat/Mono.Options
        /// </summary>
        /// <param name="args">The command line args to parse</param>
        /// <returns></returns>
        static StartupOptions DefineAndParseOptions(string[] args)
        {
            var returnVal = new StartupOptions();
            var commandLineConfig = new QfConfigModel();

            // these variables will be set when the command line is parsed

            // these are the available options, note that they set the variables
            commandLineConfig.Generators = new List<Generator>();
            var shouldShowHelp = false;
            var options = new OptionSet {
                {"c|qfDefaultConnection=", "Connection string for query generation, overrides config files.", c => commandLineConfig.DefaultConnection = c },
                {"m|makeSelfTest", "Make integration test for query. Requires xunit. Overrides config files.", m => commandLineConfig.MakeSelfTest = m != null},
                {"g|generator=" , "Generator(s) to use. Will replace generators specified in config files", g => commandLineConfig.Generators.Add(new Generator{Name = g })},
                {"w|watch", "Watch for changes in the file or directory", w => returnVal.Watch = w != null },
                {"k|keepOpen", "Wait for a keystroke before exiting the console.", k => keepOpen = k != null },
                {"h|help", "show this message and exit", h => shouldShowHelp = h != null },
                {"n|new=", "create a new QueryFirst.CommandLine query with the specified name.", n => returnVal.NewQueryName = n },
                {"j|newConfig", "create qfconfig.json in the current directory", nc => returnVal.CreateConfig = nc != null },
                {"r|newRuntimeConnection", "create QfRuntimeConnection.cs in the current directory", nc => returnVal.CreateRuntimeConnection = nc != null },
            };

            // now parse the options...
            List<string> extra;
            try
            {
                // parse the command line
                extra = options.Parse(args);
                if (extra != null && extra.Count > 0 && !string.IsNullOrEmpty(extra[0]))
                    returnVal.SourcePath = extra[0];
                else
                    // by default, process the current directory
                    returnVal.SourcePath = Environment.CurrentDirectory;
            }
            catch (OptionException e)
            {
                // output some error message
                Console.Write("QueryFirst.CommandLine: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `QueryFirst.CommandLine --help' for more information.");
                return null;
            }
            finally
            {
                // clean up if none...
                if (commandLineConfig.Generators.Count == 0)
                    commandLineConfig.Generators = null;
            }
            if (shouldShowHelp)
            {
                options.WriteOptionDescriptions(Console.Out);
                returnVal.DidShowHelp = true;
            }
            returnVal.StartupConfig = commandLineConfig;
            return returnVal;
        }

    }
}
