using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;

namespace DeltaShell.Plugins.FMSuite.Common.Dependency
{
    /// <summary>
    /// Represents a dependency expression in the form of "<c>propertyA is enabled if propertyB exists,
    /// is boolean and set to true</c>".
    /// </summary>
    public class BooleanIsTrueDependencyExpression : DependencyExpressionBase
    {
        /// <summary>
        /// Matches the pattern: start with model property key, end.
        /// </summary>
        protected override string Regex { get { return @"^\w+$"; } }

        protected internal override string OnValidate(ModelProperty evaluatedProperty, IEnumerable<ModelProperty> allProperties, string dependencyExpression)
        {
            var dependencyProperty =
                        allProperties.FirstOrDefault(
                            p =>
                            p.PropertyDefinition.FilePropertyKey.Equals(dependencyExpression,
                                                                         StringComparison.InvariantCultureIgnoreCase));

            if (dependencyProperty != null && dependencyProperty.PropertyDefinition.DataType != typeof(bool))
            {
                return $"Model property '{dependencyExpression}' should be have 'boolean' data type.";
            }

            return null;
        }

        protected internal override Func<IEnumerable<ModelProperty>, bool> OnCompile(ModelProperty evaluatedProperty, IEnumerable<ModelProperty> allProperties, string dependencyExpression)
        {
            return properties =>
                {
                    var dependencyProperty =
                        properties?.FirstOrDefault(
                            p =>
                            p.PropertyDefinition.FilePropertyKey.Equals(dependencyExpression,
                                                                         StringComparison.InvariantCultureIgnoreCase));

                    if (dependencyProperty != null)
                    {
                        return DataTypeValueParser.FromString<bool>(dependencyProperty.GetValueAsString());
                    }
                    // Property does not exist -> Do not enable!
                    return false;
                };
        }
    }
}