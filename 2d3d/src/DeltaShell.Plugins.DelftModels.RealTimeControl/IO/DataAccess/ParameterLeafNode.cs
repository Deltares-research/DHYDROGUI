namespace DeltaShell.Plugins.DelftModels.RealTimeControl.IO.DataAccess
{
    /// <summary>
    /// Represents a leaf node containing a parameter name as value.
    /// this parameter name is a reference to either an Input or another Mathematical Expression.
    /// </summary>
    public class ParameterLeafNode : ILeafNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterLeafNode"/> class.
        /// </summary>
        /// <param name="value"> The parameter name. </param>
        public ParameterLeafNode(string value)
        {
            Value = value;
        }

        /// <summary>
        /// Gets or sets the value of the leaf node, referencing an Input or Mathematical Expression.
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