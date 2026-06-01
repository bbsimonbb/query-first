![ibm-sj-ad-1962.jpg](ibm-sj-ad-1962.jpg)

# QueryFirst - Frictionless Data Access
The QueryFirst extension for visual studio generates a C# wrapper for your .sql files. The documentation is on **[queryfirst.net](https://www.queryfirst.ovh)**.

### Author your SQL in a real environment
 Syntax checking- Intellisense for tables and columns - Get familiar with your data

### A generated C# wrapper
- Repo, POCO, Interface.
- Repository pattern - Parameter discovery - Type safety - POCOs generated from your result schema 

- Supports Sql Server, Postgres and MySql
- Supports all C# project types including .net core.

**To use QueryFirst, install the extension, then use the QueryFirst .sql template under Add => New item => Visual C# Items**

### TL;dr

Query-first is a visual studio extension for working intelligently with SQL in C# projects. Use the provided .sql template to develop your queries. When you save the file, Query-first runs your query, retrieves the schema and generates two classes and an interface: a wrapper class with methods Execute(), ExecuteScalar(), ExecuteNonQuery() etc, its corresponding interface, and a POCO encapsulating a line of results.

**As such, your query stays intact** in its own .sql file. It can be edited, validated and test-run "in-situ" with design-time mock inputs and intellisense for your tables and columns. In your application code, running your query takes one line of code, and returns a POCO (or an IEnumerable of POCOs, or a List of POCOs) with meaningful parameter and property names, so _enabling intellisense for your input parameters and results_. The interface and POCO are directly usable for unit testing.

The generated code stands alone. There is no runtime dll and no reflection. The only dependencies are System libraries. You can quietly forget about ADO : Command, Connection, Reader and parameter creation are all handled for you, with best practices baked in. At no point do you have to remember the name of a column, or its type, or its index in the reader. And you've absolutely nothing new  to learn, provided you still remember how to write SQL :-)

QueryFirst puts a command in your Tools menu that will run all queries in your application and regenerate all wrapper classes. As such, you can integration-test all your queries at any time, and changes in your database schema will directly produce compilation errors in your application. If the query is broken, the wrapper classes will not compile. If the query runs but your code tries to access columns that are no longer present in the result, the error will point at _the line in your code_ that tries to access the missing property.

QueryFirst, first published in 2016, represents an original approach to the database problem _in any language_. Your comments are very welcome. 

### Ambitious?
We expect that QueryFirst will quickly become your favourite data access method. Please urgently let us know if this is not the case.

|                                                       |Dapper|EF|Stored procs|QueryFirst|
|:-------------------------------------|:-------:|:-:|:--------------:|:---------:|
| Lightweight, low ceremony |x|||x|
| Does whatever SQL can do |x||x|x|
| Interact with your data as you develop projections |||x|x|
| Type safety/compile-time checking of your projection ||x|meh|x|
| No POCOs to maintain ||x|x*|x|
| Provides an interface to facilitate testing the calling code ||x|x*|x|
| Prevents SQL injection |**|x|x|x|
| Queries distributed and versioned with application code |x|x||x|
| Bundles an integration test to verify your production schema ||||x|
| Typesafe dynamic ORDER BY||x||x|
| Expando params (EF Contains)|x|x||x|
| Scaffolded INSERTS and UPDATES ||na||x|
| Generated Typescript interfaces from your projections ||||x|

\* only with the help of EF reverse POCO generator (not actually part of EF), but you'll need to open your solution _as administrator_, find the .tt and right-click on 'run custom tool'. Not kidding.

\** Dapper makes parameters much easier, so you're encouraged, but not obliged, to use them. 







**[As seen on youtube !](https://www.youtube.com/watch?v=QkJDccYSw8A)**

