using System;
using System.Collections.Generic;
using Scot.Massie.EquationParser.Equations;

namespace Scot.Massie.EquationParser.Operators
{
    internal interface IAffixOperator : IOperator
    {
        string Symbol { get; }
    }
    
    internal class AffixOperator : IAffixOperator
    {
        public string Symbol { get; }

        public decimal Precedence { get; set; }

        public bool IsLeftAssociative { get; set; }

        private readonly Func<double, double> _implementation;

        protected AffixOperator(string               symbol,
                                decimal              precedence,
                                bool                 isLeftAssociative,
                                Func<double, double> implementation)
        {
            Symbol            = symbol;
            Precedence        = precedence;
            IsLeftAssociative = isLeftAssociative;
            _implementation   = implementation;
        }

        public double Evaluate(IList<IEquation> operands)
        {
            return _implementation(operands[0].Evaluate());
        }
    }
}
