using System.Linq;
using DelftTools.Functions.Filters;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.FunctionStores;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class FouFileFunctionStoreTest
    {
        [Test]
        public void GivenFouFileFunctionStore_ReadingFouFile_ShouldGiveCorrectCoverages()
        {
            //Arrange
            var path = TestHelper.GetTestFilePath(@"output_foufiles\moergestels_broek_fou.nc");
            var store = new FouFileFunctionStore { Path = path };

            // Act
            var functions = store.Functions;

            // Assert

            // check NetworkCoverages
            Assert.AreEqual(8, functions.OfType<INetworkCoverage>().Count());
            
            var firstNetworkCoverage = functions.OfType<INetworkCoverage>().First();
            var component = firstNetworkCoverage.Components[0];
            Assert.NotNull(firstNetworkCoverage.Network);

            var locations = firstNetworkCoverage.Locations.Values;
            var locationValues = firstNetworkCoverage.GetValues();

            Assert.AreEqual(296, locations.Count);
            Assert.AreEqual(296, locationValues.Count);

            // check filtering
            var locationFilter = new VariableValueFilter<INetworkLocation>(firstNetworkCoverage.Arguments[0], new[] { locations[6], locations[9]});
            Assert.AreEqual(new[] { locationValues[6], locationValues[9] }, firstNetworkCoverage.GetValues(locationFilter));

            // check min/max
            Assert.AreEqual(11.6384, (double)component.MaxValue, 1e-4);
            Assert.AreEqual(8.53675, (double)component.MinValue, 1e-4);

            // check UnstructuredGridCellCoverages
            Assert.AreEqual(8, functions.OfType<UnstructuredGridCellCoverage>().Count());
            var firstCellCoverage = functions.OfType<UnstructuredGridCellCoverage>().FirstOrDefault();
            var argument = firstCellCoverage.Arguments[0];
            component = firstCellCoverage.Components[0];

            Assert.NotNull(firstCellCoverage.Grid);
            Assert.AreEqual(8745, argument.Values.Count);
            
            var cellValues = firstCellCoverage.GetValues();
            Assert.AreEqual(8745, cellValues.Count);

            // check filtering
            var cellFilter = new VariableValueFilter<int>(argument, new []{3, 5});
            Assert.AreEqual(new [] {cellValues[3], cellValues[5]}, firstCellCoverage.GetValues(cellFilter));

            // check min/max
            Assert.AreEqual(15, (double)component.MaxValue, 1e-4);
            Assert.AreEqual(15, (double)component.MinValue, 1e-4);
        }
    }
}