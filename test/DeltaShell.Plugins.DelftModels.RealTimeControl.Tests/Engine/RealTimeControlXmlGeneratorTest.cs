using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using DelftTools.Functions.Generic;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport;
using DeltaShell.Plugins.DelftModels.RealTimeControl.rtc_kernel;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils.Domain;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Engine
{
    [TestFixture]
    public class RealTimeControlXmlGeneratorTest
    {
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
        private string XsdPath
        {
            get
            {
                return RealTimeControlModelDll.DllPath;
            }
        }

        private const string FewsXmlheader = " xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"" +
                                         " xmlns:rtc=\"http://www.wldelft.nl/fews\"" +
                                         " xmlns=\"http://www.wldelft.nl/fews\"" +
                                         " xsi:schemaLocation=\"" +
                                         @"http://www.wldelft.nl/fews ";//\xsd\";

        private const string PiXmlheader = " xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"" +
                                         // no rtc namespace necessary
                                         " xmlns=\"http://www.wldelft.nl/fews/PI\"" +
                                         " xsi:schemaLocation=\"" +
                                         @"http://www.wldelft.nl/fews/PI ";//\xsd\";

        private string RtcToolsConfigxsd
        {
            get
            {
                return XsdPath + Path.DirectorySeparatorChar + "rtcToolsConfig.xsd\"";
            }
        }
        private string RtcDataConfigxsd
        {
            get
            {
                return XsdPath + Path.DirectorySeparatorChar + "rtcDataConfig.xsd\"";
            }
        }
        private string RtcRuntimeConfigxsd
        {
            get
            {
                return XsdPath + Path.DirectorySeparatorChar + "rtcRuntimeConfig.xsd\"";
            }
        }
        private string PiTimeSeriesxsd
        {
            get
            {
                return XsdPath + Path.DirectorySeparatorChar + "pi_timeseries.xsd\"";
            }
        }

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
                Feature = new RtcTestFeature { Name = "MeasureStationA" },
                SetPoint = "PIDRule Test_SP"
            };

            output = new Output
            {
                ParameterName = "Crest level",
                Feature = new RtcTestFeature { Name = "WeirdWeir" },
                IntegralPart = "PIDRule Test_IP"
            };

            condition = new StandardCondition
            {
                Name = "Trigger31",
                Reference = "IMPLICIT",
                Operation = Operation.Greater,
                Input = new Input
                {
                    ParameterName = "CondInputQuantityId",
                    Feature = new RtcTestFeature { Name = "CondInputLocation" },
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
                    Feature = new RtcTestFeature { Name = "CondInputLocation" },
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
                    Feature = new RtcTestFeature { Name = "CondInputLocation" },
                },
                Value = 0.5
            };
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

            var pidRule2 = GetNewSetUpPIDRule("PIDRule2 Test", input, output);
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
            intervalRule.Setting = new Setting { Below = 0.2, Above = 0.3, MaxSpeed = 0.7 };
            intervalRule.TimeSeries[new DateTime(2010, 1, 19, 12, 0, 0)] = 3.0;
            intervalRule.TimeSeries[new DateTime(2010, 1, 20, 12, 0, 0)] = 4.0;
            intervalRule.TimeSeries[new DateTime(2010, 1, 21, 12, 0, 0)] = 5.0;

            controlGroup.Rules.Add(intervalRule);

            controlGroup.Inputs.Add(input);
            controlGroup.Outputs.Add(output);
        }

        private void SetUpLookupSignal()
        {
            var input2 = new Input
            {
                ParameterName = "Discharge",
                Feature = new RtcTestFeature { Name = "MeasureStationB" },
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

        private ControlGroup GetNewControlGroupWithNewPidRule(string pidRuleName)
        {
            var newInput = new Input();
            var newOutput = new Output();
            var newPidRule = GetNewSetUpPIDRule(pidRuleName, newInput, newOutput);

            var newCondition = new StandardCondition();
            newCondition.TrueOutputs.Add(newPidRule);
            newCondition.FalseOutputs.Add(newPidRule);

            var newControlGroup = new ControlGroup();
            newControlGroup.Rules.Add(newPidRule);
            newControlGroup.Inputs.Add(newInput);
            newControlGroup.Outputs.Add(newOutput);

            return newControlGroup;
        }

        private PIDRule GetNewSetUpPIDRule(String pidRuleName, Input inputForRule, Output outputForRule)
        {
            var newPidRule = new PIDRule(pidRuleName);
            newPidRule.Inputs.Add(inputForRule);
            newPidRule.Outputs.Add(outputForRule);

            newPidRule.Kd = 0.1;
            newPidRule.Ki = 0.2;
            newPidRule.Kp = 0.3;
            newPidRule.Setting = new Setting {Min = 1.1, Max = 1.2, MaxSpeed = 1.3};
            newPidRule.PidRuleSetpointType = PIDRule.PIDRuleSetpointType.TimeSeries;
            newPidRule.TimeSeries[new DateTime(2010, 1, 19, 12, 0, 0)] = 3.0;
            newPidRule.TimeSeries[new DateTime(2010, 1, 20, 12, 0, 0)] = 4.0;
            newPidRule.TimeSeries[new DateTime(2010, 1, 21, 12, 0, 0)] = 5.0;
            newPidRule.TimeSeries.Time.InterpolationType = InterpolationType.Linear;

            return newPidRule;
        }

        [Test]
        public void PidRuleToolsConfigGenerationTest()
        {
            SetUpGlobalPidRuleForGlobalControlGroup();
            var header = "<rtcToolsConfig" + FewsXmlheader + RtcToolsConfigxsd + ">";
            string strPid = header +
                          "<general>" +
                          "<description>RTC Model DeltaShell</description>" +
                          "<poolRoutingScheme>Theta</poolRoutingScheme>" +
                          "<theta>0.5</theta>" +
                          "</general>" +
                          "<rules>" +
                          "<rule>" +
                          "<unitDelay id=\"PIDRule Test_unitDelay\">" +
                          "<input>" +
                          "<x>output_WeirdWeir_Crest level</x>" +
                          "</input>" +
                          "<output>" +
                          "<y>output_WeirdWeir_Crest level</y>" +
                          "</output>" +
                          "</unitDelay>" +
                          "</rule>" +
                          "<rule>" +
                          "<pid id=\"PIDRule Test\">" +
                          "<mode>PIDVEL</mode>" +
                          "<settingMin>1.1</settingMin>" +
                          "<settingMax>1.2</settingMax>" +
                          "<settingMaxSpeed>1.3</settingMaxSpeed>" +
                          "<kp>0.3</kp>" +
                          "<ki>0.2</ki>" +
                          "<kd>0.1</kd>" +
                          "<input>" +
                          "<x>input_MeasureStationA_Water level</x>" +
                          "<setpointSeries>PIDRule Test_SP</setpointSeries>" +
                          "</input>" +
                          "<output>" +
                          "<y>output_WeirdWeir_Crest level</y>" +
                          "<integralPart>PIDRule Test_IP</integralPart>" +
                          "<differentialPart>PIDRule Test_DP</differentialPart>" +
                          "</output>" +
                          "</pid>" +
                          "</rule>" +
                          "</rules>" +
                          "<triggers>" +
                          "<trigger>" +
                          "<standard id=\"Trigger31\">" +
                          "<condition>" +
                          "<x1Series ref=\"IMPLICIT\">input_CondInputLocation_CondInputQuantityId</x1Series>" +
                          "<relationalOperator>Greater</relationalOperator>" +
                          "<x2Value>1.1</x2Value>" +
                          "</condition>" +
                          "<true>" +
                          "<trigger>" +
                          "<ruleReference>PIDRule Test</ruleReference>" +
                          "</trigger>" +
                          "</true>" +
                          "<false>" +
                          "<trigger>" +
                          "<ruleReference>PIDRule Test</ruleReference>" +
                          "</trigger>" +
                          "</false>" +
                          "<output>" +
                          "<status>Status_Trigger31</status>" +
                          "</output>" +
                          "</standard>" +
                          "</trigger>" +
                          "</triggers>" +
                          "</rtcToolsConfig>";
            var xDocument = RealTimeControlXmlWriter.GetToolsConfigXml(XsdPath, new List<ControlGroup> { controlGroup });
            Assert.IsNotNull(xDocument);
            Assert.AreEqual(strPid, xDocument.ToString(SaveOptions.DisableFormatting));
        }

        [Test]
        public void TwoPidRulesSameOutputToolsConfigGenerationTest()
        {
            SetUpTwoPidRulesSameOutput();
            var header = "<rtcToolsConfig" + FewsXmlheader + RtcToolsConfigxsd + ">";
            string strPid = header +
                            "<general>" +
                            "<description>RTC Model DeltaShell</description>" +
                            "<poolRoutingScheme>Theta</poolRoutingScheme>" +
                            "<theta>0.5</theta>" +
                            "</general>" +
                            "<rules>" +
                            "<rule>" +
                            "<unitDelay id=\"PIDRule Test_unitDelay\">" +
                            "<input>" +
                            "<x>output_WeirdWeir_Crest level</x>" +
                            "</input>" +
                            "<output>" +
                            "<y>output_WeirdWeir_Crest level</y>" +
                            "</output>" +
                            "</unitDelay>" +
                            "</rule>" +
                            "<rule>" +
                            "<pid id=\"PIDRule Test\">" +
                            "<mode>PIDVEL</mode>" +
                            "<settingMin>1.1</settingMin>" +
                            "<settingMax>1.2</settingMax>" +
                            "<settingMaxSpeed>1.3</settingMaxSpeed>" +
                            "<kp>0.3</kp>" +
                            "<ki>0.2</ki>" +
                            "<kd>0.1</kd>" +
                            "<input>" +
                            "<x>input_MeasureStationA_Water level</x>" +
                            "<setpointSeries>PIDRule Test_SP</setpointSeries>" +
                            "</input>" +
                            "<output>" +
                            "<y>output_WeirdWeir_Crest level</y>" +
                            "<integralPart>PIDRule Test_IP</integralPart>" +
                            "<differentialPart>PIDRule Test_DP</differentialPart>" +
                            "</output>" +
                            "</pid>" +
                            "</rule>" +
                            "<rule>" +
                            "<pid id=\"PIDRule2 Test\">" +
                            "<mode>PIDVEL</mode>" +
                            "<settingMin>1.1</settingMin>" +
                            "<settingMax>1.2</settingMax>" +
                            "<settingMaxSpeed>1.3</settingMaxSpeed>" +
                            "<kp>0.3</kp>" +
                            "<ki>0.2</ki>" +
                            "<kd>0.1</kd>" +
                            "<input>" +
                            "<x>input_MeasureStationA_Water level</x>" +
                            "<setpointSeries>PIDRule2 Test_SP</setpointSeries>" +
                            "</input>" +
                            "<output>" +
                            "<y>output_WeirdWeir_Crest level</y>" +
                            "<integralPart>PIDRule2 Test_IP</integralPart>" +
                            "<differentialPart>PIDRule2 Test_DP</differentialPart>" +
                            "</output>" +
                            "</pid>" +
                            "</rule>" +
                            "</rules>" +
                            "<triggers>" +
                            "<trigger>" +
                            "<standard id=\"Trigger31\">" +
                            "<condition>" +
                            "<x1Series ref=\"IMPLICIT\">input_CondInputLocation_CondInputQuantityId</x1Series>" +
                            "<relationalOperator>Greater</relationalOperator>" +
                            "<x2Value>1.1</x2Value>" +
                            "</condition>" +
                            "<true>" +
                            "<trigger>" +
                            "<ruleReference>PIDRule Test</ruleReference>" +
                            "</trigger>" +
                            "</true>" +
                            "<false>" +
                            "<trigger>" +
                            "<ruleReference>PIDRule2 Test</ruleReference>" +
                            "</trigger>" +
                            "</false>" +
                            "<output>" +
                            "<status>Status_Trigger31</status>" +
                            "</output>" +
                            "</standard>" +
                            "</trigger>" +
                            "</triggers>" +
                            "</rtcToolsConfig>";
            var xDocument = RealTimeControlXmlWriter.GetToolsConfigXml(XsdPath, new List<ControlGroup> { controlGroup });
            Assert.IsNotNull(xDocument);
            Assert.AreEqual(strPid, xDocument.ToString(SaveOptions.DisableFormatting));
        }

        [Test]
        public void IntervalRuleToolsConfigGenerationTest()
        {
            SetUpIntervalRule();

            condition.TrueOutputs.Add(intervalRule);
            condition.FalseOutputs.Add(intervalRule);

            var header = "<rtcToolsConfig" + FewsXmlheader + RtcToolsConfigxsd + ">";
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
                "<x>output_WeirdWeir_Crest level</x>" +
                "</input>" +
                "<output>" +
                "<y>output_WeirdWeir_Crest level</y>" +
                "</output>" +
                "</unitDelay>" +
                "</rule>" +
                "<rule>" +
                "<interval id=\"Interval Test\">" +
                "<settingBelow>0.2</settingBelow>" +
                "<settingAbove>0.3</settingAbove>" +
                "<settingMaxStep>0</settingMaxStep>" +
                "<deadbandSetpointAbsolute>0.1</deadbandSetpointAbsolute>" +
                "<input>" +
                "<x ref=\"EXPLICIT\">input_MeasureStationA_Water level</x>" +
                "<setpoint>Interval Test_SP</setpoint>" +
                "</input>" +
                "<output>" +
                "<y>output_WeirdWeir_Crest level</y>" +
                "<status>Interval Test_status</status>" + 
                "</output>" +
                "</interval>" +
                "</rule>" +
                "</rules>" +
                "<triggers>" +
                "<trigger>" +
                "<standard id=\"Trigger31\">" +
                "<condition>" +
                "<x1Series ref=\"IMPLICIT\">input_CondInputLocation_CondInputQuantityId</x1Series>" +
                "<relationalOperator>Greater</relationalOperator>" +
                "<x2Value>1.1</x2Value>" +
                "</condition>" +
                "<true>" +
                "<trigger>" +
                "<ruleReference>Interval Test</ruleReference>" +
                "</trigger>" +
                "</true>" +
                "<false>" +
                "<trigger>" +
                "<ruleReference>Interval Test</ruleReference>" +
                "</trigger>" +
                "</false>" +
                "<output>" +
                "<status>Status_Trigger31</status>" +
                "</output>" +
                "</standard>" +
                "</trigger>" +
                "</triggers>" +
                "</rtcToolsConfig>";
            var xDocument = RealTimeControlXmlWriter.GetToolsConfigXml(XsdPath, new List<ControlGroup> { controlGroup });
            Assert.IsNotNull(xDocument);
            Assert.AreEqual(strPid, xDocument.ToString(SaveOptions.DisableFormatting));
        }


        [Test]
        public void HydraulicRuleWithTimeLagToolsConfigGenerationTest()
        {
            HydraulicRule hydraulicRule = GetHydraulicRuleWithTimeLagAddedToControlGroup();

            var index = hydraulicRule.TimeLagInTimeSteps - 2;

            var header = "<rtcToolsConfig" + FewsXmlheader + RtcToolsConfigxsd + ">";
            string strHydraulicRuleTimeLag =
                header +
                "<general>" +
                "<description>RTC Model DeltaShell</description>" +
                "<poolRoutingScheme>Theta</poolRoutingScheme>" +
                "<theta>0.5</theta>" +
                "</general>" +
                "<components>" +
                "<component>" +
                "<unitDelay id=\"input_MeasureStationA_Water levelDelay\">" +
                "<input>" +
                "<x>input_MeasureStationA_Water level</x>" +
                "</input>" +
                "<output>" +
                "<yVector>delayedinput_MeasureStationA_Water level</yVector>" +
                "</output>" +
                "</unitDelay>" +
                "</component>" +
                "</components>" +
                "<rules>" +
                "<rule>" +
                "<lookupTable id=\"HydraulicRule\">" +
                "<table>" +
                "<record x=\"0\" y=\"0\" />" +
                "</table>" +
                "<interpolationOption>BLOCK</interpolationOption>" +
                "<extrapolationOption>BLOCK</extrapolationOption>" +
                "<input>" +
                "<x ref=\"EXPLICIT\">delayedinput_MeasureStationA_Water level[" + index + "]</x>" +
                "</input>" +
                "<output>" +
                "<y>output_WeirdWeir_Crest level</y>" +
                "</output>" +
                "</lookupTable>" +
                "</rule>" +
                "</rules>" +
                "<triggers>" +
                "<trigger>" +
                "<standard id=\"Trigger31\">" +
                "<condition>" +
                "<x1Series ref=\"IMPLICIT\">input_CondInputLocation_CondInputQuantityId</x1Series>" +
                "<relationalOperator>Greater</relationalOperator>" +
                "<x2Value>1.1</x2Value>" +
                "</condition>" +
                "<true>" +
                "<trigger>" +
                "<ruleReference>HydraulicRule</ruleReference>" +
                "</trigger>" +
                "</true>" +
                "<false>" +
                "<trigger>" +
                "<ruleReference>HydraulicRule</ruleReference>" +
                "</trigger>" +
                "</false>" +
                "<output>" +
                "<status>Status_Trigger31</status>" +
                "</output>" +
                "</standard>" +
                "</trigger>" +
                "</triggers>" +
                "</rtcToolsConfig>";
            var xDocument = RealTimeControlXmlWriter.GetToolsConfigXml(XsdPath, new List<ControlGroup> { controlGroup });
            Assert.IsNotNull(xDocument);
            Assert.AreEqual(strHydraulicRuleTimeLag, xDocument.ToString(SaveOptions.DisableFormatting));
        }


        [Test]
        public void HydraulicRuleWithTimeLagDataConfigGenerationTest()
        {
            HydraulicRule hydraulicRule = GetHydraulicRuleWithTimeLagAddedToControlGroup();

            var index = hydraulicRule.TimeLagInTimeSteps;
            var length = (index - 1);

            var header = "<rtcDataConfig" + FewsXmlheader + RtcDataConfigxsd + ">";
            string strDataConfigWithHydraulicRuleTimeLag =
                header +
                "<importSeries><timeSeries id=\"input_MeasureStationA_Water level\"><OpenMIExchangeItem><elementId>MeasureStationA</elementId><quantityId>Water level</quantityId><unit>m</unit></OpenMIExchangeItem></timeSeries></importSeries>" +
                "<exportSeries><CSVTimeSeriesFile decimalSeparator=\".\" delimiter=\",\" adjointOutput=\"false\"></CSVTimeSeriesFile><PITimeSeriesFile><timeSeriesFile>timeseries_export.xml</timeSeriesFile><useBinFile>false</useBinFile></PITimeSeriesFile><timeSeries id=\"output_WeirdWeir_Crest level\"><OpenMIExchangeItem><elementId>WeirdWeir</elementId><quantityId>Crest level</quantityId><unit>m</unit></OpenMIExchangeItem></timeSeries><timeSeries id=\"Status_Trigger31\" />" +
                "<timeSeries id=\"delayedinput_MeasureStationA_Water level\" vectorLength=\"" + length +
                "\"><PITimeSeries><locationId>MeasureStationA</locationId><parameterId>Water level</parameterId></PITimeSeries></timeSeries>" +
                "</exportSeries></rtcDataConfig>";

            var xDocument = RealTimeControlXmlWriter.GetDataConfigXml(XsdPath, realTimeControlModel,
                                                         new List<ControlGroup> { controlGroup },
                                                         null);
            Assert.IsNotNull(xDocument);
            string actual = xDocument.ToString(SaveOptions.DisableFormatting);
            Assert.AreEqual(strDataConfigWithHydraulicRuleTimeLag, actual);
        }

        private HydraulicRule GetHydraulicRuleWithTimeLagAddedToControlGroup()
        {
            var hydraulicRule = new HydraulicRule();
            hydraulicRule.Name = "HydraulicRule";
            hydraulicRule.TimeLag = 2000;
            hydraulicRule.SetTimeLagToTimeSteps(new TimeSpan(0,0,200));
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

            return hydraulicRule;
        }


        [Test]
        public void GetToolsDataXmlOnePIDRule()
        {
            SetUpGlobalPidRuleForGlobalControlGroup();
            var strOutputXml = DataResultXml(input, output, false);

            var xDocument = RealTimeControlXmlWriter.GetDataConfigXml(XsdPath, realTimeControlModel, new List<ControlGroup> { controlGroup }, null);
            Assert.IsNotNull(xDocument);
            Assert.AreEqual(strOutputXml, xDocument.ToString(SaveOptions.DisableFormatting));
        }

        private string DataResultXml(ConnectionPoint testInput, ConnectionPoint testOutput, bool addLookupSignal)
        {
            var result = "<rtcDataConfig"+ FewsXmlheader + RtcDataConfigxsd +">";
            result += "<importSeries>";
            result += "<timeSeries id=\"input_" + testInput.Name + "\">" +
                      "<OpenMIExchangeItem>" +
                      "<elementId>" + testInput.LocationName + "</elementId>" +
                      "<quantityId>" + input.ParameterName + "</quantityId>" +
                      "<unit>" + "m" + "</unit>" +
                      "</OpenMIExchangeItem>" +
                      "</timeSeries>" +
                      "<timeSeries id=\"PIDRule Test_SP\">" +
                      "<PITimeSeries>" +
                      "<locationId>PIDRule Test</locationId>" +
                      "<parameterId>SP</parameterId>" +
                        "<interpolationOption>LINEAR</interpolationOption>" +
                        "<extrapolationOption>BLOCK</extrapolationOption>" +
                      "</PITimeSeries>" +
                      "</timeSeries>";
            result += "</importSeries>";
            result += "<exportSeries>";
            result += "<CSVTimeSeriesFile decimalSeparator=\".\" delimiter=\",\" adjointOutput=\"false\"></CSVTimeSeriesFile>";
            result += "<PITimeSeriesFile>" +
                      "<timeSeriesFile>timeseries_export.xml</timeSeriesFile>" +
                      "<useBinFile>false</useBinFile>" +
                      "</PITimeSeriesFile>";
            result += "<timeSeries id=\"output_" + testOutput.Name + "\">" +
                      "<OpenMIExchangeItem>" +
                      "<elementId>" + testOutput.LocationName + "</elementId>" +
                      "<quantityId>" + output.ParameterName + "</quantityId>" +
                      "<unit>" + "m" + "</unit>" +
                      "</OpenMIExchangeItem>" +
                      "</timeSeries>" +
                      "<timeSeries id=\"" + condition.StatusOutputSeriesName + "\" />" +
                      //"<timeSeries id=\"" + pidRule.IntegralPart + "\" />" +
                      "<timeSeries id=\"PIDRule Test_IP\" />" +
                      "<timeSeries id=\"PIDRule Test_DP\" />";
            if (addLookupSignal)
            {
                result += "<timeSeries id=\"SetPointForPID\" />";
            }
            result += "</exportSeries>";
            result += "</rtcDataConfig>";
            return result;
        }

        [Test]
        public void GetRunTimeConfig()
        {
            var strOutputXml = "<rtcRuntimeConfig" + FewsXmlheader + RtcRuntimeConfigxsd + ">" +
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
                               "<timeStep unit=\"hour\" multiplier=\"" + "7" /*realTimeControlModel.TimeStep.Minutes*/+
                               "\" divider=\"1\" />" + //= optional "<numberEnsembles>1</numberEnsembles>" + 
                               "</userDefined>" +
                               "</period>" +
                               "<mode>" +
                               "<simulation>" +
                               "<limitedMemory>false</limitedMemory>" +
                               "</simulation>" +
                               "</mode>" +
                               "</rtcRuntimeConfig>";
            var xDocument = RealTimeControlXmlWriter.GetRuntimeXml(XsdPath, realTimeControlModel, false, 1);
            Assert.IsNotNull(xDocument);
            Assert.AreEqual(strOutputXml, xDocument.ToString(SaveOptions.DisableFormatting));
        }

        [Test]
        public void GetRunTimeConfigIncludingLogging()
        {
            var strOutputXml = "<rtcRuntimeConfig" + FewsXmlheader + RtcRuntimeConfigxsd + ">" +
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
                               "<timeStep unit=\"hour\" multiplier=\"" + "7" /*realTimeControlModel.TimeStep.Minutes*/+
                               "\" divider=\"1\" />" + //= optional "<numberEnsembles>1</numberEnsembles>" + 
                               "</userDefined>" +
                               "</period>" +
                               "<mode>" +
                               "<simulation>" +
                               "<limitedMemory>false</limitedMemory>" +
                               "</simulation>" +
                               "</mode>" +
                               "<logging>" +
                               "<logLevel>4</logLevel>" +
                               "<flushing>true</flushing>" +
                               "</logging>" +
                               "</rtcRuntimeConfig>";
            var xDocument = RealTimeControlXmlWriter.GetRuntimeXml(XsdPath, realTimeControlModel, false, 4);
            Assert.IsNotNull(xDocument);
            Assert.AreEqual(strOutputXml, xDocument.ToString(SaveOptions.DisableFormatting));
        }

        [Test]
        public void GetSTimeSeries()
        {
            SetUpGlobalPidRuleForGlobalControlGroup();
            // preferred minimal coding in test string to avoid missing
            string piTimeSeries =
                "<TimeSeries" + PiXmlheader + PiTimeSeriesxsd + " version=\"1.2\">" +
                "<series>" +
                "<header>" +
                "<type>instantaneous</type>" +
                "<locationId>PIDRule Test</locationId>" +
                "<parameterId>SP</parameterId>" +
                "<timeStep unit=\"hour\" multiplier=\"7\" divider=\"1\" />" +
                "<startDate date=\"2010-01-19\" time=\"12:00:00\" />" +
                "<endDate date=\"2010-01-21\" time=\"12:00:00\" />" +
                "<missVal>-999.0</missVal>" +
                "<stationName />" +
                "<units />" +
                "</header>" +
                "<event date=\"2010-01-19\" time=\"12:00:00\" value=\"3\" />" +
                "<event date=\"2010-01-20\" time=\"12:00:00\" value=\"4\" />" +
                "<event date=\"2010-01-21\" time=\"12:00:00\" value=\"5\" />" +
                "</series>" +
                "</TimeSeries>";

            var xDocument = RealTimeControlXmlWriter.GetTimeSeriesXml(XsdPath, realTimeControlModel, new List<ControlGroup> { controlGroup });
            Assert.IsNotNull(xDocument);
            Assert.AreEqual(piTimeSeries, xDocument.ToString(SaveOptions.DisableFormatting));
        }

        [Test]
        public void GetSTimeSeriesReturnsNullWhenSetPointIsConstantTest()
        {
            SetUpGlobalPidRuleForGlobalControlGroup();
            //SOBEK3-1074: If set point has been set to constant PID Controller should not write set time.
            pidRule.PidRuleSetpointType = PIDRule.PIDRuleSetpointType.Constant;
            //Because it's constant and there are no more rules nothing should be written.
            var controlGroupList = new List<ControlGroup> { controlGroup };
            var xDocument = RealTimeControlXmlWriter.GetTimeSeriesXml(XsdPath, realTimeControlModel, controlGroupList);
            Assert.IsNull(xDocument);

            //When changed to time series it should be valid, thus written.
            pidRule.PidRuleSetpointType = PIDRule.PIDRuleSetpointType.TimeSeries;
            var descendantsWithLocalName = GetxDocumentDescendantsForControlGroupListTimeSeries("locationId", controlGroupList);
            Assert.AreEqual( 1, descendantsWithLocalName.Count);
            Assert.AreEqual(pidRule.Name, descendantsWithLocalName[0].Value);
        }

        [Test]
        public void GetSTimeSeriesReturnsDocumentWhenSetPointIsConstantOnlyInOneRuleTest()
        {
            SetUpGlobalPidRuleForGlobalControlGroup();
            var pidrule02TestName = "PIDRule02 Test";
            var secondControlGroup = GetNewControlGroupWithNewPidRule(pidrule02TestName);
            var controlGroupList = new List<ControlGroup>(){ controlGroup, secondControlGroup};

            //SOBEK3-1074: If set point has been set to constant PID Controller should not write set time.
            pidRule.PidRuleSetpointType = PIDRule.PIDRuleSetpointType.Constant;
            
            //Only one of the rules is constant, the document should still be written with the values of the second.
            var descendantsWithLocalName = GetxDocumentDescendantsForControlGroupListTimeSeries("locationId", controlGroupList);
            Assert.AreEqual(1, descendantsWithLocalName.Count);
            Assert.AreEqual(pidrule02TestName, descendantsWithLocalName[0].Value); /* only the PidRule frome the second control group*/

            /*Set both to time series, there should be two nodes now*/
            pidRule.PidRuleSetpointType = PIDRule.PIDRuleSetpointType.TimeSeries;
            descendantsWithLocalName = GetxDocumentDescendantsForControlGroupListTimeSeries("locationId", controlGroupList);
            Assert.AreEqual(2, descendantsWithLocalName.Count);

            var valuesInNodes = descendantsWithLocalName.Select(d => d.Value).ToList();
            Assert.IsTrue(valuesInNodes.Contains(pidRule.Name));
            Assert.IsTrue(valuesInNodes.Contains(pidrule02TestName));
        }

        [Test]
        public void GetSTimeSeriesReturnsDocumentWhenTwoRulesInAControlGroupAndOneSetPointIsConstantTest()
        {
            SetUpTwoPidRulesSameOutput();
            pidRule.PidRuleSetpointType = PIDRule.PIDRuleSetpointType.Constant;
            var pidrule02 = controlGroup.Rules.FirstOrDefault(r => r != pidRule);
            Assert.NotNull(pidrule02);
            var pidrule02TestName = pidrule02.Name;

            //The document should be written but the constant one will be excluded
            var controlGroupList = new List<ControlGroup> { controlGroup };
            var descendantsWithLocalName = GetxDocumentDescendantsForControlGroupListTimeSeries("locationId", controlGroupList);
            Assert.AreEqual(1, descendantsWithLocalName.Count);
            Assert.AreNotEqual(pidRule.Name, descendantsWithLocalName[0].Value);
            Assert.AreEqual( controlGroup.Rules[1].Name, descendantsWithLocalName[0].Value);

            /*Set both to time series, there should be two nodes now*/
            pidRule.PidRuleSetpointType = PIDRule.PIDRuleSetpointType.TimeSeries;
            descendantsWithLocalName = GetxDocumentDescendantsForControlGroupListTimeSeries("locationId", controlGroupList);
            Assert.AreEqual(2, descendantsWithLocalName.Count);

            var valuesInNodes = descendantsWithLocalName.Select(d => d.Value).ToList();
            Assert.IsTrue(valuesInNodes.Contains(pidRule.Name));
            Assert.IsTrue(valuesInNodes.Contains(pidrule02TestName));

        }

        private List<XElement> GetxDocumentDescendantsForControlGroupListTimeSeries(string descendantsLocalName, List<ControlGroup> controlGroupList)
        {
            XDocument xDocument;
            xDocument = RealTimeControlXmlWriter.GetTimeSeriesXml(XsdPath, realTimeControlModel, controlGroupList);
            Assert.IsNotNull(xDocument);

            var descendantsWithLocalName = xDocument.Descendants().Where(d => d.Name.LocalName == descendantsLocalName).ToList();
            Assert.IsNotNull(descendantsWithLocalName);

            return descendantsWithLocalName;
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void ValidateGenerateHydraulicRuleAgainstXsds()
        {
            var controlledModel = new ControlledTestModel();
            var controlGroup = RealTimeControlModelHelper.CreateGroupHydraulicRule(true);
            ((HydraulicRule)controlGroup.Rules[0]).Function[0.0] = -1.0;  // empy lookupTable is not allowed
            RealTimeControlTestHelper.AddDummyLinksToGroup(controlledModel, controlGroup);
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
        public void ValidateGenerate2IdenticalHydraulicRulesAgainstXsds()
        {
            var controlledModel = new ControlledTestModel();
            var controlGroup1 = RealTimeControlModelHelper.CreateGroupHydraulicRule(true);
            ((HydraulicRule)controlGroup1.Rules[0]).Function[0.0] = -1.0;  // empy lookupTable is not allowed
            RealTimeControlTestHelper.AddDummyLinksToGroup(controlledModel, controlGroup1);

            var controlGroup2 = RealTimeControlModelHelper.CreateGroupHydraulicRule(true);
            ((HydraulicRule)controlGroup2.Rules[0]).Function[0.0] = -1.0;  // empy lookupTable is not allowed
            RealTimeControlTestHelper.AddDummyLinksToGroup(controlledModel, controlGroup2);

            // As last step of the generation process an exception will be thrown if
            // validation against the internal xsd fails.
            RealTimeControlXmlWriter.GetToolsConfigXml(XsdPath,
                                                          new List<ControlGroup> { controlGroup1, controlGroup2 });
            RealTimeControlXmlWriter.GetDataConfigXml(XsdPath, realTimeControlModel,
                                                         new List<ControlGroup> { controlGroup1, controlGroup2 },
                                                         null);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void ValidateGenerateHydraulicRuleWithoutTriggerAgainstXsds()
        {
            var controlledModel = new ControlledTestModel();
            var controlGroup = RealTimeControlModelHelper.CreateGroupHydraulicRule(true);
            ((HydraulicRule)controlGroup.Rules[0]).Function[0.0] = -1.0;  // empy lookupTable is not allowed
            RealTimeControlTestHelper.AddDummyLinksToGroup(controlledModel, controlGroup);

            //delete conditions
            controlGroup.Conditions.Clear();

            //triggers element should not be generated
            RealTimeControlXmlWriter.GetToolsConfigXml(XsdPath,
                                                          new List<ControlGroup> { controlGroup });
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void ValidateGeneratePidRuleAgainstXsds()
        {
            var controlledModel = new ControlledTestModel();
            var controlGroup = RealTimeControlModelHelper.CreateGroupPidRule(true);

            RealTimeControlTestHelper.AddDummyLinksToGroup(controlledModel, controlGroup);
            // As last step of the generation process an exception will be thrown if
            // validation against the internal xsd fails.
            RealTimeControlXmlWriter.GetToolsConfigXml(XsdPath,
                                                          new List<ControlGroup> { controlGroup });
            RealTimeControlXmlWriter.GetDataConfigXml(XsdPath, realTimeControlModel,
                                                         new List<ControlGroup> { controlGroup },
                                                         null);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void ValidateGenerateIntervalRuleAgainstXsds()
        {
            var controlledModel = new ControlledTestModel();
            var controlGroup = RealTimeControlModelHelper.CreateGroupIntervalRule();

            RealTimeControlTestHelper.AddDummyLinksToGroup(controlledModel, controlGroup);
            // As last step of the generation process an exception will be thrown if
            // validation against the internal xsd fails.
            RealTimeControlXmlWriter.GetToolsConfigXml(XsdPath,
                                                          new List<ControlGroup> { controlGroup });
            RealTimeControlXmlWriter.GetDataConfigXml(XsdPath, realTimeControlModel,
                                                         new List<ControlGroup> { controlGroup },
                                                         null);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void ValidateGenerateTimeRuleAgainstXsds()
        {
            var controlledModel = new ControlledTestModel();
            var controlGroup = RealTimeControlModelHelper.CreateGroupTimeRuleWithCondition();

            RealTimeControlTestHelper.AddDummyLinksToGroup(controlledModel, controlGroup);
            // As last step of the generation process an exception will be thrown if
            // validation against the internal xsd fails.
            RealTimeControlXmlWriter.GetToolsConfigXml(XsdPath,
                                                          new List<ControlGroup> { controlGroup });
            RealTimeControlXmlWriter.GetDataConfigXml(XsdPath, realTimeControlModel,
                                                         new List<ControlGroup> { controlGroup },
                                                         null);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void ValidateGenerateRelativeTimeRuleAgainstXsds()
        {
            var controlledModel = new ControlledTestModel();
            var controlGroup = RealTimeControlModelHelper.CreateGroupRelativeTimeRule();
            ((RelativeTimeRule)controlGroup.Rules[0]).Function[0.0] = -1.0;  // empy lookupTable is not allowed
            RealTimeControlTestHelper.AddDummyLinksToGroup(controlledModel, controlGroup);
            // As last step of the generation process an exception will be thrown if
            // validation against the internal xsd fails.
            RealTimeControlXmlWriter.GetToolsConfigXml(XsdPath,
                                                          new List<ControlGroup> { controlGroup });
            RealTimeControlXmlWriter.GetDataConfigXml(XsdPath, realTimeControlModel,
                                                         new List<ControlGroup> { controlGroup },
                                                         null);
        }

        /// <summary>
        /// Check if duplicate input items are handled well in the xml for dataconfig
        /// note timeseries.xml will be empty
        /// </summary>
        [Test]
        [Category(TestCategory.Integration)]
        public void ValidateDataConfigFor2HydraulicRules()
        {
            var controlGroups = CreateModelWithDuplicateInputOutputItems();

            var dataconfig = RealTimeControlXmlWriter.GetDataConfigXml(XsdPath, realTimeControlModel, controlGroups, null).ToString();
            // generate the xml for dataconfig
            var dataconfigXML = XElement.Parse(dataconfig);
            // parse the generated xml and check the number of input and output items
            var descendants = dataconfigXML.Descendants();
            Assert.AreEqual(2,
                            descendants.Where(
                                d => d.Name.ToString().Contains("OpenMIExchangeItem")).Count());
            Assert.AreEqual(1,
                            descendants.Where(
                                d =>
                                (d.Value.ToUpper().StartsWith("locationWater level".ToUpper())) &&
                                (d.Name.ToString().Contains("OpenMIExchangeItem"))).Count());
            Assert.AreEqual(1,
                            descendants.Where(
                                d =>
                                (d.Value.ToUpper().StartsWith("locationCrest level".ToUpper())) &&
                                (d.Name.ToString().Contains("OpenMIExchangeItem"))).Count());
        }

        private static IList<ControlGroup> CreateModelWithDuplicateInputOutputItems()
        {
            var controlGroups = new List<ControlGroup>();
            var controlledModel = new ControlledTestModel();
            var controlGroup1 = RealTimeControlModelHelper.CreateGroupHydraulicRule(true);
            ((HydraulicRule)controlGroup1.Rules[0]).Function[0.0] = -1.0;  // empy lookupTable is not allowed
            controlGroup1.Rules[0].Name = "Rule1";
            controlGroup1.Conditions[0].Name = "Condition1";
            RealTimeControlTestHelper.AddDummyLinksToGroup(controlledModel, controlGroup1);
            controlGroups.Add(controlGroup1);

            var controlGroup2 = RealTimeControlModelHelper.CreateGroupHydraulicRule(true);
            ((HydraulicRule)controlGroup2.Rules[0]).Function[0.0] = -1.0;  // empy lookupTable is not allowed
            controlGroup2.Rules[0].Name = "Rule2";
            controlGroup2.Conditions[0].Name = "Condition2";
            RealTimeControlTestHelper.AddDummyLinksToGroup(controlledModel, controlGroup2);
            controlGroups.Add(controlGroup2);

            // all rules and condition now have as input an input item linked to: location Waterlevel
            // total of 4 input items, these 4 input items will result in only 1 exchange item (RTC)
            // all rules now have as output an output item: locationCrest level : 2 output items
            return controlGroups;
        }



        /// <summary>
        /// See RTC Document Jaco
        /// 6.3.2.a
        /// </summary>
        /// <returns></returns>
        [Test]
        [Category(TestCategory.Integration)]
        public void GenerateXmlC1And_C2orC3()
        {
            var header = "<rtcToolsConfig" + FewsXmlheader + RtcToolsConfigxsd + ">";
            string strC1And_C2OrC3 =
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
                "<x>output_WeirdWeir_Crest level</x>" +
                "</input>" +
                "<output>" +
                "<y>output_WeirdWeir_Crest level</y>" +
                "</output>" +
                "</unitDelay>" +
                "</rule>" +
                "<rule>" +
                "<interval id=\"Interval Test\">" +
                "<settingBelow>0.2</settingBelow>" +
                "<settingAbove>0.3</settingAbove>" +
                "<settingMaxStep>0</settingMaxStep>" +
                "<deadbandSetpointAbsolute>0.1</deadbandSetpointAbsolute>" +
                "<input>" +
                "<x ref=\"EXPLICIT\">input_MeasureStationA_Water level</x>" +
                "<setpoint>Interval Test_SP</setpoint>" +
                "</input>" +
                "<output>" +
                "<y>output_WeirdWeir_Crest level</y>" +
                "<status>Interval Test_status</status>" +
                "</output>" +
                "</interval>" +
                "</rule>" +
                "</rules>" +
                "<triggers>" +
                "<trigger>" +
                "<standard id=\"C1\">" +
                "<condition>" +
                "<x1Series ref=\"IMPLICIT\">input_CondInputLocation_CondInputQuantityId</x1Series>" +
                "<relationalOperator>Greater</relationalOperator>" +
                "<x2Value>1.1</x2Value>" +
                "</condition>" +
                "<true>" +
                    "<trigger>" +
                    "<standard id=\"C2\">" +
                    "<condition>" +
                    "<x1Series ref=\"IMPLICIT\">input_CondInputLocation_CondInputQuantityId</x1Series>" +
                    "<relationalOperator>Greater</relationalOperator>" +
                    "<x2Value>2.2</x2Value>" +
                    "</condition>" +
                    "<true>" +
                    "<trigger>" +
                    "<ruleReference>Interval Test</ruleReference>" +
                    "</trigger>" +
                    "</true>" +
                    "<false>" +
                            "<trigger>" +
                            "<standard id=\"C3\">" +
                            "<condition>" +
                            "<x1Series ref=\"IMPLICIT\">input_CondInputLocation_CondInputQuantityId</x1Series>" +
                            "<relationalOperator>Less</relationalOperator>" +
                            "<x2Value>0.5</x2Value>" +
                            "</condition>" +
                            "<true>" +
                            "<trigger>" +
                            "<ruleReference>Interval Test</ruleReference>" +
                            "</trigger>" +
                            "</true>" +
                            "<output>" +
                            "<status>Status_C3</status>" +
                            "</output>" +
                            "</standard>" +
                            "</trigger>" +
                    "</false>" +
                    "<output>" +
                    "<status>Status_C2</status>" +
                    "</output>" +
                    "</standard>" +
                    "</trigger>" +
                "</true>" +
                "<output>" +
                "<status>Status_C1</status>" +
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

            var xDocument = RealTimeControlXmlWriter.GetToolsConfigXml(XsdPath, new List<ControlGroup> { controlGroup });
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
            var header = "<rtcToolsConfig" + FewsXmlheader + RtcToolsConfigxsd + ">";
            string strC1AndC2_OrC3 =
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
                "<x>output_WeirdWeir_Crest level</x>" +
                "</input>" +
                "<output>" +
                "<y>output_WeirdWeir_Crest level</y>" +
                "</output>" +
                "</unitDelay>" +
                "</rule>" +
                "<rule>" +
                "<interval id=\"Interval Test\">" +
                "<settingBelow>0.2</settingBelow>" +
                "<settingAbove>0.3</settingAbove>" +
                "<settingMaxStep>0</settingMaxStep>" +
                "<deadbandSetpointAbsolute>0.1</deadbandSetpointAbsolute>" +
                "<input>" +
                "<x ref=\"EXPLICIT\">input_MeasureStationA_Water level</x>" +
                "<setpoint>Interval Test_SP</setpoint>" +
                "</input>" +
                "<output>" +
                "<y>output_WeirdWeir_Crest level</y>" +
                "<status>Interval Test_status</status>" +
                "</output>" +
                "</interval>" +
                "</rule>" +
                "</rules>" +
                "<triggers>" +
                "<trigger>" +
                "<standard id=\"C1\">" +
                "<condition>" +
                "<x1Series ref=\"IMPLICIT\">input_CondInputLocation_CondInputQuantityId</x1Series>" +
                "<relationalOperator>Greater</relationalOperator>" +
                "<x2Value>1.1</x2Value>" +
                "</condition>" +
                "<true>" +
                    "<trigger>" +
                    "<standard id=\"C2\">" +
                    "<condition>" +
                    "<x1Series ref=\"IMPLICIT\">input_CondInputLocation_CondInputQuantityId</x1Series>" +
                    "<relationalOperator>Greater</relationalOperator>" +
                    "<x2Value>2.2</x2Value>" +
                    "</condition>" +
                    "<true>" +
                    "<trigger>" +
                    "<ruleReference>Interval Test</ruleReference>" +
                    "</trigger>" +
                    "</true>" +
                    "<false>" +
                            "<trigger>" +
                            "<standard id=\"C3\">" +
                            "<condition>" +
                            "<x1Series ref=\"IMPLICIT\">input_CondInputLocation_CondInputQuantityId</x1Series>" +
                            "<relationalOperator>Less</relationalOperator>" +
                            "<x2Value>0.5</x2Value>" +
                            "</condition>" +
                            "<true>" +
                            "<trigger>" +
                            "<ruleReference>Interval Test</ruleReference>" +
                            "</trigger>" +
                            "</true>" +
                            "<output>" +
                            "<status>Status_C3</status>" +
                            "</output>" +
                            "</standard>" +
                            "</trigger>" +
                    "</false>" +
                    "<output>" +
                    "<status>Status_C2</status>" +
                    "</output>" +
                    "</standard>" +
                    "</trigger>" +
                "</true>" +
                "<false>" +
                        "<trigger>" +
                        "<standard id=\"C3\">" +
                        "<condition>" +
                        "<x1Series ref=\"IMPLICIT\">input_CondInputLocation_CondInputQuantityId</x1Series>" +
                        "<relationalOperator>Less</relationalOperator>" +
                        "<x2Value>0.5</x2Value>" +
                        "</condition>" +
                        "<true>" +
                        "<trigger>" +
                        "<ruleReference>Interval Test</ruleReference>" +
                        "</trigger>" +
                        "</true>" +
                        "<output>" +
                        "<status>Status_C3</status>" +
                        "</output>" +
                        "</standard>" +
                        "</trigger>" +
                "</false>" +
                "<output>" +
                "<status>Status_C1</status>" +
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

            var xDocument = RealTimeControlXmlWriter.GetToolsConfigXml(XsdPath, new List<ControlGroup> { controlGroup });
            Assert.IsNotNull(xDocument);
            Assert.AreEqual(strC1AndC2_OrC3, xDocument.ToString(SaveOptions.DisableFormatting));
        }

        /// <summary>
        /// See RTC Document Jaco
        /// 6.3.2.c
        /// </summary>
        [Test]
        [Category(TestCategory.Integration)]
        public void GenerateXml_C1AndC2_Or_NotC1AndC3()
        {
            var header = "<rtcToolsConfig" + FewsXmlheader + RtcToolsConfigxsd + ">";
            string strC1AndC2_Or_NotC1AndC3 =
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
                "<x>output_WeirdWeir_Crest level</x>" +
                "</input>" +
                "<output>" +
                "<y>output_WeirdWeir_Crest level</y>" +
                "</output>" +
                "</unitDelay>" +
                "</rule>" +
                "<rule>" +
                "<interval id=\"Interval Test\">" +
                "<settingBelow>0.2</settingBelow>" +
                "<settingAbove>0.3</settingAbove>" +
                "<settingMaxStep>0</settingMaxStep>" +
                "<deadbandSetpointAbsolute>0.1</deadbandSetpointAbsolute>" +
                "<input>" +
                "<x ref=\"EXPLICIT\">input_MeasureStationA_Water level</x>" +
                "<setpoint>Interval Test_SP</setpoint>" +
                "</input>" +
                "<output>" +
                "<y>output_WeirdWeir_Crest level</y>" +
                "<status>Interval Test_status</status>" +
                "</output>" +
                "</interval>" +
                "</rule>" +
                "</rules>" +
                "<triggers>" +
                "<trigger>" +
                "<standard id=\"C1\">" +
                "<condition>" +
                "<x1Series ref=\"IMPLICIT\">input_CondInputLocation_CondInputQuantityId</x1Series>" +
                "<relationalOperator>Greater</relationalOperator>" +
                "<x2Value>1.1</x2Value>" +
                "</condition>" +
                "<true>" +
                    "<trigger>" +
                    "<standard id=\"C2\">" +
                    "<condition>" +
                    "<x1Series ref=\"IMPLICIT\">input_CondInputLocation_CondInputQuantityId</x1Series>" +
                    "<relationalOperator>Greater</relationalOperator>" +
                    "<x2Value>2.2</x2Value>" +
                    "</condition>" +
                    "<true>" +
                    "<trigger>" +
                    "<ruleReference>Interval Test</ruleReference>" +
                    "</trigger>" +
                    "</true>" +
                    "<output>" +
                    "<status>Status_C2</status>" +
                    "</output>" +
                    "</standard>" +
                    "</trigger>" +
                "</true>" +
                "<false>" +
                        "<trigger>" +
                        "<standard id=\"C3\">" +
                        "<condition>" +
                        "<x1Series ref=\"IMPLICIT\">input_CondInputLocation_CondInputQuantityId</x1Series>" +
                        "<relationalOperator>Less</relationalOperator>" +
                        "<x2Value>0.5</x2Value>" +
                        "</condition>" +
                        "<true>" +
                        "<trigger>" +
                        "<ruleReference>Interval Test</ruleReference>" +
                        "</trigger>" +
                        "</true>" +
                        "<output>" +
                        "<status>Status_C3</status>" +
                        "</output>" +
                        "</standard>" +
                        "</trigger>" +
                "</false>" +
                "<output>" +
                "<status>Status_C1</status>" +
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

            var xDocument = RealTimeControlXmlWriter.GetToolsConfigXml(XsdPath, new List<ControlGroup> { controlGroup });
            Assert.IsNotNull(xDocument);
            Assert.AreEqual(strC1AndC2_Or_NotC1AndC3, xDocument.ToString(SaveOptions.DisableFormatting));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GenerateXml_DirectionalCondition()
        {
            var expectedXml =
                "<rtcToolsConfig" + FewsXmlheader + RtcToolsConfigxsd + ">" +
                "<general>" +
                "<description>RTC Model DeltaShell</description>" +
                "<poolRoutingScheme>Theta</poolRoutingScheme>" +
                "<theta>0.5</theta>" +
                "</general>" +
                "<components>" +
                "<component>" +
                "<unitDelay id=\"input_CondInputLocation_CondInputQuantityIdDelay\">" +
                "<input>" +
                "<x>input_CondInputLocation_CondInputQuantityId</x>" +
                "</input>" +
                "<output>" +
                "<y>input_CondInputLocation_CondInputQuantityId-1</y>" +
                "</output>" +
                "</unitDelay>" +
                "</component>" +
                "</components>" +
                "<rules>" +
                "<rule>" +
                "<unitDelay id=\"Interval Test_unitDelay\">" +
                "<input>" +
                "<x>output_WeirdWeir_Crest level</x>" +
                "</input>" +
                "<output>" +
                "<y>output_WeirdWeir_Crest level</y>" +
                "</output>" +
                "</unitDelay>" +
                "</rule>" +
                "<rule>" +
                "<interval id=\"Interval Test\">" +
                "<settingBelow>0.2</settingBelow>" +
                "<settingAbove>0.3</settingAbove>" +
                "<settingMaxStep>0</settingMaxStep>" +
                "<deadbandSetpointAbsolute>0.1</deadbandSetpointAbsolute>" +
                "<input>" +
                "<x ref=\"EXPLICIT\">input_MeasureStationA_Water level</x>" +
                "<setpoint>Interval Test_SP</setpoint>" +
                "</input>" +
                "<output>" +
                "<y>output_WeirdWeir_Crest level</y>" +
                "<status>Interval Test_status</status>" +
                "</output>" +
                "</interval>" +
                "</rule>" +
                "</rules>" +
                "<triggers>" +
                "<trigger>" +
                "<standard id=\"C5\">" +
                "<condition>" +
                "<x1Series ref=\"EXPLICIT\">input_CondInputLocation_CondInputQuantityId</x1Series>" +
                "<relationalOperator>Less</relationalOperator>" +
                "<x2Series ref=\"EXPLICIT\">input_CondInputLocation_CondInputQuantityId-1</x2Series>" +
                "</condition>" +
                "<true>" +
                "<trigger>" +
                "<ruleReference>Interval Test</ruleReference>" +
                "</trigger>" +
                "</true>" +
                "<output>" +
                "<status>Status_C5</status>" +
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
                                                     Feature = new RtcTestFeature { Name = "CondInputLocation" },
                                                 },
                                 };

            condition4.TrueOutputs.Add(intervalRule);

            controlGroup.Conditions.Add(condition4);

            var xDocument = RealTimeControlXmlWriter.GetToolsConfigXml(XsdPath, new List<ControlGroup> { controlGroup });
            Assert.IsNotNull(xDocument);
            var actualString = xDocument.ToString(SaveOptions.DisableFormatting);
            Assert.AreEqual(expectedXml, actualString);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GenerateXml_LookupSignal()
        { 
            var expectedXml =
                "<rtcToolsConfig" + FewsXmlheader + RtcToolsConfigxsd + ">" +
                "<general>" +
                "<description>RTC Model DeltaShell</description>" +
                "<poolRoutingScheme>Theta</poolRoutingScheme>" +
                "<theta>0.5</theta>" +
                "</general>" +
                "<rules>" +
                "<rule>" +
                "<unitDelay id=\"PIDRule Test_unitDelay\">" +
                "<input>" +
                "<x>output_WeirdWeir_Crest level</x>" +
                "</input>" +
                "<output>" +
                "<y>output_WeirdWeir_Crest level</y>" +
                "</output>" +
                "</unitDelay>" +
                "</rule>" +
                "<rule>" +
                "<lookupTable id=\"SetPointForPID\">" +
                "<table>" +
                "<record x=\"10\" y=\"3\" />" +
                "<record x=\"100\" y=\"6\" />" +
                "</table>" +
                "<interpolationOption>LINEAR</interpolationOption>" +
                "<extrapolationOption>BLOCK</extrapolationOption>" +
                "<input>" +
                "<x ref=\"IMPLICIT\">input_MeasureStationB_Discharge</x>" +
                "</input>" +
                "<output>" +
                "<y>SetPointForPID</y>" +
                "</output>" +
                "</lookupTable>" +
                "</rule>" +
                "<rule>" +
                "<pid id=\"PIDRule Test\">" +
                "<mode>PIDVEL</mode>" +
                "<settingMin>1.1</settingMin>" +
                "<settingMax>1.2</settingMax>" +
                "<settingMaxSpeed>1.3</settingMaxSpeed>" +
                "<kp>0.3</kp>" +
                "<ki>0.2</ki>" +
                "<kd>0.1</kd>" +
                "<input>" +
                "<x>input_MeasureStationA_Water level</x>" +
                "<setpointSeries>PIDRule Test_SP</setpointSeries>" +
                "</input>" +
                "<output>" +
                "<y>output_WeirdWeir_Crest level</y>" +
                "<integralPart>PIDRule Test_IP</integralPart>" +
                "<differentialPart>PIDRule Test_DP</differentialPart>" +
                "</output>" +
                "</pid>" +
                "</rule>" +
                "</rules>" +
                "<triggers>" +
                "<trigger>" +
                "<standard id=\"Trigger31\">" +
                "<condition>" +
                "<x1Series ref=\"IMPLICIT\">input_CondInputLocation_CondInputQuantityId</x1Series>" +
                "<relationalOperator>Greater</relationalOperator>" +
                "<x2Value>1.1</x2Value>" +
                "</condition>" +
                "<true>" +
                "<trigger>" +
                "<ruleReference>PIDRule Test</ruleReference>" +
                "</trigger>" +
                "</true>" +
                "<false>" +
                "<trigger>" +
                "<ruleReference>PIDRule Test</ruleReference>" +
                "</trigger>" +
                "</false>" +
                "<output>" +
                "<status>Status_Trigger31</status>" +
                "</output>" +
                "</standard>" +
                "</trigger>" +
                "</triggers>" +
                "</rtcToolsConfig>";

            SetUpGlobalPidRuleForGlobalControlGroup();
            SetUpLookupSignal();

            var xDocument = RealTimeControlXmlWriter.GetToolsConfigXml(XsdPath, new List<ControlGroup> { controlGroup });
            Assert.IsNotNull(xDocument);
            var actualString = xDocument.ToString(SaveOptions.DisableFormatting);
            Assert.AreEqual(expectedXml, actualString);
        }

        [Test]
        public void GetToolsDataXml_OnePIDRuleOneLookupSignal()
        {
            SetUpGlobalPidRuleForGlobalControlGroup();
            SetUpLookupSignal();

            var strOutputXml = DataResultXml(input, output, true);

            var xDocument = RealTimeControlXmlWriter.GetDataConfigXml(XsdPath, realTimeControlModel, new List<ControlGroup> { controlGroup }, null);
            Assert.IsNotNull(xDocument);
            Assert.AreEqual(strOutputXml, xDocument.ToString(SaveOptions.DisableFormatting));
        }

    }
}
