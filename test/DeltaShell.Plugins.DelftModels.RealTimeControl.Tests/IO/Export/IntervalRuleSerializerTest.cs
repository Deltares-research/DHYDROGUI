using System;
using System.Linq;
using System.Xml.Linq;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Export;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Xml;
using NUnit.Framework;
using ValidationAspects;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.IO.Export
{
    [TestFixture]
    public class IntervalRuleSerializerTest
    {
        private const string ruleName = "INTERVAL RULE";
        private const string parameterName = "parameter name";
        private const string inputFeatureName = "element name";
        private const double settingBelow = 0.0;
        private const double settingAbove = 1.0;
        private const double settingMaxSpeed = 0.1;
        private const double deadbandAroundSetpoint = 0.4;
        private const string outputFeatureName = "output";
        private static readonly XNamespace fns = "http://www.wldelft.nl/fews";
        private Setting setting;
        private Input input;
        private Output output;

        [SetUp]
        public void SetUp()
        {
            setting = new Setting
            {
                Below = settingBelow,
                Above = settingAbove,
                MaxSpeed = settingMaxSpeed
            };
            input = new Input
            {
                ParameterName = parameterName,
                Feature = new RtcTestFeature {Name = inputFeatureName},
                SetPoint = RtcXmlTag.SP + ruleName
            };
            output = new Output
            {
                ParameterName = parameterName,
                Feature = new RtcTestFeature {Name = outputFeatureName}
            };
        }

        [Test]
        public void XmlTimeSeriesIntervalRuleWithConstantSetpoint()
        {
            IntervalRule intervalRule = CreateIntervalRule();

            var serializer = new IntervalRuleSerializer(intervalRule);

            //Set constant value
            Assert.AreEqual(OriginXmlIntervalTypeFixedDeadbandTypeAbsolute(),
                            serializer.ToXml(fns, "").Single().ToString(SaveOptions.DisableFormatting));

            DateTime start = DateTime.Now;
            DateTime stop = start.Add(new TimeSpan(3, 0, 0));
            var step = new TimeSpan(1, 0, 0);

            IXmlTimeSeries constantValueTimeSeries =
                serializer.XmlImportTimeSeries("prefix", start, stop, step).FirstOrDefault();
            Assert.IsNotNull(constantValueTimeSeries);
            Assert.AreEqual(2, constantValueTimeSeries.TimeSeries.Time.Values.Count);
            Assert.AreEqual(1.23, constantValueTimeSeries.TimeSeries[constantValueTimeSeries.StartTime]);

            //Validate
            Assert.IsTrue(intervalRule.Validate().IsValid);
        }

        [Test]
        [Category("Quarantine")]
        public void GivenAnIntervalRuleWithSignalAsSetPoint_WhenCallingTheTimeSeries_ThenAnExceptionShouldBeThrown()
        {
            IntervalRule intervalRule = CreateIntervalRule();
            intervalRule.IntervalType = IntervalRule.IntervalRuleIntervalType.Signal;

            DateTime start = DateTime.Now;
            DateTime stop = start.Add(new TimeSpan(3, 0, 0));
            var step = new TimeSpan(1, 0, 0);

            var serializer = new IntervalRuleSerializer(intervalRule);

            Assert.Throws<InvalidOperationException>(() => serializer.XmlImportTimeSeries("prefix", start, stop, step)
                                                                     .ToArray());
        }

        [Test]
        public void CheckXmlGenerationIntervalTypeFixedDeadbandTypeAbsolute()
        {
            IntervalRule intervalRule = CreateIntervalRule();
            intervalRule.IntervalType = IntervalRule.IntervalRuleIntervalType.Fixed;
            intervalRule.DeadBandType = IntervalRule.IntervalRuleDeadBandType.Fixed;

            var serializer = new IntervalRuleSerializer(intervalRule);

            Assert.AreEqual(OriginXmlIntervalTypeFixedDeadbandTypeAbsolute(),
                            serializer.ToXml(fns, "").Single().ToString(SaveOptions.DisableFormatting));
        }

        [Test]
        public void CheckXmlGenerationIntervalTypeVariableDeadbandTypeRelative()
        {
            IntervalRule intervalRule = CreateIntervalRule();
            intervalRule.IntervalType = IntervalRule.IntervalRuleIntervalType.Variable;
            intervalRule.DeadBandType = IntervalRule.IntervalRuleDeadBandType.PercentageDischarge;

            var serializer = new IntervalRuleSerializer(intervalRule);

            Assert.AreEqual(OriginXmlIntervalTypeVariableDeadbandTypeRelative(),
                            serializer.ToXml(fns, "").Single().ToString(SaveOptions.DisableFormatting));
        }

        [Test]
        public void
            CheckXmlGenerationIntervalTypeVariableDeadbandTypeRelativeWithNegativeMaxVelocityShouldBeAbsoluteInXml()
        {
            setting.MaxSpeed = -setting.MaxSpeed;
            IntervalRule intervalRule = CreateIntervalRule();
            intervalRule.IntervalType = IntervalRule.IntervalRuleIntervalType.Variable;
            intervalRule.DeadBandType = IntervalRule.IntervalRuleDeadBandType.PercentageDischarge;

            var serializer = new IntervalRuleSerializer(intervalRule);

            Assert.AreEqual(OriginXmlIntervalTypeVariableDeadbandTypeRelative(),
                            serializer.ToXml(fns, "").Single().ToString(SaveOptions.DisableFormatting));
        }

        [TestCase(IntervalRule.IntervalRuleSetPointType.Fixed, 1.23)]
        [TestCase(IntervalRule.IntervalRuleSetPointType.Variable, 5.0)]
        public void GivenAnIntervalRuleWithAFixedOrVariableSetPoint_WhenCallingTheTimeSeries_ThenTheseShouldBeGenerated(
            IntervalRule.IntervalRuleSetPointType intervalRuleIntervalType, double expectedValue)
        {
            IntervalRule intervalRule = CreateIntervalRule();
            intervalRule.SetPointType = intervalRuleIntervalType;

            DateTime start = DateTime.Now;
            DateTime stop = start.Add(new TimeSpan(3, 0, 0));
            var step = new TimeSpan(1, 0, 0);

            var serializer = new IntervalRuleSerializer(intervalRule);

            IXmlTimeSeries constantValueTimeSeries =
                serializer.XmlImportTimeSeries("prefix", start, stop, step).FirstOrDefault();
            Assert.IsNotNull(constantValueTimeSeries);
            Assert.AreEqual(2, constantValueTimeSeries.TimeSeries.Time.Values.Count);
            Assert.AreEqual(expectedValue, constantValueTimeSeries.TimeSeries[constantValueTimeSeries.StartTime]);
            Assert.AreEqual(expectedValue, constantValueTimeSeries.TimeSeries[constantValueTimeSeries.EndTime]);
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

        private IntervalRule CreateIntervalRule()
        {
            const double defaultValue = 1.23;
            var intervalRule = new IntervalRule
            {
                Name = ruleName,
                Setting = setting,
                Inputs = new EventedList<IInput> {input},
                Outputs = new EventedList<Output> {output},
                DeadbandAroundSetpoint = deadbandAroundSetpoint,
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