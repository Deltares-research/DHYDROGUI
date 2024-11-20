using System;
using System.Linq;
using DelftTools.Functions.Filters;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.FunctionStores;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class FouFileFunctionStoreTest
    {
        [Test, Category(TestCategory.DataAccess)]
        public void GivenFouFileFunctionStore_ReadingFouFile_ShouldGiveCorrectFunctions()
        {
            //Arrange
            var path = TestHelper.GetTestFilePath(@"output_foufiles\flowfm_fou.nc");
            var store = new FouFileFunctionStore { Path = path };

            // Act
            var functions = store.Functions.ToArray();

            // Assert
            Assert.AreEqual(9, functions.Length);
            Assert.AreEqual(@"Maximum 001: water level, maximum value (mesh1d_fourier001_max)", functions[0].Name);
            Assert.AreEqual(@"Maximum 001: water level, maximum depth value (mesh1d_fourier001_max_depth)", functions[1].Name);
            Assert.AreEqual(@"Maximum 002: velocity magnitude, maximum value (mesh1d_fourier002_max)", functions[2].Name);
            Assert.AreEqual(@"Fourier analysis 003: volume_on_ground, average value (mesh1d_fourier003_mean)", functions[3].Name);
            Assert.AreEqual(@"Maximum 004: volume_on_ground, maximum value (mesh1d_fourier004_max)", functions[4].Name);
            Assert.AreEqual(@"Minimum 005: volume_on_ground, minimum value (mesh1d_fourier005_min)", functions[5].Name);
            Assert.AreEqual(@"Fourier analysis 006: waterdepth_on_ground, average value (mesh1d_fourier006_mean)", functions[6].Name);
            Assert.AreEqual(@"Maximum 007: waterdepth_on_ground, maximum value (mesh1d_fourier007_max)", functions[7].Name);
            Assert.AreEqual(@"Minimum 008: waterdepth_on_ground, minimum value (mesh1d_fourier008_min)", functions[8].Name);
        }

        [Test, Category(TestCategory.DataAccess)]
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

        [Test, Category(TestCategory.DataAccess)]
        public void GivenFouFileFunctionStoreTest_FileBasedItemActions_ShouldWorkCorrectly()
        {
            //Arrange
            var path = TestHelper.GetTestFilePath(@"output_foufiles\moergestels_broek_fou.nc");
            var pathToCopyTo = TestHelper.GetTestWorkingDirectoryGeneratedTestFilePath("nc", null, "_fou");
            var store = new FouFileFunctionStore();

            FileUtils.DeleteIfExists(pathToCopyTo);

            // Act & Assert
            store.CreateNew(path); // should do nothing (readonly)

            Assert.IsNull(store.Path);
            Assert.IsEmpty(store.Functions);

            store.SwitchTo(path);
            store.Open(path); // should do nothing (switch also opens)

            Assert.AreEqual(path, store.Path);
            Assert.NotNull(store.Functions);

            store.CopyTo(pathToCopyTo);
            store.SwitchTo(pathToCopyTo);

            Assert.AreEqual(pathToCopyTo, store.Path);
            Assert.NotNull(store.Functions);
        }

        [Test]
        public void GivenFouFileFunctionStore_CallingWriteMethods_ShouldReturnAnError()
        {
            //Arrange
            var store = new FouFileFunctionStore();
            var expectedError = "Function store is readonly";

            // Assert

            Assert.Throws<NotSupportedException>(()=> store.SetVariableValues<double>(null, null), expectedError);
            Assert.Throws<NotSupportedException>(() => store.RemoveFunctionValues(null), expectedError);
            Assert.Throws<NotSupportedException>(() => store.AddIndependentVariableValues(null, Array.Empty<double>()), expectedError);
            Assert.Throws<NotSupportedException>(() => store.UpdateVariableSize(null), expectedError);
        }
    }
}