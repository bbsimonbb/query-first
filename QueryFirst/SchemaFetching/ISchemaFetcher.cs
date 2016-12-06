using System.Collections.Generic;
using System.Configuration;

namespace QueryFirst
{
    public interface ISchemaFetcher
    {
        List<ResultFieldDetails> GetFields( ConnectionStringSettings strconn, string Query);
    }
}