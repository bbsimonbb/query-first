using System.IO;
using System.Reflection;

namespace QueryFirstTests
{
    public class Helpers
    {
        public static string ReadAllTextFromManifestResource(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
