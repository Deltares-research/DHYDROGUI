using System;
using System.Linq;
using System.Xml.Linq;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils.Domain;
using NUnit.Framework;
using ValidationAspects;
using ValidationAspects.Exceptions;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Domain
{
    [TestFixture]
    public class IntervalRuleTests
    {
        private static readonly XNamespace Fns = "http://www.wldelft.nl/fews";

        private const string RuleName = "INTERVAL RULE";
        private const string parameterName = "parameter name";
        private const string inputFeatureName = "element name";
        private Setting setting;
        private Input input;
        private Output output;
        private static double SettingBelow = 0.0;
        private static double SettingAbove = 1.0;
        private static double SettingMaxSpeed = 0.1;
        private static double DeadbandAroundSetpoint = 0.4;
        private const string outputFeatureName = "output";
        
        [SetUp]
        public void SetUp()
        {
            setting = new Setting { Below = SettingBelow, Above = SettingAbove, MaxSpeed = SettingMaxSpeed};
            input = new Input
            {
                ParameterName = parameterName,
                Feature = new RtcTestFeature {Name = inputFeatureName},
                SetPoint = RtcXmlTag.SP + RuleName
            };
            
            output = new Output
            {
                ParameterName = parameterName,
                Feature = new RtcTestFeature { Name = outputFeatureName }
            };
        }

        [Test]
        public void CheckXmlGenerationIntervalTypeFixedDeadbandTypeAbsolute()
        {
            var intervalRule = CreateIntervalRule();
            intervalRule.IntervalType = IntervalRule.IntervalRuleIntervalType.Fixed;
            intervalRule.DeadBandType = IntervalRule.IntervalRuleDeadBandType.Fixed;
           Assert.AreEqual(OriginXmlIntervalTypeFixedDeadbandTypeAbsolute(), intervalRule.ToXml(Fns, "").ToString(SaveOptions.DisableFormatting));
        }

        [Test]
        public void CheckXmlGenerationIntervalTypeVariableDeadbandTypeRelative()
        {
            var intervalRule = CreateIntervalRule();
            intervalRule.IntervalType = IntervalRule.IntervalRuleIntervalType.Variable;
            intervalRule.DeadBandType = IntervalRule.IntervalRuleDeadBandType.PercentageDischarge;

            Assert.AreEqual(OriginXmlIntervalTypeVariableDeadbandTypeRelative(), intervalRule.ToXml(Fns, "").ToString(SaveOptions.DisableFormatting));
        }

        [Test]
        public void CheckXmlGenerationIntervalTypeVariableDeadbandTypeRelativeWithNegativeMaxVelocityShouldBeAbsoluteInXml()
        {
            setting.MaxSpeed = -setting.MaxSpeed;
            var intervalRule = CreateIntervalRule();
            intervalRule.IntervalType = IntervalRule.IntervalRuleIntervalType.Variable;
            intervalRule.DeadBandType = IntervalRule.IntervalRuleDeadBandType.PercentageDischarge;

            Assert.AreEqual(OriginXmlIntervalTypeVariableDeadbandTypeRelative(), intervalRule.ToXml(Fns, "").ToString(SaveOptions.DisableFormatting));
        }
        [Test]
        [Ignore("Waste of time to keep this up to date")]
        public void RuleFromXml()
        {
            var intervalRule = new IntervalRule {Name = RuleName};
            var origXmlAsElement = XElement.Parse(OriginXmlIntervalTypeFixedDeadbandTypeAbsolute());

            Assert.AreEqual(RuleName, intervalRule.Name);
            Assert.IsTrue(RealTimeControlTestHelper.CompareEqualityOfInput(input, intervalRule.Inputs.FirstOrDefault()));
        }

        private static string OriginXmlIntervalTypeFixedDeadbandTypeAbsolute()
        {
            return "<rule xmlns=\"http://www.wldelft.nl/fews\">" +
                "<interval id=\"[IntervalRule]INTERVAL RULE\">" +
                "<settingBelow>0</settingBelow>" +
                "<settingAbove>1</settingAbove>" +
                "<settingMaxStep>0.1</settingMaxStep>" +
                "<deadbandSetpointAbsolute>0.4</deadbandSetpointAbsolute>" +
                "<input>" +
                "<x>" + RtcXmlTag.Input+ "element name/parameter name</x>" +
                "<setpoint>" + RtcXmlTag.SP + "INTERVAL RULE</setpoint>" +
                "</input>" +
                "<output>" +
                "<y>" + RtcXmlTag.Output + "output/parameter name</y>" +
                "<status>[Status]INTERVAL RULE</status>" +
                "</output>" +
                "</interval>" +
                "</rule>";
        }

        private static string OriginXmlIntervalTypeVariableDeadbandTypeRelative()
        {
            return "<rule xmlns=\"http://www.wldelft.nl/fews\">" +
                "<interval id=\"[IntervalRule]INTERVAL RULE\">" +
                "<settingBelow>0</settingBelow>" +
                "<settingAbove>1</settingAbove>" +
                "<settingMaxSpeed>0.1</settingMaxSpeed>" +
                "<deadbandSetpointRelative>0.4</deadbandSetpointRelative>" +
                "<input>" +
                   "<x>" + RtcXmlTag.Input + "element name/parameter name</x>" +
                "<setpoint>" + RtcXmlTag.SP + "INTERVAL RULE</setpoint>" +
                "</input>" +
                "<output>" +
                "<y>" + RtcXmlTag.Output + "output/parameter name</y>" +
                "<status>[Status]INTERVAL RULE</status>" +
                "</output>" +
                "</interval>" +
                "</rule>";
        }

        [Test]
        public void CopyFromAndClone()
        {
            var source = CreateIntervalRule();
            source.IntervalType = IntervalRule.IntervalRuleIntervalType.Fixed;
            source.DeadBandType = IntervalRule.IntervalRuleDeadBandType.PercentageDischarge;
            
            var newRule = new IntervalRule();

            newRule.CopyFrom(source);

            Assert.AreEqual(source.Name, newRule.Name);
            Assert.AreEqual(source.InterpolationOptionsTime, newRule.InterpolationOptionsTime);
            Assert.AreEqual(source.DeadbandAroundSetpoint, newRule.DeadbandAroundSetpoint);
            for (int i = 0; i < source.TimeSeries.Arguments[0].Values.Count; i++)
            {
                Assert.AreEqual(source.TimeSeries.Arguments[0].Values[i], newRule.TimeSeries.Arguments[0].Values[i]);
                Assert.AreEqual(source.TimeSeries.Components[0].Values[i], newRule.TimeSeries.Components[0].Values[i]);
            }
            Assert.AreEqual(source.Setting, newRule.Setting);
            Assert.AreEqual(source.IntervalType, newRule.IntervalType);
            Assert.AreEqual(source.FixedInterval, newRule.FixedInterval);
            Assert.AreEqual(source.DeadBandType, newRule.DeadBandType);

            var clone = (IntervalRule)source.Clone();
            Assert.IsFalse(ReferenceEquals(source, clone));
            Assert.AreEqual(source.Name, clone.Name);
        }

        [Test]
        public void XmlTimeSeriesIntervalRuleWithConstantSetpoint()
        {
            var intervalRule = CreateIntervalRule();

            //Set constant value
            Assert.AreEqual(OriginXmlIntervalTypeFixedDeadbandTypeAbsolute(), intervalRule.ToXml(Fns, "").ToString(SaveOptions.DisableFormatting));

            var start = DateTime.Now;
            var stop = start.Add(new TimeSpan(3, 0, 0));
            var step = new TimeSpan(1, 0, 0);

            var constantValueTimeSeries = intervalRule.XmlImportTimeSeries("prefix", start, stop, step).FirstOrDefault();
            Assert.IsNotNull(constantValueTimeSeries);
            Assert.AreEqual(2, constantValueTimeSeries.TimeSeries.Time.Values.Count);
            Assert.AreEqual(1.23, constantValueTimeSeries.TimeSeries[constantValueTimeSeries.StartTime]);

            //Validate
            Assert.IsTrue(intervalRule.Validate().IsValid);
        }

        [TestCase(IntervalRule.IntervalRuleIntervalType.Fixed, 1.23)]
        [TestCase(IntervalRule.IntervalRuleIntervalType.Variable, 5.0)]
        public void GivenAnIntervalRuleWithAFixedOrVariableSetPoint_WhenCallingTheTimeSeries_ThenTheseShouldBeGenerated(
            IntervalRule.IntervalRuleIntervalType intervalRuleIntervalType, double expectedValue)
        {
            var intervalRule = CreateIntervalRule();
            intervalRule.IntervalType = intervalRuleIntervalType;

            var start = DateTime.Now;
            var stop = start.Add(new TimeSpan(3, 0, 0));
            var step = new TimeSpan(1, 0, 0);

            var constantValueTimeSeries =
                intervalRule.XmlImportTimeSeries("prefix", start, stop, step).FirstOrDefault();
            Assert.IsNotNull(constantValueTimeSeries);
            Assert.AreEqual(2, constantValueTimeSeries.TimeSeries.Time.Values.Count);
            Assert.AreEqual(expectedValue, constantValueTimeSeries.TimeSeries[constantValueTimeSeries.StartTime]);
            Assert.AreEqual(expectedValue, constantValueTimeSeries.TimeSeries[constantValueTimeSeries.EndTime]);
        }

        [Test]
        public void GivenAnIntervalRuleWithSignalAsSetPoint_WhenCallingTheTimeSeries_ThenAnExceptionShouldBeThrown()
        {
            var intervalRule = CreateIntervalRule();
            intervalRule.IntervalType = IntervalRule.IntervalRuleIntervalType.Signal;

            var start = DateTime.Now;
            var stop = start.Add(new TimeSpan(3, 0, 0));
            var step = new TimeSpan(1, 0, 0);
            
            Assert.Throws<InvalidOperationException>(() =>
            {
                intervalRule.XmlImportTimeSeries("prefix", start, stop, step).ToList();
            });

        }

        [Test]
        public void GivenAnIntervalRuleWithVariableAsSetPointAndNoTimeSeries_WhenCallingValidate_ThenAnExceptionShouldBeThrown()
        {
            var intervalRule = new IntervalRule
            {
                Inputs = new EventedList<Input> {input},
                IntervalType = IntervalRule.IntervalRuleIntervalType.Variable
            };

            var exception = Assert.Throws<ValidationContextException>(() =>
            {
                IntervalRule.Validate(intervalRule);
            });

            var counter = exception.Exceptions.Count();

            Assert.AreEqual(1, counter, $"{counter} exceptions are thrown instead of 1");
            Assert.AreEqual(string.Format(Resources.RealTimeControlModelIntervalRule_Interval_rule__0__has_empty_time_series, intervalRule.Name), exception.Message, "The exception message is different than expected");
        }

        [Test]
        public void GivenAnIntervalRuleWithNoInput_WhenCallingValidate_ThenAnExceptionShouldBeThrown()
        {
            var intervalRule = new IntervalRule
            {
               IntervalType = IntervalRule.IntervalRuleIntervalType.Fixed
            };
            
            var exception = Assert.Throws<ValidationContextException>(() =>
            {
                IntervalRule.Validate(intervalRule);
            });

            var counter = exception.Exceptions.Count();

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
                Inputs = new EventedList<Input> {input},
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

