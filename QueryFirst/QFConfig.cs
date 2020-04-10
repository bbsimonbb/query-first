using System.IO;
using System.Text.RegularExpressions;

namespace QueryFirst
{
    public class ConfigFileReader : IConfigFileReader
    {
        /// <summary>
        /// Returns the string contents of the first qfconfig.json file found,
        /// starting in the directory of the path supplied and going up.
        /// </summary>
        /// <param name="filePath">Full path name of the query file</param>
        /// <returns></returns>
        public string GetConfigFile(string filePath)
        {
            filePath = Path.GetDirectoryName(filePath);
            while (filePath != null)
            {
                if (File.Exists(filePath + "\\qfconfig.json"))
                {
                    return File.ReadAllText(filePath + "\\qfconfig.json");
                }
                filePath = Directory.GetParent(filePath)?.FullName;
            }
            return null;
        }
    }
    

    public class QFConfigModel
    {
        public string defaultConnection { get; set; }
        public string provider { get; set; }
        public string helperAssembly { get; set; }
        public bool makeSelfTest { get; set; }
    }
}
