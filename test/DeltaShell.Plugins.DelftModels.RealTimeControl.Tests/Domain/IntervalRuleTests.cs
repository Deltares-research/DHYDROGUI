using System;
using System.Linq;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils.Domain;
using NUnit.Framework;
using ValidationAspects.Exceptions;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Domain
{
    [TestFixture]
    public class IntervalRuleTests
    {
        private const string RuleName = "INTERVAL RULE";
        private const string parameterName = "parameter name";
        private const string inputFeatureName = "element name";
        private const string outputFeatureName = "output";
        private Setting setting;
        private Input input;
        private Output output;
        private static double SettingBelow = 0.0;
        private static double SettingAbove = 1.0;
        private static double SettingMaxSpeed = 0.1;
        private static double DeadbandAroundSetpoint = 0.4;

        [SetUp]
        public void SetUp()
        {
            setting = new Setting
            {
                Below = SettingBelow,
                Above = SettingAbove,
                MaxSpeed = SettingMaxSpeed
            };
            input = new Input
            {
                ParameterName = parameterName,
                Feature = new RtcTestFeature {Name = inputFeatureName},
                SetPoint = RtcXmlTag.SP + RuleName
            };

            output = new Output
            {
                ParameterName = parameterName,
                Feature = new RtcTestFeature {Name = outputFeatureName}
            };
        }

        [Test]
        public void CopyFromAndClone()
        {
            IntervalRule source = CreateIntervalRule();
            source.IntervalType = IntervalRule.IntervalRuleIntervalType.Fixed;
            source.DeadBandType = IntervalRule.IntervalRuleDeadBandType.PercentageDischarge;

            var newRule = new IntervalRule();

            newRule.CopyFrom(source);

            Assert.AreEqual(source.Name, newRule.Name);
            Assert.AreEqual(source.InterpolationOptionsTime, newRule.InterpolationOptionsTime);
            Assert.AreEqual(source.DeadbandAroundSetpoint, newRule.DeadbandAroundSetpoint);
            for (var i = 0; i < source.TimeSeries.Arguments[0].Values.Count; i++)
            {
                Assert.AreEqual(source.TimeSeries.Arguments[0].Values[i], newRule.TimeSeries.Arguments[0].Values[i]);
                Assert.AreEqual(source.TimeSeries.Components[0].Values[i], newRule.TimeSeries.Components[0].Values[i]);
            }

            Assert.AreEqual(source.Setting, newRule.Setting);
            Assert.AreEqual(source.IntervalType, newRule.IntervalType);
            Assert.AreEqual(source.FixedInterval, newRule.FixedInterval);
            Assert.AreEqual(source.DeadBandType, newRule.DeadBandType);

            var clone = (IntervalRule) source.Clone();
            Assert.IsFalse(ReferenceEquals(source, clone));
            Assert.AreEqual(source.Name, clone.Name);
        }

        [Test]
        public void GivenAnIntervalRuleWithVariableAsSetPointAndNoTimeSeries_WhenCallingValidate_ThenAnExceptionShouldBeThrown()
        {
            var intervalRule = new IntervalRule
            {
                Inputs = new EventedList<IInput> {input},
                SetPointType = IntervalRule.IntervalRuleSetPointType.Variable
            };

            var exception = Assert.Throws<ValidationContextException>(() => { IntervalRule.Validate(intervalRule); });

            int counter = exception.Exceptions.Count();

            Assert.AreEqual(1, counter, $"{counter} exceptions are thrown instead of 1");
            Assert.AreEqual(string.Format(Resources.RealTimeControlModelIntervalRule_Interval_rule__0__has_empty_time_series, intervalRule.Name), exception.Message, "The exception message is different than expected");
        }

        [Test]
        public void GivenAnIntervalRuleWithNoInput_WhenCallingValidate_ThenAnExceptionShouldBeThrown()
        {
            var intervalRule = new IntervalRule {IntervalType = IntervalRule.IntervalRuleIntervalType.Fixed};

            var exception = Assert.Throws<ValidationContextException>(() => { IntervalRule.Validate(intervalRule); });

            int counter = exception.Exceptions.Count();

            Assert.AreEqual(1, counter, $"{counter} exceptions are thrown instead of 1");
            Assert.AreEqual(string.Format(Resources.RealTimeControlModelIntervalRule_Interval_rule__0__requires_1_input, intervalRule.Name), exception.Message, "The exception message is different than expected");
        }

        private IntervalRule CreateIntervalRule()
        {
            var defaultValue = 1.23;
            var intervalRule = new IntervalRule
            {
                Name = RuleName,
                Setting = setting,
                Inputs = new EventedList<IInput> {input},
                Outputs = new EventedList<Output> {output},
                DeadbandAroundSetpoint = DeadbandAroundSetpoint,
                FixedInterval = 0.1,
                TimeSeries =
                {
                    [new DateTime(2010, 1, 19, 12, 0, 0)] = 3.0,
                    [new DateTime(2010, 1, 20, 12, 0, 0)] = 4.0,
                    [new DateTime(2010, 1, 21, 12, 0, 0)] = 5.0,
                    Components = {[0] = {DefaultValue = defaultValue}}
                }
            };
            return intervalRule;
        }
    }
}