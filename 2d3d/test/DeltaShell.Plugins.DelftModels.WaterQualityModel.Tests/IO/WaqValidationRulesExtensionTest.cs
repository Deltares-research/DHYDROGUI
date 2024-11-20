using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Properties;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.IO
{
    [TestFixture]
    public class WaqValidationRulesExtensionTest
    {
        [Test]
        public void Test_ConstantProcessWithinRuleLimits_Passes_When_No_Rules_Given()
        {
            var parameterName = "testParameter";
            IFunction parameter =
                WaterQualityFunctionFactory.CreateConst(parameterName, 0.0, string.Empty, string.Empty, string.Empty,
                                                        string.Empty);
            Assert.IsNotNull(parameter);

            var reasonList = new List<string>();
            bool result = parameter.IsWithinRulesLimits(null, null, out reasonList);
            Assert.IsTrue(result);
            string expectedReason = string.Format(Resources.WaqValidationRulesExtension_ConstantProcessWithinRuleLimits_No_rules_found_for__0__, parameter.Name);
            Assert.IsTrue(reasonList.Contains(expectedReason));
        }

        [Test]
        public void Test_ConstantProcessWithinRuleLimits_Passes_When_No_Rules_Found()
        {
            var parameterName = "testParameter";
            IFunction parameter =
                WaterQualityFunctionFactory.CreateConst(parameterName, 0.0, string.Empty, string.Empty, string.Empty,
                                                        string.Empty);
            Assert.IsNotNull(parameter);

            var reasonList = new List<string>();
            bool result = parameter.IsWithinRulesLimits(new List<WaqProcessValidationRule>(), null, out reasonList);
            Assert.IsTrue(result);
            string expectedReason = string.Format(Resources.WaqValidationRulesExtension_ConstantProcessWithinRuleLimits_No_rules_found_for__0__, parameter.Name);
            Assert.IsTrue(reasonList.Contains(expectedReason));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void Test_HasParameterDependency_ReturnsFalse_IfStringIsNullOrEmpty(string dependency)
        {
            var rule = new WaqProcessValidationRule()
            {
                MaxValue = "2",
                MinValue = "5",
                ParameterName = "parameterName",
                ValueType = typeof(double),
                Dependency = dependency
            };

            Assert.IsFalse((bool) TypeUtils.CallPrivateStaticMethod(typeof(WaqValidationRulesExtension), "HasParameterDependency", rule, null));
        }

        [Test]
        public void Test_HasParameterDependency_ReturnsFalse_IfDependencyDoesNotExist()
        {
            var rule = new WaqProcessValidationRule()
            {
                MaxValue = "2",
                MinValue = "5",
                ParameterName = "parameterName",
                ValueType = typeof(double),
                Dependency = "ValidDependencyLine = 1"
            };
            //Parameter list does not exist, so it cannot be found.
            Assert.IsFalse((bool) TypeUtils.CallPrivateStaticMethod(typeof(WaqValidationRulesExtension), "HasParameterDependency", rule, null));
        }

        [Test]
        [TestCase(0, false)]
        [TestCase(2, true)]
        [TestCase(4, true)]
        [TestCase(5, true)]
        [TestCase(6, false)]
        public void Test_ConstantProcessWithinRuleLimits_Passes_When_No_Dependency_AndWithinLimits(double value, bool expectedResult)
        {
            var parameterName = "testParameter";
            IFunction parameter =
                WaterQualityFunctionFactory.CreateConst(parameterName, value, string.Empty, string.Empty, string.Empty,
                                                        string.Empty);
            Assert.IsNotNull(parameter);

            var reasonList = new List<string>();
            //Define a basic rule.
            var maxValue = 5;
            var minValue = 2;
            var rule = new WaqProcessValidationRule()
            {
                MaxValue = maxValue.ToString(),
                MinValue = minValue.ToString(),
                ParameterName = parameterName,
                ValueType = typeof(double)
            };

            //Verify first the limits are satisfied
            if (expectedResult)
            {
                Assert.IsTrue(minValue <= value);
                Assert.IsTrue(value <= maxValue);
            }

            var waqProcessValidationRules = new List<WaqProcessValidationRule>();
            waqProcessValidationRules.Add(rule);
            //Check the rule with the parameter. 
            bool result = parameter.IsWithinRulesLimits(waqProcessValidationRules, null, out reasonList);
            Assert.AreEqual(expectedResult, result);

            if (!expectedResult)
            {
                string expectedReason = GetWaqProcessValidationRuleAsString(parameterName, value);
                Assert.IsTrue(reasonList.Any(r => r.Contains(expectedReason)));
            }
        }

        [Test]
        [TestCase(3, typeof(int), true)]
        [TestCase(3.5, typeof(int), false)]
        [TestCase(3, typeof(double), true)]
        [TestCase(3.5, typeof(double), true)]
        public void Test_ConstantProcessWithinRuleLimits_Fails_When_TheType_IsNot_Correct(double value, Type valueType, bool expectedResult)
        {
            var parameterName = "testParameter";
            IFunction parameter =
                WaterQualityFunctionFactory.CreateConst(parameterName, value, string.Empty, string.Empty, string.Empty,
                                                        string.Empty);
            Assert.IsNotNull(parameter);

            var reasonList = new List<string>();
            //Define a basic rule.
            var maxValue = 5;
            var minValue = 0;
            var rule = new WaqProcessValidationRule()
            {
                MaxValue = maxValue.ToString(),
                MinValue = minValue.ToString(),
                ParameterName = parameterName,
                ValueType = valueType
            };

            //Verify first the limits are satisfied
            Assert.IsTrue(minValue <= value);
            Assert.IsTrue(value <= maxValue);

            var waqProcessValidationRules = new List<WaqProcessValidationRule>();
            waqProcessValidationRules.Add(rule);

            //Check the rule with the parameter. 
            bool result = parameter.IsWithinRulesLimits(waqProcessValidationRules, null, out reasonList);
            Assert.AreEqual(expectedResult, result);

            if (!expectedResult)
            {
                string expectedReason = GetWaqProcessValidationRuleAsString(parameterName, value);
                Assert.IsTrue(reasonList.Any(r => r.Contains(expectedReason)));
            }
        }

        [Test]
        public void Test_WithinRuleLimits_If_No_Dependencies_MultipleRules_CanApply()
        {
            var parameterName = "testParameter";
            var value = 3.5;
            IFunction parameter =
                WaterQualityFunctionFactory.CreateConst(parameterName, value, string.Empty, string.Empty, string.Empty,
                                                        string.Empty);
            Assert.IsNotNull(parameter);

            var reasonList = new List<string>();
            //Define a basic rule.
            var rule1MaxValue = 4;
            var rule1MinValue = 0;
            var ruleMustBeInt = new WaqProcessValidationRule()
            {
                MaxValue = rule1MaxValue.ToString(),
                MinValue = rule1MinValue.ToString(),
                ParameterName = parameterName,
                ValueType = typeof(int)
            };

            //Verify first the rule limits are satisfied, but the type is not.
            Assert.IsTrue(rule1MinValue <= value);
            Assert.IsTrue(value <= rule1MaxValue);
            Assert.AreNotEqual(value.GetType(), ruleMustBeInt.ValueType);

            var rule2MaxValue = 8;
            var rule2MinValue = 5;
            var ruleMustBeGreaterThan5 = new WaqProcessValidationRule()
            {
                MaxValue = rule2MaxValue.ToString(),
                MinValue = rule2MinValue.ToString(),
                ParameterName = parameterName,
                ValueType = typeof(double)
            };

            //Verify first the rule limits are not satisfied, but the type is.
            Assert.IsFalse(rule2MinValue <= value);
            Assert.AreEqual(value.GetType(), ruleMustBeGreaterThan5.ValueType);

            //Add the rules.
            var waqProcessValidationRules = new List<WaqProcessValidationRule>();
            waqProcessValidationRules.Add(ruleMustBeInt);
            waqProcessValidationRules.Add(ruleMustBeGreaterThan5);

            //Check the rule with the parameter. 
            bool result = parameter.IsWithinRulesLimits(waqProcessValidationRules, null, out reasonList);
            Assert.IsFalse(result);

            string expectedReason = GetWaqProcessValidationRuleAsString(parameterName, value);
            Assert.IsTrue(reasonList.Count == 2);
            Assert.IsTrue(reasonList.Any(r => r.Contains(expectedReason)));
        }

        [Test]
        public void Test_WithinRuleLimits_DependencyRule_DoesNot_Apply_If_Is_Not_Met()
        {
            var parameterName = "testParameter";
            var dependencyName = "dependencyName";
            var value = 3;
            IFunction parameter =
                WaterQualityFunctionFactory.CreateConst(parameterName, value, string.Empty, string.Empty, string.Empty,
                                                        string.Empty);
            Assert.IsNotNull(parameter);

            var reasonList = new List<string>();
            //Define a basic rule.
            var maxValue = 8;
            var minValue = 5;
            var rule = new WaqProcessValidationRule()
            {
                MaxValue = maxValue.ToString(),
                MinValue = minValue.ToString(),
                ParameterName = parameterName,
                ValueType = typeof(double),
                Dependency = dependencyName
            };

            //Verify our parameter does not met the rule requirements:
            Assert.IsFalse(minValue <= value);

            var waqProcessValidationRules = new List<WaqProcessValidationRule>();
            waqProcessValidationRules.Add(rule);

            //Check the rule with the parameter and the dependency, it will pass because the dependency won´t be found. 
            bool result = parameter.IsWithinRulesLimits(waqProcessValidationRules, null, out reasonList);
            Assert.IsTrue(result);
            Assert.IsFalse(reasonList.Any());
        }

        [Test]
        [TestCase("dependencyName = 3", 3.0, false)]
        [TestCase("dependencyName = 3", 1.0, true)]             /*rule does not apply because dependency has a value of 1*/
        [TestCase("dependencyName = notValidValue", 1.0, true)] /*rule does not apply because dependency has a value of 1*/
        public void Test_WithinRuleLimits_DependencyRule_Applies_If_Is_Met(string dependencyRule, double dependencyValue, bool expectedResult)
        {
            var parameterName = "testParameter";
            var dependencyName = "dependencyName";
            var value = 3;
            IFunction parameter =
                WaterQualityFunctionFactory.CreateConst(parameterName, value, string.Empty, string.Empty, string.Empty,
                                                        string.Empty);
            Assert.IsNotNull(parameter);

            IFunction dependencyParam =
                WaterQualityFunctionFactory.CreateConst(dependencyName, dependencyValue, string.Empty, string.Empty, string.Empty,
                                                        string.Empty);
            Assert.IsNotNull(dependencyParam);
            Assert.IsTrue(dependencyRule.Contains(dependencyName));

            var reasonList = new List<string>();
            //Define a basic rule.
            var maxValue = 8;
            var minValue = 5;
            var rule = new WaqProcessValidationRule()
            {
                MaxValue = maxValue.ToString(),
                MinValue = minValue.ToString(),
                ParameterName = parameterName,
                ValueType = typeof(double),
                Dependency = dependencyRule
            };

            //Verify our parameter does not met the rule requirements:
            Assert.IsFalse(minValue <= value);

            var waqProcessValidationRules = new List<WaqProcessValidationRule>();
            waqProcessValidationRules.Add(rule);

            //Check the rule with the parameter and the dependency, it will pass because the dependency won´t be found. 
            bool result = parameter.IsWithinRulesLimits(waqProcessValidationRules, new List<IFunction> {dependencyParam}, out reasonList);
            Assert.AreEqual(expectedResult, result);
            Assert.AreEqual(!expectedResult, reasonList.Any());
        }

        [Test]
        public void Test_WithinRuleLimits_Passes_With_MultipleRules_If_DependencyRule_Is_Met_Succesfully()
        {
            var parameterName = "testParameter";
            var dependencyName = "dependencyName";
            var value = 3;
            IFunction parameter =
                WaterQualityFunctionFactory.CreateConst(parameterName, value, string.Empty, string.Empty, string.Empty,
                                                        string.Empty);
            Assert.IsNotNull(parameter);

            var dependencyValue = 1;
            IFunction dependencyParam =
                WaterQualityFunctionFactory.CreateConst(dependencyName, dependencyValue, string.Empty, string.Empty, string.Empty,
                                                        string.Empty);
            Assert.IsNotNull(dependencyParam);

            var reasonList = new List<string>();
            //Define a basic rule.
            double maxValue1 = double.PositiveInfinity;
            double minValue1 = double.NegativeInfinity;
            var rule1 = new WaqProcessValidationRule()
            {
                MaxValue = maxValue1.ToString(),
                MinValue = minValue1.ToString(),
                ParameterName = parameterName,
                ValueType = typeof(double),
                Dependency = $"{dependencyName} = {dependencyValue}"
            };

            double maxValue2 = double.PositiveInfinity;
            double minValue2 = double.PositiveInfinity;
            var rule2 = new WaqProcessValidationRule()
            {
                MaxValue = maxValue2.ToString(),
                MinValue = minValue2.ToString(),
                ParameterName = parameterName,
                ValueType = typeof(double),
                Dependency = $"{dependencyName} = {dependencyValue}"
            };

            //Verify our parameter DOES NOT meet these rule requirements:
            Assert.IsFalse(minValue2 <= value);

            var waqProcessValidationRules = new List<WaqProcessValidationRule>();
            waqProcessValidationRules.Add(rule1);
            waqProcessValidationRules.Add(rule2);

            //Check the rule with the parameter and the dependency, it will pass because the dependency won´t be found. 
            bool result = parameter.IsWithinRulesLimits(waqProcessValidationRules, new List<IFunction> {dependencyParam}, out reasonList);
            Assert.IsTrue(result);
            Assert.IsFalse(reasonList.Any());
        }

        [Test]
        public void Test_WithinRuleLimits_DependencyRule_DoesNotApply_If_DependencyParam_Contains_No_Values()
        {
            var parameterName = "testParameter";
            var dependencyName = "dependencyName";
            var value = 3;
            IFunction parameter =
                WaterQualityFunctionFactory.CreateConst(parameterName, value, string.Empty, string.Empty, string.Empty,
                                                        string.Empty);
            Assert.IsNotNull(parameter);

            var dependencyParam = new Function(dependencyName);

            var reasonList = new List<string>();
            //Define a basic rule.
            var maxValue = 8;
            var minValue = 5;
            var rule = new WaqProcessValidationRule()
            {
                MaxValue = maxValue.ToString(),
                MinValue = minValue.ToString(),
                ParameterName = parameterName,
                ValueType = typeof(double),
                Dependency = $"{dependencyName} = 3.0"
            };

            //Verify our parameter does not met the rule requirements:
            Assert.IsFalse(minValue <= value);

            var waqProcessValidationRules = new List<WaqProcessValidationRule>();
            waqProcessValidationRules.Add(rule);

            //Rule will be ignored because the dependency was not met.
            bool result = parameter.IsWithinRulesLimits(waqProcessValidationRules, new List<IFunction> {dependencyParam}, out reasonList);
            Assert.IsTrue(result);
            Assert.IsFalse(reasonList.Any());
        }

        [Test]
        public void Test_WithinRuleLimits_Only_OneRule_Applies_If_Dependency_Is_Met()
        {
            var parameterName = "testParameter";
            var dependencyName = "dependencyName";
            var value = 3.5;
            IFunction parameter =
                WaterQualityFunctionFactory.CreateConst(parameterName, value, string.Empty, string.Empty, string.Empty,
                                                        string.Empty);
            Assert.IsNotNull(parameter);

            IFunction dependencyParam =
                WaterQualityFunctionFactory.CreateConst(dependencyName, 3.0, string.Empty, string.Empty, string.Empty,
                                                        string.Empty);
            Assert.IsNotNull(dependencyParam);

            var reasonList = new List<string>();
            //Define a basic rule.
            var maxValue1 = 8;
            var minValue1 = 5;
            var rule1 = new WaqProcessValidationRule()
            {
                MaxValue = maxValue1.ToString(),
                MinValue = minValue1.ToString(),
                ParameterName = parameterName,
                ValueType = typeof(double),
                Dependency = "dependencyName = 3"
            };
            //Verify our parameter does not met the rule requirements:
            Assert.IsFalse(minValue1 <= value);
            Assert.AreEqual(rule1.ValueType, value.GetType());

            var maxValue2 = 4;
            var minValue2 = 3;
            var rule2 = new WaqProcessValidationRule()
            {
                MaxValue = maxValue2.ToString(),
                MinValue = minValue2.ToString(),
                ParameterName = parameterName,
                ValueType = typeof(int),
                Dependency = "dependencyName = 3"
            };
            Assert.IsTrue(minValue2 <= value);
            Assert.IsTrue(value <= maxValue2);
            Assert.AreNotEqual(rule2.ValueType, value.GetType());

            var waqProcessValidationRules = new List<WaqProcessValidationRule>();
            waqProcessValidationRules.Add(rule1);
            waqProcessValidationRules.Add(rule2);

            //Check the rule with the parameter and the dependency, it will pass because the dependency won´t be found. 
            bool result = parameter.IsWithinRulesLimits(waqProcessValidationRules, new List<IFunction> {dependencyParam}, out reasonList);
            Assert.IsFalse(result);
            Assert.IsTrue(reasonList.Count == 1);
        }

        [Test]
        public void Test_WithinRuleLimits_Fails_If_Value_IsNan()
        {
            var parameterName = "testParameter";
            double value = double.NaN;
            IFunction parameter =
                WaterQualityFunctionFactory.CreateConst(parameterName, value, string.Empty, string.Empty, string.Empty,
                                                        string.Empty);
            Assert.IsNotNull(parameter);

            var reasonList = new List<string>();
            //Define a basic rule.
            var maxValue = 8;
            var minValue = 0;
            var rule = new WaqProcessValidationRule()
            {
                MaxValue = maxValue.ToString(),
                MinValue = minValue.ToString(),
                ParameterName = parameterName,
                ValueType = typeof(double)
            };

            Assert.AreEqual(rule.ValueType, value.GetType());

            var waqProcessValidationRules = new List<WaqProcessValidationRule>();
            waqProcessValidationRules.Add(rule);

            //Check the rule with the parameter and the dependency, it will pass because the dependency won´t be found. 
            bool result = parameter.IsWithinRulesLimits(waqProcessValidationRules, new List<IFunction>(), out reasonList);
            Assert.IsFalse(result);
            Assert.IsTrue(reasonList.Count == 1);
        }

        [Test]
        [TestCase("[1:3]", "[7:9]", 4, false)]
        [TestCase("[1:3]", "[7:9]", 3, true)]
        [TestCase("[1:3]", "[7:9]", 8, true)]
        [TestCase("[1:3]", "[7:9]", 10, false)]
        [TestCase("[1:3]", "[7:9]", 0, false)]
        [TestCase("3", "[7:9]", 3, false)] //The range has preference.
        [TestCase("3", "[7:9]", 8, true)]
        [TestCase("3", "[7:9:11]", 8, false)] //Limit is not correct
        [TestCase("3", "[7:]", 8, false)]     //Limit is not correct
        public void Test_WithinRuleLimits_Checks_For_Range_Rules(string ruleMin, string ruleMax, double paramValue,
                                                                 bool expectedResult)
        {
            var parameterName = "testParameter";
            IFunction parameter =
                WaterQualityFunctionFactory.CreateConst(parameterName, paramValue, string.Empty, string.Empty,
                                                        string.Empty,
                                                        string.Empty);
            Assert.IsNotNull(parameter);

            var reasonList = new List<string>();
            //Define a basic rule.
            var rule = new WaqProcessValidationRule()
            {
                MaxValue = ruleMax,
                MinValue = ruleMin,
                ParameterName = parameterName,
                ValueType = typeof(double)
            };

            var waqProcessValidationRules = new List<WaqProcessValidationRule>();
            waqProcessValidationRules.Add(rule);

            //Check the rule with the parameter. 
            bool result = parameter.IsWithinRulesLimits(waqProcessValidationRules, null, out reasonList);
            Assert.AreEqual(expectedResult, result);

            if (!expectedResult)
            {
                string expectedReason = GetWaqProcessValidationRuleAsString(parameterName, paramValue);
                Assert.IsTrue(reasonList.Any(r => r.Contains(expectedReason)));
            }
        }

        [Test]
        [TestCase("inf", "5", -4, true)]
        [TestCase("inf", "5", 6, false)]
        [TestCase("0", "inf", 99999, true)]
        [TestCase("0", "inf", -4, false)]
        public void Test_WithinRuleLimits_Considers_Infinity_Rules(string ruleMin, string ruleMax, double paramValue,
                                                                   bool expectedResult)
        {
            var parameterName = "testParameter";
            IFunction parameter =
                WaterQualityFunctionFactory.CreateConst(parameterName, paramValue, string.Empty, string.Empty,
                                                        string.Empty,
                                                        string.Empty);
            Assert.IsNotNull(parameter);

            var reasonList = new List<string>();
            //Define a basic rule.
            var rule = new WaqProcessValidationRule()
            {
                MaxValue = ruleMax,
                MinValue = ruleMin,
                ParameterName = parameterName,
                ValueType = typeof(double)
            };

            var waqProcessValidationRules = new List<WaqProcessValidationRule>();
            waqProcessValidationRules.Add(rule);

            //Check the rule with the parameter. 
            bool result = parameter.IsWithinRulesLimits(waqProcessValidationRules, null, out reasonList);
            Assert.AreEqual(expectedResult, result);

            if (!expectedResult)
            {
                string expectedReason = GetWaqProcessValidationRuleAsString(parameterName, paramValue);
                Assert.IsTrue(reasonList.Any(r => r.Contains(expectedReason)));
            }
        }

        private static string GetWaqProcessValidationRuleAsString(string paramName, double value)
        {
            string message = Resources.WaqValidationRulesExtension_GetWaqProcessValidationRuleAsString_Process_coefficient__0___value__1____2__3__;
            message = message.Replace(".", string.Empty); //small trick.
            string expectedMssg = string.Format(message, paramName, value, string.Empty, string.Empty);
            return expectedMssg;
        }
    }
}