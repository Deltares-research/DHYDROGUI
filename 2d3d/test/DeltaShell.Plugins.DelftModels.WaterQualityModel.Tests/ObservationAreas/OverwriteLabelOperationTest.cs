using DeltaShell.Plugins.DelftModels.WaterQualityModel.ObservationAreas;
using NetTopologySuite.Extensions.Grids;
using NUnit.Framework;
using SharpMap.Data.Providers;
using SharpMap.SpatialOperations;
using SharpMapTestUtils;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.ObservationAreas
{
    [TestFixture]
    public class OverwriteLabelOperationTest
    {
        private FeatureCollection featureCollection;

        [SetUp]
        public void SetUp()
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
            featureCollection = coverageFeatureProvider;
        }

        [Test]
        public void SetLabelOperation()
        {
            var operation = new OverwriteLabelOperation()
            {
                Label = "batman",
                X = 2.5,
                Y = 2.5
            };

            operation.SetInputData(SpatialOperation.MainInputName, featureCollection);

            operation.Execute();

            var coverage = (WaterQualityObservationAreaCoverage) operation.Output.Provider.Features[0];

            CollectionAssert.AreEquivalent(new[]
            {
                "na",
                "na",
                "na",
                "na",
                "na",
                "na",
                "na",
                "na",
                "batman"
            }, coverage.GetValuesAsLabels());
        }
    }
}