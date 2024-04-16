using System;
using System.Collections.Generic;
using System.Linq;
using Scot.Massie.EquationParser.Equations;

namespace Scot.Massie.EquationParser.Functions
{
    internal interface IFunction
    {
        double Evaluate(IList<IEquation> arguments);
    }

    internal class Function : IFunction
    {
        private readonly Func<IList<double>, double> _implementation;

        public Function(Func<IList<double>, double> implementation)
        {
            _implementation = implementation;
        }

        public double Evaluate(IList<IEquation> arguments)
        {
            return _implementation(arguments.Select(x => x.Evaluate()).ToList());
        }
    }
}
