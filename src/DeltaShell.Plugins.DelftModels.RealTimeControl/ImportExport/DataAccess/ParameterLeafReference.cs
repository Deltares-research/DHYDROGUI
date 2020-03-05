namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport.DataAccess
{
    /// <summary>
    /// Represents a reference to a parameter leaf value
    /// </summary>
    public class ParameterLeafReference : IExpressionReference
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LeafReference"/> class.
        /// </summary>
        /// <param name="value">The parameter name.</param>
        public ParameterLeafReference(string value)
        {
            Value = value;
        }

        /// <summary>
        /// Gets the parameter name.
        /// </summary>
        public string Value { get; }
    }
}
