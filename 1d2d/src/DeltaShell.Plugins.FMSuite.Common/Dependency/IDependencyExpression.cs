using System;
using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;

namespace DeltaShell.Plugins.FMSuite.Common.Dependency
{
    public interface IDependencyExpression
    {
        /// <summary>
        /// Indicates if this expression can compiled by this type of dependency.
        /// </summary>
        /// <param name="expression">The dependency expression.</param>
        /// <returns>True if able to compile the expression; false otherwise.</returns>
        bool CanHandleExpression(string expression);

        /// <summary>
        /// Compiles the <see cref="ModelPropertyDefinition.EnabledDependencies"/> of a <see cref="ModelProperty"/>.
        /// </summary>
        /// <param name="evaluatedProperty">Property whose dependencyExpression are to be compiled.</param>
        /// <param name="allProperties">Dictionary of all available model properties.</param>
        /// <param name="dependencyExpression">The dependencyExpression to check if the expression can be handled.</param>
        /// <returns>A method that returns true when the property is enabled, and false when it's not.</returns>
        /// <exception cref="FormatException">When <see cref="CanHandleExpression"/> returns false.</exception>
        /// <exception cref="ArgumentException">When there is a syntax error.</exception>
        Func<IEnumerable<ModelProperty>, bool> CompileExpression(ModelProperty evaluatedProperty,
                                                                         IEnumerable<ModelProperty> allProperties, string dependencyExpression);
    }
}