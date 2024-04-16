using Scot.Massie.EquationParser.Operators;

namespace Scot.Massie.EquationParser
{
    /// <summary>
    /// A view of <see cref="IEquationParser"/> that exposes methods for configuring the last bracketed operator you
    /// added.
    /// </summary>
    public interface IEquationBracketedOperatorParser : IEquationParser
    {
        /// <summary>
        /// Specifies that you should be able to pass no arguments to the bracketed operator. That is, the opening
        /// bracket for this operator followed by the closing bracket with nothing between will be a valid invocation.
        /// </summary>
        /// <remarks>
        /// With bracketed operator where the opening and closing brackets are the same, (e.g. in the "absolute value"
        /// operator, `|x|`) this may interfere with the ability to nest calls to it. e.g. `||x||` could be validly read
        /// as `|(|x|)|` or `((||)x)(||)`.
        /// </remarks>
        /// <returns>This.</returns>
        IEquationBracketedOperatorParser AllowingNoOperands();
    }
    
    /// <inheritdoc cref="IEquationBracketedOperatorParser"/>
    public class EquationBracketedOperatorParser : EquationSpecifiedParserBase, IEquationBracketedOperatorParser
    {
        private readonly IBracketedOperator _operator;

        internal EquationBracketedOperatorParser(IEquationParser parent, IBracketedOperator @operator)
            : base(parent)
        {
            _operator = @operator;
        }

        public IEquationBracketedOperatorParser AllowingNoOperands()
        {
            _operator.AllowsNoOperands = true;
            return this;
        }
    }
}
