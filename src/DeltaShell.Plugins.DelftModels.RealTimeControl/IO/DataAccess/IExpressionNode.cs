namespace DeltaShell.Plugins.DelftModels.RealTimeControl.IO.DataAccess
{
    /// <summary>
    /// Expression node that can be part of a binary expression tree
    /// </summary>
    public interface IExpressionNode
    {
        /// <summary>
        /// Gets the expression of the binary tree
        /// with this instance as root node
        /// </summary>
        /// <returns>The expression.</returns>
        string GetExpression();
    }
}