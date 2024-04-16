using System;
using System.Collections.Generic;
using Scot.Massie.EquationParser.Equations;
using Scot.Massie.EquationParser.Utils;

namespace Scot.Massie.EquationParser.EquationSubParsers
{
    internal class ScientificNotationSubParser : IEquationSubParser
    {
        public IEquation? Parse(string equationString, IEquationStores stores, int depthRemaining)
        {
            var eIndex = equationString.IndexOf('e');

            if(eIndex < 0)
                return null;

            if(!double.TryParse(equationString[..eIndex].TrimEnd(), out var mantissa))
                return null;

            if(!double.TryParse(equationString[(eIndex + 1)..].TrimStart(), out var exponent))
                return null;
            
            return new LiteralValue(mantissa * (Math.Pow(10, exponent)));
        }

        public IEnumerable<(IEquation equation, string equationSource)> ReadFromEnd(string          equationString,
                                                                                    IEquationStores stores,
                                                                                    int             depthRemaining,
                                                                                    DepthAdjuster   depthAdjuster)
        {
            var eIndex = equationString.IndexOf('e');

            if(eIndex < 0)
                yield break;

            if(!double.TryParse(equationString[(eIndex + 1)..].TrimStart(), out var exponent))
                yield break;

            var textBeforeE = equationString[..eIndex].TrimEnd();

            foreach(var (mantissa, mantissaSource) in textBeforeE.GetEndingDoublesWithSources())
            {
                var source = equationString[^(equationString.Length - textBeforeE.Length + mantissaSource.Length)..];
                yield return (new LiteralValue(mantissa * (Math.Pow(10, exponent))), source);
            }
        }
    }
}
