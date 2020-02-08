using QueryFirst;
using Newtonsoft.Json;
using Xunit;
using FluentAssertions;

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
            config.DefaultConnection.Should().Be("connectionFromFake");
            config.Provider.Should().Be("providerFromFake");
            config.HelperAssembly.Should().Be("helperAssemblyFromFake");
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
            config.DefaultConnection.Should().Be("connectionFromQuery");
            config.Provider.Should().Be("providerFromQuery");
            config.HelperAssembly.Should().Be("helperAssemblyFromFake");
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
            config.DefaultConnection.Should().Be("connectionFromQuery");
            config.Provider.Should().Be("System.Data.SqlClient");
            config.HelperAssembly.Should().Be("helperAssemblyFromFake");
        }

        // todo we need some tests that target config files and the overriding of values.
    }
    public class FakeConfigFileReader : IConfigFileReader
    {
        public string GetConfigFile(string filePath)
        {
            return JsonConvert.SerializeObject(
                new QFConfigModel
                {
                    DefaultConnection = "connectionFromFake",
                    Provider = "providerFromFake",
                    HelperAssembly = "helperAssemblyFromFake"
                }
            );
        }
    }
}
