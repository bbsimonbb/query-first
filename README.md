# query-first
A much smarter way to work with SQL.

Query first is a "lightweight, low ceremony" data access tech, for working with SQL in C# projects. Develop your queries using the QueryFirst .sql template (in Visual C# items). When you save your file, QueryFirst will generate a wrapper class and a POCO for your results.

You will need to install the VSIX, then add a connection string QfDefaultConnection to you app or web.config. You will also need to create a class, QfRuntimeConnection, with a method... static string GetConnectionString().

Read more and download the VSIX [here](https://visualstudiogallery.msdn.microsoft.com/eaf390af-afc1-4994-a442-ec95923dafcb). There's a little code project article [here](www.codeproject.com/Tips/1108776/QueryFirst-Worlds-First-Implementation-of-the-Domi).
