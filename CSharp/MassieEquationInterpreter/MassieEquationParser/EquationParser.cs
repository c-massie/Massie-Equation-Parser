using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Scot.Massie.EquationParser.Equations;
using Scot.Massie.EquationParser.EquationSubParsers;
using Scot.Massie.EquationParser.Exceptions;
using Scot.Massie.EquationParser.Functions;
using Scot.Massie.EquationParser.Operators;
using Scot.Massie.EquationParser.Utils;

namespace Scot.Massie.EquationParser
{
    /// <summary>
    /// A parser for equations, allowing you to specify how you want them to be parsed.
    /// </summary>
    /// <remarks>
    /// A "symbol" in this context is any meaningful chunk of text in an equation. e.g. a variable name, the text placed
    /// between two values to indicate a binary operator, a bracket, etc.
    /// </remarks>
    /// <remarks>
    /// An "operand" in this context are the arguments passed into an operator by having the operator's normal symbols
    /// placed in relation to them. e.g. a unary operator's operand is after the operator's symbol, a binary operator's
    /// operands are the values either side of the operator's symbol, and a bracketed operator's operands are the
    /// possibly comman-separated arguments placed between the two brackets that represent the operator.
    ///
    /// In "-5", "-" is the operator's symbol and "5" is the operand.
    /// </remarks>
    public interface IEquationParser
    {
        // TO DO: Add support for having juxtaposition represent a provided operator at that operator's precedence.
        //        This will probably mean compiling the equation, making the change, decompiling it, then re-compiling
        //        it.
        //
        //        An optimisation I could make would be only going through this process for equations that contain
        //        juxtapositions.
        //
        //        The method for this could be added to EquationOperatorParser, where I can describe an operator as
        //        being used for juxtaposition.

        // TO DO: Add the ability to record specific reasons an equation isn't parseable.
        //        Have sub-parsers add reasons (a class called EquationParseFailureReason?) it couldn't parse an
        //        equation to a shared list. If the equation can't be parsed, those reasons could then be given in the
        //        exception thrown.
        //
        //        This should be able to drill down recursively. e.g. A function call may not be parseable because one
        //        of its arguments may not be parseable, which may be for a number of reasons.
        //
        //        Possibly only include reasons where something indicates that a sub-parser's functionality *may* have
        //        intended to be used? e.g. don't include the reasoning for a function call failing to parse if there
        //        weren't any brackets. (As such, there would be nothing to indicate that the writer was trying to call
        //        a function)

        /// <summary>
        /// Specifies the text used to represent an open and closing bracket. Brackets are used to explicitly specify
        /// the order of operation, where the contents of brackets will be grouped and evaluated before the result of
        /// whatever is in the brackets is passed into whatever contains the equation in brackets.
        ///
        /// The opening and closing bracket symbols are "(" and ")" by default, respectively.
        /// </summary>
        /// <param name="openingBracketSymbol">The opening bracket symbol.</param>
        /// <param name="closingBracketSymbol">The closing bracket symbol.</param>
        /// <returns>This.</returns>
        IEquationParser WithBracketSymbols(string openingBracketSymbol, string closingBracketSymbol);

        /// <summary>
        /// Specifies the text used to separate arguments where arguments are needed. e.g. in the arguments passed to a
        /// function, in the arguments passed to a bracketed operation that can accept multiple arguments, etc.
        ///
        /// The argument separator is "," (comma) by default.
        /// </summary>
        /// <param name="argumentSeparatorSymbol">The argument separator symbol.</param>
        /// <returns>This.</returns>
        IEquationParser WithArgumentSeparatorSymbol(string argumentSeparatorSymbol);

        /// <summary>
        /// Specifies a numeric value to associate with a string of text. You will then be able to refer to the numeric
        /// value in equations by the variable name provided.
        /// </summary>
        /// <remarks>
        /// Variables are read before anything else, meaning that if an equation reads `*5` and there's both a prefix
        /// operator with the symbol "*" and a variable called "*5", it will evaluated to the value of the variable.
        /// </remarks>
        /// <param name="name">The name of the variable, as it can be referred to in equations.</param>
        /// <param name="value">The numeric value of the variable.</param>
        /// <returns>This.</returns>
        IEquationParser WithVariable(string name, double value);

        /// <summary>
        /// Specifies a function that accepts a set of arguments and provides a numeric value as a result. You will then
        /// be able to invoke this function with `name(...)` where "name" is the name of the function provided, and
        /// "..." is a list of arguments; other equations which will be passed to the function.
        /// </summary>
        /// <remarks>
        /// "(" and ")" when calling may require different symbols where <see cref="WithBracketSymbols"/> has been
        /// called.
        /// </remarks>
        /// <remarks>
        /// The separator used to separate arguments passed to the function is "," by default, but may be different
        /// where <see cref="WithArgumentSeparatorSymbol"/> has been called.
        /// </remarks>
        /// <param name="name">The name of the function.</param>
        /// <param name="implementation">
        /// The implementation of the function. The list of doubles passed in is the list of arguments passed into the
        /// function in the equation, after the arguments have been evaluated.
        /// </param>
        /// <returns>This.</returns>
        /// <example>
        /// With a function called "doot" that expected 3 arguments, an equation could be `doot(5, myvariable, 7 * 6)`
        /// </example>
        IEquationParser WithFunction(string name, Func<IList<double>, double> implementation);

        /// <summary>
        /// Specifies a prefix operator. A prefix operator is affixed to the start of an operand. It takes the evaluated
        /// value of the operand and returns the result of applying some operation to that value.
        /// </summary>
        /// <remarks>
        /// Where there are multiple operators with the same level of precedence, but some are left-associative and some
        /// are right-associative, the ones that are right-associative will always be treated as having a higher level
        /// of precedence.
        /// </remarks>
        /// <remarks>
        /// Where there are prefix and postfix operators with the same level of precedence, if they're left-associative,
        /// the prefix operators will be treated as having a higher level of precedence. Otherwise, the postfix
        /// operators will be treated as having a higher level of precedence.
        /// </remarks>
        /// <remarks>
        /// Where an equation could be read as having any one of a number of different prefix operators, the one with
        /// the lowest precedence will be preferred. Where they have the same precedence, the one with the longest
        /// symbol will be preferred.
        /// </remarks>
        /// <param name="symbol">The text representation of the operator.</param>
        /// <param name="implementation">
        /// The function that takes the operand and returns the result of applying this operator to that value.
        /// </param>
        /// <returns>A wrapper of this, which also exposes additional methods for customising the operator.</returns>
        /// <example>
        /// With a prefix operator defined using the symbol "*", `*5` would be an operation that applies the "*" prefix
        /// operator to "5".
        /// </example>
        IEquationOperatorParser WithPrefixOperator(string               symbol,
                                                   Func<double, double> implementation);

        /// <summary>
        /// Specifies a postfix operation. A postfix operator is affixed to the end of an operand. It takes the
        /// evaluated value of the operand and returns the result of applying some operation to that value.
        /// </summary>
        /// <remarks>
        /// Where there are multiple operators with the same level of precedence, but some are left-associative and some
        /// are right-associative, the ones that are right-associative will always be treated as having a higher level
        /// of precedence.
        /// </remarks>
        /// <remarks>
        /// Where there are prefix and postfix operators with the same level of precedence, if they're left-associative,
        /// the prefix operators will be treated as having a higher level of precedence. Otherwise, the postfix
        /// operators will be treated as having a higher level of precedence.
        /// </remarks>
        /// <remarks>
        /// Where an equation could be read as having any one of a number of different postfix operators, the one with
        /// the lowest precedence will be preferred. Where they have the same precedence, the one with the longest
        /// symbol will be preferred.
        /// </remarks>
        /// <param name="symbol">The text representation of the operator.</param>
        /// <param name="implementation">
        /// The function that takes the operand's evaluated value and returns the result of applying this operator to
        /// that value.
        /// </param>
        /// <returns>A wrapper of this, which also exposes additional methods for customising the operator.</returns>
        /// <example>
        /// With a postfix operator defined using the symbol "*", `5*` would be an operation that applies the "*"
        /// postfix operator to "5".
        /// </example>
        IEquationOperatorParser WithPostfixOperator(string               symbol,
                                                    Func<double, double> implementation);

        /// <summary>
        /// Specifies a binary operator. A binary operator is placed between two operands. It takes the evaluated value
        /// of each operand and returns the result of applying some operation to those values.
        /// </summary>
        /// <remarks>
        /// Where multiple binary operators could be read from a single equation, but their symbols overlap, the one
        /// with the lowest precedence will be preferred, for technical reasons.
        /// </remarks>
        /// <remarks>
        /// Where there are multiple operators with the same level of precedence, but some are left-associative and some
        /// are right-associative, the ones that are right-associative will always be treated as having a higher level
        /// of precedence.
        /// </remarks>
        /// <remarks>
        /// Where multiple binary operators overlap and have the same level of precedence, the one whose symbol starts
        /// and ends furthest in the direction opposite associativity will be preferred. Where their symbols start and
        /// end at the same indices, which one is preferred is undefined.
        /// </remarks>
        /// <param name="symbol">The text representation of the operator.</param>
        /// <param name="implementation">
        /// The function that takes two operands' evaluated values and returns the result of applying this operator to
        /// them.
        /// </param>
        /// <returns>A wrapper of this, which also exposes additional methods for customising the operator.</returns>
        /// <example>
        /// With a binary operator defined using the symbol "*", `5*7` would be an operation that applies the "*" binary
        /// operator to the operands "5" and "7".
        /// </example>
        IEquationOperatorParser WithBinaryOperator(string                       symbol,
                                                   Func<double, double, double> implementation);

        /// <summary>
        /// Specifies an infix operator. An infix operation involves the symbols of an infix operator being interleaved
        /// between the operands to be passed to it in order, and returns the result of applying some operation to the
        /// evaluated values of those operands.
        /// </summary>
        /// <remarks>
        /// Where there are multiple operators with the same level of precedence, but some are left-associative and some
        /// are right-associative, the ones that are right-associative will always be treated as having a higher level
        /// of precedence.
        /// </remarks>
        /// <remarks>
        /// Where multiple infix operators could be read from a single equation, but their symbols overlap, the one with
        /// the lowest precedence will be preferred, for technical reasons.
        /// </remarks>
        /// <remarks>
        /// Where multiple infix operators overlap and have the same level of precedence, the one whose symbols start
        /// and end furthest in the direction opposite associativity will be preferred. Where their symbols start and
        /// end at the same indices, which one is preferred is undefined.
        /// </remarks>
        /// <param name="symbols">The text representations of the operator.</param>
        /// <param name="implementation">
        /// The function that takes the operands' evaluated values and returns the result of applying this operator to
        /// them.
        /// </param>
        /// <returns>A wrapper of this, which also exposes additional methods for customising the operator.</returns>
        /// <example>
        /// With an infix operator defined using the symbols "?" and ":", `5?7:9` would be an operation that applies the
        /// "?"/":" operator to the operands "5", "7", and "9".
        /// </example>
        IEquationOperatorParser WithInfixOperator(IList<string>               symbols,
                                                  Func<IList<double>, double> implementation);

        /// <summary>
        /// Specifies a bracketed operator. The brackets of a bracketed operator are placed around the operand. It takes
        /// the evaluated value of the operand and returns the result of applying some operation to that value.
        /// </summary>
        /// <param name="openingSymbol">The opening bracket symbol of the operator.</param>
        /// <param name="closingSymbol">The closing bracket symbol of the operator.</param>
        /// <param name="implementation">
        /// The function that takes the operand's evaluated value and returns the result of applying this operator to
        /// that value.
        /// </param>
        /// <returns>This.</returns>
        /// <example>
        /// With a bracketed operator defined using the symbols "{" and "}", `{5}` would be an operation that applies
        /// the "{"/"}" bracketed operator to "5".
        /// </example>
        IEquationParser WithBracketedOperator(string               openingSymbol,
                                              string               closingSymbol,
                                              Func<double, double> implementation);

        /// <summary>
        /// Specifies a bracketed operator that can accept multiple operands. The brackets of a bracketed operator are
        /// placed around an argument list of operands. It takes the evaluated values of the operands and returns the
        /// result of applying some operation to them.
        /// </summary>
        /// <param name="openingSymbol">The opening bracket symbol of the operator.</param>
        /// <param name="closingSymbol">The closing bracket symbol of the operator.</param>
        /// <param name="implementation">
        /// The function that takes the operands' evaluated values and returns the result of applying this operator to
        /// those values.
        /// </param>
        /// <returns>A wrapper of this, which also exposes additional methods for customising the operator.</returns>
        /// <example>
        /// With a bracketed operator defined using the symbols "{" and "}", `{5, 7, 9}` would be an operation that
        /// applies the "{"/"}" bracketed operator to "5", "7", and "9".
        /// </example>
        IEquationBracketedOperatorParser WithBracketedMultiOperator(string                      openingSymbol,
                                                                    string                      closingSymbol,
                                                                    Func<IList<double>, double> implementation);

        /// <summary>
        /// Specifies that equations should be able to utilise implicit multiplication through juxtaposition. With
        /// juxtaposition enabled, when two equations are written consecutively in a way they couldn't otherwise be
        /// interpreted as a single equation, they will be interpreted as a multiplication of the two juxtapands.
        /// </summary>
        /// <remarks>
        /// I've used "Juxtapand" here to differentiate operands as part of an operation by juxtaposition from operands
        /// as part of a regular operation.
        /// </remarks>
        /// <remarks>Juxtaposition has a higher precedence than any operators.</remarks>
        /// <remarks>
        /// Where juxtaposition could be interpreted in multiple different ways, it will always be such that the right
        /// juxtapand is the longest juxtapand possible that doesn't contain juxtaposition itself.
        /// </remarks>
        /// <returns>This.</returns>
        /// <example>
        /// Where the variables `a` and `b` are available, `ab` would be interpreted as `a * b`, or a multiplied by b.
        /// </example>
        IEquationParser WithJuxtaposition();

        /// <summary>
        /// Specifies that equations should be able to utilise an implicit operation through juxtaposition. With this
        /// enabled and specified, when two equations are written consecutively in a way they couldn't otherwise be
        /// interpreted as a single equation, they will be interpreted as the two juxtapands being passed to the given
        /// juxtaposition implementation.
        /// </summary>
        /// <remarks>
        /// I've used "Juxtapand" here to differentiate operands as part of an operation by juxtaposition from operands
        /// as part of a regular operation.
        /// </remarks>
        /// <remarks>Juxtaposition has a higher precedence than any operators.</remarks>
        /// <remarks>
        /// Where juxtaposition could be interpreted in multiple different ways, it will always be such that the right
        /// juxtapand is the longest juxtapand possible that doesn't contain juxtaposition itself.
        /// </remarks>
        /// <param name="juxtapositionFunc">The operation to be performed upon juxtapands.</param>
        /// <returns>This.</returns>
        IEquationParser WithJuxtaposition(Func<double, double, double> juxtapositionFunc);

        /// <summary>
        /// Specifies the maximum number of layers deep an equation can be parsed to. Almost every way an equation can
        /// be parsed involves breaking up the equation into smaller components that are recursively then parsed as
        /// equations themselves - the maximum depth defines how many recursive layers deep this process can go before
        /// just giving up and failing to parse the equation.
        /// </summary>
        /// <remarks>
        /// This is primarily to prevent maliciously complex user-submitted equations from putting too much of a strain
        /// on the server. If users submit equations to be run remotely, I would also recommend limiting the length of
        /// strings that are passed into the equation parser to something sane. (e.g. 256 characters)
        /// </remarks>
        /// <remarks>
        /// By default, the depth limit is the maximum value of an integer.
        /// </remarks>
        /// <param name="depth">The maximum depth equations should be able to parsed to before giving up.</param>
        /// <returns>This.</returns>
        IEquationParser WithMaximumDepth(int depth);

        /// <summary>
        /// Specifies a small pre-defined set of basic core functions and operators for use in maths equations.
        /// </summary>
        /// <remarks>
        /// This adds a subset of what is added by <see cref="WithStandardMaths"/>, and should not be used in
        /// conjunction with that.
        /// </remarks>
        /// <returns>This.</returns>
        IEquationParser WithBasicMaths();

        /// <summary>
        /// Specifies a pre-defined set of standard variables, functions, and operators for use in general maths
        /// equations.
        /// </summary>
        /// <remarks>Also enables multiplication through juxtaposition.</remarks>
        /// <remarks>
        /// This adds a superset of what is added by <see cref="WithBasicMaths"/>, and should not be used in conjunction
        /// with that.
        /// </remarks>
        /// <returns>This.</returns>
        IEquationParser WithStandardMaths();

        /// <summary>
        /// Specifies a pre-defined set of functions and operators to support the use of logic in equations.
        /// </summary>
        /// <remarks>This can be used in conjunction with <see cref="WithStandardMaths"/>.</remarks>
        /// <returns>This.</returns>
        IEquationParser WithLogic();

        /// <summary>
        /// Parses the equation provided, given the configuration applied to this parser, into an equation object that
        /// can then be evaluated.
        /// </summary>
        /// <remarks>The same equation parser, once configured, may be used to parse multiple equations.</remarks>
        /// <remarks>
        /// The same equation can be evaluated multiple times. This may be useful where the results of functions may
        /// change, or variables used in the equation may be re-assigned. Evaluating the equation multiple times, even
        /// with different variable values and function implementations, will be faster than parsing it multiple times.
        /// </remarks>
        /// <param name="equation"></param>
        /// <returns>
        /// An equation object, compiled from the given text representation of the equation. This can have variables
        /// re-assigned and functions re-implemented to produce different results upon evaluation.
        /// </returns>
        /// <exception cref="EquationParsingException">Where an equation cannot be parsed.</exception>
        IAdjustableEquation Parse(string equation);

        /// <summary>
        /// Parses the equation provided given the configuration applied to this parser and produces the result of that
        /// equation as a double.
        /// </summary>
        /// <remarks>
        /// If the same equation may be evaluated multiple times, even with different variable values or different
        /// function implementations, you should use <see cref="Parse"/> instead - it will provide an equation object
        /// that can be evaluated multiple times without having to be re-parsed.
        /// </remarks>
        /// <param name="equation">The equation to be parsed and evaluated.</param>
        /// <returns>The result of the equation.</returns>
        /// <exception cref="EquationParsingException">Where an equation cannot be parsed.</exception>
        double Evaluate(string equation);
    }

    /// <inheritdoc cref="IEquationParser"/>
    public class EquationParser : IEquationParser
    {
        private readonly IDictionary<string, double> _variables = new Dictionary<string, double>();

        private readonly IDictionary<string, IFunction> _functions = new Dictionary<string, IFunction>();

        private readonly ICollection<IOperator> _operators = new List<IOperator>();

        private readonly ICollection<IBracketedOperator> _bracketedOperators = new List<IBracketedOperator>();

        private string _openingBracketSymbol = "(";

        private string _closingBracketSymbol = ")";

        private string _argumentSeparatorSymbol = ",";
        
        private Func<double, double, double>? _juxtapositionFunc;

        private int _maxDepth = int.MaxValue;

        internal static readonly IEquationSubParser[] SubParsers =
        {
            new VariableAccessSubParser(),
            new BracketedOperationSubParser(),
            new BracketsSubParser(),
            new FunctionCallSubParser(),
            new OperationSubParser(),
            new ScientificNotationSubParser(),
            new LiteralSubParser(),
            new JuxtapositionSubParser(),
        };

        public IEquationParser WithBracketSymbols(string openingBracketSymbol, string closingBracketSymbol)
        {
            _openingBracketSymbol = openingBracketSymbol.Trim();
            _closingBracketSymbol = closingBracketSymbol.Trim();
            return this;
        }

        public IEquationParser WithArgumentSeparatorSymbol(string argumentSeparatorSymbol)
        {
            _argumentSeparatorSymbol = argumentSeparatorSymbol.Trim();
            return this;
        }

        public IEquationParser WithVariable(string name, double value)
        {
            _variables[name.Trim()] = value;
            return this;
        }

        public IEquationParser WithFunction(string name, Func<IList<double>, double> implementation)
        {
            _functions[name.Trim()] = new Function(implementation);
            return this;
        }

        public IEquationOperatorParser WithPrefixOperator(string symbol, Func<double, double> implementation)
        {
            var op = new PrefixOperator(symbol.Trim(), implementation);
            _operators.Add(op);
            return new EquationOperatorParser(this, op);
        }

        public IEquationOperatorParser WithPostfixOperator(string symbol, Func<double, double> implementation)
        {
            var op = new PostfixOperator(symbol.Trim(), implementation);
            _operators.Add(op);
            return new EquationOperatorParser(this, op);
        }

        public IEquationOperatorParser WithBinaryOperator(string symbol, Func<double, double, double> implementation)
        {
            var op = new InfixOperator(symbol.Trim(), implementation);
            _operators.Add(op);
            return new EquationOperatorParser(this, op);
        }

        public IEquationOperatorParser WithInfixOperator(IList<string>               symbols, 
                                                         Func<IList<double>, double> implementation)
        {
            var op = new InfixOperator(symbols.Select(x => x.Trim()).ToList(), implementation);
            _operators.Add(op);
            return new EquationOperatorParser(this, op);
        }

        public IEquationParser WithBracketedOperator(string               openingSymbol,
                                                     string               closingSymbol,
                                                     Func<double, double> implementation)
        {
            var op = new BracketedOperator(openingSymbol.Trim(), closingSymbol.Trim(), x => implementation(x[0]));
            _bracketedOperators.Add(op);
            return new EquationBracketedOperatorParser(this, op);
        }

        public IEquationBracketedOperatorParser WithBracketedMultiOperator(string                      openingSymbol,
                                                                           string                      closingSymbol,
                                                                           Func<IList<double>, double> implementation)
        {
            var op = new BracketedOperator(openingSymbol.Trim(), closingSymbol.Trim(), implementation);
            op.AllowsMultipleOperands = true;
            _bracketedOperators.Add(op);
            return new EquationBracketedOperatorParser(this, op);
        }

        public IEquationParser WithJuxtaposition()
        {
            return WithJuxtaposition((a, b) => a * b);
        }

        public IEquationParser WithJuxtaposition(Func<double, double, double> juxtapositionFunc)
        {
            _juxtapositionFunc = juxtapositionFunc;
            return this;
        }

        public IEquationParser WithMaximumDepth(int depth)
        {
            _maxDepth = depth;
            return this;
        }

        public IEquationParser WithBasicMaths()
        {
            WithBinaryOperator("+", (a, b) => a + b).WithPrecedence(100);
            WithBinaryOperator("-", (a, b) => a - b).WithPrecedence(100);
            
            WithBinaryOperator("*", (a, b) => a * b).WithPrecedence(200);
            WithBinaryOperator("/", (a, b) => a / b).WithPrecedence(200);
            WithBinaryOperator("%", (a, b) => a % b).WithPrecedence(200);
            
            WithBinaryOperator("^", (a, b) => Math.Pow(a, b)).WithPrecedence(300).RightAssociative();

            WithPrefixOperator("+", x => x).WithPrecedence(500);
            WithPrefixOperator("-", x => -x).WithPrecedence(500);
            WithPrefixOperator("√", x => Math.Sqrt(x)).WithPrecedence(500);

            WithFunction("sqrt", args =>
            {
                if(args.Count == 0)
                    return 1;

                return Math.Sqrt(args[0]);
            });

            WithFunction("round", args =>
            {
                if(args.Count == 0)
                    return 0;

                if(args.Count == 1)
                    return Math.Round(args[0]);

                return Math.Round(args[0], (int)args[1]);
            });
            
            WithFunction("max", args =>
            {
                if(args.Count == 0)
                    return 0;

                return args.Max();
            });

            WithFunction("min", args =>
            {
                if(args.Count == 0)
                    return 0;

                return args.Min();
            });

            return this;
        }

        public IEquationParser WithStandardMaths()
        {
            WithVariable("pi",  Math.PI);
            WithVariable("e",   Math.E);
            WithVariable("tau", Math.Tau);
            WithVariable("phi", (1d + Math.Sqrt(5)) / 2);

            WithVariable("π", Math.PI);
            WithVariable("τ", Math.Tau);
            WithVariable("φ", (1d + Math.Sqrt(5)) / 2);
            WithVariable("ϕ", (1d + Math.Sqrt(5)) / 2);
            
            WithBinaryOperator("+", (a, b) => a + b).WithPrecedence(100);
            WithBinaryOperator("-", (a, b) => a - b).WithPrecedence(100);

            WithBinaryOperator("*", (a, b) => a * b).WithPrecedence(200);
            WithBinaryOperator("×", (a, b) => a * b).WithPrecedence(200);
            WithBinaryOperator("·", (a, b) => a * b).WithPrecedence(200);
            WithBinaryOperator("/", (a, b) => a / b).WithPrecedence(200);
            WithBinaryOperator("÷", (a, b) => a / b).WithPrecedence(200);
            WithBinaryOperator("%", (a, b) => a % b).WithPrecedence(200);

            WithBinaryOperator("^", (a, b) => Math.Pow(a, b)).WithPrecedence(300).RightAssociative();

            WithBinaryOperator("√", (a, b) => Math.Pow(b, 1d / a)).WithPrecedence(400);

            WithPrefixOperator("+", x => x).WithPrecedence(500);
            WithPrefixOperator("-", x => -x).WithPrecedence(500);
            WithPrefixOperator("√", x => Math.Sqrt(x)).WithPrecedence(500);

            WithPostfixOperator("%", x => x / 100).WithPrecedence(600);
            WithPostfixOperator("‰", x => x / 1000).WithPrecedence(600);

            WithBracketedOperator("⌊", "⌋", Math.Floor);
            WithBracketedOperator("⌈", "⌉", Math.Ceiling);
            WithBracketedOperator("|", "|", Math.Abs);

            WithFunction("log", args =>
            {
                if(args.Count == 0)
                    return 1;

                if(args.Count == 1)
                    return Math.Log(args[0]);

                return Math.Log(args[0], args[1]);
            });

            WithFunction("sqrt", args =>
            {
                if(args.Count == 0)
                    return 1;

                return Math.Sqrt(args[0]);
            });

            WithFunction("cbrt", args =>
            {
                if(args.Count == 0)
                    return 1;

                return Math.Cbrt(args[0]);
            });

            WithFunction("ceil", args =>
            {
                if(args.Count == 0)
                    return 0;

                return Math.Ceiling(args[0]);
            });

            WithFunction("floor", args =>
            {
                if(args.Count == 0)
                    return 0;

                return Math.Floor(args[0]);
            });

            WithFunction("abs", args =>
            {
                if(args.Count == 0)
                    return 0;

                return Math.Abs(args[0]);
            });

            WithFunction("round", args =>
            {
                if(args.Count == 0)
                    return 0;

                if(args.Count == 1)
                    return Math.Round(args[0]);

                return Math.Round(args[0], (int)args[1]);
            });

            WithFunction("max", args =>
            {
                if(args.Count == 0)
                    return 0;

                return args.Max();
            });

            WithFunction("min", args =>
            {
                if(args.Count == 0)
                    return 0;

                return args.Min();
            });

            WithJuxtaposition();

            return this;
        }

        public IEquationParser WithLogic()
        {
            WithInfixOperator(new[] { "?", ":" }, operands => operands[0] >= 0.5 ? operands[1] : operands[2])
               .RightAssociative()
               .WithPrecedence(700);

            WithBinaryOperator("||", (a, b) => (a >= 0.5) || (b >= 0.5) ? 1 : 0).WithPrecedence(800);

            WithBinaryOperator("&&", (a, b) => (a >= 0.5) && (b >= 0.5) ? 1 : 0).WithPrecedence(900);

            WithInfixOperator(new[] { "==", "~" },
                              operands => operands[0].EqualsWithMargin(operands[1], operands[2]) ? 1 : 0)
               .WithPrecedence(950);

            WithBinaryOperator("==", (a, b) => a.EqualsWithMargin(b) ? 1 : 0).WithPrecedence(1000);

            WithBinaryOperator("<",  (a, b) => (a < b) ? 1 : 0).WithPrecedence(1100);
            WithBinaryOperator(">",  (a, b) => (a < b) ? 1 : 0).WithPrecedence(1100);
            WithBinaryOperator("<=", (a, b) => (a < b) ? 1 : 0).WithPrecedence(1100);
            WithBinaryOperator(">=", (a, b) => (a < b) ? 1 : 0).WithPrecedence(1100);

            WithPrefixOperator("!", x => x >= 0.5 ? 0 : 1).WithPrecedence(1200);

            WithFunction("any", args => args.Any(x => x >= 0.5) ? 1 : 0);
            WithFunction("all", args => args.All(x => x >= 0.5) ? 1 : 0);

            return this;
        }

        public IAdjustableEquation Parse(string equation)
        {
            equation = Regex.Replace(equation, @"\s", " ");
            
            var stores = new EquationStores(this,
                                            _openingBracketSymbol,
                                            _closingBracketSymbol,
                                            _argumentSeparatorSymbol,
                                            _variables,
                                            _functions,
                                            _operators,
                                            _bracketedOperators,
                                            _juxtapositionFunc);

            var eq = Parse(equation, stores, _maxDepth);

            if(eq is null)
                throw new EquationParsingException(equation);

            return new AdjustableEquation(eq, stores);
        }

        public double Evaluate(string equation)
        {
            return Parse(equation).Evaluate();
        }

        internal static IEquation? Parse(string equation, IEquationStores stores, int depthRemaining)
        {
            equation = equation.Trim();
            
            if(depthRemaining-- <= 0)
                return null;

            foreach(var subParser in SubParsers)
                if(subParser.Parse(equation, stores, depthRemaining) is { } eq)
                    return eq;

            return null;
        }

        internal static IList<IEquation>? ParseArgumentList(string          argumentListString,
                                                            IEquationStores stores,
                                                            int             depthRemaining)
        {
            argumentListString = argumentListString.Trim();

            if(argumentListString.Length == 0)
                return new List<IEquation>();

            var argumentStrings = stores.StringUtils.SplitStringBySeparatorsNotInBrackets(argumentListString);
            var arguments       = new List<IEquation>(argumentStrings.Count);

            foreach(var argStr in argumentStrings)
            {
                if(!(Parse(argStr, stores, depthRemaining) is { } argument))
                    return null;

                arguments.Add(argument);
            }

            return arguments;
        }
    }
}
