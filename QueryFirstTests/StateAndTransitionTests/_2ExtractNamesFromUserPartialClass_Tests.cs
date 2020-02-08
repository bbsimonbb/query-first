using FluentAssertions;
using QueryFirst;
using Xunit;

namespace QueryFirstTests.StateAndTransitionTests
{
    public class _2ExtractNamesFromUserPartialClass_Tests
    {
        [Fact]
        public void ExtractNames_WhenClassOnly_ClassAndNamespaceExtracted()
        {
            // Arrange
            var state = new State();
            var userPartial = @"
using System;

namespace QfTestConsoleFramework
{
    [Serializable]
    public partial class SharesightTrade
    {
        // Partial class extends the generated results class
        // Serializable by default, but you can change this here		
        // Put your methods here :-)
        internal void OnLoad()
        {
        }
    }
    // POCO factory, called for each line of results. For polymorphic POCOs, put your instantiation logic here.
    // Tag the results class as abstract above, add some virtual methods, create some subclasses, then instantiate them here based on data in the row.
    public partial class GetSharesightConfs
    {
        SharesightTrade CreatePoco(System.Data.IDataRecord record)
        {
            return new SharesightTrade();
        }
    }
}
            ";
            // Act
            new _2ExtractNamesFromUserPartialClass().Go(state, userPartial);
            // Assert
            state._2ResultClassName.Should().Be("SharesightTrade");
            state._2ResultInterfaceName.Should().Be("");
            state._2Namespace.Should().Be("QfTestConsoleFramework");

        }
    }
}
