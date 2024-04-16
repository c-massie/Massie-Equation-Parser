using FluentAssertions;
using Xunit;

namespace Scot.Massie.EquationParser.Tests
{
    public class FunctionsTests
    {
        [Fact]
        public void Simple()
        {
            var eqp    = new EquationParser().WithFunction("f", x => 7);
            var eq     = eqp.Parse("f()");
            var result = eq.Evaluate();

            result.Should().Be(7);
        }

        [Fact]
        public void SingleArg()
        {
            var eqp    = new EquationParser().WithFunction("f", args => args[0] * 3);
            var eq     = eqp.Parse("f(7)");
            var result = eq.Evaluate();

            result.Should().Be(21);
        }

        [Fact]
        public void MultipleArgs()
        {
            var eqp    = new EquationParser().WithFunction("f", args => double.Parse($"{args[0]}{args[1]}{args[2]}"));
            var eq     = eqp.Parse("f(2, 4, 7)");
            var result = eq.Evaluate();

            result.Should().Be(247);
        }

        [Fact]
        public void Nested()
        {
            var eqp    = new EquationParser().WithFunction("f", args => double.Parse($"{args[0]}{args[1]}{args[2]}"));
            var eq     = eqp.Parse("f(2, f(1, 4, 3), 7)");
            var result = eq.Evaluate();

            result.Should().Be(21437);
        }

        [Fact]
        public void AlternativeArgumentSeparatorSymbol()
        {
            var eqp = new EquationParser().WithFunction("f", args => double.Parse($"{args[0]}{args[1]}{args[2]}"))
                                           .WithArgumentSeparatorSymbol("@");

            var eq     = eqp.Parse("f(2@ f(1@ 4@ 3)@ 7)");
            var result = eq.Evaluate();

            result.Should().Be(21437);
        }

        [Fact]
        public void AlternativeBracketSymbols()
        {
            var eqp    = new EquationParser().WithFunction("f", args => double.Parse($"{args[0]}{args[1]}{args[2]}"))
                                              .WithBracketSymbols("{", "}");
            
            var eq     = eqp.Parse("f{2, f{1, 4, 3}, 7}");
            var result = eq.Evaluate();

            result.Should().Be(21437);
        }

        [Fact]
        public void AlternativeBracketSymbols_SameOpenerAndCloser()
        {
            var eqp = new EquationParser().WithFunction("f", args => double.Parse($"{args[0]}{args[1]}{args[2]}"))
                                           .WithBracketSymbols("|", "|");
            
            var eq     = eqp.Parse("f|1, 4, 3|");
            var result = eq.Evaluate();

            result.Should().Be(143);
        }

        [Fact]
        public void AlternativeBracketAndArgumentSeparatorSymbols()
        {
            var eqp = new EquationParser().WithFunction("f", args => double.Parse($"{args[0]}{args[1]}{args[2]}"))
                                           .WithBracketSymbols("{", "}")
                                           .WithArgumentSeparatorSymbol("@");
            
            var eq     = eqp.Parse("f{2@ f{1@ 4@ 3}@ 7}");
            var result = eq.Evaluate();

            result.Should().Be(21437);
        }
    }
}
