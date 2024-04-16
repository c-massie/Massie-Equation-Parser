using FluentAssertions;
using Xunit;

namespace Scot.Massie.EquationParser.Tests
{
    public class JuxtapositionTests
    {
        [Fact]
        public void Simple()
        {
            var eqp = new EquationParser()
                     .WithJuxtaposition((l, r) => l * r)
                     .WithVariable("a", 5)
                     .WithVariable("b", 7);

            var eq     = eqp.Parse("ab");
            var result = eq.Evaluate();

            result.Should().Be(35);
        }

        [Fact]
        public void Chained()
        {
            var eqp = new EquationParser()
                     .WithJuxtaposition((l, r) => l * r)
                     .WithVariable("a", 5)
                     .WithVariable("b", 7)
                     .WithVariable("c", 9);

            var eq     = eqp.Parse("abc");
            var result = eq.Evaluate();

            result.Should().Be(315);
        }

        [Fact]
        public void Bracketed()
        {
            var eqp = new EquationParser()
                     .WithJuxtaposition((l, r) => l * r)
                     .WithVariable("a", 5)
                     .WithVariable("b", 7)
                     .WithVariable("c", 9);

            var eq     = eqp.Parse("a(bc)");
            var result = eq.Evaluate();

            result.Should().Be(315);
        }
    }
}
