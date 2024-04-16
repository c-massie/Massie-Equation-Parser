using FluentAssertions;
using Xunit;

namespace Scot.Massie.EquationParser.Tests
{
    public class BracketedOperatorsTests
    {
        [Fact]
        public void Simple()
        {
            var eqp    = new EquationParser().WithBracketedOperator("[", "]", x => x * 3);
            var eq     = eqp.Parse("[7]");
            var result = eq.Evaluate();

            result.Should().Be(21);
        }

        [Fact]
        public void MultipleArgs()
        {
            var eqp = new EquationParser()
               .WithBracketedMultiOperator("[", "]", args => double.Parse(string.Join("", args)));

            var eq     = eqp.Parse("[7, 3, 2]");
            var result = eq.Evaluate();

            result.Should().Be(732);
        }

        [Fact]
        public void NestedSame()
        {
            var eqp    = new EquationParser().WithBracketedOperator("[", "]", x => x * 3);
            var eq     = eqp.Parse("[[7]]");
            var result = eq.Evaluate();

            result.Should().Be(63);
        }

        [Fact]
        public void NestedDifferent()
        {
            var eqp = new EquationParser()
                     .WithBracketedOperator("[", "]", x => x * 3)
                     .WithBracketedOperator("{", "}", x => x + 5);
            
            var eq     = eqp.Parse("[{7}]");
            var result = eq.Evaluate();

            result.Should().Be(36);
        }

        [Fact]
        public void MultipleArgsNestedSame()
        {
            var eqp    = new EquationParser()
               .WithBracketedMultiOperator("[", "]", args => double.Parse(string.Join("", args)) + 3);
            
            var eq     = eqp.Parse("[1, [2, 3, 4], 5]");
            var result = eq.Evaluate();

            result.Should().Be(12378);
        }

        [Fact]
        public void MultipleArgsNestedDifferent()
        {
            var eqp = new EquationParser()
                     .WithBracketedMultiOperator("[", "]", args => double.Parse(string.Join("", args)) + 3)
                     .WithBracketedMultiOperator("{", "}", args => double.Parse(string.Join("", args)) * 2);

            var eq     = eqp.Parse("[1, {2, 3, 4}, 5]");
            var result = eq.Evaluate();

            result.Should().Be(14688);
        }

        [Fact]
        public void SameSymbolForOpenerAndCloser()
        {
            var eqp    = new EquationParser().WithBracketedOperator("|", "|", x => x * 3);
            var eq     = eqp.Parse("|7|");
            var result = eq.Evaluate();

            result.Should().Be(21);
        }

        [Fact]
        public void SameSymbolForOpenerAndCloser_NestedSame()
        {
            var eqp    = new EquationParser().WithBracketedOperator("|", "|", x => x * 3);
            var eq     = eqp.Parse("||7||");
            var result = eq.Evaluate();

            result.Should().Be(63);
        }

        [Fact]
        public void SameSymbolForOpenerAndCloser_NestedDifferent()
        {
            var eqp = new EquationParser()
                     .WithBracketedOperator("|", "|", x => x * 3)
                     .WithBracketedOperator(":", ":", x => x + 5);

            var eq     = eqp.Parse("|:7:|");
            var result = eq.Evaluate();

            result.Should().Be(36);
        }

        [Fact]
        public void SameSymbolForOpenerAndCloser_MultipleArguments()
        {
            var eqp = new EquationParser()
               .WithBracketedMultiOperator("|", "|", x => double.Parse($"{x[0]}{x[1]}{x[2]}"));

            var eq     = eqp.Parse("|7, 2, 3|");
            var result = eq.Evaluate();

            result.Should().Be(723);
        }

        [Fact]
        public void SameSymbolForOpenerAndCloser_MultipleArgumentsNestedSame()
        {
            var eqp = new EquationParser()
               .WithBracketedMultiOperator("|", "|", x => x[0]*2 + x[1]*2 + x[2]*3);

            var eq     = eqp.Parse("||1, 2, 3|, |4, 5, 6|, |7, 8, 9||");
            var result = eq.Evaluate();

            result.Should().Be(273);
        }

        [Fact]
        public void SameSymbolForOpenerAndCloser_MultipleArgumentsNestedDifferent()
        {
            var eqp = new EquationParser()
                     .WithBracketedMultiOperator("|", "|", x => x[0] * 2 + x[1] * 2 + x[2] * 3)
                     .WithBracketedMultiOperator(":", ":", x => x[0] * 2 + x[1] * 2 + x[2] * 5);

            var eq     = eqp.Parse("|:1, 2, 3:, :4, 5, 6:, :7, 8, 9:|");
            var result = eq.Evaluate();

            result.Should().Be(363);
        }
    }
}
