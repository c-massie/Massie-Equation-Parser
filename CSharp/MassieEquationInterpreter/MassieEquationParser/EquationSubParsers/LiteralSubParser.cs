using System.Collections.Generic;
using System.Linq;
using Scot.Massie.EquationParser.Equations;
using Scot.Massie.EquationParser.Utils;

namespace Scot.Massie.EquationParser.EquationSubParsers
{
    internal class LiteralSubParser : IEquationSubParser
    {
        public IEquation? Parse(string equationString, IEquationStores stores, int depthRemaining)
        {
            if(!double.TryParse(equationString, out var value))
                return null;

            return new LiteralValue(value);
        }

        public IEnumerable<(IEquation equation, string equationSource)> ReadFromEnd(
            string          equationString,
            IEquationStores stores,
            int             depthRemaining,
            DepthAdjuster   depthAdjuster)
        {
            return equationString.GetEndingDoublesWithSources()
                                 .Select(x => ((IEquation)new LiteralValue(x.result), x.source));
        }
    }
}
