using System;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.IO
{
    [TestFixture]
    public class WaqProcessValidationRuleTest
    {
        [Test]
        public void Test_WaqProcessValidationRule_IsNotNull()
        {
            var rule = new WaqProcessValidationRule();
            Assert.IsNotNull(rule);
        }

        [Test]
        public void Test_WaqProcessValidationRule_ProcessName_Can_Be_Set()
        {
            var ruleProcessName = "ProcessNameTest";
            var rule = new WaqProcessValidationRule() {ProcessName = ruleProcessName};
            Assert.IsNotNull(rule);
            Assert.AreEqual(ruleProcessName, rule.ProcessName);
        }

        [Test]
        public void Test_WaqProcessValidationRule_ParameterName_Can_Be_Set()
        {
            var ruleParameterName = "ParameterNameTest";
            var rule = new WaqProcessValidationRule() {ParameterName = ruleParameterName};
            Assert.IsNotNull(rule);
            Assert.AreEqual(ruleParameterName, rule.ParameterName);
        }

        [Test]
        public void Test_WaqProcessValidationRule_MinValue_Can_Be_Set()
        {
            var ruleMinValue = "MinValueTest";
            var rule = new WaqProcessValidationRule() {MinValue = ruleMinValue};
            Assert.IsNotNull(rule);
            Assert.AreEqual(ruleMinValue, rule.MinValue);
        }

        [Test]
        public void Test_WaqProcessValidationRule_MaxValue_Can_Be_Set()
        {
            var ruleMaxValue = "MaxValueTest";
            var rule = new WaqProcessValidationRule() {MaxValue = ruleMaxValue};
            Assert.IsNotNull(rule);
            Assert.AreEqual(ruleMaxValue, rule.MaxValue);
        }

        [Test]
        public void Test_WaqProcessValidationRule_Dependency_Can_Be_Set()
        {
            var ruleDependency = "DependencyTest";
            var rule = new WaqProcessValidationRule() {Dependency = ruleDependency};
            Assert.IsNotNull(rule);
            Assert.AreEqual(ruleDependency, rule.Dependency);
        }

        [Test]
        public void Test_WaqProcessValidationRule_Type_Can_Be_Set()
        {
            Type ruleValueType = typeof(double);
            var rule = new WaqProcessValidationRule() {ValueType = ruleValueType};
            Assert.IsNotNull(rule);
            Assert.AreEqual(ruleValueType, rule.ValueType);
        }
    }
}