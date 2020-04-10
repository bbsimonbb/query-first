using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;

namespace QueryFirst
{
    public class _4ResolveConfig
    {
        private IConfigFileReader _configFileReader;
        public _4ResolveConfig(IConfigFileReader configFileReader)
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
        public State Go(State state)
        {
            QFConfigModel config = new QFConfigModel();
            var configFileContents = _configFileReader.GetConfigFile(state._1CurrDir);
            if (!string.IsNullOrEmpty(configFileContents))
            {
                using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(configFileContents)))
                {
                    var ser = new DataContractJsonSerializer(typeof(QFConfigModel));
                    config = ser.ReadObject(ms) as QFConfigModel;
                    ms.Close();
                }
                if (string.IsNullOrEmpty(config.provider))
                {
                    config.provider = "System.Data.SqlClient";
                }
            }
            // if the query defines a QfDefaultConnection, use it.
            var match = Regex.Match(state._3InitialQueryText, "^--QfDefaultConnection(=|:)(?<cstr>[^\r\n]*)", RegexOptions.Multiline);
            if (match.Success)
            {
                config.defaultConnection = match.Groups["cstr"].Value;
                var matchProviderName = Regex.Match(state._3InitialQueryText, "^--QfDefaultConnectionProviderName(=|:)(?<pn>[^\r\n]*)", RegexOptions.Multiline);
                if (matchProviderName.Success)
                {
                    config.provider = matchProviderName.Groups["pn"].Value;
                }
                else
                {
                    config.provider = "System.Data.SqlClient";
                }

            }
            state._4Config = config;
            return state;
        }
    }
}