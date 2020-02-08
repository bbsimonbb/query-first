using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinyIoC;

namespace QueryFirst
{
    public interface IResultClassMaker
    {
        string Usings();
        string StartClass(State state);
        string MakeProperty(ResultFieldDetails fld);
        string CloseClass();
    }
    public class ResultClassMaker : IResultClassMaker
    {
        public virtual string Usings() { return ""; }

        private string nl = Environment.NewLine;
        public virtual string StartClass(State state)
        {
            return string.Format("public partial class {0} {{" + nl, state._2ResultClassName);
        }
        public virtual string MakeProperty(ResultFieldDetails fld)
        {
            StringBuilder code = new StringBuilder();
            code.AppendLine($"protected {fld.TypeCsShort} _{fld.CSColumnName}; //({fld.TypeDb} {(fld.AllowDBNull ? "null" : "not null")})");
            code.AppendLine($"public {fld.TypeCsShort} {fld.CSColumnName}{{\nget{{return _{fld.CSColumnName};}}\nset{{_{fld.CSColumnName} = value;}}\n}}");
            return code.ToString();
        }

        public virtual string CloseClass()
        {
            return "}" + nl;
        }
    }
}
