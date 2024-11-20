using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DeltaShell.IntegrationTestUtils.NHibernate;
using DeltaShell.NGHS.TestUtils.Builders;
using DeltaShell.Plugins.Data.NHibernate.DelftTools.Shell.Core.Dao;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.BoundaryData;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.ObservationAreas;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Utils;
using DeltaShell.Plugins.SharpMapGis.SpatialOperations;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using NUnit.Framework;
using Rhino.Mocks;
using SharpMap.Extensions.CoordinateSystems;
using SharpMapTestUtils;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.NHibernate
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class WaterQualityModelNHibernateTest : NHibernateIntegrationTestBase
    {
        private const string LOADTYPE = "Sewer";
        private const string LOADNAME = "Load 1";
        private const double LOAD_X = 0.1d;
        private const double LOAD_Y = 0.2d;
        private const double LOAD_Z = 0.3d;

        [Test]
        [Category(TestCategory.Slow)]
        public void SaveAndRetrieveDataTable()
        {
            // setup
            string folderPath = TestHelper.GetCurrentMethodName();
            FileUtils.DeleteIfExists(folderPath);
            Directory.CreateDirectory(folderPath);

            string dataFilePath = Path.Combine(folderPath, "A.tbl");
            const string datafileContents = "datafile";
            File.WriteAllText(dataFilePath, datafileContents);

            string useforFilePath = Path.Combine(folderPath, "A.usefors");
            const string useforFileContents = "usefors";
            File.WriteAllText(useforFilePath, useforFileContents);

            try
            {
                var dataFileOnDisk = new TextDocumentFromFile();
                dataFileOnDisk.Open(dataFilePath);

                var substanceFileOnDisk = new TextDocumentFromFile();
                substanceFileOnDisk.Open(useforFilePath);

                var entity = new DataTable
                {
                    IsEnabled = false,
                    Name = "A",
                    DataFile = dataFileOnDisk,
                    SubstanceUseforFile = substanceFileOnDisk
                };

                // call
                DataTable retrievedEntity = SaveAndRetrieveObject(entity);

                // assert
                Assert.AreEqual("A", retrievedEntity.Name);
                Assert.IsFalse(retrievedEntity.IsEnabled);

                TextDocumentFromFile retrievedDataFileOnDisk = retrievedEntity.DataFile;
                Assert.IsFalse(retrievedDataFileOnDisk.ReadOnly);
                Assert.IsTrue(retrievedDataFileOnDisk.IsOpen);
                Assert.AreEqual(dataFilePath, retrievedDataFileOnDisk.Path);
                Assert.AreEqual(datafileContents, retrievedDataFileOnDisk.Content);

                TextDocumentFromFile retrievedSubstanceFileOnDisk = retrievedEntity.SubstanceUseforFile;
                Assert.IsFalse(retrievedSubstanceFileOnDisk.ReadOnly);
                Assert.IsTrue(retrievedSubstanceFileOnDisk.IsOpen);
                Assert.AreEqual(useforFilePath, retrievedSubstanceFileOnDisk.Path);
                Assert.AreEqual(useforFileContents, retrievedSubstanceFileOnDisk.Content);
            }
            finally
            {
                FileUtils.DeleteIfExists(folderPath);
            }
        }

        [Test]
        public void SaveAndRetrieveWaterQualityObservationAreaCoverage()
        {
            // setup
            UnstructuredGrid grid = UnstructuredGridTestHelper.GenerateRegularGrid(5, 5, 1, 1);
            var entity = new WaterQualityObservationAreaCoverage(grid);
            foreach (int i in Enumerable.Range(0, grid.Cells.Count - 5))
            {
                entity[i] = i % 3;
            }

            string noDataLabel = WaterQualityObservationAreaCoverage.NoDataLabel;
            string[] expectedLabels = new[]
            {
                "zero",
                "one",
                "two",
                "zero",
                "one",
                "two",
                "zero",
                "one",
                "two",
                "zero",
                "one",
                "two",
                "zero",
                "one",
                "two",
                "zero",
                "one",
                "two",
                "zero",
                "one",
                "two",
                "zero",
                noDataLabel,
                noDataLabel,
                noDataLabel
            };
            entity.SetValuesAsLabels(expectedLabels);

            // call
            WaterQualityObservationAreaCoverage retrievedEntity = SaveAndRetrieveObject(entity);

            // assert
            Assert.IsNotNull(retrievedEntity.Grid);
            Assert.IsTrue(retrievedEntity.Grid.IsEmpty);
            Assert.AreEqual(1, retrievedEntity.Arguments.Count);
            Assert.AreEqual("cell_index", retrievedEntity.Arguments[0].Name);
            CollectionAssert.AreEqual(Enumerable.Range(0, grid.Cells.Count).ToArray(), retrievedEntity.Arguments[0].GetValues<int>().ToArray());

            Assert.AreEqual(1, retrievedEntity.Components.Count);
            Assert.AreEqual("value", retrievedEntity.Components[0].Name);
            Assert.AreEqual(typeof(int), retrievedEntity.Components[0].ValueType);

            IList<string> retrievedLabels = retrievedEntity.GetValuesAsLabels();
            Assert.AreEqual(expectedLabels.Length, retrievedLabels.Count);
            CollectionAssert.AreEquivalent(expectedLabels, retrievedLabels);
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void SaveAndRetrieveDataTableManager()
        {
            // setup
            string folderPath = TestHelper.GetCurrentMethodName();
            FileUtils.DeleteIfExists(folderPath);
            Directory.CreateDirectory(folderPath);

            try
            {
                var entity = new DataTableManager
                {
                    Name = "<name>",
                    FolderPath = folderPath
                };
                entity.CreateNewDataTable("A", "datatablecontents A", "A.usefors", "useforscontents A");
                entity.CreateNewDataTable("B", "datatablecontents B", "B.usefors", "useforscontents B");
                entity.DataTables.First().IsEnabled = false;

                // call
                DataTableManager retrievedEntity = SaveAndRetrieveObject(entity);

                // assert
                Assert.AreEqual(folderPath, retrievedEntity.FolderPath);
                Assert.AreEqual("<name>", retrievedEntity.Name);

                DataTable[] dataTables = retrievedEntity.DataTables.ToArray();
                Assert.AreEqual(2, dataTables.Length);

                Assert.AreEqual("A", dataTables[0].Name);
                Assert.IsFalse(dataTables[0].IsEnabled);
                Assert.IsTrue(File.Exists(dataTables[0].DataFile.Path));
                Assert.IsTrue(dataTables[0].DataFile.IsOpen);
                Assert.AreEqual("datatablecontents A", dataTables[0].DataFile.Content);
                Assert.IsTrue(File.Exists(dataTables[0].SubstanceUseforFile.Path));
                Assert.IsTrue(dataTables[0].SubstanceUseforFile.IsOpen);
                Assert.AreEqual("useforscontents A", dataTables[0].SubstanceUseforFile.Content);

                Assert.AreEqual("B", dataTables[1].Name);
                Assert.IsTrue(dataTables[1].IsEnabled);
                Assert.IsTrue(File.Exists(dataTables[1].DataFile.Path));
                Assert.IsTrue(dataTables[1].DataFile.IsOpen);
                Assert.AreEqual("datatablecontents B", dataTables[1].DataFile.Content);
                Assert.IsTrue(File.Exists(dataTables[1].SubstanceUseforFile.Path));
                Assert.IsTrue(dataTables[1].SubstanceUseforFile.IsOpen);
                Assert.AreEqual("useforscontents B", dataTables[1].SubstanceUseforFile.Content);
            }
            finally
            {
                FileUtils.DeleteIfExists(folderPath);
            }
        }

        [Test]
        public void SaveAndRetrieveHydFileData()
        {
            // setup
            string filePath = Path.Combine(TestHelper.GetTestDataDirectory(), "IO", "real", "uni3d.hyd");
            using (var entity = new HydFileData
            {
                Path = new FileInfo(filePath),
                Checksum = "123456789abcdeffedcba987654321"
            })
            {
                // call
                using (HydFileData retrievedEntity = SaveAndRetrieveObject(entity))
                {
                    // assert
                    Assert.AreEqual(filePath, retrievedEntity.Path.FullName);
                    Assert.AreEqual("123456789abcdeffedcba987654321", retrievedEntity.Checksum);
                }
            }
        }

        [Test]
        [TestCase(ObservationPointType.SinglePoint)]
        [TestCase(ObservationPointType.Average)]
        [TestCase(ObservationPointType.OneOnEachLayer)]
        public void SaveAndRetrieveWaterQualityObservationPoint(ObservationPointType type)
        {
            // setup
            var entity = new WaterQualityObservationPoint
            {
                Name = "Observation Point 1",
                X = 1.1,
                Y = 2.2,
                Z = double.NaN, // test special case!
                ObservationPointType = type
            };

            // call
            WaterQualityObservationPoint retrievedEntity = SaveAndRetrieveObject(entity);

            // assert
            Assert.AreEqual(type, retrievedEntity.ObservationPointType);

            Assert.AreEqual("Observation Point 1", retrievedEntity.Name);
            Assert.AreEqual(1.1, retrievedEntity.X);
            Assert.AreEqual(2.2, retrievedEntity.Y);
            Assert.IsNaN(retrievedEntity.Z);
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void SaveAndLoadWaterQualityLoad()
        {
            WaterQualityLoad entity = CreateLoad();

            WaterQualityLoad retrievedEntity = SaveAndRetrieveObject(entity);
            Assert.IsNotNull(retrievedEntity);
            Assert.IsNotNull(retrievedEntity.Geometry);
            Assert.AreEqual(LOAD_X, retrievedEntity.X);
            Assert.AreEqual(LOAD_Y, retrievedEntity.Y);
            Assert.AreEqual(LOAD_Z, retrievedEntity.Z);
            Assert.AreEqual(LOADNAME, retrievedEntity.Name);
            Assert.AreEqual(LOADTYPE, retrievedEntity.LoadType);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void SaveAndRetrieveFunctionFromHydroDynamics()
        {
            // setup
            string filePath = Path.Combine(TestHelper.GetTestDataDirectory(), "IO", "real", "uni3d.sal");
            FunctionFromHydroDynamics entity = WaterQualityFunctionFactory.CreateFunctionFromHydroDynamics("Name", 1.2, "Component name", "Unit name", "My Description");
            entity.FilePath = filePath;

            // call
            FunctionFromHydroDynamics retrievedEntity = SaveAndRetrieveObject(entity);

            // assert
            Assert.AreEqual("Name", retrievedEntity.Name);
            Assert.AreEqual(1, retrievedEntity.Components.Count);
            Assert.AreEqual("Component name", retrievedEntity.Components[0].Name);
            Assert.AreEqual(1.2, retrievedEntity.Components[0].DefaultValue);
            Assert.AreEqual("Unit name", retrievedEntity.Components[0].Unit.Name);
            Assert.AreEqual("Unit name", retrievedEntity.Components[0].Unit.Symbol);
            Assert.AreEqual(filePath, retrievedEntity.FilePath);
            Assert.AreEqual("My Description", retrievedEntity.Attributes[WaterQualityFunctionFactory.DESCRIPTION_ATTRIBUTE]);
        }

        [Test]
        public void SaveAndRetrieveStandaloneWaterQualityModelWithoutHydFile()
        {
            // setup
            string commonFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), "IO");

            var entity = new WaterQualityModel
            {
                Name = "Test",
                VerticalDispersion = 1.1,
                UseAdditionalHydrodynamicVerticalDiffusion = true,
                HorizontalDispersion = 5.5
            };

            TypeUtils.SetPrivatePropertyValue(entity, nameof(WaterQualityModel.LayerType),
                                              LayerType.ZLayer);

            Assert.IsTrue(entity.Grid.IsEmpty);
            Assert.IsNull(entity.HydroData);

            entity.UseSaveStateTimeRange = true;
            entity.SaveStateStartTime = new DateTime(2015, 3, 17, 7, 16, 05);
            entity.SaveStateTimeStep = new TimeSpan(0, 1, 0, 0);
            entity.SaveStateStopTime = new DateTime(2015, 3, 18, 7, 16, 05);
            entity.ReferenceTime = new DateTime(2015, 3, 17, 7, 16, 05);

            entity.InputFileCommandLine.Content = "<Content for command line input file>";
            entity.InputFileHybrid.Content = "<Content for hybrid model input file>";

            entity.ObservationPoints.AddRange(new[]
            {
                new WaterQualityObservationPoint {Name = "obs1"}
            });

            new SubFileImporter().Import(entity.SubstanceProcessLibrary, Path.Combine(commonFilePath, "03d_Tewor2003.sub"));
            Assert.AreEqual(5, entity.InitialConditions.Count, "Precondition: read sub-file contains 5 substances.");

            entity.Loads.Add(CreateLoad());

            entity.BoundaryDataManager.CreateNewDataTable("A", "test", "A.usefors", "bla");
            entity.LoadsDataManager.CreateNewDataTable("B", "test", "B.usefors", "bla");

            // settings
            entity.ModelSettings.NrOfThreads = 6;
            entity.ModelSettings.ClosureErrorCorrection = false;
            entity.ModelSettings.DryCellThreshold = 0.2;
            entity.ModelSettings.IterationMaximum = 10;
            entity.ModelSettings.Tolerance = 1;
            entity.ModelSettings.WriteIterationReport = true;

            // call
            WaterQualityModel retrievedEntity = SaveAndRetrieveObject(entity);

            // assert
            Assert.IsNotNull(retrievedEntity);
            Assert.AreEqual("Test", retrievedEntity.Name);
            Assert.AreEqual(1.1, retrievedEntity.VerticalDispersion);
            Assert.IsTrue(retrievedEntity.UseAdditionalHydrodynamicVerticalDiffusion);
            Assert.AreEqual(5.5, retrievedEntity.HorizontalDispersion);

            Assert.AreEqual(LayerType.ZLayer, retrievedEntity.LayerType);

            Assert.IsTrue(retrievedEntity.Grid.IsEmpty);
            Assert.IsNull(retrievedEntity.HydroData);
            Assert.AreEqual(LayerType.ZLayer, retrievedEntity.LayerType);

            Assert.IsTrue(retrievedEntity.UseSaveStateTimeRange);
            Assert.AreEqual(new DateTime(2015, 3, 17, 7, 16, 05), retrievedEntity.SaveStateStartTime);
            Assert.AreEqual(new TimeSpan(0, 1, 0, 0), retrievedEntity.SaveStateTimeStep);
            Assert.AreEqual(new DateTime(2015, 3, 18, 7, 16, 05), retrievedEntity.SaveStateStopTime);
            Assert.AreEqual(new DateTime(2015, 3, 17, 7, 16, 05), retrievedEntity.ReferenceTime);

            Assert.AreEqual("<Content for command line input file>", entity.InputFileCommandLine.Content);
            Assert.AreEqual("<Content for hybrid model input file>", entity.InputFileHybrid.Content);

            Assert.AreEqual(1, retrievedEntity.ObservationPoints.Count);
            Assert.AreEqual("obs1", retrievedEntity.ObservationPoints[0].Name);

            Assert.AreEqual("03d_Tewor2003", retrievedEntity.SubstanceProcessLibrary.Name);
            Assert.AreEqual(entity.SubstanceProcessLibrary.Substances.Count, entity.SubstanceProcessLibrary.Substances.Count);
            Assert.AreEqual(entity.SubstanceProcessLibrary.Processes.Count, entity.SubstanceProcessLibrary.Processes.Count);
            Assert.AreEqual(entity.SubstanceProcessLibrary.Parameters.Count, entity.SubstanceProcessLibrary.Parameters.Count);
            Assert.AreEqual(entity.SubstanceProcessLibrary.OutputParameters.Count, entity.SubstanceProcessLibrary.OutputParameters.Count);
            Assert.AreEqual(entity.SubstanceProcessLibrary.InActiveSubstances.Count(), entity.SubstanceProcessLibrary.InActiveSubstances.Count());

            Assert.AreEqual(5, retrievedEntity.InitialConditions.Count);

            Assert.AreEqual(entity.DataItems.Count, retrievedEntity.DataItems.Count);
            Assert.AreEqual(entity.Dispersion.Count, retrievedEntity.Dispersion.Count);

            Assert.AreEqual(1, retrievedEntity.Loads.Count);
            Assert.AreEqual(retrievedEntity.Loads[0].LoadType, LOADTYPE);

            DataTable[] dataTables = retrievedEntity.BoundaryDataManager.DataTables.ToArray();
            Assert.AreEqual(1, dataTables.Length);
            Assert.AreEqual("A", dataTables[0].Name);
            Assert.IsTrue(dataTables[0].DataFile.IsOpen);
            Assert.AreEqual("test", dataTables[0].DataFile.Content);
            Assert.IsTrue(dataTables[0].SubstanceUseforFile.IsOpen);
            Assert.AreEqual("bla", dataTables[0].SubstanceUseforFile.Content);

            DataTable[] loadsTables = retrievedEntity.LoadsDataManager.DataTables.ToArray();
            Assert.AreEqual(1, loadsTables.Length);
            Assert.AreEqual("B", loadsTables[0].Name);
            Assert.IsTrue(loadsTables[0].DataFile.IsOpen);
            Assert.AreEqual("test", loadsTables[0].DataFile.Content);
            Assert.IsTrue(loadsTables[0].SubstanceUseforFile.IsOpen);
            Assert.AreEqual("bla", loadsTables[0].SubstanceUseforFile.Content);

            // assert settings
            Assert.IsNotNull(retrievedEntity.ModelSettings);
            Assert.AreEqual(false, retrievedEntity.ModelSettings.ClosureErrorCorrection);
            Assert.AreEqual(6, retrievedEntity.ModelSettings.NrOfThreads);
            Assert.AreEqual(false, retrievedEntity.ModelSettings.ClosureErrorCorrection);
            Assert.AreEqual(0.2, retrievedEntity.ModelSettings.DryCellThreshold);
            Assert.AreEqual(10, retrievedEntity.ModelSettings.IterationMaximum);
            Assert.AreEqual(1, retrievedEntity.ModelSettings.Tolerance);
            Assert.AreEqual(true, retrievedEntity.ModelSettings.WriteIterationReport);
        }
        
        [Test]
        [Category(TestCategory.Slow)]
        public void SaveAndRetrieveStandAloneWaterQualityModelWithHydFileImported()
        {
            string filePath = TestHelper.GetTestFilePath(@"IO\real\uni3d.hyd");
            using (var app = CreateApplication())
            {
                app.Run();
                IProjectService projectService = app.ProjectService;
                Project project = projectService.CreateProject();

                var importedWaq = (WaterQualityModel) new HydFileImporter().ImportItem(filePath);

                project.RootFolder.Add(importedWaq);

                double middleHeight = (importedWaq.ZTop + importedWaq.ZBot) / 2;
                importedWaq.ObservationPoints.Add(new WaterQualityObservationPoint {Z = middleHeight});
                importedWaq.Loads.Add(new WaterQualityLoad {Z = middleHeight});

                var startTime = new DateTime(2015, 3, 24, 11, 15, 0);
                var timeStep = new TimeSpan(0, 0, 10);
                var stopTime = new DateTime(2015, 3, 24, 11, 16, 0);

                importedWaq.StartTime = startTime;
                importedWaq.TimeStep = timeStep;
                importedWaq.StopTime = stopTime;

                const string boundaryAlias = "I need somebody to love";
                importedWaq.Boundaries[0].LocationAliases = boundaryAlias;

                importedWaq.ObservationAreas.SetValuesAsLabels(Enumerable.Repeat("Model wide", importedWaq.Grid.Cells.Count));

                // call
                string savePath = Path.GetRandomFileName() + ".dsproj";
                projectService.SaveProjectAs(savePath);

                projectService.CloseProject();

                project = projectService.OpenProject(savePath);
                WaterQualityModel openedWaq = project.RootFolder.Models.OfType<WaterQualityModel>().FirstOrDefault();
                Assert.IsNotNull(openedWaq);

                // assert
                Assert.AreEqual(filePath, openedWaq.HydroData.ToString());
                Assert.IsFalse(openedWaq.Grid.IsEmpty);
                Assert.AreEqual(importedWaq.Grid.Cells.Count, openedWaq.Grid.Cells.Count);
                Assert.AreEqual(importedWaq.Grid.Vertices.Count, openedWaq.Grid.Vertices.Count);

                Assert.AreEqual(importedWaq.AreasRelativeFilePath, openedWaq.AreasRelativeFilePath);
                Assert.AreEqual(importedWaq.AttributesRelativeFilePath, openedWaq.AttributesRelativeFilePath);
                Assert.AreEqual(importedWaq.FlowsRelativeFilePath, openedWaq.FlowsRelativeFilePath);
                Assert.AreEqual(importedWaq.LengthsRelativeFilePath, openedWaq.LengthsRelativeFilePath);
                Assert.AreEqual(importedWaq.PointersRelativeFilePath, openedWaq.PointersRelativeFilePath);
                Assert.AreEqual(importedWaq.SalinityRelativeFilePath, openedWaq.SalinityRelativeFilePath);
                Assert.AreEqual(importedWaq.ShearStressesRelativeFilePath, openedWaq.ShearStressesRelativeFilePath);
                Assert.AreEqual(importedWaq.SurfacesRelativeFilePath, openedWaq.SurfacesRelativeFilePath);
                Assert.AreEqual(importedWaq.TemperatureRelativeFilePath, openedWaq.TemperatureRelativeFilePath);
                Assert.AreEqual(importedWaq.VerticalDiffusionRelativeFilePath, openedWaq.VerticalDiffusionRelativeFilePath);
                Assert.AreEqual(importedWaq.VolumesRelativeFilePath, openedWaq.VolumesRelativeFilePath);

                Assert.AreEqual(importedWaq.NumberOfHydrodynamicLayers, openedWaq.NumberOfHydrodynamicLayers);
                CollectionAssert.AreEqual(importedWaq.HydrodynamicLayerThicknesses, openedWaq.HydrodynamicLayerThicknesses);
                Assert.AreEqual(importedWaq.NumberOfWaqSegmentLayers, openedWaq.NumberOfWaqSegmentLayers);
                CollectionAssert.AreEqual(importedWaq.NumberOfHydrodynamicLayersPerWaqLayer, openedWaq.NumberOfHydrodynamicLayersPerWaqLayer);

                Assert.AreEqual(importedWaq.ModelType, openedWaq.ModelType);
                Assert.AreEqual(importedWaq.LayerType, openedWaq.LayerType);

                Assert.AreEqual(middleHeight, openedWaq.ObservationPoints[0].Z);
                Assert.AreEqual(middleHeight, openedWaq.Loads[0].Z);

                Assert.AreEqual(startTime, openedWaq.StartTime);
                Assert.AreEqual(timeStep, openedWaq.TimeStep);
                Assert.AreEqual(stopTime, openedWaq.StopTime);

                Assert.AreEqual(6, openedWaq.Boundaries.Count);
                Assert.AreEqual(boundaryAlias, importedWaq.Boundaries[0].LocationAliases);

                CollectionAssert.AreEquivalent(Enumerable.Repeat("model wide", importedWaq.Grid.Cells.Count), importedWaq.ObservationAreas.GetValuesAsLabels());
            }
        }

        [Test] // TOOLS-22124, repro manual test 31-march-2015 11:01
        [Category(TestCategory.Slow)]
        public void SaveAndRetrieveStandAloneWaterQualityModelWithHydFileImportedAndSpatialProcessCoefficient()
        {
            // setup
            string commonFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), "IO");
            string filePath = Path.Combine(commonFilePath, "real", "uni3d.hyd");

            using (var entity = new WaterQualityModel())
            {
                new HydFileImporter().ImportItem(filePath, entity);

                new SubFileImporter().Import(entity.SubstanceProcessLibrary, Path.Combine(commonFilePath, "03d_Tewor2003.sub"));

                IFunctionTypeCreator creator = FunctionTypeCreatorFactory.CreateUnstructuredGridCoverageCreator();
                FunctionTypeCreator.ReplaceFunctionUsingCreator(entity.ProcessCoefficients, entity.ProcessCoefficients.First(), creator, entity);

                // call
                using (WaterQualityModel retrievedEntity = SaveAndRetrieveObject(entity))
                {
                    // assert
                    var processCoefficientsSet = (DataItemSet) retrievedEntity.GetDataItemByTag("ProcessCoefficientsTag");
                    IDataItem dataItem = processCoefficientsSet.DataItems.First();
                    IDataItem unproxiedDataItem = TypeUtils.Unproxy(dataItem);
                    Assert.IsInstanceOf<CoverageSpatialOperationValueConverter>(unproxiedDataItem.ValueConverter);
                    Assert.IsInstanceOf<UnstructuredGridCellCoverage>(unproxiedDataItem.ValueConverter.OriginalValue);
                }
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        [TestCase(1)]
        [TestCase(2)]
        public void SaveLoadTwoCoverages_AssertGrids(int nrOfCoverages)
        {
            var waqModels = new List<WaterQualityModel>();
            var mocks = new MockRepository();
            var app = mocks.DynamicMock<IApplication>();
            var activityRunner = mocks.DynamicMock<IActivityRunner>();

            app.Expect(a => a.ActivityRunner).Return(activityRunner);
            
            var projectService = mocks.Stub<IProjectService>();
            app.Expect(a => a.ProjectService).Return(projectService);
            
            mocks.ReplayAll();

            var _ = new WaterQualityModelApplicationPlugin {Application = app};

            string commonFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), "IO");

            string filePath = Path.Combine(commonFilePath, "real", "uni3d.hyd");

            using (var entity = new WaterQualityModel())
            {
                new HydFileImporter().ImportItem(filePath, entity);
                new SubFileImporter().Import(entity.SubstanceProcessLibrary, Path.Combine(commonFilePath, "03d_Tewor2003.sub"));

                Assert.IsTrue(entity.InitialConditions.Count >= nrOfCoverages);

                // make x coverages instead of constant functions
                for (var i = 0; i < nrOfCoverages; i++)
                {
                    FunctionTypeCreator.ReplaceFunctionUsingCreator(
                        entity.InitialConditions, entity.InitialConditions[i],
                        FunctionTypeCreatorFactory.CreateUnstructuredGridCoverageCreator(), entity);
                }

                // assert before
                AssertModelCoverages(entity, nrOfCoverages);

                // save and retrieve
                // call
                Project project = SaveAndRetrieveObjectCore(entity);
                using (var retrievedEntity = RetrievePersistedObjectFromProject<WaterQualityModel>(project))
                {
                    waqModels.Add(retrievedEntity);
                    projectService.Raise(a => a.ProjectOpened += null, this, new EventArgs<Project>(project));

                    // perform spatial operations just like WaterQualityModelApplicationPlugin
                    foreach (CoverageSpatialOperationValueConverter spatialOperationValueConverter in retrievedEntity.AllDataItems.Select(di => di.ValueConverter).OfType<CoverageSpatialOperationValueConverter>())
                    {
                        spatialOperationValueConverter.SpatialOperationSet.Execute();
                    }

                    // assert after
                    Assert.IsNotNull(retrievedEntity);
                    AssertModelCoverages(retrievedEntity, nrOfCoverages);
                }
            }
        }

        [Test]
        public void SaveLoadWaterQualityBoundary()
        {
            var entity = new WaterQualityBoundary();
            const string alias = "my milkshake brings all the boys to the yard";
            entity.LocationAliases = alias;

            WaterQualityBoundary retrieved = SaveAndRetrieveObject(entity);

            Assert.AreEqual(alias, retrieved.LocationAliases);
        }

        [Test]
        public void SaveLoadSetLabelOperation()
        {
            var entity = new SetLabelOperation()
            {
                Label = "zee",
                Name = "Set operation",
                OperationType = PointwiseOperationType.OverwriteWhereMissing
            };

            SetLabelOperation retrieved = SaveAndRetrieveObject(entity);

            Assert.AreEqual(entity.Label, retrieved.Label);
            Assert.AreEqual(entity.Name, retrieved.Name);
            Assert.AreEqual(entity.OperationType, retrieved.OperationType);
        }

        [Test]
        public void SaveLoadOverwriteLabelOperation()
        {
            var entity = new OverwriteLabelOperation()
            {
                Label = "zee",
                Name = "Set operation",
                X = 5,
                Y = 7,
                CoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(2500)
            };

            OverwriteLabelOperation retrieved = SaveAndRetrieveObject(entity);

            Assert.AreEqual(entity.Label, retrieved.Label);
            Assert.AreEqual(entity.Name, retrieved.Name);
            Assert.AreEqual(entity.X, retrieved.X);
            Assert.AreEqual(entity.Y, retrieved.Y);
            Assert.AreEqual(entity.CoordinateSystem, retrieved.CoordinateSystem);
        }

        private static WaterQualityLoad CreateLoad()
        {
            var entity = new WaterQualityLoad
            {
                LoadType = LOADTYPE,
                Name = LOADNAME,
                X = LOAD_X,
                Y = LOAD_Y,
                Z = LOAD_Z
            };

            return entity;
        }

        private static void AssertModelCoverages(WaterQualityModel model, int nrOfCoverages)
        {
            Assert.IsTrue(model.Grid.Cells.Count > 0);

            for (var i = 0; i < nrOfCoverages; i++)
            {
                Assert.IsNotNull(model.InitialConditions[i]);

                IDataItem dataItem = model.AllDataItems.First(
                    di => Equals(di.Value, model.InitialConditions[i]));
                Assert.IsTrue(dataItem.ValueConverter is CoverageSpatialOperationValueConverter);

                Assert.IsTrue(((UnstructuredGridCoverage) model.InitialConditions[i]).Grid.Cells.Count > 0);
            }
        }
        
        private static IApplication CreateApplication()
        {
            return new DHYDROApplicationBuilder().WithWaterQuality().Build();
        }

        protected override NHibernateProjectRepository CreateProjectRepository()
        {
            return new DHYDRONHibernateProjectRepositoryBuilder().WithWaterQuality().Build();
        }
    }
}