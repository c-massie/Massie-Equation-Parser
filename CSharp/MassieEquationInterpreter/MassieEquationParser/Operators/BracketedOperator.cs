using System;
using System.Collections.Generic;
using System.Linq;
using Scot.Massie.EquationParser.Equations;

namespace Scot.Massie.EquationParser.Operators
{
    internal interface IBracketedOperator
    {
        string OpeningSymbol { get; }
        
        string ClosingSymbol { get; }

        // If it does, it's as a comma-separated list of operands
        bool AllowsMultipleOperands { get; set; }

        // If it does, an empty set of brackets will be a valid equation
        bool AllowsNoOperands { get; set; }

        Func<IList<double>, double> Implementation { get; }

        double Evaluate(IList<IEquation> operands);
    }
    
    internal class BracketedOperator : IBracketedOperator
    {
        public string                      OpeningSymbol          { get; }
        public string                      ClosingSymbol          { get; }
        public bool                        AllowsMultipleOperands { get; set;  }
        public bool                        AllowsNoOperands       { get; set; }
        public Func<IList<double>, double> Implementation         { get; }

        public BracketedOperator(string                      openingSymbol,
                                 string                      closingSymbol,
                                 bool                        allowsMultipleOperands,
                                 bool                        allowsEmptyOperandList,
                                 Func<IList<double>, double> implementation)
        {
            OpeningSymbol          = openingSymbol;
            ClosingSymbol          = closingSymbol;
            AllowsMultipleOperands = allowsMultipleOperands;
            AllowsNoOperands       = allowsEmptyOperandList;
            Implementation         = implementation;
        }

        public BracketedOperator(string openingSymbol, string closingSymbol, Func<IList<double>, double> implementation)
            : this(openingSymbol, closingSymbol, false, false, implementation)
        { }

        public double Evaluate(IList<IEquation> operands)
        {
            return Implementation(operands.Select(x => x.Evaluate()).ToList());
        }
    }
}
