using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.KnownStructureProperties;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
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
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Grids;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using Rhino.Mocks;
using SharpMap;
using SharpMap.Extensions.CoordinateSystems;
using SharpMap.SpatialOperations;
using ObservationCrossSection2D = DelftTools.Hydro.ObservationCrossSection2D;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [TestFixture]
    public class WaterFlowFMModelTest
    {
        private MockRepository mocks;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
        }

        [TearDown]
        public void TearDown()
        {
            mocks.VerifyAll();
        }

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
        public void CheckWeirFormulaPropertyChangeEventPropagatesToModel()
        {
            var model = new WaterFlowFMModel();

            var weir = new Weir2D()
            {
                Name = "weir01",
                WeirFormula = new SimpleWeirFormula()
            };

            var collectionChangedCount = 0;
            ((INotifyCollectionChanged) model).CollectionChanged += (s, e) =>
            {
                if (e.Item != weir) return;
                collectionChangedCount++;
            };

            var weirFormulaChangeCount = 0;
            ((INotifyPropertyChanged)model).PropertyChanged += (s, e) =>
            {
                if (e.PropertyName != TypeUtils.GetMemberName<Weir>(w => w.WeirFormula)) return;
                weirFormulaChangeCount++;
            };
            // add weir to model
            model.Area.Weirs.Add(weir);
            Assert.AreEqual(1, collectionChangedCount);
            
            // change weirformula
            weir.WeirFormula = new GeneralStructureWeirFormula();
            Assert.AreEqual(1, weirFormulaChangeCount);
        }

        [Test]
        public void CheckDataItemsAfterChangeOfWeirFormula()
        {
            var model = new WaterFlowFMModel();

            var weir = new Weir2D()
            {
                Name = "weir01",
                WeirFormula = new SimpleWeirFormula()
            };
            model.Area.Weirs.Add(weir);
            
            var dataItems = model.GetChildDataItems(weir).ToList();
            
            Assert.AreEqual(1, dataItems.Count);

            Assert.AreEqual(weir.Name, dataItems[0].Name);
            Assert.AreEqual(KnownStructureProperties.CrestLevel, dataItems[0].Tag);
            Assert.AreEqual(DataItemRole.Input, dataItems[0].Role);
            Assert.AreEqual(weir, ((WaterFlowFMFeatureValueConverter)dataItems[0].ValueConverter).Location);
            Assert.AreEqual(model, ((WaterFlowFMFeatureValueConverter)dataItems[0].ValueConverter).Model);
            Assert.AreEqual(KnownStructureProperties.CrestLevel, ((WaterFlowFMFeatureValueConverter)dataItems[0].ValueConverter).ParameterName);

            // change weir formula
            weir.WeirFormula = new GeneralStructureWeirFormula();
            dataItems = model.GetChildDataItems(weir).ToList();
            Assert.AreEqual(5, dataItems.Count);

            var generalStructureDataItems = new List<string>
            {
                KnownStructureProperties.CrestLevel,
                EnumDescriptionAttributeTypeConverter.GetEnumDescription(KnownGeneralStructureProperties.GateHeight),
                KnownStructureProperties.GateLowerEdgeLevel,
                EnumDescriptionAttributeTypeConverter.GetEnumDescription(KnownGeneralStructureProperties.WidthCenter),
                EnumDescriptionAttributeTypeConverter.GetEnumDescription(KnownGeneralStructureProperties.LevelCenter)
            };
            Assert.That(generalStructureDataItems.Count == dataItems.Count);

            for (int i = 0; i < dataItems.Count; ++i)
            {
                Assert.AreEqual(weir.Name, dataItems[i].Name);
                Assert.AreEqual(generalStructureDataItems[i], dataItems[i].Tag);
                Assert.AreEqual(DataItemRole.Input, dataItems[i].Role);
                Assert.AreEqual(weir, ((WaterFlowFMFeatureValueConverter)dataItems[i].ValueConverter).Location);
                Assert.AreEqual(model, ((WaterFlowFMFeatureValueConverter)dataItems[i].ValueConverter).Model);
                Assert.AreEqual(generalStructureDataItems[i], ((WaterFlowFMFeatureValueConverter)dataItems[i].ValueConverter).ParameterName);
            }
        }

        [Test]
        public void CheckSedimentFormulaPropertyEventPropagatesToModel()
        {
            var model = new WaterFlowFMModel();
            model.ModelDefinition.UseMorphologySediment = true;
            var sedFrac = new SedimentFraction
            {
                Name = "testFrac",
                CurrentSedimentType = SedimentFractionHelper.GetSedimentationTypes()[1],
                CurrentFormulaType = SedimentFractionHelper.GetSedimentationFormulas()[0]
            };
            model.SedimentFractions.Add(sedFrac);

            var modelCount = 0;
            ((INotifyPropertyChanged)model).PropertyChanged += (s, e) =>
            {
                if (e.PropertyName != "IsSpatiallyVarying") return;
                modelCount++;
            };

            var sedFracCount = 0;
            ((INotifyPropertyChanged)sedFrac).PropertyChanged += (s, e) => sedFracCount++;

            var prop = sedFrac.CurrentFormulaType.Properties.OfType<ISpatiallyVaryingSedimentProperty>().First();
            prop.IsSpatiallyVarying = true;

            Assert.AreEqual(1, sedFracCount);
            Assert.AreEqual(1, modelCount); // IsSpatiallyVarying
        }

        [Test]
        public void CheckSedimentPropertyEventPropagatesToModel()
        {
            var model = new WaterFlowFMModel();
            model.ModelDefinition.UseMorphologySediment = true;
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
                    if (line.Contains("** INFO   :"))
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
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        [NUnit.Framework.Category(TestCategory.Slow)]
        public void TestDiaFileIsRetrievedAfterModelRun()
        {
            var mduPath = TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel(mduPath);

            ActivityRunner.RunActivity(model);

            var diaFileDataItem = model.DataItems.FirstOrDefault(di => di.Tag == WaterFlowFMModel.DiaFileDataItemTag);
            Assert.NotNull(diaFileDataItem, "DiaFile not retrieved after model run, check WaterFlowFMModel.DiaFileDataItemTag");
            Assert.NotNull(diaFileDataItem.Value, "DiaFile not retrieved after model run, check WaterFlowFMModel.DiaFileDataItemTag");
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        public void TestWarningGivenIfDiaFileFileNotFound()
        {
            var mduPath = TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel(mduPath);

            var outputDirectory = FileUtils.CreateTempDirectory();
            var diaFileName = string.Format("{0}.dia", model.Name);
            var diaFilePath = Path.Combine(outputDirectory, diaFileName);

            TestHelper.AssertAtLeastOneLogMessagesContains(() =>
                TypeUtils.CallPrivateMethod(model, "ReadDiaFile", new[] { outputDirectory }),
                string.Format(Properties.Resources.WaterFlowFMModel_ReadDiaFile_Could_not_find_log_file___0__at_expected_path___1_, diaFileName, diaFilePath)
            );
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

            var testFile = TestHelper.GetTestFilePath(@"ugrid\Custom_Ugrid.nc");
            Assert.IsTrue(File.Exists(testFile));

            var localCopyOfTestFile = TestHelper.CreateLocalCopy(testFile);

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

        [TestCase(
            new[]
            {
                UnstructuredGridFileHelper.BedLevelLocation.Faces,
                UnstructuredGridFileHelper.BedLevelLocation.NodesMeanLev,
                UnstructuredGridFileHelper.BedLevelLocation.Faces
            }, 
            new[]
            {
                typeof(UnstructuredGridCellCoverage),
                typeof(UnstructuredGridVertexCoverage),
                typeof(UnstructuredGridCellCoverage)
            }
        )]
        [TestCase(
            new[]
            {
                UnstructuredGridFileHelper.BedLevelLocation.NodesMaxLev,
                UnstructuredGridFileHelper.BedLevelLocation.FacesMeanLevFromNodes,
                UnstructuredGridFileHelper.BedLevelLocation.NodesMinLev
            }, 
            new[]
            {
                typeof(UnstructuredGridVertexCoverage),
                typeof(UnstructuredGridCellCoverage),
                typeof(UnstructuredGridVertexCoverage)
            }
        )]
        [TestCase(
            new[]
            {
                UnstructuredGridFileHelper.BedLevelLocation.CellEdges
            }, 
            new[]
            {
                // UnstructuredGridEdgeCoverage not currently supported
                // returns UnstructuredGridVertexCoverage instead
                typeof(UnstructuredGridVertexCoverage) 
            }
        )]

        public void TestUpdateBathymetryCoverage(UnstructuredGridFileHelper.BedLevelLocation[] bedLevelLocations, Type[] coverageTypes)
        {
            // if this is false, the test cases are not correct
            Assert.AreEqual(bedLevelLocations.Length, coverageTypes.Length);

            var fmModel = new WaterFlowFMModel();

            for (var i = 0; i < bedLevelLocations.Length; i++)
            {
                TypeUtils.CallPrivateMethod(fmModel, "UpdateBathymetryCoverage", bedLevelLocations[i]);
                Assert.AreEqual(coverageTypes[i], fmModel.Bathymetry.GetType());
            }
        }

        [TestCase(UnstructuredGridFileHelper.BedLevelLocation.Faces, typeof(UnstructuredGridCellCoverage))]
        [TestCase(UnstructuredGridFileHelper.BedLevelLocation.CellEdges, typeof(UnstructuredGridVertexCoverage))] // UnstructuredGridEdgeCoverages not yet supported
        [TestCase(UnstructuredGridFileHelper.BedLevelLocation.NodesMeanLev, typeof(UnstructuredGridVertexCoverage))]
        [TestCase(UnstructuredGridFileHelper.BedLevelLocation.NodesMinLev, typeof(UnstructuredGridVertexCoverage))]
        [TestCase(UnstructuredGridFileHelper.BedLevelLocation.NodesMaxLev, typeof(UnstructuredGridVertexCoverage))]
        [TestCase(UnstructuredGridFileHelper.BedLevelLocation.FacesMeanLevFromNodes, typeof(UnstructuredGridCellCoverage))]

        public void TestInitializeUnstructuredGridCoveragesSetsCorrectBathymetryCoverageType(UnstructuredGridFileHelper.BedLevelLocation bedLevelLocation, Type coverageType)
        {
            // setup
            var fmModel = new WaterFlowFMModel();

            var bedLevelTypeProperty = fmModel.ModelDefinition.Properties.FirstOrDefault(p => p.PropertyDefinition.MduPropertyName.ToLower() == KnownProperties.BedlevType);
            Assert.NotNull(bedLevelTypeProperty);
            
            bedLevelTypeProperty.SetValueAsString(((int)bedLevelLocation).ToString());
            
            // execution
            TypeUtils.CallPrivateMethod(fmModel, "InitializeUnstructuredGridCoverages");

            // check result
            Assert.AreEqual(coverageType, fmModel.Bathymetry.GetType());
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
            var isPartOf1D2DModelGuiProperty = model.ModelDefinition.GetModelProperty(GuiProperties.PartOf1D2DModel);
            isPartOf1D2DModelGuiProperty.Value = false;

            Assert.IsFalse((bool)isPartOf1D2DModelGuiProperty.Value);
            model.SetVar(new[] {true}, WaterFlowFMModel.IsPartOf1D2DModelPropertyName);
            Assert.IsTrue((bool)isPartOf1D2DModelGuiProperty.Value);
        }

        [Test]
        public void FmModelSetVarDisableFlowNodeRenumbering()
        {
            var model = new WaterFlowFMModel();
            Assert.IsFalse(model.DisableFlowNodeRenumbering);
            model.SetVar(new[] {true}, WaterFlowFMModel.DisableFlowNodeRenumberingPropertyName, null, null);
            Assert.IsTrue(model.DisableFlowNodeRenumbering);
        }

        [Test]
        public void StateInfoRetreivesTheSameNameAndZipPathTest()
        {
            try
            {
                var stateInfo = new StateInfo("StateName", "ZipPath");
                Assert.AreEqual(stateInfo.Name, "StateName");
                Assert.AreEqual(stateInfo.ZipPath, "ZipPath");
            }
            catch (Exception e)
            {
                Assert.Fail("Creation of a StateInfo object should not fail. Exception thrown: {0}.", e.Message);
            }
        }

        [Test]
        public void WriteSnappedFeaturesTest()
        {
            var model = new WaterFlowFMModel();

            /* Default is false */
            Assert.IsFalse(model.WriteSnappedFeatures);
            Assert.AreEqual( model.WriteSnappedFeatures, model.ModelDefinition.WriteSnappedFeatures);

            /* Value is the same in the model definition */
            model.ModelDefinition.WriteSnappedFeatures = true;
            Assert.IsTrue(model.WriteSnappedFeatures);
            Assert.AreEqual(model.WriteSnappedFeatures, model.ModelDefinition.WriteSnappedFeatures);
        }

        [Test]
        public void GivenFmModel_WhenAddingAnAreaFeatureWithGroupNameEqualToPathThatIsPointingToASubFolderOfMduFolder_ThenGroupNameIsAlwaysRelative()
        {
            // Make local copy of project
            const string baseFolderPath = @"HydroAreaCollection/MduFileProjects";
            var filePath = Path.Combine(baseFolderPath, "FlowFM.mdu");
            var mduFilePath = Path.Combine(Directory.GetCurrentDirectory(), filePath);
            TestHelper.CreateLocalCopy(TestHelper.GetTestFilePath(baseFolderPath));

            // Make FM model from Mdu file
            var fmModel = new WaterFlowFMModel(mduFilePath);
            
            // Import dry points
            
            fmModel.Area.DryPoints.Add(new GroupablePointFeature()
            {
                GroupName = Path.Combine(Directory.GetCurrentDirectory(), baseFolderPath, @"SubFolder/MyDryPoints_dry.xyz")
            });
            fmModel.Area.LandBoundaries.Add(new DelftTools.Hydro.LandBoundary2D()
            {
                GroupName = Path.Combine(Directory.GetCurrentDirectory(), baseFolderPath, @"SubFolder/MyLandBoundaries.ldb")
            });

            // Check that group name gives a relative path from the mdu folder
            Assert.That(fmModel.Area.DryPoints.FirstOrDefault().GroupName, Is.EqualTo(@"SubFolder/MyDryPoints_dry.xyz"));
            Assert.That(fmModel.Area.LandBoundaries.FirstOrDefault().GroupName, Is.EqualTo(@"SubFolder/MyLandBoundaries.ldb"));
        }

        [Test]
        public void GivenFmModel_WhenAddingAStructureWithAreaFeatureGroupNameToPathThatIsPointingToASubFolderOfMduFolder_ThenGroupNameIsPointingToItsReferencingStructureFile()
        {
            // Make local copy of project
            const string baseFolderPath = @"HydroAreaCollection/MduFileProjects";
            var filePath = Path.Combine(baseFolderPath, "MduFileWithoutFeatureFileReferences/FlowFM.mdu");
            var mduFilePath = Path.Combine(Directory.GetCurrentDirectory(), filePath);
            TestHelper.CreateLocalCopy(TestHelper.GetTestFilePath(baseFolderPath));

            // Make FM model from Mdu file
            var fmModel = new WaterFlowFMModel(mduFilePath);

            // Import dry points
            fmModel.Area.Gates.Add(new Gate2D
            {
                GroupName = Path.Combine(Directory.GetCurrentDirectory(), baseFolderPath, @"MduFileWithoutFeatureFileReferences/FeatureFiles/gate01.pli")
            });
            fmModel.Area.Pumps.Add(new Pump2D
            {
                GroupName = Path.Combine(Directory.GetCurrentDirectory(), baseFolderPath, @"MduFileWithoutFeatureFileReferences/FeatureFiles/gate01.pli")
            });
            fmModel.Area.Weirs.Add(new Weir2D
            {
                GroupName = Path.Combine(Directory.GetCurrentDirectory(), baseFolderPath, @"MduFileWithoutFeatureFileReferences/FeatureFiles/gate01.pli")
            });

            // Check that group name gives a relative path from the mdu folder
            Assert.That(fmModel.Area.Gates.FirstOrDefault().GroupName, Is.EqualTo(@"FeatureFiles/FlowFM_structures.ini"));
            Assert.That(fmModel.Area.Pumps.FirstOrDefault().GroupName, Is.EqualTo(@"FeatureFiles/FlowFM_structures.ini"));
            Assert.That(fmModel.Area.Weirs.FirstOrDefault().GroupName, Is.EqualTo(@"FeatureFiles/FlowFM_structures.ini"));
        }

        [Test]
        public void GivenFmModel_WhenAddingAStructureWithAreaFeatureGroupNameEqualToPathThatIsNotReferencedByAStructureFile_ThenGroupNameIsEqualToDefaultStructuresFileNameInTheSameFolder()
        {
            // Make local copy of project
            const string baseFolderPath = @"HydroAreaCollection/MduFileProjects";
            var filePath = Path.Combine(baseFolderPath, "MduFileWithoutFeatureFileReferences/FlowFM.mdu");
            var mduFilePath = Path.Combine(Directory.GetCurrentDirectory(), filePath);
            TestHelper.CreateLocalCopy(TestHelper.GetTestFilePath(baseFolderPath));

            // Make FM model from Mdu file
            var fmModel = new WaterFlowFMModel(mduFilePath);

            // Import dry points
            fmModel.Area.Gates.Add(new Gate2D
            {
                GroupName = Path.Combine(Directory.GetCurrentDirectory(), baseFolderPath, @"MduFileWithoutFeatureFileReferences/FeatureFiles/nonReferencedGates.pli")
            });

            // Check that group name gives a relative path from the mdu folder
            Assert.That(fmModel.Area.Gates.FirstOrDefault().GroupName, Is.EqualTo("FeatureFiles/" + fmModel.Name + "_structures.ini"));
        }

        [Test]
        public void GivenFmModel_WhenAddingAnAreaFeatureWithGroupNameToPathThatIsPointingToNotASubFolderOfMduFolder_ThenGroupNameIsEqualToFileName()
        {
            // Make local copy of project
            const string baseFolderPath = @"HydroAreaCollection/MduFileProjects";
            var filePath = Path.Combine(baseFolderPath, "MduFileWithoutFeatureFileReferences/FlowFM.mdu");
            var mduFilePath = Path.Combine(Directory.GetCurrentDirectory(), filePath);
            TestHelper.CreateLocalCopy(TestHelper.GetTestFilePath(baseFolderPath));

            // Make FM model from Mdu file
            var fmModel = new WaterFlowFMModel(mduFilePath);

            // Import dry points
            fmModel.Area.DryAreas.Add(new GroupableFeature2DPolygon()
            {
                GroupName = Path.Combine(Directory.GetCurrentDirectory(), baseFolderPath, @"MyDryAreas_dry.pol")
            });

            // Check that group name gives a relative path from the mdu folder
            Assert.That(fmModel.Area.DryAreas.FirstOrDefault().GroupName, Is.EqualTo(@"MyDryAreas_dry.pol"));
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Slow)]
        public void RunModelWithGeneralStructuresAcceptanceTest()
        {
            var filePath = TestHelper.GetTestFilePath(@"GeneralStructures\BasicModel\FlowFM.mdu");
            Assert.IsTrue( File.Exists(filePath) );
            filePath = TestHelper.CreateLocalCopy(filePath);
            Assert.IsTrue(File.Exists(filePath));

            using (var model = new WaterFlowFMModel(filePath))
            {
                Assert.AreEqual( 2, model.Boundaries.Count);
                Assert.AreEqual( 1, model.Area.Weirs.Count );
                Assert.AreEqual( 3, model.Area.ObservationPoints.Count);

                /* Verify the OP exist and OP1.X < OP2.X < OP2.X and the Y is the same */
                var op1 = model.Area.ObservationPoints[0];
                var op2 = model.Area.ObservationPoints[1];
                var op3 = model.Area.ObservationPoints[2];

                Assert.NotNull(op1);
                Assert.NotNull(op2);
                Assert.NotNull(op3);

                Assert.AreEqual(op1.Geometry.Coordinate.Y, op2.Geometry.Coordinate.Y);
                Assert.AreEqual(op1.Geometry.Coordinate.Y, op3.Geometry.Coordinate.Y);
                Assert.Less(op1.Geometry.Coordinate.X, op2.Geometry.Coordinate.X);
                Assert.Less(op2.Geometry.Coordinate.X, op3.Geometry.Coordinate.X);

                /* Check the Weir currently is a General Structure */
                var weir2D = model.Area.Weirs.First();
                Assert.IsTrue( weir2D.WeirFormula is GeneralStructureWeirFormula);

                //Storing results for the General Structure
                var op1WLResults = new List<double>();
                var op2WLResults = new List<double>();
                var op3WLResults = new List<double>();
                
                var op1VelResults = new List<double>();
                var op2VelResults = new List<double>();
                var op3VelResults = new List<double>();

                try
                {
                    ActivityRunner.RunActivity(model);
                    /*Water Level*/
                    op1WLResults = GetWaterLevelValuesAtPoint(model, op1.Geometry.Coordinate);
                    op2WLResults = GetWaterLevelValuesAtPoint(model, op2.Geometry.Coordinate);
                    op3WLResults = GetWaterLevelValuesAtPoint(model, op3.Geometry.Coordinate);
                    Assert.IsNotEmpty(op1WLResults);
                    Assert.IsNotEmpty(op2WLResults);
                    Assert.IsNotEmpty(op3WLResults);

                    /*Velocity*/
                    op1VelResults = GetVelocityValuesAtPoint(model, op1.Geometry.Coordinate);
                    op2VelResults = GetVelocityValuesAtPoint(model, op2.Geometry.Coordinate);
                    op3VelResults = GetVelocityValuesAtPoint(model, op3.Geometry.Coordinate);

                    Assert.IsNotEmpty(op1VelResults);
                    Assert.IsNotEmpty(op2VelResults);
                    Assert.IsNotEmpty(op3VelResults);
                }
                catch (Exception e)
                {
                    Assert.Fail("Fail to run model: {0}", e.Message);
                }

                #region Check Water Level
                /* OP2TN < OP1TN && OP2TN < OP3TN */
                Assert.AreEqual(op1WLResults.Count, op2WLResults.Count);
                Assert.AreEqual(op1WLResults.Count, op3WLResults.Count);

                var nResults = op1WLResults.Count;
                var lastResult = nResults - 1;

                /* Ensure the LAST time recorded on the OP2 has some water level. */
                var op2ValidValues = op2WLResults.Where(val => val > 0.0).ToList();
                var op3ValidValues = op3WLResults.Where(val => val > 0.0).ToList();

                Assert.IsNotEmpty(op2ValidValues);
                Assert.IsNotEmpty(op3ValidValues);
                
                /* Check at any time the OP2 or OP3 are less than at the same time at OP1
                  and that OP2 results are always Greater Or Equal than OP3 */
                for (int i = 1; i < nResults; i++)
                {
                    Assert.Less(op2WLResults[i], op1WLResults[i]);
                    Assert.Less(op3WLResults[i], op1WLResults[i]);
                    Assert.GreaterOrEqual(op2WLResults[i], op3WLResults[i]);
                }
                #endregion

                #region Check Velocity

                /* OP2TN > OP1TN && OP2TN > OP3TN */
                Assert.AreEqual(op1WLResults.Count, op1VelResults.Count);
                Assert.AreEqual(op1VelResults.Count, op2VelResults.Count);
                Assert.AreEqual(op1VelResults.Count, op3VelResults.Count);

                Assert.Greater(op2VelResults[lastResult], op1VelResults[lastResult]);
                #endregion
            }

            FileUtils.DeleteIfExists(filePath);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.VerySlow)]
        public void ResultsFromWeirGeneralStructuresShouldDifferFromSimpleWeirAcceptanceTest()
        {
            var filePath = TestHelper.GetTestFilePath(@"GeneralStructures\BasicModel\FlowFM.mdu");
            Assert.IsTrue(File.Exists(filePath));
            filePath = TestHelper.CreateLocalCopy(filePath);
            Assert.IsTrue(File.Exists(filePath));

            using (var model = new WaterFlowFMModel(filePath))
            {
                Assert.AreEqual(2, model.Boundaries.Count);
                Assert.AreEqual(1, model.Area.Weirs.Count);
                Assert.AreEqual(3, model.Area.ObservationPoints.Count);

                /* Verify the OP exist and OP1.X < OP2.X < OP2.X and the Y is the same */
                var op1 = model.Area.ObservationPoints[0];
                var op2 = model.Area.ObservationPoints[1];
                var op3 = model.Area.ObservationPoints[2];

                Assert.NotNull(op1);
                Assert.NotNull(op2);
                Assert.NotNull(op3);

                Assert.AreEqual(op1.Geometry.Coordinate.Y, op2.Geometry.Coordinate.Y);
                Assert.AreEqual(op2.Geometry.Coordinate.Y, op3.Geometry.Coordinate.Y);

                Assert.Less(op1.Geometry.Coordinate.X, op2.Geometry.Coordinate.X);
                Assert.Less(op2.Geometry.Coordinate.X, op3.Geometry.Coordinate.X);

                /* Check the Weir currently is a General Structure */
                var weir2D = model.Area.Weirs.First();
                Assert.IsTrue(weir2D.WeirFormula is GeneralStructureWeirFormula);

                //Storing results for the General Structure
                var op1GSResults = new List<double>();
                var op2GSResults = new List<double>();
                var op3GSResults = new List<double>();

               //Storing results for the Simple Weir
                var op1SWResults = new List<double>();
                var op2SWResults = new List<double>();
                var op3SWResults = new List<double>();

                try
                {
                    ActivityRunner.RunActivity(model);
                    /*Water Level*/
                    op1GSResults = GetWaterLevelValuesAtPoint(model, op1.Geometry.Coordinate);
                    op2GSResults = GetWaterLevelValuesAtPoint(model, op2.Geometry.Coordinate);
                    op3GSResults = GetWaterLevelValuesAtPoint(model, op3.Geometry.Coordinate);
                    Assert.IsNotEmpty(op1GSResults);
                    Assert.IsNotEmpty(op2GSResults);
                    Assert.IsNotEmpty(op3GSResults);

                    //Change weir to another kind of formula.
                    weir2D.WeirFormula = new SimpleWeirFormula();
                    Assert.IsFalse(weir2D.WeirFormula is GeneralStructureWeirFormula);

                    ActivityRunner.RunActivity(model);
                    op1SWResults = GetWaterLevelValuesAtPoint(model, op1.Geometry.Coordinate);
                    op2SWResults = GetWaterLevelValuesAtPoint(model, op2.Geometry.Coordinate);
                    op3SWResults = GetWaterLevelValuesAtPoint(model, op3.Geometry.Coordinate);
                    Assert.IsNotEmpty(op1SWResults);
                    Assert.IsNotEmpty(op2SWResults);
                    Assert.IsNotEmpty(op3SWResults);
                }
                catch (Exception e)
                {
                    Assert.Fail("Fail to run model: {0}", e.Message);
                }

                /* Make sure the results are not the same when being a General Structure or Plain Weir*/
                Assert.AreNotEqual(op1GSResults, op1SWResults);
                Assert.AreNotEqual(op2GSResults, op2SWResults);
                Assert.AreNotEqual(op3GSResults, op3SWResults);
            }

            FileUtils.DeleteIfExists(filePath);
        }

        /* Clone of function from WaterFlowFMModel */
        private static List<double> GetWaterLevelValuesAtPoint(WaterFlowFMModel model, Coordinate measureLocation)
        {
            var result = new List<double>();
            if (model == null || model.OutputWaterLevel == null || model.OutputWaterLevel.Time == null)
            {
                Assert.Fail("Water level coverage not found.");
            }

            foreach (var time in model.OutputWaterLevel.Time.Values)
                result.Add((double)model.OutputWaterLevel.Evaluate(measureLocation, time));

            return result;
        }
        /* Custom made function to retreive velocity */
        private static List<double> GetVelocityValuesAtPoint(WaterFlowFMModel model, Coordinate measureLocation)
        {
            var result = new List<double>();
            if (model == null || model.OutputMapFileStore == null ||
                model.OutputMapFileStore.CustomVelocityCoverage == null)
            {
                Assert.Fail("Velocity coverage not found.");
            }

            var velocity = model.OutputMapFileStore.CustomVelocityCoverage as UnstructuredGridCellCoverage;
            if (velocity == null || velocity.Time == null)
            {
                Assert.Fail("Velocity coverage not found.");
            }

            foreach (var time in velocity.Time.Values)
                result.Add((double)velocity.Evaluate(measureLocation, time));

            return result;
        }
    }
}
