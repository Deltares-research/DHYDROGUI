using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using DelftTools.Functions;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.BoundaryData;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.Model;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Model;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.Properties;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.Model
{
    [TestFixture]
    public class WaqFileBasedPreProcessorTest
    {
        [Test]
        public void InitializeWaqWithoutInitializationSettings()
        {
            // Call
            void Call() => new WaqFileBasedPreProcessor().InitializeWaq(null);

            // Assert
            Assert.That(Call, Throws.Exception.TypeOf<NullReferenceException>()
                                    .With.Message.EqualTo("Initialization settings may not be null"));
        }

        [Test]
        public void InitializeWaqWithEmptyInputFile()
        {
            // Call
            void Call() => new WaqFileBasedPreProcessor().InitializeWaq(new WaqInitializationSettings());

            // Assert
            Assert.That(Call, Throws.Exception.TypeOf<NullReferenceException>()
                                    .With.Message.EqualTo("Input file may not be null"));
        }

        [Test]
        public void InitializeWaqWithoutWorkDirectory()
        {
            var preprocessor = new WaqFileBasedPreProcessor();
            var waqInitializationSettings = new WaqInitializationSettings
            {
                InputFile = new TextDocument {Content = "a"},
                SubstanceProcessLibrary = SubstanceProcessLibraryTestHelper.CreateDefaultSubstanceProcessLibrary(),
                Settings = Substitute.For<IWaterQualityModelSettings>()
            };

            waqInitializationSettings.Settings.WorkDirectory.Returns(string.Empty);

            var exception = Assert.Throws<NullReferenceException>(() => preprocessor.InitializeWaq(waqInitializationSettings));
            Assert.AreEqual(exception.Message, "Work directory must be set");
        }

        [Test]
        [Category(TestCategory.Slow)]
        [Category(TestCategory.DataAccess)]
        public void PathConversion()
        {
            // arrange
            var preprocessor = new WaqFileBasedPreProcessor();

            string currentDirectory = Directory.GetCurrentDirectory();
            string waqModelDataDir = Path.Combine(currentDirectory, "A");
            string workingDirectory = Path.GetFullPath(@".\TestInitializeWaqForFileBasedProcessing\");
            string workingOutputDirectory = Path.Combine(workingDirectory, FileConstants.OutputDirectoryName);
            string includeDirectory = Path.Combine(workingOutputDirectory, FileConstants.IncludesDirectoryName);

            // Delete directory to avoid failing test on build server when there is already a previous version of the directory
            FileUtils.DeleteIfExists(workingDirectory);
            Directory.CreateDirectory(workingDirectory);

            FileUtils.DeleteIfExists(waqModelDataDir);
            Directory.CreateDirectory(waqModelDataDir);
            try
            {
                string boundaryDataFolderPath = Path.Combine(waqModelDataDir, "haha");
                var manager = new DataTableManager {FolderPath = boundaryDataFolderPath};
                manager.CreateNewDataTable("A", "B", "C.usefors", "E");
                manager.CreateNewDataTable("F", "G", "H.usefors", "J");

                string loadsDataFolderPath = Path.Combine(waqModelDataDir, "lol");
                var loadsManager = new DataTableManager {FolderPath = loadsDataFolderPath};
                loadsManager.CreateNewDataTable("O", "P", "Q.usefors", "R");
                loadsManager.CreateNewDataTable("S", "T", "U.usefors", "V");

                DataTable[] loads = loadsManager.DataTables.ToArray();
                loads[1].IsEnabled = false;

                var model = Substitute.For<WaterQualityModel>();

                model.DataItems = new EventedList<IDataItem>();

                WaqInitializationSettings waqInitializationSettings =
                    CreateValidWaqInitializationSettings(workingDirectory, manager, loadsManager);

                //act
                preprocessor.InitializeWaq(waqInitializationSettings);
                string[] files = Directory.GetFiles(includeDirectory);
                string volumeFilePath = File.ReadAllText(files.FirstOrDefault(file => file.Contains("B3_volumes.inc")));
                string attributesFilePath =
                    File.ReadAllText(files.FirstOrDefault(file => file.Contains("B3_attributes.inc")));
                string areaFilePath = File.ReadAllText(files.FirstOrDefault(file => file.Contains("B4_area.inc")));
                string flowsFilePath = File.ReadAllText(files.FirstOrDefault(file => file.Contains("B4_flows.inc")));
                string lengthFilePath = File.ReadAllText(files.FirstOrDefault(file => file.Contains("B4_length.inc")));
                string pointersFilePath = File.ReadAllText(files.FirstOrDefault(file => file.Contains("B4_pointers.inc")));
                string parametersFilePath =
                    File.ReadAllText(files.FirstOrDefault(file => file.Contains("B7_parameters.inc")));
                string verticalDiffusionFilePath =
                    File.ReadAllText(files.FirstOrDefault(file => file.Contains("B7_vdiffusion.inc")));
                string loadsFilePath = File.ReadAllText(files.FirstOrDefault(file => file.Contains("B6_loads_data.inc")));

                //assert
                Assert.That(volumeFilePath, Does.Contain(@"dir1/dir2/volumesfile.inc"));
                Assert.That(attributesFilePath, Does.Contain(@"dir1/dir2/attributesfile.inc"));
                Assert.That(areaFilePath, Does.Contain(@"dir1/dir2/areasfile.inc"));
                Assert.That(flowsFilePath, Does.Contain(@"dir1/dir2/flowsfile.inc"));
                Assert.That(lengthFilePath, Does.Contain(@"dir1/dir2/lengthsfile.inc"));
                Assert.That(pointersFilePath, Does.Contain(@"dir1/dir2/pointersfile.inc"));
                Assert.That(parametersFilePath, Does.Contain(@"dir1/dir2/parametersfile.inc"));
                Assert.That(loadsFilePath, Does.Contain(@"INCLUDE '../../A/lol/O.tbl'"));
                Assert.That(verticalDiffusionFilePath, Does.Contain(@"dir1/dir2/verticaldiffusionfile.inc"));
            }
            finally
            {
                FileUtils.DeleteIfExists(workingDirectory);
                FileUtils.DeleteIfExists(waqModelDataDir);
            }
        }

        [Test]
        [Category(TestCategory.Slow)]
        [Category(TestCategory.DataAccess)]
        public void TestInitializeWaqForFileBasedProcessing()
        {
            // setup
            var preprocessor = new WaqFileBasedPreProcessor();

            string currentDirectory = Directory.GetCurrentDirectory();
            string waqModelDataDir = Path.Combine(currentDirectory, "A");
            string workingDirectory = Path.GetFullPath(@".\TestInitializeWaqForFileBasedProcessing\");
            string workingOutputDirectory = Path.Combine(workingDirectory, FileConstants.OutputDirectoryName);
            string includeDirectory = Path.Combine(workingOutputDirectory, FileConstants.IncludesDirectoryName);

            // Delete directory to avoid failing test on build server when there is already a previous version of the directory
            FileUtils.DeleteIfExists(workingDirectory);
            Directory.CreateDirectory(workingDirectory);

            FileUtils.DeleteIfExists(waqModelDataDir);
            Directory.CreateDirectory(waqModelDataDir);

            try
            {
                string boundaryDataFolderPath = Path.Combine(waqModelDataDir, "haha");
                var manager = new DataTableManager {FolderPath = boundaryDataFolderPath};
                manager.CreateNewDataTable("A", "B", "C.usefors", "E");
                manager.CreateNewDataTable("F", "G", "H.usefors", "J");

                string loadsDataFolderPath = Path.Combine(waqModelDataDir, "lol");
                var loadsManager = new DataTableManager {FolderPath = loadsDataFolderPath};
                loadsManager.CreateNewDataTable("K", "L", "M.usefors", "N");
                loadsManager.CreateNewDataTable("O", "P", "Q.usefors", "R");
                loadsManager.CreateNewDataTable("S", "T", "U.usefors", "V");

                DataTable[] loads = loadsManager.DataTables.ToArray();
                loads[2].IsEnabled = false;

                var model = Substitute.For<WaterQualityModel>();

                model.DataItems = new EventedList<IDataItem>();

                WaqInitializationSettings waqInitializationSettings = CreateValidWaqInitializationSettings(workingDirectory, manager, loadsManager);

                // call
                preprocessor.InitializeWaq(waqInitializationSettings);

                // setup
                Assert.IsTrue(File.Exists(Path.Combine(workingOutputDirectory, FileConstants.InputFileName)));
                Assert.IsTrue(Directory.Exists(includeDirectory));

                var includeFilesToCheck = new List<string>
                {
                    "B1_t0.inc",
                    "B1_sublist.inc",
                    "B2_numsettings.inc",
                    "B2_simtimers.inc",
                    "B2_outlocs.inc",
                    "B2_outputtimers.inc",
                    "B3_ugrid.inc",
                    "B3_nrofseg.inc",
                    "B3_attributes.inc",
                    "B3_volumes.inc",
                    "B4_nrofexch.inc",
                    "B4_pointers.inc",
                    "B4_cdispersion.inc",
                    "B4_area.inc",
                    "B4_flows.inc",
                    "B4_length.inc",
                    "B5_boundlist.inc",
                    "B5_bounddata.inc",
                    "B5_boundaliases.inc",
                    "B6_loads.inc",
                    "B6_loads_data.inc",
                    "B6_loads_aliases.inc",
                    "B7_processes.inc",
                    "B7_constants.inc",
                    "B7_functions.inc",
                    "B7_parameters.inc",
                    "B7_dispersion.inc",
                    "B7_vdiffusion.inc",
                    "B7_segfunctions.inc",
                    "B7_numerical_options.inc",
                    "B8_initials.inc",
                    "B9_Mapvar.inc",
                    "B9_Hisvar.inc"
                };

                var useforsFilesToCheck = new Dictionary<string, string[]>
                {
                    {
                        "haha", new[]
                        {
                            "C.usefors",
                            "H.usefors"
                        }
                    },
                    {
                        "lol", new[]
                        {
                            "M.usefors",
                            "Q.usefors"
                        }
                    }
                };

                List<string> includeFilesFound = Directory.GetFiles(includeDirectory, "*.inc").Select(Path.GetFileName).ToList();
                List<string> missingFiles = includeFilesToCheck.Except(includeFilesFound).ToList();
                List<string> unexpectedFiles = includeFilesFound.Except(includeFilesToCheck).ToList();

                if (missingFiles.Any())
                {
                    Assert.Fail("Missing files in folder {0}: {1}",
                                includeDirectory, string.Join(",", missingFiles));
                }

                if (unexpectedFiles.Any())
                {
                    Assert.Fail("Unexpected files in folder {0}: {1}",
                                includeDirectory, string.Join(",", unexpectedFiles));
                }

                foreach (KeyValuePair<string, string[]> folderNameAndExpectedUseforsFiles in useforsFilesToCheck)
                {
                    string folderPathToCheck = Path.Combine(includeDirectory, folderNameAndExpectedUseforsFiles.Key);
                    string[] useforsFilesFound = Directory.GetFiles(folderPathToCheck, "*.usefors").Select(Path.GetFileName).ToArray();
                    missingFiles = folderNameAndExpectedUseforsFiles.Value.Except(useforsFilesFound).ToList();
                    unexpectedFiles = useforsFilesFound.Except(folderNameAndExpectedUseforsFiles.Value).ToList();
                    if (missingFiles.Any())
                    {
                        Assert.Fail("Missing files in folder {0}: {1}",
                                    folderPathToCheck, string.Join(",", missingFiles));
                    }

                    if (unexpectedFiles.Any())
                    {
                        Assert.Fail("Unexpected files in folder {0}: {1}",
                                    folderPathToCheck, string.Join(",", unexpectedFiles));
                    }
                }
            }
            finally
            {
                Directory.SetCurrentDirectory(currentDirectory);
                FileUtils.DeleteIfExists(workingDirectory);
                FileUtils.DeleteIfExists(waqModelDataDir);
            }
        }

        [Test]
        public void GetDataTableUserforsRelativeFolderPathTest()
        {
            // setup
            var managerStub = Substitute.For<DataTableManager>();
            managerStub.FolderPath = Path.Combine("bla", "foo", "bar");

            // call
            string relativePath = WaqFileBasedPreProcessor.GetDataTableUseforsRelativeFolderPath(managerStub);

            // assert
            Assert.AreEqual(Path.Combine("includes_deltashell", "bar"), relativePath);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void WriteDisabledDatatables_CreatesNoFolder()
        {
            // setup
            var preprocessor = new WaqFileBasedPreProcessor();

            string currentDirectory = Directory.GetCurrentDirectory();
            string waqModelDataDir = Path.Combine(currentDirectory, "B");
            string workingDirectory = Path.GetFullPath(@".\TestInitializeWaqForFileBasedProcessing\");
            string workingOutputDirectory = Path.Combine(workingDirectory, FileConstants.OutputDirectoryName);
            string includeDirectory = Path.Combine(workingOutputDirectory, FileConstants.IncludesDirectoryName);
            string listFile = workingOutputDirectory + "deltashell.lst";
            string processFile = workingOutputDirectory + "deltashell.lsp";

            // Delete directory to avoid failing test on build server when there is already a previous version of the directory
            FileUtils.DeleteIfExists(workingDirectory);
            Directory.CreateDirectory(workingDirectory);

            FileUtils.DeleteIfExists(waqModelDataDir);
            Directory.CreateDirectory(waqModelDataDir);

            try
            {
                FileStream listFileStream = File.Create(listFile);
                FileStream processFileStream = File.Create(processFile);
                byte[] byteArray = Encoding.ASCII.GetBytes("Test file");

                listFileStream.Write(byteArray, 0, byteArray.Length);
                processFileStream.Write(byteArray, 0, byteArray.Length);

                listFileStream.Close();
                processFileStream.Close();

                string boundaryDataFolderPath = Path.Combine(waqModelDataDir, "haha");
                var boundaryDataManager = new DataTableManager {FolderPath = boundaryDataFolderPath};
                boundaryDataManager.CreateNewDataTable("A", "B", "C.usefors", "E");
                boundaryDataManager.CreateNewDataTable("F", "G", "H.usefors", "J");

                foreach (DataTable dataTable in boundaryDataManager.DataTables)
                {
                    dataTable.IsEnabled = false; // disable all data tables, so no folder is created
                }

                var model = Substitute.For<WaterQualityModel>();

                model.DataItems = new EventedList<IDataItem>();

                WaqInitializationSettings waqInitializationSettings = CreateValidWaqInitializationSettings(workingDirectory,
                                                                                                           boundaryDataManager,
                                                                                                           new DataTableManager());
                // call
                preprocessor.InitializeWaq(waqInitializationSettings);

                // setup
                Assert.IsTrue(Directory.Exists(includeDirectory));

                Assert.IsFalse(Directory.Exists(Path.Combine(includeDirectory, Path.GetFileName(boundaryDataManager.FolderPath))));
            }
            finally
            {
                Directory.SetCurrentDirectory(currentDirectory);
                FileUtils.DeleteIfExists(workingDirectory);
                FileUtils.DeleteIfExists(waqModelDataDir);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void WriteEmptyDatatableManager_CreatesNoFolder()
        {
            // setup
            var preprocessor = new WaqFileBasedPreProcessor();

            string currentDirectory = Directory.GetCurrentDirectory();
            string waqModelDataDir = Path.Combine(currentDirectory, "B");
            string workingDirectory = Path.GetFullPath(@".\TestInitializeWaqForFileBasedProcessing\");
            string workingOutputDirectory = Path.Combine(workingDirectory, FileConstants.OutputDirectoryName);
            string includeDirectory = Path.Combine(workingOutputDirectory, FileConstants.IncludesDirectoryName);
            string listFile = workingOutputDirectory + "deltashell.lst";
            string processFile = workingOutputDirectory + "deltashell.lsp";

            // Delete directory to avoid failing test on build server when there is already a previous version of the directory
            FileUtils.DeleteIfExists(workingDirectory);
            Directory.CreateDirectory(workingDirectory);

            FileUtils.DeleteIfExists(waqModelDataDir);
            Directory.CreateDirectory(waqModelDataDir);

            try
            {
                FileStream listFileStream = File.Create(listFile);
                FileStream processFileStream = File.Create(processFile);
                byte[] byteArray = Encoding.ASCII.GetBytes("Test file");

                listFileStream.Write(byteArray, 0, byteArray.Length);
                processFileStream.Write(byteArray, 0, byteArray.Length);

                listFileStream.Close();
                processFileStream.Close();

                string boundaryDataFolderPath = Path.Combine(waqModelDataDir, "haha");
                var boundaryDataManager = new DataTableManager {FolderPath = boundaryDataFolderPath};
                boundaryDataManager.CreateNewDataTable("A", "B", "C.usefors", "E");
                boundaryDataManager.CreateNewDataTable("F", "G", "H.usefors", "J");

                string loadsDataFolderPath = Path.Combine(waqModelDataDir, "lol");
                var loadsManager = new DataTableManager {FolderPath = loadsDataFolderPath};

                var model = Substitute.For<WaterQualityModel>();

                model.DataItems = new EventedList<IDataItem>();

                var waqInitializationSettings = new WaqInitializationSettings
                {
                    InputFile = new TextDocument {Content = Resources.TestInputFile},
                    SubstanceProcessLibrary = SubstanceProcessLibraryTestHelper.CreateDefaultSubstanceProcessLibrary(),
                    SimulationStartTime = new DateTime(2010, 1, 1),
                    SimulationStopTime = new DateTime(2010, 1, 2),
                    SimulationTimeStep = new TimeSpan(1, 0, 0, 0),
                    InitialConditions = new Collection<IFunction>(new[]
                    {
                        WaterQualityFunctionFactory.CreateConst("AAP", 10.0, "AAP", "mg/s", "AAP")
                    }),
                    ProcessCoefficients = new Collection<IFunction>(),
                    Dispersion = new Collection<IFunction>(new[]
                    {
                        WaterQualityFunctionFactory.CreateConst("Dispersion", 0.2d, "Dispersion", "m2/s", null)
                    }),
                    Settings = Substitute.For<IWaterQualityModelSettings>(),
                    BoundaryNodeIds = new Dictionary<WaterQualityBoundary, int[]>(),
                    LoadAndIds = new Dictionary<WaterQualityLoad, int>(),
                    OutputLocations = new Dictionary<string, IList<int>>(),
                    BoundaryDataManager = boundaryDataManager,
                    LoadsDataManager = loadsManager, // same data for loads as for boundary data
                    LoadsAliases = new Dictionary<string, IList<string>>(),
                    BoundaryAliases = new Dictionary<string, IList<string>>()
                };

                waqInitializationSettings.Settings.WorkDirectory.Returns(workingDirectory);

                // call
                preprocessor.InitializeWaq(waqInitializationSettings);

                // setup
                Assert.IsTrue(Directory.Exists(includeDirectory));

                Assert.IsTrue(Directory.Exists(Path.Combine(includeDirectory, Path.GetFileName(boundaryDataManager.FolderPath))));
                Assert.IsFalse(Directory.Exists(Path.Combine(includeDirectory, Path.GetFileName(loadsManager.FolderPath))));
            }
            finally
            {
                Directory.SetCurrentDirectory(currentDirectory);
                FileUtils.DeleteIfExists(workingDirectory);
                FileUtils.DeleteIfExists(waqModelDataDir);
            }
        }

        [Test]
        public void InitializeWaq_ThenWorkingOutputDirectoryIsCreated()
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Setup
                string workDirectoryPath = tempDirectory.Path;
                string expectedOutputDirectory = Path.Combine(workDirectoryPath, FileConstants.OutputDirectoryName);

                WaqInitializationSettings initializationSettings =
                    CreateValidWaqInitializationSettings(workDirectoryPath, new DataTableManager(),
                                                         new DataTableManager());

                // Precondition
                Assert.That(!Directory.Exists(expectedOutputDirectory),
                            "This test is unreliable when the working directory already exists.");

                // Call
                new WaqFileBasedPreProcessor().InitializeWaq(initializationSettings);

                // Assert
                Assert.That(Directory.Exists(expectedOutputDirectory),
                            "Working output directory should be created when initializing.");
            }
        }

        [Test]
        public void InitializeWaq_WhenNonEmptyWorkingOutputDirectoryAlreadyExists_ThenNewEmptyFolderIsCreated()
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Setup
                string workDirectoryPath = tempDirectory.Path;
                string expectedOutputDirectory = Path.Combine(workDirectoryPath, FileConstants.OutputDirectoryName);
                string filePath = Path.Combine(expectedOutputDirectory, "file.txt");

                WaqInitializationSettings initializationSettings =
                    CreateValidWaqInitializationSettings(workDirectoryPath, new DataTableManager(),
                                                         new DataTableManager());

                Directory.CreateDirectory(expectedOutputDirectory);
                File.WriteAllText(filePath, "output_file");

                // Precondition
                Assert.That(File.Exists(filePath),
                            "This test is unreliable when this file does not exist.");

                // Call
                new WaqFileBasedPreProcessor().InitializeWaq(initializationSettings);

                // Assert
                Assert.That(Directory.Exists(expectedOutputDirectory),
                            "Working output directory should be created when initializing.");
                Assert.That(!File.Exists(filePath),
                            "Working output directory should be emptied to delete old output files");
            }
        }

        private static WaqInitializationSettings CreateValidWaqInitializationSettings(
            string workingDirectory, DataTableManager manager, DataTableManager loadsManager)
        {
            var waqInitializationSettings = new WaqInitializationSettings
            {
                InputFile = new TextDocument {Content = Resources.TestInputFile},
                SubstanceProcessLibrary = SubstanceProcessLibraryTestHelper.CreateDefaultSubstanceProcessLibrary(),
                SimulationStartTime = new DateTime(2010, 1, 1),
                SimulationStopTime = new DateTime(2010, 1, 2),
                SimulationTimeStep = new TimeSpan(1, 0, 0, 0),
                InitialConditions = new Collection<IFunction>(new[]
                {
                    WaterQualityFunctionFactory.CreateConst("AAP", 10.0, "AAP", "mg/s", "AAP")
                }),
                ProcessCoefficients = new Collection<IFunction>(),
                Dispersion = new Collection<IFunction>(new[]
                {
                    WaterQualityFunctionFactory.CreateConst("Dispersion", 0.2d, "Dispersion", "m2/s", null)
                }),
                Settings = Substitute.For<IWaterQualityModelSettings>(),
                BoundaryNodeIds = new Dictionary<WaterQualityBoundary, int[]>(),
                LoadAndIds = new Dictionary<WaterQualityLoad, int>(),
                OutputLocations = new Dictionary<string, IList<int>>(),
                BoundaryDataManager = manager,
                LoadsDataManager = loadsManager, // same data for loads as for boundary data
                LoadsAliases = new Dictionary<string, IList<string>>(),
                BoundaryAliases = new Dictionary<string, IList<string>>(),
                VolumesFile = @"dir1\dir2\volumesfile.inc",
                AttributesFile = @"dir1\dir2\attributesfile.inc",
                AreasFile = @"dir1\dir2\areasfile.inc",
                FlowsFile = @"dir1\dir2\flowsfile.inc",
                LengthsFile = @"dir1\dir2\lengthsfile.inc",
                PointersFile = @"dir1\dir2\pointersfile.inc",
                SurfacesFile = @"dir1\dir2\parametersfile.inc",
                VerticalDiffusionFile = @"dir1\dir2\verticaldiffusionfile.inc",
                UseAdditionalVerticalDiffusion = true
            };

            IWaterQualityModelSettings settings = waqInitializationSettings.Settings;
            settings.WorkDirectory.Returns(workingDirectory);
            settings.ProcessesActive.Returns(true);

            return waqInitializationSettings;
        }
    }
}