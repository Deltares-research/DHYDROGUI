using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.NGHS.IO;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;

namespace DeltaShell.Plugins.FMSuite.Common.Dependency
{
    /// <summary>
    /// Represents a dependency expression in the form of "<c>propertyA is enabled if propertyB exists and
    /// equals to any of the following values</c>".
    /// </summary>
    public class ValueEqualsDependencyExpression : DependencyExpressionBase
    {
        /// <summary>
        /// Matches the pattern: start with model property key, followed by any white space characters.
        /// </summary>
        private const string KeywordPart = @"^\w+\s*";

        /// <summary>
        /// Matches the pattern: end with a array of integer values.
        /// </summary>
        private const string ValueArrayPart = @"\s*-?\d+(\|-?\d+)*$";

        protected override string Regex
        {
            get { return KeywordPart + "=" + ValueArrayPart; }
        }

        protected internal override string OnValidate(ModelProperty evaluatedProperty, IEnumerable<ModelProperty> allProperties, string dependencyExpression)
        {
            var dependencyPropertyName = GetDependencyPropertyName(dependencyExpression);
            var dependencyProperty =
                allProperties?.FirstOrDefault(
                    p =>
                    p.PropertyDefinition.FilePropertyKey.Equals(dependencyPropertyName,
                                                                 StringComparison.InvariantCultureIgnoreCase));
            if (dependencyProperty != null && dependencyProperty.PropertyDefinition.DataType != typeof(bool) &&
                dependencyProperty.PropertyDefinition.DataType != typeof(int) &&
                !dependencyProperty.PropertyDefinition.DataType.IsEnum)
            {
                return $"Model property '{dependencyPropertyName}' should be have 'bool', 'int' or 'enum' data type.";
            }

            return null;
        }

        protected internal override Func<IEnumerable<ModelProperty>, bool> OnCompile(ModelProperty evaluatedProperty, IEnumerable<ModelProperty> allProperties, string dependencyExpression)
        {
            return properties =>
                {
                    var dependencyPropertyName = GetDependencyPropertyName(dependencyExpression);
                    var dependencyProperty =
                        properties?.FirstOrDefault(
                            p =>
                            p.PropertyDefinition.FilePropertyKey.Equals(dependencyPropertyName,
                                                                         StringComparison.InvariantCultureIgnoreCase));

                    if (dependencyProperty != null)
                    {
                        var comparisonValues = GetComparisonValues(dependencyExpression);

                        return comparisonValues.Contains(Convert.ToInt32(dependencyProperty.Value));
                    }
                    // Property does not exist -> Is not true -> Do not enable!
                    return false;
                };
        }

        private static IEnumerable<int> GetComparisonValues(string dependencyExpression)
        {
            var numberValue = RegularExpression.GetFirstMatch(ValueArrayPart, dependencyExpression).Value.Trim();

            // Do not catch exceptions, as value should be kept valid by application
            return numberValue.Split('|').Select(DataTypeValueParser.FromString<int>);
        }

        private static string GetDependencyPropertyName(string dependencyExpression)
        {
            return RegularExpression.GetFirstMatch(KeywordPart, dependencyExpression).Value.Trim();
        }
    }
}