using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.Dimr.RtcXsd;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.IO
{
    [TestFixture]
    public class RealTimeControlDataConfigXmlConverterTest
    {
        private const string InputId = RtcXmlTag.Input + "parameter/quantity";
        private const string OutputId = RtcXmlTag.Output + "parameter/quantity";
        private const string ControlGroupName = "control_group_name";

        [Test]
        public void GivenParameterIsNull_WhenCreateConnectionPointsFromXmlElementsIsCalled_ThenNothingHappensAndMethodReturn()
        {
            IEnumerable<ConnectionPoint> connectionPoints = null;

            // Given, When
            Assert.DoesNotThrow(
                () => connectionPoints = RealTimeControlDataConfigXmlConverter.CreateConnectionPointsFromXmlElements(null),
                "Method throws an unexpected exception when parameter 'elements' is null.");

            // Then
            AssertNotNullAndEmpty(connectionPoints);
        }

        [TestCase(InputId, typeof(Input))]
        [TestCase(OutputId, typeof(Output))]
        public void GivenAnRTCTimeSeriesComplexTypeElement_WhenCreateConnectionPointsFromXmlElementsIsCalled_ThenCollectionOfConnectionPointsIsReturned(
            string elementId, Type expectedConnectionPointType)
        {
            // Given
            List<RTCTimeSeriesComplexType> timeSeriesElements = CreateTimeSeriesXml(elementId);

            // When
            List<ConnectionPoint> connectionPoints =
                RealTimeControlDataConfigXmlConverter.CreateConnectionPointsFromXmlElements(timeSeriesElements).ToList();

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
        public void GivenAnRTCTimeSeriesComplexTypeElementWithInvalidOrWithoutValidTag_WhenCreateConnectionPointsFromXmlElementsIsCalled_ThenElementIsIgnored(
            string elementId)
        {
            // Given
            List<RTCTimeSeriesComplexType> timeSeriesElements = CreateTimeSeriesXml(elementId);

            // When
            IEnumerable<ConnectionPoint> connectionPoints =
                RealTimeControlDataConfigXmlConverter.CreateConnectionPointsFromXmlElements(timeSeriesElements);

            // Then
            AssertNotNullAndEmpty(connectionPoints);
        }

        [TestCase(InputId)]
        [TestCase(OutputId)]
        public void GivenAnRTCTimeSeriesComplexTypeElementWithoutOpenExchangeItemWithId_WhenCreateConnectionPointsFromXmlElementsIsCalled_ThenElementIsIgnored(
            string elementId)
        {
            // Given
            List<RTCTimeSeriesComplexType> timeSeriesElements = CreateTimeSeriesXml(elementId, true);

            // When
            IEnumerable<ConnectionPoint> connectionPoints =
                RealTimeControlDataConfigXmlConverter.CreateConnectionPointsFromXmlElements(timeSeriesElements);

            // Then
            AssertNotNullAndEmpty(connectionPoints);
        }

        private static List<RTCTimeSeriesComplexType> CreateTimeSeriesXml(string elementId, bool isEmpty = false)
        {
            var timeSeriesElements = new List<RTCTimeSeriesComplexType>
            {
                new RTCTimeSeriesComplexType
                {
                    id = elementId,
                    OpenMIExchangeItem = new OpenMIExchangeItemComplexType {elementId = isEmpty ? null : "not_empty"}
                }
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