using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Shell.Core;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DeltaShell.Core;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.SharpMapGis;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Grids;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using SharpMap.Extensions.CoordinateSystems;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using FixedWeir = DelftTools.Hydro.Structures.FixedWeir;
using LandBoundary2D = DelftTools.Hydro.LandBoundary2D;
using ObservationCrossSection2D = DelftTools.Hydro.ObservationCrossSection2D;
using TimeSpan = System.TimeSpan;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [ExcludeFromCodeCoverage]
    [Ignore]
    [TestFixture]
    public class WaterFlowFmModelDirectoryStructureTest
    {
        private const string TestDataDirName = "TestPlanFM";
        private const string ProjectFileExtension = ".dsproj";
        private const string ProjectDirExtension = ".dsproj_data";

        private const string InputDirName = "input";
        private const string OutputDirName = "output";
        private const string SnappedDirName = "snapped";
        private const string Dfm_Output_WaqDirName = "DFM_OUTPUT_WAQ";
        private const string DflowfmDirName = "dflowfm";

        private const string ModelName = "FlowFM1";
        private const string ProjectName = "Project1";

        private const string WindFileName = "windfield.apwxwy";
        private const string GridFileName = "windfield.grd";

        private static string testDataDirPath;

        private static string workDirPath;
        private static string tempDirPath;

        private static string projectDirName;
        private static string projectDirPath;

        private static string projectFileName;
        private static string projectFilePath;

        private static string modelDirName;
        private static string modelDirPath;

        private static string inputDirPath;
        private static string outputDirPath;

        private static string tempMduFilePath;
        private static string tempProjectFilePath;
        private static string exportMduFilePath;

        private static string dfm_Output_WaqDirPath;
        private static string snappedDirPath;
        private static string dflowfmDirPath;

        private static string mduFileName;

        [TestFixtureSetUp]
        public void Setup()
        {
            // Get TestData Directory
            testDataDirPath = Path.Combine(TestHelper.GetDataDir(), TestDataDirName);

            // Create work directory in Temp
            workDirPath = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));

            // Create extra directory to copy testdata to so that workdirectory is not "contaminated" with other files.
            tempDirPath = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
            

            // Set the rest of the expected paths
            projectFileName = ProjectName + ProjectFileExtension;
            projectFilePath = Path.Combine(workDirPath, projectFileName);
            projectDirName = ProjectName + ProjectDirExtension;
            projectDirPath = Path.Combine(workDirPath, projectDirName);

            modelDirName = ModelName;
            modelDirPath = Path.Combine(projectDirPath, modelDirName);
            mduFileName = ModelName + ".mdu";

            inputDirPath = Path.Combine(modelDirPath, InputDirName);
            outputDirPath = Path.Combine(modelDirPath, OutputDirName);
            dfm_Output_WaqDirPath = Path.Combine(outputDirPath, Dfm_Output_WaqDirName);
            snappedDirPath = Path.Combine(outputDirPath, SnappedDirName);
            dflowfmDirPath = Path.Combine(projectDirPath, DflowfmDirName);

            tempMduFilePath = Path.Combine(tempDirPath, mduFileName);
            tempProjectFilePath = Path.Combine(tempDirPath, projectFileName);
            exportMduFilePath = Path.Combine(projectDirPath, mduFileName);
        }

        [Test]
        //1.1
        public void GivenAnFMModelWithTrachytopes_WhenProjectSavedAs_ThenInputFolderWithCorrectFilesAreGiven()
        {
            CreateTestDirectories();

            var expectedFileExtensions = new List<string>
            {
                ".mdu",
                ".meta",
                "_net.nc",
                ".pol",
                ".pli",
                ".pliz",
                ".xyz",
                ".xyn",
                ".ini",
                ".fou",
                ".arl",
                ".ttd",
                ".bc",
                ".cld",
                ".cll",
                ".ext",
                ".ldb",
            };

            try
            {
                CopyTestDataToTemp();
                CopyTrachytopeDataToTemp();

                using (var app = GetConfiguredApplication())
                {
                    using (var model = new WaterFlowFMModel())
                    {
                        AddFeaturesToModel(model);
                        EnableSalinityAndTemperature(model);
                        model.ExportTo(tempMduFilePath);
                        model.ReloadGrid(true, true);
                    }

                    SimulateUserAddingReferencesInMduFile();
                    SimulateUserAddingTrachytopesInMduFile();

                    using (var model = new WaterFlowFMModel(tempMduFilePath))
                    {
                        AdjustSettingsOutputParameters(model);
                        UpdateBedLevel(model);
                        AddModelToProject(model, app);


                        app.SaveProjectAs(projectFilePath);

                        AssertProjectFileAndFolderExist();
                        AssertModelDirectoryExists();
                        AssertInputDirectoryExists();

                        AssertFilesExtensionsExistInDirectory(expectedFileExtensions, inputDirPath, InputDirName);

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
        //1.2
        public void GivenAnFMModelWithMorphology_WhenProjectSavedAs_ThenInputFolderWithCorrectFilesAreGiven()
        {
            CreateTestDirectories();

            var expectedFileExtensions = new List<string>
            {
                ".mdu",
                ".meta",
                "_net.nc",
                ".pol",
                ".pli",
                ".pliz",
                ".xyz",
                ".xyn",
                ".ini",
                ".fou",
                ".sed",
                ".mor",
                ".bc",
                ".bcm",
                ".cld",
                ".cll",
                ".ext",
                ".ldb"
            };

            try
            {
                CopyTestDataToTemp();

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
                        AssertModelDirectoryExists();
                        AssertInputDirectoryExists();

                        AssertFilesExtensionsExistInDirectory(expectedFileExtensions, inputDirPath, InputDirName);

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
        //1.3
        public void GivenAnFMModelWithWind_WhenProjectSavedAs_ThenInputFolderWithCorrectFilesAreGiven()
        {
            CreateTestDirectories();

            var expectedFileExtensions = new List<string>
            {
                ".mdu",
                ".meta",
                "_net.nc",
                ".pol",
                ".pli",
                ".pliz",
                ".xyz",
                ".xyn",
                ".ini",
                ".fou",
                ".grd",
                ".apwxwy",
                ".bc",
                ".cld",
                ".cll",
                ".ext",
                ".ldb"
            };

            try
            {
                CopyTestDataToTemp();
                CopyWindDataToTemp();

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
                        AssertModelDirectoryExists();
                        AssertInputDirectoryExists();
                        AssertOutputDirectoryExists();

                        AssertFilesExtensionsExistInDirectory(expectedFileExtensions, inputDirPath, InputDirName);

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
        //2.1
        public void GivenAnFMModelWithTrachytopes_WhenRun_ThenOutputFolderWithCorrectFilesAreGiven()
        {
            CreateTestDirectories();

            var expectedFileExtensions_Output = new List<string>
            {
                ".out",
                ".dia",
                ".nc",
                ".txt",
                ".tek",
                "_rst.nc",
                "_his.nc",
                "_map.nc"
            };

            var expectedFileExtensions_DFM_OUTPUT_Waq = new List<string>
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
                ".tem",
                ".vdf"
            };

            var expectedFileExtensions_Snapped = new List<string>
            {
                ".shp",
                ".dbf",
                ".shx"
            };

            try
            {
                CopyTestDataToTemp();
                CopyTrachytopeDataToTemp();

                using (var app = GetConfiguredApplication())
                {
                    using (var model = new WaterFlowFMModel())
                    {
                        AddFeaturesToModel(model);
                        EnableSalinityAndTemperature(model);
                        model.ExportTo(tempMduFilePath);
                        model.ReloadGrid(true, true);
                    }

                    SimulateUserAddingReferencesInMduFile();
                    SimulateUserAddingTrachytopesInMduFile();

                    using (var model = new WaterFlowFMModel(tempMduFilePath))
                    {
                        AdjustSettingsOutputParameters(model);
                        UpdateBedLevel(model);
                        AddModelToProject(model, app);

                        app.SaveProjectAs(projectFilePath);

                        app.SaveProject();

                        app.RunActivity(model);

                        AssertProjectFileAndFolderExist();
                        AssertModelDirectoryExists();

                        AssertOutputDirectoryExists();
                        AssertDfmOutputWaqDirectoryExists();
                        AssertSnappedDirectoryExists();

                        var directoriesInOutputFolder = Directory.GetDirectories(outputDirPath);
                        Assert.AreEqual(2, directoriesInOutputFolder.Length,
                            $"The number of folders in '{OutputDirName}' is not as expected.");

                        AssertFilesExtensionsExistInDirectory(expectedFileExtensions_Output, outputDirPath,
                            OutputDirName);
                        AssertFilesExtensionsExistInDirectory(expectedFileExtensions_DFM_OUTPUT_Waq,
                            dfm_Output_WaqDirPath, Dfm_Output_WaqDirName);
                        AssertFilesExtensionsExistInDirectory(expectedFileExtensions_Snapped, snappedDirPath,
                            SnappedDirName);

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
        //2.2
        public void GivenAnFMModelWithMorphology_WhenRun_ThenOutputFolderWithCorrectFilesAreGiven()
        {
            CreateTestDirectories();

            var expectedFileExtensions_Output = new List<string>
            {
                ".out",
                ".dia",
                ".nc",
                ".txt",
                ".tek",
                "_rst.nc",
                "_his.nc",
                "_map.nc"
            };

            var expectedFileExtensions_DFM_OUTPUT_Waq = new List<string>
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
                ".tem",
                ".vdf"
            };

            var expectedFileExtensions_Snapped = new List<string>
            {
                ".shp",
                ".dbf",
                ".shx"
            };

            try
            {
                CopyTestDataToTemp();

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

                        app.SaveProject();

                        app.RunActivity(model);

                        AssertProjectFileAndFolderExist();
                        AssertModelDirectoryExists();

                        AssertOutputDirectoryExists();
                        AssertDfmOutputWaqDirectoryExists();
                        AssertSnappedDirectoryExists();

                        var directoriesInOutputFolder = Directory.GetDirectories(outputDirPath);
                        Assert.AreEqual(2, directoriesInOutputFolder.Length,
                            $"The number of folders in '{OutputDirName}' is not as expected.");

                        AssertFilesExtensionsExistInDirectory(expectedFileExtensions_Output, outputDirPath,
                            OutputDirName);
                        AssertFilesExtensionsExistInDirectory(expectedFileExtensions_DFM_OUTPUT_Waq,
                            dfm_Output_WaqDirPath, Dfm_Output_WaqDirName);
                        AssertFilesExtensionsExistInDirectory(expectedFileExtensions_Snapped, snappedDirPath,
                            SnappedDirName);

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
        //2.3
        public void GivenAnFMModelWithWind_WhenRun_ThenOutputFolderWithCorrectFilesAreGiven()
        {
            CreateTestDirectories();

            var expectedFileExtensions_Output = new List<string>
            {
                ".out",
                ".dia",
                ".nc",
                ".txt",
                ".tek",
                "_rst.nc",
                "_his.nc",
                "_map.nc"
            };

            var expectedFileExtensions_DFM_OUTPUT_Waq = new List<string>
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
                ".tem",
                ".vdf"
            };

            var expectedFileExtensions_Snapped = new List<string>
            {
                ".shp",
                ".dbf",
                ".shx"
            };

            try
            {
                CopyTestDataToTemp();
                CopyWindDataToTemp();

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

                        app.SaveProject();

                        app.RunActivity(model);

                        AssertProjectFileAndFolderExist();
                        AssertModelDirectoryExists();
                        AssertInputDirectoryExists();

                        AssertDfmOutputWaqDirectoryExists();
                        AssertSnappedDirectoryExists();

                        var directoriesInOutputFolder = Directory.GetDirectories(outputDirPath);
                        Assert.AreEqual(2, directoriesInOutputFolder.Length,
                            $"The number of folders in '{OutputDirName}' is not as expected.");

                        AssertFilesExtensionsExistInDirectory(expectedFileExtensions_Output, outputDirPath,
                            OutputDirName);
                        AssertFilesExtensionsExistInDirectory(expectedFileExtensions_DFM_OUTPUT_Waq,
                            dfm_Output_WaqDirPath, Dfm_Output_WaqDirName);
                        AssertFilesExtensionsExistInDirectory(expectedFileExtensions_Snapped, snappedDirPath,
                            SnappedDirName);

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
        //3.1
        public void GivenAnFMModelWithTrachytopes_WhenExported_ThenCorrectFoldersAndFilesAreGiven()
        {
            CreateTestDirectories();

            var expectedFileExtensions = new List<string>
            {
                ".mdu",
                ".meta",
                "_net.nc",
                ".pol",
                ".pli",
                ".pliz",
                ".xyz",
                ".xyn",
                ".ini",
                ".fou",
                ".arl",
                ".ttd",
                ".bc",
                ".cld",
                ".cll",
                ".ext",
                ".ldb",
            };

            var expectedExtension = ".xml";

            try
            {
                CopyTestDataToTemp();
                CopyTrachytopeDataToTemp();

                using (var app = GetConfiguredApplication())
                {
                    using (var model = new WaterFlowFMModel())
                    {
                        AddFeaturesToModel(model);
                        EnableSalinityAndTemperature(model);
                        model.ExportTo(tempMduFilePath);
                        model.ReloadGrid(true, true);
                    }

                    SimulateUserAddingReferencesInMduFile();
                    SimulateUserAddingTrachytopesInMduFile();

                    using (var model = new WaterFlowFMModel(tempMduFilePath))
                    {
                        AdjustSettingsOutputParameters(model);
                        UpdateBedLevel(model);
                        AddModelToProject(model, app);

                        app.SaveProjectAs(projectFilePath);

                        model.ExportTo(exportMduFilePath);

                        AssertProjectFileAndFolderExist();

                        AssertDflowfmDirectoryExists();

                        var expectedFileCount = 1;

                        var actualFileCount = Directory.GetFiles(projectDirPath, $"*{expectedExtension}").Length;

                        Assert.AreEqual(expectedFileCount, actualFileCount,
                            Message_WrongNumberOfFilesOrFolders(expectedFileCount, "folders", expectedExtension,
                                actualFileCount, projectDirPath));

                        AssertFilesExtensionsExistInDirectory(expectedFileExtensions, dflowfmDirPath, DflowfmDirName);

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
        //3.2, discuss this with PO or Quality Owner
        public void GivenAnFMModelWithMorphology_WhenExported_ThenCorrectFoldersAndFilesAreGiven()
        {
            CreateTestDirectories();

            var expectedFileExtensions = new List<string>
            {
                ".mdu",
                ".meta",
                "_net.nc",
                ".pol",
                ".pli",
                ".pliz",
                ".xyz",
                ".xyn",
                ".ini",
                ".fou",
                ".sed",
                ".mor",
                ".bc",
                ".bcm",
                ".cld",
                ".cll",
                ".ext",
                ".ldb"
            };

            var expectedExtension = ".xml";

            try
            {
                CopyTestDataToTemp();

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

                        model.ExportTo(exportMduFilePath);

                        AssertProjectFileAndFolderExist();

                        AssertDflowfmDirectoryExists();

                        var expectedFileCount = 1;

                        var actualFileCount = Directory.GetFiles(projectDirPath, $"*{expectedExtension}").Length;

                        Assert.AreEqual(expectedFileCount, actualFileCount,
                            Message_WrongNumberOfFilesOrFolders(expectedFileCount, "folders", expectedExtension,
                                actualFileCount, projectDirPath));

                        AssertFilesExtensionsExistInDirectory(expectedFileExtensions, dflowfmDirPath, DflowfmDirName);

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
        //3.3, discuss this with PO or Quality Owner
        public void GivenAnFMModelWithWind_WhenExported_ThenCorrectFoldersAndFilesAreGiven()
        {
            CreateTestDirectories();

            var expectedFileExtensions = new List<string>
            {
                ".mdu",
                ".meta",
                "_net.nc",
                ".pol",
                ".pli",
                ".pliz",
                ".xyz",
                ".xyn",
                ".ini",
                ".fou",
                ".grd",
                ".apwxwy",
                ".bc",
                ".cld",
                ".cll",
                ".ext",
                ".ldb"
            };

            var expectedExtension = ".xml";

            try
            {
                CopyTestDataToTemp();
                CopyWindDataToTemp();

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

                        model.ExportTo(exportMduFilePath);

                        AssertProjectFileAndFolderExist();

                        AssertDflowfmDirectoryExists();

                        var expectedFileCount = 1;

                        var actualFileCount = Directory.GetFiles(projectDirPath, $"*{expectedExtension}").Length;

                        Assert.AreEqual(expectedFileCount, actualFileCount,
                            Message_WrongNumberOfFilesOrFolders(expectedFileCount, "folders", expectedExtension,
                                actualFileCount, projectDirPath));

                        AssertFilesExtensionsExistInDirectory(expectedFileExtensions, dflowfmDirPath, DflowfmDirName);

                        app.CloseProject();
                    }
                }
            }

            finally
            {
                DeleteTestDirectories();
            }
        }

        private DeltaShellApplication GetConfiguredApplication()
        {
            var app = new DeltaShellApplication();
            app.IsProjectCreatedInTemporaryDirectory = true;
            AddPluginsToApplication(app);
            app.SaveProjectAs(tempProjectFilePath);
            return app;
        }

        private static void AddPluginsToApplication(DeltaShellApplication app)
        {
            app.Plugins.Add(new NHibernateDaoApplicationPlugin());
            app.Plugins.Add(new CommonToolsApplicationPlugin());
            app.Plugins.Add(new SharpMapGisApplicationPlugin());
            app.Plugins.Add(new FlowFMApplicationPlugin());
            app.Plugins.Add(new NetworkEditorApplicationPlugin());
            app.Run();
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

        private static void CopyTestDataToTemp()
        {
            CopyTestDataFileToTemp("fourier.fou");
            CopyTestDataFileToTemp("calibration.cll");
            CopyTestDataFileToTemp("calibration.cld");
        }

        private static void CopyWindDataToTemp()
        {
            CopyTestDataFileToTemp(WindFileName);
            CopyTestDataFileToTemp(GridFileName);
        }

        private static void CopyTrachytopeDataToTemp()
        {
            CopyTestDataFileToTemp("trachytopes.arl");
            CopyTestDataFileToTemp("trachytopes.ttd");
        }

        private UnstructuredGrid CreateNewGrid(int width, int length)
        {
            var coordinateSystemFactory = new OgrCoordinateSystemFactory();
            var grid = new UnstructuredGrid
            {
                CoordinateSystem = coordinateSystemFactory.CreateFromEPSG(3857) // WGS 84 / Pseudo-Mercator
            };

            int numberOfCoordinates = width * length;
            for (int n = 0; n < length; n++)
            {
                for (int m = 0; m < width; m++)
                {
                    grid.Vertices.Add(new Coordinate(m, n, -1));
                }
            }

            for (int i = 0; i < numberOfCoordinates; i++)
            {
                if ((i + 1) % width != 0) grid.Edges.Add(new Edge(i, i + 1));
            }

            for (int i = 0; i < numberOfCoordinates - width; i++)
            {
                grid.Edges.Add(new Edge(i, i + width));
            }

            return grid;
        }

        private static LandBoundary2D CreateLandBoundary()
        {
            return WaterFlowFMMduFileTestHelper.GetNewLandBoundary2D("Boundaries", "LandBoundary");
        }

        private GroupableFeature2DPolygon CreateDryArea()
        {
            Coordinate[] polygon =
            {
                new Coordinate(1,1),
                new Coordinate(1,2),
                new Coordinate(2,2),
                new Coordinate(1,1)
            };

            return new GroupableFeature2DPolygon
            {
                Geometry = new Polygon(new LinearRing(polygon)),
                Name = "DryArea"
            };
        }

        private GroupablePointFeature CreateDryPoints()
        {
            return WaterFlowFMMduFileTestHelper.GetNewGroupablePointFeature("DryPoints");
        }

        private GroupableFeature2DPoint CreateObservationPoint()
        {
            return new GroupableFeature2DPoint
            {
                Geometry = new NetTopologySuite.Geometries.Point(5, 5),
                Name = "ObservationPoint"
            };
        }

        private Weir2D CreateWeir()
        {
            Coordinate[] lineString =
            {
                new Coordinate(7,7),
                new Coordinate(8,8)
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
                new Coordinate(3,3),
                new Coordinate(4,4)
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
                new Coordinate(3,3),
                new Coordinate(4,4)
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
            model.ModelDefinition.GetModelProperty(GuiProperties.UseTemperature).Value = true;
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
                Feature = feature,
            };

            flowBoundaryCondition.AddPoint(0);

            var timeSeries = flowBoundaryCondition.GetDataAtPoint(0);
            timeSeries[model.StartTime] = 0.5;
            timeSeries[model.StopTime] = 0.5;

            model.Boundaries.Add(feature);
            var set = new BoundaryConditionSet { Feature = feature };
            set.BoundaryConditions.Add(flowBoundaryCondition);
            model.BoundaryConditionSets.Add(set);
        }

        private void AddTracer(WaterFlowFMModel model)
        {
            var tracer = "Tracer1";
            model.TracerDefinitions.AddRange(new List<string> { tracer });

            var boundary = new Feature2D()
            {
                Name = "TracerBoundary1",
                Geometry =
                    new LineString(new[] { new Coordinate(0, 0), new Coordinate(1, 0) })
            };
            var set01 = new BoundaryConditionSet()
            {
                Feature = boundary
            };
            model.BoundaryConditionSets.Add(set01);

            set01.BoundaryConditions.Add(new FlowBoundaryCondition(FlowBoundaryQuantityType.Tracer, BoundaryConditionDataType.Empty)
            {
                Feature = boundary,
                TracerName = tracer
            });
        }

        private static void EnableMorphology(WaterFlowFMModel model)
        {
            model.ModelDefinition.GetModelProperty(GuiProperties.UseMorSed).Value = true;
        }

        private void AddMorphologyBoundaryConditionToModel(WaterFlowFMModel model)
        {
            var feature = new Feature2D
            {
                Name = "Boundary2",
                Geometry = new LineString(new[] { new Coordinate(1, 0), new Coordinate(0, 1) })
            };

            var morphologyBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.MorphologyBedLoadTransport, 
                BoundaryConditionDataType.TimeSeries)
            {
                Feature = feature,
                SedimentFractionNames = new List<string> { "SedimentFraction1" }
            };

            morphologyBoundaryCondition.AddPoint(0);

            var timeSeries = morphologyBoundaryCondition.GetDataAtPoint(0);
            timeSeries[model.StartTime] = 0.5;
            timeSeries[model.StopTime] = 0.5;

            model.Boundaries.Add(feature);
            var set = new BoundaryConditionSet { Feature = feature };
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
            using (StreamWriter sw = File.AppendText(tempMduFilePath))
            {
                sw.WriteLine("FouFile           = fourier.fou");
                sw.WriteLine("");
                sw.WriteLine("[calibration]");
                sw.WriteLine("UseCalibration    = 1");
                sw.WriteLine("DefinitionFile    = calibration.cld");
                sw.WriteLine("AreaFile          = calibration.cll");
                sw.WriteLine("");
            }
        }

        private static void SimulateUserAddingTrachytopesInMduFile()
        {
            using (StreamWriter sw = File.AppendText(tempMduFilePath))
            {
                sw.WriteLine("[trachytopes]");
                sw.WriteLine("TrtRou            = Y");
                sw.WriteLine("TrtDef            = trachytopes.ttd");
                sw.WriteLine("TrtL              = trachytopes.arl");
                sw.WriteLine("TrtDt             = 60");
                sw.WriteLine("DtTrt             = 60");
                sw.WriteLine("TrtMxR            = 100");
            }
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
            TypeUtils.CallPrivateMethod(model, "UpdateBathymetryCoverage", UnstructuredGridFileHelper.BedLevelLocation.NodesMinLev);
        }

        private static void AddModelToProject(WaterFlowFMModel model, IApplication app)
        {
            var project = app.Project;
            project.RootFolder.Add(model);
        }

        private void AssertFilesExtensionsExistInDirectory(List<string> expectedExtensions, string DirPath, string DirName)
        {
            expectedExtensions.ForEach(ext =>
                Assert.IsTrue(Directory.GetFiles(DirPath, $"*{ext}").Any(),
                    Message_MissingFileOrFolderExtension("file", ext, DirName)));
        }

        private void AssertProjectFileAndFolderExist()
        {
            var subDirectories = Directory.GetDirectories(workDirPath);
            Assert.NotNull(subDirectories);
            Assert.AreEqual(1, subDirectories.Length, "There should only be one folder: the project (.dsproj_data) folder.");

            var subFiles = Directory.GetFiles(workDirPath);
            Assert.NotNull(subFiles);
            Assert.AreEqual(1, subFiles.Length, "There should only be one file: the project (.dsproj) file.");

            var expectedCount = 1;

            // Get all files/folders with the project extension
            var actualFileCount = Directory.GetFiles(workDirPath, $"*{ProjectFileExtension}").Length;
            var actualFolderCount = Directory.GetDirectories(workDirPath, $"*{ProjectDirExtension}").Length;

            Assert.AreEqual(expectedCount, actualFileCount,
                Message_WrongNumberOfFilesOrFolders(expectedCount, "files", ProjectFileExtension, actualFileCount,
                    "Temp"));

            Assert.AreEqual(expectedCount, actualFolderCount,
                Message_WrongNumberOfFilesOrFolders(expectedCount, "folder", ProjectDirExtension, actualFolderCount,
                    "Temp"));
        }

        private void AssertModelDirectoryExists()
        {
            Assert.IsTrue(Directory.Exists(modelDirPath),
                Message_MissingFileOrFolderName("folder", modelDirName, projectDirName));
        }

        private void AssertInputDirectoryExists()
        {
            Assert.IsTrue(Directory.Exists(inputDirPath),
                Message_MissingFileOrFolderName("folder", InputDirName, modelDirName));
        }

        private void AssertOutputDirectoryExists()
        {
            Assert.IsTrue(Directory.Exists(outputDirPath), 
                Message_MissingFileOrFolderName("folder", OutputDirName, modelDirName));
        }

        private void AssertSnappedDirectoryExists()
        {
            Assert.IsTrue(Directory.Exists(outputDirPath),
                Message_MissingFileOrFolderName("folder", SnappedDirName, OutputDirName));
        }

        private void AssertDflowfmDirectoryExists()
        {
            Assert.IsTrue(Directory.Exists(dflowfmDirPath),
                Message_MissingFileOrFolderName("folder", DflowfmDirName, projectDirName));
        }

        private void AssertDfmOutputWaqDirectoryExists()
        {
            Assert.IsTrue(Directory.Exists(dfm_Output_WaqDirPath),
                Message_MissingFileOrFolderName("folder", Dfm_Output_WaqDirName, OutputDirName));
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
            FileUtils.CreateDirectoryIfNotExists(workDirPath);
            FileUtils.CreateDirectoryIfNotExists(tempDirPath);
        }

        private static void DeleteTestDirectories()
        {
            FileUtils.DeleteIfExists(workDirPath);
            FileUtils.DeleteIfExists(tempDirPath);
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            DeleteTestDirectories();
        }
    }
}

