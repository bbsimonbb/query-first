using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryFirst
{
    class SignatureMaker : ISignatureMaker
    {
        public void MakeMethodAndCallingSignatures(List<QueryParamInfo> ParamNamesAndTypes, out string MethodSignature, out string CallingSignature)
        {
            StringBuilder sig = new StringBuilder();
            StringBuilder call = new StringBuilder();
            foreach (var qp in ParamNamesAndTypes)
            {
                sig.Append(qp.CSType + ' ' + qp.CSName + ", ");
                call.Append(qp.CSName + ", ");
            }
            //signature trailing comma trimmed in place if needed. 
            call.Append("conn"); // calling args always used to call overload with connection
            MethodSignature = sig.ToString();
            CallingSignature = call.ToString();
        }
    }
}
