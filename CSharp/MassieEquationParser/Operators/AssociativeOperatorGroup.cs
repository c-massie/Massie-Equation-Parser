using System;
using System.Collections.Generic;

namespace Scot.Massie.EquationParser.Operators
{
    internal interface IAssociativeOperatorGroup
    {
        bool IsLeftAssociative { get; }

        ICollection<IInfixOperator>? InfixOperators { get; }

        ICollection<IPrefixOperator>? PrefixOperators { get; }

        ICollection<IPostfixOperator>? PostfixOperators { get; }

        void AddOperator(IOperator op);
    }

    internal sealed class AssociativeOperatorGroup : IAssociativeOperatorGroup
    {
        public bool IsLeftAssociative { get; }

        public ICollection<IInfixOperator>?   InfixOperators   { get; private set; }
        public ICollection<IPrefixOperator>?  PrefixOperators  { get; private set; }
        public ICollection<IPostfixOperator>? PostfixOperators { get; private set; }

        public AssociativeOperatorGroup(bool isLeftAssociative)
        {
            IsLeftAssociative = isLeftAssociative;
        }

        public void AddOperator(IOperator op)
        {
            switch(op)
            {
                case IInfixOperator   inOp:   (InfixOperators   ??= new List<IInfixOperator  >()).Add(inOp  ); break;
                case IPrefixOperator  preOp:  (PrefixOperators  ??= new List<IPrefixOperator >()).Add(preOp ); break;
                case IPostfixOperator postOp: (PostfixOperators ??= new List<IPostfixOperator>()).Add(postOp); break;
                
                default: throw new NotImplementedException();
            }
        }
    }
}
