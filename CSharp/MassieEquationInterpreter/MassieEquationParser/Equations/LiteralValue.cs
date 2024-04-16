namespace Scot.Massie.EquationParser.Equations
{
    internal interface ILiteralValue : IEquation
    {
        double Value { get; }
    }
    
    internal class LiteralValue : ILiteralValue
    {
        public double Value { get; }

        public LiteralValue(double value)
        {
            this.Value = value;
        }

        public double Evaluate()
        {
            return Value;
        }
    }
}
