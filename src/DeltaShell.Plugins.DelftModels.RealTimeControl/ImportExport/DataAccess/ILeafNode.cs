namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport.DataAccess
{
    /// <summary>
    /// Represents a leaf node that has a leaf value such as a value or a parameter.
    /// </summary>
    public interface ILeafNode : IExpressionNode
    {
        /// <summary>
        /// Gets the value of this leaf node.
        /// </summary>
        string Value { get; set; }

    }
}