using System.Collections.Generic;
using Scot.Massie.EquationParser.Operators;

namespace Scot.Massie.EquationParser.Equations
{
    internal interface IBracketedOperation : IEquation
    {
        IBracketedOperator BracketedOperator { get; }

        IList<IEquation> Operands { get; }
    }
    
    internal class BracketedOperation : IBracketedOperation
    {
        public IBracketedOperator BracketedOperator { get; }
        public IList<IEquation>   Operands          { get; }

        public BracketedOperation(IBracketedOperator bracketedOperator, IList<IEquation> operands)
        {
            BracketedOperator = bracketedOperator;
            Operands          = operands;
        }

        public double Evaluate()
        {
            return BracketedOperator.Evaluate(Operands);
        }
    }
}
