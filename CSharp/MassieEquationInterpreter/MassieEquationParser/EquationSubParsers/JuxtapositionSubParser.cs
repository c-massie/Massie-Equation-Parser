using System;
using System.Collections.Generic;
using System.Linq;
using Scot.Massie.EquationParser.Equations;
using Scot.Massie.EquationParser.Utils;

namespace Scot.Massie.EquationParser.EquationSubParsers
{
    internal class JuxtapositionSubParser : IEquationSubParser
    {
        public IEquation? Parse(string          equationString,
                                IEquationStores stores,
                                int depthRemaining)
        {
            // Note: I don't have to check for postfix operators in the right juxtapand - juxtaposition should always
            //       have a higher precedence than any operators, so any unparsed equation should already have all
            //       operators on the rightmost side processed before it reaches this point.

            if(stores.JuxtapositionFunc is null)
                return null;
            
            // ReSharper disable once CollectionNeverUpdated.Local Values are only set via default supplier.
            var savedRightJuxtapandLengthResults
                = new DefaultingDictionary<int, (int newDepth, int rightJuxtapandLengthWithPrefixes)>(
                    rightJuxtapandLength =>
                    {
                        var leftJuxtapand = equationString[..^rightJuxtapandLength].TrimEnd();
                        var newDepth      = depthRemaining;
                        
                        while(true)
                        {
                            var endingPrefixLength
                                = stores.OperatorGroups
                                        .SelectAndCombineMany(
                                             x => x.Value.LeftAssociativeOperators?.PrefixOperators,
                                             x => x.Value.RightAssociativeOperators?.PrefixOperators)
                                         // ReSharper disable once AccessToModifiedClosure
                                        .Where(x => leftJuxtapand.EndsWith(x.Symbol))
                                        .Select(x => x.Symbol.Length)
                                        .OrderByDescending(x => x)
                                        .FirstOrDefault(0);

                            if(endingPrefixLength == 0)
                                break;

                            newDepth--;
                            leftJuxtapand = leftJuxtapand[..^endingPrefixLength].TrimEnd();
                        }

                        return (newDepth, equationString.Length - leftJuxtapand.Length);
                    });

            var rightJuxtapandLengthsAlreadyTried = new HashSet<int>();
            
            foreach(var subParser in EquationParser.SubParsers)
            {
                if(subParser is JuxtapositionSubParser || subParser is OperationSubParser)
                    continue;

                var possibleRightJuxtapands
                    = subParser.ReadFromEnd(equationString, stores, depthRemaining,
                                            (length) => savedRightJuxtapandLengthResults[length].newDepth);

                foreach(var (_, rightJuxtapandText) in possibleRightJuxtapands)
                {
                    // Don't try to go down a road where I parse the left juxtapand if I've already established that the
                    // left juxtapand of the same length cannot be parsed.
                    if(!rightJuxtapandLengthsAlreadyTried.Add(rightJuxtapandText.Length))
                        continue;
                    
                    var rightJuxtapandLengthWithPrefixes
                        = savedRightJuxtapandLengthResults[rightJuxtapandText.Length]
                           .rightJuxtapandLengthWithPrefixes;
                    
                    var leftJuxtapandText = equationString[..^(rightJuxtapandLengthWithPrefixes)].TrimEnd();
                    var leftJuxtapand     = EquationParser.Parse(leftJuxtapandText, stores, depthRemaining);

                    if(leftJuxtapand is null)
                        continue;

                    var rightJuxtapandTextWithPrefixes = equationString[^rightJuxtapandLengthWithPrefixes..];
                    var rightJuxtapand = EquationParser.Parse(rightJuxtapandTextWithPrefixes, stores, depthRemaining);

                    if(rightJuxtapand is null)
                    {
                        throw new Exception("The right juxtapand should be able to parsed where the subparser has "
                                          + "already given that text as an example of an equation that could be read "
                                          + "from the end.");
                    }

                    return new Juxtaposition(stores.JuxtapositionFunc!, leftJuxtapand, rightJuxtapand);
                }
            }

            return null;
        }

        public IEnumerable<(IEquation equation, string equationSource)> ReadFromEnd(
            string          equationString,
            IEquationStores stores,
            int             depthRemaining,
            DepthAdjuster   depthAdjuster)
        {
            throw new NotSupportedException(
                "Reading an operation by juxtaposition from the end of a string isn't supported.");
        }
    }
}
