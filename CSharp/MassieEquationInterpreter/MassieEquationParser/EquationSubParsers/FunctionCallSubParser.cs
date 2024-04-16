using System.Collections.Generic;
using System.Linq;
using Scot.Massie.EquationParser.Equations;

namespace Scot.Massie.EquationParser.EquationSubParsers
{
    internal class FunctionCallSubParser : IEquationSubParser
    {
        public IEquation? Parse(string equationString, IEquationStores stores, int depthRemaining)
        {
            if(!equationString.EndsWith(stores.ClosingBracketSymbol))
                return null;
            
            var closingBracketIndex = equationString.Length - stores.ClosingBracketSymbol.Length;
            var openingBracketIndex = stores.StringUtils.GetOpeningBracketIndex(equationString, closingBracketIndex);
            
            if(openingBracketIndex is -1)
                return null;
            
            var functionName = equationString[..openingBracketIndex].TrimEnd();
            
            if(!stores.Functions.ContainsKey(functionName))
                return null;
            
            var argsIndex  = openingBracketIndex + stores.OpeningBracketSymbol.Length;
            var argsString = equationString[(argsIndex)..^(stores.ClosingBracketSymbol.Length)];
            var args       = EquationParser.ParseArgumentList(argsString, stores, depthRemaining);
            
            if(args is null)
                return null;
            
            return new FunctionCall(functionName, args, stores.Functions);
        }

        public IEnumerable<(IEquation equation, string equationSource)> ReadFromEnd(
            string          equationString,
            IEquationStores stores,
            int             depthRemaining,
            DepthAdjuster   depthAdjuster)
        {
            if(!equationString.EndsWith(stores.ClosingBracketSymbol))
                yield break;
            
            var closingBracketIndex = equationString.Length - stores.ClosingBracketSymbol.Length;
            var openingBracketIndex = stores.StringUtils.GetOpeningBracketIndex(equationString, closingBracketIndex);

            if(openingBracketIndex is -1)
                yield break;
            
            var textBeforeBrackets        = equationString[..openingBracketIndex];
            var textBeforeBracketsTrimmed = textBeforeBrackets.TrimEnd();

            var possibleFunctionNames = stores.Functions
                                              .Keys
                                              .Where(fname => textBeforeBracketsTrimmed.EndsWith(fname))
                                              .OrderByDescending(fname => fname.Length)
                                              .ToList();
            
            if(possibleFunctionNames.Count == 0)
                yield break;
            
            var argsIndex  = openingBracketIndex + stores.OpeningBracketSymbol.Length;
            var argsString = equationString[argsIndex..closingBracketIndex];

            foreach(var fname in possibleFunctionNames)
            {
                var argDepth = depthAdjuster(equationString.Length - textBeforeBracketsTrimmed.Length);
                var args     = EquationParser.ParseArgumentList(argsString, stores, argDepth);

                if(args is null)
                    continue;
                
                var textBeforeFcall = textBeforeBracketsTrimmed[..^(fname.Length)];
                var fcallString     = equationString[(textBeforeFcall.Length)..];
                var fcall           = new FunctionCall(fname, args, stores.Functions);

                yield return (fcall, fcallString);
            }
        }
    }
}
