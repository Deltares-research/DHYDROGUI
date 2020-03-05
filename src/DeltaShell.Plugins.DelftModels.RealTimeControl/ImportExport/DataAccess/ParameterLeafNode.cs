namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport.DataAccess
{
    /// <summary>
    /// Represents an leaf node containing a value
    /// </summary>
    public class ParameterLeafNode : ILeafNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterLeafNode"/> class.
        /// </summary>
        /// <param name="value">The leaf value.</param>
        public ParameterLeafNode(string value)
        {
            Value = value;
        }

        /// <summary>
        /// Gets or sets the value of the leaf node.
        /// </summary>
        public string Value { get; set; }

        public string GetExpression()
        {
            return Value;
        }

        public override string ToString()
        {
            return GetExpression();
        }
    }
}
