namespace DeltaShell.Plugins.DelftModels.RealTimeControl.IO.DataAccess
{
    /// <summary>
    /// Represents a leaf node containing a constant value or a parameter.
    /// </summary>
    public interface ILeafNode : IExpressionNode
    {
        /// <summary>
        /// Gets the value of this leaf node.
        /// </summary>
        string Value { get; set; }
    }
}