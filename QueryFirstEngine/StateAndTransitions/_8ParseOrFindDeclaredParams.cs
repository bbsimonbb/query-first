using System;
using System.Linq;
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
            var queryParams = _provider.ParseDeclaredParameters(state._6QueryWithParamsAdded, state._4Config.defaultConnection);
            if (queryParams.Where(param => param.DbType == "UserDefinedTableType").Count() != 0)
                state._8HasTableValuedParams = true;
            var fullSig = new StringBuilder();
            var inputOnlySig = new StringBuilder();
            var callSig = new StringBuilder();
            var inputOnlyCallSig = new StringBuilder();

            foreach (var qp in queryParams)
            {
                string modifier;
                if (qp.IsInput && qp.IsOutput)
                    modifier = "ref ";
                else if (qp.IsOutput)
                    modifier = "out ";
                else
                {
                    modifier = "";
                    inputOnlySig.Append(modifier + qp.CSType + ' ' + qp.CSNameCamel + ", ");
                    inputOnlyCallSig.Append(modifier + qp.CSNameCamel + ", ");
                }

                fullSig.Append(modifier + qp.CSType + ' ' + qp.CSNameCamel + ", ");
                callSig.Append(modifier + qp.CSNameCamel + ", ");
            }
            //signature trailing comma trimmed in place if needed. 

            state._8QueryParams = queryParams;
            state._8MethodSignature = fullSig.ToString();
            state._8CallingArgs = callSig.ToString();
            state._8InputOnlyCallingArgs = inputOnlyCallSig.ToString();
            state._8InputOnlyMethodSignature = inputOnlySig.ToString();
            state._8HookupExecutionMessagesMethodText = _provider.HookUpForExecutionMessages();

            return state;
        }

    }
}
