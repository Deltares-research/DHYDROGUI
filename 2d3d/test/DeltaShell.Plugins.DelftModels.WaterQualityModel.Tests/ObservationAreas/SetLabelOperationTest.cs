using DeltaShell.Plugins.DelftModels.WaterQualityModel.ObservationAreas;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Geometries;
using NetTopologySuite.Extensions.Grids;
using NUnit.Framework;
using SharpMap.Data.Providers;
using SharpMap.SpatialOperations;
using SharpMapTestUtils;
using PointwiseOperationType = DeltaShell.Plugins.DelftModels.WaterQualityModel.ObservationAreas.PointwiseOperationType;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.ObservationAreas
{
    [TestFixture]
    public class SetLabelOperationTest
    {
        private FeatureCollection featureCollection;
        private FeatureCollection mask;

        [SetUp]
        public void SetUp()
        {
            featureCollection = CreateSquareCoverageFeatureProvider();
            var polygons = new[]
            {
                new Feature()
                {
                    Geometry = new[]
                    {
                        new Coordinate(-5, -5),
                        new Coordinate(2, -5),
                        new Coordinate(2, 2),
                        new Coordinate(-5, 2)
                    }.ToPolygon()
                }
            };

            mask = new FeatureCollection(polygons, typeof(WaterQualityObservationAreaCoverage));
        }

        [Test]
        public void SetLabelOperation()
        {
            var operation = new SetLabelOperation()
            {
                Label = "party",
                OperationType = PointwiseOperationType.Overwrite
            };

            operation.SetInputData(SpatialOperation.MaskInputName, mask);
            operation.SetInputData(SpatialOperation.MainInputName, featureCollection);

            operation.Execute();

            var coverage = (WaterQualityObservationAreaCoverage) operation.Output.Provider.Features[0];

            CollectionAssert.AreEquivalent(new[]
            {
                "party",
                "party",
                "na",
                "party",
                "party",
                "na",
                "na",
                "na",
                "na"
            }, coverage.GetValuesAsLabels());
        }

        private static FeatureCollection CreateSquareCoverageFeatureProvider()
        {
            UnstructuredGrid grid = UnstructuredGridTestHelper.GenerateRegularGrid(3, 3, 1, 1);

            var coverage = new WaterQualityObservationAreaCoverage(grid);
            coverage.SetValuesAsLabels(new[]
            {
                "na",
                "na",
                "na",
                "na",
                "na",
                "na",
                "na",
                "na",
                "na"
            });
            coverage.Components[0].NoDataValue = -999.0;
            var coverageFeatureProvider = new FeatureCollection(new[]
            {
                coverage
            }, typeof(WaterQualityObservationAreaCoverage));
            return coverageFeatureProvider;
        }
    }
}