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
        public void GivenModelWithFlowBoundaryCondition_WhenSettingTheFlowQuantityTypeToBedLoadTransport_ThenBoundaryConditionAreOfBedLoadTransportType()
        {
            //Given
            var mduPath = TestHelper.GetTestFilePath(@"ExBedLoadTransportBoundary\FlowFM.mdu");
            var localMduFilePath = TestHelper.CreateLocalCopy(mduPath);
            var model = new WaterFlowFMModel(localMduFilePath);

            var boundaryCondition = model.ModelDefinition.BoundaryConditions.ElementAt(0);
            Assert.NotNull(boundaryCondition);
            Assert.That(boundaryCondition, Is.TypeOf<FlowBoundaryCondition>());

            //When
            var feature = CreateFeature2D();
            boundaryCondition = ChangeFlowBoundaryConditionToBedLoadTransportQuantityType(feature);

            var flowBoundaryConditionPointDataA = new FlowBoundaryConditionPointData((FlowBoundaryCondition)boundaryCondition, 1, true);
            var flowBoundaryConditionPointDataB = new FlowBoundaryConditionPointData((FlowBoundaryCondition)boundaryCondition, 1, true);
            var flowBoundaryConditionPointDataC = new FlowBoundaryConditionPointData((FlowBoundaryCondition)boundaryCondition, 1, true);

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
            var totalOfBackGroundFunctions = factory.BackgroundFunctions.Count;
            Assert.That(totalOfBackGroundFunctions, Is.EqualTo(3));

            var boundaryConditionFlowQuantity1 = factory.BackgroundFunctions.ElementAt(0).BoundaryCondition.FlowQuantity;
            var boundaryConditionFlowQuantity2 = factory.BackgroundFunctions.ElementAt(1).BoundaryCondition.FlowQuantity;
            var boundaryConditionFlowQuantity3 = factory.BackgroundFunctions.ElementAt(2).BoundaryCondition.FlowQuantity;
            Assert.IsTrue(boundaryConditionFlowQuantity1 == FlowBoundaryQuantityType.MorphologyBedLoadTransport);
            Assert.IsTrue(boundaryConditionFlowQuantity2 == FlowBoundaryQuantityType.MorphologyBedLoadTransport);
            Assert.IsTrue(boundaryConditionFlowQuantity3 == FlowBoundaryQuantityType.MorphologyBedLoadTransport);
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
