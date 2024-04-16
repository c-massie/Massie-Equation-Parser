using System;

namespace Scot.Massie.EquationParser.Operators
{
    internal interface IPostfixOperator : IAffixOperator
    { }
    
    internal class PostfixOperator : AffixOperator, IPostfixOperator
    {
        public PostfixOperator(string               symbol,
                               decimal              precedence,
                               bool                 isLeftAssociative,
                               Func<double, double> implementation)
            : base(symbol, precedence, isLeftAssociative, implementation)
        { }

        public PostfixOperator(string symbol, Func<double, double> implementation)
            : base(symbol, 100, true, implementation)
        { }
    }
}
