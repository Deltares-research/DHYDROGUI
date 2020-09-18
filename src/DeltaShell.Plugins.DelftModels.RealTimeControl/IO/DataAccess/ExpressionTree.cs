using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.IO.DataAccess
{
    /// <summary>
    /// Represents a binary expression tree starting at a root node.
    /// </summary>
    /// <seealso cref="IRtcDataAccessObject{MathematicalExpression}"/>
    public class ExpressionTree : IRtcDataAccessObject<MathematicalExpression>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionTree"/> class.
        /// </summary>
        /// <param name="rootNode">The root node.</param>
        /// <param name="controlGroupName">Name of the control group.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="expression">The expression.</param>
        public ExpressionTree(IBranchNode rootNode, string controlGroupName, string id, MathematicalExpression expression)
        {
            RootNode = rootNode;
            Id = id;
            ControlGroupName = controlGroupName;
            Object = expression;
        }

        /// <summary>
        /// Gets the root node of the tree.
        /// </summary>
        public IBranchNode RootNode { get; }

        public string ControlGroupName { get; }

        /// <summary>
        /// Gets the <see cref="MathematicalExpression"/> that was created from the tools config file.
        /// </summary>
        public MathematicalExpression Object { get; }

        public string Id { get; }
    }
}