using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;

namespace DeltaShell.Plugins.FMSuite.Common.Dependency
{
    /// <summary>
    /// Represents a dependency expression in the form of "
    /// <c>
    /// propertyA is enabled if propertyB exists,
    /// is boolean and set to true
    /// </c>
    /// ".
    /// </summary>
    public class BooleanIsTrueDependencyExpression : DependencyExpressionBase
    {
        protected internal override string OnValidate(ModelProperty evaluatedProperty,
                                                      IEnumerable<ModelProperty> allProperties,
                                                      string dependencyExpression)
        {
            ModelProperty dependencyProperty = allProperties.FirstOrDefault(p => p.PropertyDefinition.FilePropertyKey.Equals(dependencyExpression,
                                                                                                                             StringComparison.InvariantCultureIgnoreCase));

            if (dependencyProperty != null && dependencyProperty.PropertyDefinition.DataType != typeof(bool))
            {
                return $"Model property '{dependencyExpression}' should be have 'boolean' data type.";
            }

            return null;
        }

        protected internal override Func<IEnumerable<ModelProperty>, bool> OnCompile(ModelProperty evaluatedProperty, string dependencyExpression)
        {
            return properties =>
            {
                ModelProperty dependencyProperty = properties?.FirstOrDefault(p => p.PropertyDefinition.FilePropertyKey.Equals(dependencyExpression,
                                                                                                                               StringComparison.InvariantCultureIgnoreCase));

                return dependencyProperty != null && FMParser.FromString<bool>(dependencyProperty.GetValueAsString());
            };
        }

        /// <summary>
        /// Matches the pattern: start with model property key, end.
        /// </summary>
        protected override string Regex => @"^\w+$";
    }
}