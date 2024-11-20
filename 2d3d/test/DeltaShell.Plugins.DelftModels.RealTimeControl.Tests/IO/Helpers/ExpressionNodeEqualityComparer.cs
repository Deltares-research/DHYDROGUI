using System.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.DataAccess;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.IO.Helpers
{
    /// <summary>
    /// Compares the equality of expression nodes.
    /// </summary>
    /// <seealso cref="IEqualityComparer{T}"/>
    internal class ExpressionNodeEqualityComparer : IEqualityComparer<IExpressionNode>
    {
        public bool Equals(IExpressionNode x, IExpressionNode y)
        {
            switch (x)
            {
                case BranchNode xBranch when y is BranchNode yBranch:
                    return EqualsBranchNode(xBranch, yBranch);
                case ParameterLeafNode xParameterLeaf when y is ParameterLeafNode yParameterLeaf:
                    return EqualsLeafNode(xParameterLeaf, yParameterLeaf);
                case ConstantValueLeafNode xConstantLeaf when y is ConstantValueLeafNode yConstantLeaf:
                    return EqualsLeafNode(xConstantLeaf, yConstantLeaf);
                default:
                    return false;
            }
        }

        public int GetHashCode(IExpressionNode node)
        {
            switch (node)
            {
                case ParameterLeafNode leaf:
                    return leaf.Value.GetHashCode();
                case BranchNode branch:
                    return branch.OperatorValue.GetHashCode() ^
                           GetHashCode(branch.FirstNode) ^
                           GetHashCode(branch.SecondNode);
                default:
                    return 0;
            }
        }

        private bool EqualsBranchNode(IBranchNode x, IBranchNode y)
        {
            return x.OperatorValue == y.OperatorValue &&
                   Equals(x.FirstNode, y.FirstNode) &&
                   Equals(x.SecondNode, y.SecondNode);
        }

        private static bool EqualsLeafNode(ILeafNode x, ILeafNode y)
        {
            return x.Value == y.Value;
        }
    }
}