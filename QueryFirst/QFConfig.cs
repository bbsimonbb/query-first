using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
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
    public class ConfigResolver : IConfigResolver
    {
        private IConfigFileReader _configFileReader;
        public ConfigResolver(IConfigFileReader configFileReader)
        {
            _configFileReader = configFileReader;
        }
        /// <summary>
        /// Returns the QueryFirst config for a given query file. Values specified directly in 
        /// the query file will trump values specified in the qfconfig.json file.
        /// We look for a qfconfig.json file beside the query file. If none found, we look in the parent directory,
        /// and so on up to the root directory. 
        /// 
        /// If the query specifies a QfDefaultConnection but no QfDefaultConnectionProviderName, "System.Data.SqlClient"
        /// will be assumed.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="queryText"></param>
        /// <returns></returns>
        public QFConfigModel GetConfig(string filePath, string queryText)
        {
            QFConfigModel config = new QFConfigModel();
            var configFileContents = _configFileReader.GetConfigFile(filePath);
            if (!string.IsNullOrEmpty(configFileContents))
            {
                config = JsonConvert.DeserializeObject<QFConfigModel>(configFileContents);
                if (string.IsNullOrEmpty(config.Provider))
                {
                    config.Provider = "System.Data.SqlClient";
                }
            }
            // if the query defines a QfDefaultConnection, use it.
            var match = Regex.Match(queryText, "^--QfDefaultConnection(=|:)(?<cstr>[^\r\n]*)", RegexOptions.Multiline);
            if (match.Success)
            {
                config.DefaultConnection = match.Groups["cstr"].Value;
                var matchProviderName = Regex.Match(queryText, "^--QfDefaultConnectionProviderName(=|:)(?<pn>[^\r\n]*)", RegexOptions.Multiline);
                if (matchProviderName.Success)
                {
                    config.Provider = matchProviderName.Groups["pn"].Value;
                }
                else
                {
                    config.Provider = "System.Data.SqlClient";
                }

            }
            return config;
        }
    }

    public class QFConfigModel
    {
        public string DefaultConnection { get; set; }
        public string Provider { get; set; }
        public string HelperAssembly { get; set; }
        public bool MakeSelfTest { get; set; }
    }
}
