using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DeltaShell.Core;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.HydroModel.Export;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Exporters;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.SharpMapGis;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Grids;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using Rhino.Mocks;
using SharpMap.Extensions.CoordinateSystems;
using FixedWeir = DelftTools.Hydro.Structures.FixedWeir;
using LandBoundary2D = DelftTools.Hydro.LandBoundary2D;
using ObservationCrossSection2D = DelftTools.Hydro.ObservationCrossSection2D;

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

        private const string StateFilesDirectoryPostfix = "_states";
        private const string InputDirName = "input";
        private const string OutputDirName = "output";
        private const string SnappedDirName = "snapped";
        private const string DflowfmDirName = "dflowfm";

        private static string projectDirName;
        private static string projectFileName;
        private static string modelDirName;
        private static string outputFMDirName;
        private static string outputWAQDirName;

        private static string projectDirPath;
        private static string projectFilePath;
        private static string modelDirPath;
        private static string inputDirPath;
        private static string outputDirPath;
        private static string outputFMDirPath;
        private static string outputWAQDirPath;
        private static string snappedDirPath;

        private static string dflowfmDirPath;

        private static string testDataDirPath;
        private static string destinationDirPath;
        private static string tempDirPath;
        private static string workingDirPath;
        private static string tempMduFilePath;
        private static string tempProjectFilePath;
        private static string exportMduFilePath;
        private static string exportDimrDirPath;
        private static string exportDimrFilePath;

        private static string mduFileName;

        private const string NoordzeeModelProjectDirName = "NoordzeeModel";
        private const string TrachytopesModelProjectDirName = "TrachytopesModel";

        private static List<string> filtersCommonInput;
        private static List<string> filtersInputWithTrachytopes;
        private static List<string> filtersInputWithMorphology;
        private static List<string> filtersInputWithWind;

        private static List<string> filtersOutput;
        private static List<string> filtersOutputFM_NewModel;
        private static List<string> filtersOutputFM_OldModel;
        private static List<string> filtersOutputWAQ;
        private static List<string> filtersSnapped;

        [TestFixtureSetUp]
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
            outputFMDirName = $"DFM_OUTPUT_{ModelName}";
            outputWAQDirName = $"DFM_DELWAQ_{ModelName}";

            projectFilePath = Path.Combine(destinationDirPath, projectFileName);
            projectDirPath = Path.Combine(destinationDirPath, projectDirName);
            modelDirPath = Path.Combine(projectDirPath, modelDirName);
            inputDirPath = Path.Combine(modelDirPath, InputDirName);
            outputDirPath = Path.Combine(modelDirPath, OutputDirName);
            outputFMDirPath = Path.Combine(outputDirPath, outputFMDirName);
            outputWAQDirPath = Path.Combine(outputDirPath, outputWAQDirName);
            snappedDirPath = Path.Combine(outputDirPath, SnappedDirName);

            exportDimrDirPath = Path.Combine(projectDirPath, ExportDimrDirName);
            exportDimrFilePath = Path.Combine(exportDimrDirPath, ExportDimrFileName);
            dflowfmDirPath = Path.Combine(exportDimrDirPath, DflowfmDirName);
            exportMduFilePath = Path.Combine(projectDirPath, mduFileName);
            tempMduFilePath = Path.Combine(tempDirPath, mduFileName);
            tempProjectFilePath = Path.Combine(tempDirPath, projectFileName);

            filtersCommonInput = new List<string>
            {
                ".mdu",
                ".meta",
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

            var fourierFileFilters = new[] {".fou", ".cld", ".cll"};

            filtersInputWithTrachytopes = filtersCommonInput.Union(fourierFileFilters).Union(new List<string> {".arl", ".ttd"}).ToList();
            filtersInputWithMorphology = filtersCommonInput.Union(fourierFileFilters).Union(new List<string> {".mor", ".sed"}).ToList();
            filtersInputWithWind = filtersCommonInput.Union(fourierFileFilters).Union(new List<string> {".grd", ".apwxwy"}).ToList();
            
            filtersOutputFM_NewModel = new List<string>
            {
                "_numlimdt.xyz",
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

            try
            {
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

                    using (var model = new WaterFlowFMModel(tempMduFilePath))
                    {
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
            finally
            {
                DeleteTestDirectories();
            }
        }

        [Test]
        //1.2
        public void GivenAnFMModelWithMorphology_WhenProjectSavedAs_ThenInputFolderWithCorrectFilesAreGiven()
        {
            CreateTestDirectories();

            try
            {
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

                    using (var model = new WaterFlowFMModel(tempMduFilePath))
                    {
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
            finally
            {
                DeleteTestDirectories();
            }
        }

        [Test]
        //1.3
        public void GivenAnFMModelWithWind_WhenProjectSavedAs_ThenInputFolderWithCorrectFilesAreGiven()
        {
            CreateTestDirectories();

            try
            {
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

                    using (var model = new WaterFlowFMModel(tempMduFilePath))
                    {
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
            finally
            {
                DeleteTestDirectories();
            }
        }

        [Test]
        //2.1
        public void GivenAnFMModelWithTrachytopes_WhenRun_ThenWorkDirIsCreatedInTemp()
        {
            CreateTestDirectories();

            try
            {
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

                    using (var model = new WaterFlowFMModel(tempMduFilePath))
                    {
                        AdjustSettingsOutputParameters(model);
                        UpdateBedLevel(model);
                        AddModelToProject(model, app);

                        app.SaveProjectAs(projectFilePath);

                        ValidateAndRunModel(model, app);

                        app.CloseProject();

                        var workDirInfo = new DirectoryInfo(model.WorkingDirectoryPath);
                        Assert.That(workDirInfo.Exists, "The working directory does not exist.");

                        var retrievedTempFolderPathFromWorkingDirectory = workDirInfo.Parent?.Parent?.FullName;
                        Assert.NotNull(retrievedTempFolderPathFromWorkingDirectory, "Retrieved temp folder path is null");

                        var tempFolderPathFullName = Path.GetTempPath();
                        Assert.That(retrievedTempFolderPathFromWorkingDirectory, Is.SamePath(tempFolderPathFullName), "The working directory is not created in Temp");

                        AssertWorkingOutputDirectoryWithSubFoldersExists(model.WorkingOutputDirectoryPath);
                    }
                }
            }
            finally
            {
                DeleteTestDirectories();
            }
        }

        [Test]
        //2.2
        public void GivenAnFMModelWithMorphology_WhenRun_ThenWorkDirIsCreatedInTemp()
        {
            CreateTestDirectories();

            try
            {
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

                    using (var model = new WaterFlowFMModel(tempMduFilePath))
                    {
                        AdjustSettingsOutputParameters(model);
                        UpdateBedLevel(model);
                        AddModelToProject(model, app);

                        app.SaveProjectAs(projectFilePath);

                        ValidateAndRunModel(model, app);

                        app.CloseProject();

                        var workDirInfo = new DirectoryInfo(model.WorkingDirectoryPath);
                        Assert.That(workDirInfo.Exists, "The working directory does not exist.");

                        var retrievedTempFolderPathFromWorkingDirectory = workDirInfo.Parent?.Parent?.FullName;
                        Assert.NotNull(retrievedTempFolderPathFromWorkingDirectory, "Retrieved temp folder path is null");

                        var tempFolderPathFullName = Path.GetTempPath();
                        Assert.That(retrievedTempFolderPathFromWorkingDirectory, Is.SamePath(tempFolderPathFullName), "The working directory is not created in Temp");

                        AssertWorkingOutputDirectoryWithSubFoldersExists(model.WorkingOutputDirectoryPath);
                    }
                }
            }

            finally
            {
                DeleteTestDirectories();
            }
        }

        [Test]
        //2.3
        public void GivenAnFMModelWithWind_WhenRun_ThenWorkDirIsCreatedInTemp()
        {
            CreateTestDirectories();

            try
            {
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

                    using (var model = new WaterFlowFMModel(tempMduFilePath))
                    {
                        AdjustSettingsOutputParameters(model);
                        UpdateBedLevel(model);
                        AddModelToProject(model, app);

                        app.SaveProjectAs(projectFilePath);

                        ValidateAndRunModel(model, app);

                        app.CloseProject();

                        var workDirInfo = new DirectoryInfo(model.WorkingDirectoryPath);
                        Assert.That(workDirInfo.Exists, "The working directory does not exist.");

                        var retrievedTempFolderPathFromWorkingDirectory = workDirInfo.Parent?.Parent?.FullName;
                        Assert.NotNull(retrievedTempFolderPathFromWorkingDirectory, "Retrieved temp folder path is null");

                        var tempFolderPathFullName = Path.GetTempPath();
                        Assert.That(retrievedTempFolderPathFromWorkingDirectory, Is.SamePath(tempFolderPathFullName), "The working directory is not created in Temp");

                        AssertWorkingOutputDirectoryWithSubFoldersExists(model.WorkingOutputDirectoryPath);
                    }
                }
            }
            finally
            {
                DeleteTestDirectories();
            }
        }

        [Test]
        //2.4
        public void GivenAnFMModelWithWind_WhenRunAndSaved_ThenAllOutputFromWorkingDirIsCopiedToPersistentDirWhileMaintainingTheStructure()
        {
            CreateTestDirectories();

            try
            {
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

                    using (var model = new WaterFlowFMModel(tempMduFilePath))
                    {
                        AdjustSettingsOutputParameters(model);
                        UpdateBedLevel(model);
                        AddModelToProject(model, app);

                        app.SaveProjectAs(projectFilePath);

                        ValidateAndRunModel(model, app);

                        var workingDirectoryPath = model.WorkingDirectoryPath;
                        var workingOutputDirectoryPath = model.WorkingOutputDirectoryPath;

                        var outputWorkingDirStructure =
                            GetDirectoryStructure(workingOutputDirectoryPath, ".", doChecksum: true);

                        AssertWorkingDirectoryWithOutputExists(workingDirectoryPath, workingOutputDirectoryPath);

                        app.SaveProject();

                        AssertWorkingDirectoryIsCleaned(workingDirectoryPath, workingOutputDirectoryPath);

                        app.CloseProject();

                        var outputPersistentDirStructure = GetDirectoryStructure(outputDirPath, ".", doChecksum: true);
                        AssertEqualDirectoryStructure(".", ref outputWorkingDirStructure,
                            ref outputPersistentDirStructure, true);
                    }
                }
            }
            finally
            {
                DeleteTestDirectories();
            }
        }

        [Test]
        //3.1
        public void GivenAnFMModelWithTrachytopes_WhenDimrExported_ThenCorrectFoldersAndFilesAreGiven()
        {
            CreateTestDirectories();

            var expectedExtension = ".xml";

            try
            {
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

                    using (var model = new WaterFlowFMModel(tempMduFilePath))
                    {
                        AdjustSettingsOutputParameters(model);
                        UpdateBedLevel(model);
                        AddModelToProject(model, app);

                        app.SaveProjectAs(projectFilePath);

                        AssertProjectFileAndFolderExist();

                        FileUtils.CreateDirectoryIfNotExists(exportDimrDirPath);
                        Assert.IsTrue(Directory.Exists(exportDimrDirPath));

                        new DHydroConfigXmlExporter().Export(model, exportDimrFilePath);

                        app.CloseProject();

                        Assert.IsTrue(File.Exists(exportDimrFilePath));
                        var expectedFileCount = 1;
                        var actualFileCount = Directory.GetFiles(exportDimrDirPath, $"*{expectedExtension}").Length;
                        Assert.AreEqual(expectedFileCount, actualFileCount,
                            Message_WrongNumberOfFilesOrFolders(expectedFileCount, "folders", expectedExtension,
                                actualFileCount, exportDimrDirPath));
                        AssertDflowfmDirectoryExists();
                        AssertFilesExtensionsExistInDirectory(filtersInputWithTrachytopes.Except(new[] { ".meta" }),
                            dflowfmDirPath);
                    }
                }
            }
            finally
            {
                DeleteTestDirectories();
            }
        }

        [Test]
        //3.2
        public void GivenAnFMModelWithMorphology_WhenDimrExported_ThenCorrectFoldersAndFilesAreGiven()
        {
            CreateTestDirectories();

            var expectedExtension = ".xml";

            try
            {
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

                    using (var model = new WaterFlowFMModel(tempMduFilePath))
                    {
                        AdjustSettingsOutputParameters(model);
                        UpdateBedLevel(model);
                        AddModelToProject(model, app);

                        app.SaveProjectAs(projectFilePath);

                        AssertProjectFileAndFolderExist();

                        FileUtils.CreateDirectoryIfNotExists(exportDimrDirPath);
                        Assert.IsTrue(Directory.Exists(exportDimrDirPath));

                        new DHydroConfigXmlExporter().Export(model, exportDimrFilePath);

                        app.CloseProject();

                        Assert.IsTrue(File.Exists(exportDimrFilePath));
                        var expectedFileCount = 1;
                        var actualFileCount = Directory.GetFiles(exportDimrDirPath, $"*{expectedExtension}").Length;
                        Assert.AreEqual(expectedFileCount, actualFileCount,
                            Message_WrongNumberOfFilesOrFolders(expectedFileCount, "folders", expectedExtension,
                                actualFileCount, exportDimrDirPath));
                        AssertDflowfmDirectoryExists();
                        AssertFilesExtensionsExistInDirectory(filtersInputWithMorphology.Except(new[] { ".meta" }),
                            dflowfmDirPath);
                    }
                }
            }

            finally
            {
                DeleteTestDirectories();
            }
        }

        [Test]
        //3.3
        public void GivenAnFMModelWithWind_WhenDimrExported_ThenCorrectFoldersAndFilesAreGiven()
        {
            CreateTestDirectories();

            var expectedExtension = ".xml";

            try
            {
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

                    using (var model = new WaterFlowFMModel(tempMduFilePath))
                    {
                        AdjustSettingsOutputParameters(model);
                        UpdateBedLevel(model);
                        AddModelToProject(model, app);

                        app.SaveProjectAs(projectFilePath);

                        AssertProjectFileAndFolderExist();

                        FileUtils.CreateDirectoryIfNotExists(exportDimrDirPath);
                        Assert.IsTrue(Directory.Exists(exportDimrDirPath));

                        new DHydroConfigXmlExporter().Export(model, exportDimrFilePath);

                        Assert.IsTrue(File.Exists(exportDimrFilePath));
                        var expectedFileCount = 1;
                        var actualFileCount = Directory.GetFiles(exportDimrDirPath, $"*{expectedExtension}").Length;
                        Assert.AreEqual(expectedFileCount, actualFileCount,
                            Message_WrongNumberOfFilesOrFolders(expectedFileCount, "folders", expectedExtension,
                                actualFileCount, exportDimrDirPath));
                        AssertDflowfmDirectoryExists();
                        AssertFilesExtensionsExistInDirectory(filtersInputWithWind.Except(new[] { ".meta" }),
                            dflowfmDirPath);

                        app.CloseProject();
                    }
                }
            }

            finally
            {
                DeleteTestDirectories();
            }
        }

        [Test]
        //4.1
        public void GivenAnFMModelWithTrachytopesThatRunsAndSaves_WhenUserClearsOutputAndSaves_ThenOutputFolderShouldBePresentButEmpty()
        {
            CreateTestDirectories();

            try
            {
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

                    using (var model = new WaterFlowFMModel(tempMduFilePath))
                    {
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
            finally
            {
                DeleteTestDirectories();
            }
        }

        [TestCase(TrachytopesModelProjectDirName)]
        [TestCase("TestModel")]
        //[TestCase(NoordzeeModelProjectDirName)] JIRA issue: D3DFMIQ-627 | original files too big, need to be replaced before uncommenting
        //5.1 & 5.2
        public void GivenAnFMModelWithInputAndOutput_WhenOpeningTheProject_ThenDirectoryStructureShouldBeMigratedToNewVersion(
            string projectFolder)
        {
            List<string> filtersInput;
            if (projectFolder == TrachytopesModelProjectDirName) filtersInput = filtersInputWithTrachytopes;
            if (projectFolder == NoordzeeModelProjectDirName) filtersInput = filtersInputWithWind;
            else filtersInput = filtersCommonInput;

            FileUtils.CreateDirectoryIfNotExists(destinationDirPath);

            try
            {
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
                    var directoriesInOutputFolder = Directory.GetDirectories(outputDirPath);
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
            finally
            {
                FileUtils.DeleteIfExists(destinationDirPath);
            }
        }

        [Test]
        // 7
        public void GivenAnFMModelWithTrachytopesAfterARun_WhenSavingItAndRenamingItAndSavingItAgain_OnlyFolderNameOfModelIsChangedAndTheMDUFileName()
        {
            CreateTestDirectories();

            try
            {
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

                    using (var model = new WaterFlowFMModel(tempMduFilePath))
                    {
                        AdjustSettingsOutputParameters(model);
                        UpdateBedLevel(model);
                        AddModelToProject(model, app);

                        app.SaveProjectAs(projectFilePath);

                        model.ValidateBeforeRun = true;
                        var report = model.Validate();
                        Assert.AreEqual(0, report.AllErrors.Count(),
                            "There are errors in the model after importing the MDU file");
                        app.RunActivity(model);
                        Assert.AreEqual(ActivityStatus.Cleaned, model.Status);

                        app.SaveProject();

                        var projectFolderBeforeRename =
                            GetDirectoryStructure(Path.Combine(projectFilePath, modelDirPath), ".",
                                "mdu");

                        //Rename
                        model.Name = "FlowFM2";

                        app.SaveProject();

                        app.CloseProject();

                        AssertProjectFileAndFolderExist();

                        //MDU file name check
                        var newModelDirPath = Path.Combine(projectDirPath, "FlowFM2");
                        Assert.IsTrue(Directory.Exists(newModelDirPath),
                            Message_MissingFileOrFolderName("folder", "FlowFM2", projectDirName));
                        Assert.IsFalse(Directory.Exists(modelDirPath),
                            Message_MissingFileOrFolderName("folder", "FlowFM2", projectDirName));
                        var newInputDirPath = Path.Combine(newModelDirPath, InputDirName);
                        var newMduFileNameWithoutExtension =
                            Path.GetFileNameWithoutExtension(Directory.GetFiles(newInputDirPath, $"*{".mdu"}")[0]);
                        Assert.AreEqual("FlowFM2", newMduFileNameWithoutExtension);

                        var projectFolderAfterRename =
                            GetDirectoryStructure(Path.Combine(projectFilePath, newModelDirPath), ".",
                                "mdu");
                        AssertEqualDirectoryStructure(".", ref projectFolderBeforeRename, ref projectFolderAfterRename);
                    }
                }
            }
            finally
            {
                DeleteTestDirectories();
            }
        }

        [Test]
        //8.1
        public void GivenAnFMModelWithTrachytopes_WhenRunAndProjectSavedAsInAnotherDirectory_ThenInputFolderWithCorrectFilesAreGiven()
        {
            CreateTestDirectories();

            try
            {
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


                    using (var model = new WaterFlowFMModel(tempMduFilePath))
                    {
                        AdjustSettingsOutputParameters(model);
                        UpdateBedLevel(model);
                        AddModelToProject(model, app);

                        app.SaveProjectAs(projectFilePath);

                        model.ValidateBeforeRun = true;
                        var report = model.Validate();
                        Assert.AreEqual(0, report.AllErrors.Count(),
                            "There are errors in the model after importing the MDU file");
                        app.RunActivity(model);
                        Assert.AreEqual(ActivityStatus.Cleaned, model.Status);

                        app.SaveProject();

                        AssertProjectFileAndFolderExist();
                        AssertModelDirectoryExists();
                        AssertInputDirectoryExists();

                        var projectDirStructureBeforeSaveAs =
                            GetDirectoryStructure(projectDirPath, ".", doChecksum: true);

                        var newSaveAsDestinationDirPath = Path.Combine(Path.GetTempPath(),
                            Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
                        var newSaveAsProjectFilePath = Path.Combine(newSaveAsDestinationDirPath, projectFileName);

                        app.SaveProjectAs(newSaveAsProjectFilePath);

                        app.CloseProject();
                        var projectDirStructureAfterSaveAs =
                            GetDirectoryStructure(projectDirPath, ".", doChecksum: true);
                        var newDirStructureAfterSaveAs = GetDirectoryStructure(
                            Path.Combine(newSaveAsDestinationDirPath, ProjectName + ProjectDirExtension), ".",
                            doChecksum: true);

                        // Since we asserted that everything is correct before we started, if the structure is the same, it should still be correct after.
                        AssertEqualDirectoryStructure(".", ref projectDirStructureBeforeSaveAs,
                            ref projectDirStructureAfterSaveAs, true);
                        AssertEqualDirectoryStructure(".", ref projectDirStructureAfterSaveAs,
                            ref newDirStructureAfterSaveAs, true);
                    }
                }
            }
            finally
            {
                DeleteTestDirectories();
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

                    var integratedModel = app.GetAllModelsInProject().FirstOrDefault(m => m is IHydroModel);
                    Assert.NotNull(integratedModel, "Expected: one integrated model (hydromodel) in the project.");
                    var fmModel = (WaterFlowFMModel)app.GetAllModelsInProject().FirstOrDefault(m => m is WaterFlowFMModel);
                    Assert.NotNull(fmModel, "Expected: one waterflow fm model in the project.");

                    app.RunActivity(integratedModel);

                    // Get directory structure of output in working directory before saving
                    var outputWorkingDirectoryPath = Path.Combine(integratedModel.ExplicitWorkingDirectory,
                        DflowfmDirName, OutputDirName);
                    var outputWorkingDirStructure = GetDirectoryStructure(outputWorkingDirectoryPath, ".", doChecksum: true);

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
                    var outputPersistentDirStructure = GetDirectoryStructure(outputDirPath, ".", doChecksum: true);

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
        ///   AND a project
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
            var exportPath = Path.Combine(destinationDirPath, "exportPath");
            FileUtils.CreateDirectoryIfNotExists(exportPath);

            var newSavePath = Path.Combine(destinationDirPath, "newSaveLocation");
            FileUtils.CreateDirectoryIfNotExists(newSavePath);

            try
            {
                CopyProjectToDestinationDir("TestModel");

                using (var app = GetConfiguredApplication())
                {
                    // Given
                    CreateExportedFmModel(app, exportPath, out var fmModel);
                    var exportFilePath = Path.Combine(exportPath,
                                                      Path.GetFileName(fmModel.MduFilePath));

                    var preImportExportDirStructure = GetDirectoryStructure(exportPath, ".", doChecksum: true);

                    var originalInputFolder = Path.Combine(projectDirPath, fmModel.Name, "input");
                    var originalInputDirStructure = GetDirectoryStructure(originalInputFolder, ".");

                    // When
                    app.CreateNewProject();
                    var relevantImporter = app.FileImporters
                                              .FirstOrDefault(importer => importer is WaterFlowFMFileImporter);

                    var importedModel = relevantImporter.ImportItem(exportFilePath) as WaterFlowFMModel;
                    Assert.That(importedModel, Is.Not.Null,
                                "Expected the imported model to exist.");
                    AddModelToProject(importedModel, app);

                    var newSaveFilePath = Path.Combine(newSavePath, "Project1.dsproj");
                    app.SaveProjectAs(newSaveFilePath);

                    // Then
                    // The import location does not change.
                    var postImportExportDirStructure = GetDirectoryStructure(exportPath, ".", doChecksum: true);
                    AssertEqualDirectoryStructure(".",
                                                  ref preImportExportDirStructure,
                                                  ref postImportExportDirStructure,
                                                  true);

                    // a new model is created in the project folder
                    var newDsProjData = Path.Combine(newSavePath, "Project1.dsproj_data");
                    var projectDirSubFolders =
                        Directory.EnumerateDirectories(newDsProjData).ToList();

                    Assert.That(projectDirSubFolders.Count, Is.EqualTo(2),
                                $"Expected two folders in {newDsProjData}: '{ModelName}' and '{ModelName + StateFilesDirectoryPostfix}'.");

                    var modelPath = Path.Combine(newDsProjData, fmModel.Name);
                    Assert.That(projectDirSubFolders.First(), Is.EqualTo(modelPath),
                        "Expected a different model folder name.");

                    AssertInputImportedModelIsCorrect(modelPath,
                                                      originalInputDirStructure);

                    importedModel.Dispose();
                    fmModel.Dispose();
                }
            }
            finally
            {
                DeleteTestDirectories();
                FileUtils.DeleteIfExists(exportPath);
                FileUtils.DeleteIfExists(newSavePath);
            }
        }

        /// <summary>
        /// GIVEN an exported WaterFlowFMModel
        ///   AND an Integrated Model
        /// WHEN the exported model is imported in to the Integrated Model
        /// THEN a new model is created in the integrated model
        ///  AND the input files are copied into this project folder
        ///  AND no output files are copied
        ///  AND the import location does not change
        /// </summary>
        [Test]
        public void GivenAnExportedWaterFlowFMModelAndAnIntegratedModel_WhenTheExportedModelIsImportedInToTheIntegratedModel_ThenANewModelIsCreatedInTheIntegratedModelWithTheCorrectStructure()
        {
            CreateTestDirectories();
            var exportPath = Path.Combine(destinationDirPath, "exportPath");
            FileUtils.CreateDirectoryIfNotExists(exportPath);

            var newSavePath = Path.Combine(destinationDirPath, "newSaveLocation");
            FileUtils.CreateDirectoryIfNotExists(newSavePath);

            try
            {
                CopyProjectToDestinationDir("TestModel");

                using (var app = GetConfiguredApplication())
                {
                    // Given
                    CreateExportedFmModel(app, exportPath, out var fmModel);
                    var exportFilePath = Path.Combine(exportPath,
                                                      Path.GetFileName(fmModel.MduFilePath));

                    var preImportExportDirStructure = GetDirectoryStructure(exportPath, ".", doChecksum: true);

                    var originalInputFolder = Path.Combine(projectDirPath, fmModel.Name, "input");
                    var originalInputDirStructure = GetDirectoryStructure(originalInputFolder,
                                                                          ".",
                                                                          "meta"); // The restart.meta file is ignored, as it should not be present in the HydroModel.

                    // When
                    app.CreateNewProject();
                    var newIntegratedModel = new HydroModel()
                    {
                        Name = "Blastoise"
                    };

                    AddModelToProject(newIntegratedModel, app);

                    var relevantImporter = app.FileImporters
                                              .FirstOrDefault(importer => importer is WaterFlowFMFileImporter);

                    relevantImporter.ImportItem(exportFilePath, newIntegratedModel);

                    var newSaveFilePath = Path.Combine(newSavePath, "Project1.dsproj");
                    app.SaveProjectAs(newSaveFilePath);

                    // Then
                    // A new model is created in the integrated model
                    var integratedModelActivities = newIntegratedModel.Activities.ToList();
                    Assert.That(integratedModelActivities.Count, Is.EqualTo(1), "Expected the integrated model to contain a single activity.");
                    Assert.That(integratedModelActivities.First().Name, Is.EqualTo(fmModel.Name), "Expected the imported model name to be equal to the exported model name.");

                    // The import location does not change.
                    var postImportExportDirStructure = GetDirectoryStructure(exportPath, ".", doChecksum: true);
                    AssertEqualDirectoryStructure(".",
                                                  ref preImportExportDirStructure,
                                                  ref postImportExportDirStructure,
                                                  true);

                    // a new model is created in the project folder
                    var newDsProjData = Path.Combine(newSavePath, "Project1.dsproj_data");
                    var fmModelPath = Path.Combine(newDsProjData, fmModel.Name);
                    var hydroModelPath = Path.Combine(newDsProjData, newIntegratedModel.Name);

                    Assert.That(Directory.Exists(newDsProjData), "Expected the dsproj_data folder to exist.");
                    Assert.That(Directory.Exists(fmModelPath), "Expected the D-FlowFM model folder to exist.");
                    Assert.That(Directory.Exists(hydroModelPath), "Expected the Integrated Model folder to exist.");

                    foreach (var subDirectory in Directory.EnumerateDirectories(newDsProjData))
                    {
                        var dirName = new DirectoryInfo(subDirectory).Name;
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
            finally
            {
                DeleteTestDirectories();
                FileUtils.DeleteIfExists(exportPath);
                FileUtils.DeleteIfExists(newSavePath);
            }
        }

        private static void CreateExportedFmModel(IApplication app, 
                                                  string exportPath, 
                                                  out WaterFlowFMModel fmModel)
        {
            var hasOpened = app.OpenProject(projectFilePath);
            Assert.That(hasOpened, Is.True,
                        $"Could not open project at {projectFilePath}");

            fmModel = app.GetAllModelsInProject()
                         .FirstOrDefault(item => item is WaterFlowFMModel) as WaterFlowFMModel;
            Assert.That(fmModel, Is.Not.Null,
                        "Expected the FlowFM Model to be not null.");

            var exportFilePath = Path.Combine(exportPath,
                                              Path.GetFileName(fmModel.MduFilePath));
            var relevantExporter = app.FileExporters
                                      .FirstOrDefault(exporter => exporter is WaterFlowFMFileExporter);
            Assert.That(relevantExporter, Is.Not.Null,
                        "Expected the app to contain a WaterFlowFMMileExporter.");

            var hasExported = relevantExporter.Export(fmModel, exportFilePath);
            Assert.That(hasExported, Is.True,
                        "Expected the export to succeed.");

            app.CloseProject();
        }

        private static void AssertInputImportedModelIsCorrect(string fmModelPath,
                                                              Dictionary<string, Tuple<Tuple<string, string>[], string[]>> originalInputDirStructure)
        {
            var modelDirSubFolders = Directory.EnumerateDirectories(fmModelPath).ToList();

            Assert.That(modelDirSubFolders.Count, Is.EqualTo(1),
                        $"Expected only one folder in {fmModelPath}.");

            var inputDirPath = Path.Combine(fmModelPath, "input");
            Assert.That(modelDirSubFolders.First(), Is.EqualTo(inputDirPath),
                        "Expected only an input folder in the model folder.");

            var modelDirFiles = Directory.EnumerateFiles(fmModelPath);
            Assert.That(modelDirFiles.Any(), Is.False, $"Expected no files in {fmModelPath}");

            // the input files are copied into this project folder
            var newInputDirStructure = GetDirectoryStructure(inputDirPath, ".");

            AssertEqualDirectoryStructure(".",
                                          ref originalInputDirStructure,
                                          ref newInputDirStructure);
        }
        
        private static void AssertCorrectOutputIsLinked(string outputDirectoryPath, WaterFlowFMModel fmModel)
        {
            Assert.AreEqual(Path.Combine(outputDirectoryPath, "FlowFM1_map.nc"), fmModel.OutputMapFileStore.Path);
            Assert.AreEqual(Path.Combine(outputDirectoryPath, "FlowFM1_his.nc"), fmModel.OutputHisFileStore.Path);
        }

        private DeltaShellApplication GetConfiguredApplication()
        {
            var applicationSettingsMock = MockRepository.GenerateStub<ApplicationSettingsBase>();
            applicationSettingsMock["WorkDirectory"] = workingDirPath;

            applicationSettingsMock.Replay();

            var app = new DeltaShellApplication
            {
                UserSettings = applicationSettingsMock,
                IsProjectCreatedInTemporaryDirectory = true
            };

            AddPluginsToApplication(app);
            app.Run();
            return app;
        }

        private DeltaShellApplication GetConfiguredHydroApplication()
        {
            var applicationSettingsMock = MockRepository.GenerateStub<ApplicationSettingsBase>();
            applicationSettingsMock["WorkDirectory"] = workingDirPath;

            applicationSettingsMock.Replay();

            var app = new DeltaShellApplication
            {
                UserSettings = applicationSettingsMock,
                IsProjectCreatedInTemporaryDirectory = true
            };
            
            AddPluginsToApplication(app);
            app.Plugins.Add(new HydroModelApplicationPlugin());
            app.Run();
            return app;
        }

        private static void AddPluginsToApplication(DeltaShellApplication app)
        {
            app.Plugins.Add(new NHibernateDaoApplicationPlugin());
            app.Plugins.Add(new CommonToolsApplicationPlugin());
            app.Plugins.Add(new SharpMapGisApplicationPlugin());
            app.Plugins.Add(new FlowFMApplicationPlugin());
            app.Plugins.Add(new NetworkEditorApplicationPlugin());
        }

        private void AddFeaturesToModel(WaterFlowFMModel model)
        {
            model.Grid = CreateNewGrid(5, 10);
            model.Area.LandBoundaries.Add(CreateLandBoundary());
            model.Area.DryAreas.Add(CreateDryArea());
            model.Area.DryPoints.Add(CreateDryPoints());
            model.Area.ObservationPoints.Add(CreateObservationPoint());
            model.Area.Weirs.Add(CreateWeir());
            model.Area.FixedWeirs.Add(CreateFixedWeir());
            model.Area.ObservationCrossSections.Add(CreateObservationCrossSection());
            AddFlowBoundaryConditionToModel(model);
            AddTracer(model);
        }

        private static void CopyTestDataFileToTemp(string fileName)
        {
            var sourceFilePath = Path.Combine(testDataDirPath, fileName);
            var targetFilePath = Path.Combine(tempDirPath, fileName);
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
            var sourceFilePath = Path.Combine(testDataDirPath, folderName);

            var sourceDirectory = new DirectoryInfo(sourceFilePath);

            sourceDirectory.GetFiles().ForEach(f =>
            {
                var targetFilePath = Path.Combine(destinationDirPath, f.Name);
                FileUtils.CopyFile(f.FullName, targetFilePath);
            });
            sourceDirectory.GetDirectories().ForEach(d =>
            {
                var targetDirPath = Path.Combine(destinationDirPath, d.Name);
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

            var numberOfCoordinates = width * length;
            for (var n = 0; n < length; n++)
            {
                for (var m = 0; m < width; m++)
                {
                    grid.Vertices.Add(new Coordinate(m, n, -1));
                }
            }

            for (var i = 0; i < numberOfCoordinates; i++)
            {
                if ((i + 1) % width != 0) grid.Edges.Add(new Edge(i, i + 1));
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
                Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(0, 100), new Coordinate(50, 50) })
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

        private GroupablePointFeature CreateDryPoints()
        {
            return new GroupablePointFeature
            {
                GroupName = "DryPoints",
                Geometry = new Point(new Coordinate(0, 100))
            };
        }

        private GroupableFeature2DPoint CreateObservationPoint()
        {
            return new GroupableFeature2DPoint
            {
                Geometry = new Point(5, 5),
                Name = "ObservationPoint"
            };
        }

        private Weir2D CreateWeir()
        {
            Coordinate[] lineString =
            {
                new Coordinate(7, 7),
                new Coordinate(8, 8)
            };

            return new Weir2D
            {
                Name = "Weir",
                WeirFormula = new SimpleWeirFormula(),
                Geometry = new LineString(lineString)
            };
        }

        private FixedWeir CreateFixedWeir()
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

        private ObservationCrossSection2D CreateObservationCrossSection()
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
                .SetValueAsString(((int)HeatFluxModelType.TransportOnly).ToString());
        }

        private void AddFlowBoundaryConditionToModel(WaterFlowFMModel model)
        {
            var feature = new Feature2D
            {
                Name = "Boundary1",
                Geometry =
                    new LineString(new[] { new Coordinate(0, 0), new Coordinate(1, 0) })
            };

            var flowBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.Discharge,
                BoundaryConditionDataType.TimeSeries)
            {
                Feature = feature
            };

            flowBoundaryCondition.AddPoint(0);
            flowBoundaryCondition.PointData[0].Arguments[0].SetValues(new[] { model.StartTime, model.StopTime });
            flowBoundaryCondition.PointData[0][model.StartTime] = 0.5;
            flowBoundaryCondition.PointData[0][model.StopTime] = 0.6;

            var set = new BoundaryConditionSet { Feature = feature };
            set.BoundaryConditions.Add(flowBoundaryCondition);
            model.BoundaryConditionSets.Add(set);
        }

        private void AddTracer(WaterFlowFMModel model)
        {
            var tracer = "Tracer1";
            model.TracerDefinitions.AddRange(new List<string> { tracer });

            var boundary = new Feature2D
            {
                Name = "TracerBoundary1",
                Geometry =
                    new LineString(new[] { new Coordinate(0, 0), new Coordinate(1, 0) })
            };
            var set01 = new BoundaryConditionSet
            {
                Feature = boundary
            };
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
            var cellsValue = ((int)UnstructuredGridFileHelper.BedLevelLocation.Faces).ToString();
            model.ModelDefinition.GetModelProperty(KnownProperties.BedlevType).SetValueAsString(cellsValue);
        }

        private void AddMorphologyBoundaryConditionToModel(WaterFlowFMModel model)
        {
            var feature = new Feature2D
            {
                Name = "Boundary2",
                Geometry = new LineString(new[] { new Coordinate(1, 0), new Coordinate(0, 1) })
            };

            var morphologyBoundaryCondition = new FlowBoundaryCondition(
                FlowBoundaryQuantityType.MorphologyBedLevelPrescribed,
                BoundaryConditionDataType.TimeSeries)
            {
                Feature = feature,
                SedimentFractionNames = new List<string> { "SedimentFraction1" }
            };

            morphologyBoundaryCondition.AddPoint(0);
            morphologyBoundaryCondition.PointData[0].Arguments[0].SetValues(new[] { model.StartTime, model.StopTime });
            morphologyBoundaryCondition.PointData[0][model.StartTime] = 0.5;
            morphologyBoundaryCondition.PointData[0][model.StopTime] = 0.6;

            var flowBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel,
                BoundaryConditionDataType.TimeSeries)
            {
                Feature = feature
            };

            flowBoundaryCondition.AddPoint(0);
            flowBoundaryCondition.PointData[0].Arguments[0].SetValues(new[] { model.StartTime, model.StopTime });
            flowBoundaryCondition.PointData[0][model.StartTime] = 0.5;
            flowBoundaryCondition.PointData[0][model.StopTime] = 0.6;

            var set = new BoundaryConditionSet { Feature = feature };
            set.BoundaryConditions.Add(flowBoundaryCondition);
            set.BoundaryConditions.Add(morphologyBoundaryCondition);

            model.BoundaryConditionSets.Add(set);
        }

        private void AddSedimentFraction(WaterFlowFMModel model)
        {
            model.SedimentFractions = new EventedList<ISedimentFraction>();
            model.SedimentFractions.Add(new SedimentFraction { Name = "SedimentFraction2" });
        }

        private static void AddWindToModel(WaterFlowFMModel model)
        {
            var windFilePath = Path.Combine(tempDirPath, WindFileName);
            var gridFilePath = Path.Combine(tempDirPath, GridFileName);
            var windField = GriddedWindField.CreateCurviField(windFilePath, gridFilePath);
            model.ModelDefinition.WindFields.Add(windField);
        }

        private void SimulateUserAddingReferencesInMduFile()
        {
            using (var sw = File.AppendText(tempMduFilePath))
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
            model.ModelDefinition.GetModelProperty(KnownProperties.TrtRou).SetValueAsString("Y");
            model.ModelDefinition.GetModelProperty(KnownProperties.TrtDef).SetValueAsString("trachytopes.ttd");
            model.ModelDefinition.GetModelProperty(KnownProperties.TrtL).SetValueAsString("trachytopes.arl");
            model.ModelDefinition.GetModelProperty(KnownProperties.DtTrt).SetValueAsString("300");
        }

        private void AdjustSettingsOutputParameters(WaterFlowFMModel model)
        {
            model.ModelDefinition.WriteSnappedFeatures = true;
            model.ModelDefinition.GetModelProperty(GuiProperties.SpecifyWaqOutputInterval).Value = true;
            model.ModelDefinition.GetModelProperty(GuiProperties.SpecifyWaqOutputStartTime).Value = true;
            model.ModelDefinition.GetModelProperty(GuiProperties.SpecifyWaqOutputStopTime).Value = true;
            model.ModelDefinition.GetModelProperty("Writebalancefile").Value = true;
            model.ModelDefinition.GetModelProperty(GuiProperties.WaqOutputDeltaT).Value =
                new TimeSpan(0, 0, 10, 0);
        }

        private void UpdateBedLevel(WaterFlowFMModel model)
        {
            TypeUtils.CallPrivateMethod(model, "UpdateBathymetryCoverage",
                UnstructuredGridFileHelper.BedLevelLocation.NodesMinLev);
        }

        private static void ValidateAndRunModel(WaterFlowFMModel model, DeltaShellApplication app)
        {
            model.ValidateBeforeRun = true;
            var report = model.Validate();
            Assert.AreEqual(0, report.AllErrors.Count(), "There are errors in the model after importing the MDU file");
            app.RunActivity(model);
            Assert.AreEqual(ActivityStatus.Cleaned, model.Status);
        }

        private static void AddModelToProject(IHydroModel model, IApplication app)
        {
            var project = app.Project;
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
            var subDirectories = Directory.GetDirectories(destinationDirPath);
            Assert.NotNull(subDirectories);
            Assert.AreEqual(1, subDirectories.Length,
                "There should only be one folder: the project (.dsproj_data) folder.");

            var subFiles = Directory.GetFiles(destinationDirPath);
            Assert.NotNull(subFiles);
            Assert.AreEqual(1, subFiles.Length, "There should only be one file: the project (.dsproj) file.");

            var expectedCount = 1;

            // Get all files/folders with the project extension
            var actualFileCount = Directory.GetFiles(destinationDirPath, $"*{ProjectFileExtension}").Length;
            var actualFolderCount = Directory.GetDirectories(destinationDirPath, $"*{ProjectDirExtension}").Length;

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
            var parentName = dirInfo.Parent.Name;

            Assert.IsTrue(dirInfo.Exists,
                Message_MissingFileOrFolderName("folder", dirInfo.Name, parentName));
        }

        private void AssertWorkingOutputDirectoryWithSubFoldersExists(string workingOutputDirPath)
        {
            var workingOutputFMDirPath = workingOutputDirPath;
            var workingOutputWAQDirPath = Path.Combine(workingOutputDirPath, outputWAQDirName);
            var workingSnappedDirPath = Path.Combine(workingOutputDirPath, SnappedDirName);
            
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
            string ignoreFileExtension = "",
            bool doChecksum = false)
        {
            var dirStructure = new Dictionary<string, Tuple<Tuple<string, string>[], string[]>>();
            GetDirectoryStructure(basePath, relativePath, ref dirStructure, ignoreFileExtension, doChecksum);
            return dirStructure;
        }

        private static void GetDirectoryStructure(
            string basePath, string relativePath,
            ref Dictionary<string, Tuple<Tuple<string, string>[], string[]>> structure,
            string ignoreFileExtension = "",
            bool doChecksum = false)
        {
            var currentPath = Path.Combine(basePath, relativePath);

            // Get relevant data
            var files = Directory.GetFiles(currentPath);
            var subdirs = Directory.GetDirectories(currentPath);

            var filesList = new List<Tuple<string, string>>();

            // Format relevant data
            for (var i = 0; i < files.Length; i++)
            {
                var checksum = doChecksum ? FileUtils.GetChecksum(files[i]) : "";

                files[i] = Path.GetFileName(files[i]);
                if (ignoreFileExtension == "" || !files[i].Contains(ignoreFileExtension))
                    filesList.Add(new Tuple<string, string>(files[i], checksum));
            }

            var filesAndChecksums = filesList.ToArray();

            Array.Sort(files, StringComparer.InvariantCultureIgnoreCase);

            for (var i = 0; i < subdirs.Length; i++)
            {
                subdirs[i] = Path.Combine(relativePath, new DirectoryInfo(subdirs[i]).Name);
            }

            Array.Sort(subdirs, StringComparer.InvariantCultureIgnoreCase);

            // Add to structure
            structure.Add(relativePath, new Tuple<Tuple<string, string>[], string[]>(filesAndChecksums, subdirs));

            foreach (var s in subdirs)
            {
                GetDirectoryStructure(basePath, s, ref structure, ignoreFileExtension, doChecksum);
            }
        }

        private static void AssertEqualDirectoryStructure(string curDir,
            ref Dictionary<string, Tuple<Tuple<string, string>[], string[]>> sourceDirStructure,
            ref Dictionary<string, Tuple<Tuple<string, string>[], string[]>> targetDirStructure,
            bool doCompareChecksums = false)
        {
            var sourceDirFilesAndChecksums = sourceDirStructure[curDir].Item1;
            var sourceDirSubDirectories = sourceDirStructure[curDir].Item2;

            var targetDirFilesAndChecksums = targetDirStructure[curDir].Item1;
            var targetDirSubDirectories = targetDirStructure[curDir].Item2;


            //First check if the number of files/directories are the same
            Assert.AreEqual(sourceDirFilesAndChecksums.Length, targetDirFilesAndChecksums.Length,
                $"The number of files in source and target {curDir} do not correspond.");
            Assert.AreEqual(sourceDirSubDirectories.Length, targetDirSubDirectories.Length,
                $"The number of subfolders in source and target {curDir} do not correspond.");

            //If the number of files are correct, then check the names of them.
            for (var i = 0; i < targetDirFilesAndChecksums.Length; i++)
            {
                // Compare strings: source > target -> source is missing a file, else target missing a file.
                var assertMsgNotEqualName =
                    string.Compare(sourceDirFilesAndChecksums[i].Item1, targetDirFilesAndChecksums[i].Item1,
                        StringComparison.InvariantCultureIgnoreCase) > 0
                        ? $"File {targetDirFilesAndChecksums[i].Item1} does not exist in source {curDir}."
                        : $"File {sourceDirFilesAndChecksums[i].Item1} does not exist in target {curDir}.";
                Assert.AreEqual(sourceDirFilesAndChecksums[i].Item1, targetDirFilesAndChecksums[i].Item1,
                    assertMsgNotEqualName);

                if (!doCompareChecksums) continue;
                var assertMsgNotEqualContent =
                    $"Checksum of file {sourceDirFilesAndChecksums[i].Item1} does not match checksum of file {targetDirFilesAndChecksums[i].Item1} in {curDir}";

                // time is written in mdu
                if (sourceDirFilesAndChecksums[i].Item1 != ModelName + ".mdu")
                    Assert.That(sourceDirFilesAndChecksums[i].Item2, Is.EqualTo(targetDirFilesAndChecksums[i].Item2),
                        assertMsgNotEqualContent);
            }

            // If the number of directories are correct, then check the names of them.
            for (var i = 0; i < sourceDirSubDirectories.Length; i++)
            {
                var assertMsg =
                    string.Compare(sourceDirSubDirectories[i], targetDirSubDirectories[i],
                        StringComparison.InvariantCultureIgnoreCase) > 0
                        ? $"Folder {targetDirSubDirectories[i]} does not exist in source {curDir}."
                        : $"Folder {sourceDirSubDirectories[i]} does not exist in target {curDir}.";
                Assert.AreEqual(sourceDirSubDirectories[i], targetDirSubDirectories[i], assertMsg);
            }

            // Continue the process for the subfolders.
            foreach (var s in sourceDirSubDirectories)
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