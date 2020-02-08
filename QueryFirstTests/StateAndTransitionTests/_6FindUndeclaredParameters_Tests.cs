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
        public void FindUndeclared_WhenAllGood_PropsAssigned()
        {
            // Arrange
            var provider = new FakeProvider();
            var transition = new _6FindUndeclaredParameters(provider);
            var state = new State
            {
                _3InitialQueryText = @"/* .sql query managed by QueryFirst add-in */


-- designTime - put parameter declarations and design time initialization here


-- endDesignTime
select * from customers
"
            };
            string outMsg;
            // Act
            transition.Go(ref state, out outMsg);
            // Assert
            state._6FinalQueryTextForCode.Should().NotBeNullOrEmpty();
            state._6NewParamDeclarations.Should().NotBeNullOrEmpty();
            state._6QueryWithParamsAdded.Should().NotBeNullOrEmpty();
        }
    }
}
