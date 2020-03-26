using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using QueryFirst;
using Xunit;

namespace QueryFirstTests.StateAndTransitionTests
{
    public class _6FindUndeclaredParameters_Tests
    {
        [Fact]
        public void FindUndeclared_WhenAllGood_PropsAssigned()
        {
            // Arrange
            var provider = new FakeProvider();
            var transition = new _6FindUndeclaredParameters(provider);
            var state = new State
            {
                _5QueryAfterScaffolding = @"/* .sql query managed by QueryFirst add-in */


-- designTime - put parameter declarations and design time initialization here


-- endDesignTime
select * from customers,
",
                _4Config = new QFConfigModel
                {
                    defaultConnection = "SomeConnectionString"
                }
            };
            string outMsg;
            // Act
            transition.Go(ref state, out outMsg);
            // Assert
            state._6FinalQueryTextForCode.Should().NotBeNullOrEmpty();
            state._6FinalQueryTextForCode.Should().NotContain("-- designTime", "designTime flags have been converted to hide section");
            state._6NewParamDeclarations.Should().NotBeNullOrEmpty();
            state._6QueryWithParamsAdded.Should().NotBeNullOrEmpty();
        }
    }
}
