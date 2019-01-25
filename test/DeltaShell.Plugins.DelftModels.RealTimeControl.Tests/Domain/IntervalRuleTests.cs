using System;
using System.Linq;
using System.Xml.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils.Domain;
using NUnit.Framework;
using Rhino.Mocks;
using ValidationAspects;

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

        MockRepository mockRepository = new MockRepository();
        private ITimeDependentModel timeDependentModelMock;

        [SetUp]
        public void SetUp()
        {
            setting = new Setting { Below = SettingBelow, Above = SettingAbove, MaxSpeed = SettingMaxSpeed};
            input = new Input
            {
                ParameterName = parameterName,
                Feature = new RtcTestFeature { Name = inputFeatureName }
            };
            output = new Output
            {
                ParameterName = parameterName,
                Feature = new RtcTestFeature { Name = outputFeatureName }
            };

            timeDependentModelMock = mockRepository.Stub<ITimeDependentModel>();
        }

        [Test]
        public void CheckXmlGenerationIntervalTypeFixedDeadbandTypeAbsolute()
        {
            var intervalRule = new IntervalRule
                                   {
                                       Name = RuleName,
                                       Setting = setting,
                                       Inputs = new EventedList<Input> { input },
                                       Outputs = new EventedList<Output> { output },
                                       DeadbandAroundSetpoint = 0.4,
                                       IntervalType = IntervalRule.IntervalRuleIntervalType.Fixed,
                                       FixedInterval = 0.1,
                                       DeadBandType = IntervalRule.IntervalRuleDeadBandType.Fixed
                                   };
            Assert.AreEqual(OriginXmlIntervalTypeFixedDeadbandTypeAbsolute(), intervalRule.ToXml(Fns, "").ToString(SaveOptions.DisableFormatting));
        }

        [Test]
        public void CheckXmlGenerationIntervalTypeVariableDeadbandTypeRelative()
        {
            var intervalRule = new IntervalRule
            {
                Name = RuleName,
                Setting = setting,
                Inputs = new EventedList<Input> { input },
                Outputs = new EventedList<Output> { output },
                DeadbandAroundSetpoint = 0.4,
                IntervalType = IntervalRule.IntervalRuleIntervalType.Variable,
                FixedInterval = 0.1,
                DeadBandType = IntervalRule.IntervalRuleDeadBandType.PercentageDischarge,
            };
            Assert.AreEqual(OriginXmlIntervalTypeVariableDeadbandTypeRelative(), intervalRule.ToXml(Fns, "").ToString(SaveOptions.DisableFormatting));
        }

        [Test]
        public void CheckXmlGenerationIntervalTypeVariableDeadbandTypeRelativeWithNegativeMaxVelocityShouldBeAbsoluteInXml()
        {
            setting.MaxSpeed = -setting.MaxSpeed;
            Assert.Less(setting.MaxSpeed,0);

            var intervalRule = new IntervalRule
            {
                Name = RuleName,
                Setting = setting,
                Inputs = new EventedList<Input> { input },
                Outputs = new EventedList<Output> { output },
                DeadbandAroundSetpoint = 0.4,
                IntervalType = IntervalRule.IntervalRuleIntervalType.Variable,
                FixedInterval = 0.1,
                DeadBandType = IntervalRule.IntervalRuleDeadBandType.PercentageDischarge,
            };
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
                "<setpoint>[SP]INTERVAL RULE</setpoint>" +
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
                "<setpoint>[SP]INTERVAL RULE</setpoint>" +
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
            var source = new IntervalRule()
            {
                Name = "test",
                InterpolationOptionsTime = InterpolationType.Linear,
                DeadbandAroundSetpoint = 1.1,
                Setting = new Setting(),
                IntervalType = IntervalRule.IntervalRuleIntervalType.Variable,
                FixedInterval = 1.23,
                DeadBandType = IntervalRule.IntervalRuleDeadBandType.PercentageDischarge
            };
            var timeSeries = new TimeSeries();
            timeSeries.Arguments[0].DefaultValue = new DateTime(2000, 1, 1);
            timeSeries.Components.Add(new Variable<double>("someThing"));
            var time = DateTime.Now;
            timeSeries[time] = 1.0;
            source.TimeSeries = timeSeries;

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
            var defaultValue = 1.23;
            var intervalRule = new IntervalRule
                                   {
                                       Name = RuleName,
                                       Setting = setting,
                                       Inputs = new EventedList<Input> {input},
                                       Outputs = new EventedList<Output> {output},
                                       DeadbandAroundSetpoint = DeadbandAroundSetpoint,
                                       FixedInterval = 0.1
            };

            //Set constant value
            intervalRule.TimeSeries.Components[0].DefaultValue = defaultValue;
            Assert.AreEqual(OriginXmlIntervalTypeFixedDeadbandTypeAbsolute(), intervalRule.ToXml(Fns, "").ToString(SaveOptions.DisableFormatting));

            var start = DateTime.Now;
            var stop = start.Add(new TimeSpan(3, 0, 0));
            var step = new TimeSpan(1, 0, 0);

            var constantValueTimeSeries = intervalRule.XmlImportTimeSeries("prefix", start, stop, step).FirstOrDefault();
            Assert.IsNotNull(constantValueTimeSeries);
            Assert.AreEqual(2, constantValueTimeSeries.TimeSeries.Time.Values.Count);
            Assert.AreEqual(defaultValue, constantValueTimeSeries.TimeSeries[constantValueTimeSeries.StartTime]);

            //Validate
            Assert.IsTrue(intervalRule.Validate().IsValid);
        }
    }

}
