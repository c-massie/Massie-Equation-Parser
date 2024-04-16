using System.Collections.Generic;

namespace Scot.Massie.EquationParser.Equations
{
    internal interface IVariableAccess : IEquation
    {
        string VariableName { get; }

        IDictionary<string, double> SharedVariableStore { get; }
    }
    
    internal class VariableAccess : IVariableAccess
    {
        public string                      VariableName        { get; }
        
        public IDictionary<string, double> SharedVariableStore { get; }

        public VariableAccess(string variableName, IDictionary<string, double> sharedVariableStore)
        {
            VariableName        = variableName;
            SharedVariableStore = sharedVariableStore;
        }

        public double Evaluate()
        {
            return SharedVariableStore[VariableName];
        }
    }
}
