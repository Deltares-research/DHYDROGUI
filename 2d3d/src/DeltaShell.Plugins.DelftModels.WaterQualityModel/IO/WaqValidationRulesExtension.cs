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
        /// <param name="parameter"> The parameter. </param>
        /// <param name="rules"> The rules. </param>
        /// <param name="parameterList"> The parameter list. </param>
        /// <param name="reasonList"> The reason list. </param>
        /// <returns>
        /// <c> true </c> if [is within rules limits] [the specified rules]; otherwise, <c> false </c>.
        /// </returns>
        public static bool IsWithinRulesLimits(this IFunction parameter, IList<WaqProcessValidationRule> rules,
                                               IList<IFunction> parameterList, out List<string> reasonList)
        {
            reasonList = new List<string>();

            if (rules == null || !rules.Any())
            {
                reasonList.Add(string.Format(
                                   Resources
                                       .WaqValidationRulesExtension_ConstantProcessWithinRuleLimits_No_rules_found_for__0__,
                                   parameter.Name));
                return true; /* We do not want to show a warning for ALL parameters without rules. */
            }

            foreach (WaqProcessValidationRule rule in rules)
            {
                /*First evaluate the dependency, if it applies, then we skip the rest.*/
                if (!string.IsNullOrEmpty(rule.Dependency))
                {
                    bool dependencyRuleApplies = rule.HasParameterDependency(parameterList);
                    if (!dependencyRuleApplies)
                    {
                        continue; /* If there is a dependency that is not met then it should not be evaluated.. */
                    }

                    reasonList.Clear();
                    return parameter.ValidateRuleParameter(rule, ref reasonList);
                }

                parameter.ValidateRuleParameter(rule, ref reasonList);
            }

            return !reasonList.Any();
        }

        private static bool HasParameterDependency(this WaqProcessValidationRule rule, IList<IFunction> parameterList)
        {
            if (string.IsNullOrEmpty(rule.Dependency))
            {
                return false;
            }

            /*For now we only accept equal dependencies, so we can keep this code simple (but handled). */
            string[] fields = rule.Dependency.Split('=');
            if (fields.Length != 2)
            {
                return false; /* Invalid rule */
            }

            //Find the dependency parameter.
            string dependencyName = fields[0].Trim().ToLowerInvariant();
            string dependencyValue = fields[1].Trim();

            IFunction dependency = parameterList?.FirstOrDefault(p => p.Name.ToLowerInvariant().Equals(dependencyName));

            double allowedValue;
            double parameterValue;
            if (dependency == null
                || !double.TryParse(dependencyValue, out allowedValue)
                || !dependency.GetParameterValue(out parameterValue))
            {
                return false;
            }

            //If the values were retrieved then we can check whether the rule applies or not.
            return parameterValue.Equals(allowedValue);
        }

        private static bool GetParameterValue(this IFunction parameter, out double parameterValue)
        {
            parameterValue = double.NaN;
            if (parameter.Components == null || !parameter.Components.Any())
            {
                return false;
            }

            parameterValue = (double) parameter.Components[0].DefaultValue;
            return true;
        }

        private static bool ValidateParameterValueType(this IFunction parameter, WaqProcessValidationRule rule,
                                                       out double parameterValue)
        {
            if (!parameter.GetParameterValue(out parameterValue)
                || double.IsNaN(parameterValue))
            {
                return false;
            }

            if (rule.ValueType == typeof(int))
            {
                return Math.Abs(parameterValue % 1) <= double.Epsilon * 100;
            }

            return true;
        }

        private static bool ValidateRuleParameter(this IFunction parameter, WaqProcessValidationRule rule,
                                                  ref List<string> issuesReasons)
        {
            double parameterValue;

            if (!parameter.ValidateParameterValueType(rule, out parameterValue))
            {
                issuesReasons.Add(
                    parameter.GetWaqProcessValidationRuleAsString(rule, $"should not be a decimal value"));
                return false;
            }

            string ruleMin = rule.MinValue;
            string ruleMax = rule.MaxValue;

            if (rule.MinValue.Contains(":")
                || rule.MaxValue.Contains(":"))
            {
                /* If the rule contains ranges, we are only interested in one of them being met. */
                bool valueInRange = ruleMin.CheckValueInRange(parameterValue) ||
                                    ruleMax.CheckValueInRange(parameterValue);
                if (!valueInRange)
                {
                    issuesReasons.Add(parameter.GetWaqProcessValidationRuleAsString(
                                          rule, $"should be in the range of {rule.MinValue} or {rule.MaxValue}"));
                }

                return valueInRange;
            }

            /* Otherwise both limits need to be met. */
            bool validValue = rule.MinValue.ValueIsWithinLimit(parameterValue, true)
                              && rule.MaxValue.ValueIsWithinLimit(parameterValue, false);
            if (!validValue)
            {
                issuesReasons.Add(parameter.GetWaqProcessValidationRuleAsString(rule, rule.GetRuleLimitMessage()));
            }

            return validValue;
        }

        private static bool CheckValueInRange(this string rangeString, double value)
        {
            if (!rangeString.Contains(":"))
            {
                return false; /* Not a range. */
            }

            string[] rangeList = rangeString
                                 .Replace('[', ' ')
                                 .Replace(']', ' ')
                                 .Split(':');

            if (rangeList.Length != 2)
            {
                return false; /* Not a valid range. */
            }

            string minInRange = rangeList[0];
            string maxInRange = rangeList[1];
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

        private static string GetWaqProcessValidationRuleAsString(this IFunction parameter,
                                                                  WaqProcessValidationRule rule, string reason)
        {
            string dependency = string.IsNullOrEmpty(rule.Dependency) ? string.Empty : $", when {rule.Dependency}.";
            double paramValue = double.NaN;
            parameter.GetParameterValue(out paramValue);
            string message = Resources
                .WaqValidationRulesExtension_GetWaqProcessValidationRuleAsString_Process_coefficient__0___value__1____2__3__;
            return string.Format(message, parameter.Name, paramValue, reason, dependency);
        }

        private static string GetRuleLimitMessage(this WaqProcessValidationRule rule)
        {
            double value = double.NaN;
            var minString = string.Empty;
            var maxString = string.Empty;
            if (double.TryParse(rule.MinValue, out value) && !double.IsInfinity(value))
            {
                minString = $"at least {rule.MinValue}";
            }

            if (double.TryParse(rule.MaxValue, out value) && !double.IsInfinity(value))
            {
                maxString = $"at most {rule.MaxValue}";
                if (!string.IsNullOrEmpty(minString))
                {
                    maxString = " and " + maxString;
                }
            }

            return $"should be {minString}{maxString}";
        }
    }
}