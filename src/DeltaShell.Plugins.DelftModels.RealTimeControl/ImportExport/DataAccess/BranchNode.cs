using System.Collections.Generic;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport.DataAccess
{
    public class BranchNode : IBranchNode
    {
        public BranchNode(Operator operatorValue)
        {
            OperatorValue = operatorValue;
            FirstNodeReference = new NodeReference();
            SecondNodeReference = new NodeReference();
        }

        public BranchNode(Operator operatorValue, string yName) : this(operatorValue)
        {
            YName = yName;
        }

        public NodeReference FirstNodeReference { get; }

        public IExpressionNode FirstNode
        {
            get => FirstNodeReference.Node;
            set => FirstNodeReference.Node = value;
        }

        public Operator OperatorValue { get; }

        public NodeReference SecondNodeReference{ get; }

        public IExpressionNode SecondNode
        {
            get => SecondNodeReference.Node;
            set => SecondNodeReference.Node = value;
        }

        public string YName { get; set; }

        public string GetExpression()
        {
            string left = FirstNode.GetExpression();
            string formatString = OperatorValue.ToFormatString();
            string right = SecondNode.GetExpression();

            return string.Format(formatString, left, right);
        }

        public IEnumerable<IExpressionNode> GetChildNodes()
        {
            yield return FirstNode;
            yield return SecondNode;

            if (FirstNode is IBranchNode firstBranchNode)
            {
                foreach (IExpressionNode childNode in firstBranchNode.GetChildNodes())
                {
                    yield return childNode;
                }
            }

            if (SecondNode is IBranchNode secondBranchNode)
            {
                foreach (IExpressionNode childNode in secondBranchNode.GetChildNodes())
                {
                    yield return childNode;
                }
            }
        }

        public override string ToString()
        {
            return GetExpression();
        }
    }
}