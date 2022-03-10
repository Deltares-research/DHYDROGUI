using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Filters;
using DelftTools.Hydro.Link1d2d;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
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

                var store = new FMMapFileFunctionStore { Path = mapFilePath };

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
                var store = new FMMapFileFunctionStore { Path = mapFilePath };

                Assert.AreEqual(12, store.Functions.Count);
            });
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
                var store = new FMMapFileFunctionStore { Path = mapFilePath };

                Assert.AreEqual(19, store.Functions.Count);
            });
        }

        [Test]
        [Category(TestCategory.Slow)]
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
                var store = new FMMapFileFunctionStore { Path = mapFilePath };

                // 0.0 is the lowest and 17.8562 the highest value of the first time slice
                IFunction storeFunction = store.Functions[0];
                Assert.AreEqual(0.0, store.GetMinValue<double>(storeFunction.Components[0]), 0.001);
                Assert.AreEqual(17.8562, store.GetMaxValue<double>(storeFunction.Components[0]), 0.001);
            });
        }

        [Test]
        [Category(TestCategory.Slow)]
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

                var store = new FMMapFileFunctionStore { Path = mapFilePath };

                var waterLevelFunction = (UnstructuredGridCellCoverage)store.Functions.FirstOrDefault(f => f.Components[0].Name == "mesh2d_s1");

                Assert.IsNotNull(waterLevelFunction);
                Assert.IsNotNull(waterLevelFunction.Grid);
                Assert.AreEqual(400, waterLevelFunction.Grid.Cells.Count);
                Assert.AreEqual(4000, waterLevelFunction.GetValues().Count);
                Assert.AreEqual(10, waterLevelFunction.Time.Values.Count);
                Assert.AreEqual(new DateTime(1992, 8, 31), waterLevelFunction.Time.Values.First());
                Assert.AreEqual(2.74655, (double)waterLevelFunction.Components[0].Values[0], 0.001);
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

                var store = new FMMapFileFunctionStore { Path = mapFilePath };

                var waterLevelFunction = (UnstructuredGridCellCoverage)store.Functions.FirstOrDefault(f => f.Components[0].Name == "mesh2d_s1");

                Assert.IsNotNull(waterLevelFunction);

                IMultiDimensionalArray timeSlice = waterLevelFunction.GetValues(new VariableValueFilter<DateTime>(waterLevelFunction.Time,
                                                                                                                  new DateTime(1992, 8, 31)));

                Assert.AreEqual(400, timeSlice.Count);
                Assert.AreEqual(2.74655, (double)timeSlice[0], 0.001);
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

                var store = new FMMapFileFunctionStore { Path = mapFilePath };

                var waterLevelFunction = (UnstructuredGridCellCoverage)store.Functions.FirstOrDefault(f => f.Components[0].Name == "mesh2d_s1");

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
                var store = new FMMapFileFunctionStore { Path = mapFilePath };

                // 0.0 is the lowest and 17.8562 the highest value of the first time slice
                IFunction storeFunction = store.Functions[0];
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
            });
        }

        [Test]
        [Category(TestCategory.Slow)]
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
                var store = new FMMapFileFunctionStore { Path = mapFilePath };

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
        [Category(TestCategory.Slow)]
        public void OpenMapFileAndSetCoordinateSystemShouldChangeCoordinateSystem()
        {
            string testDataFilePath = TestHelper.GetTestFilePath(@"output_mapfiles");
            string zmDfmZipFilePath = Path.Combine(testDataFilePath, "zm_dfm_map.zip");

            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                FileUtils.CopyDirectory(testDataFilePath, tempDir);
                ZipFileUtils.Extract(zmDfmZipFilePath, tempDir);

                var store = new FMMapFileFunctionStore { Path = Path.Combine(tempDir, "simplebox_hex7_map.nc") };

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

                var store = new FMMapFileFunctionStore { Path = mapFilePath };
                var function = (UnstructuredGridCellCoverage)store.Functions.FirstOrDefault(f => f.Components[0].Name == "mesh2d_sxtot");

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
        /// AND an error is logged.
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

                var store = new FMMapFileFunctionStore { Path = mapFilePath };

                const string compName = "mesh2d_q1";
                var function =
                    (UnstructuredGridEdgeCoverage)store.Functions
                                                        .FirstOrDefault(f => f.Components[0].Name == compName);

                // When
                void testAction()
                {
                    DateTime filterTime = function.Time.Values.FirstOrDefault();
                    var filter = new VariableValueFilter<DateTime>(function.Time, filterTime);

                    IMultiDimensionalArray filteredValues = function.GetValues(filter);

                    // Trigger lazy initialization
                    IEnumerator _ = filteredValues.GetEnumerator();
                }

                void executeTestActionWithoutException()
                {
                    Assert.DoesNotThrow(testAction);
                }

                List<string> msgs = TestHelper.GetAllRenderedMessages(executeTestActionWithoutException)?.ToList();

                // Then
                Assert.That(msgs, Is.Not.Null, "Expected the messages not to be null:");
                Assert.That(msgs, Has.Count.EqualTo(1), "Expected a single message when accessing a variable:");

                string expectedMsg = string.Format("While reading variable {0} from the file {1} an error was encountered: {2}",
                                                   compName, mapFileName, ""); // we ignore the actual error message, and just test for the beginning of the message.
                Assert.IsTrue(msgs[0].StartsWith(expectedMsg), "Expected a different msg:");
            }
        }

        private static void Get3DMapFile(TemporaryDirectory tempDir)
        {
            const string zipName = "map3d.zip";
            const string relPath = @"output_mapfiles\" + zipName;

            string srcPath = TestHelper.GetTestFilePath(relPath);

            ZipFileUtils.Extract(srcPath, tempDir.Path);
        }

        [Test]
        public void GivenFMMapFileFunctionStore_Reading1D2DFunctions_ShouldGiveCorrectData()
        {
            //Arrange
            var path = TestHelper.GetTestFilePath(@"output_mapfiles\1D2DWithLinks.nc");
            Assert.IsTrue(File.Exists(path));

            // Act
            var store = new FMMapFileFunctionStore { Path = path };

            // Assert
            Assert.AreEqual(9, store.Functions.OfType<UnstructuredGridCoverage>().Count());
            
            var linkCoverages = store.Functions.OfType<IFeatureCoverage>().ToList();
            Assert.AreEqual(2, linkCoverages.Count);

            var featureCoverage = linkCoverages[0];

            var numberOfLinks = 67;
            var numberOfTimeSteps = 4;
            var firstTimeStep = new DateTime(2020, 06, 10);
            var secondTimeStep = new DateTime(2020, 06, 10, 00, 20, 00);
            var expectedValueFirstLinkSecondTimeStep = 3.771207220762112E-9;

            // check links
            Assert.AreEqual(numberOfLinks,featureCoverage.Features.Count);
            Assert.AreEqual(numberOfLinks, featureCoverage.FeatureVariable.Values.Count);
            Assert.AreEqual(store.Links[0], featureCoverage.FeatureVariable.Values[0]);
            
            // check times
            Assert.AreEqual(numberOfTimeSteps, featureCoverage.Time.Values.Count);
            Assert.AreEqual(firstTimeStep, featureCoverage.Time.Values[0]);

            // check values
            var allValues = featureCoverage.GetValues<double>();
            Assert.AreEqual(numberOfTimeSteps * numberOfLinks, allValues.Count);
            Assert.AreEqual(expectedValueFirstLinkSecondTimeStep, allValues[67]);

            // check filtering on time
            var secondTimeSlice = (IList<double>) featureCoverage[secondTimeStep];

            Assert.AreEqual(numberOfLinks, secondTimeSlice.Count);
            Assert.AreEqual(expectedValueFirstLinkSecondTimeStep, secondTimeSlice[0]);

            // check filtering on link and on link + time
            var linkFilter = new VariableValueFilter<ILink1D2D>(featureCoverage.FeatureVariable,store.Links[0]);
            var timeFilter = new VariableValueFilter<DateTime>(featureCoverage.Time, secondTimeStep);
            
            var featureSlice = (IList<double>)featureCoverage.GetValues<double>(linkFilter);
            
            Assert.AreEqual(numberOfTimeSteps, featureSlice.Count);
            Assert.AreEqual(expectedValueFirstLinkSecondTimeStep, featureSlice[1]);

            var featureValue = (IList<double>)featureCoverage.GetValues<double>(linkFilter, timeFilter);
            Assert.AreEqual(1, featureValue.Count);
            Assert.AreEqual(expectedValueFirstLinkSecondTimeStep, featureValue[0]);
        }
    }
}