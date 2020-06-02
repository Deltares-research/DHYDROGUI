using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO.Handlers;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Xsd;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.ImportExport
{
    [TestFixture]
    public class RealTimeControlDataConfigXmlConverterTest
    {
        private const string InputId = RtcXmlTag.Input + "parameter/quantity";
        private const string OutputId = RtcXmlTag.Output + "parameter/quantity";
        private const string ControlGroupName = "control_group_name";
        private RealTimeControlDataConfigXmlConverter dataConfigConverter;
        private ILogHandler logHandler;

        [SetUp]
        public void SetUp()
        {
            logHandler = new LogHandler("");
            dataConfigConverter = new RealTimeControlDataConfigXmlConverter(logHandler);
        }

        [TearDown]
        public void TearDown()
        {
            logHandler = null;
            dataConfigConverter = null;
        }

        [Test]
        public void GivenParameterIsNull_WhenCreateConnectionPointsFromXmlElementsIsCalled_ThenNothingHappensAndMethodReturn()
        {
            IEnumerable<ConnectionPoint> connectionPoints = null;

            // Given, When
            Assert.DoesNotThrow(
                () => connectionPoints = dataConfigConverter.CreateConnectionPointsFromXmlElements(null),
                "Method throws an unexpected exception when parameter 'elements' is null.");

            // Then
            AssertNotNullAndEmpty(connectionPoints);
        }

        [TestCase(InputId, typeof(Input))]
        [TestCase(OutputId, typeof(Output))]
        public void GivenAnRTCTimeSeriesXMLElement_WhenCreateConnectionPointsFromXmlElementsIsCalled_ThenCollectionOfConnectionPointsIsReturned(
            string elementId, Type expectedConnectionPointType)
        {
            // Given
            List<RTCTimeSeriesXML> timeSeriesElements = CreateTimeSeriesXml(elementId);

            // When
            List<ConnectionPoint> connectionPoints =
                dataConfigConverter.CreateConnectionPointsFromXmlElements(timeSeriesElements).ToList();

            // Then
            Assert.AreEqual(1, connectionPoints.Count, "Number of connection points was expected to be 1.");
            ConnectionPoint connectionPoint = connectionPoints.Single();
            Assert.AreEqual(elementId, connectionPoint.Name,
                            $"Name of connection point was expected to be '{elementId}' but was '{connectionPoint.Name}'.");
            Assert.AreEqual(expectedConnectionPointType, connectionPoint.GetType(),
                            $"The type of the created connection point '{elementId}' was expected to be different.");
        }

        [TestCase(RtcXmlTag.TimeRule + ControlGroupName + "/" + "time_rule_name")]
        [TestCase(OutputId + RtcXmlTag.OutputAsInput + "something")]
        [TestCase(RtcXmlTag.Delayed + OutputId)]
        [TestCase(ControlGroupName + "/" + "time_rule_name")]
        public void GivenAnRTCTimeSeriesXMLElementWithInvalidOrWithoutValidTag_WhenCreateConnectionPointsFromXmlElementsIsCalled_ThenElementIsIgnored(
            string elementId)
        {
            // Given
            List<RTCTimeSeriesXML> timeSeriesElements = CreateTimeSeriesXml(elementId);

            // When
            IEnumerable<ConnectionPoint> connectionPoints =
                dataConfigConverter.CreateConnectionPointsFromXmlElements(timeSeriesElements);

            // Then
            AssertNotNullAndEmpty(connectionPoints);
        }

        [TestCase(InputId)]
        [TestCase(OutputId)]
        public void GivenAnRTCTimeSeriesXMLElementWithoutOpenExchangeItemWithId_WhenCreateConnectionPointsFromXmlElementsIsCalled_ThenElementIsIgnored(
            string elementId)
        {
            // Given
            List<RTCTimeSeriesXML> timeSeriesElements = CreateTimeSeriesXml(elementId, true);

            // When
            IEnumerable<ConnectionPoint> connectionPoints =
                dataConfigConverter.CreateConnectionPointsFromXmlElements(timeSeriesElements);

            // Then
            AssertNotNullAndEmpty(connectionPoints);
        }

        private static List<RTCTimeSeriesXML> CreateTimeSeriesXml(string elementId, bool isEmpty = false)
        {
            var timeSeriesElements = new List<RTCTimeSeriesXML>
            {
                new RTCTimeSeriesXML
                {
                    id = elementId,
                    OpenMIExchangeItem = new OpenMIExchangeItemXML {elementId = isEmpty ? null : "not_empty"}
                },
            };
            return timeSeriesElements;
        }

        private static void AssertNotNullAndEmpty(IEnumerable<ConnectionPoint> connectionPoints)
        {
            Assert.NotNull(connectionPoints, "List of connection points was expected to not be NULL.");
            Assert.AreEqual(0, connectionPoints.Count(), "List of connection points was expected to be empty.");
        }
    }
}