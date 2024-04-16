using System;
using System.Collections.Generic;
using Scot.Massie.EquationParser.Equations;

namespace Scot.Massie.EquationParser
{
    /// <summary>
    /// A base class for equation parsers that add context-specific methods, so they don't have to wrap the original
    /// equation parser and provide implementations for all methods themselves.
    /// </summary>
    public class EquationSpecifiedParserBase : IEquationParser
    {
        private IEquationParser _parent;

        public EquationSpecifiedParserBase(IEquationParser parent)
        {
            _parent = parent;
        }


        #region Delegated implementation of IEquationParser to _parent

        public IEquationParser WithBracketSymbols(string openingBracketSymbol, string closingBracketSymbol)
        {
            return _parent.WithBracketSymbols(openingBracketSymbol, closingBracketSymbol);
        }

        public IEquationParser WithArgumentSeparatorSymbol(string argumentSeparatorSymbol)
        {
            return _parent.WithArgumentSeparatorSymbol(argumentSeparatorSymbol);
        }

        public IEquationParser WithVariable(string name, double value)
        {
            return _parent.WithVariable(name, value);
        }

        public IEquationParser WithFunction(string name, Func<IList<double>, double> implementation)
        {
            return _parent.WithFunction(name, implementation);
        }

        public IEquationOperatorParser WithPrefixOperator(string symbol, Func<double, double> implementation)
        {
            return _parent.WithPrefixOperator(symbol, implementation);
        }

        public IEquationOperatorParser WithPostfixOperator(string symbol, Func<double, double> implementation)
        {
            return _parent.WithPostfixOperator(symbol, implementation);
        }

        public IEquationOperatorParser WithBinaryOperator(string symbol, Func<double, double, double> implementation)
        {
            return _parent.WithBinaryOperator(symbol, implementation);
        }

        public IEquationOperatorParser WithInfixOperator(IList<string>               symbols,
                                                           Func<IList<double>, double> implementation)
        {
            return _parent.WithInfixOperator(symbols, implementation);
        }

        public IEquationBracketedOperatorParser
            WithBracketedMultiOperator(string                      openingSymbol,
                                       string                      closingSymbol,
                                       Func<IList<double>, double> implementation)
        {
            return _parent.WithBracketedMultiOperator(openingSymbol, closingSymbol, implementation);
        }

        public IEquationParser
            WithBracketedOperator(string openingSymbol, string closingSymbol, Func<double, double> implementation)
        {
            return _parent.WithBracketedOperator(openingSymbol, closingSymbol, implementation);
        }

        public IEquationParser WithJuxtaposition()
        {
            return _parent.WithJuxtaposition();
        }

        public IEquationParser WithJuxtaposition(Func<double, double, double> juxtapositionFunc)
        {
            return _parent.WithJuxtaposition(juxtapositionFunc);
        }

        public IEquationParser WithMaximumDepth(int depth)
        {
            return _parent.WithMaximumDepth(depth);
        }

        public IEquationParser WithBasicMaths()
        {
            return _parent.WithBasicMaths();
        }

        public IEquationParser WithStandardMaths()
        {
            return _parent.WithStandardMaths();
        }

        public IEquationParser WithLogic()
        {
            return _parent.WithLogic();
        }

        public IAdjustableEquation Parse(string equation)
        {
            return _parent.Parse(equation);
        }

        public double Evaluate(string equation)
        {
            return _parent.Evaluate(equation);
        }

        #endregion
    }
}
