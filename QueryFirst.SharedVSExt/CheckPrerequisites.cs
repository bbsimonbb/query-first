using EnvDTE;
using System;
using System.IO;
using System.Windows.Forms;

namespace QueryFirst.VSExtension
{
    static class CheckPrerequisites
    {
        private static string _checkedSolution = "";
        internal static bool HasPrerequites(Solution solution, ProjectItem item)
        {
            return true;
            // todo : not sure I like this anymore. Little bit invasive and tricky to get right.
            if (solution.FullName == _checkedSolution)
                return true;

            bool foundConfig = false;
            bool foundQfRuntimeConnection = false;
            string caption = "QueryFirst.VSExtension missing prerequisites";
            string message = "";
            foreach (Project proj in solution.Projects)
            {
                if (proj.ProjectItems != null)
                    checkInFolder(proj.ProjectItems, ref foundConfig, ref foundQfRuntimeConnection);
            }
            if (!foundConfig && !foundQfRuntimeConnection)
                message = @"QueryFirst.VSExtension requires a config file (qfconfig.json) and a connection factory (QfDbConnectionFactory.cs)

Would you like us to create these for you? 

You will need to modify the connection string in each file.";
            else if (!foundConfig)
                message = @"QueryFirst.VSExtension requires a config file (qfconfig.json)

Would you like us to create this for you? You will need to modify the connection string.";
            else if (!foundQfRuntimeConnection)
                message = @"QueryFirst.VSExtension requires a connection factory (QfDbConnectionFactory.cs).

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
  ""provider"": ""Microsoft.Data.SqlClient"",
  ""connectEditor2DB"":true
}"
                );
                    var config = item.ContainingProject.ProjectItems.AddFromFile(configFileNameAndPath);
                    config.Open();
                }
                if (!foundQfRuntimeConnection)
                {
                    //make QfRuntimeConnection
                    string rootNamespace = "";
                    foreach (Property prop in item.ContainingProject.Properties)
                    {
                        if (prop.Name == "RootNamespace")
                        {
                            rootNamespace = prop.Value as string;
                            break;
                        }
                    }
                    var configFileNameAndPath = Path.GetDirectoryName(item.ContainingProject.FileName) + @"\QfDbConnectionFactory.cs";
                    File.WriteAllText(configFileNameAndPath,
$@"using System.Data;

namespace QueryFirst
{{
    /// <summary>
    /// If you're already referencing QueryFirst.CoreLib for self tests, you've already got this interface and you can delete this copy.
    /// </summary>
    public interface IQfDbConnectionFactory
    {{
        IDbConnection CreateConnection();
    }}
}}

namespace MyProjectNamespace
{{
    using Microsoft.Data.SqlClient;
    using QueryFirst;

    /// <summary>
    /// QueryFirst NEEDS YOU to implement its connection factory. You will need to customise this
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
                    var newClass = item.ContainingProject.ProjectItems.AddFromFile(configFileNameAndPath);
                    newClass.Open();
                }

            }
            return false;
        }
        private static void checkInFolder(ProjectItems items, ref bool foundConfig, ref bool foundQfRuntimeConnection)
        {
            _ = items ?? throw new ArgumentException(nameof(items));

            foreach (ProjectItem item in items)
            {
                try
                {
                    if (item.FileNames[1].ToLower().EndsWith("qfconfig.json"))
                        foundConfig = true;
                    if (item.FileNames[1].ToLower().EndsWith("qfruntimeconnection.cs"))
                        foundQfRuntimeConnection = true;
                    if (item.Kind == "{6BB5F8EF-4483-11D3-8BCF-00C04F8EC28C}" && item.ProjectItems != null) //folder
                        checkInFolder(item.ProjectItems, ref foundConfig, ref foundQfRuntimeConnection);
                }
                catch (Exception)
                {
                }
            }
        }
    }
}
