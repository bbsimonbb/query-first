using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;

namespace QueryFirst
{
    public class BackwardCompatibility
    {
        public void InjectPOCOFactory(ICodeGenerationContext ctx, ProjectItem partialClass)
        {
            bool rememberToClose = false;
            if (!partialClass.IsOpen)
            {
                partialClass.Open();
                rememberToClose = true;
            }
            var textDoc = ((TextDocument)partialClass.Document.Object());
            var ep = textDoc.CreateEditPoint();
            string textOfDoc = ep.GetText(textDoc.EndPoint);
            if (textOfDoc.IndexOf("Results CreatePoco(") == -1)
            {

                StringBuilder bldr = new StringBuilder();
                bldr.AppendLine("// POCO factory, called for each line of results. For polymorphic POCOs, put your instantiation logic here.");
                bldr.AppendLine("// Tag the results class as abstract above, add some virtual methods, create some subclasses, then instantiate them here based on data in the row.");
                bldr.AppendLine("public partial class " + ctx.BaseName + "\n{");
                bldr.AppendLine(ctx.BaseName + "Results CreatePoco(System.Data.IDataRecord record)\n{");
                bldr.AppendLine("return new " + ctx.BaseName + "Results();\n}\n}");

                int insertHere = textOfDoc.LastIndexOf('}');
                string newContents = textOfDoc.Substring(0, insertHere) + bldr.ToString() + textOfDoc.Substring(insertHere, textOfDoc.Length - insertHere);
                ep.ReplaceText(textDoc.EndPoint, newContents, 0);
                ep.SmartFormat(textDoc.EndPoint);
                partialClass.Save();

            }
            if (rememberToClose)
            {
                partialClass.Document.Close();
            }
        }
    }
}
