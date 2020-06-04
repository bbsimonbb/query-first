using EnvDTE;
using System;
using System.IO;
using System.Windows.Forms;

namespace QueryFirst
{
    static class CheckPrerequisites
    {
        private static string _checkedSolution = "";
        internal static bool HasPrerequites(Solution solution, ProjectItem item)
        {
            if (solution.FullName == _checkedSolution)
                return true;

            bool foundConfig = false;
            bool foundQfRuntimeConnection = false;
            string caption = "QueryFirst missing prerequisites";
            string message = "";
            foreach (Project proj in solution.Projects)
            {
                checkInFolder(proj.ProjectItems, ref foundConfig, ref foundQfRuntimeConnection);
            }
            if (!foundConfig && !foundQfRuntimeConnection)
                message = @"QueryFirst requires a config file (qfconfig.json) and a runtime connection (QfRuntimeConnection.cs)

Would you like us to create these for you? 

You will need to modify the connection string in each file.";
            else if (!foundConfig)
                message = @"QueryFirst requires a config file (qfconfig.json)

Would you like us to create this for you? You will need to modify the connection string.";
            else if (!foundQfRuntimeConnection)
                message = @"QueryFirst requires a runtime connection (QfRuntimeConnection.cs)

Would you like us to create this for you? You will need to modify the connection string.";
            else
            {
                _checkedSolution = solution.FullName;
                return true;
            }


            // Initializes the variables to pass to the MessageBox.Show method.
            MessageBoxButtons buttons = MessageBoxButtons.YesNo;
            DialogResult result;

            // Displays the MessageBox.
            result = MessageBox.Show(message, caption, buttons);
            if (result == DialogResult.Yes)
            {
                if (!foundConfig)
                {
                    //make config
                    var configFileNameAndPath = Path.GetDirectoryName(item.ContainingProject.FileName) + @"\qfconfig.json";
                    File.WriteAllText(configFileNameAndPath,
@"{
  ""defaultConnection"": ""Server=localhost\\SQLEXPRESS;Database=NORTHWND;Trusted_Connection=True;"",
  ""provider"": ""System.Data.SqlClient"",
  ""connectEditor2DB"":false
}"
                );
                    var config = item.ContainingProject.ProjectItems.AddFromFile(configFileNameAndPath);
                    config.Open();
                }
                if(!foundQfRuntimeConnection)
                {
                    //make QfRuntimeConnection
                    string rootNamespace = "";
                    foreach(Property prop in item.ContainingProject.Properties)
                    {
                        if(prop.Name == "RootNamespace")
                        {
                            rootNamespace = prop.Value as string;
                            break;
                        }
                    }
                    var configFileNameAndPath = Path.GetDirectoryName(item.ContainingProject.FileName) + @"\QfRuntimeConnection.cs";
                    File.WriteAllText(configFileNameAndPath,
@"using System.Data;
using System.Data.SqlClient;

/* What provider are you using? For SqlClient, you will need to add a project reference (.net framework) or 
the System.Data.SqlClient nuget package (.net core). */


namespace " + rootNamespace + @"
{
    class QfRuntimeConnection
    {
        public static IDbConnection GetConnection()
        {
            return new SqlConnection(""Server=localhost\\SQLEXPRESS;Database=NORTHWND;Trusted_Connection=True;"");
        }
    }
}"
                );
                    var newClass = item.ContainingProject.ProjectItems.AddFromFile(configFileNameAndPath);
                    newClass.Open();
                }

            }
            return false;
        }
        private static void checkInFolder(ProjectItems items, ref bool foundConfig, ref bool foundQfRuntimeConnection)
        {
            foreach (ProjectItem item in items)
            {
                try
                {
                    if (item.FileNames[1].ToLower().EndsWith("qfconfig.json"))
                        foundConfig = true;
                    if (item.FileNames[1].ToLower().EndsWith("qfruntimeconnection.cs"))
                        foundQfRuntimeConnection = true;
                    if (item.Kind == "{6BB5F8EF-4483-11D3-8BCF-00C04F8EC28C}") //folder
                        checkInFolder(item.ProjectItems, ref foundConfig, ref foundQfRuntimeConnection);
                }
                catch (Exception ex)
                {
                }
            }
        }
    }
}
