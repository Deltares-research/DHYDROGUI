using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using DelftTools.Functions.Generic;
using DelftTools.TestUtils;
using DeltaShell.Dimr;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Export;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils.Domain;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Engine
{
    [TestFixture]
    public class RealTimeControlXmlGeneratorTest
    {
        private const string FewsXmlheader = " xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"" +
                                             " xmlns:rtc=\"http://www.wldelft.nl/fews\"" +
                                             " xmlns=\"http://www.wldelft.nl/fews\"" +
                                             " xsi:schemaLocation=\"" +
                                             @"http://www.wldelft.nl/fews "; //\xsd\";

        private const string PiXmlheader = " xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"" +
                                           // no rtc namespace necessary
                                           " xmlns=\"http://www.wldelft.nl/fews/PI\"" +
                                           " xsi:schemaLocation=\"" +
                                           @"http://www.wldelft.nl/fews/PI "; //\xsd\";

        private RealTimeControlModel realTimeControlModel;
        private ControlGroup controlGroup;
        private Input input;
        private Output output;
        private PIDRule pidRule;
        private LookupSignal lookupSignal;
        private IntervalRule intervalRule;
        private StandardCondition condition;
        private StandardCondition condition2;
        private StandardCondition condition3;

        private string XsdPath => DimrApiDataSet.RtcXsdDirectory;

        private string RtcToolsConfigxsd => XsdPath + Path.DirectorySeparatorChar + "rtcToolsConfig.xsd\"";

        private string RtcDataConfigxsd => XsdPath + Path.DirectorySeparatorChar + "rtcDataConfig.xsd\"";

        private string RtcRuntimeConfigxsd => XsdPath + Path.DirectorySeparatorChar + "rtcRuntimeConfig.xsd\"";

        private string PiTimeSeriesxsd => XsdPath + Path.DirectorySeparatorChar + "pi_timeseries.xsd\"";

        [SetUp]
        public void Setup()
        {
            realTimeControlModel = new RealTimeControlModel
            {
                StartTime = new DateTime(2000, 1, 1, 0, 15, 30),
                StopTime = new DateTime(2001, 2, 3, 4, 15, 45),
                TimeStep = new TimeSpan(7, 0, 0)
            };

            controlGroup = new ControlGroup();

            input = new Input
            {
                ParameterName = "Water level",
                Feature = new RtcTestFeature {Name = "MeasureStationA"},
                SetPoint = "PIDRule Test"
            };

            output = new Output
            {
                ParameterName = "Crest level",
                Feature = new RtcTestFeature {Name = "WeirdWeir"},
                IntegralPart = "PIDRule Test"
            };

            condition = new StandardCondition
            {
                Name = "Trigger31",
                Reference = "IMPLICIT",
                Operation = Operation.Greater,
                Input = new Input
                {
                    ParameterName = "CondInputQuantityId",
                    Feature = new RtcTestFeature {Name = "CondInputLocation"}
                },
                Value = 1.1
            };

            controlGroup.Conditions.Add(condition);

            condition2 = new StandardCondition
            {
                Name = "C2",
                Reference = "IMPLICIT",
                Operation = Operation.Greater,
                Input = new Input
                {
                    ParameterName = "CondInputQuantityId",
                    Feature = new RtcTestFeature {Name = "CondInputLocation"}
                },
                Value = 2.2
            };

            condition3 = new StandardCondition
            {
                Name = "C3",
                Reference = "IMPLICIT",
                Operation = Operation.Less,
                Input = new Input
                {
                    ParameterName = "CondInputQuantityId",
                    Feature = new RtcTestFeature {Name = "CondInputLocation"}
                },
                Value = 0.5
            };
        }

        /// <summary>
        /// See RTC Document Jaco
        /// 6.3.2.c
        /// </summary>
        [Test]
        [Category(TestCategory.Integration)]
        public void GenerateXml_C1AndC2_Or_NotC1AndC3()
        {
            // Setup
            string header = "<rtcToolsConfig" + FewsXmlheader + RtcToolsConfigxsd + ">";
            string strC1AndC2_Or_NotC1AndC3 =
                header +
                "<general><description>RTC Model DeltaShell</description><poolRoutingScheme>Theta</poolRoutingScheme><theta>0.5</theta>" +
                "</general><rules><rule><unitDelay id=\"Interval Test_unitDelay\"><input><x>[Output]WeirdWeir/Crest level</x>" +
                "</input><output><y>[Output]WeirdWeir/Crest level</y>" +
                "</output>" +
                "</unitDelay>" +
                $"</rule><rule><interval id=\"{AppendDefaultControlGroupName(RtcXmlTag.IntervalRule)}/Interval Test\"><settingBelow>0.2</settingBelow><settingAbove>0.3</settingAbove><settingMaxStep>0</settingMaxStep><deadbandSetpointAbsolute>0.1</deadbandSetpointAbsolute><input><x ref=\"EXPLICIT\">[Input]MeasureStationA/Water level</x><setpoint>{AppendDefaultControlGroupName(RtcXmlTag.SP)}/Interval Test</setpoint>" +
                $"</input><output><y>[Output]WeirdWeir/Crest level</y><status>{AppendDefaultControlGroupName(RtcXmlTag.Status)}/Interval Test</status>" +
                "</output>" +
                "</interval>" +
                "</rule>" +
                $"</rules><triggers><trigger><standard id=\"{AppendDefaultControlGroupName(RtcXmlTag.StandardCondition)}/C1\"><condition><x1Series ref=\"IMPLICIT\">[Input]CondInputLocation/CondInputQuantityId</x1Series><relationalOperator>Greater</relationalOperator><x2Value>1.1</x2Value>" +
                $"</condition><true><trigger><standard id=\"{AppendDefaultControlGroupName(RtcXmlTag.StandardCondition)}/C2\"><condition><x1Series ref=\"IMPLICIT\">[Input]CondInputLocation/CondInputQuantityId</x1Series><relationalOperator>Greater</relationalOperator><x2Value>2.2</x2Value>" +
                $"</condition><true><trigger><ruleReference>{AppendDefaultControlGroupName(RtcXmlTag.IntervalRule)}/Interval Test</ruleReference>" +
                "</trigger>" +
                $"</true><output><status>{AppendDefaultControlGroupName(RtcXmlTag.Status)}/C2</status>" +
                "</output>" +
                "</standard>" +
                "</trigger>" +
                $"</true><false><trigger><standard id=\"{AppendDefaultControlGroupName(RtcXmlTag.StandardCondition)}/C3\"><condition><x1Series ref=\"IMPLICIT\">[Input]CondInputLocation/CondInputQuantityId</x1Series><relationalOperator>Less</relationalOperator><x2Value>0.5</x2Value>" +
                $"</condition><true><trigger><ruleReference>{AppendDefaultControlGroupName(RtcXmlTag.IntervalRule)}/Interval Test</ruleReference>" +
                "</trigger>" +
                $"</true><output><status>{AppendDefaultControlGroupName(RtcXmlTag.Status)}/C3</status>" +
                "</output>" +
                "</standard>" +
                "</trigger>" +
                $"</false><output><status>{AppendDefaultControlGroupName(RtcXmlTag.Status)}/C1</status>" +
                "</output>" +
                "</standard>" +
                "</trigger>" +
                "</triggers>" +
                "</rtcToolsConfig>";

            SetUpIntervalRule();

            condition.Name = "C1";

            condition.TrueOutputs.Add(condition2);
            condition.FalseOutputs.Add(condition3);

            condition2.TrueOutputs.Add(intervalRule);

            condition3.TrueOutputs.Add(intervalRule);

            controlGroup.Conditions.Add(condition2);
            controlGroup.Conditions.Add(condition3);

            // Call
            XDocument xDocument = RealTimeControlXmlWriter.GetToolsConfigXml(XsdPath, new List<ControlGroup> {controlGroup});

            // Assert
            Assert.IsNotNull(xDocument);
            Assert.AreEqual(strC1AndC2_Or_NotC1AndC3, xDocument.ToString(SaveOptions.DisableFormatting));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GenerateXml_DirectionalCondition()
        {
            // Setup
            string expectedXml =
                "<rtcToolsConfig" + FewsXmlheader + RtcToolsConfigxsd + ">" +
                "<general><description>RTC Model DeltaShell</description><poolRoutingScheme>Theta</poolRoutingScheme><theta>0.5</theta>" +
                $"</general><components><component><unitDelay id=\"{RtcXmlTag.Delayed}[Input]CondInputLocation/CondInputQuantityId\"><input><x>[Input]CondInputLocation/CondInputQuantityId</x>" +
                "</input><output><y>[Input]CondInputLocation/CondInputQuantityId-1</y>" +
                "</output>" +
                "</unitDelay>" +
                "</component>" +
                "</components><rules><rule><unitDelay id=\"Interval Test_unitDelay\"><input><x>[Output]WeirdWeir/Crest level</x>" +
                "</input><output><y>[Output]WeirdWeir/Crest level</y>" +
                "</output>" +
                "</unitDelay>" +
                $"</rule><rule><interval id=\"{AppendDefaultControlGroupName(RtcXmlTag.IntervalRule)}/Interval Test\"><settingBelow>0.2</settingBelow><settingAbove>0.3</settingAbove><settingMaxStep>0</settingMaxStep><deadbandSetpointAbsolute>0.1</deadbandSetpointAbsolute><input><x ref=\"EXPLICIT\">[Input]MeasureStationA/Water level</x><setpoint>{AppendDefaultControlGroupName(RtcXmlTag.SP)}/Interval Test</setpoint>" +
                $"</input><output><y>[Output]WeirdWeir/Crest level</y><status>{AppendDefaultControlGroupName(RtcXmlTag.Status)}/Interval Test</status>" +
                "</output>" +
                "</interval>" +
                "</rule>" +
                $"</rules><triggers><trigger><standard id=\"{AppendDefaultControlGroupName(RtcXmlTag.DirectionalCondition)}/C5\"><condition><x1Series ref=\"EXPLICIT\">[Input]CondInputLocation/CondInputQuantityId</x1Series><relationalOperator>Less</relationalOperator><x2Series ref=\"EXPLICIT\">[Input]CondInputLocation/CondInputQuantityId-1</x2Series>" +
                $"</condition><true><trigger><ruleReference>{AppendDefaultControlGroupName(RtcXmlTag.IntervalRule)}/Interval Test</ruleReference>" +
                "</trigger>" +
                $"</true><output><status>{AppendDefaultControlGroupName(RtcXmlTag.Status)}/C5</status>" +
                "</output>" +
                "</standard>" +
                "</trigger>" +
                "</triggers>" +
                "</rtcToolsConfig>";

            SetUpIntervalRule();

            var condition4 = new DirectionalCondition
            {
                Name = "C5",
                Operation = Operation.Less,
                Input = new Input
                {
                    ParameterName = "CondInputQuantityId",
                    Feature = new RtcTestFeature {Name = "CondInputLocation"}
                }
            };

            condition4.TrueOutputs.Add(intervalRule);

            controlGroup.Conditions.Add(condition4);
            controlGroup.Conditions.Remove(condition);

            // Call
            XDocument xDocument = RealTimeControlXmlWriter.GetToolsConfigXml(XsdPath, new List<ControlGroup> {controlGroup});

            // Assert
            Assert.IsNotNull(xDocument);
            Assert.AreEqual(expectedXml, xDocument.ToString(SaveOptions.DisableFormatting));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GenerateXml_LookupSignal()
        {
            // Setup
            string expectedXml =
                "<rtcToolsConfig" + FewsXmlheader + RtcToolsConfigxsd + ">" +
                "<general><description>RTC Model DeltaShell</description><poolRoutingScheme>Theta</poolRoutingScheme><theta>0.5</theta>" +
                "</general><rules><rule><unitDelay id=\"PIDRule Test_unitDelay\"><input><x>[Output]WeirdWeir/Crest level</x>" +
                "</input><output><y>[Output]WeirdWeir/Crest level</y>" +
                "</output>" +
                "</unitDelay>" +
                $"</rule><rule><lookupTable id=\"{AppendDefaultControlGroupName(RtcXmlTag.LookupSignal)}/SetPointForPID\"><table><record x=\"10\" y=\"3\" /><record x=\"100\" y=\"6\" />" +
                "</table><interpolationOption>LINEAR</interpolationOption><extrapolationOption>BLOCK</extrapolationOption><input><x ref=\"IMPLICIT\">[Input]MeasureStationB/Discharge</x>" +
                $"</input><output><y>{AppendDefaultControlGroupName(RtcXmlTag.Signal)}/SetPointForPID</y>" +
                "</output>" +
                "</lookupTable>" +
                $"</rule><rule><pid id=\"{AppendDefaultControlGroupName(RtcXmlTag.PIDRule)}/PIDRule Test\"><mode>PIDVEL</mode><settingMin>1.1</settingMin><settingMax>1.2</settingMax><settingMaxSpeed>1.3</settingMaxSpeed><kp>0.3</kp><ki>0.2</ki><kd>0.1</kd><input><x>[Input]MeasureStationA/Water level</x><setpointSeries>{AppendDefaultControlGroupName(RtcXmlTag.SP)}/PIDRule Test</setpointSeries>" +
                $"</input><output><y>[Output]WeirdWeir/Crest level</y><integralPart>{AppendDefaultControlGroupName(RtcXmlTag.IP)}/PIDRule Test</integralPart><differentialPart>{AppendDefaultControlGroupName(RtcXmlTag.DP)}/PIDRule Test</differentialPart>" +
                "</output>" +
                "</pid>" +
                "</rule>" +
                $"</rules><triggers><trigger><standard id=\"{AppendDefaultControlGroupName(RtcXmlTag.StandardCondition)}/Trigger31\"><condition><x1Series ref=\"IMPLICIT\">[Input]CondInputLocation/CondInputQuantityId</x1Series><relationalOperator>Greater</relationalOperator><x2Value>1.1</x2Value>" +
                $"</condition><true><trigger><ruleReference>{AppendDefaultControlGroupName(RtcXmlTag.PIDRule)}/PIDRule Test</ruleReference>" +
                "</trigger>" +
                $"</true><false><trigger><ruleReference>{AppendDefaultControlGroupName(RtcXmlTag.PIDRule)}/PIDRule Test</ruleReference>" +
                "</trigger>" +
                $"</false><output><status>{AppendDefaultControlGroupName(RtcXmlTag.Status)}/Trigger31</status>" +
                "</output>" +
                "</standard>" +
                "</trigger>" +
                "</triggers>" +
                "</rtcToolsConfig>";

            SetUpGlobalPidRuleForGlobalControlGroup();
            SetUpLookupSignalForPidRule();

            // Call
            XDocument xDocument = RealTimeControlXmlWriter.GetToolsConfigXml(XsdPath, new List<ControlGroup> {controlGroup});

            // Assert
            Assert.IsNotNull(xDocument);
            Assert.AreEqual(expectedXml, xDocument.ToString(SaveOptions.DisableFormatting));
        }

        /// <summary>
        /// See RTC Document Jaco
        /// 6.3.2.a
        /// </summary>
        [Test]
        [Category(TestCategory.Integration)]
        public void GenerateXmlC1And_C2orC3()
        {
            // Setup
            string header = "<rtcToolsConfig" + FewsXmlheader + RtcToolsConfigxsd + ">";
            string strC1And_C2OrC3 =
                header +
                "<general><description>RTC Model DeltaShell</description><poolRoutingScheme>Theta</poolRoutingScheme><theta>0.5</theta>" +
                "</general><rules><rule><unitDelay id=\"Interval Test_unitDelay\"><input><x>[Output]WeirdWeir/Crest level</x>" +
                "</input><output><y>[Output]WeirdWeir/Crest level</y>" +
                "</output>" +
                "</unitDelay>" +
                $"</rule><rule><interval id=\"{AppendDefaultControlGroupName(RtcXmlTag.IntervalRule)}/Interval Test\"><settingBelow>0.2</settingBelow><settingAbove>0.3</settingAbove><settingMaxStep>0</settingMaxStep><deadbandSetpointAbsolute>0.1</deadbandSetpointAbsolute><input><x ref=\"EXPLICIT\">[Input]MeasureStationA/Water level</x><setpoint>{AppendDefaultControlGroupName(RtcXmlTag.SP)}/Interval Test</setpoint>" +
                $"</input><output><y>[Output]WeirdWeir/Crest level</y><status>{AppendDefaultControlGroupName(RtcXmlTag.Status)}/Interval Test</status>" +
                "</output>" +
                "</interval>" +
                "</rule>" +
                $"</rules><triggers><trigger><standard id=\"{AppendDefaultControlGroupName(RtcXmlTag.StandardCondition)}/C1\"><condition><x1Series ref=\"IMPLICIT\">[Input]CondInputLocation/CondInputQuantityId</x1Series><relationalOperator>Greater</relationalOperator><x2Value>1.1</x2Value>" +
                $"</condition><true><trigger><standard id=\"{AppendDefaultControlGroupName(RtcXmlTag.StandardCondition)}/C2\"><condition><x1Series ref=\"IMPLICIT\">[Input]CondInputLocation/CondInputQuantityId</x1Series><relationalOperator>Greater</relationalOperator><x2Value>2.2</x2Value>" +
                $"</condition><true><trigger><ruleReference>{AppendDefaultControlGroupName(RtcXmlTag.IntervalRule)}/Interval Test</ruleReference>" +
                "</trigger>" +
                $"</true><false><trigger><standard id=\"{AppendDefaultControlGroupName(RtcXmlTag.StandardCondition)}/C3\"><condition><x1Series ref=\"IMPLICIT\">[Input]CondInputLocation/CondInputQuantityId</x1Series><relationalOperator>Less</relationalOperator><x2Value>0.5</x2Value>" +
                $"</condition><true><trigger><ruleReference>{AppendDefaultControlGroupName(RtcXmlTag.IntervalRule)}/Interval Test</ruleReference>" +
                "</trigger>" +
                $"</true><output><status>{AppendDefaultControlGroupName(RtcXmlTag.Status)}/C3</status>" +
                "</output>" +
                "</standard>" +
                "</trigger>" +
                $"</false><output><status>{AppendDefaultControlGroupName(RtcXmlTag.Status)}/C2</status>" +
                "</output>" +
                "</standard>" +
                "</trigger>" +
                $"</true><output><status>{AppendDefaultControlGroupName(RtcXmlTag.Status)}/C1</status>" +
                "</output>" +
                "</standard>" +
                "</trigger>" +
                "</triggers>" +
                "</rtcToolsConfig>";

            SetUpIntervalRule();

            condition.Name = "C1";

            condition.TrueOutputs.Add(condition2);
            condition2.TrueOutputs.Add(intervalRule);
            condition2.FalseOutputs.Add(condition3);

            condition3.TrueOutputs.Add(intervalRule);

            controlGroup.Conditions.Add(condition2);
            controlGroup.Conditions.Add(condition3);

            // Call
            XDocument xDocument = RealTimeControlXmlWriter.GetToolsConfigXml(XsdPath, new List<ControlGroup> {controlGroup});

            // Assert
            Assert.IsNotNull(xDocument);
            Assert.AreEqual(strC1And_C2OrC3, xDocument.ToString(SaveOptions.DisableFormatting));
        }

        /// <summary>
        /// See RTC Document Jaco
        /// 6.3.2.b(C1 and C2) or C3
        /// </summary>
        [Test]
        [Category(TestCategory.Integration)]
        public void GenerateXmlOfC1AndC2_OrC3()
        {
            // Setup
            string header = "<rtcToolsConfig" + FewsXmlheader + RtcToolsConfigxsd + ">";
            string strC1AndC2_OrC3 =
                header +
                "<general><description>RTC Model DeltaShell</description><poolRoutingScheme>Theta</poolRoutingScheme><theta>0.5</theta>" +
                "</general><rules><rule><unitDelay id=\"Interval Test_unitDelay\"><input><x>[Output]WeirdWeir/Crest level</x>" +
                "</input><output><y>[Output]WeirdWeir/Crest level</y>" +
                "</output>" +
                "</unitDelay>" +
                $"</rule><rule><interval id=\"{AppendDefaultControlGroupName(RtcXmlTag.IntervalRule)}/Interval Test\"><settingBelow>0.2</settingBelow><settingAbove>0.3</settingAbove><settingMaxStep>0</settingMaxStep><deadbandSetpointAbsolute>0.1</deadbandSetpointAbsolute><input><x ref=\"EXPLICIT\">[Input]MeasureStationA/Water level</x><setpoint>{AppendDefaultControlGroupName(RtcXmlTag.SP)}/Interval Test</setpoint>" +
                $"</input><output><y>[Output]WeirdWeir/Crest level</y><status>{AppendDefaultControlGroupName(RtcXmlTag.Status)}/Interval Test</status>" +
                "</output>" +
                "</interval>" +
                "</rule>" +
                $"</rules><triggers><trigger><standard id=\"{AppendDefaultControlGroupName(RtcXmlTag.StandardCondition)}/C1\"><condition><x1Series ref=\"IMPLICIT\">[Input]CondInputLocation/CondInputQuantityId</x1Series><relationalOperator>Greater</relationalOperator><x2Value>1.1</x2Value>" +
                $"</condition><true><trigger><standard id=\"{AppendDefaultControlGroupName(RtcXmlTag.StandardCondition)}/C2\"><condition><x1Series ref=\"IMPLICIT\">[Input]CondInputLocation/CondInputQuantityId</x1Series><relationalOperator>Greater</relationalOperator><x2Value>2.2</x2Value>" +
                $"</condition><true><trigger><ruleReference>{AppendDefaultControlGroupName(RtcXmlTag.IntervalRule)}/Interval Test</ruleReference>" +
                "</trigger>" +
                $"</true><false><trigger><standard id=\"{AppendDefaultControlGroupName(RtcXmlTag.StandardCondition)}/C3\"><condition><x1Series ref=\"IMPLICIT\">[Input]CondInputLocation/CondInputQuantityId</x1Series><relationalOperator>Less</relationalOperator><x2Value>0.5</x2Value>" +
                $"</condition><true><trigger><ruleReference>{AppendDefaultControlGroupName(RtcXmlTag.IntervalRule)}/Interval Test</ruleReference>" +
                "</trigger>" +
                $"</true><output><status>{AppendDefaultControlGroupName(RtcXmlTag.Status)}/C3</status>" +
                "</output>" +
                "</standard>" +
                "</trigger>" +
                $"</false><output><status>{AppendDefaultControlGroupName(RtcXmlTag.Status)}/C2</status>" +
                "</output>" +
                "</standard>" +
                "</trigger>" +
                $"</true><false><trigger><standard id=\"{AppendDefaultControlGroupName(RtcXmlTag.StandardCondition)}/C3\"><condition><x1Series ref=\"IMPLICIT\">[Input]CondInputLocation/CondInputQuantityId</x1Series><relationalOperator>Less</relationalOperator><x2Value>0.5</x2Value>" +
                $"</condition><true><trigger><ruleReference>{AppendDefaultControlGroupName(RtcXmlTag.IntervalRule)}/Interval Test</ruleReference>" +
                "</trigger>" +
                $"</true><output><status>{AppendDefaultControlGroupName(RtcXmlTag.Status)}/C3</status>" +
                "</output>" +
                "</standard>" +
                "</trigger>" +
                $"</false><output><status>{AppendDefaultControlGroupName(RtcXmlTag.Status)}/C1</status>" +
                "</output>" +
                "</standard>" +
                "</trigger>" +
                "</triggers>" +
                "</rtcToolsConfig>";

            SetUpIntervalRule();

            condition.Name = "C1";

            condition.TrueOutputs.Add(condition2);
            condition.FalseOutputs.Add(condition3);

            condition2.TrueOutputs.Add(intervalRule);
            condition2.FalseOutputs.Add(condition3);

            condition3.TrueOutputs.Add(intervalRule);

            controlGroup.Conditions.Add(condition2);
            controlGroup.Conditions.Add(condition3);

            // Call
            XDocument xDocument = RealTimeControlXmlWriter.GetToolsConfigXml(XsdPath, new List<ControlGroup> {controlGroup});

            // Assert
            Assert.IsNotNull(xDocument);
            Assert.AreEqual(strC1AndC2_OrC3, xDocument.ToString(SaveOptions.DisableFormatting));
        }

        [Test]
        public void GetRunTimeConfig()
        {
            string strOutputXml = "<rtcRuntimeConfig" + FewsXmlheader + RtcRuntimeConfigxsd + ">" +
                                  "<period>" +
                                  "<userDefined>" +
                                  "<startDate date=\"" + "2000-01-01"
                                  //string.Format("{0:0000}-{1:00}-{2:00}", realTimeControlModel.StartTime.Year, realTimeControlModel.StartTime.Month, realTimeControlModel.StartTime.Day) 
                                  + "\" time=\"" + "00:15:30"
                                  //string.Format("{0:00}:{1:00}", realTimeControlModel.StartTime.Hour, realTimeControlModel.StartTime.Minute)
                                  + "\" />" + "<endDate date=\"" + "2001-02-03"
                                  //string.Format("{0:0000}-{1:00}-{2:00}", realTimeControlModel.StopTime.Year, realTimeControlModel.StopTime.Month, realTimeControlModel.StopTime.Day)
                                  + "\" time=\"" + "04:15:45"
                                  //string.Format("{0:00}:{1:00}", realTimeControlModel.StopTime.Hour, realTimeControlModel.StopTime.Minute)
                                  + "\" />" +
                                  "<timeStep unit=\"hour\" multiplier=\"" + "7" /*realTimeControlModel.TimeStep.Minutes*/ +
                                  "\" divider=\"1\" />" + //= optional "<numberEnsembles>1</numberEnsembles>" + 
                                  "</userDefined>" +
                                  "</period>" +
                                  "<mode>" +
                                  "<simulation>" +
                                  "<limitedMemory>true</limitedMemory>" +
                                  "</simulation>" +
                                  "</mode>" +
                                  "</rtcRuntimeConfig>";
            TestHelper.AssertAtLeastOneLogMessagesContains(() =>
                                                           {
                                                               XDocument xDocument = RealTimeControlXmlWriter.GetRuntimeConfigXml(XsdPath, realTimeControlModel, false, 1);
                                                               Assert.IsNotNull(xDocument);
                                                               Assert.AreEqual(strOutputXml, xDocument.ToString(SaveOptions.DisableFormatting));
                                                           },
                                                           "Depricated option \"Limited Memory\" of D-RTC model is set to True");
        }

        [Test]
        public void GetRunTimeConfigIncludingLogging()
        {
            string strOutputXml = "<rtcRuntimeConfig" + FewsXmlheader + RtcRuntimeConfigxsd + ">" +
                                  "<period>" +
                                  "<userDefined>" +
                                  "<startDate date=\"" + "2000-01-01"
                                  //string.Format("{0:0000}-{1:00}-{2:00}", realTimeControlModel.StartTime.Year, realTimeControlModel.StartTime.Month, realTimeControlModel.StartTime.Day) 
                                  + "\" time=\"" + "00:15:30"
                                  //string.Format("{0:00}:{1:00}", realTimeControlModel.StartTime.Hour, realTimeControlModel.StartTime.Minute)
                                  + "\" />" + "<endDate date=\"" + "2001-02-03"
                                  //string.Format("{0:0000}-{1:00}-{2:00}", realTimeControlModel.StopTime.Year, realTimeControlModel.StopTime.Month, realTimeControlModel.StopTime.Day)
                                  + "\" time=\"" + "04:15:45"
                                  //string.Format("{0:00}:{1:00}", realTimeControlModel.StopTime.Hour, realTimeControlModel.StopTime.Minute)
                                  + "\" />" +
                                  "<timeStep unit=\"hour\" multiplier=\"" + "7" /*realTimeControlModel.TimeStep.Minutes*/ +
                                  "\" divider=\"1\" />" + //= optional "<numberEnsembles>1</numberEnsembles>" + 
                                  "</userDefined>" +
                                  "</period>" +
                                  "<mode>" +
                                  "<simulation>" +
                                  "<limitedMemory>true</limitedMemory>" +
                                  "</simulation>" +
                                  "</mode>" +
                                  "<logging>" +
                                  "<logLevel>4</logLevel>" +
                                  "<flushing>true</flushing>" +
                                  "</logging>" +
                                  "</rtcRuntimeConfig>";
            TestHelper.AssertAtLeastOneLogMessagesContains(() =>
                                                           {
                                                               XDocument xDocument = RealTimeControlXmlWriter.GetRuntimeConfigXml(XsdPath, realTimeControlModel, false, 4);
                                                               Assert.IsNotNull(xDocument);
                                                               Assert.AreEqual(strOutputXml, xDocument.ToString(SaveOptions.DisableFormatting));
                                                           },
                                                           "Depricated option \"Limited Memory\" of D-RTC model is set to True");
        }

        [Test]
        public void GetSTimeSeries()
        {
            SetUpGlobalPidRuleForGlobalControlGroup();
            // preferred minimal coding in test string to avoid missing
            string piTimeSeries =
                "<TimeSeries" + PiXmlheader + PiTimeSeriesxsd + " version=\"1.2\">" +
                $"<series><header><type>instantaneous</type><locationId>{AppendDefaultControlGroupName(RtcXmlTag.PIDRule)}/PIDRule Test</locationId><parameterId>SP</parameterId><timeStep unit=\"hour\" multiplier=\"7\" divider=\"1\" /><startDate date=\"2000-01-01\" time=\"00:15:30\" /><endDate date=\"2001-02-03\" time=\"04:15:45\" /><missVal>-999.0</missVal><stationName /><units />" +
                "</header><event date=\"2000-01-01\" time=\"00:15:30\" value=\"3\" /><event date=\"2001-02-03\" time=\"04:15:45\" value=\"4\" />" +
                "</series>" +
                "</TimeSeries>";

            XDocument xDocument = RealTimeControlXmlWriter.GetTimeSeriesXml(XsdPath, realTimeControlModel,
                                                                            new List<ControlGroup> {controlGroup});
            Assert.IsNotNull(xDocument);
            Assert.AreEqual(piTimeSeries, xDocument.ToString(SaveOptions.DisableFormatting));
        }

        [Test]
        public void GetSTimeSeriesReturnsDocumentWhenSetPointIsConstantOnlyInOneRuleTest()
        {
            SetUpGlobalPidRuleForGlobalControlGroup();
            var pidrule02TestName = "PIDRule02 Test";
            ControlGroup secondControlGroup = GetNewControlGroupWithNewPidRule(pidrule02TestName);
            var controlGroupList = new List<ControlGroup>
            {
                controlGroup,
                secondControlGroup
            };

            //SOBEK3-1074: If set point has been set to constant PID Controller should not write set time.
            pidRule.PidRuleSetpointType = PIDRule.PIDRuleSetpointType.Constant;

            //Only one of the rules is constant, the document should still be written with the values of the second.
            List<XElement> descendantsWithLocalName =
                GetxDocumentDescendantsForControlGroupListTimeSeries("locationId", controlGroupList);
            Assert.AreEqual(1, descendantsWithLocalName.Count);

            var expectedLocalName = $"{AppendDefaultControlGroupName(RtcXmlTag.PIDRule)}/{pidrule02TestName}";
            Assert.AreEqual(expectedLocalName, descendantsWithLocalName[0].Value); // only the PidRule from the second control group

            /*Set both to time series, there should be two nodes now*/
            pidRule.PidRuleSetpointType = PIDRule.PIDRuleSetpointType.TimeSeries;
            descendantsWithLocalName =
                GetxDocumentDescendantsForControlGroupListTimeSeries("locationId", controlGroupList);
            Assert.AreEqual(2, descendantsWithLocalName.Count);

            IEnumerable<string> valuesInNodes = descendantsWithLocalName.Select(d => d.Value);
            CollectionAssert.Contains(valuesInNodes, $"{AppendDefaultControlGroupName(RtcXmlTag.PIDRule)}/{pidRule.Name}");
            CollectionAssert.Contains(valuesInNodes, $"{AppendDefaultControlGroupName(RtcXmlTag.PIDRule)}/{pidrule02TestName}");
        }

        [Test]
        public void GetSTimeSeriesReturnsDocumentWhenTwoRulesInAControlGroupAndOneSetPointIsConstantTest()
        {
            SetUpTwoPidRulesSameOutput();
            pidRule.PidRuleSetpointType = PIDRule.PIDRuleSetpointType.Constant;
            RuleBase pidrule02 = controlGroup.Rules.FirstOrDefault(r => r != pidRule);
            Assert.NotNull(pidrule02);
            string pidrule02TestName = pidrule02.Name;

            //The document should be written but the constant one will be excluded
            var controlGroupList = new List<ControlGroup> {controlGroup};
            List<XElement> descendantsWithLocalName =
                GetxDocumentDescendantsForControlGroupListTimeSeries("locationId", controlGroupList);
            Assert.AreEqual(1, descendantsWithLocalName.Count);
            Assert.AreNotEqual(pidRule.Name, descendantsWithLocalName[0].Value);

            var expectedLocalName = $"{AppendDefaultControlGroupName(RtcXmlTag.PIDRule)}/{controlGroup.Rules[1].Name}";
            Assert.AreEqual(expectedLocalName, descendantsWithLocalName[0].Value);

            /*Set both to time series, there should be two nodes now*/
            pidRule.PidRuleSetpointType = PIDRule.PIDRuleSetpointType.TimeSeries;
            descendantsWithLocalName =
                GetxDocumentDescendantsForControlGroupListTimeSeries("locationId", controlGroupList);
            Assert.AreEqual(2, descendantsWithLocalName.Count);

            IEnumerable<string> valuesInNodes = descendantsWithLocalName.Select(d => d.Value);
            CollectionAssert.Contains(valuesInNodes, $"{AppendDefaultControlGroupName(RtcXmlTag.PIDRule)}/{pidRule.Name}");
            CollectionAssert.Contains(valuesInNodes, $"{AppendDefaultControlGroupName(RtcXmlTag.PIDRule)}/{pidrule02TestName}");
        }

        [Test]
        public void GetSTimeSeriesReturnsNullWhenSetPointIsConstantTest()
        {
            SetUpGlobalPidRuleForGlobalControlGroup();
            //SOBEK3-1074: If set point has been set to constant PID Controller should not write set time.
            pidRule.PidRuleSetpointType = PIDRule.PIDRuleSetpointType.Constant;
            //Because it's constant and there are no more rules nothing should be written.
            var controlGroupList = new List<ControlGroup> {controlGroup};
            XDocument xDocument = RealTimeControlXmlWriter.GetTimeSeriesXml(XsdPath, realTimeControlModel, controlGroupList);
            Assert.IsNull(xDocument);

            //When changed to time series it should be valid, thus written.
            pidRule.PidRuleSetpointType = PIDRule.PIDRuleSetpointType.TimeSeries;
            List<XElement> descendantsWithLocalName =
                GetxDocumentDescendantsForControlGroupListTimeSeries("locationId", controlGroupList);
            Assert.AreEqual(1, descendantsWithLocalName.Count);

            var expectedLocalNameValue = $"{AppendDefaultControlGroupName(RtcXmlTag.PIDRule)}/{pidRule.Name}";
            Assert.AreEqual(expectedLocalNameValue, descendantsWithLocalName[0].Value);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GivenAnIntervalRuleWithSignalAsSetPoint_WhenAskingForTimeSeriesFile_ThenThisFileShouldNotBeCreated()
        {
            // Given
            SetUpIntervalRule();
            condition.TrueOutputs.Add(intervalRule);
            condition.FalseOutputs.Add(intervalRule);
            SetUpLookupSignalForIntervalRule();
            intervalRule.SetPointType = IntervalRule.IntervalRuleSetPointType.Signal;
            var controlGroupList = new List<ControlGroup> {controlGroup};

            // When
            XDocument xDocument = RealTimeControlXmlWriter.GetTimeSeriesXml(XsdPath, realTimeControlModel, controlGroupList);

            // Then
            Assert.IsNull(xDocument);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GivenAnIntervalRuleWithSignalAsSetPoint_WhenAskingForDataConfigFile_ThenAReferenceToATimeSerieShouldNotBeWritten()
        {
            // Given
            SetUpIntervalRule();
            condition.TrueOutputs.Add(intervalRule);
            condition.FalseOutputs.Add(intervalRule);
            SetUpLookupSignalForIntervalRule();
            intervalRule.SetPointType = IntervalRule.IntervalRuleSetPointType.Signal;
            var controlGroupList = new List<ControlGroup> {controlGroup};

            // When
            XDocument xDocument = RealTimeControlXmlWriter.GetDataConfigXml(XsdPath, realTimeControlModel, controlGroupList, null);

            // Then
            Assert.IsNotNull(xDocument);

            string header = "<rtcDataConfig" + FewsXmlheader + RtcDataConfigxsd + ">";
            string strIntervalRule =
                header +
                "<importSeries><timeSeries id=\"[Input]MeasureStationA/Water level\"><OpenMIExchangeItem><elementId>MeasureStationA</elementId><quantityId>Water level</quantityId><unit>m</unit>" +
                "</OpenMIExchangeItem>" +
                "</timeSeries>" +
                "</importSeries><exportSeries><CSVTimeSeriesFile decimalSeparator=\".\" delimiter=\",\" adjointOutput=\"false\">" +
                "</CSVTimeSeriesFile><PITimeSeriesFile><timeSeriesFile>timeseries_export.xml</timeSeriesFile><useBinFile>false</useBinFile>" +
                "</PITimeSeriesFile><timeSeries id=\"[Output]WeirdWeir/Crest level\"><OpenMIExchangeItem><elementId>WeirdWeir</elementId><quantityId>Crest level</quantityId><unit>m</unit>" +
                "</OpenMIExchangeItem>" +
                $"</timeSeries><timeSeries id=\"{AppendDefaultControlGroupName(RtcXmlTag.Status)}/Trigger31\" /><timeSeries id=\"{AppendDefaultControlGroupName(RtcXmlTag.Status)}/{intervalRule.Name}\" /><timeSeries id=\"{AppendDefaultControlGroupName(RtcXmlTag.Signal)}/SetPointForIntervalRule\" />" +
                "</exportSeries>" +
                "</rtcDataConfig>";
            Assert.AreEqual(strIntervalRule, xDocument.ToString(SaveOptions.DisableFormatting));
        }

        [Test]
        [Category(TestCategory.Integration)]
        [TestCase(IntervalRule.IntervalRuleSetPointType.Variable, 3)]
        [TestCase(IntervalRule.IntervalRuleSetPointType.Fixed, 6)]
        public void GivenAnIntervalRuleWithAVariableOrFixedSetPoint_WhenAskingForTimeSeriesFile_ThenThisFileShouldBeCreatedWithOneTimeSerie(IntervalRule.IntervalRuleSetPointType intervalRuleSetPointType, int expectedValueInTimeSeriesFile)
        {
            // Given
            SetUpIntervalRule();
            condition.TrueOutputs.Add(intervalRule);
            condition.FalseOutputs.Add(intervalRule);
            intervalRule.SetPointType = intervalRuleSetPointType;
            var controlGroupList = new List<ControlGroup> {controlGroup};

            // When
            XDocument xDocument = RealTimeControlXmlWriter.GetTimeSeriesXml(XsdPath, realTimeControlModel, controlGroupList);

            // Then
            Assert.IsNotNull(xDocument);

            string piTimeSeries =
                "<TimeSeries" + PiXmlheader + PiTimeSeriesxsd + " version=\"1.2\">" +
                $"<series><header><type>instantaneous</type><locationId>{AppendDefaultControlGroupName(RtcXmlTag.IntervalRule)}/Interval Test</locationId><parameterId>SP</parameterId><timeStep unit=\"hour\" multiplier=\"7\" divider=\"1\" /><startDate date=\"2000-01-01\" time=\"00:15:30\" /><endDate date=\"2001-02-03\" time=\"04:15:45\" /><missVal>-999.0</missVal><stationName /><units />" +
                $"</header><event date=\"2000-01-01\" time=\"00:15:30\" value=\"{expectedValueInTimeSeriesFile}\" /><event date=\"2001-02-03\" time=\"04:15:45\" value=\"{expectedValueInTimeSeriesFile}\" />" +
                "</series>" +
                "</TimeSeries>";

            Assert.AreEqual(piTimeSeries, xDocument.ToString(SaveOptions.DisableFormatting));
        }

        [Test]
        public void GetToolsDataXml_OnePIDRuleOneLookupSignal()
        {
            SetUpGlobalPidRuleForGlobalControlGroup();
            SetUpLookupSignalForPidRule();

            string strOutputXml = DataResultXml(input, output, true);

            XDocument xDocument = RealTimeControlXmlWriter.GetDataConfigXml(XsdPath, realTimeControlModel,
                                                                            new List<ControlGroup> {controlGroup}, null);
            Assert.IsNotNull(xDocument);
            Assert.AreEqual(strOutputXml, xDocument.ToString(SaveOptions.DisableFormatting));
        }

        [Test]
        public void GetToolsDataXmlOnePIDRule()
        {
            SetUpGlobalPidRuleForGlobalControlGroup();
            string strOutputXml = DataResultXml(input, output, false);

            XDocument xDocument = RealTimeControlXmlWriter.GetDataConfigXml(XsdPath, realTimeControlModel,
                                                                            new List<ControlGroup> {controlGroup}, null);
            Assert.IsNotNull(xDocument);
            Assert.AreEqual(strOutputXml, xDocument.ToString(SaveOptions.DisableFormatting));
        }

        [Test]
        public void HydraulicRuleWithTimeLagDataConfigGenerationTest()
        {
            // Setup
            AddHydraulicRuleWithTimeLagToControlGroup();

            string header = "<rtcDataConfig" + FewsXmlheader + RtcDataConfigxsd + ">";
            string strDataConfigWithHydraulicRuleTimeLag =
                header +
                "<importSeries><timeSeries id=\"[Input]MeasureStationA/Water level\"><OpenMIExchangeItem><elementId>MeasureStationA</elementId><quantityId>Water level</quantityId><unit>m</unit>" +
                "</OpenMIExchangeItem>" +
                "</timeSeries>" +
                "</importSeries><exportSeries><CSVTimeSeriesFile decimalSeparator=\".\" delimiter=\",\" adjointOutput=\"false\">" +
                "</CSVTimeSeriesFile><PITimeSeriesFile><timeSeriesFile>timeseries_export.xml</timeSeriesFile><useBinFile>false</useBinFile>" +
                "</PITimeSeriesFile><timeSeries id=\"[Output]WeirdWeir/Crest level\"><OpenMIExchangeItem><elementId>WeirdWeir</elementId><quantityId>Crest level</quantityId><unit>m</unit>" +
                "</OpenMIExchangeItem>" +
                $"</timeSeries><timeSeries id=\"{AppendDefaultControlGroupName(RtcXmlTag.Status)}/Trigger31\" /><timeSeries id=\"{RtcXmlTag.Delayed}[Input]MeasureStationA/Water level\" vectorLength=\"9\"><PITimeSeries><locationId>MeasureStationA</locationId><parameterId>Water level</parameterId>" +
                "</PITimeSeries>" +
                "</timeSeries>" +
                "</exportSeries>" +
                "</rtcDataConfig>";

            XDocument xDocument = RealTimeControlXmlWriter.GetDataConfigXml(XsdPath, realTimeControlModel,
                                                                            new List<ControlGroup> {controlGroup},
                                                                            null);

            //  Assert
            Assert.IsNotNull(xDocument);
            Assert.AreEqual(strDataConfigWithHydraulicRuleTimeLag, xDocument.ToString(SaveOptions.DisableFormatting));
        }

        [Test]
        public void HydraulicRuleWithTimeLagToolsConfigGenerationTest()
        {
            // Set
            AddHydraulicRuleWithTimeLagToControlGroup();

            string header = "<rtcToolsConfig" + FewsXmlheader + RtcToolsConfigxsd + ">";
            string strHydraulicRuleTimeLag =
                header +
                "<general><description>RTC Model DeltaShell</description><poolRoutingScheme>Theta</poolRoutingScheme><theta>0.5</theta>" +
                $"</general><components><component><unitDelay id=\"{RtcXmlTag.Delayed}[Input]MeasureStationA/Water level\"><input><x>[Input]MeasureStationA/Water level</x>" +
                $"</input><output><yVector>{RtcXmlTag.Delayed}[Input]MeasureStationA/Water level</yVector>" +
                "</output>" +
                "</unitDelay>" +
                "</component>" +
                $"</components><rules><rule><lookupTable id=\"{AppendDefaultControlGroupName(RtcXmlTag.HydraulicRule)}/HydraulicRule\"><table><record x=\"0\" y=\"0\" />" +
                $"</table><interpolationOption>BLOCK</interpolationOption><extrapolationOption>BLOCK</extrapolationOption><input><x ref=\"EXPLICIT\">{RtcXmlTag.Delayed}[Input]MeasureStationA/Water level[8]</x>" +
                "</input><output><y>[Output]WeirdWeir/Crest level</y>" +
                "</output>" +
                "</lookupTable>" +
                "</rule>" +
                $"</rules><triggers><trigger><standard id=\"{AppendDefaultControlGroupName(RtcXmlTag.StandardCondition)}/Trigger31\"><condition><x1Series ref=\"IMPLICIT\">[Input]CondInputLocation/CondInputQuantityId</x1Series><relationalOperator>Greater</relationalOperator><x2Value>1.1</x2Value>" +
                $"</condition><true><trigger><ruleReference>{AppendDefaultControlGroupName(RtcXmlTag.HydraulicRule)}/HydraulicRule</ruleReference>" +
                "</trigger>" +
                $"</true><false><trigger><ruleReference>{AppendDefaultControlGroupName(RtcXmlTag.HydraulicRule)}/HydraulicRule</ruleReference>" +
                "</trigger>" +
                $"</false><output><status>{AppendDefaultControlGroupName(RtcXmlTag.Status)}/Trigger31</status>" +
                "</output>" +
                "</standard>" +
                "</trigger>" +
                "</triggers>" +
                "</rtcToolsConfig>";

            // Call
            XDocument xDocument = RealTimeControlXmlWriter.GetToolsConfigXml(XsdPath, new List<ControlGroup> {controlGroup});

            // Assert
            Assert.IsNotNull(xDocument);
            Assert.AreEqual(strHydraulicRuleTimeLag, xDocument.ToString(SaveOptions.DisableFormatting));
        }

        [Test]
        public void IntervalRuleToolsConfigGenerationTest()
        {
            // Setup
            SetUpIntervalRule();

            condition.TrueOutputs.Add(intervalRule);
            condition.FalseOutputs.Add(intervalRule);

            string header = "<rtcToolsConfig" + FewsXmlheader + RtcToolsConfigxsd + ">";
            string strPid =
                header +
                "<general>" +
                "<description>RTC Model DeltaShell</description>" +
                "<poolRoutingScheme>Theta</poolRoutingScheme>" +
                "<theta>0.5</theta>" +
                "</general>" +
                "<rules>" +
                "<rule>" +
                "<unitDelay id=\"Interval Test_unitDelay\">" +
                "<input>" +
                "<x>[Output]WeirdWeir/Crest level</x>" +
                "</input>" +
                "<output>" +
                "<y>[Output]WeirdWeir/Crest level</y>" +
                "</output>" +
                "</unitDelay>" +
                "</rule>" +
                "<rule>" +
                $"<interval id=\"{AppendDefaultControlGroupName(RtcXmlTag.IntervalRule)}/Interval Test\">" +
                "<settingBelow>0.2</settingBelow>" +
                "<settingAbove>0.3</settingAbove>" +
                "<settingMaxStep>0</settingMaxStep>" +
                "<deadbandSetpointAbsolute>0.1</deadbandSetpointAbsolute>" +
                "<input>" +
                "<x ref=\"EXPLICIT\">[Input]MeasureStationA/Water level</x>" +
                $"<setpoint>{AppendDefaultControlGroupName(RtcXmlTag.SP)}/Interval Test</setpoint>" +
                "</input>" +
                "<output>" +
                "<y>[Output]WeirdWeir/Crest level</y>" +
                $"<status>{AppendDefaultControlGroupName(RtcXmlTag.Status)}/Interval Test</status>" +
                "</output>" +
                "</interval>" +
                "</rule>" +
                "</rules>" +
                "<triggers>" +
                "<trigger>" +
                $"<standard id=\"{AppendDefaultControlGroupName(RtcXmlTag.StandardCondition)}/Trigger31\">" +
                "<condition>" +
                "<x1Series ref=\"IMPLICIT\">[Input]CondInputLocation/CondInputQuantityId</x1Series>" +
                "<relationalOperator>Greater</relationalOperator>" +
                "<x2Value>1.1</x2Value>" +
                "</condition>" +
                "<true>" +
                "<trigger>" +
                $"<ruleReference>{AppendDefaultControlGroupName(RtcXmlTag.IntervalRule)}/Interval Test</ruleReference>" +
                "</trigger>" +
                "</true>" +
                "<false>" +
                "<trigger>" +
                $"<ruleReference>{AppendDefaultControlGroupName(RtcXmlTag.IntervalRule)}/Interval Test</ruleReference>" +
                "</trigger>" +
                "</false>" +
                "<output>" +
                $"<status>{AppendDefaultControlGroupName(RtcXmlTag.Status)}/Trigger31</status>" +
                "</output>" +
                "</standard>" +
                "</trigger>" +
                "</triggers>" +
                "</rtcToolsConfig>";

            // Call
            XDocument xDocument = RealTimeControlXmlWriter.GetToolsConfigXml(XsdPath, new List<ControlGroup> {controlGroup});

            // Assert
            Assert.IsNotNull(xDocument);
            Assert.AreEqual(strPid, xDocument.ToString(SaveOptions.DisableFormatting));
        }

        [Test]
        public void PidRuleToolsConfigGenerationTest()
        {
            // Setup
            SetUpGlobalPidRuleForGlobalControlGroup();
            string header = "<rtcToolsConfig" + FewsXmlheader + RtcToolsConfigxsd + ">";
            string strPid = header +
                            "<general><description>RTC Model DeltaShell</description><poolRoutingScheme>Theta</poolRoutingScheme><theta>0.5</theta>" +
                            "</general><rules><rule><unitDelay id=\"PIDRule Test_unitDelay\"><input><x>[Output]WeirdWeir/Crest level</x>" +
                            "</input><output><y>[Output]WeirdWeir/Crest level</y>" +
                            "</output>" +
                            "</unitDelay>" +
                            $"</rule><rule><pid id=\"{AppendDefaultControlGroupName(RtcXmlTag.PIDRule)}/PIDRule Test\"><mode>PIDVEL</mode><settingMin>1.1</settingMin><settingMax>1.2</settingMax><settingMaxSpeed>1.3</settingMaxSpeed><kp>0.3</kp><ki>0.2</ki><kd>0.1</kd><input><x>[Input]MeasureStationA/Water level</x><setpointSeries>{AppendDefaultControlGroupName(RtcXmlTag.SP)}/PIDRule Test</setpointSeries>" +
                            $"</input><output><y>[Output]WeirdWeir/Crest level</y><integralPart>{AppendDefaultControlGroupName(RtcXmlTag.IP)}/PIDRule Test</integralPart><differentialPart>{AppendDefaultControlGroupName(RtcXmlTag.DP)}/PIDRule Test</differentialPart>" +
                            "</output>" +
                            "</pid>" +
                            "</rule>" +
                            $"</rules><triggers><trigger><standard id=\"{AppendDefaultControlGroupName(RtcXmlTag.StandardCondition)}/Trigger31\"><condition><x1Series ref=\"IMPLICIT\">[Input]CondInputLocation/CondInputQuantityId</x1Series><relationalOperator>Greater</relationalOperator><x2Value>1.1</x2Value>" +
                            $"</condition><true><trigger><ruleReference>{AppendDefaultControlGroupName(RtcXmlTag.PIDRule)}/PIDRule Test</ruleReference>" +
                            "</trigger>" +
                            $"</true><false><trigger><ruleReference>{AppendDefaultControlGroupName(RtcXmlTag.PIDRule)}/PIDRule Test</ruleReference>" +
                            "</trigger>" +
                            $"</false><output><status>{AppendDefaultControlGroupName(RtcXmlTag.Status)}/Trigger31</status>" +
                            "</output>" +
                            "</standard>" +
                            "</trigger>" +
                            "</triggers>" +
                            "</rtcToolsConfig>";

            // Call
            XDocument xDocument = RealTimeControlXmlWriter.GetToolsConfigXml(XsdPath, new List<ControlGroup> {controlGroup});

            // Assert
            Assert.IsNotNull(xDocument);
            Assert.AreEqual(strPid, xDocument.ToString(SaveOptions.DisableFormatting));
        }

        [Test]
        public void TwoPidRulesSameOutputToolsConfigGenerationTest()
        {
            // Setup
            SetUpTwoPidRulesSameOutput();
            string header = "<rtcToolsConfig" + FewsXmlheader + RtcToolsConfigxsd + ">";
            string strPid = header +
                            "<general><description>RTC Model DeltaShell</description><poolRoutingScheme>Theta</poolRoutingScheme><theta>0.5</theta>" +
                            "</general><rules><rule><unitDelay id=\"PIDRule Test_unitDelay\"><input><x>[Output]WeirdWeir/Crest level</x>" +
                            "</input><output><y>[Output]WeirdWeir/Crest level</y>" +
                            "</output>" +
                            "</unitDelay>" +
                            $"</rule><rule><pid id=\"{AppendDefaultControlGroupName(RtcXmlTag.PIDRule)}/PIDRule Test\"><mode>PIDVEL</mode><settingMin>1.1</settingMin><settingMax>1.2</settingMax><settingMaxSpeed>1.3</settingMaxSpeed><kp>0.3</kp><ki>0.2</ki><kd>0.1</kd><input><x>[Input]MeasureStationA/Water level</x><setpointSeries>{AppendDefaultControlGroupName(RtcXmlTag.SP)}/PIDRule Test</setpointSeries>" +
                            $"</input><output><y>[Output]WeirdWeir/Crest level</y><integralPart>{AppendDefaultControlGroupName(RtcXmlTag.IP)}/PIDRule Test</integralPart><differentialPart>{AppendDefaultControlGroupName(RtcXmlTag.DP)}/PIDRule Test</differentialPart>" +
                            "</output>" +
                            "</pid>" +
                            $"</rule><rule><pid id=\"{AppendDefaultControlGroupName(RtcXmlTag.PIDRule)}/PIDRule2 Test\"><mode>PIDVEL</mode><settingMin>1.1</settingMin><settingMax>1.2</settingMax><settingMaxSpeed>1.3</settingMaxSpeed><kp>0.3</kp><ki>0.2</ki><kd>0.1</kd><input><x>[Input]MeasureStationA/Water level</x><setpointSeries>{AppendDefaultControlGroupName(RtcXmlTag.SP)}/PIDRule2 Test</setpointSeries>" +
                            $"</input><output><y>[Output]WeirdWeir/Crest level</y><integralPart>{AppendDefaultControlGroupName(RtcXmlTag.IP)}/PIDRule2 Test</integralPart><differentialPart>{AppendDefaultControlGroupName(RtcXmlTag.DP)}/PIDRule2 Test</differentialPart>" +
                            "</output>" +
                            "</pid>" +
                            "</rule>" +
                            $"</rules><triggers><trigger><standard id=\"{AppendDefaultControlGroupName(RtcXmlTag.StandardCondition)}/Trigger31\"><condition><x1Series ref=\"IMPLICIT\">[Input]CondInputLocation/CondInputQuantityId</x1Series><relationalOperator>Greater</relationalOperator><x2Value>1.1</x2Value>" +
                            $"</condition><true><trigger><ruleReference>{AppendDefaultControlGroupName(RtcXmlTag.PIDRule)}/PIDRule Test</ruleReference>" +
                            "</trigger>" +
                            $"</true><false><trigger><ruleReference>{AppendDefaultControlGroupName(RtcXmlTag.PIDRule)}/PIDRule2 Test</ruleReference>" +
                            "</trigger>" +
                            $"</false><output><status>{AppendDefaultControlGroupName(RtcXmlTag.Status)}/Trigger31</status>" +
                            "</output>" +
                            "</standard>" +
                            "</trigger>" +
                            "</triggers>" +
                            "</rtcToolsConfig>";

            // Call
            XDocument xDocument = RealTimeControlXmlWriter.GetToolsConfigXml(XsdPath, new List<ControlGroup> {controlGroup});

            // Assert
            Assert.IsNotNull(xDocument);
            Assert.AreEqual(strPid, xDocument.ToString(SaveOptions.DisableFormatting));
        }

        /// <summary>
        /// Check if duplicate input items are handled well in the xml for data config
        /// note timeseries.xml will be empty
        /// </summary>
        [Test]
        [Category(TestCategory.Integration)]
        public void ValidateDataConfigFor2HydraulicRules()
        {
            IList<ControlGroup> controlGroups = CreateModelWithDuplicateInputOutputItems();

            var dataConfig = RealTimeControlXmlWriter
                             .GetDataConfigXml(XsdPath, realTimeControlModel, controlGroups, null).ToString();
            // generate the xml for data config
            XElement dataConfigComplexType = XElement.Parse(dataConfig);
            // parse the generated xml and check the number of input and output items
            IEnumerable<XElement> descendants = dataConfigComplexType.Descendants();
            Assert.AreEqual(2, descendants.Count(d => d.Name.ToString().Contains("OpenMIExchangeItem")));
            Assert.AreEqual(1, descendants.Count(d => d.Value.ToUpper().StartsWith("locationWater level".ToUpper()) &&
                                                      d.Name.ToString().Contains("OpenMIExchangeItem")));
            Assert.AreEqual(1, descendants.Count(d => d.Value.ToUpper().StartsWith("locationCrest level".ToUpper()) &&
                                                      d.Name.ToString().Contains("OpenMIExchangeItem")));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void ValidateGenerate2IdenticalHydraulicRulesAgainstXsds()
        {
            ControlGroup controlGroup1 = RealTimeControlModelHelper.CreateGroupHydraulicRule(true);
            ((HydraulicRule) controlGroup1.Rules[0]).Function[0.0] = -1.0; // empy lookupTable is not allowed
            RealTimeControlTestHelper.AddDummyLinksToGroup(controlGroup1);

            ControlGroup controlGroup2 = RealTimeControlModelHelper.CreateGroupHydraulicRule(true);
            ((HydraulicRule) controlGroup2.Rules[0]).Function[0.0] = -1.0; // empy lookupTable is not allowed
            RealTimeControlTestHelper.AddDummyLinksToGroup(controlGroup2);

            // As last step of the generation process an exception will be thrown if
            // validation against the internal xsd fails.
            RealTimeControlXmlWriter.GetToolsConfigXml(XsdPath,
                                                       new List<ControlGroup>
                                                       {
                                                           controlGroup1,
                                                           controlGroup2
                                                       });
            RealTimeControlXmlWriter.GetDataConfigXml(XsdPath, realTimeControlModel,
                                                      new List<ControlGroup>
                                                      {
                                                          controlGroup1,
                                                          controlGroup2
                                                      },
                                                      null);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void ValidateGenerateHydraulicRuleAgainstXsds()
        {
            ControlGroup controlGroup = RealTimeControlModelHelper.CreateGroupHydraulicRule(true);
            ((HydraulicRule) controlGroup.Rules[0]).Function[0.0] = -1.0; // empy lookupTable is not allowed
            RealTimeControlTestHelper.AddDummyLinksToGroup(controlGroup);
            // As last step of the generation process an exception will be thrown if
            // validation against the internal xsd fails.
            RealTimeControlXmlWriter.GetToolsConfigXml(XsdPath,
                                                       new List<ControlGroup> {controlGroup});
            RealTimeControlXmlWriter.GetDataConfigXml(XsdPath, realTimeControlModel,
                                                      new List<ControlGroup> {controlGroup},
                                                      null);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void ValidateGenerateHydraulicRuleWithoutTriggerAgainstXsds()
        {
            ControlGroup controlGroup = RealTimeControlModelHelper.CreateGroupHydraulicRule(true);
            ((HydraulicRule) controlGroup.Rules[0]).Function[0.0] = -1.0; // empy lookupTable is not allowed
            RealTimeControlTestHelper.AddDummyLinksToGroup(controlGroup);

            //delete conditions
            controlGroup.Conditions.Clear();

            //triggers element should not be generated
            RealTimeControlXmlWriter.GetToolsConfigXml(XsdPath,
                                                       new List<ControlGroup> {controlGroup});
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void ValidateGenerateIntervalRuleAgainstXsds()
        {
            ControlGroup controlGroup = RealTimeControlModelHelper.CreateGroupIntervalRule();

            RealTimeControlTestHelper.AddDummyLinksToGroup(controlGroup);
            // As last step of the generation process an exception will be thrown if
            // validation against the internal xsd fails.
            RealTimeControlXmlWriter.GetToolsConfigXml(XsdPath,
                                                       new List<ControlGroup> {controlGroup});
            RealTimeControlXmlWriter.GetDataConfigXml(XsdPath, realTimeControlModel,
                                                      new List<ControlGroup> {controlGroup},
                                                      null);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void ValidateGeneratePidRuleAgainstXsds()
        {
            ControlGroup controlGroup = RealTimeControlModelHelper.CreateGroupPidRule(true);

            RealTimeControlTestHelper.AddDummyLinksToGroup(controlGroup);
            // As last step of the generation process an exception will be thrown if
            // validation against the internal xsd fails.
            RealTimeControlXmlWriter.GetToolsConfigXml(XsdPath,
                                                       new List<ControlGroup> {controlGroup});
            RealTimeControlXmlWriter.GetDataConfigXml(XsdPath, realTimeControlModel,
                                                      new List<ControlGroup> {controlGroup},
                                                      null);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void ValidateGenerateRelativeTimeRuleAgainstXsds()
        {
            ControlGroup controlGroup = RealTimeControlModelHelper.CreateGroupRelativeTimeRule();
            ((RelativeTimeRule) controlGroup.Rules[0]).Function[0.0] = -1.0; // empy lookupTable is not allowed
            RealTimeControlTestHelper.AddDummyLinksToGroup(controlGroup);
            // As last step of the generation process an exception will be thrown if
            // validation against the internal xsd fails.
            RealTimeControlXmlWriter.GetToolsConfigXml(XsdPath,
                                                       new List<ControlGroup> {controlGroup});
            RealTimeControlXmlWriter.GetDataConfigXml(XsdPath, realTimeControlModel,
                                                      new List<ControlGroup> {controlGroup},
                                                      null);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void ValidateGenerateTimeRuleAgainstXsds()
        {
            ControlGroup controlGroup = RealTimeControlModelHelper.CreateGroupTimeRuleWithCondition();

            RealTimeControlTestHelper.AddDummyLinksToGroup(controlGroup);
            // As last step of the generation process an exception will be thrown if
            // validation against the internal xsd fails.
            RealTimeControlXmlWriter.GetToolsConfigXml(XsdPath,
                                                       new List<ControlGroup> {controlGroup});
            RealTimeControlXmlWriter.GetDataConfigXml(XsdPath, realTimeControlModel,
                                                      new List<ControlGroup> {controlGroup},
                                                      null);
        }

        private void SetUpGlobalPidRuleForGlobalControlGroup()
        {
            pidRule = GetNewSetUpPIDRule("PIDRule Test", input, output);

            controlGroup.Rules.Add(pidRule);
            condition.TrueOutputs.Add(pidRule);
            condition.FalseOutputs.Add(pidRule);

            controlGroup.Inputs.Add(input);
            controlGroup.Outputs.Add(output);
        }

        private void SetUpTwoPidRulesSameOutput()
        {
            pidRule = GetNewSetUpPIDRule("PIDRule Test", input, output);
            controlGroup.Rules.Add(pidRule);

            PIDRule pidRule2 = GetNewSetUpPIDRule("PIDRule2 Test", input, output);
            controlGroup.Rules.Add(pidRule2);

            condition.TrueOutputs.Add(pidRule);
            condition.FalseOutputs.Add(pidRule2);

            controlGroup.Inputs.Add(input);
            controlGroup.Outputs.Add(output);
        }

        private void SetUpIntervalRule()
        {
            intervalRule = new IntervalRule("Interval Test");
            intervalRule.Inputs.Add(input);
            intervalRule.Outputs.Add(output);

            intervalRule.DeadbandAroundSetpoint = 0.1;
            intervalRule.Setting = new Setting
            {
                Below = 0.2,
                Above = 0.3,
                MaxSpeed = 0.7
            };
            intervalRule.TimeSeries[new DateTime(2010, 1, 19, 12, 0, 0)] = 3.0;
            intervalRule.TimeSeries[new DateTime(2010, 1, 20, 12, 0, 0)] = 4.0;
            intervalRule.TimeSeries[new DateTime(2010, 1, 21, 12, 0, 0)] = 5.0;
            intervalRule.ConstantValue = 6;

            controlGroup.Rules.Add(intervalRule);

            controlGroup.Inputs.Add(input);
            controlGroup.Outputs.Add(output);
        }

        private void SetUpLookupSignalForPidRule()
        {
            var input2 = new Input
            {
                ParameterName = "Discharge",
                Feature = new RtcTestFeature {Name = "MeasureStationB"}
            };

            lookupSignal = new LookupSignal("SetPointForPID");
            lookupSignal.Inputs.Add(input2);
            lookupSignal.RuleBases.Add(pidRule);

            lookupSignal.Function[10.0] = 3.0;
            lookupSignal.Function[100.0] = 6.0;
            lookupSignal.Interpolation = InterpolationType.Linear;
            lookupSignal.Extrapolation = ExtrapolationType.Constant;

            controlGroup.Signals.Add(lookupSignal);
        }

        private void SetUpLookupSignalForIntervalRule()
        {
            var input2 = new Input
            {
                ParameterName = "Discharge",
                Feature = new RtcTestFeature {Name = "MeasureStationB"}
            };

            lookupSignal = new LookupSignal("SetPointForIntervalRule");
            lookupSignal.Inputs.Add(input2);
            lookupSignal.RuleBases.Add(intervalRule);

            lookupSignal.Function[10.0] = 3.0;
            lookupSignal.Function[100.0] = 6.0;
            lookupSignal.Interpolation = InterpolationType.Linear;
            lookupSignal.Extrapolation = ExtrapolationType.Constant;

            controlGroup.Signals.Add(lookupSignal);
        }

        private ControlGroup GetNewControlGroupWithNewPidRule(string pidRuleName)
        {
            var newInput = new Input();
            var newOutput = new Output();
            PIDRule newPidRule = GetNewSetUpPIDRule(pidRuleName, newInput, newOutput);

            var newCondition = new StandardCondition();
            newCondition.TrueOutputs.Add(newPidRule);
            newCondition.FalseOutputs.Add(newPidRule);

            var newControlGroup = new ControlGroup();
            newControlGroup.Rules.Add(newPidRule);
            newControlGroup.Inputs.Add(newInput);
            newControlGroup.Outputs.Add(newOutput);

            return newControlGroup;
        }

        private PIDRule GetNewSetUpPIDRule(string pidRuleName, Input inputForRule, Output outputForRule)
        {
            var newPidRule = new PIDRule(pidRuleName);
            newPidRule.Inputs.Add(inputForRule);
            newPidRule.Outputs.Add(outputForRule);

            newPidRule.Kd = 0.1;
            newPidRule.Ki = 0.2;
            newPidRule.Kp = 0.3;
            newPidRule.Setting = new Setting
            {
                Min = 1.1,
                Max = 1.2,
                MaxSpeed = 1.3
            };
            newPidRule.PidRuleSetpointType = PIDRule.PIDRuleSetpointType.TimeSeries;
            newPidRule.TimeSeries[new DateTime(2000, 1, 1, 0, 15, 30)] = 3.0;
            newPidRule.TimeSeries[new DateTime(2001, 2, 3, 4, 15, 45)] = 4.0;
            newPidRule.TimeSeries[new DateTime(2002, 3, 4, 5, 16, 0)] = 5.0;
            newPidRule.TimeSeries.Time.InterpolationType = InterpolationType.Linear;

            return newPidRule;
        }

        private void AddHydraulicRuleWithTimeLagToControlGroup()
        {
            var hydraulicRule = new HydraulicRule();
            hydraulicRule.Name = "HydraulicRule";
            hydraulicRule.TimeLag = 2000;
            hydraulicRule.SetTimeLagToTimeSteps(new TimeSpan(0, 0, 200));
            hydraulicRule.Function[0.0] = 0.0;

            //reset some values
            input.SetPoint = "";
            output.IntegralPart = "";

            hydraulicRule.Inputs.Add(input);
            hydraulicRule.Outputs.Add(output);

            condition.TrueOutputs.Add(hydraulicRule);
            condition.FalseOutputs.Add(hydraulicRule);

            controlGroup.Rules.Add(hydraulicRule);
            controlGroup.Outputs.Add(output);
            controlGroup.Inputs.Add(input);
        }

        private string DataResultXml(Input testInput, Output testOutput, bool addLookupSignal)
        {
            var inputSerializer = new InputSerializer(input);
            var outputSerializer = new OutputSerializer(testOutput);
            string result = "<rtcDataConfig" + FewsXmlheader + RtcDataConfigxsd + ">";
            result += "<importSeries>";
            result += "<timeSeries id=\"" + inputSerializer.GetXmlName() + "\">" +
                      "<OpenMIExchangeItem>" +
                      "<elementId>" + testInput.LocationName + "</elementId>" +
                      "<quantityId>" + input.ParameterName + "</quantityId>" +
                      "<unit>" + "m" + "</unit>" +
                      "</OpenMIExchangeItem>" +
                      "</timeSeries>" +
                      $"<timeSeries id=\"{AppendDefaultControlGroupName(RtcXmlTag.SP)}/PIDRule Test\">" +
                      "<PITimeSeries>" +
                      $"<locationId>{AppendDefaultControlGroupName(RtcXmlTag.PIDRule)}/PIDRule Test</locationId>" +
                      "<parameterId>SP</parameterId>" +
                      "<interpolationOption>LINEAR</interpolationOption>" +
                      "<extrapolationOption>BLOCK</extrapolationOption>" +
                      "</PITimeSeries>" +
                      "</timeSeries>";
            result += "</importSeries>";
            result += "<exportSeries>";
            result +=
                "<CSVTimeSeriesFile decimalSeparator=\".\" delimiter=\",\" adjointOutput=\"false\">" +
                "</CSVTimeSeriesFile>";
            result += "<PITimeSeriesFile>" +
                      "<timeSeriesFile>timeseries_export.xml</timeSeriesFile>" +
                      "<useBinFile>false</useBinFile>" +
                      "</PITimeSeriesFile>";
            result += "<timeSeries id=\"" + outputSerializer.GetXmlName() + "\">" +
                      "<OpenMIExchangeItem>" +
                      "<elementId>" + testOutput.LocationName + "</elementId>" +
                      "<quantityId>" + output.ParameterName + "</quantityId>" +
                      "<unit>" + "m" + "</unit>" +
                      "</OpenMIExchangeItem>" +
                      "</timeSeries>" +
                      $"<timeSeries id=\"{AppendDefaultControlGroupName(RtcXmlTag.Status)}/{condition.Name}" +
                      "\" />" +
                      //"<timeSeries id=\"" + pidRule.IntegralPart + "\" />" +
                      $"<timeSeries id=\"{AppendDefaultControlGroupName(RtcXmlTag.IP)}/PIDRule Test\" />" +
                      $"<timeSeries id=\"{AppendDefaultControlGroupName(RtcXmlTag.DP)}/PIDRule Test\" />";
            if (addLookupSignal)
            {
                result += $"<timeSeries id=\"{AppendDefaultControlGroupName(RtcXmlTag.Signal)}/SetPointForPID\" />";
            }

            result += "</exportSeries>";
            result += "</rtcDataConfig>";
            return result;
        }

        private List<XElement> GetxDocumentDescendantsForControlGroupListTimeSeries(string descendantsLocalName,
                                                                                    List<ControlGroup> controlGroupList)
        {
            XDocument xDocument;
            xDocument = RealTimeControlXmlWriter.GetTimeSeriesXml(XsdPath, realTimeControlModel, controlGroupList);
            Assert.IsNotNull(xDocument);

            List<XElement> descendantsWithLocalName =
                xDocument.Descendants().Where(d => d.Name.LocalName == descendantsLocalName).ToList();
            Assert.IsNotNull(descendantsWithLocalName);

            return descendantsWithLocalName;
        }

        private static IList<ControlGroup> CreateModelWithDuplicateInputOutputItems()
        {
            var controlGroups = new List<ControlGroup>();
            ControlGroup controlGroup1 = RealTimeControlModelHelper.CreateGroupHydraulicRule(true);
            ((HydraulicRule) controlGroup1.Rules[0]).Function[0.0] = -1.0; // empy lookupTable is not allowed
            controlGroup1.Rules[0].Name = "Rule1";
            controlGroup1.Conditions[0].Name = "Condition1";
            RealTimeControlTestHelper.AddDummyLinksToGroup(controlGroup1);
            controlGroups.Add(controlGroup1);

            ControlGroup controlGroup2 = RealTimeControlModelHelper.CreateGroupHydraulicRule(true);
            ((HydraulicRule) controlGroup2.Rules[0]).Function[0.0] = -1.0; // empy lookupTable is not allowed
            controlGroup2.Rules[0].Name = "Rule2";
            controlGroup2.Conditions[0].Name = "Condition2";
            RealTimeControlTestHelper.AddDummyLinksToGroup(controlGroup2);
            controlGroups.Add(controlGroup2);

            // all rules and condition now have as input an input item linked to: location Waterlevel
            // total of 4 input items, these 4 input items will result in only 1 exchange item (RTC)
            // all rules now have as output an output item: locationCrest level : 2 output items
            return controlGroups;
        }

        [Category(TestCategory.Integration)]
        [TestCase(IntervalRule.IntervalRuleIntervalType.Variable)]
        [TestCase(IntervalRule.IntervalRuleIntervalType.Fixed)]
        public void GivenAnIntervalRuleWithAVariableOrFixedSetPoint_WhenAskingForDataConfigFile_ThenAReferenceToATimeSerieShouldBeWritten(IntervalRule.IntervalRuleIntervalType intervalRuleIntervalType)
        {
            // Given
            SetUpIntervalRule();
            condition.TrueOutputs.Add(intervalRule);
            condition.FalseOutputs.Add(intervalRule);
            intervalRule.IntervalType = intervalRuleIntervalType;
            var controlGroupList = new List<ControlGroup> {controlGroup};
            var timeSeriesFileName = "timeseries_import.xml";

            // When
            XDocument xDocument = RealTimeControlXmlWriter.GetDataConfigXml(XsdPath, realTimeControlModel, controlGroupList, timeSeriesFileName);

            // Then
            Assert.IsNotNull(xDocument);

            string header = "<rtcDataConfig" + FewsXmlheader + RtcDataConfigxsd + ">";
            string strIntervalRule =
                header +
                $"<importSeries><PITimeSeriesFile><timeSeriesFile>{timeSeriesFileName}</timeSeriesFile><useBinFile>false</useBinFile></PITimeSeriesFile>" +
                "<timeSeries id=\"[Input]MeasureStationA/Water level\"><OpenMIExchangeItem><elementId>MeasureStationA</elementId><quantityId>Water level</quantityId><unit>m</unit>" +
                "</OpenMIExchangeItem>" +
                "</timeSeries>" +
                $"<timeSeries id=\"{AppendDefaultControlGroupName(RtcXmlTag.SP)}/{intervalRule.Name}\"><PITimeSeries><locationId>{AppendDefaultControlGroupName(RtcXmlTag.IntervalRule)}/{intervalRule.Name}</locationId><parameterId>SP</parameterId>" +
                "<interpolationOption>BLOCK</interpolationOption><extrapolationOption>BLOCK</extrapolationOption>" +
                "</PITimeSeries>" +
                "</timeSeries>" +
                "</importSeries>" +
                "<exportSeries><CSVTimeSeriesFile decimalSeparator=\".\" delimiter=\",\" adjointOutput=\"false\">" +
                "</CSVTimeSeriesFile><PITimeSeriesFile><timeSeriesFile>timeseries_export.xml</timeSeriesFile><useBinFile>false</useBinFile>" +
                "</PITimeSeriesFile><timeSeries id=\"[Output]WeirdWeir/Crest level\"><OpenMIExchangeItem><elementId>WeirdWeir</elementId><quantityId>Crest level</quantityId><unit>m</unit>" +
                "</OpenMIExchangeItem>" +
                $"</timeSeries><timeSeries id=\"{AppendDefaultControlGroupName(RtcXmlTag.Status)}/Trigger31\" /><timeSeries id=\"{AppendDefaultControlGroupName(RtcXmlTag.Status)}/{intervalRule.Name}\" />" +
                "</exportSeries>" +
                "</rtcDataConfig>";
            Assert.AreEqual(strIntervalRule, xDocument.ToString(SaveOptions.DisableFormatting));
        }

        private static string AppendDefaultControlGroupName(string tag)
        {
            return $"{tag}Control Group";
        }
    }
}