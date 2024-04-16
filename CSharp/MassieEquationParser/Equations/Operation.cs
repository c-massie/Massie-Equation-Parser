using System.Collections.Generic;
using Scot.Massie.EquationParser.Operators;

namespace Scot.Massie.EquationParser.Equations
{
    internal interface IOperation : IEquation
    {
        IOperator Operator { get; }

        IList<IEquation> Operands { get; }
    }
    
    internal class Operation : IOperation
    {
        public IOperator Operator { get; }
        
        public IList<IEquation> Operands { get; }

        public Operation(IOperator @operator, IList<IEquation> operands)
        {
            Operator = @operator;
            Operands = new List<IEquation>(operands).AsReadOnly();
        }

        public double Evaluate()
        {
            return Operator.Evaluate(Operands);
        }
    }
}
