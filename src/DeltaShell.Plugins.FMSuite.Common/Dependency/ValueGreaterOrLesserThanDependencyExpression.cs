using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;

namespace DeltaShell.Plugins.FMSuite.Common.Dependency
{
    /// <summary>
    /// Represents a dependency expression in the form of "
    /// <c>
    /// propertyA is enabled if propertyB exists,
    /// is greater/less than (or equal to) a certain value, where propertyB represents a number'
    /// </c>
    /// ".
    /// </summary>
    public class ValueGreaterOrLesserThanDependencyExpression : DependencyExpressionBase
    {
        /// <summary>
        /// Matches the pattern: start with model property key, followed by any white space characters.
        /// </summary>
        private const string KeywordPart = @"^\w+\s*";

        /// <summary>
        /// Matches the pattern: end with a non-scientific intiger or double in <see cref="CultureInfo.InvariantCulture" />.
        /// </summary>
        private const string ValuePart = @"\s*-?\d+(\.\d+)?$";

        /// <summary>
        /// Matches either &gt;, &gt;=, &lt; or &lt;= .
        /// </summary>
        private const string ComparisonTokenPart = @"(>=|>|<=|<)"; // Note: Order matters!

        /// <summary>
        /// Matches the pattern: start with model property key, followed by any white space characters,
        /// followed by either &gt;, &gt;=, &lt; or &lt;=, followed by any white space characters and
        /// ending with a non-scientific integer or double in <see cref="CultureInfo.InvariantCulture" />.
        /// </summary>
        protected override string Regex => KeywordPart + ComparisonTokenPart + ValuePart;

        protected internal override string OnValidate(ModelProperty evaluatedProperty,
                                                      IEnumerable<ModelProperty> allProperties,
                                                      string dependencyExpression)
        {
            string dependencyPropertyName = GetDependencyPropertyName(evaluatedProperty, dependencyExpression);
            ModelProperty dependencyProperty =
                allProperties.FirstOrDefault(
                    p =>
                        p.PropertyDefinition.FilePropertyName.Equals(dependencyPropertyName,
                                                                     StringComparison.InvariantCultureIgnoreCase));
            if (dependencyProperty != null)
            {
                if (dependencyProperty.PropertyDefinition.DataType != typeof(double) &&
                    dependencyProperty.PropertyDefinition.DataType != typeof(int))
                {
                    return string.Format("Model property '{0}' should be have 'double' or 'integer' data type.",
                                         dependencyPropertyName);
                }
            }

            return null;
        }

        protected internal override Func<IEnumerable<ModelProperty>, bool> OnCompile(
            ModelProperty evaluatedProperty, IEnumerable<ModelProperty> allProperties, string dependencyExpression)
        {
            return properties =>
            {
                string dependencyPropertyName = GetDependencyPropertyName(evaluatedProperty, dependencyExpression);
                ModelProperty dependencyProperty =
                    properties?.FirstOrDefault(
                        p =>
                            p.PropertyDefinition.FilePropertyName.Equals(dependencyPropertyName,
                                                                         StringComparison.InvariantCultureIgnoreCase));
                if (dependencyProperty != null)
                {
                    double comparisonValue = GetComparisonValue(evaluatedProperty, dependencyExpression);
                    ComparisonType comparisonType = GetComparisonType(evaluatedProperty, dependencyExpression);

                    return EvaluateComparison(dependencyProperty, comparisonValue, comparisonType);
                }

                // Property does not exist -> Is not true -> Do not enable!
                return false;
            };
        }

        private static ComparisonType GetComparisonType(ModelProperty evaluatedProperty, string dependencyExpression)
        {
            string comparisonToken = RegularExpression.GetFirstMatch(ComparisonTokenPart, dependencyExpression).Value;
            switch (comparisonToken)
            {
                case ">":  return ComparisonType.GreaterThen;
                case ">=": return ComparisonType.GreaterThenEqual;
                case "<":  return ComparisonType.LessThen;
                case "<=": return ComparisonType.LessThenEqual;
                default:   throw new NotImplementedException();
            }
        }

        private static double GetComparisonValue(ModelProperty evaluatedProperty, string dependencyExpression)
        {
            string numberValue = RegularExpression.GetFirstMatch(ValuePart, dependencyExpression).Value.Trim();

            // Do not catch exceptions, as value should be kept valid by application
            return FMParser.FromString<double>(numberValue);
        }

        private static bool EvaluateComparison(ModelProperty dependencyProperty, double referenceValue,
                                               ComparisonType type)
        {
            var dependencyValue = FMParser.FromString<double>(dependencyProperty.GetValueAsString());
            switch (type)
            {
                case ComparisonType.LessThen:         return dependencyValue < referenceValue;
                case ComparisonType.LessThenEqual:    return dependencyValue <= referenceValue;
                case ComparisonType.GreaterThen:      return dependencyValue > referenceValue;
                case ComparisonType.GreaterThenEqual: return dependencyValue >= referenceValue;
                default:                              throw new NotImplementedException();
            }
        }

        private static string GetDependencyPropertyName(ModelProperty evaluatedProperty, string dependencyExpression)
        {
            return RegularExpression.GetFirstMatch(KeywordPart, dependencyExpression).Value.Trim();
        }

        private enum ComparisonType
        {
            LessThen,
            LessThenEqual,
            GreaterThen,
            GreaterThenEqual
        }
    }
}