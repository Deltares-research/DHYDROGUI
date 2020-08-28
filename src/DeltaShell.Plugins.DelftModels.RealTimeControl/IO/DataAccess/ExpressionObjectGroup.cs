using System.Collections.Generic;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.IO.DataAccess
{
    /// <summary>
    /// Represents a group of expression objects that should be assembled into one or multiple trees.
    /// </summary>
    public class ExpressionObjectGroup
    {
        /// <summary>
        /// Gets the expression objects.
        /// </summary>
        public IList<ExpressionObject> ExpressionObjects { get; } = new List<ExpressionObject>();

        /// <summary>
        /// Adds the specified <paramref name="expressionObject"/> to the group
        /// </summary>
        /// <param name="expressionObject">The expression object.</param>
        public void Add(ExpressionObject expressionObject)
        {
            ExpressionObjects.Add(expressionObject);
        }
    }
}