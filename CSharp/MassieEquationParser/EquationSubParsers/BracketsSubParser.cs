using System;
using System.Collections.Generic;
using Scot.Massie.EquationParser.Equations;

namespace Scot.Massie.EquationParser.EquationSubParsers
{
    internal class BracketsSubParser : IEquationSubParser
    {
        public IEquation? Parse(string equationString, IEquationStores stores, int depthRemaining)
        {
            if(!equationString.StartsWith(stores.OpeningBracketSymbol))
                return null;

            int matchingClosingBracketIndex             = stores.StringUtils.GetClosingBracketIndex(equationString, 0);
            int matchingClosingBracketIndexIfInBrackets = equationString.Length - stores.ClosingBracketSymbol.Length;
            
            if(matchingClosingBracketIndex != matchingClosingBracketIndexIfInBrackets)
                return null;

            var textInBrackets
                = equationString[(stores.OpeningBracketSymbol.Length)..^(stores.ClosingBracketSymbol.Length)];

            if(textInBrackets.Trim().Length == 0)
                return null;
            
            return EquationParser.Parse(textInBrackets, stores, depthRemaining);
        }

        public IEnumerable<(IEquation equation, string equationSource)> ReadFromEnd(
            string          equationString,
            IEquationStores stores,
            int             depthRemaining,
            DepthAdjuster   depthAdjuster)
        {
            if(!equationString.EndsWith(stores.ClosingBracketSymbol, StringComparison.Ordinal))
                yield break;

            var closingBracketLength   = stores.ClosingBracketSymbol.Length;
            var closingBracketPosition = equationString.Length - closingBracketLength;
            
            var openingBracketIndex  = stores.StringUtils.GetOpeningBracketIndex(equationString,
                                                                                 closingBracketPosition);
            
            if(openingBracketIndex < 0)
                yield break;

            var textInBrackets
                = equationString[(openingBracketIndex + stores.OpeningBracketSymbol.Length)..(closingBracketPosition)];
            
            if(textInBrackets.Trim().Length == 0)
                yield break;

            var trailingEquationLength = equationString.Length - openingBracketIndex;
            depthRemaining = depthAdjuster(trailingEquationLength);
            
            var equation = EquationParser.Parse(textInBrackets, stores, depthRemaining);
            
            if(equation is null)
                yield break;
            
            yield return (equation, equationString[openingBracketIndex..]);
        }
    }
}
