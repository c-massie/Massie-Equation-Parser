using System;
using System.Collections.Generic;
using System.Linq;
using Scot.Massie.EquationParser.Functions;
using Scot.Massie.EquationParser.Operators;
using Scot.Massie.EquationParser.Utils;

namespace Scot.Massie.EquationParser
{
    internal interface IEquationStores
    {
        IEquationParser EquationParser { get; }

        string OpeningBracketSymbol { get; }

        string ClosingBracketSymbol { get; }

        string ArgumentSeparatorSymbol { get; }

        IDictionary<string, double> Variables { get; }

        IDictionary<string, IFunction> Functions { get; }

        IDictionary<decimal, IOperatorGroup> OperatorGroups { get; }

        IList<IOperatorGroup> OperatorGroupsInOrder { get; }

        ICollection<IBracketedOperator> BracketedOperators { get; }
        
        IDictionary<string, Dictionary<string, IBracketedOperator>> BracketedOperatorDict { get; }
        
        IDictionary<string, Dictionary<string, IBracketedOperator>> BracketedOperatorReverseDict { get; }

        Func<double, double, double>? JuxtapositionFunc { get; }

        IEquationStringUtils StringUtils { get; }
    }

    internal class EquationStores : IEquationStores
    {
        public IEquationParser EquationParser { get; }

        public string OpeningBracketSymbol { get; }

        public string ClosingBracketSymbol { get; }

        public string ArgumentSeparatorSymbol { get; }

        public IDictionary<string, double> Variables { get; }

        public IDictionary<string, IFunction> Functions { get; }

        public IDictionary<decimal, IOperatorGroup> OperatorGroups { get; } = new Dictionary<decimal, IOperatorGroup>();

        public IList<IOperatorGroup> OperatorGroupsInOrder { get; }

        public ICollection<IBracketedOperator> BracketedOperators { get; }

        // <opening bracket symbol, <closing bracket symbol, brop>>
        public IDictionary<string, Dictionary<string, IBracketedOperator>> BracketedOperatorDict { get; }

        // <closing bracket symbol, <opening bracket symbol, brop>>
        public IDictionary<string, Dictionary<string, IBracketedOperator>> BracketedOperatorReverseDict { get; }

        public Func<double, double, double>? JuxtapositionFunc { get; }

        public IEquationStringUtils StringUtils { get; }

        public EquationStores(IEquationParser                generalEquationParser,
                              string                          openingBracketSymbol,
                              string                          closingBracketSymbol,
                              string                          argumentSeparatorSymbol,
                              IDictionary<string, double>     variables,
                              IDictionary<string, IFunction>  functions,
                              ICollection<IOperator>          operators,
                              ICollection<IBracketedOperator> bracketedOperators,
                              Func<double, double, double>?   juxtapositionFunc)
        {
            EquationParser = generalEquationParser;

            OpeningBracketSymbol    = openingBracketSymbol;
            ClosingBracketSymbol    = closingBracketSymbol;
            ArgumentSeparatorSymbol = argumentSeparatorSymbol;
            
            Variables = new Dictionary<string, double>(variables);
            Functions = new Dictionary<string, IFunction>(functions);

            foreach(var op in operators)
            {
                if(!OperatorGroups.TryGetValue(op.Precedence, out var operatorGroup))
                    OperatorGroups[op.Precedence] = operatorGroup = new OperatorGroup();

                operatorGroup.AddOperator(op);
            }

            OperatorGroupsInOrder = OperatorGroups.OrderBy(kv => kv.Key)
                                                  .Select(kv => kv.Value)
                                                  .ToList();

            BracketedOperators = new List<IBracketedOperator>(bracketedOperators);
            JuxtapositionFunc = juxtapositionFunc;

            BracketedOperatorDict        = new Dictionary<string, Dictionary<string, IBracketedOperator>>();
            BracketedOperatorReverseDict = new Dictionary<string, Dictionary<string, IBracketedOperator>>();

            foreach(var brop in bracketedOperators)
            {
                BracketedOperatorDict       .SetInNestedDictionary(brop.OpeningSymbol, brop.ClosingSymbol, brop);
                BracketedOperatorReverseDict.SetInNestedDictionary(brop.ClosingSymbol, brop.OpeningSymbol, brop);
            }

            StringUtils = new EquationStringUtils(this);
        }
    }
}
