using System.Collections.Generic;
using System.Linq;
using Scot.Massie.EquationParser.Equations;

namespace Scot.Massie.EquationParser.EquationSubParsers
{
    internal class VariableAccessSubParser : IEquationSubParser
    {
        public IEquation? Parse(string equationString, IEquationStores stores, int depthRemaining)
        {
            if(!stores.Variables.ContainsKey(equationString))
                return null;
            
            return new VariableAccess(equationString, stores.Variables);
        }

        public IEnumerable<(IEquation equation, string equationSource)> ReadFromEnd(
            string          equationString,
            IEquationStores stores,
            int             depthRemaining,
            DepthAdjuster   depthAdjuster)
        {
            var variableNames = stores.Variables
                                      .Keys
                                      .Where(equationString.EndsWith)
                                      .OrderByDescending(vname => vname.Length);

            foreach(var varName in variableNames)
                yield return (new VariableAccess(varName, stores.Variables), varName);
        }
    }
}
