using System.Collections.Generic;
using Scot.Massie.EquationParser.Equations;

namespace Scot.Massie.EquationParser.Operators
{
    internal interface IOperator
    {
        decimal Precedence { get; set; }

        bool IsLeftAssociative { get; set; }

        double Evaluate(IList<IEquation> operands);
    }
}
