namespace DeltaShell.Plugins.DelftModels.RealTimeControl.IO.DataAccess
{
    /// <summary>
    /// Represents a reference to a constant leaf value
    /// </summary>
    public class ConstantLeafReference : IExpressionReference
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConstantLeafReference"/> class.
        /// </summary>
        /// <param name="value">The constant value.</param>
        public ConstantLeafReference(string value)
        {
            Value = value;
        }

        /// <summary>
        /// Gets the constant value.
        /// </summary>
        public string Value { get; }
    }
}