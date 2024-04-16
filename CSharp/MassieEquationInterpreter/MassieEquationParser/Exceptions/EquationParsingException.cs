using System;

namespace Scot.Massie.EquationParser.Exceptions
{
    /// <summary>
    /// Thrown when an instance of <see cref="EquationParser"/> is asked to parse an equation from text that, given its
    /// configuration, is not a valid equation.
    /// </summary>
    public class EquationParsingException : FormatException
    {
        private const string DefaultMessage = "Could not parse the equation: ";
        
        /// <summary>
        /// The equation that could not be parsed.
        /// </summary>
        public string UnparsedEquation { get; }

        public EquationParsingException(string unparsedEquation)
            : this(unparsedEquation, DefaultMessage + unparsedEquation)
        { }

        public EquationParsingException(string unparsedEquation, string message)
            : base(message)
        {
            UnparsedEquation = unparsedEquation;
        }

        public EquationParsingException(string unparsedEquation, string message, Exception innerException)
            : base(message, innerException)
        {
            UnparsedEquation = unparsedEquation;
        }
    }
}
