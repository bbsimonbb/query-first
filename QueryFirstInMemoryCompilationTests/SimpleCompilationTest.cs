using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using QueryFirst;
using QueryFirstTests;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using Xunit;

namespace QueryFirstInMemoryCompilationTests
{
    public class FirstIntegrationTest
    {
        [Fact]
        public void GenerateSimpleQuery_WhenAllGood_Compiles()
        {
            // Arrange
            var errors = new StringBuilder();
            var assembly = Assembly.GetExecutingAssembly();

            var state = new State();
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(Helpers.ReadAllTextFromManifestResource("QueryFirstTests.Fixtures.state.json"))))
            {
                var ser = new DataContractJsonSerializer(typeof(State));
                state = ser.ReadObject(ms) as State;
                ms.Close();
            }

            var conductor = new Conductor(null, new WrapperClassMaker(), new ResultClassMaker());
            // Act
            var code = conductor.GenerateCode(state);

            // Compile it
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var userPartial = CSharpSyntaxTree.ParseText(state._2UserPartialFullText);
            var qfRuntimeConnection = CSharpSyntaxTree.ParseText(@"
using System.Data;
using System.Data.SqlClient;

namespace QFNorthwind
{
    class QfRuntimeConnection
    {
        public static IDbConnection GetConnection()
        {
            return new SqlConnection(""Server = localhost\\SQLEXPRESS; Database = NORTHWND; Trusted_Connection = True; "");
        }
    }
}
");

            string assemblyName = Path.GetRandomFileName();
            MetadataReference[] references = new MetadataReference[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(SqlConnection).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(MemoryStream).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(List<string>).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Component).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Conductor).Assembly.Location),
            };


            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: new[] { syntaxTree, userPartial, qfRuntimeConnection },
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using (var ms = new MemoryStream())
            {
                EmitResult result = compilation.Emit(ms);

                if (!result.Success)
                {
                    IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == DiagnosticSeverity.Error);

                    foreach (Diagnostic diagnostic in failures)
                    {
                        errors.AppendLine(string.Format("{0}: {1}", diagnostic.Id, diagnostic.GetMessage()));
                    }
                }
                else
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    Assembly outAssembly = Assembly.Load(ms.ToArray());
                }
            }
            // Assert
            errors.ToString().Should().BeEmpty();
        }
    }
}
