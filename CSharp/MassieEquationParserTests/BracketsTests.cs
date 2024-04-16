using FluentAssertions;
using Xunit;

namespace Scot.Massie.EquationParser.Tests
{
    public class BracketsTests
    {
        [Fact]
        public void Simple()
        {
            var eqp    = new EquationParser();
            var eq     = eqp.Parse("(7)");
            var result = eq.Evaluate();

            result.Should().Be(7);
        }

        [Fact]
        public void Nested()
        {
            var eqp    = new EquationParser();
            var eq     = eqp.Parse("((7))");
            var result = eq.Evaluate();

            result.Should().Be(7);
        }

        [Fact]
        public void Multiple()
        {
            var eqp    = new EquationParser().WithBinaryOperator("+", (a, b) => a + b);
            var eq     = eqp.Parse("(4) + (7)");
            var result = eq.Evaluate();

            result.Should().Be(11);
        }
        
        [Fact]
        public void MultipleNested()
        {
            var eqp = new EquationParser()
                     .WithBinaryOperator("+", (a, b) => a + b)
                     .WithBinaryOperator("*", (a, b) => a * b);
            
            var eq     = eqp.Parse("3 + ((4) + (7)) * 2");
            var result = eq.Evaluate();

            result.Should().Be(28);
        }

        [Fact]
        public void AlternativeBracketSymbols()
        {
            var eqp = new EquationParser()
                     .WithBinaryOperator("+", (a, b) => a + b)
                     .WithBinaryOperator("*", (a, b) => a * b)
                     .WithBracketSymbols("{", "}");

            var eq     = eqp.Parse("4 + {7 * 2}");
            var result = eq.Evaluate();

            result.Should().Be(18);
        }

        [Fact]
        public void AlternativeBracketSymbols_LongerThan1Char()
        {
            var eqp = new EquationParser()
                     .WithBinaryOperator("+", (a, b) => a + b)
                     .WithBinaryOperator("*", (a, b) => a * b)
                     .WithBracketSymbols("[:", ":]");

            var eq     = eqp.Parse("4 + [:7 * 2:]");
            var result = eq.Evaluate();

            result.Should().Be(18);
        }

        [Fact]
        public void AlternativeBracketSymbols_SameSymbols()
        {
            var eqp = new EquationParser()
                     .WithBinaryOperator("+", (a, b) => a + b)
                     .WithBinaryOperator("*", (a, b) => a * b)
                     .WithBracketSymbols("|", "|");

            var eq     = eqp.Parse("4 + |7 * 2|");
            var result = eq.Evaluate();

            result.Should().Be(18);
        }
    }
}
