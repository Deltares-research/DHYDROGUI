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
    [Category(TestCategory.Integration)]
    public class FlowBoundaryConditionSeriesFactoryIntegrationTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void Given_FmModel_With_Empty_FlowBoundaryCondition_When_Changed_To_BedLoadTransport_ThenBoundaryConditionAreOfBedLoadTransportType()
        {
            //Given
            var expectedBackgroundFunctions = 3;
            FlowBoundaryQuantityType expectedBoundaryQuantyType = FlowBoundaryQuantityType.MorphologyBedLoadTransport;
            string mduPath = TestHelper.GetTestFilePath(@"ExBedLoadTransportBoundary\FlowFM.mdu");
            string localMduFilePath = TestHelper.CreateLocalCopy(mduPath);
            var model = new WaterFlowFMModel(localMduFilePath);

            FlowBoundaryCondition boundaryCondition = model.ModelDefinition.BoundaryConditions.ElementAt(0) as FlowBoundaryCondition;
            Assert.That(boundaryCondition, Is.Not.Null);
            
            //When
            Feature2D feature = CreateFeature2D();
            boundaryCondition = ChangeFlowBoundaryConditionToBedLoadTransportQuantityType(feature);
            int supportPoint = 1;
            bool useLayers = true;
            var flowBoundaryConditionPointDataA = new FlowBoundaryConditionPointData(boundaryCondition, supportPoint, useLayers);
            var flowBoundaryConditionPointDataB = new FlowBoundaryConditionPointData(boundaryCondition, supportPoint, useLayers);
            var flowBoundaryConditionPointDataC = new FlowBoundaryConditionPointData(boundaryCondition, supportPoint, useLayers);

            var factory = new FlowBoundaryConditionSeriesFactory
            {
                BackgroundFunctions = new EventedList<FlowBoundaryConditionPointData>()
                {
                    flowBoundaryConditionPointDataA,
                    flowBoundaryConditionPointDataB,
                    flowBoundaryConditionPointDataC
                }
            };

            //Then
            int totalOfBackGroundFunctions = factory.BackgroundFunctions.Count;
            Assert.That(totalOfBackGroundFunctions, Is.EqualTo(expectedBackgroundFunctions));

            FlowBoundaryQuantityType boundaryConditionFlowQuantity1 = GetBackgroundFunctionFlowQuantityType(factory, 0);
            FlowBoundaryQuantityType boundaryConditionFlowQuantity2 = GetBackgroundFunctionFlowQuantityType(factory, 1);
            FlowBoundaryQuantityType boundaryConditionFlowQuantity3 = GetBackgroundFunctionFlowQuantityType(factory, 2);

            Assert.That(boundaryConditionFlowQuantity1 , Is.EqualTo(expectedBoundaryQuantyType));
            Assert.That(boundaryConditionFlowQuantity2 , Is.EqualTo(expectedBoundaryQuantyType));
            Assert.That(boundaryConditionFlowQuantity3 , Is.EqualTo(expectedBoundaryQuantyType));
        }

        private static FlowBoundaryQuantityType GetBackgroundFunctionFlowQuantityType(FlowBoundaryConditionSeriesFactory factory, int elementIndex)
        {
            return factory.BackgroundFunctions.ElementAt(elementIndex).BoundaryCondition.FlowQuantity;
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
            Assert.NotNull(flowBoundaryCondition);
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
