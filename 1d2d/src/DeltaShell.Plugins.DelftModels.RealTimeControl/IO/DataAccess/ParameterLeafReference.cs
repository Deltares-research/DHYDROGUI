namespace DeltaShell.Plugins.DelftModels.RealTimeControl.IO.DataAccess
{
    /// <summary>
    /// Represents a reference to a parameter leaf value.
    /// </summary>
    public class ParameterLeafReference : IExpressionReference
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterLeafReference"/> class.
        /// </summary>
        /// <param name="value"> The parameter reference. </param>
        public ParameterLeafReference(string value)
        {
            Value = value;
        }

        /// <summary>
        /// Gets the parameter reference.
        /// </summary>
        public string Value { get; }
    }
}