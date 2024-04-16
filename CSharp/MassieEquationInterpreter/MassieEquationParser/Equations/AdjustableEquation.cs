using System;
using System.Collections.Generic;
using Scot.Massie.EquationParser.Functions;

namespace Scot.Massie.EquationParser.Equations
{
    /// <summary>
    /// A compiled equation with the ability to re-assign variables and re-implement functions.
    /// </summary>
    public interface IAdjustableEquation : IEquation
    {
        /// <summary>
        /// Sets the value of a variable. If the variable is referenced in the equation, the value assigned by calling
        /// this will be returned by that variable thenceforth.
        /// </summary>
        /// <remarks>
        /// Trying to set a variable not present when the equation was built will have no effect on the equation.
        /// </remarks>
        /// <param name="name">The name of the variable, as it can be referred to as in equations.</param>
        /// <param name="newValue">The value that the variable should represent.</param>
        void SetVariable(string name, double newValue);

        /// <summary>
        /// Sets the implementation of a function. If the function is called in the equation, the implementation
        /// assigned by calling this will be invoked in that call thenceforth.
        /// </summary>
        /// <remarks>
        /// Trying to add an implementation for a function that was not present when the equation was built will have no
        /// effect on the equation.
        /// </remarks>
        /// <param name="name">The name of the function, as it can be called with in equations.</param>
        /// <param name="implementation">The new implementation of the function.</param>
        void ReimplementFunction(string name, Func<IList<double>, double> implementation);
    }
    
    /// <inheritdoc cref="IAdjustableEquation"/>
    public class AdjustableEquation : IAdjustableEquation
    {
        private readonly IEquation       _inner;
        private readonly IEquationStores _equationStores;

        internal AdjustableEquation(IEquation inner, IEquationStores equationStores)
        {
            _inner          = inner;
            _equationStores = equationStores;
        }

        public void SetVariable(string name, double newValue)
        {
            _equationStores.Variables[name] = newValue;
        }

        public void ReimplementFunction(string name, Func<IList<double>, double> implementation)
        {
            _equationStores.Functions[name] = new Function(implementation);
        }

        public double Evaluate()
        {
            return _inner.Evaluate();
        }
    }
}
