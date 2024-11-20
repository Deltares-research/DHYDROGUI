using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui
{
    [TestFixture]
    public class FlowBoundaryConditionSeriesFactoryIntegrationTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void Given_FmModel_With_Empty_FlowBoundaryCondition_When_Changed_To_BedLoadTransport_ThenBoundaryConditionAreOfBedLoadTransportType()
        {
            //Given
            const int expectedBackgroundFunctions = 3;
            const int supportPoint = 1;
            const bool useLayers = true;
            var expectedBoundaryQuantyType = FlowBoundaryQuantityType.MorphologyBedLoadTransport;
            FlowBoundaryConditionSeriesFactory factory = null;

            using (var tempDirectory = new TemporaryDirectory())
            {
                // Get test data
                string mduPath = TestHelper.GetTestFilePath(@"ExBedLoadTransportBoundary\FlowFM.mdu");
                mduPath = tempDirectory.CopyTestDataFileAndDirectoryToTempDirectory(mduPath);

                var model = new WaterFlowFMModel();
                model.ImportFromMdu(mduPath);

                var boundaryCondition = model.ModelDefinition.BoundaryConditions.ElementAt(0) as FlowBoundaryCondition;
                Assert.That(boundaryCondition, Is.Not.Null);

                //When
                Feature2D feature = CreateFeature2D();
                boundaryCondition = ChangeFlowBoundaryConditionToBedLoadTransportQuantityType(feature);
                Func<FlowBoundaryConditionPointData> createPointData = () =>
                    new FlowBoundaryConditionPointData(boundaryCondition, supportPoint, useLayers);
                IEnumerable<FlowBoundaryConditionPointData> flowBoundaryConditions = Enumerable.Repeat(createPointData.Invoke(), 3);
                factory = new FlowBoundaryConditionSeriesFactory {BackgroundFunctions = new EventedList<FlowBoundaryConditionPointData>(flowBoundaryConditions)};
            }

            //Then
            int totalOfBackGroundFunctions = factory.BackgroundFunctions.Count;
            var errorMssgWrongFunctionNumber = $"Expected number of functions ({expectedBackgroundFunctions}) does not match retrieved ({totalOfBackGroundFunctions}) ";
            Assert.That(totalOfBackGroundFunctions, Is.EqualTo(expectedBackgroundFunctions), errorMssgWrongFunctionNumber);

            FlowBoundaryCondition flowBoundaryCondition1 = GetBackgroundFunctionFlowQuantityType(factory, 0);
            FlowBoundaryCondition flowBoundaryCondition2 = GetBackgroundFunctionFlowQuantityType(factory, 1);
            FlowBoundaryCondition flowBoundaryCondition3 = GetBackgroundFunctionFlowQuantityType(factory, 2);

            Assert.That(flowBoundaryCondition1.FlowQuantity, Is.EqualTo(expectedBoundaryQuantyType), $"Boundary {flowBoundaryCondition1.Name} expected type {expectedBoundaryQuantyType}, but was {flowBoundaryCondition1.FlowQuantity}.");
            Assert.That(flowBoundaryCondition2.FlowQuantity, Is.EqualTo(expectedBoundaryQuantyType), $"Boundary {flowBoundaryCondition2.Name} expected type {expectedBoundaryQuantyType}, but was {flowBoundaryCondition2.FlowQuantity}.");
            Assert.That(flowBoundaryCondition3.FlowQuantity, Is.EqualTo(expectedBoundaryQuantyType), $"Boundary {flowBoundaryCondition3.Name} expected type {expectedBoundaryQuantyType}, but was {flowBoundaryCondition3.FlowQuantity}.");
        }

        private static FlowBoundaryCondition GetBackgroundFunctionFlowQuantityType(FlowBoundaryConditionSeriesFactory factory, int elementIndex)
        {
            return factory.BackgroundFunctions.ElementAt(elementIndex).BoundaryCondition;
        }

        #region Helper methods

        private static FlowBoundaryCondition ChangeFlowBoundaryConditionToBedLoadTransportQuantityType(Feature2D feature)
        {
            var flowBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.MorphologyBedLoadTransport,
                                                                  BoundaryConditionDataType.TimeSeries)
            {
                Feature = feature,
                SedimentFractionNames = new List<string>()
                {
                    "Frac1",
                    "Frac2"
                }
            };
            flowBoundaryCondition.AddPoint(0);
            Assert.That(flowBoundaryCondition, Is.Not.Null, "There was a problem while trying to create a test FlowBoundaryCondition.");
            return flowBoundaryCondition;
        }

        private static Feature2D CreateFeature2D()
        {
            var feature = new Feature2D
            {
                Name = "Boundary1",
                Geometry =
                    new LineString(new[]
                    {
                        new Coordinate(0, 0),
                        new Coordinate(1, 0)
                    })
            };
            return feature;
        }

        #endregion
    }
}