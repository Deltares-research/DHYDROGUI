namespace DeltaShell.Plugins.DelftModels.RealTimeControl.IO.DataAccess
{
    /// <summary>
    /// Represents an expression parameter reference that references another <see cref="Xsd.ExpressionComplexType"/>,
    /// this could be another Mathematical Expression or a sub expression of the same Mathematical Expression. 
    /// </summary>
    public class ExpressionReference : IExpressionReference
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionReference"/> class.
        /// </summary>
        /// <param name="value"> The reference to the expression. </param>
        public ExpressionReference(string value)
        {
            Value = value;
        }

        /// <summary>
        /// Gets the reference to the expression.
        /// </summary>
        public string Value { get; }
    }
}