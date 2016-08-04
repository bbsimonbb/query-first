# query-first
World first implementation of the Dominic Strauss-Kahn method of data access.

Query first is a "lightweight, low ceremony" data access tech, for working with SQL in C# projects. Develop your queries using the QueryFirst .sql template (in Visual C# items). When you save your file, QueryFirst will generate a wrapper class and a POCO for your results.

You will need to install the VSIX, then add a connection string QfDefaultConnection to you app or web.config. You will also need to create a class, QfRuntimeConnection, with a method... static string GetConnectionString().
