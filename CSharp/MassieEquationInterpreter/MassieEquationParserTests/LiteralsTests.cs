using System.Globalization;
using FluentAssertions;
using Xunit;

namespace Scot.Massie.EquationParser.Tests
{
    public class LiteralsTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(7)]
        [InlineData(0.5)]
        [InlineData(132.9)]
        [InlineData(0.33333333)]
        [InlineData(-1)]
        [InlineData(-50)]
        [InlineData(-0.5)]
        [InlineData(-133.9)]
        public void Test(double value)
        {
            // Negative numbers are included, even though these should normally be handled by the "-" prefix unary
            // operator rather than being directly parsed as a number, because they should still be able to before
            // trying to parse it as a variable.
            
            var number = value.ToString(CultureInfo.InvariantCulture);

            var equationBuilder = new EquationParser();
            
            var equation        = equationBuilder.Parse(number);
            var result          = equation.Evaluate();

            result.Should().Be(value);
        }
    }
}
