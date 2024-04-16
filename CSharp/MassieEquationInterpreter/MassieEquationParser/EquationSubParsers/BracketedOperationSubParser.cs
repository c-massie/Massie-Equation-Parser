using System.Collections.Generic;
using System.Linq;
using Scot.Massie.EquationParser.Equations;
using Scot.Massie.EquationParser.Operators;

namespace Scot.Massie.EquationParser.EquationSubParsers
{
    internal class BracketedOperationSubParser : IEquationSubParser
    {
        public IEquation? Parse(string equationString, IEquationStores stores, int depthRemaining)
        {
            bool EquationEndsWithMatchingCloser(IBracketedOperator x)
            {
                var (closerIndex, closer) = stores.StringUtils.GetClosingBracketIndex(
                    equationString, 0, x.OpeningSymbol, new[] { x.ClosingSymbol });

                return closer is { } && closerIndex + closer.Length == equationString.Length;
            }

            var possibleBracketedOperators = stores.BracketedOperators
                                                   .Where(x => equationString.StartsWith(x.OpeningSymbol))
                                                   .Where(EquationEndsWithMatchingCloser)
                                                   .OrderByDescending(x => x.OpeningSymbol.Length)
                                                   .ThenByDescending(x => x.ClosingSymbol.Length);

            foreach(var brop in possibleBracketedOperators)
            {
                var textInsideBrackets 
                    = equationString[(brop.OpeningSymbol.Length)..^(brop.ClosingSymbol.Length)].Trim();

                var operands = ParseOperands(brop, textInsideBrackets, stores, depthRemaining);

                if(operands is null)
                    continue;

                return new BracketedOperation(brop, operands);
            }

            return null;
        }

        public IEnumerable<(IEquation equation, string equationSource)> ReadFromEnd(
            string          equationString,
            IEquationStores stores,
            int             depthRemaining,
            DepthAdjuster   depthAdjuster)
        {
            var possibleBracketedOperators
                = stores.BracketedOperators
                        .Where(op => equationString.EndsWith(op.ClosingSymbol))
                        .Select(op =>
                         {
                             var openingBracketIndex = stores.StringUtils
                                                             .GetOpeningBracketIndex(
                                                                  equationString,
                                                                  equationString.Length - op.ClosingSymbol.Length,
                                                                  op.ClosingSymbol,
                                                                  new[] { op.OpeningSymbol })
                                                             .OpeningBracketIndex;

                             return (op, position: openingBracketIndex);
                         })
                        .Where(op => op.position >= 0)
                        .OrderBy(x => x.position)
                        .ThenByDescending(x => x.op.OpeningSymbol.Length)
                        .ThenByDescending(x => x.op.ClosingSymbol.Length);

            foreach(var (brop, openerPos) in possibleBracketedOperators)
            {
                var textInsideBrackets
                    = equationString[(openerPos + brop.OpeningSymbol.Length)..^(brop.ClosingSymbol.Length)].Trim();

                var trailingEquationLength = equationString.Length - openerPos;

                depthRemaining = depthAdjuster(trailingEquationLength);
                
                var operands = ParseOperands(brop, textInsideBrackets, stores, depthRemaining);

                if(operands is null)
                    continue;

                yield return (new BracketedOperation(brop, operands), equationString[openerPos..]);
            }
        }

        private IList<IEquation>? ParseOperands(IBracketedOperator brop,
                                                string             textInsideBrackets,
                                                IEquationStores    stores,
                                                int                depthRemaining)
        {
            if(textInsideBrackets.Length == 0)
                return new List<IEquation>();

            if(brop.AllowsMultipleOperands)
                return EquationParser.ParseArgumentList(textInsideBrackets, stores, depthRemaining);

            var operand = EquationParser.Parse(textInsideBrackets, stores, depthRemaining);
            return operand is null ? null : new[] { operand };
        }
    }
}
