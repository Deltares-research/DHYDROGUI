using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;

namespace DeltaShell.Plugins.FMSuite.Common.Dependency
{
    public abstract class DependencyExpressionBase : IDependencyExpression
    {
        public virtual bool CanHandleExpression(string expression)
        {
            if (string.IsNullOrEmpty(expression))
            {
                return false;
            }

            MatchCollection canHandleExpression = RegularExpression.GetMatches(Regex, expression);
            if (canHandleExpression.Count > 1)
            {
                throw new NotImplementedException("This should never happen.");
            }

            return canHandleExpression.Count == 1;
        }

        public Func<IEnumerable<ModelProperty>, bool> CompileExpression(ModelProperty evaluatedProperty,
                                                                        IEnumerable<ModelProperty> allProperties,
                                                                        string dependencyExpression)
        {
            if (!CanHandleExpression(dependencyExpression))
            {
                throw new FormatException("Cannot compile unsupported dependency expression.");
            }

            // Check if dependencyExpression are okey:
            string errorMessage = OnValidate(evaluatedProperty, allProperties, dependencyExpression);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                throw new ArgumentException(errorMessage);
            }

            return OnCompile(evaluatedProperty, dependencyExpression);
        }

        /// <summary>
        /// Perform precondition checks to see if compilation is possible.
        /// </summary>
        /// <param name="evaluatedProperty"> Property whose dependencyExpression are to be compiled. </param>
        /// <param name="allProperties"> Dictionary of all available model properties. </param>
        /// <param name="dependencyExpression">
        /// A substring from <see cref="ModelPropertyDefinition.EnabledDependencies"/>, to be
        /// determined by combinatory expressions.
        /// </param>
        /// <returns> Error message, or empty string or null when okey. </returns>
        protected internal abstract string OnValidate(ModelProperty evaluatedProperty,
                                                      IEnumerable<ModelProperty> allProperties,
                                                      string dependencyExpression);

        /// <summary>
        /// Compiles the dependency expression into a method that determines the enabled state.
        /// </summary>
        /// <param name="evaluatedProperty"> Property whose dependencyExpression are to be compiled. </param>
        /// <param name="dependencyExpression">
        /// A substring from <see cref="ModelPropertyDefinition.EnabledDependencies"/>, to be
        /// determined by combinatory expressions.
        /// </param>
        /// <returns> The method returning the enabled state. </returns>
        protected internal abstract Func<IEnumerable<ModelProperty>, bool> OnCompile(
            ModelProperty evaluatedProperty, string dependencyExpression);

        /// <summary>
        /// Regular expression used to determine if this class can handle the dependency expression.
        /// </summary>
        protected abstract string Regex { get; }
    }
}