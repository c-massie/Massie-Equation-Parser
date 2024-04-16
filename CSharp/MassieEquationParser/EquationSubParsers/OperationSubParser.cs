using System;
using System.Collections.Generic;
using System.Linq;
using Scot.Massie.EquationParser.Equations;
using Scot.Massie.EquationParser.Operators;
using Scot.Massie.EquationParser.Utils;

namespace Scot.Massie.EquationParser.EquationSubParsers
{
    internal class OperationSubParser : IEquationSubParser
    {
        public IEquation? Parse(string s, IEquationStores stores, int depthRemaining)
        {
            return stores.OperatorGroupsInOrder
                         .Select(x => ParseOperationFromGroup(s, stores, x, depthRemaining))
                         .FirstOrDefault(x => !(x is null));
        }

        private static IEquation? ParseOperationFromGroup(string          s,
                                                          IEquationStores stores,
                                                          IOperatorGroup  operatorGroup,
                                                          int             depthRemaining)
        {
            if(operatorGroup.LeftAssociativeOperators is { } leftAssocOps)
            {
                var result = ParseInfixOperation(s, stores, leftAssocOps, depthRemaining)
                          ?? ParseLeftAssociativeAffixOperation(s, stores, leftAssocOps, depthRemaining);
                
                if(result is { })
                    return result;
            }
            
            if(operatorGroup.RightAssociativeOperators is { } rightAssocOps)
            {
                var result = ParseInfixOperation(s, stores, rightAssocOps, depthRemaining)
                          ?? ParseRightAssociativeAffixOperation(s, stores, rightAssocOps, depthRemaining);
            
                if(result is { })
                    return result;
            }
            
            return null;
        }

        private static IEquation? ParseInfixOperation(string                    s,
                                                      IEquationStores           stores,
                                                      IAssociativeOperatorGroup associativeOperatorGroup,
                                                      int                       depthRemaining)
        {
            // Where two infix operators have the same precedence, the one that appears "first" (first symbol being
            // left-most if it's left-associative, last symbol being right-most if it's right-associative) is considered
            // to have the higher precedence. e.g. "a + b + c" would be equivalent to "(a + b) + c".
            //
            // Where two infix operators have the same precedence and are both tied for appearing "first" in the
            // equation, the one whose final symbol appears "last" (last symbol ending at the right-most position if
            // left-associative, first symbol starting at the left-most position if right-associative) is considered to
            // have the higher precedence.
            //
            // Where two infix operators have the same precedence, start at the same index in the equation, and end at
            // the same index in the equation, which one is chosen is undefined.

            // Where two infix operators are both present and have overlapping operator symbols, the one with the lower
            // precedence will be parsed. Where two infix operators with the same precedence are both present and have
            // overlapping operator symbols, the one that leaves the most text for the operand on the side of
            // associativity will be parsed. e.g. for groups of left-associative operators, the operator with the
            // longest left-most operand will be parsed.
            //
            // ...
            //
            // ... Please don't make operators with symbols that will make equations easily ambiguous? Please?f
            
            // var operatorsWithOperandStrings
            //     = associativeOperatorGroup
            //      .InfixOperators?
            //      .SelectMany(op => s.FindPossibleSymbolIndices(op.Symbols)
            //                         .Select(indices
            //                                     => (Op: op,
            //                                         SymbolIndices: indices,
            //                                         OperandStrings: s.SplitBySeparatorsAtIndices(op.Symbols, indices))))
            //      .Where(x => x.OperandStrings.All(operand => operand.Trim().Length > 0));

            var operatorsWithOperandStrings
                = associativeOperatorGroup
                 .InfixOperators?
                 .SelectMany(op => stores.StringUtils.FindPossibleOperatorSymbolIndices(s, op)
                                         .Select(indices
                                                     => (Op: op,
                                                         SymbolIndices: indices,
                                                         OperandStrings: s.SplitBySeparatorsAtIndices(
                                                             op.Symbols, indices))))
                 .Where(x => x.OperandStrings.All(operand => operand.Trim().Length > 0));
            
            if(operatorsWithOperandStrings is null)
                return null;

            if(associativeOperatorGroup.IsLeftAssociative)
            {
                // Order by:
                // - Furthest right start of the first symbol
                // - Furthest right start of the last  symbol
                // - Furthest right end   of the first symbol
                // - Furthest right end   of the last  symbol

                operatorsWithOperandStrings
                    = operatorsWithOperandStrings
                     .OrderByDescending(x => x.SymbolIndices[0])
                     .ThenByDescending(x => x.SymbolIndices[^1])
                     .ThenByDescending(x => x.SymbolIndices[0]  + x.Op.Symbols[0].Length)
                     .ThenByDescending(x => x.SymbolIndices[^1] + x.Op.Symbols[^1].Length)
                     .ToList();
            }
            else
            {
                // Order by:
                // - Furthest left end   of the last  symbol
                // - Furthest left end   of the first symbol
                // - Furthest left start of the last  symbol
                // - Furthest left start of the first symbol

                operatorsWithOperandStrings
                    = operatorsWithOperandStrings
                     .OrderBy(x => x.SymbolIndices[^1] + x.Op.Symbols[^1].Length)
                     .ThenBy(x => x.SymbolIndices[0]   + x.Op.Symbols[0].Length)
                     .ThenBy(x => x.SymbolIndices[^1])
                     .ThenBy(x => x.SymbolIndices[0])
                     .ToList();
            }

            foreach(var (infixOperator, _, operandStrings) in operatorsWithOperandStrings)
            {
                var operands = operandStrings.Select(x => EquationParser.Parse(x, stores, depthRemaining))
                                             .ToList();

                if(operands.Contains(null))
                    continue;

                return new Operation(infixOperator, (IList<IEquation>)operands);
            }

            return null;
        }

        private static IEquation? ParseLeftAssociativeAffixOperation(string                    s,
                                                                     IEquationStores           stores,
                                                                     IAssociativeOperatorGroup associativeOperatorGroup,
                                                                     int                       depthRemaining)
        {
            return ParsePostfixOperation(s, stores, associativeOperatorGroup, depthRemaining)
                ?? ParsePrefixOperation(s, stores, associativeOperatorGroup, depthRemaining);
        }
        
        private static IEquation? ParseRightAssociativeAffixOperation(string                    s,
                                                                     IEquationStores           stores,
                                                                     IAssociativeOperatorGroup associativeOperatorGroup,
                                                                     int                       depthRemaining)
        {
            return ParsePrefixOperation(s, stores, associativeOperatorGroup, depthRemaining)
                ?? ParsePostfixOperation(s, stores, associativeOperatorGroup, depthRemaining);
        }

        private static IEquation? ParsePrefixOperation(string          eqString,
                                                       IEquationStores stores,
                                                       IAssociativeOperatorGroup  associativeOperatorGroup,
                                                       int             depthRemaining)
        {
            var prefixOperator = associativeOperatorGroup
                                .PrefixOperators?
                                .Where(op => eqString.StartsWith(op.Symbol, StringComparison.Ordinal))
                                .Where(op => eqString[(op.Symbol.Length)..].TrimStart().Length > 0)
                                .OrderByDescending(op => op.Symbol.Length)
                                .Cast<IPrefixOperator?>()
                                .FirstOrDefault();

            if(prefixOperator is null)
                return null;

            var operandString = eqString[(prefixOperator.Symbol.Length)..].TrimStart();
            var operand       = EquationParser.Parse(operandString, stores, depthRemaining);

            if(operand is null)
                return null;

            return new Operation(prefixOperator, new List<IEquation> { operand });
        }

        private static IEquation? ParsePostfixOperation(string          eqString,
                                                        IEquationStores stores,
                                                        IAssociativeOperatorGroup  associativeOperatorGroup,
                                                        int             depthRemaining)
        {
            var postfixOperator = associativeOperatorGroup.PostfixOperators?
                                               .Where(op => eqString.EndsWith(op.Symbol, StringComparison.Ordinal))
                                               .Where(op => eqString[..^(op.Symbol.Length)].TrimEnd().Length > 0)
                                               .OrderByDescending(op => op.Symbol.Length)
                                               .Cast<IPostfixOperator?>()
                                               .FirstOrDefault();

            if(postfixOperator is null)
                return null;
            
            var operandString = eqString[..^(postfixOperator.Symbol.Length)].TrimEnd();
            var operand       = EquationParser.Parse(operandString, stores, depthRemaining);

            if(operand is null)
                return null;
            
            return new Operation(postfixOperator, new List<IEquation> { operand });
        }

        public IEnumerable<(IEquation equation, string equationSource)> ReadFromEnd(
            string          equationString,
            IEquationStores stores,
            int             depthRemaining,
            DepthAdjuster   depthAdjuster)
        {
            throw new NotSupportedException("From an operation from the end isn't supported.");
        }
    }
}
