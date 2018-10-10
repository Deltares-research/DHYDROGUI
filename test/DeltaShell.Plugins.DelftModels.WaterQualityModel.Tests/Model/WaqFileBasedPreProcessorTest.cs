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
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Extensions;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Model;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.Properties;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.Model
{
    [TestFixture]
    public class WaqFileBasedPreProcessorTest
    {

        [Test]
        public void InitializeWaqWithoutInitializationSettings()
        {
            var preprocessor = new WaqFileBasedPreProcessor();
            var exception = Assert.Throws<NullReferenceException>(() => preprocessor.InitializeWaq(null, null));
            Assert.AreEqual(exception.Message, "Initialization settings may not be null");
        }

        [Test]
        public void InitializeWaqWithEmptyInputFile()
        {
            var preprocessor = new WaqFileBasedPreProcessor();
            var exception = Assert.Throws<NullReferenceException>(() => preprocessor.InitializeWaq(new WaqInitializationSettings(), null));
            Assert.AreEqual(exception.Message, "Input file may not be null");
        }

        [Test]
        public void InitializeWaqWithoutWorkDirectory()
        {
            var preprocessor = new WaqFileBasedPreProcessor();
            var waqInitializationSettings = new WaqInitializationSettings
                {
                    InputFile = new TextDocument { Content = "a" },
                    SubstanceProcessLibrary = SubstanceProcessLibraryTestHelper.CreateDefaultSubstanceProcessLibrary()
                };

            var exception = Assert.Throws<NullReferenceException>(() => preprocessor.InitializeWaq(waqInitializationSettings, null));
            Assert.AreEqual(exception.Message, "Work directory must be set");
        }

        [Test]
        [Category(TestCategory.Slow)]
        [Category(TestCategory.DataAccess)]
        public void PathConversion()
        {
            // arrange
            var preprocessor = new WaqFileBasedPreProcessor();

            var currentDirectory = Directory.GetCurrentDirectory();
            var waqModelDataDir = Path.Combine(currentDirectory, "A");
            var workingDirectory = Path.GetFullPath(@".\TestInitializeWaqForFileBasedProcessing\");
            var includeDirectory = workingDirectory + @"includes_deltashell\";

            // Delete directory to avoid failing test on build server when there is already a previous version of the directory
            FileUtils.DeleteIfExists(workingDirectory);
            Directory.CreateDirectory(workingDirectory);

            FileUtils.DeleteIfExists(waqModelDataDir);
            Directory.CreateDirectory(waqModelDataDir);

            var boundaryDataFolderPath = Path.Combine(waqModelDataDir, "haha");
            var manager = new DataTableManager { FolderPath = boundaryDataFolderPath };
            manager.CreateNewDataTable("A", "B", "C.usefors", "E");
            manager.CreateNewDataTable("F", "G", "H.usefors", "J");

            var loadsDataFolderPath = Path.Combine(waqModelDataDir, "lol");
            var loadsManager = new DataTableManager { FolderPath = loadsDataFolderPath };
            loadsManager.CreateNewDataTable("O", "P", "Q.usefors", "R");
            loadsManager.CreateNewDataTable("S", "T", "U.usefors", "V");

            var loads = loadsManager.DataTables.ToArray();
            loads[1].IsEnabled = false;

            var mocks = new MockRepository();
            var model = mocks.Stub<WaterQualityModel>();

            model.DataItems = new EventedList<IDataItem>();

            mocks.ReplayAll();

            var waqInitializationSettings = new WaqInitializationSettings
            {
                InputFile = new TextDocument { Content = Resources.TestInputFile },
                SubstanceProcessLibrary = SubstanceProcessLibraryTestHelper.CreateDefaultSubstanceProcessLibrary(),
                SimulationStartTime = new DateTime(2010, 1, 1),
                SimulationStopTime = new DateTime(2010, 1, 2),
                SimulationTimeStep = new TimeSpan(1, 0, 0, 0),
                InitialConditions = new Collection<IFunction>(new[] { WaterQualityFunctionFactory.CreateConst("AAP", 10.0, "AAP", "mg/s", "AAP") }),
                ProcessCoefficients = new Collection<IFunction>(),
                Dispersion = new Collection<IFunction>(new[] { WaterQualityFunctionFactory.CreateConst("Dispersion", 0.2d, "Dispersion", "m2/s", null) }),
                Settings = new WaterQualityModelSettings { WorkDirectory = workingDirectory },
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
                UseAdditionalVerticalDiffusion = true,
            };
          
            //act
            preprocessor.InitializeWaq(waqInitializationSettings, (displayName, filePath) => model.AddTextDocument(displayName, filePath));
            string[] files = Directory.GetFiles(includeDirectory);
            var volumeFilePath = File.ReadAllText(files.FirstOrDefault(file => file.Contains("B3_volumes.inc")));
            var attributesFilePath = File.ReadAllText(files.FirstOrDefault(file => file.Contains("B3_attributes.inc")));
            var areaFilePath = File.ReadAllText(files.FirstOrDefault(file => file.Contains("B4_area.inc")));
            var flowsFilePath = File.ReadAllText(files.FirstOrDefault(file => file.Contains("B4_flows.inc")));
            var lengthFilePath = File.ReadAllText(files.FirstOrDefault(file => file.Contains("B4_length.inc")));
            var pointersFilePath = File.ReadAllText(files.FirstOrDefault(file => file.Contains("B4_pointers.inc")));
            var parametersFilePath = File.ReadAllText(files.FirstOrDefault(file => file.Contains("B7_parameters.inc")));
            var verticalDiffusionFilePath = File.ReadAllText(files.FirstOrDefault(file => file.Contains("B7_vdiffusion.inc")));
            var loadsFilePath = File.ReadAllText(files.FirstOrDefault(file => file.Contains("B6_loads_data.inc")));

            //assert
            Assert.That(volumeFilePath, Is.StringContaining(@"dir1/dir2/volumesfile.inc"));
            Assert.That(attributesFilePath, Is.StringContaining(@"dir1/dir2/attributesfile.inc"));
            Assert.That(areaFilePath, Is.StringContaining(@"dir1/dir2/areasfile.inc"));
            Assert.That(flowsFilePath, Is.StringContaining(@"dir1/dir2/flowsfile.inc"));
            Assert.That(lengthFilePath, Is.StringContaining(@"dir1/dir2/lengthsfile.inc"));
            Assert.That(pointersFilePath, Is.StringContaining(@"dir1/dir2/pointersfile.inc"));
            Assert.That(parametersFilePath, Is.StringContaining(@"dir1/dir2/parametersfile.inc"));
            Assert.That(loadsFilePath, Is.StringContaining(@"INCLUDE '../A/lol/O.tbl'"));
            Assert.That(verticalDiffusionFilePath, Is.StringContaining(@"dir1/dir2/verticaldiffusionfile.inc"));
        }

        [Test]
        [Category(TestCategory.Slow)]
        [Category(TestCategory.DataAccess)]
        public void TestInitializeWaqForFileBasedProcessing()
        {
            // setup
            var preprocessor = new WaqFileBasedPreProcessor();

            var currentDirectory = Directory.GetCurrentDirectory();
            var waqModelDataDir = Path.Combine(currentDirectory, "A");
            var workingDirectory = Path.GetFullPath(@".\TestInitializeWaqForFileBasedProcessing\");
            var includeDirectory = workingDirectory + @"includes_deltashell\";

            // Delete directory to avoid failing test on build server when there is already a previous version of the directory
            FileUtils.DeleteIfExists(workingDirectory);
            Directory.CreateDirectory(workingDirectory);

            FileUtils.DeleteIfExists(waqModelDataDir);
            Directory.CreateDirectory(waqModelDataDir);

            try
            {
                var boundaryDataFolderPath = Path.Combine(waqModelDataDir, "haha");
                var manager = new DataTableManager { FolderPath = boundaryDataFolderPath };
                manager.CreateNewDataTable("A", "B", "C.usefors", "E");
                manager.CreateNewDataTable("F", "G", "H.usefors", "J");

                var loadsDataFolderPath = Path.Combine(waqModelDataDir, "lol");
                var loadsManager = new DataTableManager { FolderPath = loadsDataFolderPath };
                loadsManager.CreateNewDataTable("K", "L", "M.usefors", "N");
                loadsManager.CreateNewDataTable("O", "P", "Q.usefors", "R");
                loadsManager.CreateNewDataTable("S", "T", "U.usefors", "V");

                var loads = loadsManager.DataTables.ToArray();
                loads[2].IsEnabled = false;

                var mocks = new MockRepository();
                var model = mocks.Stub<WaterQualityModel>();

                model.DataItems = new EventedList<IDataItem>();

                mocks.ReplayAll();

                var waqInitializationSettings = new WaqInitializationSettings
                {
                    InputFile = new TextDocument { Content = Resources.TestInputFile },
                    SubstanceProcessLibrary = SubstanceProcessLibraryTestHelper.CreateDefaultSubstanceProcessLibrary(),
                    SimulationStartTime = new DateTime(2010, 1, 1),
                    SimulationStopTime = new DateTime(2010, 1, 2),
                    SimulationTimeStep = new TimeSpan(1, 0, 0, 0),
                    InitialConditions = new Collection<IFunction>(new[] { WaterQualityFunctionFactory.CreateConst("AAP", 10.0, "AAP", "mg/s", "AAP") }),
                    ProcessCoefficients = new Collection<IFunction>(),
                    Dispersion = new Collection<IFunction>(new [] {WaterQualityFunctionFactory.CreateConst("Dispersion", 0.2d, "Dispersion", "m2/s", null)} ),
                    Settings = new WaterQualityModelSettings { WorkDirectory = workingDirectory },
                    BoundaryNodeIds = new Dictionary<WaterQualityBoundary, int[]>(),
                    LoadAndIds = new Dictionary<WaterQualityLoad, int>(),
                    OutputLocations = new Dictionary<string, IList<int>>(),
                    BoundaryDataManager = manager,
                    LoadsDataManager = loadsManager, // same data for loads as for boundary data
                    LoadsAliases = new Dictionary<string, IList<string>>(),
                    BoundaryAliases = new Dictionary<string, IList<string>>(),
                };

                // call
                preprocessor.InitializeWaq(waqInitializationSettings, (displayName, filePath) => model.AddTextDocument(displayName, filePath));

                // setup
                Assert.IsTrue(File.Exists(workingDirectory + "deltashell.inp"));
                Assert.IsTrue(Directory.Exists(includeDirectory));

                var includeFilesToCheck = new List<string>
                {
                    "B1_t0.inc",
                    "B1_sublist.inc",
                    "B2_numsettings.inc", 
                    "B2_simtimers.inc",
                    "B2_outlocs.inc", 
                    "B2_outputtimers.inc",
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
                    {"haha", new[]{"C.usefors","H.usefors"}},
                    {"lol", new[]{"M.usefors", "Q.usefors"}}
                };

                var includeFilesFound = Directory.GetFiles(includeDirectory, "*.inc").Select(Path.GetFileName).ToList();
                var missingFiles = includeFilesToCheck.Except(includeFilesFound).ToList();
                var unexpectedFiles = includeFilesFound.Except(includeFilesToCheck).ToList();

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

                foreach (var folderNameAndExpectedUseforsFiles in useforsFilesToCheck)
                {
                    var folderPathToCheck = Path.Combine(includeDirectory, folderNameAndExpectedUseforsFiles.Key);
                    var useforsFilesFound = Directory.GetFiles(folderPathToCheck, "*.usefors").Select(Path.GetFileName).ToArray();
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
                
                var listFileTextDocument = model.DataItems
                    .Select(d => d.Value)
                    .OfType<TextDocumentFromFile>()
                    .ToList()
                    .FirstOrDefault(t => t.Name == WaterQualityModel.ListFileDataItemMetaData.Name);
                Assert.IsNotNull(listFileTextDocument);
                Assert.IsTrue(listFileTextDocument.Content.Length > 0);
                Assert.IsFalse(listFileTextDocument.Content.Contains("Test file")); // Previous list file output should be removed

                mocks.VerifyAll();
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
            var mocks = new MockRepository();
            var managerStub = mocks.Stub<DataTableManager>();
            managerStub.FolderPath = Path.Combine("bla", "foo", "bar");
            mocks.ReplayAll();

            // call
            var relativePath = WaqFileBasedPreProcessor.GetDataTableUseforsRelativeFolderPath(managerStub);

            // assert
            Assert.AreEqual(Path.Combine("includes_deltashell", "bar"), relativePath);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void WriteDisabledDatatables_CreatesNoFolder()
        {
            // setup
            var preprocessor = new WaqFileBasedPreProcessor();

            var currentDirectory = Directory.GetCurrentDirectory();
            var waqModelDataDir = Path.Combine(currentDirectory, "B");
            var workingDirectory = Path.GetFullPath(@".\TestInitializeWaqForFileBasedProcessing\");
            var includeDirectory = workingDirectory + @"includes_deltashell\";
            var listFile = workingDirectory + "deltashell.lst";
            var processFile = workingDirectory + "deltashell.lsp";

            // Delete directory to avoid failing test on build server when there is already a previous version of the directory
            FileUtils.DeleteIfExists(workingDirectory);
            Directory.CreateDirectory(workingDirectory);

            FileUtils.DeleteIfExists(waqModelDataDir);
            Directory.CreateDirectory(waqModelDataDir);

            try
            {
                var listFileStream = File.Create(listFile);
                var processFileStream = File.Create(processFile);
                var byteArray = Encoding.ASCII.GetBytes("Test file");

                listFileStream.Write(byteArray, 0, byteArray.Length);
                processFileStream.Write(byteArray, 0, byteArray.Length);

                listFileStream.Close();
                processFileStream.Close();

                var boundaryDataFolderPath = Path.Combine(waqModelDataDir, "haha");
                var boundaryDataManager = new DataTableManager { FolderPath = boundaryDataFolderPath };
                boundaryDataManager.CreateNewDataTable("A", "B", "C.usefors", "E");
                boundaryDataManager.CreateNewDataTable("F", "G", "H.usefors", "J");

                foreach (var dataTable in boundaryDataManager.DataTables)
                {
                    dataTable.IsEnabled = false; // disable all data tables, so no folder is created
                }

                var mocks = new MockRepository();
                var model = mocks.Stub<WaterQualityModel>();

                model.DataItems = new EventedList<IDataItem>();

                mocks.ReplayAll();

                var waqInitializationSettings = new WaqInitializationSettings
                {
                    InputFile = new TextDocument { Content = Resources.TestInputFile },
                    SubstanceProcessLibrary = SubstanceProcessLibraryTestHelper.CreateDefaultSubstanceProcessLibrary(),
                    SimulationStartTime = new DateTime(2010, 1, 1),
                    SimulationStopTime = new DateTime(2010, 1, 2),
                    SimulationTimeStep = new TimeSpan(1, 0, 0, 0),
                    InitialConditions = new Collection<IFunction>(new[] { WaterQualityFunctionFactory.CreateConst("AAP", 10.0, "AAP", "mg/s", "AAP") }),
                    ProcessCoefficients = new Collection<IFunction>(),
                    Dispersion = new Collection<IFunction>(new[] { WaterQualityFunctionFactory.CreateConst("Dispersion", 0.2d, "Dispersion", "m2/s", null) }),
                    Settings = new WaterQualityModelSettings { WorkDirectory = workingDirectory },
                    BoundaryNodeIds = new Dictionary<WaterQualityBoundary, int[]>(),
                    LoadAndIds = new Dictionary<WaterQualityLoad, int>(),
                    OutputLocations = new Dictionary<string, IList<int>>(),
                    BoundaryDataManager = boundaryDataManager,
                    LoadsDataManager = new DataTableManager(),
                    LoadsAliases = new Dictionary<string, IList<string>>(),
                    BoundaryAliases = new Dictionary<string, IList<string>>(),
                };

                // call
                preprocessor.InitializeWaq(waqInitializationSettings, (displayName, filePath) => model.AddTextDocument(displayName, filePath));

                // setup
                Assert.IsTrue(Directory.Exists(includeDirectory));

                Assert.IsFalse(Directory.Exists(Path.Combine(includeDirectory, Path.GetFileName(boundaryDataManager.FolderPath))));

                mocks.VerifyAll();
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

            var currentDirectory = Directory.GetCurrentDirectory();
            var waqModelDataDir = Path.Combine(currentDirectory, "B");
            var workingDirectory = Path.GetFullPath(@".\TestInitializeWaqForFileBasedProcessing\");
            var includeDirectory = workingDirectory + @"includes_deltashell\";
            var listFile = workingDirectory + "deltashell.lst";
            var processFile = workingDirectory + "deltashell.lsp";

            // Delete directory to avoid failing test on build server when there is already a previous version of the directory
            FileUtils.DeleteIfExists(workingDirectory);
            Directory.CreateDirectory(workingDirectory);

            FileUtils.DeleteIfExists(waqModelDataDir);
            Directory.CreateDirectory(waqModelDataDir);

            try
            {
                var listFileStream = File.Create(listFile);
                var processFileStream = File.Create(processFile);
                var byteArray = Encoding.ASCII.GetBytes("Test file");

                listFileStream.Write(byteArray, 0, byteArray.Length);
                processFileStream.Write(byteArray, 0, byteArray.Length);

                listFileStream.Close();
                processFileStream.Close();

                var boundaryDataFolderPath = Path.Combine(waqModelDataDir, "haha");
                var boundaryDataManager = new DataTableManager { FolderPath = boundaryDataFolderPath };
                boundaryDataManager.CreateNewDataTable("A", "B", "C.usefors", "E");
                boundaryDataManager.CreateNewDataTable("F", "G", "H.usefors", "J");

                var loadsDataFolderPath = Path.Combine(waqModelDataDir, "lol");
                var loadsManager = new DataTableManager { FolderPath = loadsDataFolderPath };

                var mocks = new MockRepository();
                var model = mocks.Stub<WaterQualityModel>();

                model.DataItems = new EventedList<IDataItem>();

                mocks.ReplayAll();

                var waqInitializationSettings = new WaqInitializationSettings
                {
                    InputFile = new TextDocument { Content = Resources.TestInputFile },
                    SubstanceProcessLibrary = SubstanceProcessLibraryTestHelper.CreateDefaultSubstanceProcessLibrary(),
                    SimulationStartTime = new DateTime(2010, 1, 1),
                    SimulationStopTime = new DateTime(2010, 1, 2),
                    SimulationTimeStep = new TimeSpan(1, 0, 0, 0),
                    InitialConditions = new Collection<IFunction>(new[] { WaterQualityFunctionFactory.CreateConst("AAP", 10.0, "AAP", "mg/s", "AAP") }),
                    ProcessCoefficients = new Collection<IFunction>(),
                    Dispersion = new Collection<IFunction>(new[] { WaterQualityFunctionFactory.CreateConst("Dispersion", 0.2d, "Dispersion", "m2/s", null) }),
                    Settings = new WaterQualityModelSettings { WorkDirectory = workingDirectory },
                    BoundaryNodeIds = new Dictionary<WaterQualityBoundary, int[]>(),
                    LoadAndIds = new Dictionary<WaterQualityLoad, int>(),
                    OutputLocations = new Dictionary<string, IList<int>>(),
                    BoundaryDataManager = boundaryDataManager,
                    LoadsDataManager = loadsManager, // same data for loads as for boundary data
                    LoadsAliases = new Dictionary<string, IList<string>>(),
                    BoundaryAliases = new Dictionary<string, IList<string>>(),
                };

                // call
                preprocessor.InitializeWaq(waqInitializationSettings, (displayName, filePath) => model.AddTextDocument(displayName, filePath));

                // setup
                Assert.IsTrue(Directory.Exists(includeDirectory));

                Assert.IsTrue(Directory.Exists(Path.Combine(includeDirectory, Path.GetFileName(boundaryDataManager.FolderPath))));
                Assert.IsFalse(Directory.Exists(Path.Combine(includeDirectory, Path.GetFileName(loadsManager.FolderPath))));

                mocks.VerifyAll();
            }
            finally
            {
                Directory.SetCurrentDirectory(currentDirectory);
                FileUtils.DeleteIfExists(workingDirectory);
                FileUtils.DeleteIfExists(waqModelDataDir);
            }
        }



    }
}