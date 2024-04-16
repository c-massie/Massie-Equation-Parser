namespace Scot.Massie.EquationParser.Equations
{
    /// <summary>
    /// A compiled equation.
    /// </summary>
    public interface IEquation
    {
        /// <summary>
        /// Evaluates the equation; produces the numeric result of the equation.
        /// </summary>
        /// <remarks>
        /// The result of this may be different from call-to-call if it refers to variables that have been re-assigned
        /// or calls functions that have been re-implemented or where the result has changed.
        /// </remarks>
        /// <returns>The numeric result of the equation.</returns>
        double Evaluate();
    }
}
