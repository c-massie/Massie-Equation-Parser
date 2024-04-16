using System;

namespace Scot.Massie.EquationParser.Operators
{
    internal interface IPrefixOperator : IAffixOperator
    { }
    
    internal class PrefixOperator : AffixOperator, IPrefixOperator
    {
        public PrefixOperator(string               symbol,
                              decimal              precedence,
                              bool                 isLeftAssociative,
                              Func<double, double> implementation)
            : base(symbol, precedence, isLeftAssociative, implementation)
        { }

        public PrefixOperator(string symbol, Func<double, double> implementation)
            : base(symbol, 100, true, implementation)
        { }
    }
}
