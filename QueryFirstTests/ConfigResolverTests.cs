using QueryFirst;
using Xunit;
using FluentAssertions;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text;

namespace QueryFirstTests
{
    public class ConfigResolverTests
    {
        private _4ResolveConfig _configResolver;
        public ConfigResolverTests()
        {
            _configResolver = new _4ResolveConfig(new FakeConfigFileReader());
        }
        [Fact]
        public void When_NoValuesInQuery_ReturnsValuesFromFile()
        {
            // arrange
            var state = new State
            {
                _1CurrDir = "c:\\somePath",
                _3InitialQueryText = "no special values in here"
            };
            // act
            var config = _configResolver.Go(state)._4Config;
            // assert
            config.defaultConnection.Should().Be("connectionFromFake");
            config.provider.Should().Be("providerFromFake");
            config.helperAssembly.Should().Be("helperAssemblyFromFake");
        }
        [Fact]
        public void When_ConnectionAndProviderInQuery_ReturnsValueFromQuery()
        {
            // arrange
            var state = new State
            {
                _1CurrDir = "c:\\somePath",
                _3InitialQueryText = @"/* .sql query managed by QueryFirst add-in */


/*designTime - put parameter declarations and design time initialization here


endDesignTime*/
--QfDefaultConnection=connectionFromQuery
--QfDefaultConnectionProviderName=providerFromQuery
"
            };
            // act
            var config = _configResolver.Go(state)._4Config;
            // assert
            config.defaultConnection.Should().Be("connectionFromQuery");
            config.provider.Should().Be("providerFromQuery");
            config.helperAssembly.Should().Be("helperAssemblyFromFake");
        }
        [Fact]
        public void When_ConnectionInQueryWOProvider_ReturnsDefaultProvider()
        {
            // arrange
            var state = new State
            {
                _1CurrDir = "c:\\somePath",
                _3InitialQueryText = @"/* .sql query managed by QueryFirst add-in */


/*designTime - put parameter declarations and design time initialization here


endDesignTime*/
--QfDefaultConnection=connectionFromQuery
"
            };
            // act
            var config = _configResolver.Go(state)._4Config;
            // assert
            config.defaultConnection.Should().Be("connectionFromQuery");
            config.provider.Should().Be("System.Data.SqlClient");
            config.helperAssembly.Should().Be("helperAssemblyFromFake");
        }

        // todo we need some tests that target config files and the overriding of values.
    }
    public class FakeConfigFileReader : IConfigFileReader
    {
        public string GetConfigFile(string filePath)
        {
            var config = new QFConfigModel
            {
                defaultConnection = "connectionFromFake",
                provider = "providerFromFake",
                helperAssembly = "helperAssemblyFromFake"
            };

            using (var ms = new MemoryStream())
            {
                var ser = new DataContractJsonSerializer(typeof(QFConfigModel));
                ser.WriteObject(ms, config);
                byte[] json = ms.ToArray();
                ms.Close();
                return Encoding.UTF8.GetString(json, 0, json.Length);
            }


        }
    }
}
