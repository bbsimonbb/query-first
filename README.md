# query-first
A much smarter way to work with SQL.

Query first is a "lightweight, low ceremony" data access tech, for working with SQL in C# projects. Develop your queries using the QueryFirst .sql template (in Visual C# items). When you save your file, QueryFirst will generate a wrapper class and a POCO for your results.

You will need to install the VSIX, then in your project, create qfconfig.json beside or above your .sql files. To create a query, choose New QueryFirstQuery from the Add => New Item dialogue.

```
// qfconfig.json
{
  "defaultConnection": "Data Source=localhost;Initial Catalog=Northwind;Integrated Security=True",
  "provider":["System.Data.SqlClient"(the default)/"MySql.Data.MySqlClient"/"Npgsql"],
  "helperAssembly":[pathToDll],
  "makeSelfTest":true/false
} 

// QfRuntimeConnection.cs
using System.Data;
using System.Data.SqlClient;

namespace CoreWebAppSqlServer
{
    public class QfRuntimeConnection
    {
        public static IDbConnection GetConnection()
        {
            return new SqlConnection("Data Source=localhost;Initial Catalog=Northwind;Integrated Security=True");
        }
    }
}
```

Read more and download the VSIX [here](https://visualstudiogallery.msdn.microsoft.com/eaf390af-afc1-4994-a442-ec95923dafcb). There's a little code project article [here](https://www.codeproject.com/Tips/1108776/QueryFirst-Worlds-First-Implementation-of-the-Domi).
