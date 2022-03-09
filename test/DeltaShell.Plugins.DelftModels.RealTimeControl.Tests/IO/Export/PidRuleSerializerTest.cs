using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Export;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Xml;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.IO.Export
{
    [TestFixture]
    public class PidRuleSerializerTest
    {
        private const string iffezheimKi = "[IP]pid rule";
        private const string differentialPart = "[DP]pid rule";
        private const string ruleName = "pid rule";
        private const string iffezheimHin1 = "Iffezheim";
        private const string iffezheimHin2 = "HIn";
        private const string iffezheimHsp = "[SetPoint]pid rule";
        private const string iffezheimSout1 = "Iffezheim";
        private const string iffezheimSout2 = "SOut";
        private const double sMin = 116;
        private const double sMax = 123.6;
        private const double sMaxSpeed = 0.2;
        private const double kp = 0.5;
        private const double ki = 0.2;
        private const double kd = 0;
        private const double constantSetpointValue = 1.23d;
        private static readonly XNamespace fns = "http://www.wldelft.nl/fews";

        private Setting setting;
        private Input input;
        private Output output;

        private readonly MockRepository mockRepository = new MockRepository();
        private ITimeDependentModel timeDependentModelMock;

        [SetUp]
        public void SetUp()
        {
            setting = new Setting
            {
                Min = sMin,
                Max = sMax,
                MaxSpeed = sMaxSpeed
            };
            input = new Input
            {
                ParameterName = iffezheimHin2,
                Feature = new RtcTestFeature {Name = iffezheimHin1},
                SetPoint = iffezheimHsp
            };
            output = new Output
            {
                ParameterName = iffezheimSout2,
                Feature = new RtcTestFeature {Name = iffezheimSout1},
                IntegralPart = iffezheimKi,
                DifferentialPart = differentialPart
            };

            timeDependentModelMock = mockRepository.Stub<ITimeDependentModel>();
        }

        [Test]
        public void CheckXmlGeneration()
        {
            var pidRule = new PIDRule
            {
                Name = ruleName,
                Kp = kp,
                Ki = ki,
                Kd = kd,
                Setting = setting,
                Inputs = new EventedList<IInput> {input},
                Outputs = new EventedList<Output> {output},
                PidRuleSetpointType = PIDRule.PIDRuleSetpointTypes.TimeSeries
            };

            pidRule.TimeSeries.Components[0].DefaultValue = constantSetpointValue;

            var serializer = new PidRuleSerializer(pidRule);

            Assert.AreEqual(OriginXml(), serializer.ToXml(fns, "").Single().ToString(SaveOptions.DisableFormatting));

            DateTime start = DateTime.Now;
            DateTime stop = timeDependentModelMock.StartTime.Add(new TimeSpan(3, 0, 0));
            var step = new TimeSpan(1, 0, 0);

            IXmlTimeSeries setpointTimeSeries =
                serializer.XmlImportTimeSeries("prefix", start, stop, step).FirstOrDefault();
            Assert.IsNotNull(setpointTimeSeries);
            Assert.AreEqual(2, setpointTimeSeries.TimeSeries.Time.Values.Count);
            Assert.AreEqual(constantSetpointValue, setpointTimeSeries.TimeSeries[setpointTimeSeries.StartTime]);
        }

        [Test]
        public void CheckReferenceXmlGeneration()
        {
            var pidRule = new PIDRule {Name = ruleName};

            var serializer = new PidRuleSerializer(pidRule);

            Assert.AreEqual(OriginXmlReference(),
                            serializer.ToXmlReference(fns, "").ToString(SaveOptions.DisableFormatting));
        }

        [Test]
        public void XmlImportTimeSeriesTruncatesValuesOutsideModelTimes()
        {
            var startTime = new DateTime(2012, 1, 1);
            var stopTime = new DateTime(2012, 1, 31);
            var timeStep = new TimeSpan(0, 1, 0, 0);

            var timeSeries = new TimeSeries()
            {
                Components = {new Variable<double>("SetPoint")},
                Name = "SetPoint"
            };

            timeSeries.Time.DefaultValue = new DateTime(2000, 1, 1);
            timeSeries.Time.InterpolationType = InterpolationType.Linear;
            timeSeries.Time.ExtrapolationType = ExtrapolationType.Constant;

            timeSeries[startTime] = 1.0;
            timeSeries[stopTime] = 31.0;

            DateTime modelStartTime = startTime.AddDays(1);
            DateTime modelStopTime = stopTime.AddDays(-1);

            var pidrule = new PIDRule("pid")
            {
                TimeSeries = timeSeries,
                PidRuleSetpointType = PIDRule.PIDRuleSetpointTypes.TimeSeries
            };

            var serializer = new PidRuleSerializer(pidrule);

            List<IXmlTimeSeries> truncatedTimeSeries =
                serializer.XmlImportTimeSeries("", modelStartTime, modelStopTime, timeStep).ToList();

            Assert.AreEqual(1, truncatedTimeSeries.Count);
            Assert.AreEqual(modelStartTime, truncatedTimeSeries[0].TimeSeries.Time.Values.First());
            Assert.AreEqual(modelStopTime, truncatedTimeSeries[0].TimeSeries.Time.Values.Last());
        }

        private static string OriginXml()
        {
            return "<rule xmlns=\"http://www.wldelft.nl/fews\">" +
                   "<pid id=\"[PID]" + ruleName + "\">" +
                   "<mode>PIDVEL</mode>" +
                   "<settingMin>" + sMin.ToString(CultureInfo.InvariantCulture) + "</settingMin>" +
                   "<settingMax>" + sMax.ToString(CultureInfo.InvariantCulture) + "</settingMax>" +
                   "<settingMaxSpeed>" + sMaxSpeed.ToString(CultureInfo.InvariantCulture) + "</settingMaxSpeed>" +
                   "<kp>" + kp.ToString(CultureInfo.InvariantCulture) + "</kp>" +
                   "<ki>" + ki.ToString(CultureInfo.InvariantCulture) + "</ki>" +
                   "<kd>" + kd.ToString(CultureInfo.InvariantCulture) + "</kd>" +
                   "<input>" +
                   "<x>" + RtcXmlTag.Input + iffezheimHin1 + "/" + iffezheimHin2 + "</x>" +
                   "<setpointSeries>[SetPoint]" + ruleName + "</setpointSeries>" +
                   "</input>" +
                   "<output>" +
                   "<y>" + RtcXmlTag.Output + iffezheimSout1 + "/" + iffezheimSout2 + "</y>" +
                   "<integralPart>" + iffezheimKi + "</integralPart>" +
                   "<differentialPart>" + differentialPart + "</differentialPart>" +
                   "</output>" +
                   "</pid>" +
                   "</rule>";
        }

        private static string OriginXmlReference()
        {
            return "<trigger xmlns=\"http://www.wldelft.nl/fews\"><ruleReference>[PID]" + ruleName +
                   "</ruleReference></trigger>";
        }

        [TestCase(PIDRule.PIDRuleSetpointTypes.TimeSeries, 1)]
        [TestCase(PIDRule.PIDRuleSetpointTypes.Constant, 0)]
        [TestCase(PIDRule.PIDRuleSetpointTypes.Signal, 0)]
        public void
            GivenAPidRuleWithASetPointType_WhenXmlImportTimeSeriesIsCalled_ThenExpectedNumberOfXmlTimeSeriesIsReturned(
                PIDRule.PIDRuleSetpointTypes setPointTypes, int expectedNumber)
        {
            // Given
            var pidRule = new PIDRule {PidRuleSetpointType = setPointTypes};

            var serializer = new PidRuleSerializer(pidRule);

            // When
            IEnumerable<IXmlTimeSeries> timeSeries =
                serializer.XmlImportTimeSeries("/", DateTime.Today, DateTime.Today.AddDays(1),
                                               new TimeSpan(0, 1, 0, 0));

            // Then
            Assert.AreEqual(expectedNumber, timeSeries.Count());
        }
    }
}