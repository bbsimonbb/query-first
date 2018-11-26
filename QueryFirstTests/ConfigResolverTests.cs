using QueryFirst;
using Newtonsoft.Json;
using Xunit;
using FluentAssertions;

namespace QueryFirstTests
{
    public class ConfigResolverTests
    {
        private ConfigResolver _configResolver;
        public ConfigResolverTests()
        {
            _configResolver = new ConfigResolver(new FakeConfigFileReader());
        }
        [Fact]
        public void When_NoValuesInQuery_ReturnsValuesFromFile()
        {
            // arrange
            var queryText = "no special values in here";
            // act
            var config = _configResolver.GetConfig("c:\\somePath", queryText);
            // assert
            config.DefaultConnection.Should().Be("connectionFromFake");
            config.Provider.Should().Be("providerFromFake");
            config.HelperAssembly.Should().Be("helperAssemblyFromFake");
        }
        [Fact]
        public void When_ConnectionAndProviderInQuery_ReturnsValueFromQuery()
        {
            // arrange
            var queryText = @"/* .sql query managed by QueryFirst add-in */


/*designTime - put parameter declarations and design time initialization here


endDesignTime*/
--QfDefaultConnection=connectionFromQuery
--QfDefaultConnectionProviderName=providerFromQuery
";
            // act
            var config = _configResolver.GetConfig("c:\\somePath", queryText);
            // assert
            config.DefaultConnection.Should().Be("connectionFromQuery");
            config.Provider.Should().Be("providerFromQuery");
            config.HelperAssembly.Should().Be("helperAssemblyFromFake");
        }
        [Fact]
        public void When_ConnectionInQueryWOProvider_ReturnsDefaultProvider()
        {
            // arrange
            var queryText = @"/* .sql query managed by QueryFirst add-in */


/*designTime - put parameter declarations and design time initialization here


endDesignTime*/
--QfDefaultConnection=connectionFromQuery
";
            // act
            var config = _configResolver.GetConfig("c:\\somePath", queryText);
            // assert
            config.DefaultConnection.Should().Be("connectionFromQuery");
            config.Provider.Should().Be("System.Data.SqlClient");
            config.HelperAssembly.Should().Be("helperAssemblyFromFake");
        }
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
