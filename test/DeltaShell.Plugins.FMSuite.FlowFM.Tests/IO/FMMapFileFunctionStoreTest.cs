using System;
using System.Linq;
using DelftTools.Functions.Filters;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using NUnit.Framework;
using SharpMap.Extensions.CoordinateSystems;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    [Category(TestCategory.Slow)]
    public class FMMapFileFunctionStoreTest
    {
        [Test]
        public void OpenMapFileCheckFunctions_Sedimentation() // Issue #: DELFT3DFM-775
        {
            var store = new FMMapFileFunctionStore(null)
            {
                Path = TestHelper.GetTestFilePath("output_mapfiles\\sedimentation_map.nc")
            };
            Assert.AreEqual(43, store.Functions.Count);

            var groupings = store.GetFunctionGrouping().ToList();
            Assert.AreEqual(31, groupings.Count);

            var numberOfSingleGroupings = groupings.Count(g => g.Count() == 1);
            Assert.AreEqual(20, numberOfSingleGroupings);

            var numberOfMultipleGroupings = groupings.Count(g => g.Count() > 1);
            Assert.AreEqual(11, numberOfMultipleGroupings);
        }

        [Test]
        public void OpenMapFileCheckFunctions()
        {
            var store = new FMMapFileFunctionStore(null)
            {
                Path = TestHelper.GetTestFilePath("output_mapfiles\\bendprof_map.nc")
            };
            Assert.AreEqual(12, store.Functions.Count);
        }

        [Test]
        public void Open1DFileCheckFunctions()
        {
            /*var store = new FM1DFileFunctionStore()
            {
                Path = TestHelper.GetTestFilePath("output_mapfiles\\FlowFM_map.nc")
            };*/
            var store = new FMMapFileFunctionStore(null)
            {
                Path = TestHelper.GetTestFilePath("output_mapfiles\\FlowFM_map.nc")
            };
            Console.WriteLine(store.Functions.Count);
        }

        [Test]
        [Category("Quarantine")]
        public void OpenMapFileCheckFunctions_NcFileContaining3DimensionalDataWithLowerUgridVersion()
        {
            var store = new FMMapFileFunctionStore(null)
            {
                Path = TestHelper.GetTestFilePath(@"output_mapfiles\zm_dfm_map.nc")
            };
            Assert.AreEqual(19, store.Functions.Count);
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void OpenMapFileCheckMinMax()
        {
            var zmDfmMapFile = "zm_dfm_map.nc";
            var mapFilePath = TestHelper.GetTestFilePath(@"output_mapfiles\zm_dfm_map.nc");
            var store = new FMMapFileFunctionStore(null)
            {
                Path = mapFilePath
            };

            // 0.0 is the lowest and 17.8562 the highest value of the first time slice
            var storeFunction = store.Functions[0];
            Assert.AreEqual(0.0, store.GetMinValue<double>(storeFunction.Components[0]), 0.001);
            Assert.AreEqual(17.8562, store.GetMaxValue<double>(storeFunction.Components[0]), 0.001);
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void OpenSingleTimeSliceMapFileCheckWaterLevelFunction()
        {
            var store = new FMMapFileFunctionStore(null)
            {
                Path = TestHelper.GetTestFilePath("output_mapfiles\\bendprof_map.nc")
            };

            var waterLevelFunction = (UnstructuredGridCellCoverage)store.Functions.FirstOrDefault(f => f.Components[0].Name == "mesh2d_s1");

            Assert.IsNotNull(waterLevelFunction);
            Assert.IsNotNull(waterLevelFunction.Grid);
            Assert.AreEqual(400, waterLevelFunction.Grid.Cells.Count);
            Assert.AreEqual(4000, waterLevelFunction.GetValues().Count);
            Assert.AreEqual(10, waterLevelFunction.Time.Values.Count);
            Assert.AreEqual(new DateTime(1992, 8, 31), waterLevelFunction.Time.Values.First());
            Assert.AreEqual(2.74655, (double) waterLevelFunction.Components[0].Values[0], 0.001);
        }

        [Test]
        public void OpenSingleTimeSliceMapFileFilterTime()
        {
            var store = new FMMapFileFunctionStore(null)
            {
                Path = TestHelper.GetTestFilePath("output_mapfiles\\bendprof_map.nc")
            };

            var waterLevelFunction = (UnstructuredGridCellCoverage)store.Functions.FirstOrDefault(f => f.Components[0].Name == "mesh2d_s1");

            Assert.IsNotNull(waterLevelFunction);

            var timeSlice = waterLevelFunction.GetValues(new VariableValueFilter<DateTime>(waterLevelFunction.Time,
                                                                                           new DateTime(1992, 8, 31)));

            Assert.AreEqual(400, timeSlice.Count);
            Assert.AreEqual(2.74655, (double) timeSlice[0], 0.001);
        }

        [Test]
        public void OpenMapFileReadFlowElements()
        {
            var store = new FMMapFileFunctionStore(null)
            {
                Path = TestHelper.GetTestFilePath("output_mapfiles\\bendprof_map.nc")
            };

            var waterLevelFunction = (UnstructuredGridCellCoverage)store.Functions.FirstOrDefault(f => f.Components[0].Name == "mesh2d_s1");

            Assert.IsNotNull(waterLevelFunction);
            Assert.AreEqual(400, waterLevelFunction.Arguments[1].Values.Count);
            Assert.AreEqual(0, waterLevelFunction.Arguments[1].Values[0]);
        }

        [Test]
        [Category("Quarantine")]
        public void OpenMapFileCheckWaterLevelFunction()
        {
            var zmDfmMapFile = "zm_dfm_map.nc";
            var mapFilePath = TestHelper.GetTestFilePath("output_mapfiles\\zm_dfm_map.nc");
            var store = new FMMapFileFunctionStore(null)
            {
                Path = mapFilePath
            };

            // 0.0 is the lowest and 17.8562 the highest value of the first time slice
            var storeFunction = store.Functions[0];
            Assert.AreEqual(0.0, store.GetMinValue<double>(storeFunction.Components[0]), 0.001);
            Assert.AreEqual(17.8562, store.GetMaxValue<double>(storeFunction.Components[0]), 0.001);

            var waterLevelFunction = (UnstructuredGridCellCoverage)store.Functions.FirstOrDefault(f => f.Components[0].Name == "s1");

            Assert.IsNotNull(waterLevelFunction);
            Assert.IsNotNull(waterLevelFunction.Grid);
            Assert.AreEqual("water level (s1)", waterLevelFunction.Name);
            Assert.AreEqual(19456, waterLevelFunction.Grid.Cells.Count);
            Assert.AreEqual(2, waterLevelFunction.Time.Values.Count);
            Assert.AreEqual(38912, waterLevelFunction.GetValues().Count);
            Assert.AreEqual(new DateTime(2011, 8, 1, 0, 0, 0), waterLevelFunction.Time.Values.First());
            Assert.AreEqual(new DateTime(2011, 8, 1, 2, 0, 0), waterLevelFunction.Time.Values.Last());
            Assert.AreEqual(0.0d, (double)waterLevelFunction.Components[0].Values[0], 0.001);
            Assert.AreEqual("m", waterLevelFunction.Components[0].Unit.Symbol);

            var filter = new VariableValueFilter<DateTime>(waterLevelFunction.Time, waterLevelFunction.Time.Values.First());
            Assert.AreEqual(19456, waterLevelFunction.GetValues(filter).Count);
            Assert.AreEqual(new DateTime(2011, 8, 1, 0, 0, 0), waterLevelFunction.Time.GetValues(filter)[0]);
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void ReadMapFileBoundaryLinkValues()
        {
            var path = TestHelper.GetTestFilePath("flow1d2dMapFile\\dflow-fm_map.nc");

            var store = new FMMapFileFunctionStore(null);
            
            // triggers reading the nc file
            store.Path = path;

            var linkCoverageValues = store.BoundaryCellValues;
            Assert.AreEqual(11, linkCoverageValues.Count); // Number of variables

            var firstTimeSeries = linkCoverageValues[0];

            Assert.AreEqual(2, firstTimeSeries.Time.Values.Count); // Number of timesteps
            Assert.AreEqual(76, firstTimeSeries.Arguments[1].Values.Count); // Number of links

            var timeSeries =
                firstTimeSeries.GetValues(new VariableValueFilter<FlowLink>(firstTimeSeries.Arguments[1],
                    (FlowLink)firstTimeSeries.Arguments[1].Values[0]));
            Assert.AreEqual(2, timeSeries.Count);
            Assert.AreEqual("m", firstTimeSeries.Components[0].Unit.Name);

            var expectedValues = new[] { 0.0, -9.999 };
            Assert.AreEqual(expectedValues, timeSeries);
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void OpenUgridMapFileCheckFunctions()
        {
            var store = new FMMapFileFunctionStore(null)
            {
                Path = TestHelper.GetTestFilePath("output_mapfiles\\bendprof_map.nc")
            };

            var expectedCoverages = new string[11]
            {
                "mesh2d_czs", 
                "mesh2d_q1",
                "mesh2d_s0",
                "mesh2d_s1",
                "mesh2d_taus",
                "mesh2d_u0",
                "mesh2d_u1",
                "mesh2d_ucx",
                "mesh2d_ucy",
                "mesh2d_waterdepth",
                "mesh2d_Numlimdt"
            };

            Assert.AreEqual(12, store.Functions.Count); // total == 11 (including CustomVelocity coverage)
            
            // Check coverages from MapFile variables
            var coveragesFromMapFileVariables = store.Functions.Where(f => f.Components.Count == 1).ToArray();
            Assert.AreEqual(11, coveragesFromMapFileVariables.Length);

            expectedCoverages.ForEach(expectedCoverage =>
                 Assert.IsTrue(coveragesFromMapFileVariables.Any(c => c.Components[0].Name == expectedCoverage),
                    string.Format("Expected coverage missing after opening Map file: {0}", expectedCoverage)));
            
            coveragesFromMapFileVariables.Select(c => c.Components[0].Name).ForEach(coverageName =>
                Assert.IsTrue(expectedCoverages.Contains(coverageName),
                    string.Format("Unexpected coverage found after opening Map file: {0}", coverageName)));
            
            // Check CustomVelocity coverage
            var customVelocityCoverage = store.Functions.FirstOrDefault(f => f.Components.Count == 2);
            Assert.NotNull(customVelocityCoverage, "CustomVelocity coverage not found");

            Assert.AreEqual("mesh2d_ucx", customVelocityCoverage.Components[0].Name);
            Assert.AreEqual("mesh2d_ucy", customVelocityCoverage.Components[1].Name);
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void OpenMapFileAndSetCoordinateSystemShouldChangeCoordinateSystem()
        {
            var store = new FMMapFileFunctionStore(null)
            {
                Path = TestHelper.GetTestFilePath("output_mapfiles\\simplebox_hex7_map.nc")
            };

            var grid = (UnstructuredGrid) TypeUtils.GetField(store, "grid");

            Assert.AreEqual(28992, grid.CoordinateSystem.AuthorityCode); // Amersfoort RD new
            store.CoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(4326); // WGS84
            grid = (UnstructuredGrid)TypeUtils.GetField(store, "grid");
            Assert.That(grid.CoordinateSystem, Is.Not.Null);
            Assert.That(grid.CoordinateSystem.AuthorityCode, Is.EqualTo(4326));
        }

        [Test]
        public void Test_GivenAThreeDimensionalVariable_CorrectAmountOfValuesIsGiven()
        {
            var store = new FMMapFileFunctionStore(null)
            {
                Path = TestHelper.GetTestFilePath("output_mapfiles\\my_map.nc")
            };
            var function = (UnstructuredGridCellCoverage)store.Functions.FirstOrDefault( f => f.Components[0].Name == "mesh2d_sxtot");

            var filterTime = function.Time.Values.FirstOrDefault();
            var filter = new VariableValueFilter<DateTime>(function.Time, filterTime);
            try
            {
                var filteredValues = function.GetValues(filter);
                if (function.Components[0].ValueType == typeof(double))
                {
                    Assert.That(filteredValues.Cast<double>().ToArray().Length, Is.EqualTo(551));
                }
            }
            catch (Exception e)
            {
                Assert.Fail("Something went wrong sizing shape and object of mesh2d_sxtot (we cannot render in OpenGL!): " + e.Message);
            }
        }
    }
}