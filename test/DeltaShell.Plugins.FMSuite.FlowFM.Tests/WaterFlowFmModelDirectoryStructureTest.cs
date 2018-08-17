using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Shell.Core;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DeltaShell.Core;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Import;
using DeltaShell.Plugins.SharpMapGis;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Grids;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using SharpMap.Extensions.CoordinateSystems;
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
        private static string tempPathDirectory;
        private static string mduFileDirectory;
        private static string dsProjFile;
        private static string dsProjFileDataDirectory;
        private static string flowFMFolderDirectory;
        private static string testDataFolder;
        private static string mduFileNameWithoutExtension;
        private static string flowFmFolderName;
        private bool isTrachytopesEnabled;
        private bool isWindEnabled;
        private bool isMorphologyEnabled;

        [TestFixtureSetUp]
        public void Setup()
        {
            tempPathDirectory = Path.Combine(Path.GetTempPath(),
            Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
            FileUtils.CreateDirectoryIfNotExists(tempPathDirectory);
            mduFileDirectory = Path.Combine(tempPathDirectory, "FlowFM.mdu");
            dsProjFile = Path.Combine(tempPathDirectory, "MyProject.dsproj");
            dsProjFileDataDirectory = Path.Combine(tempPathDirectory, "MyProject.dsproj_data");
            flowFMFolderDirectory = Path.Combine(dsProjFileDataDirectory, "FlowFM");
            testDataFolder = "TestPlanFM";
            var mduFileName = mduFileDirectory.Split('\\').Last();
            mduFileNameWithoutExtension = mduFileName.Split('.').First();
            flowFmFolderName = flowFMFolderDirectory.Split('\\').Last();
        }

        [Test]
        public void GivenANewFMModelWithTrachytopes_WhenSavedAs_ThenInputFolderIsCreatedWithInputFiles()
        {
            try
            {
                using (var app = GetConfiguredApplication())
                {
                    using (GetConfiguredBaseModel())
                    {
                        CopyTestDataFileToLocation(tempPathDirectory, testDataFolder, "trachytopes.arl");
                        CopyTestDataFileToLocation(tempPathDirectory, testDataFolder, "trachytopes.ttd");
                    }

                    SimulateUserAddingReferencesInMduFile(mduFileDirectory, true);

                    using (var model = new WaterFlowFMModel(mduFileDirectory))
                    {
                        AdjustSettingsOutputParameters(model);
                        UpdateBedLevel(model);

                        AddModelToProject(model, app);
                        app.SaveProject();

                        Assert.That(File.Exists(dsProjFile), Is.True);
                        Assert.That(Directory.Exists(dsProjFileDataDirectory), Is.True);
                        Assert.That(Directory.Exists(flowFMFolderDirectory), Is.True);
                        Assert.That(Directory.Exists(Path.Combine(flowFMFolderDirectory, "input")));
                       
                        app.CloseProject();
                    }

                }
            }
            finally
            {

            }
        }

        private DeltaShellApplication GetConfiguredApplication()
        {
            var app = new DeltaShellApplication();
            app.IsProjectCreatedInTemporaryDirectory = true;
            AddPluginsToApplication(app);
            app.SaveProjectAs(dsProjFile);
            return app;
        }

        private WaterFlowFMModel GetConfiguredBaseModel()
        {
            var model = new WaterFlowFMModel();
            model.Grid = CreateNewGrid(5, 10);
            model.Area.LandBoundaries.Add(WaterFlowFMMduFileTestHelper.GetNewLandBoundary2D("Boundaries", "LandBoundary1"));
            model.Area.DryAreas.Add(CreateDryArea());
            model.Area.DryPoints.Add(CreateDryPoints());
            model.Area.ObservationPoints.Add(CreateObservationPoint());
            model.Area.Weirs.Add(CreateWeir());
            model.Area.FixedWeirs.Add(CreateFixedWeir());
            model.Area.ObservationCrossSections.Add(CreateObservationCrossSection());

            AddBoundaryConditionToModel(model);
            AddTracer(model);
            EnableSalinityAndTemperature(model);

            model.ExportTo(mduFileDirectory);

            model.ReloadGrid(true, true);

            CopyTestDataFileToLocation(tempPathDirectory, testDataFolder, "fourier.fou");
            CopyTestDataFileToLocation(tempPathDirectory, testDataFolder, "calibration.cll");
            CopyTestDataFileToLocation(tempPathDirectory, testDataFolder, "calibration.cld");

            return model;
        }

        public void TestTrachytopes()
        {
            Assert.That(File.Exists(Path.Combine(flowFMFolderDirectory, "trachytopes.arl")));
            Assert.That(File.Exists(Path.Combine(flowFMFolderDirectory, "trachytopes.ttd")));
            Assert.That(File.Exists(Path.Combine(flowFMFolderDirectory, "Boundaries.ldb")));
            Assert.That(File.Exists(Path.Combine(flowFMFolderDirectory, "Boundary01.pli")));
            Assert.That(File.Exists(Path.Combine(flowFMFolderDirectory, "calibration.cld")));
            Assert.That(File.Exists(Path.Combine(flowFMFolderDirectory, "calibration.cll")));
            Assert.That(File.Exists(Path.Combine(flowFMFolderDirectory, "Discharge.bc")));
            Assert.That(File.Exists(Path.Combine(flowFMFolderDirectory, "DryPoints_dry.xyz")));
            Assert.That(File.Exists(Path.Combine(flowFMFolderDirectory, "FlowFM.mdu")));
            Assert.That(File.Exists(Path.Combine(flowFMFolderDirectory, "FlowFM_bnd.ext")));
            Assert.That(File.Exists(Path.Combine(flowFMFolderDirectory, "FlowFM_crs.pli")));
            Assert.That(File.Exists(Path.Combine(flowFMFolderDirectory, "FlowFM_dry.pol")));
            Assert.That(File.Exists(Path.Combine(flowFMFolderDirectory, "FlowFM_fxw.pliz")));
            Assert.That(File.Exists(Path.Combine(flowFMFolderDirectory, "FlowFM_fxw.pliz")));
            Assert.That(File.Exists(Path.Combine(flowFMFolderDirectory, "FlowFM_net.nc")));
            Assert.That(File.Exists(Path.Combine(flowFMFolderDirectory, "FlowFM_obs.xyn")));
            Assert.That(File.Exists(Path.Combine(flowFMFolderDirectory, "fourier.fou")));
            Assert.That(File.Exists(Path.Combine(flowFMFolderDirectory, "restart.meta")));
        }

        public void BasicInputFiles()
        {
            Assert.That(!File.Exists(Path.Combine(flowFMFolderDirectory, "trachytopes.arl")));
            Assert.That(!File.Exists(Path.Combine(flowFMFolderDirectory, "trachytopes.ttd")));
            Assert.That(File.Exists(Path.Combine(flowFMFolderDirectory, "Boundaries.ldb")));
            Assert.That(File.Exists(Path.Combine(flowFMFolderDirectory, "Boundary01.pli")));
            Assert.That(File.Exists(Path.Combine(flowFMFolderDirectory, "calibration.cld")));
            Assert.That(File.Exists(Path.Combine(flowFMFolderDirectory, "calibration.cll")));
            Assert.That(File.Exists(Path.Combine(flowFMFolderDirectory, "Discharge.bc")));
            Assert.That(File.Exists(Path.Combine(flowFMFolderDirectory, "DryPoints_dry.xyz")));
            Assert.That(File.Exists(Path.Combine(flowFMFolderDirectory, "FlowFM.mdu")));
            Assert.That(File.Exists(Path.Combine(flowFMFolderDirectory, "FlowFM_bnd.ext")));
            Assert.That(File.Exists(Path.Combine(flowFMFolderDirectory, "FlowFM_crs.pli")));
            Assert.That(File.Exists(Path.Combine(flowFMFolderDirectory, "FlowFM_dry.pol")));
            Assert.That(File.Exists(Path.Combine(flowFMFolderDirectory, "FlowFM_fxw.pliz")));
            Assert.That(File.Exists(Path.Combine(flowFMFolderDirectory, "FlowFM_fxw.pliz")));
            Assert.That(File.Exists(Path.Combine(flowFMFolderDirectory, "FlowFM_net.nc")));
            Assert.That(File.Exists(Path.Combine(flowFMFolderDirectory, "FlowFM_obs.xyn")));
            Assert.That(File.Exists(Path.Combine(flowFMFolderDirectory, "fourier.fou")));
            Assert.That(File.Exists(Path.Combine(flowFMFolderDirectory, "restart.meta")));
        }
        #region HelperFiles

        private static void CopyTestDataFileToLocation(string targetFolder, string testDataFolder, string file)
        {
            var filePath = Path.Combine(TestHelper.GetDataDir(), testDataFolder, file);
            var targetFile = Path.Combine(targetFolder, file);
            FileUtils.CopyFile(filePath, targetFile);
            Assert.IsTrue(File.Exists(targetFile));
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
        private void SimulateUserAddingReferencesInMduFile(string mduFile, bool isTrachytopesEnabled)
        {
            //user adding stuff
            using (StreamWriter sw = File.AppendText(mduFile))
            {
                sw.WriteLine("FouFile           = fourier.fou");
                sw.WriteLine("");
                sw.WriteLine("[calibration]");
                sw.WriteLine("UseCalibration    = 1");
                sw.WriteLine("DefinitionFile    = calibration.cld");
                sw.WriteLine("AreaFile          = calibration.cll");
                sw.WriteLine("");
                sw.WriteLine("[trachytopes]");
                sw.WriteLine("TrtRou            = Y");
                sw.WriteLine("TrtDef            = trachytopes.ttd");
                sw.WriteLine("TrtL              = trachytopes.arl");
                sw.WriteLine("TrtDt             = 60");
                sw.WriteLine("DtTrt             = 60");
                sw.WriteLine("TrtMxR            = 100");
            }
        }

        private void UpdateBedLevel(WaterFlowFMModel model)
        {
            TypeUtils.CallPrivateMethod(model, "UpdateBathymetryCoverage", UnstructuredGridFileHelper.BedLevelLocation.NodesMinLev);
        }

        private Weir2D CreateWeir()
        {
            return new Weir2D()
            {
                Name = "weir01",
                WeirFormula = new SimpleWeirFormula(),
            };
        }

        private GroupablePointFeature CreateDryPoints()
        {
            return WaterFlowFMMduFileTestHelper.GetNewGroupablePointFeature("DryPoints");
        }

        private void AddBoundaryConditionToModel(WaterFlowFMModel model)
        {
            var feature = new Feature2D
            {
                Name = "Boundary01",
                Geometry =
                    new LineString(new[] { new Coordinate(0, 0), new Coordinate(1, 0) })
            };

            var boundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.Discharge,
                BoundaryConditionDataType.TimeSeries)
            {
                Feature = feature,
            };

            var startTime = new DateTime(2014, 1, 1);
            var stopTime = new DateTime(2014, 1, 2);
            boundaryCondition.AddPoint(0);
            var dischargeTimeSeries = boundaryCondition.GetDataAtPoint(0);
            dischargeTimeSeries[startTime] = 10.1;
            dischargeTimeSeries[stopTime] = 20.2;

            var set01 = new BoundaryConditionSet() { Feature = feature };
            model.BoundaryConditionSets.Add(set01);

            set01.BoundaryConditions.Add(boundaryCondition);
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
                Name = "ObsCS2d",
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
                Name = "My Fixed Weir"
            };
        }

        private GroupableFeature2DPoint CreateObservationPoint()
        {
            return new GroupableFeature2DPoint
            {
                Geometry = new NetTopologySuite.Geometries.Point(5, 5),
                Name = "obsPoint"
            };
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
                Name = "dryArea"
            };
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

        private void AddTracer(WaterFlowFMModel model)
        {
            var tracer01 = "Tracer01";
            model.TracerDefinitions.AddRange(new List<string> { tracer01 });

            var boundary01 = new Feature2D()
            {
                Name = "TracerBoundary01",
                Geometry =
                    new LineString(new[] { new Coordinate(0, 0), new Coordinate(1, 0) })
            };
            var set01 = new BoundaryConditionSet()
            {
                Feature = boundary01
            };
            model.BoundaryConditionSets.Add(set01);

            set01.BoundaryConditions.Add(new FlowBoundaryCondition(FlowBoundaryQuantityType.Tracer, BoundaryConditionDataType.Empty)
            {
                Feature = boundary01,
                TracerName = tracer01
            });
        }

        private static void EnableSalinityAndTemperature(WaterFlowFMModel model)
        {
            model.ModelDefinition.GetModelProperty(KnownProperties.UseSalinity).Value = true;
            model.ModelDefinition.GetModelProperty(GuiProperties.UseTemperature).Value = true;
        }

        private static void AddModelToProject(WaterFlowFMModel model, IApplication app)
        {
            var project = app.Project;
            project.RootFolder.Add(model);
        }

        private static void ImportLandBoundariesAndGrid(WaterFlowFMModel model, IApplication app, string landBoundaryPath, string netFile)
        {
            ImportLandBoundaries(model, app, landBoundaryPath);
            ImportGrid(app, netFile, model);
        }

        private static List<GroupablePointFeature> ImportDryPoints(string path, WaterFlowFMModel model)
        {
            var importer = new GroupablePointCloudImporter();
            var dryPoints = new List<GroupablePointFeature>();
            importer.ImportItem(path, dryPoints);
            Assert.AreNotEqual(0, dryPoints.Count);

            return dryPoints;
        }

        private static void ImportLandBoundaries(WaterFlowFMModel model, IApplication app, string landBoundaryPath)
        {
            var importerLDB = app.FileImporters.OfType<LdbFileImporterExporter>().FirstOrDefault();
            Assert.IsNotNull(importerLDB);

            var ldbImported = importerLDB.ImportItem(landBoundaryPath, model.Area.LandBoundaries);
            Assert.IsNotNull(ldbImported as IList<LandBoundary2D>);
            Assert.AreEqual(2, (ldbImported as IList<LandBoundary2D>).Count);
            Assert.AreEqual(2, model.Area.LandBoundaries.Count);
        }

        private static void ImportGrid(IApplication app, string netFile, WaterFlowFMModel model)
        {
            var importerGrid = app.FileImporters.OfType<FlowFMNetFileImporter>().FirstOrDefault();
            Assert.IsNotNull(importerGrid);
            var gridImported = importerGrid.ImportItem(netFile, model.Grid);
            Assert.IsNotNull(gridImported);
            Assert.AreEqual(50, model.Grid.Cells.Count);
        }
        #endregion


        //Prisca
        private const string TestDataDirName = "TestPlanFM";
        private const string ProjectFileExtension = ".dsproj";
        private const string ProjectDirExtension = ".dsproj_data";
        private const string ModelDirName = "FlowFM";
        private const string InputDirName = "input";
        private const string OutputDirName = "output";
        private const string SnappedDirName = "snapped";
        private const string DFM_OUTPUT_WAQDirName = "DFM_OUTPUT_WAQ";
        private const string DflowfmDirName = "dflowfm";

        // 2.2.1, 2.2.2, 2.2.3
        [TestCase("")]
        public void GivenAnFMModel_WhenRun_ProperOutputIsGiven(string mduName, string path)
        {
            var outputDirPath = GetOutputDirPath(mduName, path);

            var snappedDirPath = Path.Combine(outputDirPath, SnappedDirName);
            var dfmOutputWaqDirPath = Path.Combine(outputDirPath, DFM_OUTPUT_WAQDirName);

            var directoriesInOutputFolder = Directory.GetDirectories(outputDirPath);
            Assert.AreEqual(2, directoriesInOutputFolder.Length);

            var expectedFolderPaths_Output = new List<string>
            {
                snappedDirPath, dfmOutputWaqDirPath
            };

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

            expectedFolderPaths_Output.ForEach(p => Assert.IsTrue(Directory.Exists(p),
                Message_MissingFileOrFolderName("folder", Path.GetDirectoryName(p), OutputDirName)));

            expectedFileExtensions_Output.ForEach(ext =>
                Assert.IsTrue(Directory.GetFiles(outputDirPath, $"*{ext}").Any(),
                    Message_MissingFileOrFolderExtension("file",ext, OutputDirName)));

            expectedFileExtensions_DFM_OUTPUT_Waq.ForEach(ext =>
                Assert.IsTrue(Directory.GetFiles(dfmOutputWaqDirPath, $"*{ext}").Any(),
                    Message_MissingFileOrFolderExtension("file", ext, DFM_OUTPUT_WAQDirName)));

            expectedFileExtensions_Snapped.ForEach(ext =>
                Assert.IsTrue(Directory.GetFiles(snappedDirPath, $"*{ext}").Any(),
                    Message_MissingFileOrFolderExtension("file", ext, SnappedDirName)));
        }

        // 2.3.1, (2.3.2 / 2.3.3?)
        [TestCase("*.dsproj_data")]
        public void GivenAnFMModel_WhenExported_ProperOutputIsGiven(string testDirPath)
        {
            var projectDirPath = GetProjectFolderPath(testDirPath);

            var dflowfmDirPath = Path.Combine(projectDirPath, DflowfmDirName);
            Assert.IsTrue(Directory.Exists(dflowfmDirPath), Message_MissingFileOrFolderName("folder", DflowfmDirName,projectDirPath));

            var extension = ".xml";
            var expectedFileCount = 1;
            var actualFileCount = Directory.GetFiles(projectDirPath, $"*{extension}").Length;

            Assert.AreEqual(expectedFileCount, actualFileCount,
                Message_WrongNumberOfFilesOrFolders(expectedFileCount, "folders", extension, actualFileCount, projectDirPath));
 
            var expectedFileExtensions_Dflowfm = new List<string>
            {
                ".mdu",
                ".meta",
                "._net.nc",
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
                ".ldb"
            };

            expectedFileExtensions_Dflowfm.ForEach(ext =>
                Assert.IsTrue(Directory.GetFiles(dflowfmDirPath, $"*{ext}").Any(),
                    Message_MissingFileOrFolderName("folder", ext, DflowfmDirName)));
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

        //this is the temp folder we work in
        private void AssertProjectFileAndFolderExist(string testDirPath)
        {
            var testDirName = Path.GetDirectoryName(testDirPath);

            Assert.IsTrue(Directory.Exists(testDirPath));

            var subDirectories = Directory.GetDirectories(testDirPath);
            Assert.NotNull(subDirectories);
            Assert.AreEqual(1, subDirectories.Length);

            var subFiles = Directory.GetFiles(testDirPath);
            Assert.NotNull(subFiles);
            Assert.AreEqual(1, subFiles.Length);

            var expectedCount = 1;
            var actualFileCount = Directory.GetFiles(testDirPath, $"*{ProjectFileExtension}").Length;
            var actualFolderCount = Directory.GetDirectories(testDirPath, $"*{ProjectDirExtension}").Length;


            Assert.AreEqual(expectedCount, actualFileCount,
                Message_WrongNumberOfFilesOrFolders(expectedCount, "files", ProjectFileExtension, actualFileCount,
                    testDirName));

            Assert.AreEqual(expectedCount, actualFolderCount,
                Message_WrongNumberOfFilesOrFolders(expectedCount, "folder", ProjectDirExtension, actualFolderCount,
                    testDirName));
        }

        private string GetProjectFolderPath(string testDirPath)
        {
            var projectFolder = Directory.GetDirectories(testDirPath, $"*{ProjectDirExtension}").FirstOrDefault();

            Assert.NotNull(projectFolder, 
                Message_MissingFileOrFolderExtension("folder", ProjectDirExtension, testDirPath));

            return projectFolder;
        }

        private string GetInputDirPath(string modelDirName, string testDirPath)
        {
            var inputFolder = Path.Combine(GetProjectFolderPath(testDirPath), modelDirName, InputDirName);

            Assert.IsTrue(Directory.Exists(inputFolder), 
                Message_MissingFileOrFolderName("folder", InputDirName, modelDirName));

            return inputFolder;
        }

        private string GetOutputDirPath(string modelDirName, string testDirPath)
        {
            var outputFolder = Path.Combine(GetProjectFolderPath(testDirPath), modelDirName, OutputDirName);

            Assert.IsTrue(Directory.Exists(outputFolder), 
                Message_MissingFileOrFolderName("folder", OutputDirName, modelDirName));

            return outputFolder;
        }
    }
}

