using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
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
        public QFConfigModel GetConfigObj(string filePath)
        {
            try
            {
                var configFileContents = GetConfigFile(filePath);
                if (!string.IsNullOrEmpty(configFileContents))
                {
                    using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(configFileContents)))
                    {
                        var ser = new DataContractJsonSerializer(typeof(QFConfigModel));
                        var config = ser.ReadObject(ms) as QFConfigModel;
                        ms.Close();
                        return config;
                    }
                }

                else return null;
            }
            catch (Exception ex)
            {
                throw new Exception("Error deserializing qfconfig.json. Is there anything funny in there?", ex);
            }
        }
    }


    public class QFConfigModel
    {
        public string defaultConnection { get; set; }
        public string provider { get; set; }
        public string helperAssembly { get; set; }
        public bool makeSelfTest { get; set; }
        public bool connectEditor2DB { get; set; }
    }
}
