using FluentAssertions;
using QueryFirst;
using System.Collections.Generic;
using Xunit;

namespace QueryFirstTests.StateAndTransitionTests
{
    public class _5ScaffoldUpdateOrInsertTests
    {
        [Fact]
        public void ScaffoldInsert_WhenAllGood_ConstructsStatement()
        {
            // arrange
            var maker = new _5ScaffoldUpdateOrInsert(new FakeSchemaFetcher());
            var state = new State
            {
                _3InitialQueryText = "insert into customers...",
                _4Config = new QFConfigModel
                {
                    defaultConnection = "some;connection;string",
                    provider = "System.ZQL"
                }
            };
            // act
            maker.Go(ref state);
            // assert
            var desiredResult =
@"/* .sql query managed by QueryFirst add-in */


-- designTime - put parameter declarations and design time initialization here


-- endDesignTime
INSERT INTO customers (
customerId,
customerFirstName
)
VALUES (
@customerId,
@customerFirstName
)";
            state._5QueryAfterScaffolding.Should().Be(desiredResult);
        }
    }
    public class FakeSchemaFetcher : ISchemaFetcher
    {
        public List<ResultFieldDetails> GetFields(string connectionString, string provider, string Query)
        {
            return new List<ResultFieldDetails>
            {
                new ResultFieldDetails
                {
                    ColumnName = "customerId"                    
                },
                new ResultFieldDetails
                {
                    ColumnName = "customerFirstName"
                }
            };
        }
    }
}
