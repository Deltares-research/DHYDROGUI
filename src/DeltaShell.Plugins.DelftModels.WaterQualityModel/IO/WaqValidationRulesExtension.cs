using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Properties;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.IO
{
    /// <summary>
    /// Extension class to be able to validate each of the Processes in the WAQ Model that are present
    /// at a given Substance Process library.
    /// </summary>
    public static class WaqValidationRulesExtension
    {
        /// <summary>
        /// Constants the process within rule limits.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <param name="rules">The rules.</param>
        /// <param name="parameterList">The parameter list.</param>
        /// <param name="reasonList">The reason list.</param>
        /// <returns>
        ///   <c>true</c> if [is within rules limits] [the specified rules]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsWithinRulesLimits(this IFunction parameter, IList<WaqProcessValidationRule> rules, IList<IFunction> parameterList,  out List<string> reasonList)
        {
            reasonList = new List<string>();

            if (rules == null || !rules.Any())
            {
                reasonList.Add(string.Format(Resources.WaqValidationRulesExtension_ConstantProcessWithinRuleLimits_No_rules_found_for__0__, parameter.Name));
                return true; /* We do not want to show a warning for ALL parameters without rules. */
            }

            /*First try to find if there is any rule with dependencies.*/
            foreach (var rule in rules)
            {
                var validRule = true;
                var dependencyRuleApplies = false;
                if (!string.IsNullOrEmpty(rule.Dependency))
                {
                    dependencyRuleApplies = rule.HasParameterDependency(parameterList);
                    /* If there is a dependency that is not met then it should not be evaluated.. */
                    if (!dependencyRuleApplies) continue;
                }

                if (!parameter.ValidateParameterValueType(rule)
                    || !parameter.ValidateRuleParameter(rule))
                {
                    if( dependencyRuleApplies) reasonList.Clear();

                    reasonList.Add(rule.GetWaqProcessValidationRuleAsString());
                    validRule = false;
                }
                /* If the dependency rule applies we do not look any further. */
                if (dependencyRuleApplies) return validRule;
            }

            return !reasonList.Any();
        }

        private static bool HasParameterDependency(this WaqProcessValidationRule rule, IList<IFunction> parameterList)
        {
            if (string.IsNullOrEmpty(rule.Dependency)) return false;
            
            /*For now we only accept equal dependencies, so we can keep this code simple (but handled). */
            var fields = rule.Dependency.Split('=');
            if (fields.Length != 2) return false; /* Invalid rule */

            //Find the dependency parameter.
            var dependencyName = fields[0].Trim().ToLowerInvariant();
            var dependencyValue = fields[1].Trim();

            var dependency = parameterList.FirstOrDefault(p => p.Name.ToLowerInvariant().Equals(dependencyName));

            double allowedValue;
            double parameterValue;
            if (!double.TryParse(dependencyValue, out allowedValue)
                || !dependency.GetParameterValue(out parameterValue)) return false;

            //If the values were retrieved then we can check whether the rule applies or not.
            return parameterValue.Equals(allowedValue);
        }

        private static bool GetParameterValue(this IFunction parameter, out double parameterValue)
        {
            parameterValue = double.NaN;
            if (parameter.Components == null || !parameter.Components.Any())
                return false;

            parameterValue = (double) parameter.Components[0].DefaultValue;
            return true;
        }

        private static bool ValidateParameterValueType(this IFunction parameter, WaqProcessValidationRule rule)
        {
            double parameterValue;
            if (!parameter.GetParameterValue(out parameterValue) 
                || double.IsNaN(parameterValue)) return false;

            if (rule.ValueType == typeof(int))
                return Math.Abs(parameterValue % 1) <= (double.Epsilon * 100);

            return true;
        }

        private static bool ValidateRuleParameter(this IFunction parameter, WaqProcessValidationRule rule)
        {
            double parameterValue;
            if (!parameter.GetParameterValue(out parameterValue)) return false;
            var ruleMin = rule.MinValue;
            var ruleMax = rule.MaxValue;

            if (ruleMin.Contains(":") || ruleMax.Contains(":"))
            {
                /* If the rule contains ranges, we are only interested in one of them being met. */
                return ruleMin.CheckValueInRange(parameterValue)
                       || ruleMax.CheckValueInRange(parameterValue);
            }

            /* Otherwise both limits need to be met. */
            return rule.MinValue.ValueIsWithinLimit(parameterValue, true)
                   && rule.MaxValue.ValueIsWithinLimit(parameterValue, false);
        }

        private static bool CheckValueInRange(this string rangeString, double value)
        {
            if (!rangeString.Contains(":")) return false; /* Not a range. */

            var rangeList = rangeString
                .Replace('[', ' ')
                .Replace(']', ' ')
                .Split(':');

            if (rangeList.Length != 2) return false; /* Not a valid range. */

            var minInRange = rangeList[0];
            var maxInRange = rangeList[1];
            return minInRange.ValueIsWithinLimit(value, true)
                   && maxInRange.ValueIsWithinLimit(value, false);
        }

        private static bool ValueIsWithinLimit(this string stringValue, double parameterValue, bool checkMinimum)
        {
            double value;
            if (stringValue.ToLowerInvariant().Contains("inf"))
            {
                value = checkMinimum
                    ? double.NegativeInfinity
                    : double.PositiveInfinity;
            }
            else if (!double.TryParse(stringValue, out value))
            {
                return false;
            }

            return checkMinimum
                ? value <= parameterValue
                : parameterValue <= value;
        }

        private static string GetWaqProcessValidationRuleAsString(this WaqProcessValidationRule rule)
        {
            var type = $"Type: '{(rule.ValueType == null || rule.ValueType == typeof(double) ? "Double" : "Int")}'.";
            var minValue = string.IsNullOrEmpty(rule.MinValue) ? string.Empty : $"Min value: {rule.MinValue}. ";
            var maxValue = string.IsNullOrEmpty(rule.MaxValue) ? string.Empty : $"Max value: {rule.MaxValue}. ";
            var dependency = string.IsNullOrEmpty(rule.Dependency) ? string.Empty : $"With dependency: {rule.Dependency}. ";
            return string.Concat(type, minValue, maxValue, dependency);
        }
    }
}