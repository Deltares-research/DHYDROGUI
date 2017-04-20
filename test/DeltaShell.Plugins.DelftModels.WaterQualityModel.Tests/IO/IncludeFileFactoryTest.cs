using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using DelftTools.TestUtils;
using DelftTools.Utils.IO;

using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.BoundaryData;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.Model;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Model;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;

using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.IO
{
    [TestFixture]
    public class IncludeFileFactoryTest
    {
        #region Block 2

        [Test]
        public void TestCreateOutputLocations()
        {
            var obsPoints = new Dictionary<string, IList<int>>()
            {
                {"obs1", new[] {1, 2, 3}},
                {"obs2", new[] {4, 5, 6}},
            };

            IncludeFileFactory factory = new IncludeFileFactory();

            var text = factory.CreateOutputLocationsInclude(obsPoints);

            string expectedString = "2 ; nr of monitor locations" + Environment.NewLine +
                                    "'obs1' 3" + Environment.NewLine + 
                                    "1" + Environment.NewLine +
                                    "2" + Environment.NewLine + 
                                    "3" + Environment.NewLine +
                                    "'obs2' 3" + Environment.NewLine + 
                                    "4" + Environment.NewLine +
                                    "5" + Environment.NewLine +
                                    "6" + Environment.NewLine;

            Assert.AreEqual(expectedString, text);
        }

        [Test]
        public void TestCreateZeroOutputLocations()
        {
            var obsPoints = new Dictionary<string, IList<int>>(0);

            IncludeFileFactory factory = new IncludeFileFactory();
            var text = factory.CreateOutputLocationsInclude(obsPoints);

            string expectedString = "0 ; nr of monitor locations" + Environment.NewLine;

            Assert.AreEqual(expectedString, text);
        }

        #endregion Block 2

        #region Block 3

        [Test]
        public void TestCreateNumberOfSegmentsInclude()
        {
            IncludeFileFactory factory = new IncludeFileFactory();
            const string expectedString = "446698 ; number of segments";

            Assert.AreEqual(expectedString, factory.CreateNumberOfSegmentsInclude(63814, 7));
        }

        [Test]
        public void TestCreateVolumesFileInclude()
        {
            IncludeFileFactory factory = new IncludeFileFactory();
            string expectedString = "-2 ; volumes will be interpolated from a binary file" + Environment.NewLine +
                                    "'uni3d.vol' ; volumes file from hyd file" + Environment.NewLine;

            Assert.AreEqual(expectedString, factory.CreateVolumesFileInclude("uni3d.vol"));
        }

        #endregion Block 3
       
        #region Block 4

        [Test]
        public void TestCreateNumberOfExchangesInclude()
        {
            IncludeFileFactory factory = new IncludeFileFactory();
            const string expectedString = "788900 0 382884 ; number of exchanges in three directions";

            Assert.AreEqual(expectedString, factory.CreateNumberOfExchangesInclude(788900, 382884));
        }

        [Test]
        public void TestCreatePointersFileInclude()
        {
            IncludeFileFactory factory = new IncludeFileFactory();
            string expectedString = "0 ; pointers from binary file." + Environment.NewLine +
                                    "'uni3d.poi' ; pointers file" + Environment.NewLine;

            Assert.AreEqual(expectedString, factory.CreatePointersFileInclude("uni3d.poi"));
        }

        [Test]
        public void TestCreateAreasFileInclude()
        {
            IncludeFileFactory factory = new IncludeFileFactory();
            string expectedString = "-2 ; areas will be interpolated from a binary file" + Environment.NewLine +
                                    "'uni3d.are' ; areas file" + Environment.NewLine;

            Assert.AreEqual(expectedString, factory.CreateAreasFileInclude("uni3d.are"));
        }

        [Test]
        public void TestCreateAttributesFileInclude()
        {
            IncludeFileFactory factory = new IncludeFileFactory();
            const string expectedString = "INCLUDE 'uni3d.atr' ; attributes file";

            Assert.AreEqual(expectedString, factory.CreateAttributesFileInclude("uni3d.atr"));
        }

        [Test]
        public void TestCreateFlowsFileInclude()
        {
            IncludeFileFactory factory = new IncludeFileFactory();
            string expectedString = "-2 ; flows from binary file" + Environment.NewLine +
                                    "'uni3d.flo' ; flows file" + Environment.NewLine;

            Assert.AreEqual(expectedString, factory.CreateFlowsFileInclude("uni3d.flo"));
        }

        [Test]
        public void TestCreateLengthsFileInclude()
        {
            IncludeFileFactory factory = new IncludeFileFactory();
            string expectedString = "0 ; Lengths from binary file" + Environment.NewLine +
                                    "'uni3d.len' ; lengths file" + Environment.NewLine;

            Assert.AreEqual(expectedString, factory.CreateLengthsFileInclude("uni3d.len"));
        }

        [Test]
        public void TestCreateConstantDispersionInclude()
        {
            IncludeFileFactory factory = new IncludeFileFactory();
            const string expectedString = "0.2 0.0 0.3 ; constant dispersion";

            var dispersion = WaterQualityFunctionFactory.CreateConst("Dispersion", 0.2d, "Dispersion", "m2/s", null);

            Assert.AreEqual(expectedString, factory.CreateConstantDispersionInclude(0.3d, dispersion));
        }

        [Test]
        public void TestCreateConstantDispersionInclude_HasSpatialData()
        {
            IncludeFileFactory factory = new IncludeFileFactory();
            const string expectedString = "0.0 0.0 0.3 ; constant dispersion";

            var dispersion = WaterQualityFunctionFactory.CreateUnstructuredGridCellCoverage("Dispersion", 0.2d, "Dispersion", "m2/s", null);

            Assert.AreEqual(expectedString, factory.CreateConstantDispersionInclude(0.3d, dispersion));
        }

        #endregion Block 4

        #region Block 5

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void TestGetBoundarySegmentsToWrite_Real()
        {
            string hydPath = TestHelper.GetTestFilePath(@"IO\real\uni3d.hyd");

            WaterQualityModel model = new WaterQualityModel();

            new HydFileImporter().ImportItem(hydPath, model);

            IncludeFileFactory factory = new IncludeFileFactory();
            string result =
                factory.CreateBoundaryListInclude(model.BoundaryNodeIds, model.NumberOfWaqSegmentLayers);

            string[] resultLines = result.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            // assert the count of the list
            int allSegmentsCount = model.BoundaryNodeIds.Sum(kvp => kvp.Value.Count());
            Assert.AreEqual(allSegmentsCount * model.NumberOfWaqSegmentLayers + model.NumberOfWaqSegmentLayers, resultLines.Length - 1);

            Assert.AreEqual("; Boundaries for layer 1", resultLines[1]);
            Assert.AreEqual("'1' '' 'sea_002.pli'", resultLines[2]);
            Assert.AreEqual("; Boundaries for layer 2", resultLines[2 + allSegmentsCount]);
            Assert.AreEqual("'139' '' 'sea_002.pli'", resultLines[3 + allSegmentsCount]);
        }

        [Test]
        public void TestCreateBoundaryListInclude()
        {
            const string boundaryNameOne = "one";
            const string boundaryNameTwo = "two";

            Dictionary<WaterQualityBoundary, int[]> boundaryNodes = new Dictionary<WaterQualityBoundary, int[]>(2)
            {
                {new WaterQualityBoundary() {Name = boundaryNameTwo}, new[] {6, 2}}, // added in the wrong order, but the result should still start with 1
                {new WaterQualityBoundary() {Name = boundaryNameOne}, new[] {1, 5, 3, 4}},
            };

            IncludeFileFactory factory = new IncludeFileFactory();

            string result = factory.CreateBoundaryListInclude(boundaryNodes, 3);

            string[] lines = result.Split(new []{ Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            Assert.AreEqual(22, lines.Length);

            // boundary 'one' is B2, because it is added second in the dictionary.
            // This doesn't matter, but it is also the index from the bnd file that comes from the hyd file.
            Assert.AreEqual("; Boundaries for layer 1", lines[1]);
            Assert.AreEqual("'1' '' 'one'", lines[2]);
            Assert.AreEqual("'2' '' 'two'", lines[3]);
            Assert.AreEqual("'3' '' 'one'", lines[4]);
            Assert.AreEqual("'4' '' 'one'", lines[5]);
            Assert.AreEqual("'5' '' 'one'", lines[6]);
            Assert.AreEqual("'6' '' 'two'", lines[7]);

            Assert.AreEqual("; Boundaries for layer 2", lines[8]);
            Assert.AreEqual("'7' '' 'one'", lines[9]);
            Assert.AreEqual("'8' '' 'two'", lines[10]);
            Assert.AreEqual("'9' '' 'one'", lines[11]);
            Assert.AreEqual("'10' '' 'one'", lines[12]);
            Assert.AreEqual("'11' '' 'one'", lines[13]);
            Assert.AreEqual("'12' '' 'two'", lines[14]);

            Assert.AreEqual("; Boundaries for layer 3", lines[15]);
            Assert.AreEqual("'13' '' 'one'", lines[16]);
            Assert.AreEqual("'14' '' 'two'", lines[17]);
            Assert.AreEqual("'15' '' 'one'", lines[18]);
            Assert.AreEqual("'16' '' 'one'", lines[19]);
            Assert.AreEqual("'17' '' 'one'", lines[20]);
            Assert.AreEqual("'18' '' 'two'", lines[21]);
        }

        [Test]
        public void CreateBoundaryDataIncludeTest()
        {
            // setup
            var workDirectory = Path.Combine(Directory.GetCurrentDirectory(), "My", "Work", "Dir" + Path.DirectorySeparatorChar);
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "tables" + Path.DirectorySeparatorChar);

            FileUtils.DeleteIfExists(workDirectory);
            Directory.CreateDirectory(workDirectory);
            FileUtils.DeleteIfExists(folderPath);
            Directory.CreateDirectory(folderPath);

            try
            {
                var manager = new DataTableManager { FolderPath = folderPath };
                manager.CreateNewDataTable("A", "B", "C.d", "E");
                manager.CreateNewDataTable("F", "G", "H.i", "J");
                manager.CreateNewDataTable("K", "L", "M.n", "O");

                var factory = new IncludeFileFactory();

                // call
                var boundaryDataIncludeFileContents = factory.CreateBoundaryDataInclude(manager, workDirectory);

                // assert
                var expectedContents =
                    @"INCLUDE '..\..\..\tables\K.tbl'" + Environment.NewLine +
                    @"INCLUDE '..\..\..\tables\F.tbl'" + Environment.NewLine +
                    @"INCLUDE '..\..\..\tables\A.tbl'" + Environment.NewLine;
                Assert.AreEqual(expectedContents, boundaryDataIncludeFileContents);
            }
            finally
            {
                FileUtils.DeleteIfExists(workDirectory);
                FileUtils.DeleteIfExists(folderPath);
            }
        }

        [Test]
        public void CreateBoundaryDataIncludeDisabledTest()
        {
            // setup
            var workDirectory = Path.Combine(Directory.GetCurrentDirectory(), "My", "Work", "Dir" + Path.DirectorySeparatorChar);
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "tables" + Path.DirectorySeparatorChar);

            FileUtils.DeleteIfExists(workDirectory);
            Directory.CreateDirectory(workDirectory);
            FileUtils.DeleteIfExists(folderPath);
            Directory.CreateDirectory(folderPath);

            try
            {
                var manager = new DataTableManager { FolderPath = folderPath };
                manager.CreateNewDataTable("A", "B", "C.d", "E");
                manager.CreateNewDataTable("F", "G", "H.i", "J");
                manager.CreateNewDataTable("K", "L", "M.n", "O");

                // disable the second datatable
                var dataTables = manager.DataTables.ToArray();
                dataTables[1].IsEnabled = false;

                var factory = new IncludeFileFactory();

                // call
                var boundaryDataIncludeFileContents = factory.CreateBoundaryDataInclude(manager, workDirectory);

                // assert
                var expectedContents =
                    @"INCLUDE '..\..\..\tables\K.tbl'" + Environment.NewLine +
                    @"INCLUDE '..\..\..\tables\A.tbl'" + Environment.NewLine;
                Assert.AreEqual(expectedContents, boundaryDataIncludeFileContents);
            }
            finally
            {
                FileUtils.DeleteIfExists(workDirectory);
                FileUtils.DeleteIfExists(folderPath);
            }
        }

        [Test]
        public void CreateBoundaryAliasesIncludeTest()
        {
            var input = new Dictionary<string, IList<string>>()
            {
                {"measure point 1", new List<string>() {"boundary 1", "boundary 2",}},
                {"measure point 2", new List<string>() {"boundary 1", "boundary 3",}},
                {"measure point 3", new List<string>() {"boundary 3", "boundary 1",}},
            };

            var factory = new IncludeFileFactory();

            var boundaryAliasesIncludeFileContents = factory.CreateBoundaryAliasesInclude(input);

            var expectedContents = "USEDATA_ITEM 'measure point 1' FORITEM" + Environment.NewLine +
                                   "'boundary 1'" + Environment.NewLine +
                                   "'boundary 2'" + Environment.NewLine +
                                   Environment.NewLine +
                                   "USEDATA_ITEM 'measure point 2' FORITEM" + Environment.NewLine +
                                   "'boundary 1'" + Environment.NewLine +
                                   "'boundary 3'" + Environment.NewLine +
                                   Environment.NewLine +
                                   "USEDATA_ITEM 'measure point 3' FORITEM" + Environment.NewLine +
                                   "'boundary 3'" + Environment.NewLine +
                                   "'boundary 1'" + Environment.NewLine +
                                   Environment.NewLine;

            Assert.AreEqual(expectedContents, boundaryAliasesIncludeFileContents);
        }
        
        #endregion Block 5

        #region Block 6

        [Test]
        public void TestCreateDryWasteLoadBlock()
        {
            // setup
            var load1 = new WaterQualityLoad
            {
                Name = "Load 1",
                LoadType = "Test"
            };
            var load2 = new WaterQualityLoad
            {
                Name = "Load 2",
                LoadType = "Test"
            };
            var load3 = new WaterQualityLoad
            {
                Name = "Load 3",
                LoadType = "haha"
            };

            var loadsAndIds = new Dictionary<WaterQualityLoad, int>(3);
            loadsAndIds[load1] = 948;
            loadsAndIds[load2] = 67;
            loadsAndIds[load3] = 0;

            IncludeFileFactory factory = new IncludeFileFactory();
            string expectedString =
                "; Number of loads" + Environment.NewLine +
                "3; Number of loads" + Environment.NewLine +
                ";SegmentID  Load-name  Comment  Load-type" + Environment.NewLine +
                "948 'Load 1' '' 'Test'" + Environment.NewLine +
                "67 'Load 2' '' 'Test'" + Environment.NewLine +
                "0 'Load 3' '' 'haha'" + Environment.NewLine;

            // call
            var text = factory.CreateDryWasteLoadInclude(loadsAndIds);

            // assert
            Assert.AreEqual(expectedString, text);
        }

        [Test]
        public void CreateLoadsDataIncludeTest()
        {
            // setup
            var workDirectory = Path.Combine(Directory.GetCurrentDirectory(), "My", "Work", "Dir" + Path.DirectorySeparatorChar);
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "tables" + Path.DirectorySeparatorChar);

            FileUtils.DeleteIfExists(workDirectory);
            Directory.CreateDirectory(workDirectory);
            FileUtils.DeleteIfExists(folderPath);
            Directory.CreateDirectory(folderPath);

            try
            {
                var manager = new DataTableManager { FolderPath = folderPath };
                manager.CreateNewDataTable("A", "B", "C.d", "E");
                manager.CreateNewDataTable("F", "G", "H.i", "J");
                manager.CreateNewDataTable("K", "L", "M.n", "O");

                var factory = new IncludeFileFactory();

                // call
                var dryWasteLoadDataIncludeContent = factory.CreateDryWasteLoadDataInclude(manager, workDirectory);

                // assert
                var expectedContents =
                    @"INCLUDE '..\..\..\tables\K.tbl'" + Environment.NewLine +
                    @"INCLUDE '..\..\..\tables\F.tbl'" + Environment.NewLine +
                    @"INCLUDE '..\..\..\tables\A.tbl'" + Environment.NewLine;
                Assert.AreEqual(expectedContents, dryWasteLoadDataIncludeContent);
            }
            finally
            {
                FileUtils.DeleteIfExists(workDirectory);
                FileUtils.DeleteIfExists(folderPath);
            }
        }

        [Test]
        public void CreateDryWasteLoadAliasesIncludeTest()
        {
            var input = new Dictionary<string, IList<string>>()
            {
                {"measure point 1", new List<string>() {"load 1", "load 2",}},
                {"measure point 2", new List<string>() {"load 1", "load 3",}},
                {"measure point 3", new List<string>() {"load 3", "load 1",}},
            };

            var factory = new IncludeFileFactory();

            var boundaryAliasesIncludeFileContents = factory.CreateDryWasteLoadAliasesInclude(input);

            var expectedContents = "USEDATA_ITEM 'measure point 1' FORITEM" + Environment.NewLine +
                                   "'load 1'" + Environment.NewLine +
                                   "'load 2'" + Environment.NewLine +
                                   Environment.NewLine +
                                   "USEDATA_ITEM 'measure point 2' FORITEM" + Environment.NewLine +
                                   "'load 1'" + Environment.NewLine +
                                   "'load 3'" + Environment.NewLine +
                                   Environment.NewLine +
                                   "USEDATA_ITEM 'measure point 3' FORITEM" + Environment.NewLine +
                                   "'load 3'" + Environment.NewLine +
                                   "'load 1'" + Environment.NewLine +
                                   Environment.NewLine;

            Assert.AreEqual(expectedContents, boundaryAliasesIncludeFileContents);
        }

        #endregion Block 6

        #region Block 7

        [Test]
        public void CreateParametersInclude2DTest()
        {
            var staggeredGrid = CreateTwoCellStaggeredGrid();

            var coverageA = (UnstructuredGridCellCoverage) WaterQualityFunctionFactory.CreateUnstructuredGridCellCoverage("A", 2.0, "A", "mg", "A");
            coverageA.Grid = staggeredGrid;
            coverageA[0] = 2.0;
            coverageA[1] = 4.0;

            var coverageB = (UnstructuredGridCellCoverage) WaterQualityFunctionFactory.CreateUnstructuredGridCellCoverage("D", 3.0, "D", "mg", "D");
            coverageB.Grid = staggeredGrid;
            coverageB[0] = 2.0;
            coverageB[1] = 5.5;

            var processCoefficients = new[]
            {
                coverageA, coverageB
            };

            var waqInitializationSettings = new WaqInitializationSettings
            {
                ProcessCoefficients = processCoefficients,
                NumberOfLayers = 1,
            };

            string expectedString = "PARAMETERS" + Environment.NewLine +
                                    "'A'" + Environment.NewLine +
                                    "ALL" + Environment.NewLine +
                                    "DATA" + Environment.NewLine +
                                    "2" + Environment.NewLine +
                                    "4" + Environment.NewLine +
                                    Environment.NewLine +
                                    "PARAMETERS" + Environment.NewLine +
                                    "'D'" + Environment.NewLine +
                                    "ALL" + Environment.NewLine +
                                    "DATA" + Environment.NewLine +
                                    "2" + Environment.NewLine +
                                    "5.5" + Environment.NewLine +
                                    Environment.NewLine;

            IncludeFileFactory factory = new IncludeFileFactory();

            // call
            var parametersInclude = factory.CreateParametersInclude(waqInitializationSettings);

            // assert
            Assert.AreEqual(expectedString, parametersInclude);
        }

        [Test]
        public void TestSurfParametersWritten()
        {
            const string surfacesFile = "uni3d.srf";
            var waqInitializationSettings = new WaqInitializationSettings
            {
                SurfacesFile = surfacesFile,
            };

            string expectedString = "PARAMETERS" + Environment.NewLine + 
                                    "'Surf'" + Environment.NewLine + 
                                    "ALL" + Environment.NewLine +
                                    "BINARY_FILE 'uni3d.srf' ; from horizontal-surfaces-file key in hyd file" + Environment.NewLine;

            IncludeFileFactory factory = new IncludeFileFactory();
            Assert.AreEqual(expectedString, factory.CreateParametersInclude(waqInitializationSettings));
        }

        // TODO: For CreateParametersInclude3DTest

        [Test]
        public void CreateSegfunctionsIncludeTest()
        {
            // setup
            var aFunc = WaterQualityFunctionFactory.CreateFunctionFromHydroDynamics("A", 1.2, "irrelevant", "g", "A");
            aFunc.FilePath = "<some filepath set by model>";

            var bFunc = WaterQualityFunctionFactory.CreateFunctionFromHydroDynamics("B", 3.4, "still irrelevant", "g", "B");
            bFunc.FilePath = "<another filepath set by model>";

            var initSettings = new WaqInitializationSettings
                {
                    ProcessCoefficients = new[] { aFunc, bFunc },
                };

            var factory = new IncludeFileFactory();

            // call
            var segfunctionInclude = factory.CreateSegfunctionsInclude(initSettings);

            // assert
            var expectedText =
                "SEG_FUNCTIONS" + Environment.NewLine +
                "'A'" + Environment.NewLine +
                "ALL" + Environment.NewLine +
                "BINARY_FILE '<some filepath set by model>'" + Environment.NewLine +
                Environment.NewLine +
                "SEG_FUNCTIONS" + Environment.NewLine +
                "'B'" + Environment.NewLine +
                "ALL" + Environment.NewLine +
                "BINARY_FILE '<another filepath set by model>'" + Environment.NewLine +
                Environment.NewLine;

            Assert.AreEqual(expectedText, segfunctionInclude);
        }

        [Test]
        public void CreateSegfunctionsWithUrlIncludeTest()
        {
            string dataDir = Path.Combine(TestHelper.GetDataDir(), @"TestSegFunctionFiles");
            string pathA = Path.Combine(dataDir, @"segFileA.tau");
            string pathB = Path.Combine(dataDir, @"segFileB.vol");
            // setup
            var aFunc = WaterQualityFunctionFactory.CreateSegmentFunction("A", 1.2, "irrelevant", "g", "A", pathA);
            var bFunc = WaterQualityFunctionFactory.CreateSegmentFunction("B", 3.4, "still irrelevant", "g", "B", pathB);

            var initSettings = new WaqInitializationSettings
            {
                ProcessCoefficients = new[] { aFunc, bFunc },
            };

            var factory = new IncludeFileFactory();

            // call
            var segfunctionInclude = factory.CreateSegfunctionsInclude(initSettings);

            // assert
            var expectedText =
                "SEG_FUNCTIONS" + Environment.NewLine +
                "'A'" + Environment.NewLine +
                "ALL" + Environment.NewLine +
                "BINARY_FILE '" + pathA + "'" + Environment.NewLine +
                Environment.NewLine +
                "SEG_FUNCTIONS" + Environment.NewLine +
                "'B'" + Environment.NewLine +
                "ALL" + Environment.NewLine +
                "BINARY_FILE '"+ pathB + "'" + Environment.NewLine +
                Environment.NewLine;

            Assert.AreEqual(expectedText, segfunctionInclude);
        }

        [Test]
        public void CreateNumericalOptionsIncludeTestForIterativeScheme()
        {
            // setup
            var initSettings = new WaqInitializationSettings
            {
                Settings =
                {
                    NumericalScheme = NumericalScheme.Scheme15,
                    NrOfThreads = 1,
                    ClosureErrorCorrection = true,
                    IterationMaximum = 123,
                    Tolerance = 1e-6,
                    WriteIterationReport = false,
                    DryCellThreshold = 0.01
                }
            };

            var factory = new IncludeFileFactory();

            // call
            var numericalOptionsInclude = factory.CreateNumericalOptionsInclude(initSettings);

            // assert
            var expectedText =
                "CONSTANTS 'CLOSE_ERR' DATA 1 ; If defined, allow delwaq to correct water volumes to keep concentrations continuous" + Environment.NewLine +
                "CONSTANTS 'NOTHREADS' DATA 1 ; Number of threads used by delwaq" + Environment.NewLine +
                "CONSTANTS 'DRY_THRESH' DATA 0.01 ; Dry cell threshold" + Environment.NewLine +
                "CONSTANTS 'maxiter' DATA 123 ; Maximum number of iterations" + Environment.NewLine +
                "CONSTANTS 'tolerance' DATA 1E-06 ; Convergence tolerance" + Environment.NewLine +
                "CONSTANTS 'iteration report' DATA 0 ; Write iteration report (when 1) or not (when 0)" + Environment.NewLine;
            Assert.AreEqual(expectedText, numericalOptionsInclude);
        }

        [Test]
        public void CreateNumericalOptionsIncludeTestForNonIterativeScheme()
        {
            // setup
            var initSettings = new WaqInitializationSettings
            {
                Settings =
                {
                    NumericalScheme = NumericalScheme.Scheme1,
                    NrOfThreads = 1,
                    ClosureErrorCorrection = true,
                    IterationMaximum = 123,
                    Tolerance = 1e-6,
                    WriteIterationReport = false,
                    DryCellThreshold = 0.01
                }
            };

            var factory = new IncludeFileFactory();

            // call
            var numericalOptionsInclude = factory.CreateNumericalOptionsInclude(initSettings);

            // assert
            var expectedText =
                "CONSTANTS 'CLOSE_ERR' DATA 1 ; If defined, allow delwaq to correct water volumes to keep concentrations continuous" + Environment.NewLine +
                "CONSTANTS 'NOTHREADS' DATA 1 ; Number of threads used by delwaq" + Environment.NewLine +
                "CONSTANTS 'DRY_THRESH' DATA 0.01 ; Dry cell threshold" + Environment.NewLine;
            Assert.AreEqual(expectedText, numericalOptionsInclude);
        }

        [Test]
        public void CreateNumericalOptionsDoNotContain_CLOSE_ERR_ConstantWhenFalse()
        {
            // setup
            var initSettings = new WaqInitializationSettings
            {
                Settings =
                {
                    NumericalScheme = NumericalScheme.Scheme22,
                    NrOfThreads = 0,
                    ClosureErrorCorrection = false,
                    IterationMaximum = 89,
                    Tolerance = 0.2,
                    WriteIterationReport = true,
                    DryCellThreshold = 1.23
                }
            };

            var factory = new IncludeFileFactory();

            // call
            var numericalOptionsInclude = factory.CreateNumericalOptionsInclude(initSettings);

            // assert
            var expectedText =
                "CONSTANTS 'NOTHREADS' DATA 0 ; Number of threads used by delwaq" + Environment.NewLine +
                "CONSTANTS 'DRY_THRESH' DATA 1.23 ; Dry cell threshold" + Environment.NewLine +
                "CONSTANTS 'maxiter' DATA 89 ; Maximum number of iterations" + Environment.NewLine +
                "CONSTANTS 'tolerance' DATA 0.2 ; Convergence tolerance" + Environment.NewLine +
                "CONSTANTS 'iteration report' DATA 1 ; Write iteration report (when 1) or not (when 0)" + Environment.NewLine;
            Assert.AreEqual(expectedText, numericalOptionsInclude);
        }

         [Test]
         public void CreateSpatialDispersionTest()
         {
             var staggeredGrid = CreateTwoCellStaggeredGrid();
             var coverageA = (UnstructuredGridCellCoverage) WaterQualityFunctionFactory.CreateUnstructuredGridCellCoverage("A", 2.0, "A", "mg", "A");
             coverageA.Grid = staggeredGrid;
             coverageA[0] = 2.0;
             coverageA[1] = 4.0;
 
             var factory = new IncludeFileFactory();
             var result = factory.CreateSpatialDispersionInclude(coverageA, 2);

             var expectedString = "CONSTANTS 'ACTIVE_HDisperAdd' DATA 1.0" + Environment.NewLine +
                                  "PARAMETERS" + Environment.NewLine +
                                  "'AddDispH'" + Environment.NewLine + 
                                  "ALL" + Environment.NewLine + 
                                  "DATA" + Environment.NewLine + 
                                  "2" + Environment.NewLine + 
                                  "4" + Environment.NewLine + 
                                  "2" + Environment.NewLine + 
                                  "4" + Environment.NewLine + 
                                  Environment.NewLine;
 
             Assert.AreEqual(expectedString, result);
         }

        [Test]
        public void CreateVerticalDiffusionTest()
        {
            IncludeFileFactory factory = new IncludeFileFactory();
            string result = factory.CreateVerticalDiffusionInclude("someFile.vdf", true);

            string expected = "CONSTANTS 'ACTIVE_VertDisp' DATA 1.0" + Environment.NewLine +
                              "SEG_FUNCTIONS" + Environment.NewLine +
                              "'VertDisper'" + Environment.NewLine +
                              "ALL" + Environment.NewLine +
                              "BINARY_FILE 'someFile.vdf'" + Environment.NewLine;

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void CreateVerticalDiffusionOffTest()
        {
            IncludeFileFactory factory = new IncludeFileFactory();
            string result = factory.CreateVerticalDiffusionInclude("someFile.vdf", false);

            Assert.AreEqual(string.Empty, result);
        }

        [Test]
        public void CreateVerticalDiffusionNoFileTest()
        {
            IncludeFileFactory factory = new IncludeFileFactory();
            string result = factory.CreateVerticalDiffusionInclude(null, true);

            Assert.AreEqual(string.Empty, result);
        }

        #endregion

        #region Block 8

        [Test]
        public void CreateSpatialInitialConditionsFileConstantsTest()
        {
            // setup
            var staggeredGrid = CreateTwoCellStaggeredGrid();

            var coverageA = (UnstructuredGridCellCoverage)WaterQualityFunctionFactory.CreateUnstructuredGridCellCoverage("A", 2.0, "A", "mg", "A");
            coverageA.Grid = staggeredGrid;
            coverageA[0] = 2.0;
            coverageA[1] = 4.0;

            var coverageB = (UnstructuredGridCellCoverage)WaterQualityFunctionFactory.CreateUnstructuredGridCellCoverage("D", 3.0, "D", "mg", "D");
            coverageB.Grid = staggeredGrid;
            coverageB[0] = 2.0;
            coverageB[1] = 5.5;

            var initialConditions = new[]
            {
                coverageA, coverageB
            };

            var waqInitializationSettings = new WaqInitializationSettings
            {
                InitialConditions = initialConditions,
                NumberOfLayers = 1,
            };

            string expectedString = "MASS/M2" + Environment.NewLine +
                                    "INITIALS" + Environment.NewLine +
                                    "'A'" + Environment.NewLine +
                                    "ALL" + Environment.NewLine +
                                    "DATA" + Environment.NewLine +
                                    "2" + Environment.NewLine +
                                    "4" + Environment.NewLine +
                                    Environment.NewLine +
                                    "INITIALS" + Environment.NewLine +
                                    "'D'" + Environment.NewLine +
                                    "ALL" + Environment.NewLine +
                                    "DATA" + Environment.NewLine +
                                    "2" + Environment.NewLine +
                                    "5.5" + Environment.NewLine +
                                    Environment.NewLine;

            IncludeFileFactory factory = new IncludeFileFactory();

            // call
            var parametersInclude = factory.CreateInitialConditionsInclude(waqInitializationSettings);

            // assert
            Assert.AreEqual(expectedString, parametersInclude);
        }
        
        

        #endregion Block 8

        private static UnstructuredGrid CreateTwoCellStaggeredGrid()
        {
            // setup
            // two triangles in a square
            // 2 +-----+ 3
            //   |   / |
            //   | /   |
            // 1 +-----+ 4

            var vertices = new[]
            {
                new Coordinate(0, 0),
                new Coordinate(0, 10),
                new Coordinate(10, 10),
                new Coordinate(10, 0)
            };

            var edges = new[,]
            {
                {1, 2}, {2, 3}, {3, 4}, {4, 1}, {1, 3}
            };

            var cells = new[,]
            {
                {1, 2, 3},
                {1, 3, 4}
            };

            return UnstructuredGridFactory.CreateFromVertexAndEdgeList(vertices, edges, cells);
        }
    }
}