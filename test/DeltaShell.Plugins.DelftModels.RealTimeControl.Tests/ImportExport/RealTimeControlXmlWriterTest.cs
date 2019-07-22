using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Shell.Core.Workflow;
using DeltaShell.Dimr;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.ImportExport
{
    [TestFixture]
    class RealTimeControlXmlWriterTest
    {
        [Test]
        public void CheckIfXsdFileAreAtCorrectLocation()
        {
            Assert.AreEqual(13, Directory.GetFiles(DimrApiDataSet.RtcToolsDllPath).Count(f => f.EndsWith("xsd"))); // check x64
        }


        [Test]
        public void GetDataConfigXmlForRelativeTimeRule()
        {
            var mocks = new MockRepository();

            var stubTimeDependentModel = mocks.DynamicMock<ITimeDependentModel>();

            var controlGroup = new ControlGroup {Name = "control_group_containing_relative_time_rule"};
            var output = new Output { Name = "output" };
            controlGroup.Outputs.Add(output);
            var relativeTimeRule = new RelativeTimeRule
            {
                Name = "relative_time_rule",
                FromValue = false,
                Id = 6L,
                Interpolation = InterpolationType.Constant,
                MinimumPeriod = 3,
                LongName = "relative_time_rule_long_name"
            };
            relativeTimeRule.Outputs.Add(output);
            relativeTimeRule.Function[0d] = 1d;
            relativeTimeRule.Function[3d] = 5d;
            relativeTimeRule.Function[7d] = 11d;
            controlGroup.Rules.Add(relativeTimeRule);

            mocks.ReplayAll();

            var result = RealTimeControlXmlWriter.GetDataConfigXml(DimrApiDataSet.RtcToolsDllPath, stubTimeDependentModel, new List<ControlGroup> {controlGroup}, null);

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
            var topNode = result.Nodes().OfType<XContainer>().FirstOrDefault();
            Assert.NotNull(topNode, "error in xml: top node not found.");
            var importSeriesNode = topNode.FirstNode as XContainer;
            Assert.NotNull(importSeriesNode, "error in xml: import time series node not found.");
            var importTimeSeriesNode = importSeriesNode.FirstNode as XElement;
            Assert.NotNull(importTimeSeriesNode, "error in xml: time series element not found");
            var idAttribute = importTimeSeriesNode.FirstAttribute;
            Assert.NotNull(idAttribute, "error in xml: attribute for time series element not found.");
            Assert.AreEqual("id", idAttribute.Name.LocalName, "error in xml: mismatch for first attributes' name.");
            Assert.AreEqual("Undefined", idAttribute.Value, "error in xml: mismatch for first attributes' value.");
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

            var xsdPath = DimrApiDataSet.RtcToolsDllPath;

            // When
            var resultDocument = RealTimeControlXmlWriter.GetRuntimeXml(xsdPath, model, false, 0);

            // Then
            var ns = (XNamespace) "http://www.wldelft.nl/fews";

            // stateFilesElement
            const int expectedNumberOfStateFilesChildren = 3;

            var stateFilesElement = resultDocument.Root?.Element(ns + "stateFiles");
            Assert.That(stateFilesElement, Is.Not.Null);
            Assert.That(stateFilesElement.Descendants().Count(), Is.EqualTo(expectedNumberOfStateFilesChildren));

            // startDate
            var expectedStartDate = string.Format("{0:0000}-{1:00}-{2:00}",
                model.SaveStateStartTime.Year,
                model.SaveStateStartTime.Month,
                model.SaveStateStartTime.Day);
            var expectedStartTime = string.Format("{0:00}:{1:00}:{2:00}",
                model.SaveStateStartTime.Hour,
                model.SaveStateStartTime.Minute,
                model.SaveStateStartTime.Second);

            var startDateElement = stateFilesElement.Element(ns + "startDate");
            Assert.That(startDateElement, Is.Not.Null);
            Assert.That(startDateElement.Attribute("date")?.Value, Is.EqualTo(expectedStartDate));
            Assert.That(startDateElement.Attribute("time")?.Value, Is.EqualTo(expectedStartTime));

            // endDate
            var expectedEndDate = string.Format("{0:0000}-{1:00}-{2:00}",
                model.SaveStateStartTime.Year,
                model.SaveStateStartTime.Month,
                model.SaveStateStartTime.Day);
            var expectedEndTime = string.Format("{0:00}:{1:00}:{2:00}",
                model.SaveStateStartTime.Hour,
                model.SaveStateStartTime.Minute,
                model.SaveStateStartTime.Second);

            var endDateElement = stateFilesElement.Element(ns + "endDate");
            Assert.That(endDateElement, Is.Not.Null);
            Assert.That(startDateElement.Attribute("date")?.Value, Is.EqualTo(expectedEndDate));
            Assert.That(startDateElement.Attribute("time")?.Value, Is.EqualTo(expectedEndTime));

            // stateTimeStep
            var expectedTimeStep = ((int)model.SaveStateTimeStep.TotalSeconds).ToString();
            var stateTimeStepElement = stateFilesElement.Element(ns + "stateTimeStep");
            Assert.That(stateTimeStepElement, Is.Not.Null);
            Assert.That(stateTimeStepElement.Value, Is.EqualTo(expectedTimeStep));
        }
    }
}
