using System.Collections.Generic;
using Scot.Massie.EquationParser.Functions;

namespace Scot.Massie.EquationParser.Equations
{
    internal interface IFunctionCall : IEquation
    {
        string FunctionName { get; }

        IList<IEquation> Arguments { get; }

        IDictionary<string, IFunction> FunctionStore { get; }
    }
    
    internal class FunctionCall : IFunctionCall
    {
        public string FunctionName { get; }

        public IList<IEquation> Arguments { get; }

        public IDictionary<string, IFunction> FunctionStore { get; }

        public FunctionCall(string                         functionName,
                            IList<IEquation>               arguments,
                            IDictionary<string, IFunction> functionStore)
        {
            FunctionName  = functionName;
            Arguments     = arguments;
            FunctionStore = functionStore;
        }

        public double Evaluate()
        {
            return FunctionStore[FunctionName].Evaluate(Arguments);
        }
    }
}
