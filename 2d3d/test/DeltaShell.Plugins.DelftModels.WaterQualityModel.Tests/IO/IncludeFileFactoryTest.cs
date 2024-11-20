using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.BoundaryData;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.Model;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.SubstanceProcessLibrary;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Model;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.IO
{
    [TestFixture]
    public class IncludeFileFactoryTest
    {
        [Test]
        [Category(TestCategory.Integration)]
        public void Import_Waq_Model_WithSegmentFiles_Create_SegmentFileFunctions()
        {
            string testFilePath = TestHelper.GetTestFilePath(@"ValidWaqModels\\Flow1D\\sobek.hyd");
            string subsFilePath = TestHelper.GetTestFilePath(@"ValidWaqModels\\02b_Oxygen_bod_sediment.sub");

            var importer = new HydFileImporter();
            using (var waqModel = importer.ImportItem(testFilePath) as WaterQualityModel)
            {
                Assert.IsNotNull(waqModel);

                //Import the substances now.
                Assert.IsNotNull(waqModel.SubstanceProcessLibrary);
                new SubFileImporter().Import(waqModel.SubstanceProcessLibrary, subsFilePath);

                //Check for the CHEZY seg function in the include.
                var initSettings = new WaqInitializationSettings {ProcessCoefficients = waqModel.ProcessCoefficients};
                string text = IncludeFileFactory.CreateSegfunctionsInclude(initSettings);
                Assert.IsFalse(string.IsNullOrEmpty(text));

                var expectedText = "SEG_FUNCTIONS\r\n'CHEZY'\r\nALL\r\nBINARY_FILE";
                Assert.IsTrue(text.Contains(expectedText));
            }
        }

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
                {
                    1,
                    2
                },
                {
                    2,
                    3
                },
                {
                    3,
                    4
                },
                {
                    4,
                    1
                },
                {
                    1,
                    3
                }
            };

            var cells = new[,]
            {
                {
                    1,
                    2,
                    3
                },
                {
                    1,
                    3,
                    4
                }
            };

            return UnstructuredGridFactory.CreateFromVertexAndEdgeList(vertices, edges, cells);
        }

        #region Block 1

        [Test]
        public void TestCreateT0Include()
        {
            const string ExpectedString = "'T0: 2010.01.01 13:12:11  (scu=       1s)'";

            string dispersionInclude = IncludeFileFactory.CreateT0Include(new DateTime(2010, 1, 1, 13, 12, 11));
            Assert.AreEqual(ExpectedString, dispersionInclude);
        }

        [Test]
        public void TestCreateSubstanceListInclude()
        {
            var substanceProcessLib = new SubstanceProcessLibrary();
            substanceProcessLib.Substances.Add(new WaterQualitySubstance
            {
                Name = "ActiveSubstance",
                Active = true,
                Description = "An active substance"
            });
            substanceProcessLib.Substances.Add(new WaterQualitySubstance
            {
                Name = "InActiveSubstance",
                Active = false,
                Description = "An inactive substance"
            });

            string expectedString = "; number of active and inactive substances" + Environment.NewLine +
                                    "1             1" + Environment.NewLine +
                                    "        ; active substances" + Environment.NewLine +
                                    "1            'ActiveSubstance' ;An active substance" + Environment.NewLine +
                                    "        ; passive substances" + Environment.NewLine +
                                    "2            'InActiveSubstance' ;An inactive substance" + Environment.NewLine;

            string dispersionInclude = IncludeFileFactory.CreateSubstanceListInclude(substanceProcessLib);
            Assert.AreEqual(expectedString, dispersionInclude);
        }

        #endregion Block 1

        #region Block 2

        [Test]
        public void TestCreateNumSettingsInclude()
        {
            string expectedString = "22.63 ; integration option" + Environment.NewLine +
                                    "; detailed balance options" + Environment.NewLine +
                                    "BAL_LUMPPROCESSES BAL_NOLUMPTRANSPORT BAL_LUMPLOADS" + Environment.NewLine +
                                    "BAL_NOSUPPRESSSPACE BAL_SUPPRESSTIME" + Environment.NewLine;

            var waqSettings = new WaterQualityModelSettings
            {
                Balance = true,
                BalanceUnit = BalanceUnit.Gram,
                LumpLoads = true,
                LumpTransport = false,
                LumpProcesses = true,
                SuppressSpace = false,
                SuppressTime = true,
                NumericalScheme = NumericalScheme.Scheme22,
                UseFirstOrder = false,
                NoDispersionOverOpenBoundaries = true,
                NoDispersionIfFlowIsZero = false
            };

            string dispersionInclude = IncludeFileFactory.CreateNumSettingsInclude(waqSettings);
            Assert.AreEqual(expectedString, dispersionInclude);

            waqSettings.BalanceUnit = BalanceUnit.GramPerSquareMeter;
            expectedString = "22.63 ; integration option" + Environment.NewLine +
                             "; detailed balance options" + Environment.NewLine +
                             "BAL_UNITAREA" + Environment.NewLine +
                             "BAL_LUMPPROCESSES BAL_NOLUMPTRANSPORT BAL_LUMPLOADS" + Environment.NewLine +
                             "BAL_NOSUPPRESSSPACE BAL_SUPPRESSTIME" + Environment.NewLine;
            dispersionInclude = IncludeFileFactory.CreateNumSettingsInclude(waqSettings);
            Assert.AreEqual(expectedString, dispersionInclude);

            waqSettings.BalanceUnit = BalanceUnit.GramPerCubicMeter;
            expectedString = "22.63 ; integration option" + Environment.NewLine +
                             "; detailed balance options" + Environment.NewLine +
                             "BAL_UNITVOLUME" + Environment.NewLine +
                             "BAL_LUMPPROCESSES BAL_NOLUMPTRANSPORT BAL_LUMPLOADS" + Environment.NewLine +
                             "BAL_NOSUPPRESSSPACE BAL_SUPPRESSTIME" + Environment.NewLine;
            dispersionInclude = IncludeFileFactory.CreateNumSettingsInclude(waqSettings);
            Assert.AreEqual(expectedString, dispersionInclude);
        }

        [Test]
        public void TestCreateOutputTimersInclude()
        {
            string expectedString = "; output control (see DELWAQ-manual)" + Environment.NewLine +
                                    "; yyyy/mm/dd-hh:mm:ss  yyyy/mm/dd-hh:mm:ss  dddhhmmss" + Environment.NewLine +
                                    "  2010/01/01-00:00:00  2010/01/10-00:00:00  001000000 ;  start, stop and step for balance output" + Environment.NewLine +
                                    "  2010/02/01-00:00:00  2010/02/10-00:00:00  000010000 ;  start, stop and step for map output" + Environment.NewLine +
                                    "  2010/03/01-00:00:00  2010/03/10-00:00:00  000000100 ;  start, stop and step for his output" + Environment.NewLine;

            var waqSettings = new WaterQualityModelSettings
            {
                HisStartTime = new DateTime(2010, 3, 1),
                HisStopTime = new DateTime(2010, 3, 10),
                HisTimeStep = new TimeSpan(0, 0, 1, 0),
                MapStartTime = new DateTime(2010, 2, 1),
                MapStopTime = new DateTime(2010, 2, 10),
                MapTimeStep = new TimeSpan(0, 1, 0, 0),
                BalanceStartTime = new DateTime(2010, 1, 1),
                BalanceStopTime = new DateTime(2010, 1, 10),
                BalanceTimeStep = new TimeSpan(1, 0, 0, 0)
            };

            string outputTimersInclude = IncludeFileFactory.CreateOutputTimersInclude(waqSettings);
            Assert.AreEqual(expectedString, outputTimersInclude);
        }

        [Test]
        public void TestCreateSimTimersInclude()
        {
            string expectedString = "  2010/01/01-00:00:00 ; start time" + Environment.NewLine +
                                    "  2010/01/05-00:00:00 ; stop time" + Environment.NewLine +
                                    "  0 ; timestep constant" + Environment.NewLine +
                                    "  001000000 ; timestep";

            var initializationSettings = new WaqInitializationSettings
            {
                SimulationStartTime = new DateTime(2010, 1, 1),
                SimulationStopTime = new DateTime(2010, 1, 5),
                SimulationTimeStep = new TimeSpan(1, 0, 0, 0)
            };

            string simTimersInclude = IncludeFileFactory.CreateSimTimersInclude(initializationSettings);
            Assert.AreEqual(expectedString, simTimersInclude);
        }

        [Test]
        public void TestCreateOutputLocations()
        {
            var obsPoints = new Dictionary<string, IList<int>>()
            {
                {
                    "obs1", new[]
                    {
                        1,
                        2,
                        3
                    }
                },
                {
                    "obs2", new[]
                    {
                        4,
                        5,
                        6
                    }
                }
            };

            string text = IncludeFileFactory.CreateOutputLocationsInclude(obsPoints);

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

            string text = IncludeFileFactory.CreateOutputLocationsInclude(obsPoints);

            string expectedString = "0 ; nr of monitor locations" + Environment.NewLine;

            Assert.AreEqual(expectedString, text);
        }

        #endregion Block 2

        #region Block 3

        [Test]
        public void GivenAnIncludeFileFactory_WhenCallCreateGridFileInclude_ThenExpectedStringIsReturned()
        {
            // Given
            const string inputString = "file_path";
            var expectedString = $"UGRID '{inputString}'";

            // When
            string result = IncludeFileFactory.CreateGridFileInclude(inputString);

            // Then
            Assert.That(result, Is.EqualTo(expectedString),
                        "Different string was expected when creating the content for the grid include file.");
        }

        [Test]
        public void TestCreateNumberOfSegmentsInclude()
        {
            const string ExpectedString = "446698 ; number of segments";

            Assert.AreEqual(ExpectedString, IncludeFileFactory.CreateNumberOfSegmentsInclude(63814, 7));
        }

        [Test]
        public void TestCreateVolumesFileInclude()
        {
            string expectedString = "-2 ; volumes will be interpolated from a binary file" + Environment.NewLine +
                                    "'uni3d.vol' ; volumes file from hyd file" + Environment.NewLine;

            Assert.AreEqual(expectedString, IncludeFileFactory.CreateVolumesFileInclude("uni3d.vol"));
        }

        #endregion Block 3

        #region Block 4

        [Test]
        public void TestCreateNumberOfExchangesInclude()
        {
            const string ExpectedString = "788900 0 382884 ; number of exchanges in three directions";

            Assert.AreEqual(ExpectedString, IncludeFileFactory.CreateNumberOfExchangesInclude(788900, 382884));
        }

        [Test]
        public void TestCreatePointersFileInclude()
        {
            string expectedString = "0 ; pointers from binary file." + Environment.NewLine +
                                    "'uni3d.poi' ; pointers file" + Environment.NewLine;

            Assert.AreEqual(expectedString, IncludeFileFactory.CreatePointersFileInclude("uni3d.poi"));
        }

        [Test]
        public void TestCreateAreasFileInclude()
        {
            string expectedString = "-2 ; areas will be interpolated from a binary file" + Environment.NewLine +
                                    "'uni3d.are' ; areas file" + Environment.NewLine;

            Assert.AreEqual(expectedString, IncludeFileFactory.CreateAreasFileInclude("uni3d.are"));
        }

        [Test]
        public void TestCreateAttributesFileInclude()
        {
            const string ExpectedString = "INCLUDE 'uni3d.atr' ; attributes file";

            Assert.AreEqual(ExpectedString, IncludeFileFactory.CreateAttributesFileInclude("uni3d.atr"));
        }

        [Test]
        public void TestCreateFlowsFileInclude()
        {
            string expectedString = "-2 ; flows from binary file" + Environment.NewLine +
                                    "'uni3d.flo' ; flows file" + Environment.NewLine;

            Assert.AreEqual(expectedString, IncludeFileFactory.CreateFlowsFileInclude("uni3d.flo"));
        }

        [Test]
        public void TestCreateLengthsFileInclude()
        {
            string expectedString = "0 ; Lengths from binary file" + Environment.NewLine +
                                    "'uni3d.len' ; lengths file" + Environment.NewLine;

            Assert.AreEqual(expectedString, IncludeFileFactory.CreateLengthsFileInclude("uni3d.len"));
        }

        [Test]
        public void TestCreateConstantDispersionInclude()
        {
            const string ExpectedString = "0.2 0.0 0.3 ; constant dispersion";

            IFunction dispersion = WaterQualityFunctionFactory.CreateConst("Dispersion", 0.2d, "Dispersion", "m2/s", null);

            Assert.AreEqual(ExpectedString, IncludeFileFactory.CreateConstantDispersionInclude(0.3d, dispersion));
        }

        [Test]
        public void TestCreateConstantDispersionInclude_HasSpatialData()
        {
            const string ExpectedString = "0.0 0.0 0.3 ; constant dispersion";

            IFunction dispersion = WaterQualityFunctionFactory.CreateUnstructuredGridCellCoverage("Dispersion", 0.2d, "Dispersion", "m2/s", null);

            Assert.AreEqual(ExpectedString, IncludeFileFactory.CreateConstantDispersionInclude(0.3d, dispersion));
        }

        #endregion Block 4

        #region Block 5

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void TestGetBoundarySegmentsToWrite_Real()
        {
            string hydPath = TestHelper.GetTestFilePath(@"IO\real\uni3d.hyd");

            using (var model = new WaterQualityModel())
            {
                new HydFileImporter().ImportItem(hydPath, model);

                string result = IncludeFileFactory.CreateBoundaryListInclude(model.BoundaryNodeIds, model.NumberOfWaqSegmentLayers);

                string[] resultLines = result.Split(new[]
                {
                    Environment.NewLine
                }, StringSplitOptions.RemoveEmptyEntries);

                // assert the count of the list
                int allSegmentsCount = model.BoundaryNodeIds.Sum(kvp => kvp.Value.Count());
                Assert.AreEqual((allSegmentsCount * model.NumberOfWaqSegmentLayers) + model.NumberOfWaqSegmentLayers, resultLines.Length - 1);

                Assert.AreEqual("; Boundaries for layer 1", resultLines[1]);
                Assert.AreEqual("'1' '' 'sea_002.pli'", resultLines[2]);
                Assert.AreEqual("; Boundaries for layer 2", resultLines[2 + allSegmentsCount]);
                Assert.AreEqual("'139' '' 'sea_002.pli'", resultLines[3 + allSegmentsCount]);
            }
        }

        [Test]
        public void TestCreateBoundaryListInclude()
        {
            const string BoundaryNameOne = "one";
            const string BoundaryNameTwo = "two";

            var boundaryNodes = new Dictionary<WaterQualityBoundary, int[]>(2)
            {
                {
                    new WaterQualityBoundary() {Name = BoundaryNameTwo}, new[]
                    {
                        6,
                        2
                    }
                }, // added in the wrong order, but the result should still start with 1
                {
                    new WaterQualityBoundary() {Name = BoundaryNameOne}, new[]
                    {
                        1,
                        5,
                        3,
                        4
                    }
                }
            };

            string result = IncludeFileFactory.CreateBoundaryListInclude(boundaryNodes, 3);

            string[] lines = result.Split(new[]
            {
                Environment.NewLine
            }, StringSplitOptions.RemoveEmptyEntries);

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
            string workDirectory = Path.Combine(Directory.GetCurrentDirectory(), "My", "Work", "Dir" + Path.DirectorySeparatorChar);
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "tables" + Path.DirectorySeparatorChar);

            FileUtils.DeleteIfExists(workDirectory);
            Directory.CreateDirectory(workDirectory);
            FileUtils.DeleteIfExists(folderPath);
            Directory.CreateDirectory(folderPath);

            try
            {
                var manager = new DataTableManager {FolderPath = folderPath};
                manager.CreateNewDataTable("A", "B", "C.d", "E");
                manager.CreateNewDataTable("F", "G", "H.i", "J");
                manager.CreateNewDataTable("K", "L", "M.n", "O");

                // call
                string boundaryDataIncludeFileContents = IncludeFileFactory.CreateBoundaryDataInclude(manager, workDirectory);

                // assert
                string expectedContents =
                    @"INCLUDE '../../../tables/K.tbl'" + Environment.NewLine +
                    @"INCLUDE '../../../tables/F.tbl'" + Environment.NewLine +
                    @"INCLUDE '../../../tables/A.tbl'" + Environment.NewLine;
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
            string workDirectory = Path.Combine(Directory.GetCurrentDirectory(), "My", "Work", "Dir" + Path.DirectorySeparatorChar);
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "tables" + Path.DirectorySeparatorChar);

            FileUtils.DeleteIfExists(workDirectory);
            Directory.CreateDirectory(workDirectory);
            FileUtils.DeleteIfExists(folderPath);
            Directory.CreateDirectory(folderPath);

            try
            {
                var manager = new DataTableManager {FolderPath = folderPath};
                manager.CreateNewDataTable("A", "B", "C.d", "E");
                manager.CreateNewDataTable("F", "G", "H.i", "J");
                manager.CreateNewDataTable("K", "L", "M.n", "O");

                // disable the second datatable
                DataTable[] dataTables = manager.DataTables.ToArray();
                dataTables[1].IsEnabled = false;

                // call
                string boundaryDataIncludeFileContents = IncludeFileFactory.CreateBoundaryDataInclude(manager, workDirectory);

                // assert
                string expectedContents =
                    @"INCLUDE '../../../tables/K.tbl'" + Environment.NewLine +
                    @"INCLUDE '../../../tables/A.tbl'" + Environment.NewLine;
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
                {
                    "measure point 1", new List<string>()
                    {
                        "boundary 1",
                        "boundary 2"
                    }
                },
                {
                    "measure point 2", new List<string>()
                    {
                        "boundary 1",
                        "boundary 3"
                    }
                },
                {
                    "measure point 3", new List<string>()
                    {
                        "boundary 3",
                        "boundary 1"
                    }
                }
            };

            string boundaryAliasesIncludeFileContents = IncludeFileFactory.CreateBoundaryAliasesInclude(input);

            string expectedContents = "USEDATA_ITEM 'measure point 1' FORITEM" + Environment.NewLine +
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

            var loadsAndIds = new Dictionary<WaterQualityLoad, int>(3)
            {
                [load1] = 948,
                [load2] = 67,
                [load3] = 0
            };

            string expectedString =
                "; Number of loads" + Environment.NewLine +
                "3; Number of loads" + Environment.NewLine +
                ";SegmentID  Load-name  Comment  Load-type" + Environment.NewLine +
                "948 'Load 1' '' 'Test'" + Environment.NewLine +
                "67 'Load 2' '' 'Test'" + Environment.NewLine +
                "0 'Load 3' '' 'haha'" + Environment.NewLine;

            // call
            string text = IncludeFileFactory.CreateDryWasteLoadInclude(loadsAndIds);

            // assert
            Assert.AreEqual(expectedString, text);
        }

        [Test]
        public void CreateLoadsDataIncludeTest()
        {
            // setup
            string workDirectory = Path.Combine(Directory.GetCurrentDirectory(), "My", "Work", "Dir" + Path.DirectorySeparatorChar);
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "tables" + Path.DirectorySeparatorChar);

            FileUtils.DeleteIfExists(workDirectory);
            Directory.CreateDirectory(workDirectory);
            FileUtils.DeleteIfExists(folderPath);
            Directory.CreateDirectory(folderPath);

            try
            {
                var manager = new DataTableManager {FolderPath = folderPath};
                manager.CreateNewDataTable("A", "B", "C.d", "E");
                manager.CreateNewDataTable("F", "G", "H.i", "J");
                manager.CreateNewDataTable("K", "L", "M.n", "O");

                // call
                string dryWasteLoadDataIncludeContent = IncludeFileFactory.CreateDryWasteLoadDataInclude(manager, workDirectory);

                // assert
                string expectedContents =
                    @"INCLUDE '../../../tables/K.tbl'" + Environment.NewLine +
                    @"INCLUDE '../../../tables/F.tbl'" + Environment.NewLine +
                    @"INCLUDE '../../../tables/A.tbl'" + Environment.NewLine;
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
                {
                    "measure point 1", new List<string>()
                    {
                        "load 1",
                        "load 2"
                    }
                },
                {
                    "measure point 2", new List<string>()
                    {
                        "load 1",
                        "load 3"
                    }
                },
                {
                    "measure point 3", new List<string>()
                    {
                        "load 3",
                        "load 1"
                    }
                }
            };

            string boundaryAliasesIncludeFileContents = IncludeFileFactory.CreateDryWasteLoadAliasesInclude(input);

            string expectedContents = "USEDATA_ITEM 'measure point 1' FORITEM" + Environment.NewLine +
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
        public void TestCreateProcessesInclude()
        {
            var substanceProcessLib = new SubstanceProcessLibrary();
            substanceProcessLib.Processes.Add(new WaterQualityProcess {Name = "Process A"});
            substanceProcessLib.Processes.Add(new WaterQualityProcess {Name = "Process B"});

            string expectedString = "CONSTANTS 'ACTIVE_Process A' DATA 0" + Environment.NewLine +
                                    "CONSTANTS 'ACTIVE_Process B' DATA 0" + Environment.NewLine;

            string dispersionInclude = IncludeFileFactory.CreateProcessesInclude(substanceProcessLib);
            Assert.AreEqual(expectedString, dispersionInclude);
        }

        [Test]
        public void TestCreateConstantsInclude()
        {
            IFunction[] processCoefficients = new[]
            {
                WaterQualityFunctionFactory.CreateConst("A", 2, "A", "mg/L", "A"),
                WaterQualityFunctionFactory.CreateTimeSeries("B", 5.5, "B", "g/L", "B"),
                WaterQualityFunctionFactory.CreateNetworkCoverage("C", 10.5, "C", "mg/mL", "C"),
                WaterQualityFunctionFactory.CreateConst("D", 3.5, "D", "g/mL", "D")
            };

            string expectedString = "CONSTANTS 'A' DATA 2" + Environment.NewLine +
                                    "CONSTANTS 'D' DATA 3.5" + Environment.NewLine;

            string createConstantsInclude = IncludeFileFactory.CreateConstantsInclude(processCoefficients);
            Assert.AreEqual(expectedString, createConstantsInclude);
        }

        [Test]
        public void TestCreateFunctionsInclude()
        {
            IFunction timeSeries1 = WaterQualityFunctionFactory.CreateTimeSeries("A", 2, "A", "mg/L", "A");
            timeSeries1.Arguments[0].InterpolationType = InterpolationType.Linear;
            timeSeries1[new DateTime(2010, 1, 1, 0, 0, 0)] = 0.12;
            timeSeries1[new DateTime(2010, 1, 1, 0, 10, 0)] = 0.16;

            IFunction timeSeries2 = WaterQualityFunctionFactory.CreateTimeSeries("D", 3.5, "D", "g/mL", "D");
            timeSeries2.Arguments[0].InterpolationType = InterpolationType.Constant;
            timeSeries2[new DateTime(2010, 1, 1, 0, 0, 0)] = 0.12;
            timeSeries2[new DateTime(2010, 1, 1, 0, 10, 0)] = 0.18;
            timeSeries2[new DateTime(2010, 1, 1, 0, 20, 0)] = 0.14;

            IFunction[] processCoefficients = new[]
            {
                timeSeries1,
                WaterQualityFunctionFactory.CreateConst("B", 5.5, "B", "g/L", "B"),
                WaterQualityFunctionFactory.CreateNetworkCoverage("C", 10.5, "C", "mg/mL", "C"),
                timeSeries2
            };

            string expectedString = "FUNCTIONS" + Environment.NewLine +
                                    "A" + Environment.NewLine +
                                    "LINEAR DATA" + Environment.NewLine +
                                    "2010/01/01-00:00:00 0.12" + Environment.NewLine +
                                    "2010/01/01-00:10:00 0.16" + Environment.NewLine +
                                    Environment.NewLine +
                                    "FUNCTIONS" + Environment.NewLine +
                                    "D" + Environment.NewLine +
                                    "DATA" + Environment.NewLine +
                                    "2010/01/01-00:00:00 0.12" + Environment.NewLine +
                                    "2010/01/01-00:10:00 0.18" + Environment.NewLine +
                                    "2010/01/01-00:20:00 0.14" + Environment.NewLine +
                                    Environment.NewLine;

            string functionsInclude = IncludeFileFactory.CreateFunctionsInclude(processCoefficients);
            Assert.AreEqual(expectedString, functionsInclude);
        }

        [Test]
        public void CreateParametersInclude2DTest()
        {
            UnstructuredGrid staggeredGrid = CreateTwoCellStaggeredGrid();

            var coverageA = (UnstructuredGridCellCoverage) WaterQualityFunctionFactory.CreateUnstructuredGridCellCoverage("A", 2.0, "A", "mg", "A");
            coverageA.Grid = staggeredGrid;
            coverageA[0] = 2.0;
            coverageA[1] = 4.0;

            var coverageB = (UnstructuredGridCellCoverage) WaterQualityFunctionFactory.CreateUnstructuredGridCellCoverage("D", 3.0, "D", "mg", "D");
            coverageB.Grid = staggeredGrid;
            coverageB[0] = 2.0;
            coverageB[1] = 5.5;

            UnstructuredGridCellCoverage[] processCoefficients = new[]
            {
                coverageA,
                coverageB
            };

            var waqInitializationSettings = new WaqInitializationSettings
            {
                ProcessCoefficients = processCoefficients,
                NumberOfLayers = 1
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

            // call
            string parametersInclude = IncludeFileFactory.CreateParametersInclude(waqInitializationSettings);

            // assert
            Assert.AreEqual(expectedString, parametersInclude);
        }

        [Test]
        public void TestSurfParametersWritten()
        {
            const string SurfacesFile = "uni3d.srf";
            var waqInitializationSettings = new WaqInitializationSettings {SurfacesFile = SurfacesFile};

            string expectedString = "PARAMETERS" + Environment.NewLine +
                                    "'Surf'" + Environment.NewLine +
                                    "ALL" + Environment.NewLine +
                                    "BINARY_FILE 'uni3d.srf' ; from horizontal-surfaces-file key in hyd file" + Environment.NewLine;

            Assert.AreEqual(expectedString, IncludeFileFactory.CreateParametersInclude(waqInitializationSettings));
        }

        [Test]
        public void CreateSegfunctionsIncludeTest()
        {
            // setup
            FunctionFromHydroDynamics aFunc = WaterQualityFunctionFactory.CreateFunctionFromHydroDynamics("A", 1.2, "irrelevant", "g", "A");
            aFunc.FilePath = "<some filepath set by model>";

            FunctionFromHydroDynamics bFunc = WaterQualityFunctionFactory.CreateFunctionFromHydroDynamics("B", 3.4, "still irrelevant", "g", "B");
            bFunc.FilePath = "<another filepath set by model>";

            var initSettings = new WaqInitializationSettings
            {
                ProcessCoefficients = new[]
                {
                    aFunc,
                    bFunc
                }
            };

            // call
            string segfunctionInclude = IncludeFileFactory.CreateSegfunctionsInclude(initSettings);

            // assert
            string expectedText =
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
            string dataDir = Path.Combine(TestHelper.GetTestDataDirectory(), @"TestSegFunctionFiles");
            string pathA = Path.Combine(dataDir, @"segFileA.tau");
            string pathB = Path.Combine(dataDir, @"segFileB.vol");
            // setup
            SegmentFileFunction aFunc = WaterQualityFunctionFactory.CreateSegmentFunction("A", 1.2, "irrelevant", "g", "A", pathA);
            SegmentFileFunction bFunc = WaterQualityFunctionFactory.CreateSegmentFunction("B", 3.4, "still irrelevant", "g", "B", pathB);

            var initSettings = new WaqInitializationSettings
            {
                ProcessCoefficients = new[]
                {
                    aFunc,
                    bFunc
                }
            };

            // call
            string segfunctionInclude = IncludeFileFactory.CreateSegfunctionsInclude(initSettings);

            // assert
            string expectedTextUnformatted =
                "SEG_FUNCTIONS" + Environment.NewLine +
                "'A'" + Environment.NewLine +
                "ALL" + Environment.NewLine +
                "BINARY_FILE '" + pathA + "'" + Environment.NewLine +
                Environment.NewLine +
                "SEG_FUNCTIONS" + Environment.NewLine +
                "'B'" + Environment.NewLine +
                "ALL" + Environment.NewLine +
                "BINARY_FILE '" + pathB + "'" + Environment.NewLine +
                Environment.NewLine;
            string expectedText = FileUtils.ReplaceDirectorySeparator(expectedTextUnformatted);
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

            // call
            string numericalOptionsInclude = IncludeFileFactory.CreateNumericalOptionsInclude(initSettings);

            // assert
            string expectedText =
                "CONSTANTS 'CLOSE_ERR' DATA 1 ; If defined, allow delwaq to correct water volumes to keep concentrations continuous" + Environment.NewLine +
                "CONSTANTS 'NOTHREADS' DATA 1 ; Number of threads used by delwaq" + Environment.NewLine +
                "CONSTANTS 'DRY_THRESH' DATA 0.01 ; Dry cell threshold" + Environment.NewLine +
                "CONSTANTS 'maxiter' DATA 123 ; Maximum number of iterations" + Environment.NewLine +
                "CONSTANTS 'tolerance' DATA 1E-06 ; Convergence tolerance" + Environment.NewLine +
                "CONSTANTS 'iteration report' DATA 0 ; Write iteration report (when 1) or not (when 0)" + Environment.NewLine;
            Assert.AreEqual(expectedText, numericalOptionsInclude);
        }

        [Test]
        public void CreateNumericalOptionsIncludeTestDefaultNumberOfCoresIs2()
        {
            // setup
            var initSettings = new WaqInitializationSettings();

            // call
            string numericalOptionsInclude = IncludeFileFactory.CreateNumericalOptionsInclude(initSettings);

            // assert
            var expectedText = "CONSTANTS 'NOTHREADS' DATA 2 ; Number of threads used by delwaq";
            Assert.IsTrue(numericalOptionsInclude.Contains(expectedText));
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

            // call
            string numericalOptionsInclude = IncludeFileFactory.CreateNumericalOptionsInclude(initSettings);

            // assert
            string expectedText =
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

            // call
            string numericalOptionsInclude = IncludeFileFactory.CreateNumericalOptionsInclude(initSettings);

            // assert
            string expectedText =
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
            UnstructuredGrid staggeredGrid = CreateTwoCellStaggeredGrid();
            var coverageA = (UnstructuredGridCellCoverage) WaterQualityFunctionFactory.CreateUnstructuredGridCellCoverage("A", 2.0, "A", "mg", "A");
            coverageA.Grid = staggeredGrid;
            coverageA[0] = 2.0;
            coverageA[1] = 4.0;

            string result = IncludeFileFactory.CreateSpatialDispersionInclude(coverageA, 2);

            string expectedString = "CONSTANTS 'ACTIVE_HDisperAdd' DATA 1.0" + Environment.NewLine +
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
            string result = IncludeFileFactory.CreateVerticalDiffusionInclude("someFile.vdf", true);

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
            string result = IncludeFileFactory.CreateVerticalDiffusionInclude("someFile.vdf", false);

            Assert.AreEqual(string.Empty, result);
        }

        [Test]
        public void CreateVerticalDiffusionNoFileTest()
        {
            string result = IncludeFileFactory.CreateVerticalDiffusionInclude(null, true);

            Assert.AreEqual(string.Empty, result);
        }

        #endregion

        #region Block 8

        [Test]
        public void CreateSpatialInitialConditionsFileConstantsTest()
        {
            // setup
            UnstructuredGrid staggeredGrid = CreateTwoCellStaggeredGrid();

            var coverageA = (UnstructuredGridCellCoverage) WaterQualityFunctionFactory.CreateUnstructuredGridCellCoverage("A", 2.0, "A", "mg", "A");
            coverageA.Grid = staggeredGrid;
            coverageA[0] = 2.0;
            coverageA[1] = 4.0;

            var coverageB = (UnstructuredGridCellCoverage) WaterQualityFunctionFactory.CreateUnstructuredGridCellCoverage("D", 3.0, "D", "mg", "D");
            coverageB.Grid = staggeredGrid;
            coverageB[0] = 2.0;
            coverageB[1] = 5.5;

            UnstructuredGridCellCoverage[] initialConditions = new[]
            {
                coverageA,
                coverageB
            };

            var waqInitializationSettings = new WaqInitializationSettings
            {
                InitialConditions = initialConditions,
                NumberOfLayers = 1
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

            // call
            string parametersInclude = IncludeFileFactory.CreateInitialConditionsInclude(waqInitializationSettings);

            // assert
            Assert.AreEqual(expectedString, parametersInclude);
        }

        [Test]
        public void TestCreateInitialConditionsIncludeWithoutInitialConditionsAvailable()
        {
            // setup
            var settings = new WaqInitializationSettings {InitialConditions = new List<IFunction>()};

            // call
            string fileContents = IncludeFileFactory.CreateInitialConditionsInclude(settings);

            // assert
            Assert.AreEqual(string.Empty, fileContents);
        }

        [Test]
        public void TestCreateInitialConditionsWithConstantInitialConditions()
        {
            // setup
            var settings = new WaqInitializationSettings
            {
                InitialConditions = new List<IFunction>
                {
                    WaterQualityFunctionFactory.CreateConst("A", 1.5, "A", "mg/L", "A"),
                    WaterQualityFunctionFactory.CreateConst("B", 2.9, "B", "g/L", "B"),
                    WaterQualityFunctionFactory.CreateUnstructuredGridCellCoverage("C", 99.33, "C", "test", "C")
                }
            };

            string expectedString2 = "MASS/M2" + Environment.NewLine +
                                     "INITIALS" + Environment.NewLine +
                                     "'A'" + Environment.NewLine +
                                     "'B'" + Environment.NewLine +
                                     "DEFAULTS" + Environment.NewLine +
                                     "1.5" + Environment.NewLine +
                                     "2.9" + Environment.NewLine +
                                     "INITIALS" + Environment.NewLine +
                                     "'C'" + Environment.NewLine +
                                     "ALL" + Environment.NewLine +
                                     "DATA" + Environment.NewLine + Environment.NewLine;

            // call
            string fileContents = IncludeFileFactory.CreateInitialConditionsInclude(settings);

            // assert
            Assert.AreEqual(expectedString2, fileContents);
        }

        #endregion Block 8

        #region Block 9

        [Test]
        public void TestCreateMapVarInclude()
        {
            var substanceProcessLib = new SubstanceProcessLibrary();

            substanceProcessLib.OutputParameters.AddRange(
                new[]
                {
                    new WaterQualityOutputParameter
                    {
                        Name = "Winddir",
                        ShowInMap = true
                    },
                    new WaterQualityOutputParameter
                    {
                        Name = "Vwind",
                        ShowInMap = true
                    },
                    new WaterQualityOutputParameter
                    {
                        Name = "Temp",
                        ShowInMap = true
                    },
                    new WaterQualityOutputParameter
                    {
                        Name = "Rad",
                        ShowInMap = true
                    },
                    new WaterQualityOutputParameter
                    {
                        Name = "Volume",
                        ShowInMap = true
                    },
                    new WaterQualityOutputParameter
                    {
                        Name = "Surf",
                        ShowInMap = true
                    },
                    new WaterQualityOutputParameter
                    {
                        Name = "Theta",
                        ShowInMap = true
                    },
                    new WaterQualityOutputParameter {Name = "NotSelected1"},
                    new WaterQualityOutputParameter {Name = "NotSelected2"}
                });

            string expectedString = "2 ; perform default output and extra parameters listed below" + Environment.NewLine +
                                    "7 ; number of parameters listed" + Environment.NewLine +
                                    " 'Winddir'" + Environment.NewLine +
                                    " 'Vwind'" + Environment.NewLine +
                                    " 'Temp'" + Environment.NewLine +
                                    " 'Rad'" + Environment.NewLine +
                                    " 'Volume'" + Environment.NewLine +
                                    " 'Surf'" + Environment.NewLine +
                                    " 'Theta'" + Environment.NewLine;

            string mapVarInclude = IncludeFileFactory.CreateMapVarInclude(substanceProcessLib);
            Assert.AreEqual(expectedString, mapVarInclude);
        }

        [Test]
        public void TestCreateHisVarInclude()
        {
            var substanceProcessLib = new SubstanceProcessLibrary();

            substanceProcessLib.OutputParameters.AddRange(
                new[]
                {
                    new WaterQualityOutputParameter
                    {
                        Name = "Winddir",
                        ShowInHis = true
                    },
                    new WaterQualityOutputParameter
                    {
                        Name = "Vwind",
                        ShowInHis = true
                    },
                    new WaterQualityOutputParameter
                    {
                        Name = "Temp",
                        ShowInHis = true
                    },
                    new WaterQualityOutputParameter
                    {
                        Name = "Rad",
                        ShowInHis = true
                    },
                    new WaterQualityOutputParameter
                    {
                        Name = "Volume",
                        ShowInHis = true
                    },
                    new WaterQualityOutputParameter
                    {
                        Name = "Surf",
                        ShowInHis = true
                    },
                    new WaterQualityOutputParameter
                    {
                        Name = "Theta",
                        ShowInHis = true
                    },
                    new WaterQualityOutputParameter {Name = "NotSelected1"},
                    new WaterQualityOutputParameter {Name = "NotSelected2"}
                });

            string expectedString = "2 ; perform default output and extra parameters listed below" + Environment.NewLine +
                                    "7 ; number of parameters listed" + Environment.NewLine +
                                    " 'Winddir' 'volume'" + Environment.NewLine +
                                    " 'Vwind' 'volume'" + Environment.NewLine +
                                    " 'Temp' 'volume'" + Environment.NewLine +
                                    " 'Rad' 'volume'" + Environment.NewLine +
                                    " 'Volume' ' '" + Environment.NewLine +
                                    " 'Surf' ' '" + Environment.NewLine +
                                    " 'Theta' 'volume'" + Environment.NewLine;

            string hisVarInclude = IncludeFileFactory.CreateHisVarInclude(substanceProcessLib);
            Assert.AreEqual(expectedString, hisVarInclude);
        }

        #endregion Block 9
    }
}