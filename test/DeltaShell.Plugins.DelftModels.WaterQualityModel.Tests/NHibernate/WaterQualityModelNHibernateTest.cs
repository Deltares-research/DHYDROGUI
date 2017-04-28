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

using DeltaShell.IntegrationTestUtils;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.BoundaryData;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.ObservationAreas;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Utils;
using DeltaShell.Plugins.SharpMapGis.SpatialOperations;

using NetTopologySuite.Extensions.Coverages;
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
        #region SetUp / TearDown

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            var waterQualityModelApplicationPlugin = new WaterQualityModelApplicationPlugin();
            factory.AddPlugin(waterQualityModelApplicationPlugin);
            foreach (var dataAccessListener in waterQualityModelApplicationPlugin.CreateDataAccessListeners())
            {
                factory.AddDataAccessListener(dataAccessListener);
            }
        }

        #endregion

/*        protected override void OnPostRetrieve(Project retrievedProject)
        {
            base.OnPostRetrieve(retrievedProject);
            WaterQualityModelApplicationPlugin.ExecuteAllWaterQualitySpatialOperations(retrievedProject);
        }*/

        [Test]
        public void SaveAndRetrieveDataTable()
        {
            // setup
            var folderPath = TestHelper.GetCurrentMethodName();
            FileUtils.DeleteIfExists(folderPath);
            Directory.CreateDirectory(folderPath);

            var dataFilePath = Path.Combine(folderPath, "A.tbl");
            const string datafileContents = "datafile";
            File.WriteAllText(dataFilePath, datafileContents);

            var useforFilePath = Path.Combine(folderPath, "A.usefors");
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
                var retrievedEntity = SaveAndRetrieveObject(entity);

                // assert
                Assert.AreEqual("A", retrievedEntity.Name);
                Assert.IsFalse(retrievedEntity.IsEnabled);

                var retrievedDataFileOnDisk = retrievedEntity.DataFile;
                Assert.IsFalse(retrievedDataFileOnDisk.ReadOnly);
                Assert.IsTrue(retrievedDataFileOnDisk.IsOpen);
                Assert.AreEqual(dataFilePath, retrievedDataFileOnDisk.Path);
                Assert.AreEqual(datafileContents, retrievedDataFileOnDisk.Content);

                var retrievedSubstanceFileOnDisk = retrievedEntity.SubstanceUseforFile;
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
            var grid = UnstructuredGridTestHelper.GenerateRegularGrid(5, 5, 1, 1);
            var entity = new WaterQualityObservationAreaCoverage(grid);
            foreach (var i in Enumerable.Range(0, grid.Cells.Count-5))
            {
                entity[i] = i % 3;
            }

            string noDataLabel = WaterQualityObservationAreaCoverage.NoDataLabel;
            var expectedLabels = new[] { 
                "zero", "one", "two", "zero", "one",
                "two", "zero", "one", "two", "zero",
                "one", "two", "zero", "one", "two",
                "zero", "one", "two", "zero", "one",
                "two", "zero", noDataLabel, noDataLabel, noDataLabel};
            entity.SetValuesAsLabels(expectedLabels);
            
            // call
            var retrievedEntity = SaveAndRetrieveObject(entity);

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
        public void SaveAndRetrieveDataTableManager()
        {
            // setup
            var folderPath = TestHelper.GetCurrentMethodName();
            FileUtils.DeleteIfExists(folderPath);
            Directory.CreateDirectory(folderPath);

            try
            {
                var entity = new DataTableManager { Name = "<name>", FolderPath = folderPath };
                entity.CreateNewDataTable("A", "datatablecontents A", "A.usefors", "useforscontents A");
                entity.CreateNewDataTable("B", "datatablecontents B", "B.usefors", "useforscontents B");
                entity.DataTables.First().IsEnabled = false;

                // call
                var retrievedEntity = SaveAndRetrieveObject(entity);

                // assert
                Assert.AreEqual(folderPath, retrievedEntity.FolderPath);
                Assert.AreEqual("<name>", retrievedEntity.Name);

                var dataTables = retrievedEntity.DataTables.ToArray();
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
            var filePath = Path.Combine(TestHelper.GetDataDir(), "IO", "real", "uni3d.hyd");
            var entity = new HydFileData
            {
                Path = new FileInfo(filePath), 
                Checksum = "123456789abcdeffedcba987654321"
            };

            // call
            var retrievedEntity = SaveAndRetrieveObject(entity);

            // assert
            Assert.AreEqual(filePath, retrievedEntity.Path.FullName);
            Assert.AreEqual("123456789abcdeffedcba987654321", retrievedEntity.Checksum);
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
            var retrievedEntity = SaveAndRetrieveObject(entity);

            // assert
            Assert.AreEqual(type, retrievedEntity.ObservationPointType);

            Assert.AreEqual("Observation Point 1", retrievedEntity.Name);
            Assert.AreEqual(1.1, retrievedEntity.X);
            Assert.AreEqual(2.2, retrievedEntity.Y);
            Assert.IsNaN(retrievedEntity.Z);
        }

        [Test]
        
        public void SaveAndLoadWaterQualityLoad()
        {
            var entity = CreateLoad();

            var retrievedEntity = SaveAndRetrieveObject(entity);
            Assert.IsNotNull(retrievedEntity);
            Assert.IsNotNull(retrievedEntity.Geometry);
            Assert.AreEqual(LOAD_X, retrievedEntity.X);
            Assert.AreEqual(LOAD_Y, retrievedEntity.Y);
            Assert.AreEqual(LOAD_Z, retrievedEntity.Z);
            Assert.AreEqual(LOADNAME, retrievedEntity.Name);
            Assert.AreEqual(LOADTYPE, retrievedEntity.LoadType);
        }
        [Category(TestCategory.Slow)]
        [Test]
        public void SaveAndRetrieveFunctionFromHydroDynamics()
        {
            // setup
            var filePath = Path.Combine(TestHelper.GetDataDir(), "IO", "real", "uni3d.sal");
            var entity = WaterQualityFunctionFactory.CreateFunctionFromHydroDynamics("Name", 1.2, "Component name", "Unit name", "My Description");
            entity.FilePath = filePath;

            // call
            var retrievedEntity = SaveAndRetrieveObject(entity);

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
            var commonFilePath = Path.Combine(TestHelper.GetDataDir(), "IO");

            var entity = new WaterQualityModel
            {
                Name = "Test",
                VerticalDispersion = 1.1,
                UseAdditionalHydrodynamicVerticalDiffusion = true,
                HorizontalDispersion = 5.5
            };

            TypeUtils.SetPrivatePropertyValue(entity, TypeUtils.GetMemberName<WaterQualityModel>(m => m.LayerType),
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
                new WaterQualityObservationPoint { Name = "obs1" }
            });

            new SubFileImporter().Import(entity.SubstanceProcessLibrary, Path.Combine(commonFilePath, "03d_Tewor2003.sub"));
            Assert.AreEqual(5, entity.InitialConditions.Count, "Precondition: read sub-file contains 5 substances.");

            entity.Loads.Add(CreateLoad());

            var explicitWorkingDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Test", "Bla");
            FileUtils.DeleteIfExists(explicitWorkingDirectory);
            try
            {
                entity.ExplicitWorkingDirectory = explicitWorkingDirectory;
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
                var retrievedEntity = SaveAndRetrieveObject(entity);

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

                var dataTables = retrievedEntity.BoundaryDataManager.DataTables.ToArray();
                Assert.AreEqual(1, dataTables.Length);
                Assert.AreEqual("A", dataTables[0].Name);
                Assert.IsTrue(dataTables[0].DataFile.IsOpen);
                Assert.AreEqual("test", dataTables[0].DataFile.Content);
                Assert.IsTrue(dataTables[0].SubstanceUseforFile.IsOpen);
                Assert.AreEqual("bla", dataTables[0].SubstanceUseforFile.Content);

                var loadsTables = retrievedEntity.LoadsDataManager.DataTables.ToArray();
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
            finally
            {
                FileUtils.DeleteIfExists(explicitWorkingDirectory);
            }
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void SaveAndRetrieveStandaloneWaterQualityModelWithHydFileImported()
        {
            // setup
            var waqModels = new List<WaterQualityModel>();
            var mocks = new MockRepository();
            var app = mocks.DynamicMock<IApplication>();
            var activityRunner = mocks.DynamicMock<IActivityRunner>();
            
            app.Expect(a => a.ActivityRunner).Return(activityRunner);
            app.Expect(a => a.GetAllModelsInProject()).Return(waqModels);
            mocks.ReplayAll();

            var waqAppPlugin = new WaterQualityModelApplicationPlugin {Application = app};

            var filePath = Path.Combine(TestHelper.GetDataDir(), "IO", "real", "uni3d.hyd");

            var entity = new WaterQualityModel();
            new HydFileImporter().ImportItem(filePath, entity);

            var middleHeight = (entity.ZTop + entity.ZBot) / 2;
            entity.ObservationPoints.Add(new WaterQualityObservationPoint { Z = middleHeight});
            entity.Loads.Add(new WaterQualityLoad { Z = middleHeight });

            entity.StartTime = new DateTime(2015, 3, 24, 11, 15, 0);
            entity.TimeStep = new TimeSpan(0, 0, 10);
            entity.StopTime = new DateTime(2015, 3, 24, 11, 16, 0);

            const string boundaryAlias = "I need somebody to love";
            entity.Boundaries[0].LocationAliases = boundaryAlias;

            entity.ObservationAreas.SetValuesAsLabels(Enumerable.Repeat("Model wide", entity.Grid.Cells.Count));

            // call
            var project = SaveAndRetrieveObjectCore(entity);
            var retrievedEntity = RetrievePersistedObjectFromProject<WaterQualityModel>(project);
            
            waqModels.Add(retrievedEntity);
            app.Raise( a => a.ProjectOpened +=null, project);

            // assert
            Assert.AreEqual(filePath, retrievedEntity.HydroData.ToString());
            Assert.IsFalse(retrievedEntity.Grid.IsEmpty);
            Assert.AreEqual(entity.Grid.Cells.Count, retrievedEntity.Grid.Cells.Count);
            Assert.AreEqual(entity.Grid.Vertices.Count, retrievedEntity.Grid.Vertices.Count);

            Assert.AreEqual(entity.AreasRelativeFilePath, retrievedEntity.AreasRelativeFilePath);
            Assert.AreEqual(entity.AttributesRelativeFilePath, retrievedEntity.AttributesRelativeFilePath);
            Assert.AreEqual(entity.FlowsRelativeFilePath, retrievedEntity.FlowsRelativeFilePath);
            Assert.AreEqual(entity.LengthsRelativeFilePath, retrievedEntity.LengthsRelativeFilePath);
            Assert.AreEqual(entity.PointersRelativeFilePath, retrievedEntity.PointersRelativeFilePath);
            Assert.AreEqual(entity.SalinityRelativeFilePath, retrievedEntity.SalinityRelativeFilePath);
            Assert.AreEqual(entity.ShearStressesRelativeFilePath, retrievedEntity.ShearStressesRelativeFilePath);
            Assert.AreEqual(entity.SurfacesRelativeFilePath, retrievedEntity.SurfacesRelativeFilePath);
            Assert.AreEqual(entity.TemperatureRelativeFilePath, retrievedEntity.TemperatureRelativeFilePath);
            Assert.AreEqual(entity.VerticalDiffusionRelativeFilePath, retrievedEntity.VerticalDiffusionRelativeFilePath);
            Assert.AreEqual(entity.VolumesRelativeFilePath, retrievedEntity.VolumesRelativeFilePath);

            Assert.AreEqual(entity.NumberOfHydrodynamicLayers, retrievedEntity.NumberOfHydrodynamicLayers);
            CollectionAssert.AreEqual(entity.HydrodynamicLayerThicknesses, retrievedEntity.HydrodynamicLayerThicknesses);
            Assert.AreEqual(entity.NumberOfWaqSegmentLayers, retrievedEntity.NumberOfWaqSegmentLayers);
            CollectionAssert.AreEqual(entity.NumberOfHydrodynamicLayersPerWaqLayer, retrievedEntity.NumberOfHydrodynamicLayersPerWaqLayer);

            Assert.AreEqual(entity.ModelType, retrievedEntity.ModelType);
            Assert.AreEqual(entity.LayerType, retrievedEntity.LayerType);

            Assert.AreEqual(middleHeight, retrievedEntity.ObservationPoints[0].Z);
            Assert.AreEqual(middleHeight, retrievedEntity.Loads[0].Z);

            Assert.AreEqual(new DateTime(2015, 3, 24, 11, 15, 0), retrievedEntity.StartTime);
            Assert.AreEqual(new TimeSpan(0, 0, 10), retrievedEntity.TimeStep);
            Assert.AreEqual(new DateTime(2015, 3, 24, 11, 16, 0), retrievedEntity.StopTime);

            Assert.AreEqual(6, retrievedEntity.Boundaries.Count);
            Assert.AreEqual(boundaryAlias, entity.Boundaries[0].LocationAliases);

            CollectionAssert.AreEquivalent(Enumerable.Repeat("model wide", entity.Grid.Cells.Count), entity.ObservationAreas.GetValuesAsLabels());
        }

        [Test]
        [Category(TestCategory.Jira)] // TOOLS-22124, repro manual test 31-march-2015 11:01
        [Category(TestCategory.Slow)]
        public void SaveAndRetrieveStandAloneWaterQualityModelWithHydFileImportedAndSpatialProcessCoefficient()
        {
            // setup
            var commonFilePath = Path.Combine(TestHelper.GetDataDir(), "IO");
            var filePath = Path.Combine(commonFilePath, "real", "uni3d.hyd");

            var entity = new WaterQualityModel();
            new HydFileImporter().ImportItem(filePath, entity);

            new SubFileImporter().Import(entity.SubstanceProcessLibrary, Path.Combine(commonFilePath, "03d_Tewor2003.sub"));

            var creator = FunctionTypeCreatorFactory.CreateUnstructuredGridCoverageCreator();
            FunctionTypeCreator.ReplaceFunctionUsingCreator(entity.ProcessCoefficients, 
                entity.ProcessCoefficients.First(), creator, entity);

            // call
            var retrievedEntity = SaveAndRetrieveObject(entity);

            // assert
            var processCoefficientsSet = (DataItemSet)retrievedEntity.GetDataItemByTag("ProcessCoefficientsTag");
            var dataItem = processCoefficientsSet.DataItems.First();
            var unproxiedDataItem = TypeUtils.Unproxy(dataItem);
            Assert.IsInstanceOf<CoverageSpatialOperationValueConverter>(unproxiedDataItem.ValueConverter);
            Assert.IsInstanceOf<UnstructuredGridCellCoverage>(unproxiedDataItem.ValueConverter.OriginalValue);
        }
        
        private const string LOADTYPE = "Sewer";
        private const string LOADNAME = "Load 1";
        private const double LOAD_X = 0.1d;
        private const double LOAD_Y = 0.2d;
        private const double LOAD_Z = 0.3d;

        private static WaterQualityLoad CreateLoad()
        {
            var entity = new WaterQualityLoad
            {
                LoadType = LOADTYPE, 
                Name = LOADNAME, 
                X = LOAD_X, Y = LOAD_Y, Z = LOAD_Z
            };

            return entity;
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
            app.Expect(a => a.GetAllModelsInProject()).Return(waqModels);
            mocks.ReplayAll();

            var waqAppPlugin = new WaterQualityModelApplicationPlugin { Application = app };

            var commonFilePath = Path.Combine(TestHelper.GetDataDir(), "IO");

            var filePath = Path.Combine(commonFilePath, "real", "uni3d.hyd");

            var entity = new WaterQualityModel();
            new HydFileImporter().ImportItem(filePath, entity);
            new SubFileImporter().Import(entity.SubstanceProcessLibrary, Path.Combine(commonFilePath, "03d_Tewor2003.sub"));

            Assert.IsTrue(entity.InitialConditions.Count >= nrOfCoverages);

            // make x coverages instead of constant functions
            for (int i = 0; i < nrOfCoverages; i++)
            {
                FunctionTypeCreator.ReplaceFunctionUsingCreator(
               entity.InitialConditions, entity.InitialConditions[i],
               FunctionTypeCreatorFactory.CreateUnstructuredGridCoverageCreator(), entity);
            }

            // assert before
            AssertModelCoverages(entity, nrOfCoverages);

            // save and retrieve
            //var retrievedEntity = SaveAndRetrieveObject(entity);
            // call
            var project = SaveAndRetrieveObjectCore(entity);
            var retrievedEntity = RetrievePersistedObjectFromProject<WaterQualityModel>(project);

            waqModels.Add(retrievedEntity);
            app.Raise(a => a.ProjectOpened += null, project);
            
            // perform spatial operations just like WaterQualityModelApplicationPlugin
            foreach (var spatialOperationValueConverter in retrievedEntity.AllDataItems.Select(di => di.ValueConverter).OfType<CoverageSpatialOperationValueConverter>())
            {
                spatialOperationValueConverter.SpatialOperationSet.Execute();
            }

            // assert after
            Assert.IsNotNull(retrievedEntity);
            AssertModelCoverages(retrievedEntity, nrOfCoverages);
        }

        [Test]
        public void SaveLoadWaterQualityBoundary()
        {
            var entity = new WaterQualityBoundary();
            const string alias = "my milkshake brings all the boys to the yard";
            entity.LocationAliases = alias;

            var retrieved = SaveAndRetrieveObject(entity);

            Assert.AreEqual(alias, retrieved.LocationAliases);
        }

        [Test]
        public void SaveLoadSetLabelOperation()
        {
            var entity = new SetLabelOperation()
            {
                Label = "zee",
                Name = "Set operation",
                OperationType = PointwiseOperationType.OverwriteWhereMissing,
            };

            var retrieved = SaveAndRetrieveObject(entity);

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
                CoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(2500),
            };

            var retrieved = SaveAndRetrieveObject(entity);

            Assert.AreEqual(entity.Label, retrieved.Label);
            Assert.AreEqual(entity.Name, retrieved.Name);
            Assert.AreEqual(entity.X, retrieved.X);
            Assert.AreEqual(entity.Y, retrieved.Y);
            Assert.AreEqual(entity.CoordinateSystem, retrieved.CoordinateSystem);
        }

        private static void AssertModelCoverages(WaterQualityModel model, int nrOfCoverages)
        {
            Assert.IsTrue(model.Grid.Cells.Count > 0);

            for (int i = 0; i < nrOfCoverages; i++)
            {
                Assert.IsNotNull(model.InitialConditions[i]);

                var dataItem = model.AllDataItems.First(
                    di => Equals(di.Value, model.InitialConditions[i]));
                Assert.IsTrue(dataItem.ValueConverter is CoverageSpatialOperationValueConverter);

                Assert.IsTrue(((UnstructuredGridCoverage)model.InitialConditions[i]).Grid.Cells.Count > 0);
            }
        }
    }
}