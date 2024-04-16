namespace Scot.Massie.EquationParser.Operators
{
    internal interface IOperatorGroup
    {
        IAssociativeOperatorGroup? LeftAssociativeOperators { get; }

        IAssociativeOperatorGroup? RightAssociativeOperators { get; }

        void AddOperator(IOperator op);
    }
    
    internal sealed class OperatorGroup : IOperatorGroup
    {
        public IAssociativeOperatorGroup? LeftAssociativeOperators  { get; private set; }
        public IAssociativeOperatorGroup? RightAssociativeOperators { get; private set; }

        public void AddOperator(IOperator op)
        {
            var associativeOperatorGroup = op.IsLeftAssociative
                                               ? LeftAssociativeOperators  ??= new AssociativeOperatorGroup(true)
                                               : RightAssociativeOperators ??= new AssociativeOperatorGroup(false);
            
            associativeOperatorGroup.AddOperator(op);
        }
    }
}
