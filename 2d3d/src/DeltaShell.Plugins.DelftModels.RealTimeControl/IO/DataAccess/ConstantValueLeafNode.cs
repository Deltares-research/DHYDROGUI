namespace DeltaShell.Plugins.DelftModels.RealTimeControl.IO.DataAccess
{
    /// <summary>
    /// Represents an leaf node containing a value
    /// </summary>
    public class ConstantValueLeafNode : ILeafNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConstantValueLeafNode"/> class.
        /// </summary>
        /// <param name="value">The leaf value.</param>
        public ConstantValueLeafNode(string value)
        {
            Value = value;
        }

        /// <summary>
        /// Gets or sets the value of the leaf node.
        /// </summary>
        public string Value { get; set; }

        public override string ToString()
        {
            return GetExpression();
        }

        public string GetExpression()
        {
            return Value;
        }
    }
}