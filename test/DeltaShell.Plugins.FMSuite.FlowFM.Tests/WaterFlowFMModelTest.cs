using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Coverages;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using DeltaShell.Plugins.SharpMapGis.SpatialOperations;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Grids;
using NUnit.Framework;
using SharpMap;
using SharpMap.Extensions.CoordinateSystems;
using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [TestFixture]
    public class WaterFlowFMModelTest
    {
        [Test]
        public void CheckDefaultPropertiesOfFMModel()
        {
            var model = new WaterFlowFMModel();

            Assert.AreEqual(0, model.SnapVersion);
            Assert.IsTrue(model.ValidateBeforeRun);

            // DELFT3DFM-371: Disable Model Inspection
            // Assert.IsTrue(model.ModelInspection);

        }

        [Test]
        public void CheckSedimentFormulaPropertyEventPropagatesToModel()
        {
            var model = new WaterFlowFMModel();
            model.ModelDefinition.GetModelProperty(GuiProperties.UseMorSed).Value = true;
            var sedFrac = new SedimentFraction
            {
                Name = "testFrac",
                CurrentSedimentType = SedimentFractionHelper.GetSedimentationTypes()[1],
                CurrentFormulaType = SedimentFractionHelper.GetSedimentationFormulas()[0]
            };
            model.SedimentFractions.Add(sedFrac);

            var modelCount = 0;
            ((INotifyPropertyChanged)model).PropertyChanged += (s, e) => modelCount++;
            var sedFracCount = 0;
            ((INotifyPropertyChanged)sedFrac).PropertyChanged += (s, e) => sedFracCount++;

            var prop = sedFrac.CurrentFormulaType.Properties.OfType<ISpatiallyVaryingSedimentProperty>().First();
            prop.IsSpatiallyVarying = true;

            Assert.AreEqual(1, sedFracCount);
            Assert.AreEqual(3, modelCount); /* IsSpatiallyVarying + 2 changes id AddOrRenameDataItem */
        }

        [Test]
        public void CheckSedimentPropertyEventPropagatesToModel()
        {
            var model = new WaterFlowFMModel();
            model.ModelDefinition.GetModelProperty(GuiProperties.UseMorSed).Value = true;
            var sedFrac = new SedimentFraction { Name = "testFrac" };
            model.SedimentFractions.Add(sedFrac);

            var modelCount = 0;
            ((INotifyPropertyChanged)model).PropertyChanged += (s, e) => modelCount++;
            var sedFracCount = 0;
            ((INotifyPropertyChanged)sedFrac).PropertyChanged += (s, e) => sedFracCount++;
            
            var prop = sedFrac.CurrentSedimentType.Properties.OfType<ISpatiallyVaryingSedimentProperty>().First();
            prop.IsSpatiallyVarying = true;

            Assert.AreEqual(1, sedFracCount);

            // TODO: Set the assertion value to 3 when initial condition is supported in ext-files (DELFT3DFM-996)
            //Assert.AreEqual(3, modelCount); /* IsSpatiallyVarying + 2 changes id AddOrRenameDataItem */
            Assert.AreEqual(1, modelCount); /* IsSpatiallyVarying + 2 changes id AddOrRenameDataItem */
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Slow)]
        public void RunModelCheckIfStatisticsAreWrittenToDiaFile()
        {
            var mduPath =
                TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            var workingDir = string.Empty;
            using (var model = new WaterFlowFMModel(mduPath))
            {

                ActivityRunner.RunActivity(model);
                Assert.AreEqual(ActivityStatus.Cleaned, model.Status);
                workingDir = Path.Combine(model.WorkingDirectory, model.DirectoryName);
            }
            var statisticsWritten = false;
            Parallel.ForEach(File.ReadAllLines(Path.Combine(workingDir, "bendprof.dia")),
                (line, loopstate) =>
                {
                    if (line.Contains("** INFO   : Computation started  at: "))
                    {
                        statisticsWritten = true;
                        loopstate.Stop();
                    }
                });
            Assert.That(statisticsWritten, Is.True);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Slow)]
        public void CheckFileBasedStatesofFMModel()
        {
            var model1 = new WaterFlowFMModel();
            Assert.AreEqual("FlowFM", model1.Name);
            Assert.AreEqual(null, model1.MduFilePath);

            var mduPath =
                TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var model2 = new WaterFlowFMModel(mduPath);
            Assert.AreEqual("bendprof", model2.Name);
            Assert.AreEqual("bendprof.mdu", Path.GetFileName(model2.MduFilePath));
        }

        [Test]
        public void CreateNewModelCheckStuffIsEmptyButNotNull()
        {
            var model = new WaterFlowFMModel(); // empty model
            Assert.IsTrue(model.Grid.IsEmpty);
            Assert.IsNotNull(model.Bathymetry);
            Assert.AreEqual(0, model.Bathymetry.ToPointCloud().PointValues.Count);
        }

        [Test]
        public void AddInitialSalinityTest()
        {
            // this test checks for SpatialDataLayersChanged() in WaterFlowFMModel.
            var model = new WaterFlowFMModel();

            Assert.AreEqual(1, model.InitialSalinity.Coverages.Count);
            var originalDataItem = model.GetDataItemByValue(model.InitialSalinity.Coverages[0]);
            var originalName = originalDataItem.Name;

            model.InitialSalinity.VerticalProfile = new VerticalProfileDefinition(VerticalProfileType.TopBottom);

            Assert.AreEqual(2, model.InitialSalinity.Coverages.Count);
            Assert.IsNotNull(model.GetDataItemByValue(model.InitialSalinity.Coverages[1]));
                // check if a data item was created

            Assert.AreNotEqual(originalName, model.GetDataItemByValue(model.InitialSalinity.Coverages[0]).Name);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Slow)]
        public void TransformCoordinateSystemTest()
        {
            string mduPath = TestHelper.GetTestFilePath(@"chezy_samples\chezy.mdu");
            var localMduFilePath = TestHelper.CreateLocalCopy(mduPath);

            Map.CoordinateSystemFactory = new OgrCoordinateSystemFactory();
            var factory = Map.CoordinateSystemFactory;
            var model = new WaterFlowFMModel(TestHelper.GetTestFilePath(localMduFilePath));
            model.CoordinateSystem = factory.CreateFromEPSG(28992);

            var newCoordinateSystem = factory.CreateFromEPSG(4326);
            var transformation = factory.CreateTransformation(model.CoordinateSystem, newCoordinateSystem);
            model.TransformCoordinates(transformation);

            Assert.AreEqual(model.CoordinateSystem, newCoordinateSystem);
            Assert.AreEqual(model.Roughness.CoordinateSystem, newCoordinateSystem);

            var roughnessDataItem = model.GetDataItemByValue(model.Roughness);
            var valueConverter = (SpatialOperationSetValueConverter) roughnessDataItem.ValueConverter;

            Assert.AreEqual(model.CoordinateSystem, valueConverter.SpatialOperationSet.CoordinateSystem);
            Assert.AreEqual(model.CoordinateSystem,
                valueConverter.SpatialOperationSet.Operations.Last().CoordinateSystem);
        }

        [Test]
        public void HydFileNameShouldBeBasedOnMduFileName()
        {
            var model = new WaterFlowFMModel {ExplicitWorkingDirectory = "C:\\TestWorkDir"};

            TypeUtils.SetPrivatePropertyValue(model, TypeUtils.GetMemberName(() => model.MduFilePath), "Test.mdu");

            Assert.AreEqual("C:\\TestWorkDir\\DFM_DELWAQ_Test\\Test.hyd", model.HydFilePath);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        [NUnit.Framework.Category(TestCategory.Slow)]
        public void RunModelTwice()
        {
            var mduPath =
                TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel(mduPath);

            ActivityRunner.RunActivity(model);
            var waterLevelFirstRun = (double) model.OutputWaterLevel[model.StopTime, 0];
            Assert.AreEqual(ActivityStatus.Cleaned, model.Status);
            Assert.AreEqual(4.0d, waterLevelFirstRun, 0.1);

            ActivityRunner.RunActivity(model);
            var waterLevelSecondRun = (double) model.OutputWaterLevel[model.CurrentTime, 0];
            Assert.AreEqual(ActivityStatus.Cleaned, model.Status);
            Assert.AreEqual(waterLevelSecondRun, waterLevelFirstRun, 0.005); // value changes per run (see above)

        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        [NUnit.Framework.Category(TestCategory.Slow)]
        public void SetCoordinateSystemOnModelAndExportAdjustsNetFile()
        {
            var mduPath =
                TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            var model = new WaterFlowFMModel(mduPath);

            var tempDir = Path.GetTempFileName();
            File.Delete(tempDir);
            Directory.CreateDirectory(tempDir);

            model.CoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(4326); //wgs84
            model.ExportTo(Path.Combine(tempDir, "cs\\cs.mdu"));

            Assert.AreEqual(4326, NetFile.ReadCoordinateSystem(model.NetFilePath).AuthorityCode);

            model.CoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(28992); //other number
            model.ExportTo(Path.Combine(tempDir, "cs2\\cs2.mdu"));

            Assert.AreEqual(28992, NetFile.ReadCoordinateSystem(model.NetFilePath).AuthorityCode);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        [NUnit.Framework.Category(TestCategory.Slow)]
        public void CheckStartTime()
        {
            var mduPath =
                TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel(mduPath);

            Assert.AreEqual(new DateTime(1992, 08, 31), model.StartTime);

            var newTime = new DateTime(2000, 1, 2, 11, 15, 5, 2); //time with milliseconds
            model.StartTime = newTime;
            Assert.AreEqual(newTime, model.StartTime);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        [NUnit.Framework.Category(TestCategory.Slow)]
        public void CheckCoordinateSystemBendProf()
        {
            var mduPath =
                TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            var model = new WaterFlowFMModel(mduPath);

            Assert.AreEqual(null, model.CoordinateSystem);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        [NUnit.Framework.Category(TestCategory.Slow)]
        public void CheckCoordinateSystemIvoorkust()
        {
            var mduPath = TestHelper.GetTestFilePath(@"mdu_ivoorkust\ivk.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            var model = new WaterFlowFMModel(mduPath);

            Assert.AreEqual("WGS 84", model.CoordinateSystem.Name);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        [NUnit.Framework.Category(TestCategory.Slow)]
        public void ImportIvoorkustModel()
        {
            var mduPath =
                TestHelper.GetTestFilePath(@"mdu_ivoorkust\ivk.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel(mduPath);

            model.Initialize();

            Assert.AreEqual(ActivityStatus.Initialized, model.Status);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        [NUnit.Framework.Category(TestCategory.Slow)]
        public void ImportHarlingen3DModel()
        {
            var mduPath = TestHelper.GetTestFilePath(@"harlingen_model_3d\har.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            var model = new WaterFlowFMModel(mduPath);

            Assert.AreEqual(10, model.DepthLayerDefinition.NumLayers, "depth layers");
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        [NUnit.Framework.Category(TestCategory.Slow)]
        public void ExportTwiceCheckNetFileIsCopiedCorrectly()
        {
            var mduPath =
                TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            var model = new WaterFlowFMModel(mduPath);

            var tempPath1 = Path.GetTempFileName();
            File.Delete(tempPath1);
            Directory.CreateDirectory(tempPath1);

            model.ExportTo(Path.Combine(tempPath1, "test.mdu"), false);

            // delete the first export location
            FileUtils.DeleteIfExists(tempPath1);

            var tempPath2 = Path.GetTempFileName();
            File.Delete(tempPath2);
            Directory.CreateDirectory(tempPath2);

            // export to second export location
            model.ExportTo(Path.Combine(tempPath2, "test.mdu"), false);

            Assert.IsTrue(File.Exists(Path.Combine(tempPath2, "bend1_net.nc")));
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        [NUnit.Framework.Category(TestCategory.Slow)]
        public void LoadingEmptyGridNetFileShouldNotLockIt()
        {
            var mduPath =
                TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            var model = new WaterFlowFMModel(mduPath);
            var gridFile = model.NetFilePath;

            // make grid file corrupt
            File.WriteAllText(gridFile, "");

            // attempt to reload grid
            model.ReloadGrid();

            // make sure we can still delete the file (not locked by mistake)
            File.Delete(gridFile);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        [NUnit.Framework.Category(TestCategory.Slow)]
        public void ImportHarlingenAndCheckTimeSeries()
        {
            var model = new WaterFlowFMModel(TestHelper.GetTestFilePath(@"harlingen\har.mdu"));

            var boundaryCondition =
                model.BoundaryConditions.First(
                    bc => bc is FlowBoundaryCondition && ((Feature2D) bc.Feature).Name == "071_02");

            var refDate = (DateTime) model.ModelDefinition.GetModelProperty(KnownProperties.RefDate).Value;

            var function = boundaryCondition.GetDataAtPoint(0);

            var times = function.Arguments.OfType<IVariable<DateTime>>().First();

            var bcStartTime = times.MinValue;

            Assert.AreEqual(refDate, bcStartTime);

            const double minutes = 4.7520000e+04;

            var bcTimeRange = new TimeSpan(0, 0, (int) minutes, 0);

            var bcStopTime = times.MaxValue;

            Assert.AreEqual(refDate + bcTimeRange, bcStopTime);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        [NUnit.Framework.Category(TestCategory.Slow)]
        public void ReloadGridShouldNotThrowAlotOfEvents()
        {
            var mduPath =
                TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            var model = new WaterFlowFMModel(mduPath);

            int count = 0;
            ((INotifyPropertyChanged) model).PropertyChanged += (s, e) => count++;

            model.ReloadGrid();

            Assert.Less(count, model.Grid.Vertices.Count, "expected few events");

            // if it throws many events it can cause performance problems
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        [NUnit.Framework.Category(TestCategory.Slow)]
        public void LoadManyRoughnessPolygonsForVenice()
        {
            var mduPath =
                TestHelper.GetTestFilePath(@"venice_pilot_22ott2013\n_e04e.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            var model = new WaterFlowFMModel(mduPath);

            Assert.IsTrue(model.ModelDefinition.SpatialOperations.Count > 0);

            var operation =
                (SetValueOperation)
                model.ModelDefinition.GetSpatialOperations(WaterFlowFMModelDefinition.RoughnessDataItemName)[0];
            Assert.IsTrue(operation.Mask.Provider.Features.Count > 1);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        [NUnit.Framework.Category(TestCategory.Slow)]
        public void ImportSpatialOperationsTest()
        {
            var model = new WaterFlowFMModel(TestHelper.GetTestFilePath(@"chezy_samples\chezy.mdu"));

            var valueConverter = model.GetDataItemByValue(model.Roughness).ValueConverter;
            var spatialOperationValueConverter = valueConverter as SpatialOperationSetValueConverter;

            Assert.IsNotNull(spatialOperationValueConverter);

            Assert.AreEqual(2, spatialOperationValueConverter.SpatialOperationSet.Operations.Count);
            Assert.IsTrue(spatialOperationValueConverter.SpatialOperationSet.Operations[1] is InterpolateOperation);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        [NUnit.Framework.Category(TestCategory.Slow)]
        public void ReloadBathymetryTest()
        {
            var model = new WaterFlowFMModel(TestHelper.GetTestFilePath(@"chezy_samples\chezy.mdu"));
            var originalGrid = model.Grid;
            var bathymetryDataItem = model.GetDataItemByValue(model.Bathymetry);
            var spatialOperationValueConverter =
                SpatialOperationValueConverterFactory.GetOrCreateSpatialOperationValueConverter(bathymetryDataItem,
                    model.Bathymetry.Name);

            Assert.IsNotNull(spatialOperationValueConverter);

            var eraseOperation = new EraseOperation();
            Assert.IsNotNull(spatialOperationValueConverter.SpatialOperationSet.AddOperation(eraseOperation));

            model.ReloadGrid(true);

            Assert.IsTrue(spatialOperationValueConverter.SpatialOperationSet.Dirty);

            spatialOperationValueConverter.SpatialOperationSet.Execute();
            var cov =
                spatialOperationValueConverter.SpatialOperationSet.Output.Provider.Features[0] as
                    UnstructuredGridCoverage;

            Assert.IsFalse(originalGrid == model.Grid);
            Assert.IsTrue(cov.Grid == model.Grid);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        [NUnit.Framework.Category(TestCategory.Slow)]
        public void ReloadGridShouldConstructEdges()
        {
            var model = new WaterFlowFMModel(TestHelper.GetTestFilePath(@"chezy_samples\chezy.mdu"));
            new FlowFMNetFileImporter().ImportItem(TestHelper.GetTestFilePath(@"harlingen\fm_003_net.nc"), model);
            Assert.AreEqual(12845, model.Grid.Vertices.Count);
            Assert.AreEqual(16597, model.Grid.Cells.Count);
            Assert.AreEqual(29441, model.Grid.Edges.Count);
        }

        [Test]
        public void ReloadGridShouldSetNoDataValueForBathemetry()
        {

            var model = new WaterFlowFMModel();

            Assert.That(model.Grid.Cells.Count, Is.EqualTo(0));

            var testDir = FileUtils.CreateTempDirectory();
            var localCopyOfTestFile = Path.Combine(testDir, "Custom_Ugrid.nc");
            using (var gridApi = GridApiFactory.CreateNew())
            {
                gridApi.ionc_write_geom_ugrid(localCopyOfTestFile);
            }

            try
            {
                Assert.That(model.Bathymetry.Components[0].NoDataValue, Is.EqualTo(-999.0).Within(0.01));
                TypeUtils.SetPrivatePropertyValue(model, "MduFilePath", @".\");
                model.ModelDefinition.GetModelProperty(KnownProperties.NetFile).Value = localCopyOfTestFile;
                model.ReloadGrid(false);
                Assert.That(model.Grid.Cells.Count, Is.GreaterThan(0));

                Assert.That(model.Bathymetry.Components[0].NoDataValue, Is.EqualTo(-999.0).Within(0.01));
            }
            finally
            {
                FileUtils.DeleteIfExists(localCopyOfTestFile);
            }

        }

        [Test]
        public void FmModelGetVarGridPropertyNameShouldReturnGrid()
        {
            var model = new WaterFlowFMModel();
            var grids = model.GetVar(WaterFlowFMModel.GridPropertyName) as UnstructuredGrid[];
            Assert.IsNotNull(grids);
            Assert.IsNotNull(grids[0]);
            Assert.IsTrue(grids[0].IsEmpty);
        }

        [NUnit.Framework.Category(TestCategory.DataAccess)]
        [NUnit.Framework.Category(TestCategory.Slow)]
        [Test]
        public void FmModelGetVarCellsToFeaturesNameShouldReturnEmptyTimeseries()
        {
            var model = new WaterFlowFMModel(TestHelper.GetTestFilePath(@"flow1d2dLinks\FlowFM.mdu"));
            var timeSeries = model.GetVar(WaterFlowFMModel.CellsToFeaturesName) as ITimeSeries[];
            Assert.IsNotNull(timeSeries);
            Assert.That(timeSeries.Length, Is.EqualTo(9)) ;
        }
        
        [Test]
        public void FmModelSetVarUseNetCDFMapFormat()
        {
            var model = new WaterFlowFMModel();
            Assert.IsFalse(model.UseNetCDFMapFormat);
            model.SetVar(new[] {true}, WaterFlowFMModel.UseNetCDFMapFormatPropertyName, null, null);
            Assert.IsTrue(model.UseNetCDFMapFormat);
        }

        [Test]
        public void FmModelSetVarDisableFlowNodeRenumbering()
        {
            var model = new WaterFlowFMModel();
            Assert.IsFalse(model.DisableFlowNodeRenumbering);
            model.SetVar(new[] {true}, WaterFlowFMModel.DisableFlowNodeRenumberingPropertyName, null, null);
            Assert.IsTrue(model.DisableFlowNodeRenumbering);
        }
    }
}