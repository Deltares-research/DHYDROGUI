using DelftTools.Functions.Filters;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using NUnit.Framework;
using SharpMap.Extensions.CoordinateSystems;
using System;
using System.IO;
using System.Linq;

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
            var store = new FMMapFileFunctionStore()
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
            var store = new FMMapFileFunctionStore()
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
            var store = new FMMapFileFunctionStore()
            {
                Path = TestHelper.GetTestFilePath("output_mapfiles\\FlowFM_map.nc")
            };
            Console.WriteLine(store.Functions.Count);
        }

        [Test]
        public void OpenMapFileCheckFunctions_NcFileContaining3DimensionalDataWithLowerUgridVersion()
        {
            var store = new FMMapFileFunctionStore()
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
            var store = new FMMapFileFunctionStore()
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
            var store = new FMMapFileFunctionStore()
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
            var store = new FMMapFileFunctionStore()
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
            var store = new FMMapFileFunctionStore()
            {
                Path = TestHelper.GetTestFilePath("output_mapfiles\\bendprof_map.nc")
            };

            var waterLevelFunction = (UnstructuredGridCellCoverage)store.Functions.FirstOrDefault(f => f.Components[0].Name == "mesh2d_s1");

            Assert.IsNotNull(waterLevelFunction);
            Assert.AreEqual(400, waterLevelFunction.Arguments[1].Values.Count);
            Assert.AreEqual(0, waterLevelFunction.Arguments[1].Values[0]);
        }

        [Test]
        public void OpenMapFileCheckWaterLevelFunction()
        {
            var zmDfmMapFile = "zm_dfm_map.nc";
            var mapFilePath = TestHelper.GetTestFilePath("output_mapfiles\\zm_dfm_map.nc");
            var store = new FMMapFileFunctionStore()
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

            var store = new FMMapFileFunctionStore();

            // triggers reading the nc file
            store.Path = path;

            var linkCoverageValues = store.BoundaryCellValues;
            Assert.AreEqual(9, linkCoverageValues.Count); // Number of variables

            var firstTimeSeries = linkCoverageValues[0];

            Assert.AreEqual(181, firstTimeSeries.Time.Values.Count); // Number of timesteps
            Assert.AreEqual(77, firstTimeSeries.Arguments[1].Values.Count); // Number of links

            var timeSeries =
                firstTimeSeries.GetValues(new VariableValueFilter<FlowLink>(firstTimeSeries.Arguments[1],
                    (FlowLink) firstTimeSeries.Arguments[1].Values[0]));
            Assert.AreEqual(181, timeSeries.Count);
            Assert.AreEqual("m", firstTimeSeries.Components[0].Unit.Name);

            // expected values created with a csv export of a nc file viewer
            var expectedValues = new[]
            {
                0.0, 0.5016151688871872, 0.4984981945506296, 0.4983246035437028, 0.4984468728385758,
                0.4982877432819607, 0.4945215081404562, 0.47428846629110527, 0.4504568937755573,
                0.45222280105351276, 0.47021022447089994, 0.484984472527407, 0.49071417349044644,
                0.4882775914452664, 0.48114551605445793, 0.47323722333985574, 0.4677780988256155,
                0.46591883207680007, 0.4670062136524717, 0.4696348810104308, 0.47246647199396496,
                0.47461419243142994, 0.4757291258092914, 0.4759080383408703, 0.47550269924301736,
                0.47491463080033114, 0.474450112679049, 0.47426631887086024, 0.47439107242644313,
                0.4747760038642269, 0.4753476097417516, 0.47603811504600896, 0.47679483003620926,
                0.4775763866790097, 0.47834615021587856, 0.4790694725168254, 0.47971582523153955,
                0.48026279514767034, 0.4806982057746433, 0.48101869693188243, 0.48122590280711464,
                0.4813228806941464, 0.4813129082211911, 0.4812009671527714, 0.4809965117096207,
                0.4807154949276985, 0.48038027061464794, 0.48001724773832133, 0.4796532062438241,
                0.4793115806518248, 0.47900981869365167, 0.478758342109536, 0.47856100349735026,
                0.47841656458807635, 0.4783206272267452, 0.4782674763557172, 0.4782514309530393,
                0.4782675410036266, 0.47831169484691316, 0.4783803309580706, 0.47846998550395636,
                0.4785768790392656, 0.478696677272734, 0.4788244776556351, 0.4789549994608575,
                0.4790829037661548, 0.47920315553457393, 0.47931135721955614, 0.47940399625336017,
                0.479478566496055, 0.47953357008343017, 0.4795684451069345, 0.4795834675959544,
                0.47957965580555817, 0.47955868211835184, 0.4795227831200086, 0.4794746536277845,
                0.47941731432874696, 0.4793539519728339, 0.47928774146441366, 0.4792216667940469,
                0.4791583602939965, 0.4790999769166425, 0.47904811352590215, 0.47900377479767053,
                0.4789673794990469, 0.47893879551079005, 0.4789173898585814, 0.47890208147211544,
                0.4788913899596313, 0.47888347703792217, 0.47887617622700773, 0.47886700921139513,
                0.478853192240801, 0.4788316370669103, 0.4787989495073961, 0.47875142811984195,
                0.4786850666062926, 0.47859556644774914, 0.47847837103692903, 0.47832873895307393,
                0.47814188074941155, 0.47791318683931494, 0.47763856852793996, 0.4773149491631148,
                0.4769409374478706, 0.4765446510844277, 0.4760568835393221, 0.4755351247511915,
                0.47498990196100144, 0.4743315421680506, 0.4735991027270344, 0.47302744260732155,
                0.47275828940767856, 0.47288713230937673, 0.4734348633776264, 0.4742740918024276,
                0.47543727537701264, 0.4766940611149602, 0.4775398885466129, 0.47775456060186183,
                0.47738487963152043, 0.47655454721407803, 0.4756761221184401, 0.4752198672231446,
                0.47514530347796136, 0.4752449534257467, 0.4753496961622484, 0.47544427692541946,
                0.4756194761266356, 0.4758814060746545, 0.476111729875775, 0.4761915470284859,
                0.4761489052826375, 0.47612988412452695, 0.4762665514252349, 0.4766494857770412,
                0.4770267469606514, 0.4771575728321508, 0.4769511974294556, 0.47656353519016903,
                0.476277874371461, 0.4762561395071059, 0.47664294478909625, 0.4771094293397533,
                0.4773339294403751, 0.47717617980277616, 0.4767785982398074, 0.4764415534544358,
                0.4763338265219125, 0.4766055558608108, 0.47694661601593624, 0.47704091673002014,
                0.4767613391002825, 0.47626189884650205, 0.47585140984798485, 0.47568871506311516,
                0.47593417045345554, 0.47628387599757427, 0.47642333387173746, 0.4762218897176532,
                0.475821214174172, 0.4755090217393354, 0.47544832878221865, 0.4757931497567923,
                0.47623383691974697, 0.476459612970358, 0.47633546328586984, 0.4760169365342063,
                0.4756868681053472, 0.4757005547480184, 0.4760534647514293, 0.4764722046033577,
                0.47670496689360564, 0.47671489328125016, 0.47666046954639685, 0.47669824279383,
                0.4769500917277306, 0.47720047338203453, 0.4772065516004299, 0.47687570438814786
            };
            Assert.AreEqual(expectedValues, timeSeries);
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void OpenUgridMapFileCheckFunctions()
        {
            var store = new FMMapFileFunctionStore()
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
            var store = new FMMapFileFunctionStore()
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
            var store = new FMMapFileFunctionStore()
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