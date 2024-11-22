﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DeltaShell.Dimr;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils.Domain;
using NSubstitute;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.IO
{
    [TestFixture]
    public class RealTimeControlXmlWriterTest
    {
        [Test]
        public void WriteToXml_ModelIsNull_ThrowsArgumentNullException()
        {
            RealTimeControlXmlWriter writer = new RealTimeControlXmlWriter();

            Assert.That(() => writer.WriteToXml(null, "dir"), Throws.ArgumentNullException);
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void WriteToXml_DirectoryIsNullOrEmpty_ThrowsArgumentException(string directory)
        {
            RealTimeControlModel model = new RealTimeControlModel();
            RealTimeControlXmlWriter writer = new RealTimeControlXmlWriter();

            Assert.That(() => writer.WriteToXml(model, directory), Throws.ArgumentException);
        }
        
        [Test]
        public void WriteToXml_DirectoryDoesNotExist_ThrowsDirectoryNotFoundException()
        {
            RealTimeControlModel model = new RealTimeControlModel();
            RealTimeControlXmlWriter writer = new RealTimeControlXmlWriter();

            Assert.That(() => writer.WriteToXml(model, "dir"), Throws.InstanceOf<DirectoryNotFoundException>());
        }
        
        [Test]
        public void CheckIfXsdFileAreAtCorrectLocation()
        {
            Assert.AreEqual(13, Directory.GetFiles(DimrApiDataSet.RtcXsdDirectory).Count(f => f.EndsWith("xsd"))); // check x64
        }

        [Test]
        public void GetDataConfigXml_ForRelativeTimeRule()
        {
            var mocks = new MockRepository();

            var stubTimeDependentModel = mocks.DynamicMock<ITimeDependentModel>();

            var controlGroup = new ControlGroup {Name = "control_group_containing_relative_time_rule"};
            var output = new Output {Name = "output"};
            controlGroup.Outputs.Add(output);
            RelativeTimeRule relativeTimeRule = RealTimeControlTestHelper.CreateRelativeTimeRule("relative_time_rule", output);
            controlGroup.Rules.Add(relativeTimeRule);

            mocks.ReplayAll();

            XDocument result = RealTimeControlXmlWriter.GetDataConfigXml(DimrApiDataSet.RtcXsdDirectory,
                                                                         stubTimeDependentModel,
                                                                         new List<ControlGroup> {controlGroup},
                                                                         null);

            mocks.VerifyAll();

            // "<rtcDataConfig xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:rtc='http://www.wldelft.nl/fews' xmlns='http://www.wldelft.nl/fews' xsi:schemaLocation='http://www.wldelft.nl/fews c:\src\delta-shell\Products\NGHS\bin\Debug\plugins\DeltaShell.Plugins.DelftModels.RealTimeControl\xsd\rtcDataConfig.xsd'>" +
            // "  <importSeries>" +
            // "    <timeSeries id='Undefined' />" +
            // "  </importSeries>" +
            // "  <exportSeries>" +
            // "    <CSVTimeSeriesFile decimalSeparator='.' delimiter=',' adjointOutput='false'></CSVTimeSeriesFile>" +
            // "    <PITimeSeriesFile>" +
            // "      <timeSeriesFile>timeseries_export.xml</timeSeriesFile>" +
            // "      <useBinFile>false</useBinFile>" +
            // "    </PITimeSeriesFile>" +
            // "    <timeSeries id='output_output' />" +
            // "    <timeSeries id='control_group_containing_relative_time_rulerelative_time_rule_t' />" +
            // "  </exportSeries>" +
            // "</rtcDataConfig>";

            // test for the presence of the 'Undefined' time series entry in the xml that gets generated.
            XContainer topNode = result.Nodes().OfType<XContainer>().FirstOrDefault();
            Assert.NotNull(topNode, "error in xml: top node not found.");
            var importSeriesNode = topNode.FirstNode as XContainer;
            Assert.NotNull(importSeriesNode, "error in xml: import time series node not found.");
            var importTimeSeriesNode = importSeriesNode.FirstNode as XElement;
            Assert.NotNull(importTimeSeriesNode, "error in xml: time series element not found");
            XAttribute idAttribute = importTimeSeriesNode.FirstAttribute;
            Assert.NotNull(idAttribute, "error in xml: attribute for time series element not found.");
            Assert.AreEqual("id", idAttribute.Name.LocalName, "error in xml: mismatch for first attributes' name.");
            Assert.AreEqual("Undefined", idAttribute.Value, "error in xml: mismatch for first attributes' value.");
        }

        [Test]
        public void GetDataConfigXml_ForPidRuleUsingMathematicalExpressionWith2ArgumentsAsInput()
        {
            // Arrange
            var substituteTimeDependentModel = Substitute.For<ITimeDependentModel>();

            const string expression = "A+6";
            ControlGroup controlGroup = CreateControlGroupWithPidRuleAndMathematicalExpression(expression);

            // Act
            XDocument result = RealTimeControlXmlWriter.GetDataConfigXml(DimrApiDataSet.RtcXsdDirectory,
                                                                         substituteTimeDependentModel,
                                                                         new List<ControlGroup> {controlGroup}, null);

            // Assert
            List<XNode> exportTimeSeriesList = RetrieveTimeSeriesOrTriggers(result);
            var exportTimeSeriesNode = exportTimeSeriesList.Last() as XElement;
            Assert.NotNull(exportTimeSeriesNode, "error in xml: time series element not found");

            XAttribute idAttribute = exportTimeSeriesNode.FirstAttribute;
            Assert.NotNull(idAttribute, "error in xml: attribute for time series element not found.");

            Assert.AreEqual("id", idAttribute.Name.LocalName);
            Assert.AreEqual("Control Group/f1", idAttribute.Value, "error in xml: mismatch for first attributes' value.");
        }

        [Test]
        public void GetDataConfigXml_ForPidRuleUsingMathematicalExpressionWith3ArgumentsAsInput()
        {
            // Arrange
            var substituteTimeDependentModel = Substitute.For<ITimeDependentModel>();

            const string expression = "A+6+8";
            ControlGroup controlGroup = CreateControlGroupWithPidRuleAndMathematicalExpression(expression);

            // Act
            XDocument result = RealTimeControlXmlWriter.GetDataConfigXml(DimrApiDataSet.RtcXsdDirectory,
                                                                         substituteTimeDependentModel,
                                                                         new List<ControlGroup> {controlGroup},
                                                                         null);

            // Assert
            List<XNode> exportTimeSeriesList = RetrieveTimeSeriesOrTriggers(result);

            int nrOfTimeseries = exportTimeSeriesList.Count;
            var mainMathematicalExpressionYValueReference = exportTimeSeriesList[nrOfTimeseries - 2] as XElement;
            var subMathematicalExpressionYValueReference = exportTimeSeriesList[nrOfTimeseries - 1] as XElement;
            Assert.NotNull(mainMathematicalExpressionYValueReference, "error in xml: main time series element for mathematical expression not found");
            Assert.NotNull(subMathematicalExpressionYValueReference, "error in xml: sub time series element for mathematical expression not found");

            XAttribute firstAttribute = mainMathematicalExpressionYValueReference.FirstAttribute;
            XAttribute secondAttribute = subMathematicalExpressionYValueReference.FirstAttribute;
            Assert.NotNull(firstAttribute, "error in xml: attribute for first time series element not found.");
            Assert.NotNull(secondAttribute, "error in xml: attribute for second time series element not found.");

            Assert.AreEqual("id", firstAttribute.Name.LocalName);
            Assert.AreEqual("Control Group/f1", firstAttribute.Value, "error in xml: mismatch for first attributes' value.");
            Assert.AreEqual("id", secondAttribute.Name.LocalName);
            Assert.AreEqual("Control Group/f1/([Input]feature1/waterlevel + 6)", secondAttribute.Value, "error in xml: mismatch for first attributes' value.");
        }

        [Test]
        public void GivenAModelWithUseSaveStateTimeRangeAndWriteRestartFlagsAndAValidXsdPathWhenGetRuntimeXmlIsCalledWithThisModelAndPathThenTheModelContainsValidStateFilesElement()
        {
            // Given
            var model = new RealTimeControlModel();
            model.SaveStateStartTime = DateTime.Today;
            model.SaveStateStopTime = model.SaveStateStartTime.AddDays(1);
            model.SaveStateTimeStep = TimeSpan.FromHours(1);

            model.WriteRestart = true;

            string xsdPath = DimrApiDataSet.RtcXsdDirectory;

            // When
            XDocument resultDocument = RealTimeControlXmlWriter.GetRuntimeConfigXml(xsdPath, model, false, 0);

            // Then
            var ns = (XNamespace) "http://www.wldelft.nl/fews";

            // stateFilesElement
            const int expectedNumberOfStateFilesChildren = 3;

            XElement stateFilesElement = resultDocument.Root?.Element(ns + "stateFiles");
            Assert.That(stateFilesElement, Is.Not.Null);
            Assert.That(stateFilesElement.Descendants().Count(), Is.EqualTo(expectedNumberOfStateFilesChildren));

            // startDate
            string expectedStartDate = string.Format("{0:0000}-{1:00}-{2:00}",
                                                     model.SaveStateStartTime.Year,
                                                     model.SaveStateStartTime.Month,
                                                     model.SaveStateStartTime.Day);
            string expectedStartTime = string.Format("{0:00}:{1:00}:{2:00}",
                                                     model.SaveStateStartTime.Hour,
                                                     model.SaveStateStartTime.Minute,
                                                     model.SaveStateStartTime.Second);

            XElement startDateElement = stateFilesElement.Element(ns + "startDate");
            Assert.That(startDateElement, Is.Not.Null);
            Assert.That(startDateElement.Attribute("date")?.Value, Is.EqualTo(expectedStartDate));
            Assert.That(startDateElement.Attribute("time")?.Value, Is.EqualTo(expectedStartTime));

            // endDate
            string expectedEndDate = string.Format("{0:0000}-{1:00}-{2:00}",
                                                   model.SaveStateStartTime.Year,
                                                   model.SaveStateStartTime.Month,
                                                   model.SaveStateStartTime.Day);
            string expectedEndTime = string.Format("{0:00}:{1:00}:{2:00}",
                                                   model.SaveStateStartTime.Hour,
                                                   model.SaveStateStartTime.Minute,
                                                   model.SaveStateStartTime.Second);

            XElement endDateElement = stateFilesElement.Element(ns + "endDate");
            Assert.That(endDateElement, Is.Not.Null);
            Assert.That(startDateElement.Attribute("date")?.Value, Is.EqualTo(expectedEndDate));
            Assert.That(startDateElement.Attribute("time")?.Value, Is.EqualTo(expectedEndTime));

            // stateTimeStep
            var expectedTimeStep = ((int) model.SaveStateTimeStep.TotalSeconds).ToString();
            XElement stateTimeStepElement = stateFilesElement.Element(ns + "stateTimeStep");
            Assert.That(stateTimeStepElement, Is.Not.Null);
            Assert.That(stateTimeStepElement.Value, Is.EqualTo(expectedTimeStep));
        }

        [Test]
        public void GetToolsConfigXml_WhenTwoRulesAndTwoConditionsShareOutput_ThenBothConditionsXmlElementsAreAddedToTheXDocument()
        {
            // Set-up
            ControlGroup controlGroup = RealTimeControlTestHelper.CreateControlGroupWithTwoRulesOnOneOutput();

            // Call
            XDocument xDocument = RealTimeControlXmlWriter.GetToolsConfigXml(DimrApiDataSet.RtcXsdDirectory,
                                                                             new List<ControlGroup> {controlGroup});

            // Assert
            Assert.That(xDocument.Root, Is.Not.Null);
            XElement conditionsXmlElement = xDocument.Root.Elements()
                                                     .Single(e => e.Name.LocalName.Equals("triggers"));
            Assert.That(conditionsXmlElement.Elements().Count(), Is.EqualTo(2), "Conditions XElement should contain two elements.");
        }

        [TestCase(PIDRule.PIDRuleSetpointTypes.TimeSeries, false, 0)]
        [TestCase(PIDRule.PIDRuleSetpointTypes.TimeSeries, true, 0)] // should never happen due to export validation
        [TestCase(PIDRule.PIDRuleSetpointTypes.Constant, false, 1)]
        [TestCase(PIDRule.PIDRuleSetpointTypes.Constant, true, 0)]
        [TestCase(PIDRule.PIDRuleSetpointTypes.Signal, false, 1)]
        [TestCase(PIDRule.PIDRuleSetpointTypes.Signal, true, 0)]
        public void
            GivenARealTimeControlXmlWriterAndAModelWithAPidRule_WhenGetTimeSeriesXmlIsCalled_ThenExpectedNumberOfWarningsIsGiven(PIDRule.PIDRuleSetpointTypes setPointTypes, bool hasEmptyTimeSeries, int nExpectedWarnings)
        {
            string expectedMessage = string.Format(Resources
                                                       .RealTimeControlXmlWriter_GetXmlTimeSeriesFromControlGroups_PIDRule__0__time_series_will_not_be_included_in_the_DIMR_XML_as_Set_Point_Type_is_not_TimeSeries, "PID Rule");
            string[] expectedMessages = Enumerable.Repeat(expectedMessage, nExpectedWarnings).ToArray();

            // Given
            RealTimeControlModel model = CreateRealTimeControlModelWithPidRule(setPointTypes, hasEmptyTimeSeries);

            TestHelper.AssertLogMessagesAreGenerated(
                // When
                () => RealTimeControlXmlWriter.GetTimeSeriesXml(DimrApiDataSet.RtcXsdDirectory, model, model.ControlGroups),
                // Then
                expectedMessages, nExpectedWarnings);
        }

        private RealTimeControlModel CreateRealTimeControlModelWithPidRule(PIDRule.PIDRuleSetpointTypes setPointTypes,
                                                                           bool hasEmptyTimeSeries)
        {
            var model = new RealTimeControlModel();
            var controlGroup = new ControlGroup();
            PIDRule pidRule = CreatePidRule(setPointTypes, hasEmptyTimeSeries, model.StartTime, model.StopTime);
            controlGroup.Rules.Add(pidRule);
            model.ControlGroups.Add(controlGroup);

            return model;
        }

        private static PIDRule CreatePidRule(PIDRule.PIDRuleSetpointTypes setPointTypes, bool hasEmptyTimeSeries,
                                             DateTime start, DateTime stop)
        {
            var pidRule = new PIDRule {PidRuleSetpointType = setPointTypes};

            if (hasEmptyTimeSeries)
            {
                return pidRule;
            }

            pidRule.TimeSeries.Time.AddValues(new[]
            {
                start,
                stop
            });
            pidRule.TimeSeries[start] = 1d;
            pidRule.TimeSeries[stop] = 1d;

            return pidRule;
        }

        private static ControlGroup CreateControlGroupWithPidRuleAndMathematicalExpression(string expression)
        {
            var inputME = new Input
            {
                ParameterName = "waterlevel",
                Feature = new RtcTestFeature {Name = "feature1"}
            };

            var inputPidRule = new MathematicalExpression
            {
                Name = "f1",
                Expression = expression
            };

            inputPidRule.Inputs.Add(inputME);

            var output = new Output();
            var pidRule = new PIDRule();

            var controlGroup = new ControlGroup
            {
                Name = "Control Group",
                Rules = {pidRule},
                MathematicalExpressions = {inputPidRule},
                Inputs = {inputME},
                Outputs = {output}
            };
            return controlGroup;
        }

        [Test]
        public void ControlGroupWithMathematicalExpressionWritesBranchNodesBeforeRootNode()
        {
            // Setup
            const string expression = "(B+C)+A";
            ControlGroup controlGroup = CreateControlGroupWithPidRuleAndMathematicalExpressionWithThreeInputs(expression);

            // Act
            XDocument result = RealTimeControlXmlWriter.GetToolsConfigXml(DimrApiDataSet.RtcXsdDirectory,
                                                                          new List<ControlGroup> {controlGroup},
                                                                          false);

            // Assert
            List<XNode> triggers = RetrieveTimeSeriesOrTriggers(result);
            Assert.That(triggers.Count, Is.EqualTo(2)); // One for (B+C) and one for (B+C) + A

            string yValueOfFirstTrigger = GetYValueOfTrigger(triggers[0]);
            Assert.That(yValueOfFirstTrigger, Is.EqualTo("Control Group/f1/([Input]feature2/waterlevel + [Input]feature3/waterlevel)"));

            string yValueOfSecondTrigger = GetYValueOfTrigger(triggers[1]);
            Assert.That(yValueOfSecondTrigger, Is.EqualTo("Control Group/f1")); 
        }

        private static ControlGroup CreateControlGroupWithPidRuleAndMathematicalExpressionWithThreeInputs(string expression)
        {
            // Create three inputs: A, B and C
            Input inputOne = CreateTestInput("waterlevel", "feature1");
            Input inputTwo = CreateTestInput("waterlevel", "feature2");
            Input inputThree = CreateTestInput("waterlevel", "feature3");
            
            var mathExpression = new MathematicalExpression{ Name = "f1", Expression = expression };
            mathExpression.Inputs.Add(inputOne);
            mathExpression.Inputs.Add(inputTwo);
            mathExpression.Inputs.Add(inputThree);

            var output = new Output();

            // Create a PIDRule that takes the MathematicalExpression as input and the Output as output
            var pidRule = new PIDRule { Inputs = {mathExpression}, Outputs = {output} };

            return new ControlGroup
            {
                Name = "Control Group",
                Rules = {pidRule},
                MathematicalExpressions = {mathExpression},
                Inputs = {inputOne, inputTwo, inputThree},
                Outputs = {output}
            };
        }

        private static string GetYValueOfTrigger(XNode trigger)
        {
            XNamespace rtc = "http://www.wldelft.nl/fews";
            var triggerElement = trigger as XElement;
            XElement y = triggerElement.Descendants(rtc + "y").First();

            return y.Value;
        }

        private static Input CreateTestInput(string parameterName, string name)
        {
            return new Input
            {
                ParameterName = parameterName,
                Feature = new RtcTestFeature { Name = name }
            };
        }

        private static List<XNode> RetrieveTimeSeriesOrTriggers(XDocument result)
        {
            XContainer topNode = result.Nodes().OfType<XContainer>().FirstOrDefault();
            Assert.NotNull(topNode, "error in xml: top node not found.");

            var exportSeriesNode = topNode.LastNode as XContainer;
            Assert.NotNull(exportSeriesNode, "error in xml: export time series node not found.");

            List<XNode> exportTimeSeriesList = exportSeriesNode.Nodes().ToList();
            Assert.NotNull(exportTimeSeriesList, "error in xml: time series list not found");

            return exportTimeSeriesList;
        }
    }
}