using System.Collections.Generic;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.IO.DataAccess
{
    /// <summary>
    /// Represents an expression branch node containing two child nodes and a mathematical operator.
    /// </summary>
    /// <seealso cref="IBranchNode"/>
    public class BranchNode : IBranchNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BranchNode"/> class.
        /// </summary>
        /// <param name="operatorValue">The operator value.</param>
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

        /// <summary>
        /// Gets the first child node reference.
        /// </summary>
        public NodeReference FirstNodeReference { get; }

        /// <summary>
        /// Gets the second child node reference.
        /// </summary>
        public NodeReference SecondNodeReference { get; }

        public IExpressionNode FirstNode
        {
            get => FirstNodeReference.Node;
            set => FirstNodeReference.Node = value;
        }

        public Operator OperatorValue { get; }

        public IExpressionNode SecondNode
        {
            get => SecondNodeReference.Node;
            set => SecondNodeReference.Node = value;
        }

        public string YName { get; set; }

        public override string ToString()
        {
            return GetExpression();
        }

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
    }
}