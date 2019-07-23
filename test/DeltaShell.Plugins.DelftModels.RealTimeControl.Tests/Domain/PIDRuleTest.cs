using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils.Domain;
using NUnit.Framework;
using Rhino.Mocks;
using ValidationAspects;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Domain
{
    [TestFixture]
    public class PIDRuleTest
    {
        private static readonly XNamespace Fns = "http://www.wldelft.nl/fews";

        //private const string IffezheimKi = "Iffezheim_KI";
        private const string IffezheimKi = "[IP]pid rule";
        private const string DifferentialPart = "[DP]pid rule";
        private const string RuleName = "pid rule";
        private const string IffezheimHin1 = "Iffezheim";
        private const string IffezheimHin2 = "HIn";
        //private const string IffezheimHsp = "Iffezheim_HSP";
        private const string IffezheimHsp = "[SetPoint]pid rule";
        private const string IffezheimSout1 = "Iffezheim";
        private const string IffezheimSout2 = "SOut";
        private const double SMin = 116;
        private const double SMax = 123.6;
        private const double SMaxSpeed = 0.2;
        private const double Kp = 0.5;
        private const double Ki = 0.2;
        private const double Kd = 0;
        private const double ConstantSetpointValue = 1.23d;

        private Setting setting;
        private Input input;
        private Output output;

        MockRepository mockRepository = new MockRepository();
        private ITimeDependentModel timeDependentModelMock;
        
        [SetUp]
        public void SetUp()
        {
            setting = new Setting { Min = SMin, Max = SMax, MaxSpeed = SMaxSpeed };
            input = new Input
                        {
                            ParameterName = IffezheimHin2,
                            Feature = new RtcTestFeature { Name = IffezheimHin1 },
                            SetPoint = IffezheimHsp
                        };
            output = new Output
            {
                ParameterName = IffezheimSout2,
                Feature = new RtcTestFeature { Name = IffezheimSout1 },
                IntegralPart = IffezheimKi,
                DifferentialPart = DifferentialPart
            };

            timeDependentModelMock = mockRepository.Stub<ITimeDependentModel>();
        }

        [Test]
        public void PIDRuleRequiresOneInputValidation()
        {
            var pidRule = new PIDRule
            {
                Name = RuleName,
                Kp = Kp,
                Ki = Ki,
                Kd = Kd,
                Setting = setting,
                Outputs = new EventedList<Output> { output }
            };
            var validationResult = pidRule.Validate(); // ValidationAspects call
            Assert.IsFalse(validationResult.IsValid);
        }

        [Test]
        public void CheckXmlGeneration()
        {
            var pidRule = new PIDRule
                              {
                                  Name = RuleName,
                                  Kp = Kp,
                                  Ki = Ki,
                                  Kd = Kd,
                                  Setting = setting,
                                  Inputs = new EventedList<Input> {input},
                                  Outputs = new EventedList<Output> {output},
                                  PidRuleSetpointType = PIDRule.PIDRuleSetpointType.TimeSeries
                              };

            pidRule.TimeSeries.Components[0].DefaultValue = ConstantSetpointValue;

            Assert.AreEqual(OriginXml(), pidRule.ToXml(Fns, "").ToString(SaveOptions.DisableFormatting));

            var start = DateTime.Now;
            var stop = timeDependentModelMock.StartTime.Add(new TimeSpan(3, 0, 0));
            var step = new TimeSpan(1,0,0);

            var setpointTimeSeries = pidRule.XmlImportTimeSeries("prefix", start, stop, step).FirstOrDefault();
            Assert.IsNotNull(setpointTimeSeries);
            Assert.AreEqual(2,setpointTimeSeries.TimeSeries.Time.Values.Count);
            Assert.AreEqual(ConstantSetpointValue, setpointTimeSeries.TimeSeries[setpointTimeSeries.StartTime]);
        }

        private static string OriginXml()
        {
            return "<rule xmlns=\"http://www.wldelft.nl/fews\">" +
                    "<pid id=\"[PID]" + RuleName + "\">" +
                    "<mode>PIDVEL</mode>" +
                    "<settingMin>" + SMin.ToString(CultureInfo.InvariantCulture) + "</settingMin>" +
                    "<settingMax>" + SMax.ToString(CultureInfo.InvariantCulture) + "</settingMax>" +
                    "<settingMaxSpeed>" + SMaxSpeed.ToString(CultureInfo.InvariantCulture) + "</settingMaxSpeed>" +
                    "<kp>" + Kp.ToString(CultureInfo.InvariantCulture) + "</kp>"+
                    "<ki>" + Ki.ToString(CultureInfo.InvariantCulture) + "</ki>"+
                    "<kd>" + Kd.ToString(CultureInfo.InvariantCulture) + "</kd>" + 
                    "<input>"+
                    "<x>" + RtcXmlTag.Input + IffezheimHin1 + "/" + IffezheimHin2 + "</x>" +
                    "<setpointSeries>[SetPoint]" + RuleName + "</setpointSeries>" + 
                    "</input>" + 
                    "<output>"+
                    "<y>" + RtcXmlTag.Output + IffezheimSout1 + "/" + IffezheimSout2 + "</y>" +
                    "<integralPart>" + IffezheimKi + "</integralPart>" +
                    "<differentialPart>" + DifferentialPart + "</differentialPart>" +
                    "</output>" +
                    "</pid>" +
                    "</rule>";
        }

        [Test]
        public void CheckReferenceXmlGeneration()
        {
            var pidRule = new PIDRule
                              {
                                  Name = RuleName
                              };

            Assert.AreEqual(OriginXmlReference(), pidRule.ToXmlReference(Fns, "").ToString(SaveOptions.DisableFormatting));
        }


        private static string OriginXmlReference()
        {
            return "<trigger xmlns=\"http://www.wldelft.nl/fews\"><ruleReference>[PID]" + RuleName + "</ruleReference></trigger>";
        }

        [Test]
        public void Clone()
        {
            var pidRule = new PIDRule
            {
                Name = RuleName,
                //IsAConstant = true,
                //ConstantValue = 1.0,
                Kp = Kp,
                Ki = Ki,
                Kd = Kd,
                Setting = setting,
                Inputs = new EventedList<Input> { input },
                Outputs = new EventedList<Output> { output }
            };

            var clone = (PIDRule)pidRule.Clone();

            Assert.AreEqual(pidRule.Name, clone.Name);
            //Assert.AreEqual(pidRule.IsAConstant, clone.IsAConstant);
            //Assert.AreEqual(pidRule.ConstantValue, clone.ConstantValue);
            Assert.AreEqual(pidRule.Kp, clone.Kp);
            Assert.AreEqual(pidRule.Ki, clone.Ki);
            Assert.AreEqual(pidRule.Kd, clone.Kd);
            Assert.IsNotNull(clone.Setting);

            clone.Name = "";
            //clone.IsAConstant = false;
            //clone.ConstantValue = -1;
            clone.Kp = -1;
            clone.Ki = -1;
            clone.Kd = -1;


            Assert.AreNotEqual(pidRule.Name, clone.Name);
            //Assert.AreNotEqual(pidRule.IsAConstant, clone.IsAConstant);
            //Assert.AreNotEqual(pidRule.ConstantValue, clone.ConstantValue);
            Assert.AreNotEqual(pidRule.Kp, clone.Kp);
            Assert.AreNotEqual(pidRule.Ki, clone.Ki);
            Assert.AreNotEqual(pidRule.Kd, clone.Kd);


        }

        [Test]
        public void CopyFrom()
        {
            var pidRuleSource = new PIDRule
              {
                  Name = RuleName,
                  //IsAConstant = true,
                  //ConstantValue = 1.0,
                  Kp = Kp,
                  Ki = Ki,
                  Kd = Kd,
                  Setting = setting,
                  Inputs = new EventedList<Input> {input},
                  Outputs = new EventedList<Output> {output}
              };

            var pidRule = new PIDRule();

            pidRule.CopyFrom(pidRuleSource);

            Assert.AreEqual(RuleName, pidRule.Name);
            //Assert.AreEqual(true,pidRule.IsAConstant);
            //Assert.AreEqual(1.0d,pidRule.ConstantValue);
            Assert.AreEqual(Kp,pidRule.Kp);
            Assert.AreEqual(Ki,pidRule.Ki);
            Assert.AreEqual(Kd,pidRule.Kd);
            Assert.AreEqual(setting.Min,pidRule.Setting.Min);
            Assert.AreEqual(setting.Max,pidRule.Setting.Max); 
            Assert.AreEqual(setting.MaxSpeed,pidRule.Setting.MaxSpeed);
        }

        [Test]
        public void CopyFromAndClone()
        {
            var source = new PIDRule
            {
                Name = RuleName,
                Kp = Kp,
                Ki = Ki,
                Kd = Kd,
                Setting = setting,
                Inputs = new EventedList<Input> { input },
                Outputs = new EventedList<Output> { output }
            };

            var newRule = new PIDRule();
            newRule.CopyFrom(source);

            Assert.AreEqual(RuleName, newRule.Name);
            Assert.AreEqual(Kp, newRule.Kp);
            Assert.AreEqual(Ki, newRule.Ki);
            Assert.AreEqual(Kd, newRule.Kd);
            Assert.AreEqual(setting.Min, newRule.Setting.Min);
            Assert.AreEqual(setting.Max, newRule.Setting.Max);
            Assert.AreEqual(setting.MaxSpeed, newRule.Setting.MaxSpeed);

            var clone = (PIDRule)source.Clone();
            Assert.IsFalse(ReferenceEquals(source, clone));
            Assert.AreEqual(source.Name, clone.Name);
        }

        [Test]
        public void XmlImportTimeSeriesTruncatesValuesOutsideModelTimes()
        {
            var startTime = new DateTime(2012, 1, 1);
            var stopTime = new DateTime(2012, 1, 31);
            var timeStep = new TimeSpan(0, 1, 0, 0);

            var timeSeries = new TimeSeries()
            {
                Components = { new Variable<double>("SetPoint") },
                Name = "SetPoint"
            };

            timeSeries.Time.DefaultValue = new DateTime(2000, 1, 1);
            timeSeries.Time.InterpolationType = InterpolationType.Linear;
            timeSeries.Time.ExtrapolationType = ExtrapolationType.Constant;

            timeSeries[startTime] = 1.0;
            timeSeries[stopTime] = 31.0;

            var modelStartTime = startTime.AddDays(1);
            var modelStopTime = stopTime.AddDays(-1);

            var pidrule = new PIDRule("pid")
            {
                TimeSeries = timeSeries,
                PidRuleSetpointType = PIDRule.PIDRuleSetpointType.TimeSeries
            };

            var truncatedTimeSeries = pidrule.XmlImportTimeSeries("", modelStartTime, modelStopTime, timeStep).ToList();

            Assert.AreEqual(1, truncatedTimeSeries.Count);
            Assert.AreEqual(modelStartTime, truncatedTimeSeries[0].TimeSeries.Time.Values.First());
            Assert.AreEqual(modelStopTime, truncatedTimeSeries[0].TimeSeries.Time.Values.Last());

        }

        [TestCase(PIDRule.PIDRuleSetpointType.TimeSeries, 1)]
        [TestCase(PIDRule.PIDRuleSetpointType.Constant, 0)]
        [TestCase(PIDRule.PIDRuleSetpointType.Signal, 0)]
        public void
            GivenAPidRuleWithASetPointType_WhenXmlImportTimeSeriesIsCalled_ThenExpectedNumberOfXmlTimeSeriesIsReturned(PIDRule.PIDRuleSetpointType setPointType, int expectedNumber)
        {
            // Given
            var pidRule = new PIDRule
            {
                PidRuleSetpointType = setPointType
            };

            // When
            var timeSeries = pidRule.XmlImportTimeSeries("/", DateTime.Today, DateTime.Today.AddDays(1), new TimeSpan(0, 1, 0, 0));

            // Then
            Assert.AreEqual(expectedNumber, timeSeries.Count());
        }
    }
}