using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;

namespace QueryFirst
{
    public class _3ResolveConfig
    {
        public IConfigFileReader ConfigFileReader { get; set; }
        public IConfigBuilder ConfigBuilder { get; set; }
        /// <summary>
        /// Returns the QueryFirst config for a given query file. Values specified directly in 
        /// the query's qfconfig.json file will override values specified in the project and install qfconfig.json files.
        /// 
        /// If the config specifies a QfDefaultConnection but no QfDefaultConnectionProviderName, "Microsoft.Data.SqlClient"
        /// will be used.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="queryText"></param>
        /// <returns></returns>
        public State Go(State state, QfConfigModel outerConfig)
        {
            // read the config file if there is one.
            var queryConfig = ConfigFileReader.GetQueryConfig(state._1SourceQueryFullPath) ?? new QfConfigModel();



            // to do perhaps. Do we want to make this work again?
            //// if the query defines a QfDefaultConnection, use it.
            //var match = Regex.Match(state._2InitialQueryText, "^--QfDefaultConnection(=|:)(?<cstr>[^\r\n]*)", RegexOptions.Multiline);
            //if (match.Success)
            //{
            //    queryConfig.DefaultConnection = match.Groups["cstr"].Value;
            //    var matchProviderName = Regex.Match(state._2InitialQueryText, "^--QfDefaultConnectionProviderName(=|:)(?<pn>[^\r\n]*)", RegexOptions.Multiline);
            //    if (matchProviderName.Success)
            //    {
            //        queryConfig.Provider = matchProviderName.Groups["pn"].Value;
            //    }
            //    else
            //    {
            //        queryConfig.Provider = "Microsoft.Data.SqlClient";
            //    }

            //}
            state._3Config = ConfigBuilder.Resolve2Configs(outerConfig, queryConfig);
            return state;
        }
    }
}