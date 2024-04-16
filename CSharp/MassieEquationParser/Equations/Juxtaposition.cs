using System;

namespace Scot.Massie.EquationParser.Equations
{
    internal interface IJuxtaposition : IEquation
    {
        Func<double, double, double> JuxtapositionFunc { get; }

        IEquation LeftJuxtapand { get; }

        IEquation RightJuxtapand { get; }
    }

    internal class Juxtaposition : IJuxtaposition
    {
        public Func<double, double, double> JuxtapositionFunc { get; }

        public IEquation LeftJuxtapand  { get; }
        
        public IEquation RightJuxtapand { get; }

        public Juxtaposition(Func<double, double, double> juxtapositionFunc,
                             IEquation                    leftJuxtapand,
                             IEquation                    rightJuxtapand)
        {
            JuxtapositionFunc = juxtapositionFunc;
            LeftJuxtapand     = leftJuxtapand;
            RightJuxtapand    = rightJuxtapand;
        }

        public double Evaluate()
        {
            return JuxtapositionFunc(LeftJuxtapand.Evaluate(), RightJuxtapand.Evaluate());
        }
    }
}
