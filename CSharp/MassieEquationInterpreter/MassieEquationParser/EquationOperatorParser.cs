using Scot.Massie.EquationParser.Operators;

namespace Scot.Massie.EquationParser
{
    /// <summary>
    /// A view of <see cref="IEquationParser"/> that exposes methods for configuring the last operator you added.
    /// </summary>
    public interface IEquationOperatorParser : IEquationParser
    {
        /// <summary>
        /// Specifies the precedence level of the last operator added. Precedence is how "sticky" and operator is.
        /// </summary>
        /// <remarks>
        /// As this is a top-down equation parser, operators with a lower level of precedence will be parsed
        /// first/preferably.
        /// </remarks>
        /// <param name="precedence">The precedence the last operator added should have.</param>
        /// <returns>This.</returns>
        IEquationOperatorParser WithPrecedence(decimal precedence);

        /// <summary>
        /// Specifies that the last operator added should be left-associative.
        /// </summary>
        /// <remarks>Operators are left-associative by default.</remarks>
        /// <remarks>
        /// Left or right associativity is whether, given a chain of operator invocations of the same level of
        /// precedence, the left-most or right-most operand should be "stickiest".
        /// </remarks>
        /// <returns>This.</returns>
        IEquationOperatorParser LeftAssociative();

        /// <summary>
        /// Specifies whether the last operator added should be left associative. If it shouldn't be, it will be
        /// right-associative.
        /// </summary>
        /// <remarks>
        /// Left or right associativity is whether, given a chain of operator invocations of the same level of
        /// precedence, the left-most or right-most operand should be "stickiest".
        /// </remarks>
        /// <param name="operatorIsLeftAssociative">
        /// True if the last operator added should be left associative, false if it should be right-associative.
        /// </param>
        /// <returns>This.</returns>
        IEquationOperatorParser LeftAssociative(bool operatorIsLeftAssociative);

        /// <summary>
        /// Specifies that the last operator added should be right-associative.
        /// </summary>
        /// <remarks>Operators are left-associative by default.</remarks>
        /// <remarks>
        /// Left or right associativity is whether, given a chain of operator invocations of the same level of
        /// precedence, the left-most or right-most operand should be "stickiest".
        /// </remarks>
        /// <returns>This.</returns>
        IEquationOperatorParser RightAssociative();

        /// <summary>
        /// Specifies whether the last operator added should be right associative. If it shouldn't be, it will be
        /// left-associative.
        /// </summary>
        /// <remarks>
        /// Left or right associativity is whether, given a chain of operator invocations of the same level of
        /// precedence, the left-most or right-most operand should be "stickiest".
        /// </remarks>
        /// <param name="operatorIsRightAssociative">
        /// True if the last operator added should be right associative, false if it should be left-associative.
        /// </param>
        /// <returns>This.</returns>
        IEquationOperatorParser RightAssociative(bool operatorIsRightAssociative);
    }
    
    /// <inheritdoc cref="IEquationOperatorParser"/>
    public sealed class EquationOperatorParser : EquationSpecifiedParserBase, IEquationOperatorParser
    {
        private readonly IOperator _operator;

        internal EquationOperatorParser(IEquationParser parent, IOperator @operator)
            : base(parent)
        {
            _operator = @operator;
        }

        public IEquationOperatorParser WithPrecedence(decimal precedence)
        {
            _operator.Precedence = precedence;
            return this;
        }

        public IEquationOperatorParser LeftAssociative()
        {
            _operator.IsLeftAssociative = true;
            return this;
        }

        public IEquationOperatorParser LeftAssociative(bool operatorIsLeftAssociative)
        {
            _operator.IsLeftAssociative = operatorIsLeftAssociative;
            return this;
        }

        public IEquationOperatorParser RightAssociative()
        {
            _operator.IsLeftAssociative = false;
            return this;
        }

        public IEquationOperatorParser RightAssociative(bool operatorIsRightAssociative)
        {
            _operator.IsLeftAssociative = !operatorIsRightAssociative;
            return this;
        }
    }
}
