using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DelftTools.Hydro;
using DelftTools.Hydro.Area.Objects;
using DelftTools.Hydro.Area.Objects.StructureObjects;
using DelftTools.Hydro.Area.Objects.StructureObjects.StructureFormulas;
using DelftTools.Hydro.GroupableFeatures;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DelftTools.Utils.Validation;
using DeltaShell.Core.Services;
using DeltaShell.IntegrationTestUtils;
using DeltaShell.IntegrationTestUtils.Builders;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.HydroModel.Export;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Exporters;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Sediment;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.SharpMapGis;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Grids;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using Rhino.Mocks;
using SharpMap.Extensions.CoordinateSystems;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests
{
    [ExcludeFromCodeCoverage]
    [Category(TestCategory.DataAccess)]
    [Category(TestCategory.Slow)]
    [TestFixture]
    public class WaterFlowFmModelDirectoryStructureTest
    {
        private const string TestDataDirName = "TestPlanFM";
        private const string ModelName = "FlowFM1";
        private const string ProjectName = "Project1";

        private const string ExportDimrDirName = "exported_dimr";
        private const string ExportDimrFileName = "dimr.xml";
        private const string WindFileName = "meteo.apwxwy";
        private const string GridFileName = "meteo.grd";

        private const string ProjectFileExtension = ".dsproj";
        private const string ProjectDirExtension = ".dsproj_data";

        private const string InputDirName = "input";
        private const string OutputDirName = "output";
        private const string SnappedDirName = "snapped";
        private const string DflowfmDirName = "dflowfm";

        private const string TrachytopesModelProjectDirName = "TrachytopesModel";

        private const string customOutputFolder = "we/must/go/deeper/output";

        private static string projectDirName;
        private static string projectFileName;
        private static string modelDirName;
        private static string outputWAQDirName;

        private static string projectDirPath;
        private static string projectFilePath;
        private static string modelDirPath;
        private static string inputDirPath;
        private static string outputDirPath;
        private static string outputWAQDirPath;
        private static string snappedDirPath;

        private static string dflowfmDirPath;

        private static string testDataDirPath;
        private static string destinationDirPath;
        private static string tempDirPath;
        private static string workingDirPath;
        private static string tempMduFilePath;
        private static string exportDimrDirPath;
        private static string exportDimrFilePath;

        private static string mduFileName;

        private static List<string> filtersCommonInput;
        private static List<string> filtersInputWithTrachytopes;
        private static List<string> filtersInputWithMorphology;
        private static List<string> filtersInputWithWind;

        private static List<string> filtersOutputFM_NewModel;
        private static List<string> filtersOutputFM_OldModel;
        private static List<string> filtersOutputWAQ;
        private static List<string> filtersSnapped;

        [SetUp]
        public void Setup()
        {
            // Get TestData Directory
            testDataDirPath = TestHelper.GetTestFilePath(TestDataDirName);

            // Create work directory in Temp
            destinationDirPath = FileUtils.CreateTempDirectory();

            // Create extra directory to copy testdata to so that workdirectory is not "contaminated" with other files.
            tempDirPath = FileUtils.CreateTempDirectory();
            workingDirPath = FileUtils.CreateTempDirectory();

            // Set the rest of the expected paths
            projectFileName = ProjectName + ProjectFileExtension;
            projectDirName = ProjectName + ProjectDirExtension;
            modelDirName = ModelName;
            mduFileName = ModelName + ".mdu";
            outputWAQDirName = $"DFM_DELWAQ_{ModelName}";

            projectFilePath = Path.Combine(destinationDirPath, projectFileName);
            projectDirPath = Path.Combine(destinationDirPath, projectDirName);
            modelDirPath = Path.Combine(projectDirPath, modelDirName);
            inputDirPath = Path.Combine(modelDirPath, InputDirName);
            outputDirPath = Path.Combine(modelDirPath, OutputDirName);
            outputWAQDirPath = Path.Combine(outputDirPath, outputWAQDirName);
            snappedDirPath = Path.Combine(outputDirPath, SnappedDirName);

            exportDimrDirPath = Path.Combine(projectDirPath, ExportDimrDirName);
            exportDimrFilePath = Path.Combine(exportDimrDirPath, ExportDimrFileName);
            dflowfmDirPath = Path.Combine(exportDimrDirPath, DflowfmDirName);
            tempMduFilePath = Path.Combine(tempDirPath, mduFileName);

            filtersCommonInput = new List<string>
            {
                ".mdu",
                "_net.nc",
                ".pol",
                ".pli",
                ".pliz",
                ".xyn",
                ".ini",
                ".bc",
                ".ext",
                ".ldb"
            };

            var fourierFileFilters = new[]
            {
                ".fou",
                ".cld",
                ".cll"
            };

            filtersInputWithTrachytopes = filtersCommonInput.Union(fourierFileFilters).Union(new List<string>
            {
                ".arl",
                ".ttd"
            }).ToList();
            filtersInputWithMorphology = filtersCommonInput.Union(fourierFileFilters).Union(new List<string>
            {
                ".mor",
                ".sed"
            }).ToList();
            filtersInputWithWind = filtersCommonInput.Union(fourierFileFilters).Union(new List<string>
            {
                ".grd",
                ".apwxwy"
            }).ToList();

            filtersOutputFM_NewModel = new List<string>
            {
                "_his.nc",
                "_map.nc",
                ".dia"
            };

            filtersOutputFM_OldModel = new List<string>
            {
                "_his.nc",
                "_map.nc",
                "_clm.nc",
                ".dia",
                ".out",
                "_numlimdt.xyz"
            };

            filtersOutputWAQ = new List<string>
            {
                ".are",
                ".atr",
                ".bnd",
                ".flo",
                ".hyd",
                ".len",
                ".poi",
                ".srf",
                ".srfold",
                ".tau",
                ".vol",
                "_waqgeom.nc",
                ".sal",
                ".tem"
            };

            filtersSnapped = new List<string>
            {
                ".shp",
                ".dbf",
                ".shx"
            };
        }

        [TearDown]
        public void TearDown()
        {
            DeleteTestDirectories();
        }

        [Test]
        //1.1
        public void GivenAnFMModelWithTrachytopes_WhenProjectSavedAs_ThenInputFolderWithCorrectFilesAreGiven()
        {
            CreateTestDirectories();
            CopyFourierAndCalibrationFilesToTemp();
            CopyTrachytopeFilesToTemp();

            using (var app = GetConfiguredApplication())
            {
                using (var model = new WaterFlowFMModel())
                {
                    AddFeaturesToModel(model);
                    EnableSalinityAndTemperature(model);
                    SimulateUserAddingTrachytopesInMduFile(model);
                    model.ExportTo(tempMduFilePath);
                    model.ReloadGrid(true, true);
                }

                SimulateUserAddingReferencesInMduFile();

                using (var model = new WaterFlowFMModel())
                {
                    model.ImportFromMdu(tempMduFilePath);

                    AdjustSettingsOutputParameters(model);
                    UpdateBedLevel(model);
                    AddModelToProject(model, app);

                    app.SaveProjectAs(projectFilePath);

                    app.CloseProject();

                    AssertProjectFileAndFolderExist();
                    AssertModelDirectoryExists();
                    AssertInputDirectoryExists();
                    AssertFilesExtensionsExistInDirectory(filtersInputWithTrachytopes, inputDirPath);
                }
            }
        }

        [Test]
        //1.2
        public void GivenAnFMModelWithMorphology_WhenProjectSavedAs_ThenInputFolderWithCorrectFilesAreGiven()
        {
            CreateTestDirectories();
            CopyFourierAndCalibrationFilesToTemp();

            using (var app = GetConfiguredApplication())
            {
                using (var model = new WaterFlowFMModel())
                {
                    AddFeaturesToModel(model);
                    EnableSalinityAndTemperature(model);
                    EnableMorphology(model);
                    AddMorphologyBoundaryConditionToModel(model);
                    AddSedimentFraction(model);
                    model.ExportTo(tempMduFilePath);
                    model.ReloadGrid(true, true);
                }

                SimulateUserAddingReferencesInMduFile();

                using (var model = new WaterFlowFMModel())
                {
                    model.ImportFromMdu(tempMduFilePath);

                    AdjustSettingsOutputParameters(model);
                    UpdateBedLevel(model);
                    AddModelToProject(model, app);

                    app.SaveProjectAs(projectFilePath);

                    app.CloseProject();

                    AssertProjectFileAndFolderExist();
                    AssertModelDirectoryExists();
                    AssertInputDirectoryExists();
                    AssertFilesExtensionsExistInDirectory(filtersInputWithMorphology, inputDirPath);
                }
            }
        }

        [Test]
        //1.3
        public void GivenAnFMModelWithWind_WhenProjectSavedAs_ThenInputFolderWithCorrectFilesAreGiven()
        {
            CreateTestDirectories();
            CopyFourierAndCalibrationFilesToTemp();
            CopyWindFilesToTemp();

            using (var app = GetConfiguredApplication())
            {
                using (var model = new WaterFlowFMModel())
                {
                    AddFeaturesToModel(model);
                    AddWindToModel(model);
                    EnableSalinityAndTemperature(model);
                    model.ExportTo(tempMduFilePath);
                    model.ReloadGrid(true, true);
                }

                SimulateUserAddingReferencesInMduFile();

                using (var model = new WaterFlowFMModel())
                {
                    model.ImportFromMdu(tempMduFilePath);

                    AdjustSettingsOutputParameters(model);

                    UpdateBedLevel(model);
                    AddModelToProject(model, app);

                    app.SaveProjectAs(projectFilePath);

                    app.CloseProject();

                    AssertProjectFileAndFolderExist();
                    AssertModelDirectoryExists();
                    AssertInputDirectoryExists();
                    AssertFilesExtensionsExistInDirectory(filtersInputWithWind, inputDirPath);
                }
            }
        }

        [Test]
        //2.1
        public void GivenAnFMModelWithTrachytopes_WhenRun_ThenWorkDirIsCreatedInTemp()
        {
            CreateTestDirectories();
            CopyFourierAndCalibrationFilesToTemp();
            CopyTrachytopeFilesToTemp();

            using (var app = GetConfiguredApplication())
            {
                using (var model = new WaterFlowFMModel())
                {
                    AddFeaturesToModel(model);
                    EnableSalinityAndTemperature(model);
                    SimulateUserAddingTrachytopesInMduFile(model);
                    model.ExportTo(tempMduFilePath);
                    model.ReloadGrid(true, true);
                }

                SimulateUserAddingReferencesInMduFile();

                using (var model = new WaterFlowFMModel())
                {
                    model.ImportFromMdu(tempMduFilePath);

                    AdjustSettingsOutputParameters(model);
                    UpdateBedLevel(model);
                    AddModelToProject(model, app);

                    app.SaveProjectAs(projectFilePath);

                    ValidateAndRunModel(model, app);

                    app.CloseProject();

                    var workDirInfo = new DirectoryInfo(model.WorkingDirectoryPath);
                    Assert.That(workDirInfo.Exists, "The working directory does not exist.");

                    string retrievedTempFolderPathFromWorkingDirectory = workDirInfo.Parent?.Parent?.FullName;
                    Assert.NotNull(retrievedTempFolderPathFromWorkingDirectory, "Retrieved temp folder path is null");

                    string tempFolderPathFullName = Path.GetTempPath();
                    Assert.That(retrievedTempFolderPathFromWorkingDirectory, Is.SamePath(tempFolderPathFullName), "The working directory is not created in Temp");

                    AssertWorkingOutputDirectoryWithSubFoldersExists(model.WorkingOutputDirectoryPath);
                }
            }
        }

        [Test]
        //2.2
        public void GivenAnFMModelWithMorphology_WhenRun_ThenWorkDirIsCreatedInTemp()
        {
            CreateTestDirectories();
            CopyFourierAndCalibrationFilesToTemp();

            using (var app = GetConfiguredApplication())
            {
                using (var model = new WaterFlowFMModel())
                {
                    AddFeaturesToModel(model);
                    EnableSalinityAndTemperature(model);
                    EnableMorphology(model);
                    AddMorphologyBoundaryConditionToModel(model);
                    AddSedimentFraction(model);
                    model.ExportTo(tempMduFilePath);
                    model.ReloadGrid(true, true);
                }

                SimulateUserAddingReferencesInMduFile();

                using (var model = new WaterFlowFMModel())
                {
                    model.ImportFromMdu(tempMduFilePath);

                    AdjustSettingsOutputParameters(model);
                    UpdateBedLevel(model);
                    AddModelToProject(model, app);

                    app.SaveProjectAs(projectFilePath);

                    ValidateAndRunModel(model, app);

                    app.CloseProject();

                    var workDirInfo = new DirectoryInfo(model.WorkingDirectoryPath);
                    Assert.That(workDirInfo.Exists, "The working directory does not exist.");

                    string retrievedTempFolderPathFromWorkingDirectory = workDirInfo.Parent?.Parent?.FullName;
                    Assert.NotNull(retrievedTempFolderPathFromWorkingDirectory, "Retrieved temp folder path is null");

                    string tempFolderPathFullName = Path.GetTempPath();
                    Assert.That(retrievedTempFolderPathFromWorkingDirectory, Is.SamePath(tempFolderPathFullName), "The working directory is not created in Temp");

                    AssertWorkingOutputDirectoryWithSubFoldersExists(model.WorkingOutputDirectoryPath);
                }
            }
        }

        [Test]
        //2.3
        public void GivenAnFMModelWithWind_WhenRun_ThenWorkDirIsCreatedInTemp()
        {
            CreateTestDirectories();
            CopyFourierAndCalibrationFilesToTemp();
            CopyWindFilesToTemp();

            using (var app = GetConfiguredApplication())
            {
                using (var model = new WaterFlowFMModel())
                {
                    AddFeaturesToModel(model);
                    AddWindToModel(model);
                    EnableSalinityAndTemperature(model);
                    model.ExportTo(tempMduFilePath);
                    model.ReloadGrid(true, true);
                }

                SimulateUserAddingReferencesInMduFile();

                using (var model = new WaterFlowFMModel())
                {
                    model.ImportFromMdu(tempMduFilePath);

                    AdjustSettingsOutputParameters(model);
                    UpdateBedLevel(model);
                    AddModelToProject(model, app);

                    app.SaveProjectAs(projectFilePath);

                    ValidateAndRunModel(model, app);

                    app.CloseProject();

                    var workDirInfo = new DirectoryInfo(model.WorkingDirectoryPath);
                    Assert.That(workDirInfo.Exists, "The working directory does not exist.");

                    string retrievedTempFolderPathFromWorkingDirectory = workDirInfo.Parent?.Parent?.FullName;
                    Assert.NotNull(retrievedTempFolderPathFromWorkingDirectory, "Retrieved temp folder path is null");

                    string tempFolderPathFullName = Path.GetTempPath();
                    Assert.That(retrievedTempFolderPathFromWorkingDirectory, Is.SamePath(tempFolderPathFullName), "The working directory is not created in Temp");

                    AssertWorkingOutputDirectoryWithSubFoldersExists(model.WorkingOutputDirectoryPath);
                }
            }
        }

        [Test]
        //2.4
        public void GivenAnFMModelWithWind_WhenRunAndSaved_ThenAllOutputFromWorkingDirIsCopiedToPersistentDirWhileMaintainingTheStructure()
        {
            CreateTestDirectories();
            CopyFourierAndCalibrationFilesToTemp();
            CopyWindFilesToTemp();

            using (var app = GetConfiguredApplication())
            {
                using (var model = new WaterFlowFMModel())
                {
                    AddFeaturesToModel(model);
                    AddWindToModel(model);
                    EnableSalinityAndTemperature(model);
                    model.ExportTo(tempMduFilePath);
                    model.ReloadGrid(true, true);
                }

                SimulateUserAddingReferencesInMduFile();

                using (var model = new WaterFlowFMModel())
                {
                    model.ImportFromMdu(tempMduFilePath);

                    AdjustSettingsOutputParameters(model);
                    UpdateBedLevel(model);
                    AddModelToProject(model, app);

                    app.SaveProjectAs(projectFilePath);

                    ValidateAndRunModel(model, app);

                    string workingDirectoryPath = model.WorkingDirectoryPath;
                    string workingOutputDirectoryPath = model.WorkingOutputDirectoryPath;

                    Dictionary<string, Tuple<Tuple<string, string>[], string[]>> outputWorkingDirStructure =
                        GetDirectoryStructure(workingOutputDirectoryPath, ".", true);

                    AssertWorkingDirectoryWithOutputExists(workingDirectoryPath, workingOutputDirectoryPath);

                    app.SaveProject();

                    AssertWorkingDirectoryIsCleaned(workingDirectoryPath, workingOutputDirectoryPath);

                    app.CloseProject();

                    Dictionary<string, Tuple<Tuple<string, string>[], string[]>> outputPersistentDirStructure = GetDirectoryStructure(outputDirPath, ".", true);
                    AssertEqualDirectoryStructure(".", ref outputWorkingDirStructure,
                                                  ref outputPersistentDirStructure, true);
                }
            }
        }

        [Test]
        //3.1
        public void GivenAnFMModelWithTrachytopes_WhenDimrExported_ThenCorrectFoldersAndFilesAreGiven()
        {
            var expectedExtension = ".xml";

            CreateTestDirectories();
            CopyFourierAndCalibrationFilesToTemp();
            CopyTrachytopeFilesToTemp();

            using (var app = GetConfiguredApplication())
            {
                using (var model = new WaterFlowFMModel())
                {
                    AddFeaturesToModel(model);
                    EnableSalinityAndTemperature(model);
                    SimulateUserAddingTrachytopesInMduFile(model);
                    model.ExportTo(tempMduFilePath);
                    model.ReloadGrid(true, true);
                }

                SimulateUserAddingReferencesInMduFile();

                using (var model = new WaterFlowFMModel())
                {
                    model.ImportFromMdu(tempMduFilePath);

                    AdjustSettingsOutputParameters(model);
                    UpdateBedLevel(model);
                    AddModelToProject(model, app);

                    app.SaveProjectAs(projectFilePath);

                    AssertProjectFileAndFolderExist();

                    FileUtils.CreateDirectoryIfNotExists(exportDimrDirPath);
                    Assert.IsTrue(Directory.Exists(exportDimrDirPath));

                    DHydroConfigXmlExporter exporter = GetDimrFileExporter();
                    exporter.Export(model, exportDimrFilePath);

                    app.CloseProject();

                    Assert.IsTrue(File.Exists(exportDimrFilePath));
                    var expectedFileCount = 1;
                    int actualFileCount = Directory.GetFiles(exportDimrDirPath, $"*{expectedExtension}").Length;
                    Assert.AreEqual(expectedFileCount, actualFileCount,
                                    Message_WrongNumberOfFilesOrFolders(expectedFileCount, "folders", expectedExtension,
                                                                        actualFileCount, exportDimrDirPath));
                    AssertDflowfmDirectoryExists();
                    AssertFilesExtensionsExistInDirectory(filtersInputWithTrachytopes,
                                                          dflowfmDirPath);
                }
            }
        }

        [Test]
        //3.2
        public void GivenAnFMModelWithMorphology_WhenDimrExported_ThenCorrectFoldersAndFilesAreGiven()
        {
            const string expectedExtension = ".xml";

            CreateTestDirectories();
            CopyFourierAndCalibrationFilesToTemp();

            using (var app = GetConfiguredApplication())
            {
                using (var model = new WaterFlowFMModel())
                {
                    AddFeaturesToModel(model);
                    EnableSalinityAndTemperature(model);
                    EnableMorphology(model);
                    AddMorphologyBoundaryConditionToModel(model);
                    AddSedimentFraction(model);
                    model.ExportTo(tempMduFilePath);
                    model.ReloadGrid(true, true);
                }

                SimulateUserAddingReferencesInMduFile();

                using (var model = new WaterFlowFMModel())
                {
                    model.ImportFromMdu(tempMduFilePath);

                    AdjustSettingsOutputParameters(model);
                    UpdateBedLevel(model);
                    AddModelToProject(model, app);

                    app.SaveProjectAs(projectFilePath);

                    AssertProjectFileAndFolderExist();

                    FileUtils.CreateDirectoryIfNotExists(exportDimrDirPath);
                    Assert.IsTrue(Directory.Exists(exportDimrDirPath));

                    DHydroConfigXmlExporter exporter = GetDimrFileExporter();
                    exporter.Export(model, exportDimrFilePath);

                    app.CloseProject();

                    Assert.IsTrue(File.Exists(exportDimrFilePath));
                    const int expectedFileCount = 1;
                    int actualFileCount = Directory.GetFiles(exportDimrDirPath, $"*{expectedExtension}").Length;
                    Assert.AreEqual(expectedFileCount, actualFileCount,
                                    Message_WrongNumberOfFilesOrFolders(expectedFileCount, "folders", expectedExtension,
                                                                        actualFileCount, exportDimrDirPath));
                    AssertDflowfmDirectoryExists();
                    AssertFilesExtensionsExistInDirectory(filtersInputWithMorphology,
                                                          dflowfmDirPath);
                }
            }
        }

        [Test]
        //3.3
        public void GivenAnFMModelWithWind_WhenDimrExported_ThenCorrectFoldersAndFilesAreGiven()
        {
            var expectedExtension = ".xml";

            CreateTestDirectories();
            CopyFourierAndCalibrationFilesToTemp();
            CopyWindFilesToTemp();

            using (var app = GetConfiguredApplication())
            {
                using (var model = new WaterFlowFMModel())
                {
                    AddFeaturesToModel(model);
                    AddWindToModel(model);
                    EnableSalinityAndTemperature(model);
                    model.ExportTo(tempMduFilePath);
                    model.ReloadGrid(true, true);
                }

                SimulateUserAddingReferencesInMduFile();

                using (var model = new WaterFlowFMModel())
                {
                    model.ImportFromMdu(tempMduFilePath);

                    AdjustSettingsOutputParameters(model);
                    UpdateBedLevel(model);
                    AddModelToProject(model, app);

                    app.SaveProjectAs(projectFilePath);

                    AssertProjectFileAndFolderExist();

                    FileUtils.CreateDirectoryIfNotExists(exportDimrDirPath);
                    Assert.IsTrue(Directory.Exists(exportDimrDirPath));

                    DHydroConfigXmlExporter exporter = GetDimrFileExporter();
                    exporter.Export(model, exportDimrFilePath);

                    Assert.IsTrue(File.Exists(exportDimrFilePath));
                    var expectedFileCount = 1;
                    int actualFileCount = Directory.GetFiles(exportDimrDirPath, $"*{expectedExtension}").Length;
                    Assert.AreEqual(expectedFileCount, actualFileCount,
                                    Message_WrongNumberOfFilesOrFolders(expectedFileCount, "folders", expectedExtension,
                                                                        actualFileCount, exportDimrDirPath));
                    AssertDflowfmDirectoryExists();
                    AssertFilesExtensionsExistInDirectory(filtersInputWithWind,
                                                          dflowfmDirPath);

                    app.CloseProject();
                }
            }
        }

        [Test]
        //4.1
        public void GivenAnFMModelWithTrachytopesThatRunsAndSaves_WhenUserClearsOutputAndSaves_ThenOutputFolderShouldBePresentButEmpty()
        {
            CreateTestDirectories();
            CopyFourierAndCalibrationFilesToTemp();
            CopyTrachytopeFilesToTemp();

            using (var app = GetConfiguredApplication())
            {
                using (var model = new WaterFlowFMModel())
                {
                    AddFeaturesToModel(model);
                    EnableSalinityAndTemperature(model);
                    SimulateUserAddingTrachytopesInMduFile(model);
                    model.ExportTo(tempMduFilePath);
                    model.ReloadGrid(true, true);
                }

                SimulateUserAddingReferencesInMduFile();

                using (var model = new WaterFlowFMModel())
                {
                    model.ImportFromMdu(tempMduFilePath);

                    AdjustSettingsOutputParameters(model);
                    UpdateBedLevel(model);
                    AddModelToProject(model, app);

                    app.SaveProjectAs(projectFilePath);

                    ValidateAndRunModel(model, app);

                    app.SaveProject();

                    model.ClearOutput();

                    Assert.IsFalse(FileUtils.IsDirectoryEmpty(outputDirPath),
                                   $"Expected: Output folder '{outputDirPath}' should not be emptied after ClearModelOutput.");

                    app.SaveProject();

                    app.CloseProject();

                    AssertProjectFileAndFolderExist();
                    AssertModelDirectoryExists();
                    AssertOutputDirectoryExists();
                    AssertOutputDirectoryIsEmpty();
                }
            }
        }

        [Test]
        // 7
        public void GivenAnFMModelWithTrachytopesAfterARun_WhenSavingItAndRenamingItAndSavingItAgain_OnlyFolderNameOfModelIsChangedAndTheMDUFileName()
        {
            CreateTestDirectories();
            CopyFourierAndCalibrationFilesToTemp();
            CopyTrachytopeFilesToTemp();

            using (var app = GetConfiguredApplication())
            {
                using (var model = new WaterFlowFMModel())
                {
                    AddFeaturesToModel(model);
                    EnableSalinityAndTemperature(model);
                    SimulateUserAddingTrachytopesInMduFile(model);
                    model.ExportTo(tempMduFilePath);
                    model.ReloadGrid(true, true);
                }

                SimulateUserAddingReferencesInMduFile();

                using (var model = new WaterFlowFMModel())
                {
                    model.ImportFromMdu(tempMduFilePath);

                    AdjustSettingsOutputParameters(model);
                    UpdateBedLevel(model);
                    AddModelToProject(model, app);

                    app.SaveProjectAs(projectFilePath);

                    model.ValidateBeforeRun = true;
                    ValidationReport report = model.Validate();
                    Assert.AreEqual(0, report.AllErrors.Count(),
                                    "There are errors in the model after importing the MDU file");
                    app.RunActivity(model);
                    Assert.AreEqual(ActivityStatus.Cleaned, model.Status);

                    app.SaveProject();

                    Dictionary<string, Tuple<Tuple<string, string>[], string[]>> projectFolderBeforeRename =
                        GetDirectoryStructure(
                            Path.Combine(projectFilePath, modelDirPath),
                            ".",
                            false,
                            ".mdu",
                            ".cache");

                    //Rename
                    model.Name = "FlowFM2";

                    app.SaveProject();

                    app.CloseProject();

                    AssertProjectFileAndFolderExist();

                    //MDU file name check
                    string newModelDirPath = Path.Combine(projectDirPath, "FlowFM2");
                    Assert.IsTrue(Directory.Exists(newModelDirPath),
                                  Message_MissingFileOrFolderName("folder", "FlowFM2", projectDirName));
                    Assert.IsFalse(Directory.Exists(modelDirPath),
                                   Message_MissingFileOrFolderName("folder", "FlowFM2", projectDirName));
                    string newInputDirPath = Path.Combine(newModelDirPath, InputDirName);
                    string newMduFileNameWithoutExtension =
                        Path.GetFileNameWithoutExtension(Directory.GetFiles(newInputDirPath, $"*{".mdu"}")[0]);
                    Assert.AreEqual("FlowFM2", newMduFileNameWithoutExtension);

                    Dictionary<string, Tuple<Tuple<string, string>[], string[]>> projectFolderAfterRename =
                        GetDirectoryStructure(Path.Combine(projectFilePath, newModelDirPath),
                                              ".",
                                              false,
                                              ".mdu",
                                              ".cache");
                    AssertEqualDirectoryStructure(".", ref projectFolderBeforeRename, ref projectFolderAfterRename);
                }
            }
        }

        [Test]
        //8.1
        public void GivenAnFMModelWithTrachytopes_WhenRunAndProjectSavedAsInAnotherDirectory_ThenInputFolderWithCorrectFilesAreGiven()
        {
            CreateTestDirectories();
            CopyFourierAndCalibrationFilesToTemp();
            CopyTrachytopeFilesToTemp();

            using (var app = GetConfiguredApplication())
            {
                using (var model = new WaterFlowFMModel())
                {
                    AddFeaturesToModel(model);
                    EnableSalinityAndTemperature(model);
                    SimulateUserAddingTrachytopesInMduFile(model);
                    model.ExportTo(tempMduFilePath);
                    model.ReloadGrid(true, true);
                }

                SimulateUserAddingReferencesInMduFile();

                using (var model = new WaterFlowFMModel())
                {
                    model.ImportFromMdu(tempMduFilePath);

                    AdjustSettingsOutputParameters(model);
                    UpdateBedLevel(model);
                    AddModelToProject(model, app);

                    app.SaveProjectAs(projectFilePath);

                    model.ValidateBeforeRun = true;
                    ValidationReport report = model.Validate();
                    Assert.AreEqual(0, report.AllErrors.Count(),
                                    "There are errors in the model after importing the MDU file");
                    app.RunActivity(model);
                    Assert.AreEqual(ActivityStatus.Cleaned, model.Status);

                    app.SaveProject();

                    AssertProjectFileAndFolderExist();
                    AssertModelDirectoryExists();
                    AssertInputDirectoryExists();

                    Dictionary<string, Tuple<Tuple<string, string>[], string[]>> projectDirStructureBeforeSaveAs =
                        GetDirectoryStructure(projectDirPath, ".", true);

                    string newSaveAsDestinationDirPath = Path.Combine(Path.GetTempPath(),
                                                                      Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
                    string newSaveAsProjectFilePath = Path.Combine(newSaveAsDestinationDirPath, projectFileName);

                    app.SaveProjectAs(newSaveAsProjectFilePath);

                    app.CloseProject();
                    Dictionary<string, Tuple<Tuple<string, string>[], string[]>> projectDirStructureAfterSaveAs =
                        GetDirectoryStructure(projectDirPath, ".", true);
                    Dictionary<string, Tuple<Tuple<string, string>[], string[]>> newDirStructureAfterSaveAs = GetDirectoryStructure(
                        Path.Combine(newSaveAsDestinationDirPath, ProjectName + ProjectDirExtension), ".",
                        true);

                    // Since we asserted that everything is correct before we started, if the structure is the same, it should still be correct after.
                    AssertEqualDirectoryStructure(".", ref projectDirStructureBeforeSaveAs,
                                                  ref projectDirStructureAfterSaveAs, true);
                    AssertEqualDirectoryStructure(".", ref projectDirStructureAfterSaveAs,
                                                  ref newDirStructureAfterSaveAs, true);
                }
            }
        }

        [Test]
        public void GivenAnIntegratedModelWithAnFMModel_WhenRunAndSaved_ThenAllOutputFromWorkingDirIsCopiedToPersistentDirWhileMaintainingTheStructure()
        {
            FileUtils.CreateDirectoryIfNotExists(destinationDirPath);

            try
            {
                CopyProjectToDestinationDir("IntegratedModel");

                using (var app = GetConfiguredHydroApplication())
                {
                    app.OpenProject(projectFilePath);
                    // Execute SaveAs() manually (migrating through GUI does this already).
                    app.SaveProjectAs(projectFilePath);

                    var integratedModel = app.GetAllModelsInProject().FirstOrDefault(m => m is HydroModel) as HydroModel;
                    Assert.NotNull(integratedModel, "Expected: one integrated model (hydromodel) in the project.");
                    var fmModel = (WaterFlowFMModel) app.GetAllModelsInProject().FirstOrDefault(m => m is WaterFlowFMModel);
                    Assert.NotNull(fmModel, "Expected: one waterflow fm model in the project.");

                    app.RunActivity(integratedModel);

                    // Get directory structure of output in working directory before saving
                    string outputWorkingDirectoryPath = Path.Combine(integratedModel.WorkingDirectoryPath,
                                                                     DflowfmDirName, OutputDirName);
                    Dictionary<string, Tuple<Tuple<string, string>[], string[]>> outputWorkingDirStructure = GetDirectoryStructure(outputWorkingDirectoryPath, ".", true);

                    Assert.IsTrue(Directory.Exists(outputWorkingDirectoryPath),
                                  $"Expected: path '{outputWorkingDirectoryPath}' should exist.");
                    Assert.IsTrue(Directory.GetFileSystemEntries(outputWorkingDirectoryPath).Any(),
                                  $"Expected: Directory '{outputWorkingDirectoryPath}' should not be empty.");

                    AssertCorrectOutputIsLinked(outputWorkingDirectoryPath, fmModel);

                    app.SaveProject();

                    AssertCorrectOutputIsLinked(outputDirPath, fmModel);

                    app.CloseProject();

                    AssertProjectFileAndFolderExist();
                    AssertModelDirectoryExists();
                    AssertInputDirectoryExists();
                    AssertOutputDirectoryExists();

                    // Get directory structure of output in persistent directory after saving
                    Dictionary<string, Tuple<Tuple<string, string>[], string[]>> outputPersistentDirStructure = GetDirectoryStructure(outputDirPath, ".", true);

                    // Assert that same directory structure is maintained after saving
                    AssertEqualDirectoryStructure(".", ref outputWorkingDirStructure,
                                                  ref outputPersistentDirStructure, true);
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(destinationDirPath);
            }
        }

        /// <summary>
        /// GIVEN an exported WaterFlowFMModel
        /// AND a project
        /// WHEN the exported model is imported in to the project
        /// THEN a new model is created in the project folder
        /// AND the input files are copied into this project folder
        /// AND no output files are copied
        /// AND the import location does not change
        /// </summary>
        [Test]
        public void GivenAnExportedWaterFlowFMModelAndAProject_WhenTheExportedModelIsImportedInToTheProject_ThenANewModelIsCreatedInTheProjectWithTheCorrectStructure()
        {
            CreateTestDirectories();
            string exportPath = Path.Combine(destinationDirPath, "exportPath");
            FileUtils.CreateDirectoryIfNotExists(exportPath);

            string newSavePath = Path.Combine(destinationDirPath, "newSaveLocation");
            FileUtils.CreateDirectoryIfNotExists(newSavePath);

            CopyProjectToDestinationDir("TestModel");

            using (var app = GetConfiguredApplication())
            {
                // Given
                CreateExportedFmModel(app, exportPath, out WaterFlowFMModel fmModel);
                string exportFilePath = Path.Combine(exportPath,
                                                     Path.GetFileName(fmModel.MduFilePath));

                Dictionary<string, Tuple<Tuple<string, string>[], string[]>> preImportExportDirStructure = GetDirectoryStructure(exportPath, ".", true);

                string originalInputFolder = Path.Combine(projectDirPath, fmModel.Name, "input");
                Dictionary<string, Tuple<Tuple<string, string>[], string[]>> originalInputDirStructure = GetDirectoryStructure(originalInputFolder, ".");

                // When
                app.CreateNewProject();
                IFileImporter relevantImporter = app.FileImporters
                                                    .FirstOrDefault(importer => importer is WaterFlowFMFileImporter);

                var importedModel = relevantImporter.ImportItem(exportFilePath) as WaterFlowFMModel;
                Assert.That(importedModel, Is.Not.Null,
                            "Expected the imported model to exist.");
                AddModelToProject(importedModel, app);

                string newSaveFilePath = Path.Combine(newSavePath, "Project1.dsproj");
                app.SaveProjectAs(newSaveFilePath);

                // Then
                // The import location does not change.
                Dictionary<string, Tuple<Tuple<string, string>[], string[]>> postImportExportDirStructure = GetDirectoryStructure(exportPath, ".", true);
                AssertEqualDirectoryStructure(".",
                                              ref preImportExportDirStructure,
                                              ref postImportExportDirStructure,
                                              true);

                // a new model is created in the project folder
                string newDsProjData = Path.Combine(newSavePath, "Project1.dsproj_data");
                List<string> projectDirSubFolders =
                    Directory.EnumerateDirectories(newDsProjData).ToList();

                Assert.That(projectDirSubFolders.Count, Is.EqualTo(1),
                            $"Expected one folders in {newDsProjData}: '{ModelName}'");

                string modelPath = Path.Combine(newDsProjData, fmModel.Name);
                Assert.That(projectDirSubFolders.First(), Is.EqualTo(modelPath),
                            "Expected a different model folder name.");

                AssertInputImportedModelIsCorrect(modelPath,
                                                  originalInputDirStructure);

                importedModel.Dispose();
                fmModel.Dispose();
            }
        }

        /// <summary>
        /// GIVEN an exported WaterFlowFMModel
        /// AND an Integrated Model
        /// WHEN the exported model is imported in to the Integrated Model
        /// THEN a new model is created in the integrated model
        /// AND the input files are copied into this project folder
        /// AND no output files are copied
        /// AND the import location does not change
        /// </summary>
        [Test]
        public void GivenAnExportedWaterFlowFMModelAndAnIntegratedModel_WhenTheExportedModelIsImportedInToTheIntegratedModel_ThenANewModelIsCreatedInTheIntegratedModelWithTheCorrectStructure()
        {
            CreateTestDirectories();
            string exportPath = Path.Combine(destinationDirPath, "exportPath");
            FileUtils.CreateDirectoryIfNotExists(exportPath);

            string newSavePath = Path.Combine(destinationDirPath, "newSaveLocation");
            FileUtils.CreateDirectoryIfNotExists(newSavePath);

            CopyProjectToDestinationDir("TestModel");

            using (var app = GetConfiguredApplication())
            {
                // Given
                CreateExportedFmModel(app, exportPath, out WaterFlowFMModel fmModel);
                string exportFilePath = Path.Combine(exportPath,
                                                     Path.GetFileName(fmModel.MduFilePath));

                Dictionary<string, Tuple<Tuple<string, string>[], string[]>> preImportExportDirStructure = GetDirectoryStructure(exportPath, ".", true);

                string originalInputFolder = Path.Combine(projectDirPath, fmModel.Name, "input");
                Dictionary<string, Tuple<Tuple<string, string>[], string[]>> originalInputDirStructure = GetDirectoryStructure(originalInputFolder,
                                                                                                                               ".",
                                                                                                                               ignoreFileExtension: "meta"); // The restart.meta file is ignored, as it should not be present in the HydroModel.

                // When
                app.CreateNewProject();
                var newIntegratedModel = new HydroModel() {Name = "Blastoise"};

                AddModelToProject(newIntegratedModel, app);

                IFileImporter relevantImporter = app.FileImporters
                                                    .FirstOrDefault(importer => importer is WaterFlowFMFileImporter);

                relevantImporter.ImportItem(exportFilePath, newIntegratedModel);

                string newSaveFilePath = Path.Combine(newSavePath, "Project1.dsproj");
                app.SaveProjectAs(newSaveFilePath);

                // Then
                // A new model is created in the integrated model
                List<IActivity> integratedModelActivities = newIntegratedModel.Activities.ToList();
                Assert.That(integratedModelActivities.Count, Is.EqualTo(1), "Expected the integrated model to contain a single activity.");
                Assert.That(integratedModelActivities.First().Name, Is.EqualTo(fmModel.Name), "Expected the imported model name to be equal to the exported model name.");

                // The import location does not change.
                Dictionary<string, Tuple<Tuple<string, string>[], string[]>> postImportExportDirStructure = GetDirectoryStructure(exportPath, ".", true);
                AssertEqualDirectoryStructure(".",
                                              ref preImportExportDirStructure,
                                              ref postImportExportDirStructure,
                                              true);

                // a new model is created in the project folder
                string newDsProjData = Path.Combine(newSavePath, "Project1.dsproj_data");
                string fmModelPath = Path.Combine(newDsProjData, fmModel.Name);
                string hydroModelPath = Path.Combine(newDsProjData, newIntegratedModel.Name);

                Assert.That(Directory.Exists(newDsProjData), "Expected the dsproj_data folder to exist.");
                Assert.That(Directory.Exists(fmModelPath), "Expected the D-FlowFM model folder to exist.");
                Assert.That(Directory.Exists(hydroModelPath), "Expected the Integrated Model folder to exist.");

                foreach (string subDirectory in Directory.EnumerateDirectories(newDsProjData))
                {
                    string dirName = new DirectoryInfo(subDirectory).Name;
                    Assert.That(dirName.StartsWith(fmModel.Name) ||
                                dirName.StartsWith(newIntegratedModel.Name),
                                "Expected only folders corresponding with the model names.");
                }

                Assert.That(Directory.EnumerateFiles(newDsProjData).Any(), Is.False,
                            $"Expected no files in {newDsProjData}");
                Assert.That(Directory.EnumerateFileSystemEntries(hydroModelPath).Any(), Is.False,
                            "Expected the integrated model to be empty.");

                AssertInputImportedModelIsCorrect(fmModelPath,
                                                  originalInputDirStructure);
            }
        }

        /// <summary>
        /// GIVEN an old style model with output without an OutputDir set
        /// AND a project
        /// WHEN this model is imported into this project
        /// THEN the input files are copied correctly
        /// AND the output is handled correctly
        /// AND the import location does not change
        /// </summary>
        [Test]
        public void GivenAnOldModelWithOutputAndWithoutOutputDirSet_WhenThisModelIsImported_ThenTheInputAndOutputAreHandledCorrectly()
        {
            CreateTestDirectories();
            CopyProjectToDestinationDir("TestModel");

            using (var app = GetConfiguredApplication())
            {
                // Given
                string modelDirImport = Path.Combine(destinationDirPath, "Project1.dsproj_data", "FlowFM1");
                List<string> importInputFiles = Directory.EnumerateFiles(modelDirImport).ToList();

                string mduPath = Path.Combine(modelDirImport, "FlowFM1.mdu");

                Dictionary<string, Tuple<Tuple<string, string>[], string[]>> preImportDirStructure = GetDirectoryStructure(destinationDirPath,
                                                                                                                           ".",
                                                                                                                           true);

                // When
                string dsprojSave = Path.Combine(tempDirPath, "Project1.dsproj");
                WaterFlowFMModel importedModel = ImportModelIntoProject(app, mduPath, dsprojSave);

                // Then
                Dictionary<string, Tuple<Tuple<string, string>[], string[]>> postImportDirStructure = GetDirectoryStructure(destinationDirPath,
                                                                                                                            ".",
                                                                                                                            true);
                AssertEqualDirectoryStructure(".",
                                              ref preImportDirStructure,
                                              ref postImportDirStructure,
                                              true);

                string modelDirSave = Path.Combine(dsprojSave + "_data", importedModel.Name);
                Assert.That(Directory.Exists(modelDirSave), Is.True, "Expected a model directory, but found none.");
                string inputDirSave = Path.Combine(modelDirSave, "input");
                Assert.That(Directory.Exists(inputDirSave), Is.True, "Expected an input directory, but found none.");

                IEnumerable<string> saveInputFiles = Directory.EnumerateFiles(inputDirSave);
                AssertThatInputFilesAreEqual(importInputFiles, saveInputFiles);

                AssertThatOutputNotExists(modelDirSave, importedModel);
            }
        }

        /// <summary>
        /// GIVEN an old style model with output and with an OutputDir set
        /// AND a project
        /// WHEN this model is imported into this project
        /// THEN the input files are copied correctly
        /// AND the output is handled correctly
        /// AND the import location does not change
        /// </summary>
        [Test]
        public void GivenAnOldModelWithOutputAndWithOutputDirSet_WhenThisModelIsImported_ThenTheInputAndOutputAreHandledCorrectly()
        {
            CreateTestDirectories();
            CopyProjectToDestinationDir("TestModel");

            using (var app = GetConfiguredApplication())
            {
                // Given
                string modelDirImport = Path.Combine(destinationDirPath, "Project1.dsproj_data", "FlowFM1");
                List<string> importInputFiles = Directory.EnumerateFiles(modelDirImport).ToList();

                const string importOutputFolder = "DFM_OUTPUT_FlowFM1";

                string mduPath = Path.Combine(modelDirImport, "FlowFM1.mdu");
                UpdateOutputDirInMDUTo(mduPath, importOutputFolder);

                Dictionary<string, Tuple<Tuple<string, string>[], string[]>> preImportDirStructure = GetDirectoryStructure(destinationDirPath,
                                                                                                                           ".",
                                                                                                                           true);

                // When
                string dsprojSave = Path.Combine(tempDirPath, "Project1.dsproj");
                WaterFlowFMModel importedModel = ImportModelIntoProject(app, mduPath, dsprojSave);

                // Then
                Dictionary<string, Tuple<Tuple<string, string>[], string[]>> postImportDirStructure = GetDirectoryStructure(destinationDirPath,
                                                                                                                            ".",
                                                                                                                            true);
                AssertEqualDirectoryStructure(".",
                                              ref preImportDirStructure,
                                              ref postImportDirStructure,
                                              true);

                string modelDirSave = Path.Combine(dsprojSave + "_data", importedModel.Name);
                Assert.That(Directory.Exists(modelDirSave), Is.True, "Expected a model directory, but found none.");
                string inputDirSave = Path.Combine(modelDirSave, "input");
                Assert.That(Directory.Exists(inputDirSave), Is.True, "Expected an input directory, but found none.");

                IEnumerable<string> saveInputFiles = Directory.EnumerateFiles(inputDirSave);
                AssertThatInputFilesAreEqual(importInputFiles, saveInputFiles);

                AssertThatOutputNotExists(modelDirSave, importedModel);
            }
        }

        /// <summary>
        /// GIVEN a migrated model with output at a custom location and without an OutputDir set
        /// AND a project
        /// WHEN this model is imported into this project
        /// THEN the input files are copied correctly
        /// AND the output is handled correctly
        /// AND the import location does not change
        /// </summary>
        [Test]
        public void GivenAMigratedModelWithOutputAtACustomLocationAndWithoutOutputDirValueSet_WhenThisModelIsImported_ThenTheInputAndOutputAreHandledCorrectly()
        {
            CreateTestDirectories();
            CopyProjectToDestinationDir("TestModel");

            using (var app = GetConfiguredApplication())
            {
                // Given
                string modelDirImport = Path.Combine(destinationDirPath, "Project1.dsproj_data", "FlowFM1");
                List<string> importInputFiles = Directory.EnumerateFiles(modelDirImport).ToList();

                string mduPath = Path.Combine(modelDirImport, "input", "FlowFM1.mdu");

                MigrateModel(app, projectFilePath);
                MoveOutputDirToCustomLocation(modelDirImport);

                Dictionary<string, Tuple<Tuple<string, string>[], string[]>> preImportDirStructure = GetDirectoryStructure(destinationDirPath,
                                                                                                                           ".",
                                                                                                                           true);

                // When
                string dsprojSave = Path.Combine(tempDirPath, "Project1.dsproj");
                WaterFlowFMModel importedModel = ImportModelIntoProject(app, mduPath, dsprojSave);

                // Then
                Dictionary<string, Tuple<Tuple<string, string>[], string[]>> postImportDirStructure = GetDirectoryStructure(destinationDirPath,
                                                                                                                            ".",
                                                                                                                            true);
                AssertEqualDirectoryStructure(".",
                                              ref preImportDirStructure,
                                              ref postImportDirStructure,
                                              true);

                string modelDirSave = Path.Combine(dsprojSave + "_data", importedModel.Name);
                Assert.That(Directory.Exists(modelDirSave), Is.True, "Expected a model directory, but found none.");
                string inputDirSave = Path.Combine(modelDirSave, "input");
                Assert.That(Directory.Exists(inputDirSave), Is.True, "Expected an input directory, but found none.");

                IEnumerable<string> saveInputFiles = Directory.EnumerateFiles(inputDirSave);
                AssertThatInputFilesAreEqual(importInputFiles, saveInputFiles);

                AssertThatOutputNotExists(modelDirSave, importedModel);
            }
        }

        /// <summary>
        /// GIVEN a migrated model with output at a custom location and with an OutputDir set
        /// AND a project
        /// WHEN this model is imported into this project
        /// THEN the input files are copied correctly
        /// AND the output is handled correctly
        /// AND the import location does not change
        /// </summary>
        [Test]
        public void GivenAMigratedModelWithOutputAtACustomLocationAndWithOutputDirValueSet_WhenThisModelIsImported_ThenTheInputAndOutputAreHandledCorrectly()
        {
            CreateTestDirectories();
            CopyProjectToDestinationDir("TestModel");

            using (var app = GetConfiguredApplication())
            {
                // Given
                string modelDirImport = Path.Combine(destinationDirPath, "Project1.dsproj_data", "FlowFM1");
                List<string> importInputFiles = Directory.EnumerateFiles(modelDirImport).ToList();

                string mduPath = Path.Combine(modelDirImport, "input", "FlowFM1.mdu");

                MigrateModel(app, projectFilePath);
                UpdateOutputDirInMDUTo(mduPath, "../" + customOutputFolder);
                MoveOutputDirToCustomLocation(modelDirImport);

                Dictionary<string, Tuple<Tuple<string, string>[], string[]>> preImportDirStructure = GetDirectoryStructure(destinationDirPath,
                                                                                                                           ".",
                                                                                                                           true);

                // When
                string dsprojSave = Path.Combine(tempDirPath, "Project1.dsproj");
                WaterFlowFMModel importedModel = ImportModelIntoProject(app, mduPath, dsprojSave);

                // Then
                Dictionary<string, Tuple<Tuple<string, string>[], string[]>> postImportDirStructure = GetDirectoryStructure(destinationDirPath,
                                                                                                                            ".",
                                                                                                                            true);
                AssertEqualDirectoryStructure(".",
                                              ref preImportDirStructure,
                                              ref postImportDirStructure,
                                              true);

                string modelDirSave = Path.Combine(dsprojSave + "_data", importedModel.Name);
                Assert.That(Directory.Exists(modelDirSave), Is.True, "Expected a model directory, but found none.");
                string inputDirSave = Path.Combine(modelDirSave, "input");
                Assert.That(Directory.Exists(inputDirSave), Is.True, "Expected an input directory, but found none.");

                IEnumerable<string> saveInputFiles = Directory.EnumerateFiles(inputDirSave);
                AssertThatInputFilesAreEqual(importInputFiles, saveInputFiles);

                AssertThatOutputNotExists(modelDirSave, importedModel);
            }
        }

        /// <summary>
        /// GIVEN a migrated model with output and an OutputDir set
        /// AND an Integrated Model
        /// WHEN this model is imported into this Integrated Model
        /// THEN the input files are copied correctly
        /// AND the model is added to the integrated model
        /// AND the output of the imported model is empty
        /// </summary>
        [Test]
        public void GivenAMigratedModelWithOutputAndAnOutputDirSet_WhenThisModelIsImportedIntoAnIntegratedModel_ThenTheInputFilesAreHandledCorrectlyAndTheModelIsAddedToTheIntegratedModelAndTheOutputOfTheImportedModelIsEmpty()
        {
            CreateTestDirectories();
            CopyProjectToDestinationDir("TestModel");

            using (var app = GetConfiguredApplication())
            {
                // Given
                string modelDirImport = Path.Combine(destinationDirPath, "Project1.dsproj_data", "FlowFM1");
                List<string> importInputFiles = Directory.EnumerateFiles(modelDirImport).ToList();

                string mduPath = Path.Combine(modelDirImport, "input", "FlowFM1.mdu");

                MigrateModel(app, projectFilePath);

                Dictionary<string, Tuple<Tuple<string, string>[], string[]>> preImportDirStructure = GetDirectoryStructure(destinationDirPath,
                                                                                                                           ".",
                                                                                                                           true);

                // When
                string dsprojSave = Path.Combine(tempDirPath, "Project1.dsproj");
                HydroModel integratedModel = ImportModelIntoIntegratedModel(app, mduPath, dsprojSave);

                // Then

                List<IActivity> integratedModelActivities = integratedModel.Activities.ToList();
                Assert.That(integratedModelActivities.Count, Is.EqualTo(1), "Expected the integrated model to contain a single activity.");
                var importedModel = integratedModelActivities.First() as WaterFlowFMModel;
                Assert.That(importedModel, Is.Not.Null, "Expected the imported model to be a WaterFlowFMModel.");

                Dictionary<string, Tuple<Tuple<string, string>[], string[]>> postImportDirStructure = GetDirectoryStructure(destinationDirPath,
                                                                                                                            ".",
                                                                                                                            true);
                AssertEqualDirectoryStructure(".",
                                              ref preImportDirStructure,
                                              ref postImportDirStructure,
                                              true);

                string modelDirSave = Path.Combine(dsprojSave + "_data", importedModel.Name);
                Assert.That(Directory.Exists(modelDirSave), Is.True, "Expected a model directory, but found none.");
                string inputDirSave = Path.Combine(modelDirSave, "input");
                Assert.That(Directory.Exists(inputDirSave), Is.True, "Expected an input directory, but found none.");

                IEnumerable<string> saveInputFiles = Directory.EnumerateFiles(inputDirSave);
                AssertThatInputFilesAreEqual(importInputFiles, saveInputFiles);

                AssertThatOutputNotExists(modelDirSave, importedModel);
            }
        }

        [TestCase(TrachytopesModelProjectDirName)]
        [TestCase("TestModel")]
        //5.1 & 5.2
        public void GivenAnFMModelWithInputAndOutput_WhenOpeningTheProject_ThenDirectoryStructureShouldBeMigratedToNewVersion(
            string projectFolder)
        {
            List<string> filtersInput;
            if (projectFolder == TrachytopesModelProjectDirName)
            {
                filtersInput = filtersInputWithTrachytopes;
            }
            else
            {
                filtersInput = filtersCommonInput;
            }

            FileUtils.CreateDirectoryIfNotExists(destinationDirPath);

            CopyProjectToDestinationDir(projectFolder);

            using (var app = GetConfiguredApplication())
            {
                app.OpenProject(projectFilePath);

                app.CloseProject();

                AssertProjectFileAndFolderExist();
                AssertModelDirectoryExists();
                AssertInputDirectoryExists();
                AssertOutputDirectoryExists();
                AssertFilesExtensionsExistInDirectory(filtersInput, inputDirPath);
                string[] directoriesInOutputFolder = Directory.GetDirectories(outputDirPath);
                Assert.AreEqual(2, directoriesInOutputFolder.Length,
                                $"The number of folders in '{OutputDirName}' is not as expected. There should be a waq output folder and a snapped folder.");
                AssertFilesExtensionsExistInDirectory(filtersOutputWAQ, outputWAQDirPath);
                AssertFilesExtensionsExistInDirectory(filtersOutputFM_OldModel, outputDirPath);
                Assert.IsTrue(Directory.Exists(snappedDirPath));

                app.OpenProject(projectFilePath);

                var model = (WaterFlowFMModel) app.GetAllModelsInProject().FirstOrDefault();

                Assert.NotNull(model);

                app.RunActivity(model);

                Assert.AreEqual(ActivityStatus.Cleaned, model.Status);

                app.CloseProject();
            }
        }

        /// <summary>
        /// GIVEN a migrated model with output
        /// AND a project
        /// WHEN this model is imported into this project
        /// THEN the input files are copied correctly
        /// AND the output is handled correctly
        /// AND the import location does not change
        /// </summary>
        [TestCase("")]
        [TestCase("../output")]
        public void GivenAMigratedModelWithOutput_WhenThisModelIsImported_ThenTheInputAndOutputAreHandledCorrectly(string outputDirValue)
        {
            CreateTestDirectories();
            CopyProjectToDestinationDir("TestModel");

            using (var app = GetConfiguredApplication())
            {
                // Given
                string modelDirImport = Path.Combine(destinationDirPath, "Project1.dsproj_data", "FlowFM1");
                List<string> importInputFiles = Directory.EnumerateFiles(modelDirImport).ToList();

                string mduPath = Path.Combine(modelDirImport, "input", "FlowFM1.mdu");

                MigrateModel(app, projectFilePath);
                UpdateOutputDirInMDUTo(mduPath, outputDirValue);

                Dictionary<string, Tuple<Tuple<string, string>[], string[]>> preImportDirStructure = GetDirectoryStructure(destinationDirPath,
                                                                                                                           ".",
                                                                                                                           true);

                // When
                string dsprojSave = Path.Combine(tempDirPath, "Project1.dsproj");
                WaterFlowFMModel importedModel = ImportModelIntoProject(app, mduPath, dsprojSave);

                // Then
                Dictionary<string, Tuple<Tuple<string, string>[], string[]>> postImportDirStructure = GetDirectoryStructure(destinationDirPath,
                                                                                                                            ".",
                                                                                                                            true);
                AssertEqualDirectoryStructure(".",
                                              ref preImportDirStructure,
                                              ref postImportDirStructure,
                                              true);

                string modelDirSave = Path.Combine(dsprojSave + "_data", importedModel.Name);
                Assert.That(Directory.Exists(modelDirSave), Is.True, "Expected a model directory, but found none.");
                string inputDirSave = Path.Combine(modelDirSave, "input");
                Assert.That(Directory.Exists(inputDirSave), Is.True, "Expected an input directory, but found none.");

                IEnumerable<string> saveInputFiles = Directory.EnumerateFiles(inputDirSave);
                AssertThatInputFilesAreEqual(importInputFiles, saveInputFiles);

                AssertThatOutputNotExists(modelDirSave, importedModel);
            }
        }

        private static void MigrateModel(IApplication app, string dsProjPath)
        {
            bool hasOpened = app.OpenProject(dsProjPath);
            Assert.That(hasOpened, Is.True,
                        $"Could not open project at {dsProjPath}");

            app.SaveProject();
            app.CloseProject();
        }

        private static void UpdateOutputDirInMDUTo(string mduPath, string newOutputDirPath)
        {
            // The OutputDir is already empty.
            if (string.IsNullOrEmpty(newOutputDirPath))
            {
                return;
            }

            string[] mduData = File.ReadAllLines(mduPath);
            var regex = new Regex(@"OutputDir\s*=\s*");

            for (var i = 0; i < mduData.Length; i++)
            {
                string line = mduData[i];

                if (!regex.IsMatch(line))
                {
                    continue;
                }

                mduData[i] = regex.Replace(line, $"OutputDir = {newOutputDirPath}");
                break;
            }

            File.WriteAllLines(mduPath, mduData);
        }

        private static void MoveOutputDirToCustomLocation(string outputParentDirectory)
        {
            Directory.CreateDirectory(Path.Combine(outputParentDirectory,
                                                   Path.GetDirectoryName(customOutputFolder)));
            Directory.Move(Path.Combine(outputParentDirectory, "output"),
                           Path.Combine(outputParentDirectory, customOutputFolder));
        }

        private static WaterFlowFMModel ImportModelIntoProject(IApplication app, string mduPath, string DsprojSave)
        {
            app.CreateNewProject();
            IFileImporter relevantImporter = app.FileImporters
                                                .FirstOrDefault(importer => importer is WaterFlowFMFileImporter);

            var importedModel = relevantImporter.ImportItem(mduPath) as WaterFlowFMModel;
            Assert.That(importedModel, Is.Not.Null,
                        "Expected the imported model to exist.");
            AddModelToProject(importedModel, app);

            app.SaveProjectAs(DsprojSave);
            return importedModel;
        }

        private static HydroModel ImportModelIntoIntegratedModel(IApplication app, string mduPath, string DsprojSave)
        {
            // When
            app.CreateNewProject();
            var newIntegratedModel = new HydroModel() {Name = "Blastoise"};

            AddModelToProject(newIntegratedModel, app);

            IFileImporter relevantImporter = app.FileImporters
                                                .FirstOrDefault(importer => importer is WaterFlowFMFileImporter);

            relevantImporter.ImportItem(mduPath, newIntegratedModel);

            app.SaveProjectAs(DsprojSave);
            return newIntegratedModel;
        }

        private static void AssertThatInputFilesAreEqual(IEnumerable<string> importInputFiles,
                                                         IEnumerable<string> saveInputFiles)
        {
            List<string> importFiles = importInputFiles.Select(Path.GetFileName).Where(p => p != "restart.meta").ToList();
            List<string> saveFiles = saveInputFiles.Select(Path.GetFileName).Where(p => p != "initialtracertracer.xyz" && p != "initialFields.ini").ToList();

            Assert.That(saveFiles.Count, Is.EqualTo(importFiles.Count), "Expected the number of saved input files to be equal to the original number of input files.");

            importFiles.Sort();
            saveFiles.Sort();

            for (var i = 0; i < saveFiles.Count; i++)
            {
                Assert.That(saveFiles[i], Is.EqualTo(importFiles[i]), "Expected all the files to be equal but found two different files:");
            }
        }

        private static void AssertThatOutputNotExists(string modelDirSave,
                                                      WaterFlowFMModel importedModel)
        {
            string outputPath = Path.Combine(modelDirSave, "output");
            Assert.That(Directory.Exists(outputPath), Is.False, "Did not expect an output directory.");

            Assert.That(importedModel.OutputHisFileStore, Is.Null, "Expected no OutputHisFileStore with no output");
            Assert.That(importedModel.OutputMapFileStore, Is.Null, "Expected no OutputMapFileStore with no output");
            Assert.That(importedModel.OutputClassMapFileStore, Is.Null, "Expected no OutputClassFileStore with no output");
        }

        private static void CreateExportedFmModel(IApplication app,
                                                  string exportPath,
                                                  out WaterFlowFMModel fmModel)
        {
            bool hasOpened = app.OpenProject(projectFilePath);
            Assert.That(hasOpened, Is.True,
                        $"Could not open project at {projectFilePath}");

            fmModel = app.GetAllModelsInProject()
                         .FirstOrDefault(item => item is WaterFlowFMModel) as WaterFlowFMModel;
            Assert.That(fmModel, Is.Not.Null,
                        "Expected the FlowFM Model to be not null.");

            string exportFilePath = Path.Combine(exportPath,
                                                 Path.GetFileName(fmModel.MduFilePath));
            IFileExporter relevantExporter = app.FileExporters
                                                .FirstOrDefault(exporter => exporter is FMModelFileExporter);
            Assert.That(relevantExporter, Is.Not.Null,
                        "Expected the app to contain a WaterFlowFMMileExporter.");

            bool hasExported = relevantExporter.Export(fmModel, exportFilePath);
            Assert.That(hasExported, Is.True,
                        "Expected the export to succeed.");

            app.CloseProject();
        }

        private static void AssertInputImportedModelIsCorrect(string fmModelPath,
                                                              Dictionary<string, Tuple<Tuple<string, string>[], string[]>> originalInputDirStructure)
        {
            List<string> modelDirSubFolders = Directory.EnumerateDirectories(fmModelPath).ToList();

            Assert.That(modelDirSubFolders.Count, Is.EqualTo(1),
                        $"Expected only one folder in {fmModelPath}.");

            string inputDirPath = Path.Combine(fmModelPath, "input");
            Assert.That(modelDirSubFolders.First(), Is.EqualTo(inputDirPath),
                        "Expected only an input folder in the model folder.");

            IEnumerable<string> modelDirFiles = Directory.EnumerateFiles(fmModelPath);
            Assert.That(modelDirFiles.Any(), Is.False, $"Expected no files in {fmModelPath}");

            // the input files are copied into this project folder
            Dictionary<string, Tuple<Tuple<string, string>[], string[]>> newInputDirStructure = GetDirectoryStructure(inputDirPath, ".");

            AssertEqualDirectoryStructure(".",
                                          ref originalInputDirStructure,
                                          ref newInputDirStructure);
        }

        private static void AssertCorrectOutputIsLinked(string outputDirectoryPath, WaterFlowFMModel fmModel)
        {
            Assert.AreEqual(Path.Combine(outputDirectoryPath, "FlowFM1_map.nc"), fmModel.OutputMapFileStore.Path);
            Assert.AreEqual(Path.Combine(outputDirectoryPath, "FlowFM1_his.nc"), fmModel.OutputHisFileStore.Path);
        }

        private IApplication GetConfiguredApplication()
        {
            var applicationSettingsMock = MockRepository.GenerateStub<ApplicationSettingsBase>();
            applicationSettingsMock["WorkDirectory"] = workingDirPath;

            applicationSettingsMock.Replay();

            var pluginsToAdd = new List<IPlugin>()
            {
                new NHibernateDaoApplicationPlugin(),
                new CommonToolsApplicationPlugin(),
                new SharpMapGisApplicationPlugin(),
                new FlowFMApplicationPlugin(),
                new NetworkEditorApplicationPlugin(),
            };
            
            var app = new DeltaShellApplicationBuilder().WithPlugins(pluginsToAdd).Build();
            app.UserSettings = applicationSettingsMock;
            
            app.Run();
            app.CreateNewProject();
            return app;
        }

        private IApplication GetConfiguredHydroApplication()
        {
            var applicationSettingsMock = MockRepository.GenerateStub<ApplicationSettingsBase>();
            applicationSettingsMock["WorkDirectory"] = workingDirPath;

            applicationSettingsMock.Replay();

            var pluginsToAdd = new List<IPlugin>()
            {
                new NHibernateDaoApplicationPlugin(),
                new CommonToolsApplicationPlugin(),
                new SharpMapGisApplicationPlugin(),
                new FlowFMApplicationPlugin(),
                new NetworkEditorApplicationPlugin(),
                new HydroModelApplicationPlugin(),
            };
            
            var app = new DeltaShellApplicationBuilder().WithPlugins(pluginsToAdd).Build();
            app.UserSettings = applicationSettingsMock;

            app.Run();
            app.CreateNewProject();
            return app;
        }
        

        private void AddFeaturesToModel(WaterFlowFMModel model)
        {
            model.Grid = CreateNewGrid(5, 10);
            model.Area.LandBoundaries.Add(CreateLandBoundary());
            model.Area.DryAreas.Add(CreateDryArea());
            model.Area.DryPoints.Add(CreateDryPoints());
            model.Area.ObservationPoints.Add(CreateObservationPoint());
            model.Area.Structures.Add(CreateWeir());
            model.Area.FixedWeirs.Add(CreateFixedWeir());
            model.Area.ObservationCrossSections.Add(CreateObservationCrossSection());
            AddFlowBoundaryConditionToModel(model);
            AddTracer(model);
        }

        private static DHydroConfigXmlExporter GetDimrFileExporter()
        {
            var fileExportService = new FileExportService();
            fileExportService.RegisterFileExporter(new FMModelFileExporter());
            return new DHydroConfigXmlExporter(fileExportService);
        }

        private static void CopyTestDataFileToTemp(string fileName)
        {
            string sourceFilePath = Path.Combine(testDataDirPath, fileName);
            string targetFilePath = Path.Combine(tempDirPath, fileName);
            FileUtils.CopyFile(sourceFilePath, targetFilePath);
            Assert.IsTrue(File.Exists(targetFilePath));
        }

        private static void CopyFourierAndCalibrationFilesToTemp()
        {
            CopyTestDataFileToTemp("fourier.fou");
            CopyTestDataFileToTemp("calibration.cll");
            CopyTestDataFileToTemp("calibration.cld");
        }

        private static void CopyWindFilesToTemp()
        {
            CopyTestDataFileToTemp(WindFileName);
            CopyTestDataFileToTemp(GridFileName);
        }

        private static void CopyTrachytopeFilesToTemp()
        {
            CopyTestDataFileToTemp("trachytopes.arl");
            CopyTestDataFileToTemp("trachytopes.ttd");
        }

        private static void CopyProjectToDestinationDir(string folderName)
        {
            string sourceFilePath = Path.Combine(testDataDirPath, folderName);

            var sourceDirectory = new DirectoryInfo(sourceFilePath);

            sourceDirectory.GetFiles().ForEach(f =>
            {
                string targetFilePath = Path.Combine(destinationDirPath, f.Name);
                FileUtils.CopyFile(f.FullName, targetFilePath);
            });
            sourceDirectory.GetDirectories().ForEach(d =>
            {
                string targetDirPath = Path.Combine(destinationDirPath, d.Name);
                FileUtils.CopyDirectory(d.FullName, targetDirPath);
            });
        }

        private UnstructuredGrid CreateNewGrid(int width, int length)
        {
            var coordinateSystemFactory = new OgrCoordinateSystemFactory();
            var grid = new UnstructuredGrid
            {
                CoordinateSystem = coordinateSystemFactory.CreateFromEPSG(3857) // WGS 84 / Pseudo-Mercator
            };

            int numberOfCoordinates = width * length;
            for (var n = 0; n < length; n++)
            {
                for (var m = 0; m < width; m++)
                {
                    grid.Vertices.Add(new Coordinate(m, n, -1));
                }
            }

            for (var i = 0; i < numberOfCoordinates; i++)
            {
                if ((i + 1) % width != 0)
                {
                    grid.Edges.Add(new Edge(i, i + 1));
                }
            }

            for (var i = 0; i < numberOfCoordinates - width; i++)
            {
                grid.Edges.Add(new Edge(i, i + width));
            }

            return grid;
        }

        private static LandBoundary2D CreateLandBoundary()
        {
            return new LandBoundary2D
            {
                GroupName = "Boundaries",
                Name = "LandBoundary",
                Geometry = new LineString(new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(0, 100),
                    new Coordinate(50, 50)
                })
            };
        }

        private GroupableFeature2DPolygon CreateDryArea()
        {
            Coordinate[] polygon =
            {
                new Coordinate(1, 1),
                new Coordinate(1, 2),
                new Coordinate(2, 2),
                new Coordinate(1, 1)
            };

            return new GroupableFeature2DPolygon
            {
                Geometry = new Polygon(new LinearRing(polygon)),
                Name = "DryArea"
            };
        }

        private GroupablePointFeature CreateDryPoints() =>
            new GroupablePointFeature
            {
                GroupName = "DryPoints",
                Geometry = new Point(new Coordinate(0, 100))
            };

        private static GroupableFeature2DPoint CreateObservationPoint() =>
            new GroupableFeature2DPoint
            {
                Geometry = new Point(5, 5),
                Name = "ObservationPoint"
            };

        private static Structure CreateWeir()
        {
            Coordinate[] lineString =
            {
                new Coordinate(7, 7),
                new Coordinate(8, 8)
            };

            return new Structure
            {
                Name = "Weir",
                Formula = new SimpleWeirFormula(),
                Geometry = new LineString(lineString)
            };
        }

        private static FixedWeir CreateFixedWeir()
        {
            Coordinate[] lineString =
            {
                new Coordinate(3, 3),
                new Coordinate(4, 4)
            };

            return new FixedWeir
            {
                Geometry = new LineString(lineString),
                Name = "FixedWeir"
            };
        }

        private static ObservationCrossSection2D CreateObservationCrossSection()
        {
            Coordinate[] lineString =
            {
                new Coordinate(3, 3),
                new Coordinate(4, 4)
            };

            return new ObservationCrossSection2D
            {
                Name = "ObservationCrossPoint",
                Geometry = new LineString(lineString)
            };
        }

        private static void EnableSalinityAndTemperature(WaterFlowFMModel model)
        {
            model.ModelDefinition.GetModelProperty(KnownProperties.UseSalinity).Value = true;
            model.ModelDefinition.GetModelProperty(KnownProperties.Temperature)
                 .SetValueFromString(((int) HeatFluxModelType.TransportOnly).ToString());
        }

        private void AddFlowBoundaryConditionToModel(WaterFlowFMModel model)
        {
            var feature = new Feature2D
            {
                Name = "Boundary1",
                Geometry =
                    new LineString(new[]
                    {
                        new Coordinate(0, 0),
                        new Coordinate(1, 0)
                    })
            };

            var flowBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.Discharge,
                                                                  BoundaryConditionDataType.TimeSeries) {Feature = feature};

            flowBoundaryCondition.AddPoint(0);
            flowBoundaryCondition.PointData[0].Arguments[0].SetValues(new[]
            {
                model.StartTime,
                model.StopTime
            });
            flowBoundaryCondition.PointData[0][model.StartTime] = 0.5;
            flowBoundaryCondition.PointData[0][model.StopTime] = 0.6;

            var set = new BoundaryConditionSet {Feature = feature};
            set.BoundaryConditions.Add(flowBoundaryCondition);
            model.BoundaryConditionSets.Add(set);
        }

        private void AddTracer(WaterFlowFMModel model)
        {
            var tracer = "Tracer1";
            model.TracerDefinitions.AddRange(new List<string> {tracer});

            var boundary = new Feature2D
            {
                Name = "TracerBoundary1",
                Geometry =
                    new LineString(new[]
                    {
                        new Coordinate(0, 0),
                        new Coordinate(1, 0)
                    })
            };
            var set01 = new BoundaryConditionSet {Feature = boundary};
            model.BoundaryConditionSets.Add(set01);

            set01.BoundaryConditions.Add(
                new FlowBoundaryCondition(FlowBoundaryQuantityType.Tracer, BoundaryConditionDataType.Empty)
                {
                    Feature = boundary,
                    TracerName = tracer
                });
        }

        private static void EnableMorphology(WaterFlowFMModel model)
        {
            model.ModelDefinition.GetModelProperty(GuiProperties.UseMorSed).Value = true;
            var cellsValue = ((int) UnstructuredGridFileHelper.BedLevelLocation.Faces).ToString();
            model.ModelDefinition.GetModelProperty(KnownProperties.BedlevType).SetValueFromString(cellsValue);
        }

        private void AddMorphologyBoundaryConditionToModel(WaterFlowFMModel model)
        {
            var feature = new Feature2D
            {
                Name = "Boundary2",
                Geometry = new LineString(new[]
                {
                    new Coordinate(1, 0),
                    new Coordinate(0, 1)
                })
            };

            var morphologyBoundaryCondition = new FlowBoundaryCondition(
                FlowBoundaryQuantityType.MorphologyBedLevelPrescribed,
                BoundaryConditionDataType.TimeSeries)
            {
                Feature = feature,
                SedimentFractionNames = new List<string> {"SedimentFraction1"}
            };

            morphologyBoundaryCondition.AddPoint(0);
            morphologyBoundaryCondition.PointData[0].Arguments[0].SetValues(new[]
            {
                model.StartTime,
                model.StopTime
            });
            morphologyBoundaryCondition.PointData[0][model.StartTime] = 0.5;
            morphologyBoundaryCondition.PointData[0][model.StopTime] = 0.6;

            var flowBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel,
                                                                  BoundaryConditionDataType.TimeSeries) {Feature = feature};

            flowBoundaryCondition.AddPoint(0);
            flowBoundaryCondition.PointData[0].Arguments[0].SetValues(new[]
            {
                model.StartTime,
                model.StopTime
            });
            flowBoundaryCondition.PointData[0][model.StartTime] = 0.5;
            flowBoundaryCondition.PointData[0][model.StopTime] = 0.6;

            var set = new BoundaryConditionSet {Feature = feature};
            set.BoundaryConditions.Add(flowBoundaryCondition);
            set.BoundaryConditions.Add(morphologyBoundaryCondition);

            model.BoundaryConditionSets.Add(set);
        }

        private void AddSedimentFraction(WaterFlowFMModel model)
        {
            model.SedimentFractions = new EventedList<ISedimentFraction>();
            model.SedimentFractions.Add(new SedimentFraction {Name = "SedimentFraction2"});
        }

        private static void AddWindToModel(WaterFlowFMModel model)
        {
            string windFilePath = Path.Combine(tempDirPath, WindFileName);
            string gridFilePath = Path.Combine(tempDirPath, GridFileName);
            var windField = GriddedWindField.CreateCurviField(windFilePath, gridFilePath);
            model.ModelDefinition.WindFields.Add(windField);
        }

        private void SimulateUserAddingReferencesInMduFile()
        {
            using (StreamWriter sw = File.AppendText(tempMduFilePath))
            {
                sw.WriteLine("FouFile           = fourier.fou");
                sw.WriteLine("");
                sw.WriteLine("[calibration]");
                sw.WriteLine("DefinitionFile    = calibration.cld");
                sw.WriteLine("AreaFile          = calibration.cll");
                sw.WriteLine("");
            }
        }

        private static void SimulateUserAddingTrachytopesInMduFile(WaterFlowFMModel model)
        {
            model.ModelDefinition.GetModelProperty(KnownProperties.TrtRou).SetValueFromString("Y");
            model.ModelDefinition.GetModelProperty(KnownProperties.TrtDef).SetValueFromString("trachytopes.ttd");
            model.ModelDefinition.GetModelProperty(KnownProperties.TrtL).SetValueFromString("trachytopes.arl");
            model.ModelDefinition.GetModelProperty(KnownProperties.DtTrt).SetValueFromString("300");
        }

        private void AdjustSettingsOutputParameters(WaterFlowFMModel model)
        {
            model.ModelDefinition.WriteSnappedFeatures = true;
            model.ModelDefinition.GetModelProperty(GuiProperties.SpecifyWaqOutputInterval).Value = true;
            model.ModelDefinition.GetModelProperty(GuiProperties.SpecifyWaqOutputStartTime).Value = true;
            model.ModelDefinition.GetModelProperty(GuiProperties.SpecifyWaqOutputStopTime).Value = true;
            model.ModelDefinition.GetModelProperty(GuiProperties.WaqOutputDeltaT).Value =
                new TimeSpan(0, 0, 10, 0);
        }

        private void UpdateBedLevel(WaterFlowFMModel model)
        {
            TypeUtils.CallPrivateMethod(model, "UpdateBathymetryCoverage",
                                        UnstructuredGridFileHelper.BedLevelLocation.NodesMinLev);
        }

        private static void ValidateAndRunModel(WaterFlowFMModel model, IApplication app)
        {
            model.ValidateBeforeRun = true;
            ValidationReport report = model.Validate();
            Assert.AreEqual(0, report.AllErrors.Count(), "There are errors in the model after importing the MDU file");
            app.RunActivity(model);
            Assert.AreEqual(ActivityStatus.Cleaned, model.Status);
        }

        private static void AddModelToProject(IHydroModel model, IApplication app)
        {
            Project project = app.Project;
            project.RootFolder.Add(model);
        }

        private void AssertFilesExtensionsExistInDirectory(IEnumerable<string> expectedExtensions, string dirPath)
        {
            var dirInfo = new DirectoryInfo(dirPath);
            expectedExtensions.ForEach(ext =>
                                           Assert.IsTrue(dirInfo.GetFiles($"*{ext}").Any(),
                                                         Message_MissingFileOrFolderExtension("file", ext, dirInfo.Name)));
        }

        private void AssertProjectFileAndFolderExist()
        {
            string[] subDirectories = Directory.GetDirectories(destinationDirPath);
            Assert.NotNull(subDirectories);
            Assert.AreEqual(1, subDirectories.Length,
                            "There should only be one folder: the project (.dsproj_data) folder.");

            string[] subFiles = Directory.GetFiles(destinationDirPath);
            Assert.NotNull(subFiles);
            Assert.AreEqual(1, subFiles.Length, "There should only be one file: the project (.dsproj) file.");

            var expectedCount = 1;

            // Get all files/folders with the project extension
            int actualFileCount = Directory.GetFiles(destinationDirPath, $"*{ProjectFileExtension}").Length;
            int actualFolderCount = Directory.GetDirectories(destinationDirPath, $"*{ProjectDirExtension}").Length;

            Assert.AreEqual(expectedCount, actualFileCount,
                            Message_WrongNumberOfFilesOrFolders(expectedCount, "files", ProjectFileExtension, actualFileCount,
                                                                "Temp"));

            Assert.AreEqual(expectedCount, actualFolderCount,
                            Message_WrongNumberOfFilesOrFolders(expectedCount, "folders", ProjectDirExtension, actualFolderCount,
                                                                "Temp"));
        }

        private void AssertModelDirectoryExists()
        {
            AssertDirectoryExists(modelDirPath);
        }

        private void AssertInputDirectoryExists()
        {
            AssertDirectoryExists(inputDirPath);
        }

        private void AssertOutputDirectoryExists()
        {
            AssertDirectoryExists(outputDirPath);
        }

        private void AssertDflowfmDirectoryExists()
        {
            AssertDirectoryExists(dflowfmDirPath);
        }

        private void AssertDirectoryExists(string dirPath)
        {
            var dirInfo = new DirectoryInfo(dirPath);
            string parentName = dirInfo.Parent.Name;

            Assert.IsTrue(dirInfo.Exists,
                          Message_MissingFileOrFolderName("folder", dirInfo.Name, parentName));
        }

        private void AssertWorkingOutputDirectoryWithSubFoldersExists(string workingOutputDirPath)
        {
            string workingOutputFMDirPath = workingOutputDirPath;
            string workingOutputWAQDirPath = Path.Combine(workingOutputDirPath, outputWAQDirName);
            string workingSnappedDirPath = Path.Combine(workingOutputDirPath, SnappedDirName);

            AssertDirectoryExists(workingOutputFMDirPath);
            AssertDirectoryExists(workingOutputWAQDirPath);
            AssertDirectoryExists(workingSnappedDirPath);

            AssertFilesExtensionsExistInDirectory(filtersOutputFM_NewModel, workingOutputFMDirPath);
            AssertFilesExtensionsExistInDirectory(filtersOutputWAQ, workingOutputWAQDirPath);
            AssertFilesExtensionsExistInDirectory(filtersSnapped, workingSnappedDirPath);
        }

        private static string Message_WrongNumberOfFilesOrFolders(int expectedCount, string type,
                                                                  string extension, int actualCount, string parentDirName)
        {
            return $"We expected {expectedCount} {type} with extension '{extension}', " +
                   $"but there were {actualCount} in directory '{parentDirName}'.";
        }

        private string Message_MissingFileOrFolderExtension(string type, string ext, string parentDirName)
        {
            return $"No {type} with extension '{ext}' exists in directory '{parentDirName}'.";
        }

        private string Message_MissingFileOrFolderName(string type, string name, string parentDirName)
        {
            return $"No {type} with name '{name}' exists in directory '{parentDirName}'.";
        }

        private static void CreateTestDirectories()
        {
            FileUtils.CreateDirectoryIfNotExists(destinationDirPath);
            FileUtils.CreateDirectoryIfNotExists(tempDirPath);
        }

        private static void DeleteTestDirectories()
        {
            FileUtils.DeleteIfExists(destinationDirPath);
            FileUtils.DeleteIfExists(tempDirPath);
            FileUtils.DeleteIfExists(workingDirPath);
            FileUtils.DeleteIfExists(exportDimrDirPath);
        }

        private static Dictionary<string, Tuple<Tuple<string, string>[], string[]>> GetDirectoryStructure(
            string basePath,
            string relativePath,
            bool doChecksum = false,
            params string[] ignoreFileExtension)
        {
            var dirStructure = new Dictionary<string, Tuple<Tuple<string, string>[], string[]>>();
            GetDirectoryStructure(basePath, relativePath, ref dirStructure, doChecksum, ignoreFileExtension);
            return dirStructure;
        }

        private static void GetDirectoryStructure(
            string basePath, string relativePath,
            ref Dictionary<string, Tuple<Tuple<string, string>[], string[]>> structure,
            bool doChecksum = false,
            params string[] ignoreFileExtension)
        {
            string currentPath = Path.Combine(basePath, relativePath);

            // Get relevant data
            string[] files = Directory.GetFiles(currentPath);
            string[] subdirs = Directory.GetDirectories(currentPath);

            var filesList = new List<Tuple<string, string>>();

            // Format relevant data
            for (var i = 0; i < files.Length; i++)
            {
                string checksum = doChecksum ? FileUtils.GetChecksum(files[i]) : "";

                files[i] = Path.GetFileName(files[i]);
                if (!ignoreFileExtension.Any(fileExtension => files[i].Contains(fileExtension)))
                {
                    filesList.Add(new Tuple<string, string>(files[i], checksum));
                }
            }

            Tuple<string, string>[] filesAndChecksums = filesList.ToArray();

            Array.Sort(files, StringComparer.InvariantCultureIgnoreCase);

            for (var i = 0; i < subdirs.Length; i++)
            {
                subdirs[i] = Path.Combine(relativePath, new DirectoryInfo(subdirs[i]).Name);
            }

            Array.Sort(subdirs, StringComparer.InvariantCultureIgnoreCase);

            // Add to structure
            structure.Add(relativePath, new Tuple<Tuple<string, string>[], string[]>(filesAndChecksums, subdirs));

            foreach (string s in subdirs)
            {
                GetDirectoryStructure(basePath, s, ref structure, doChecksum, ignoreFileExtension);
            }
        }

        private static void AssertEqualDirectoryStructure(string curDir,
                                                          ref Dictionary<string, Tuple<Tuple<string, string>[], string[]>> sourceDirStructure,
                                                          ref Dictionary<string, Tuple<Tuple<string, string>[], string[]>> targetDirStructure,
                                                          bool doCompareChecksums = false)
        {
            Tuple<string, string>[] sourceDirFilesAndChecksums = sourceDirStructure[curDir].Item1;
            string[] sourceDirSubDirectories = sourceDirStructure[curDir].Item2;

            Tuple<string, string>[] targetDirFilesAndChecksums = targetDirStructure[curDir].Item1;
            string[] targetDirSubDirectories = targetDirStructure[curDir].Item2;

            //First check if the number of files/directories are the same
            Assert.AreEqual(sourceDirFilesAndChecksums.Length, targetDirFilesAndChecksums.Length,
                            $"The number of files in source and target {curDir} do not correspond.");
            Assert.AreEqual(sourceDirSubDirectories.Length, targetDirSubDirectories.Length,
                            $"The number of subfolders in source and target {curDir} do not correspond.");

            //If the number of files are correct, then check the names of them.
            for (var i = 0; i < targetDirFilesAndChecksums.Length; i++)
            {
                // Compare strings: source > target -> source is missing a file, else target missing a file.
                string assertMsgNotEqualName =
                    string.Compare(sourceDirFilesAndChecksums[i].Item1, targetDirFilesAndChecksums[i].Item1,
                                   StringComparison.InvariantCultureIgnoreCase) > 0
                        ? $"File {targetDirFilesAndChecksums[i].Item1} does not exist in source {curDir}."
                        : $"File {sourceDirFilesAndChecksums[i].Item1} does not exist in target {curDir}.";
                Assert.AreEqual(sourceDirFilesAndChecksums[i].Item1, targetDirFilesAndChecksums[i].Item1,
                                assertMsgNotEqualName);

                if (!doCompareChecksums)
                {
                    continue;
                }

                var assertMsgNotEqualContent =
                    $"Checksum of file {sourceDirFilesAndChecksums[i].Item1} does not match checksum of file {targetDirFilesAndChecksums[i].Item1} in {curDir}";

                // time is written in mdu
                if (sourceDirFilesAndChecksums[i].Item1 != ModelName + ".mdu")
                {
                    Assert.That(sourceDirFilesAndChecksums[i].Item2, Is.EqualTo(targetDirFilesAndChecksums[i].Item2),
                                assertMsgNotEqualContent);
                }
            }

            // If the number of directories are correct, then check the names of them.
            for (var i = 0; i < sourceDirSubDirectories.Length; i++)
            {
                string assertMsg =
                    string.Compare(sourceDirSubDirectories[i], targetDirSubDirectories[i],
                                   StringComparison.InvariantCultureIgnoreCase) > 0
                        ? $"Folder {targetDirSubDirectories[i]} does not exist in source {curDir}."
                        : $"Folder {sourceDirSubDirectories[i]} does not exist in target {curDir}.";
                Assert.AreEqual(sourceDirSubDirectories[i], targetDirSubDirectories[i], assertMsg);
            }

            // Continue the process for the subfolders.
            foreach (string s in sourceDirSubDirectories)
            {
                AssertEqualDirectoryStructure(s, ref sourceDirStructure, ref targetDirStructure, doCompareChecksums);
            }
        }

        private static void AssertOutputDirectoryIsEmpty()
        {
            Assert.True(FileUtils.IsDirectoryEmpty(outputDirPath),
                        $"Expected: Output folder '{outputDirPath}' should be empty.");
        }

        private static void AssertWorkingDirectoryWithOutputExists(string workingDirectoryPath, string workingOutputDirectoryPath)
        {
            Assert.IsTrue(Directory.Exists(workingDirectoryPath),
                          $"The working directory '{workingDirectoryPath}' does not exist.");
            Assert.IsTrue(Directory.Exists(workingOutputDirectoryPath),
                          $"The output folder '{workingOutputDirectoryPath}' in the working directory does not exist.");
            Assert.IsTrue(Directory.GetFileSystemEntries(workingOutputDirectoryPath).Any(),
                          $"There should have been output in the output folder '{workingOutputDirectoryPath}' in the working directory.");
        }

        private static void AssertWorkingDirectoryIsCleaned(string workingDirectoryPath, string workingOutputDirectoryPath)
        {
            Assert.IsTrue(Directory.Exists(workingDirectoryPath),
                          $"The working directory '{workingDirectoryPath}' should not have been removed after saving.");
            Assert.IsFalse(Directory.Exists(workingOutputDirectoryPath),
                           $"The output directory '{workingOutputDirectoryPath}' should have been deleted after saving.");
            Assert.IsFalse(Directory.GetFileSystemEntries(workingDirectoryPath).Any(),
                           $"The working directory '{workingDirectoryPath}' should have been empty after saving.");
        }
    }
}