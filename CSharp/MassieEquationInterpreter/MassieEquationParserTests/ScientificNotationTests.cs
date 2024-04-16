using FluentAssertions;
using Xunit;

namespace Scot.Massie.EquationParser.Tests
{
    public class ScientificNotationTests
    {
        [Fact]
        public void Simple()
        {
            var eqp    = new EquationParser();
            var eq     = eqp.Parse("7e0");
            var result = eq.Evaluate();

            result.Should().Be(7);
        }

        [Fact]
        public void Above()
        {
            var eqp    = new EquationParser();
            var eq     = eqp.Parse("7e2");
            var result = eq.Evaluate();

            result.Should().Be(700);
        }

        [Fact]
        public void Below()
        {
            var eqp    = new EquationParser();
            var eq     = eqp.Parse("70e-1");
            var result = eq.Evaluate();

            result.Should().Be(7);
        }

        [Fact]
        public void Negative()
        {
            var eqp    = new EquationParser();
            var eq     = eqp.Parse("-7e2");
            var result = eq.Evaluate();

            result.Should().Be(-700);
        }
    }
}
