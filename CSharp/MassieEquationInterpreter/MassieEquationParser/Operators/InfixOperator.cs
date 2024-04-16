using System;
using System.Collections.Generic;
using System.Linq;
using Scot.Massie.EquationParser.Equations;

namespace Scot.Massie.EquationParser.Operators
{
    internal interface IInfixOperator : IOperator
    {
        IList<string> Symbols { get; }
    }
    
    internal class InfixOperator : IInfixOperator
    {
        public IList<string> Symbols { get; }
        
        public decimal Precedence { get; set; }

        public bool IsLeftAssociative { get; set; }

        private readonly Func<IList<double>, double> _implementation;

        public InfixOperator(IList<string>               symbols,
                             decimal                     precedence,
                             bool                        isLeftAssociative,
                             Func<IList<double>, double> implementation)
        {
            Symbols           = new List<string>(symbols).AsReadOnly();
            Precedence        = precedence;
            IsLeftAssociative = isLeftAssociative;
            _implementation   = implementation;
        }

        public InfixOperator(IList<string>               symbols,
                             Func<IList<double>, double> implementation)
            : this(symbols, 100, true, implementation)
        { }
        
        public InfixOperator(string                       symbol,
                             decimal                      precedence,
                             bool                         isLeftAssociative,
                             Func<double, double, double> implementation)
        {
            Symbols           = new List<string> { symbol }.AsReadOnly();
            Precedence        = precedence;
            IsLeftAssociative = isLeftAssociative;
            _implementation   = operands => implementation(operands[0], operands[1]);
        }
        
        public InfixOperator(string                       symbol,
                             Func<double, double, double> implementation)
            : this(symbol, 100, true, implementation)
        { }

        public double Evaluate(IList<IEquation> operands)
        {
            return _implementation(operands.Select(x => x.Evaluate()).ToList());
        }
    }
}
