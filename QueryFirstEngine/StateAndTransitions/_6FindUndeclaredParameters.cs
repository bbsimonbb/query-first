using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryFirst
{
    public class _6FindUndeclaredParameters
    {
        private IProvider _provider;
        public _6FindUndeclaredParameters(IProvider provider)
        {
            _provider = provider;
        }
        public State Go(ref State state, out string outputMessage)
        {
            if (state == null)
                throw new ArgumentNullException( nameof(state) );
            // also called in the bowels of schema fetching, for Postgres, because no notion of declarations.
            var undeclared = _provider.FindUndeclaredParameters(state._5QueryAfterScaffolding, state._4Config.defaultConnection, out outputMessage);
            state._6NewParamDeclarations = _provider.ConstructParameterDeclarations(undeclared);            
            state._6QueryWithParamsAdded = state._5QueryAfterScaffolding.Replace("-- endDesignTime", state._6NewParamDeclarations + "-- endDesignTime");
            state._6FinalQueryTextForCode = state._6QueryWithParamsAdded
                    .Replace("-- designTime", "/*designTime")
                    .Replace("-- endDesignTime", "endDesignTime*/")
                    // for inclusion in a verbatim string, only modif required is to double double quotes
                    .Replace("\"", "\"\"");

            return state;
        }
    }
}
