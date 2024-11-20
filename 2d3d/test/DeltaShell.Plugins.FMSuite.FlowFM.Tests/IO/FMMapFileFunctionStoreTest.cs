using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Filters;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.Common.FunctionStores;
using DeltaShell.Plugins.FMSuite.FlowFM.FunctionStores;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;
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
            string testDataFilePath = TestHelper.GetTestFilePath(@"output_mapfiles");
            var zmDfmZipFileName = "zm_dfm_map.zip";
            string zmDfmZipFilePath = Path.Combine(testDataFilePath, zmDfmZipFileName);

            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                FileUtils.CopyDirectory(testDataFilePath, tempDir);
                ZipFileUtils.Extract(zmDfmZipFilePath, tempDir);

                var simpleBoxMapFileName = "sedimentation_map.nc";
                string mapFilePath = Path.Combine(tempDir, simpleBoxMapFileName);

                var store = new FMMapFileFunctionStore {Path = mapFilePath};

                Assert.AreEqual(43, store.Functions.Count);

                List<IGrouping<string, IFunction>> groupings = store.GetFunctionGrouping().ToList();
                Assert.AreEqual(31, groupings.Count);

                int numberOfSingleGroupings = groupings.Count(g => g.Count() == 1);
                Assert.AreEqual(20, numberOfSingleGroupings);

                int numberOfMultipleGroupings = groupings.Count(g => g.Count() > 1);
                Assert.AreEqual(11, numberOfMultipleGroupings);
            });
        }

        [Test]
        public void OpenMapFileCheckFunctions()
        {
            string testDataFilePath = TestHelper.GetTestFilePath(@"output_mapfiles");
            var zmDfmZipFileName = "zm_dfm_map.zip";
            string zmDfmZipFilePath = Path.Combine(testDataFilePath, zmDfmZipFileName);

            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                FileUtils.CopyDirectory(testDataFilePath, tempDir);
                ZipFileUtils.Extract(zmDfmZipFilePath, tempDir);

                var bendprofMapFileName = "bendprof_map.nc";
                string mapFilePath = Path.Combine(tempDir, bendprofMapFileName);
                var store = new FMMapFileFunctionStore {Path = mapFilePath};

                Assert.AreEqual(12, store.Functions.Count);
            });
        }

        [Test]
        [TestCase(@"output_mapfiles\FlowFMWithTimeZones_map.nc", "Wednesday, 09 August 1950 00:00:00")]
        [TestCase(@"output_mapfiles\FlowFMWithoutTimeZones_map.nc", "Monday, 31 August 1992 00:00:00")]
        public void OpenMapFileWithOrWithoutTimeZones_ShouldSetReferenceDateInFunctionsCorrectly(string mapFilePath, string expectedReferenceDate)
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Arrange
                string mapFilePathTemp = tempDirectory.CopyTestDataFileToTempDirectory(TestHelper.GetTestFilePath(mapFilePath));

                // Act
                var store = new FMMapFileFunctionStore {Path = mapFilePathTemp};

                // Assert
                Assert.IsInstanceOf<FMNetCdfFileFunctionStore>(store);

                string retrievedReferenceDate = ((ICoverage) store.Functions.First()).Time.Attributes["ncRefDate"];
                Assert.AreEqual(expectedReferenceDate, retrievedReferenceDate);
            }
        }

        [Test]
        public void OpenMapFileCheckFunctions_NcFileContaining3DimensionalDataWithLowerUgridVersion()
        {
            string testDataFilePath = TestHelper.GetTestFilePath(@"output_mapfiles");
            var zmDfmZipFileName = "zm_dfm_map.zip";
            string zmDfmZipFilePath = Path.Combine(testDataFilePath, zmDfmZipFileName);

            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                FileUtils.CopyDirectory(testDataFilePath, tempDir);
                ZipFileUtils.Extract(zmDfmZipFilePath, tempDir);

                var zmDfmMapFile = "zm_dfm_map.nc";
                string mapFilePath = Path.Combine(tempDir, zmDfmMapFile);
                var store = new FMMapFileFunctionStore {Path = mapFilePath};

                Assert.AreEqual(19, store.Functions.Count);
            });
        }

        [Test]
        public void OpenMapFileCheckMinMax()
        {
            string testDataFilePath = TestHelper.GetTestFilePath(@"output_mapfiles");
            var zmDfmZipFileName = "zm_dfm_map.zip";
            string zmDfmZipFilePath = Path.Combine(testDataFilePath, zmDfmZipFileName);

            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                FileUtils.CopyDirectory(testDataFilePath, tempDir);
                ZipFileUtils.Extract(zmDfmZipFilePath, tempDir);

                var zmDfmMapFile = "zm_dfm_map.nc";
                string mapFilePath = Path.Combine(tempDir, zmDfmMapFile);
                var store = new FMMapFileFunctionStore {Path = mapFilePath};

                // 0.0 is the lowest and 17.8562 the highest value of the first time slice
                IFunction storeFunction = store.Functions[0];
                Assert.AreEqual(0.0, store.GetMinValue<double>(storeFunction.Components[0]), 0.001);
                Assert.AreEqual(17.8562, store.GetMaxValue<double>(storeFunction.Components[0]), 0.001);
            });
        }

        [Test]
        public void OpenSingleTimeSliceMapFileCheckWaterLevelFunction()
        {
            string testDataFilePath = TestHelper.GetTestFilePath(@"output_mapfiles");
            var zmDfmZipFileName = "zm_dfm_map.zip";
            string zmDfmZipFilePath = Path.Combine(testDataFilePath, zmDfmZipFileName);

            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                FileUtils.CopyDirectory(testDataFilePath, tempDir);
                ZipFileUtils.Extract(zmDfmZipFilePath, tempDir);

                var simpleBoxMapFileName = "bendprof_map.nc";
                string mapFilePath = Path.Combine(tempDir, simpleBoxMapFileName);

                var store = new FMMapFileFunctionStore {Path = mapFilePath};

                var waterLevelFunction = (UnstructuredGridCellCoverage) store.Functions.FirstOrDefault(f => f.Components[0].Name == "mesh2d_s1");

                Assert.IsNotNull(waterLevelFunction);
                Assert.IsNotNull(waterLevelFunction.Grid);
                Assert.AreEqual(400, waterLevelFunction.Grid.Cells.Count);
                Assert.AreEqual(4000, waterLevelFunction.GetValues().Count);
                Assert.AreEqual(10, waterLevelFunction.Time.Values.Count);
                Assert.AreEqual(new DateTime(1992, 8, 31), waterLevelFunction.Time.Values.First());
                Assert.AreEqual(2.74655, (double) waterLevelFunction.Components[0].Values[0], 0.001);
            });
        }

        [Test]
        public void OpenSingleTimeSliceMapFileFilterTime()
        {
            string testDataFilePath = TestHelper.GetTestFilePath(@"output_mapfiles");
            var zmDfmZipFileName = "zm_dfm_map.zip";
            string zmDfmZipFilePath = Path.Combine(testDataFilePath, zmDfmZipFileName);

            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                FileUtils.CopyDirectory(testDataFilePath, tempDir);
                ZipFileUtils.Extract(zmDfmZipFilePath, tempDir);

                var simpleBoxMapFileName = "bendprof_map.nc";
                string mapFilePath = Path.Combine(tempDir, simpleBoxMapFileName);

                var store = new FMMapFileFunctionStore {Path = mapFilePath};

                var waterLevelFunction = (UnstructuredGridCellCoverage) store.Functions.FirstOrDefault(f => f.Components[0].Name == "mesh2d_s1");

                Assert.IsNotNull(waterLevelFunction);

                IMultiDimensionalArray timeSlice = waterLevelFunction.GetValues(new VariableValueFilter<DateTime>(waterLevelFunction.Time,
                                                                                                                  new DateTime(1992, 8, 31)));

                Assert.AreEqual(400, timeSlice.Count);
                Assert.AreEqual(2.74655, (double) timeSlice[0], 0.001);
            });
        }

        [Test]
        public void OpenMapFileReadFlowElements()
        {
            string testDataFilePath = TestHelper.GetTestFilePath(@"output_mapfiles");
            var zmDfmZipFileName = "zm_dfm_map.zip";
            string zmDfmZipFilePath = Path.Combine(testDataFilePath, zmDfmZipFileName);

            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                FileUtils.CopyDirectory(testDataFilePath, tempDir);
                ZipFileUtils.Extract(zmDfmZipFilePath, tempDir);

                var simpleBoxMapFileName = "bendprof_map.nc";
                string mapFilePath = Path.Combine(tempDir, simpleBoxMapFileName);

                var store = new FMMapFileFunctionStore {Path = mapFilePath};

                var waterLevelFunction = (UnstructuredGridCellCoverage) store.Functions.FirstOrDefault(f => f.Components[0].Name == "mesh2d_s1");

                Assert.IsNotNull(waterLevelFunction);
                Assert.AreEqual(400, waterLevelFunction.Arguments[1].Values.Count);
                Assert.AreEqual(0, waterLevelFunction.Arguments[1].Values[0]);
            });
        }

        [Test]
        public void OpenMapFileCheckWaterLevelFunction()
        {
            string testDataFilePath = TestHelper.GetTestFilePath(@"output_mapfiles");
            var zmDfmZipFileName = "zm_dfm_map.zip";
            string zmDfmZipFilePath = Path.Combine(testDataFilePath, zmDfmZipFileName);

            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                FileUtils.CopyDirectory(testDataFilePath, tempDir);
                ZipFileUtils.Extract(zmDfmZipFilePath, tempDir);

                var zmDfmMapFile = "zm_dfm_map.nc";
                string mapFilePath = Path.Combine(tempDir, zmDfmMapFile);
                var store = new FMMapFileFunctionStore {Path = mapFilePath};

                // 0.0 is the lowest and 17.8562 the highest value of the first time slice
                IFunction storeFunction = store.Functions[0];
                Assert.AreEqual(0.0, store.GetMinValue<double>(storeFunction.Components[0]), 0.001);
                Assert.AreEqual(17.8562, store.GetMaxValue<double>(storeFunction.Components[0]), 0.001);

                var waterLevelFunction = (UnstructuredGridCellCoverage) store.Functions.FirstOrDefault(f => f.Components[0].Name == "s1");

                Assert.IsNotNull(waterLevelFunction);
                Assert.IsNotNull(waterLevelFunction.Grid);
                Assert.AreEqual("water level (s1)", waterLevelFunction.Name);
                Assert.AreEqual(19456, waterLevelFunction.Grid.Cells.Count);
                Assert.AreEqual(2, waterLevelFunction.Time.Values.Count);
                Assert.AreEqual(38912, waterLevelFunction.GetValues().Count);
                Assert.AreEqual(new DateTime(2011, 8, 1, 0, 0, 0), waterLevelFunction.Time.Values.First());
                Assert.AreEqual(new DateTime(2011, 8, 1, 2, 0, 0), waterLevelFunction.Time.Values.Last());
                Assert.AreEqual(0.0d, (double) waterLevelFunction.Components[0].Values[0], 0.001);
                Assert.AreEqual("m", waterLevelFunction.Components[0].Unit.Symbol);

                var filter = new VariableValueFilter<DateTime>(waterLevelFunction.Time, waterLevelFunction.Time.Values.First());
                Assert.AreEqual(19456, waterLevelFunction.GetValues(filter).Count);
                Assert.AreEqual(new DateTime(2011, 8, 1, 0, 0, 0), waterLevelFunction.Time.GetValues(filter)[0]);
            });
        }

        [Test]
        public void OpenUgridMapFileCheckFunctions()
        {
            string testDataFilePath = TestHelper.GetTestFilePath(@"output_mapfiles");
            var zmDfmZipFileName = "zm_dfm_map.zip";
            string zmDfmZipFilePath = Path.Combine(testDataFilePath, zmDfmZipFileName);

            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                FileUtils.CopyDirectory(testDataFilePath, tempDir);
                ZipFileUtils.Extract(zmDfmZipFilePath, tempDir);

                var zmDfmMapFile = "bendprof_map.nc";
                string mapFilePath = Path.Combine(tempDir, zmDfmMapFile);
                var store = new FMMapFileFunctionStore {Path = mapFilePath};

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
                IFunction[] coveragesFromMapFileVariables = store.Functions.Where(f => f.Components.Count == 1).ToArray();
                Assert.AreEqual(11, coveragesFromMapFileVariables.Length);

                expectedCoverages.ForEach(expectedCoverage =>
                                              Assert.IsTrue(coveragesFromMapFileVariables.Any(c => c.Components[0].Name == expectedCoverage),
                                                            string.Format("Expected coverage missing after opening Map file: {0}", expectedCoverage)));

                coveragesFromMapFileVariables.Select(c => c.Components[0].Name).ForEach(coverageName =>
                                                                                            Assert.IsTrue(expectedCoverages.Contains(coverageName),
                                                                                                          string.Format("Unexpected coverage found after opening Map file: {0}", coverageName)));

                // Check CustomVelocity coverage
                IFunction customVelocityCoverage = store.Functions.FirstOrDefault(f => f.Components.Count == 2);
                Assert.NotNull(customVelocityCoverage, "CustomVelocity coverage not found");

                Assert.AreEqual("mesh2d_ucx", customVelocityCoverage.Components[0].Name);
                Assert.AreEqual("mesh2d_ucy", customVelocityCoverage.Components[1].Name);
            });
        }

        [Test]
        public void OpenMapFileAndSetCoordinateSystemShouldChangeCoordinateSystem()
        {
            string testDataFilePath = TestHelper.GetTestFilePath(@"output_mapfiles");
            string zmDfmZipFilePath = Path.Combine(testDataFilePath, "zm_dfm_map.zip");

            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                FileUtils.CopyDirectory(testDataFilePath, tempDir);
                ZipFileUtils.Extract(zmDfmZipFilePath, tempDir);

                var store = new FMMapFileFunctionStore {Path = Path.Combine(tempDir, "simplebox_hex7_map.nc")};

                Assert.AreEqual(28992, store.Grid.CoordinateSystem.AuthorityCode); // Amersfoort RD new

                store.SetCoordinateSystem(new OgrCoordinateSystemFactory().CreateFromEPSG(4326)); // WGS84
                Assert.That(store.Grid.CoordinateSystem, Is.Not.Null);
                Assert.That(store.Grid.CoordinateSystem.AuthorityCode, Is.EqualTo(4326));
            });
        }

        [Test]
        public void Test_GivenAThreeDimensionalVariable_CorrectAmountOfValuesIsGiven()
        {
            string testDataFilePath = TestHelper.GetTestFilePath(@"output_mapfiles");
            var zmDfmZipFileName = "zm_dfm_map.zip";
            string zmDfmZipFilePath = Path.Combine(testDataFilePath, zmDfmZipFileName);

            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                FileUtils.CopyDirectory(testDataFilePath, tempDir);
                ZipFileUtils.Extract(zmDfmZipFilePath, tempDir);

                var zmDfmMapFile = "my_map.nc";
                string mapFilePath = Path.Combine(tempDir, zmDfmMapFile);

                var store = new FMMapFileFunctionStore {Path = mapFilePath};
                var function = (UnstructuredGridCellCoverage) store.Functions.FirstOrDefault(f => f.Components[0].Name == "mesh2d_sxtot");

                DateTime filterTime = function.Time.Values.FirstOrDefault();
                var filter = new VariableValueFilter<DateTime>(function.Time, filterTime);
                try
                {
                    IMultiDimensionalArray filteredValues = function.GetValues(filter);
                    if (function.Components[0].ValueType == typeof(double))
                    {
                        Assert.That(filteredValues.Cast<double>().ToArray().Length, Is.EqualTo(551));
                    }
                }
                catch (Exception e)
                {
                    Assert.Fail("Something went wrong sizing shape and object of mesh2d_sxtot (we cannot render in OpenGL!): " + e.Message);
                }
            });
        }

        /// <summary>
        /// GIVEN a 3D map file
        /// AND a function store reading this map file
        /// WHEN variables are retrieved
        /// THEN no exception is thrown
        /// AND no error is logged.
        /// </summary>
        [Test]

        public void GivenA3DMapFileAndAFunctionStoreReadingThisMapFile_WhenVariablesAreRetrieved_ThenNoExceptionIsThrownAndAnErrorIsLogged()
        {
            using (var tempDir = new TemporaryDirectory())
            {
                // Given
                Get3DMapFile(tempDir);
                const string mapFileName = "map3D.nc";

                string mapFilePath = Path.Combine(tempDir.Path, mapFileName);

                FMMapFileFunctionStore store = null;
                
                // When
                Action call = () => store = new FMMapFileFunctionStore {Path = mapFilePath};

                // Then
                IEnumerable messages = TestHelper.GetAllRenderedMessages(call)?.ToArray();

                Assert.That(messages, Is.Not.Null, "Expected the messages not to be null:");
                
                const string compName = "mesh2d_q1";
                Assert.That(store.Functions.Where(f => string.Equals(f.Components[0].Name, compName)), Is.Empty);
            }
        }

        private static void Get3DMapFile(TemporaryDirectory tempDir)
        {
            const string zipName = "map3d.zip";
            const string relPath = @"output_mapfiles\" + zipName;

            string srcPath = TestHelper.GetTestFilePath(relPath);

            ZipFileUtils.Extract(srcPath, tempDir.Path);
        }
    }
}