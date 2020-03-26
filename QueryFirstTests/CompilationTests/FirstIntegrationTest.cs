//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.CSharp;
//using Microsoft.CodeAnalysis.Emit;
//using QueryFirst;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Reflection;
//using Xunit;
//using System.Text;
//using System.Runtime.Serialization.Json;
//using System;

//namespace QueryFirstTests.CompilationTests
//{
//    public class FirstIntegrationTest
//    {
//        [Fact]
//        public void GenerateSimpleQuery_WhenAllGood_Compiles()
//        {
//            // Arrange
//            var assembly = Assembly.GetExecutingAssembly();

//            var state = new State();
//            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(Helpers.ReadAllTextFromManifestResource("QueryFirstTests.Fixtures.state.json"))))
//            {
//                var ser = new DataContractJsonSerializer(typeof(State));
//                state = ser.ReadObject(ms) as State;
//                ms.Close();
//            }

//            var conductor = new Conductor(null, new WrapperClassMaker(), new ResultClassMaker());
//            // Act
//            var code = conductor.GenerateCode(state);

//            // Compile it
//            var syntaxTree = CSharpSyntaxTree.ParseText(code);
//            string assemblyName = Path.GetRandomFileName();
//            MetadataReference[] references = new MetadataReference[]
//            {
//                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
//                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location)
//            };


//            var trustedAssembliesPaths = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")).Split(Path.PathSeparator);
//            var neededAssemblies = new[]
//            {
//    "System.Runtime",
//    "mscorlib",
//};
//            List references = trustedAssembliesPaths
//                .Where(p => neededAssemblies.Contains(Path.GetFileNameWithoutExtension(p)))
//                .Select(p => MetadataReference.CreateFromFile(p))
//                .ToList();





//            CSharpCompilation compilation = CSharpCompilation.Create(
//                assemblyName,
//                syntaxTrees: new[] { syntaxTree },
//                references: references,
//                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
//            using (var ms = new MemoryStream())
//            {
//                EmitResult result = compilation.Emit(ms);

//                if (!result.Success)
//                {
//                    IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
//                        diagnostic.IsWarningAsError ||
//                        diagnostic.Severity == DiagnosticSeverity.Error);

//                    var errors = new StringBuilder();
//                    foreach (Diagnostic diagnostic in failures)
//                    {
//                        errors.AppendLine(string.Format("{0}: {1}", diagnostic.Id, diagnostic.GetMessage()));
//                    }
//                }
//                else
//                {
//                    ms.Seek(0, SeekOrigin.Begin);
//                    Assembly outAssembly = Assembly.Load(ms.ToArray());
//                }
//            }
//        }
//    }
//}
