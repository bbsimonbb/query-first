using System;
using System.Text;

namespace QueryFirst
{
    public class _8ParseOrFindDeclaredParams
    {
        IProvider _provider;
        public _8ParseOrFindDeclaredParams(IProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));
            _provider = provider;
        }
        public State Go(ref State state)
        {
            var queryParams = _provider.ParseDeclaredParameters(state._6QueryWithParamsAdded, state._4Config.DefaultConnection);

            StringBuilder sig = new StringBuilder();
            StringBuilder call = new StringBuilder();

            foreach (var qp in queryParams)
            {
                sig.Append(qp.CSType + ' ' + qp.CSName + ", ");
                call.Append(qp.CSName + ", ");
            }
            //signature trailing comma trimmed in place if needed. 

            state._8QueryParams = queryParams;
            state._8MethodSignature = sig.ToString();
            state._8CallingArgs = call.ToString();

            return state;
        }

    }
}
