using FluentAssertions;
using Xunit;

namespace Scot.Massie.EquationParser.Tests
{
    public class VariablesTests
    {
        [Theory]
        [InlineData("x",       0)]
        [InlineData("y",       7)]
        [InlineData("$",       14)]
        [InlineData("doot",    22)]
        [InlineData("doot",    33.6)]
        [InlineData("co-op",   43)]
        [InlineData("a value", 29)]
        public void Test(string name, double value)
        {
            var eqp    = new EquationParser().WithVariable(name, value);
            var eq     = eqp.Parse(name);
            var result = eq.Evaluate();

            result.Should().Be(value);
        }
    }
}
