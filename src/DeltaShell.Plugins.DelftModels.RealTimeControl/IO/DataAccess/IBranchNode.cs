using System.Collections.Generic;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.IO.DataAccess
{
    /// <summary>
    /// Represents an expression branch node containing two child nodes and a mathematical operator.
    /// A branch node represents one sub expression, where the left and right operands may be sub expression themselves.
    /// </summary>
    /// <seealso cref="IExpressionNode"/>
    public interface IBranchNode : IExpressionNode
    {
        /// <summary>
        /// Gets the first child node.
        /// </summary>
        IExpressionNode FirstNode { get; }

        /// <summary>
        /// Gets the mathematical operator value.
        /// </summary>
        Operator OperatorValue { get; }

        /// <summary>
        /// Gets the second child node.
        /// </summary>
        IExpressionNode SecondNode { get; }

        /// <summary>
        /// Gets the y name, or output name, of the branch node.
        /// </summary>
        string YName { get; set; }

        /// <summary>
        /// Gets all child nodes recursively.
        /// </summary>
        /// <returns>
        /// All child nodes
        /// </returns>
        IEnumerable<IExpressionNode> GetChildNodes();
    }
}