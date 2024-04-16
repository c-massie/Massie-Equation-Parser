using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Scot.Massie.EquationParser.Operators;

namespace Scot.Massie.EquationParser.Utils
{
    /// <summary>
    /// A set of string utility functions that need to take into account the configuration of an equation parser.
    /// </summary>
    internal interface IEquationStringUtils
    {
        /// <summary>
        /// Splits a string into a set of substrings, separated by the argument separator except where the argument
        /// separator is inside brackets of any kind.
        /// </summary>
        /// <param name="s">The string to split.</param>
        /// <returns>
        /// A list of strings, which are the substrings of the source strings in order, separated by the argument
        /// separator when not in brackets.
        /// </returns>
        IList<string> SplitStringBySeparatorsNotInBrackets(string s);
        
        /// <summary>
        /// Finds an opening bracket's matching closing bracket.
        /// </summary>
        /// <remarks>
        /// This overload is specifically for regular brackets, not for bracketed operators. (Although the brackets from
        /// bracketed operators are still considered when trying to find the matching closing bracket.)
        /// </remarks>
        /// <param name="s">The source string.</param>
        /// <param name="openingBracketIndex">The index in the source string of the start of the opening bracket.</param>
        /// <returns>
        /// The index in the source string of the start of the matching closing bracket, or -1 if no matching closing
        /// bracket was found.
        /// </returns>
        int GetClosingBracketIndex(string s, int openingBracketIndex);

        /// <summary>
        /// Finds a closing bracket's matching opening bracket.
        /// </summary>
        /// <remarks>
        /// This overload is specifically for regular brackets, not for bracketed operators. (Although the brackets from
        /// bracketed operators are still considered when trying to find the matching opening bracket.)
        /// </remarks>
        /// <param name="s">The source string.</param>
        /// <param name="closingBracketIndex">The index in the source string of the start of the closing bracket.</param>
        /// <returns>
        /// The index in the source string of the start of the matching opening bracket, or -1 if no matching opening
        /// bracket was found.
        /// </returns>
        int GetOpeningBracketIndex(string s, int closingBracketIndex);
        
        /// <summary>
        /// Finds an opening bracket's matching closing bracket.
        /// </summary>
        /// <param name="s">The source string.</param>
        /// <param name="openingBracketIndex">
        /// The index in the source string of the start of the opening bracket.
        /// </param>
        /// <param name="openingBracket">The opening bracket symbol.</param>
        /// <param name="eligibleClosingBrackets">
        /// A collection of the possible symbols that could form a pair of brackets with the given opening bracket
        /// symbol.
        /// </param>
        /// <returns>
        /// The closing bracket symbol found and the index the start of it was found at in the source string. If no
        /// closing bracket symbol was found, the index will be -1 and the closing bracket symbol will be null.
        /// </returns>
        (int ClosingBracketIndex, string? ClosingBracket) GetClosingBracketIndex
        (
            string        s,
            int           openingBracketIndex,
            string        openingBracket,
            IList<string> eligibleClosingBrackets
        );
        
        /// <summary>
        /// Finds a closing bracket's matching opening bracket.
        /// </summary>
        /// <param name="s">The source string.</param>
        /// <param name="closingBracketIndex">
        /// The index in the source string of the start of the closing bracket.
        /// </param>
        /// <param name="closingBracket">The closing bracket symbol.</param>
        /// <param name="eligibleOpeningBrackets">
        /// A collection of the possible symbols that could form a pair of brackets with the given closing bracket
        /// symbol.
        /// </param>
        /// <returns>
        /// The opening bracket symbol found and the index the start of it was found at in the source string. If no
        /// opening bracket symbol was found, the index will be -1 and the opening bracket symbol will be null.
        /// </returns>
        (int OpeningBracketIndex, string? OpeningBracket) GetOpeningBracketIndex
        (
            string        s,
            int           closingBracketIndex,
            string        closingBracket,
            IList<string> eligibleOpeningBrackets
        );

        /// <summary>
        /// Finds all possible combinations of indices that a given infix operator's operands could be at in an
        /// equation.
        /// </summary>
        /// <param name="equation">The equation that may contain symbols of the passed operator.</param>
        /// <param name="op">The operator whose symbols will be sought in the equation string.</param>
        /// <returns>
        /// A collection of lists of ints; each list is a possible list of the indices of each of the operator's
        /// corresponding symbols in the equation string.
        /// </returns>
        ICollection<IList<int>> FindPossibleOperatorSymbolIndices(string equation, IInfixOperator op);
    }
    
    internal class EquationStringUtils : IEquationStringUtils
    {
        private readonly IEquationStores _equationStores;

        private string ArgSeparator => _equationStores.ArgumentSeparatorSymbol;
        private string Opener       => _equationStores.OpeningBracketSymbol;
        private string Closer       => _equationStores.ClosingBracketSymbol;

        private IDictionary<string, Dictionary<string, IBracketedOperator>> BracketedOperatorDict
            => _equationStores.BracketedOperatorDict;
        
        private IDictionary<string, Dictionary<string, IBracketedOperator>> BracketedOperatorReverseDict
            => _equationStores.BracketedOperatorReverseDict;

        public EquationStringUtils(IEquationStores equationStores)
        {
            _equationStores = equationStores;
        }

        public IList<string> SplitStringBySeparatorsNotInBrackets(string s)
        {
            var segments                 = new List<string>();
            int currentSegmentStartIndex = 0;

            for(int i = 0; i < s.Length; i++)
            {
                if(s.ContainsAt(ArgSeparator, i))
                {
                    segments.Add(s[currentSegmentStartIndex..i]);
                    currentSegmentStartIndex = i + ArgSeparator.Length;
                    continue;
                }

                if(StringContainsOpeningBracketAt(s, i, out var openingBracket, out var matchingClosingBrackets))
                {
                    var (closingBracketIndex, closingBracket)
                        = GetClosingBracketIndex(s, i, openingBracket, matchingClosingBrackets);
                    
                    if(closingBracket is null)
                        break;
                    
                    i = closingBracketIndex + closingBracket.Length - 1;
                }
            }
            
            segments.Add(s[currentSegmentStartIndex..]);
            
            return segments;
        }
        
        public int GetClosingBracketIndex(string s, int openingBracketIndex)
        {
            return GetClosingBracketIndex(s, openingBracketIndex, Opener, new[] { Closer })
               .ClosingBracketIndex;
        }

        public int GetOpeningBracketIndex(string s, int closingBracketIndex)
        {
            return GetOpeningBracketIndex(s, closingBracketIndex, Closer, new[] { Opener })
               .OpeningBracketIndex;
        }
        
        public (int ClosingBracketIndex, string? ClosingBracket) GetClosingBracketIndex
        (
            string        s,
            int           openingBracketIndex,
            string        openingBracket,
            IList<string> eligibleClosingBrackets
        )
        {
            int previousSeparatorPosition = -1;
            
            for(int i = openingBracketIndex + openingBracket.Length; i < s.Length; i++)
            {
                foreach(var closingBracket in eligibleClosingBrackets)
                {
                    if(s.ContainsAt(closingBracket, i))
                    {
                        bool hasRequiredNumberOfArguments
                            = BracketedOperatorAcceptsEmptyArgumentList(openingBracket, closingBracket)
                           || s[(openingBracketIndex + 1)..(i)].Trim().Length != 0;

                        bool immediatelyFollowsSeparator
                            = previousSeparatorPosition != -1
                           && s[(previousSeparatorPosition + ArgSeparator.Length)..(i)].Trim().Length == 0;
                        
                        if(hasRequiredNumberOfArguments && !immediatelyFollowsSeparator)
                            return (i, closingBracket);
                    }
                }

                if(StringContainsOpeningBracketAt(s, i, out var openerAtI, out var matchingClosingBrackets))
                {
                    var (closingBracketIndex, closingBracket)
                        = GetClosingBracketIndex(s, i, openerAtI, matchingClosingBrackets);
                    
                    if(closingBracket is null)
                        continue;
                    
                    i = closingBracketIndex + closingBracket.Length - 1;
                    continue;
                }

                if(s.ContainsAt(ArgSeparator, i))
                {
                    previousSeparatorPosition =  i;
                    
                    i += ArgSeparator.Length - 1;
                }
            }

            return (-1, default);
        }

        public (int OpeningBracketIndex, string? OpeningBracket) GetOpeningBracketIndex
        (
            string s,
            int closingBracketIndex,
            string closingBracket,
            IList<string> eligibleOpeningBrackets
        )
        {
            int previousSeparatorPosition = -1;
            
            for(int i = closingBracketIndex - 1; i >= 0; i--)
            {
                foreach(var openingBracket in eligibleOpeningBrackets)
                {
                    if(s.ContainsEndingAt(openingBracket, i))
                    {
                        bool hasRequiredNumberOfArguments
                            = BracketedOperatorAcceptsEmptyArgumentList(openingBracket, closingBracket)
                           || s[(i + 1)..(closingBracketIndex)].Trim().Length != 0;
                    
                        bool immediatelyPrecedesSeparator
                            = previousSeparatorPosition != -1
                           && s[(i + 1)..(previousSeparatorPosition)].Trim().Length == 0;
                        
                        if(hasRequiredNumberOfArguments && !immediatelyPrecedesSeparator)
                            return (i - openingBracket.Length + 1, openingBracket);
                    }
                }

                if(StringContainsClosingBracketEndingAt(s, i, out var closerEndingAtI, out var matchingOpeningBrackets))
                {
                    var (openingBracketIndex, _)
                        = GetOpeningBracketIndex(s, 
                                                 i - closingBracket.Length + 1,
                                                 closerEndingAtI,
                                                 matchingOpeningBrackets);

                    if(openingBracketIndex < 0)
                        continue;

                    i = openingBracketIndex;
                    continue;
                }

                if(s.ContainsEndingAt(ArgSeparator, i))
                {
                    i = i - ArgSeparator.Length + 1;
                    
                    previousSeparatorPosition = i;
                }
            }

            return (-1, default);
        }
        
        private bool StringContainsOpeningBracketAt(string s, 
                                                    int index,
                                                    [MaybeNullWhen(false)] out string openingBracket,
                                                    [MaybeNullWhen(false)] out IList<string> matchingClosingBrackets)
        {
            openingBracket = BracketedOperatorDict.Keys
                                                  .Append(Opener)
                                                  .OrderByDescending(x => x.Length)
                                                  .FirstOrDefault(symbol => s.ContainsAt(symbol, index));

            if(openingBracket is null)
            {
                matchingClosingBrackets = default;
                return false;
            }

            if((openingBracket.Equals(Opener, StringComparison.Ordinal)) 
            && (!BracketedOperatorDict.ContainsKey(Opener)))
            {
                matchingClosingBrackets = new[] { Closer };
                return true;
            }

            IEnumerable<string> matchingClosingBracketsUnordered = BracketedOperatorDict[openingBracket].Keys;

            if(openingBracket.Equals(Opener, StringComparison.Ordinal))
                matchingClosingBracketsUnordered = matchingClosingBracketsUnordered.Append(Closer);
                
            
            matchingClosingBrackets = matchingClosingBracketsUnordered
                                     .OrderByDescending(x => x.Length)
                                     .ToList();
            
            if(openingBracket.Equals(Opener, StringComparison.Ordinal))
                matchingClosingBrackets.Add(Closer);

            return true;
        }

        private bool StringContainsClosingBracketEndingAt
        (
            string                                   s,
            int                                      index,
            [MaybeNullWhen(false)] out string        closingBracket,
            [MaybeNullWhen(false)] out IList<string> matchingOpeningBrackets
        )
        {
            closingBracket = BracketedOperatorReverseDict.Keys
                                                         .Append(Closer)
                                                         .OrderByDescending(x => x.Length)
                                                         .FirstOrDefault(symbol => s.ContainsEndingAt(symbol, index));

            if(closingBracket is null)
            {
                matchingOpeningBrackets = default;
                return false;
            }

            if((closingBracket.Equals(Closer, StringComparison.Ordinal))
            && (!BracketedOperatorReverseDict.ContainsKey(Closer)))
            {
                matchingOpeningBrackets = new[] { Opener };
                return true;
            }

            IEnumerable<string> matchingOpeningBracketsUnordered = BracketedOperatorReverseDict[closingBracket].Keys;

            if(closingBracket.Equals(Closer, StringComparison.Ordinal))
                matchingOpeningBracketsUnordered = matchingOpeningBracketsUnordered.Append(Closer);

            matchingOpeningBrackets = matchingOpeningBracketsUnordered
                                     .OrderByDescending(x => x.Length)
                                     .ToList();

            if(closingBracket.Equals(Closer, StringComparison.Ordinal))
                matchingOpeningBrackets.Add(Opener);

            return true;
        }

        private bool BracketedOperatorAcceptsEmptyArgumentList(string opener, string closer)
        {
            if(opener.Equals(Opener, StringComparison.Ordinal)
            && closer.Equals(Closer, StringComparison.Ordinal))
                return true;
            
            return BracketedOperatorDict[opener][closer].AllowsNoOperands;
        }

        public ICollection<IList<int>> FindPossibleOperatorSymbolIndices(string equation, IInfixOperator op)
        {
            return FindPossibleOperatorSymbolIndices(equation, op.Symbols, 0, 0)
                  .Select(x => (IList<int>)x.ToList())
                  .ToList();
        }

        private IEnumerable<IEnumerable<int>> FindPossibleOperatorSymbolIndices(string        equation,
                                                                                IList<string> operatorSymbols,
                                                                                int           startingAt,
                                                                                int           startWithSymbolNumber)
        {
            var currentSymbol        = operatorSymbols[startWithSymbolNumber];
            var currentSymbolIndices = IndicesOfNotInBrackets(equation, currentSymbol, startingAt);

            if(startWithSymbolNumber == operatorSymbols.Count - 1)
            {
                foreach(var index in currentSymbolIndices)
                    yield return new[] { index };
                
                yield break;
            }

            foreach(var index in currentSymbolIndices)
            {
                var indexAfterSymbol = index + operatorSymbols[startWithSymbolNumber].Length;

                foreach(var followingIndices in FindPossibleOperatorSymbolIndices(equation,
                                                                                  operatorSymbols,
                                                                                  indexAfterSymbol,
                                                                                  startWithSymbolNumber + 1))
                {
                    yield return followingIndices.Prepend(index);
                }
            }
        }

        private IEnumerable<int> IndicesOfNotInBrackets(string equation, string lookingFor, int startingAt)
        {
            foreach(var (segment, segmentIndexInEquation) in GetSegmentsOfEquationNotInBrackets(equation))
            {
                if((segmentIndexInEquation + segment.Length - 1) < startingAt)
                    continue;

                foreach(var index in segment.AllIndicesOf(lookingFor,
                                                          startingAt - segmentIndexInEquation,
                                                          StringComparison.Ordinal))
                {
                    yield return index + segmentIndexInEquation;
                }
            }
        }

        private IEnumerable<(string Segment, int IndexInSource)> GetSegmentsOfEquationNotInBrackets(string equation)
        {
            int currentSegmentStart = 0;

            for(int i = 0; i < equation.Length; i++)
            {
                if(StringContainsOpeningBracketAt(equation, i, 
                                                  out var openingBracketSymbol, 
                                                  out var matchingClosingBrackets))
                {
                    var (closingBracketIndex, closingBracketSymbol) 
                        = GetClosingBracketIndex(equation, i, openingBracketSymbol, matchingClosingBrackets);

                    if(closingBracketSymbol != null)
                    {
                        yield return (equation[currentSegmentStart..i], currentSegmentStart);
                        i = closingBracketIndex + closingBracketSymbol.Length - 1;
                        currentSegmentStart = i + 1;
                        continue;
                    }
                }
            }
            
            yield return (equation[currentSegmentStart..], currentSegmentStart);
        }
    }
}
