using System;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.ObservationAreas;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Utils;
using DeltaShell.Plugins.SharpMapGis.SpatialOperations;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using NUnit.Framework;
using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.IO
{
    [TestFixture]
    public class HydFileImporterTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        [ExpectedException(typeof(FileNotFoundException))]
        public void GetTimesForNonexistentFileTest()
        {
            // setup
            string filePath = TestHelper.GetTestFilePath("file does not exist");

            var model = new WaterQualityModel();

            var importer = new HydFileImporter();

            // call
            importer.ImportItem(filePath, model);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportHydFileOnTargetTest()
        {
            string squareHydPath = TestHelper.GetTestFilePath(@"IO\square\square.hyd");
            WaterQualityModel model = new WaterQualityModel();

            var oldGrid = model.Grid;

            var importer = new HydFileImporter();
            var importedItem = importer.ImportItem(squareHydPath, model);

            Assert.AreSame(importedItem, model); // check that no new model was created by the importer
            Assert.IsInstanceOf<HydFileData>(model.HydroData);
            Assert.IsNotNull(model.Grid);
            Assert.AreNotSame(oldGrid, model.Grid, "New grid instance should be set.");
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportHydFileNewModelTest()
        {
            string squareHydPath = TestHelper.GetTestFilePath(@"IO\square\square.hyd");

            var importer = new HydFileImporter();
            var importedItem = importer.ImportItem(squareHydPath);

            Assert.IsNotNull(importedItem);
            Assert.IsInstanceOf<WaterQualityModel>(importedItem);

            WaterQualityModel waqModel = (WaterQualityModel)importedItem;
            Assert.IsInstanceOf<HydFileData>(waqModel.HydroData);

            Assert.IsNotNull(waqModel.Grid);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportGridTest()
        {
            WaterQualityModel model = new WaterQualityModel();

            string squareHydPath = TestHelper.GetTestFilePath(@"IO\square\square.hyd");

            var oldGrid = model.Grid;

            new HydFileImporter().ImportItem(squareHydPath, model);

            Assert.IsNotNull(model.Grid);
            Assert.IsFalse(model.Grid.IsEmpty);
            Assert.AreNotSame(oldGrid, model.Grid, "New grid instance should be set.");
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportTimesTest()
        {
            WaterQualityModel model = new WaterQualityModel();

            string squareHydPath = TestHelper.GetTestFilePath(@"IO\square\square.hyd");

            new HydFileImporter().ImportItem(squareHydPath, model);

            DateTime expectedStartTime = new DateTime(2001, 1, 1, 0, 0, 0);
            DateTime expectedStopTime = new DateTime(2001, 1, 1, 0, 10, 0);
            TimeSpan expectedTimeStep = new TimeSpan(0, 0, 5, 0);
            DateTime expectedReferenceTime = new DateTime(2001, 1, 1, 0, 0, 0);

            Assert.AreEqual(expectedStartTime, model.StartTime);
            Assert.AreEqual(expectedStopTime, model.StopTime);
            Assert.AreEqual(expectedTimeStep, model.TimeStep);
            Assert.AreEqual(expectedReferenceTime, model.ReferenceTime);

            Assert.AreEqual(expectedStartTime, model.ModelSettings.MapStartTime);
            Assert.AreEqual(expectedStopTime, model.ModelSettings.MapStopTime);
            Assert.AreEqual(expectedTimeStep, model.ModelSettings.MapTimeStep);

            Assert.AreEqual(expectedStartTime, model.ModelSettings.HisStartTime);
            Assert.AreEqual(expectedStopTime, model.ModelSettings.HisStopTime);
            Assert.AreEqual(expectedTimeStep, model.ModelSettings.HisTimeStep);

            Assert.AreEqual(expectedStartTime, model.ModelSettings.BalanceStartTime);
            Assert.AreEqual(expectedStopTime, model.ModelSettings.BalanceStopTime);
            Assert.AreEqual(expectedTimeStep, model.ModelSettings.BalanceTimeStep);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ImportBathymetryTest()
        {
            WaterQualityModel model = new WaterQualityModel();

            var oldBathymetry = model.Bathymetry;

            string hydPath = TestHelper.GetTestFilePath(@"IO\real\uni3d.hyd");

            new HydFileImporter().ImportItem(hydPath, model);

            Assert.IsNotNull(model.Bathymetry);
            Assert.AreEqual(-0.2531332006424027, model.Bathymetry.Components[0].Values[0]);
            Assert.IsFalse(model.Bathymetry.IsEditable);
            Assert.AreNotSame(oldBathymetry, model.Bathymetry, "New bathymetry instance should be set.");
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ImportBoundariesTest()
        {
            WaterQualityModel model = new WaterQualityModel();

            Assert.AreEqual(0, model.Boundaries.Count);
            Assert.AreEqual(0, model.BoundaryNodeIds.Count);

            string hydPath = TestHelper.GetTestFilePath(@"IO\real\uni3d.hyd");

            new HydFileImporter().ImportItem(hydPath, model);

            Assert.IsNotNull(model.Boundaries);
            Assert.AreEqual(6, model.Boundaries.Count);
            var expectedBoundaries = new[]
            {
                "sea_002.pli", "sacra_001.pli", "sanjoa_001.pli",
                "yolo_001.pli", "CC.pli", "tracy.pli"
            };
            CollectionAssert.AreEqual(expectedBoundaries, model.Boundaries.Select(b => b.Name).ToArray());

            Assert.IsNotNull(model.BoundaryNodeIds);
            Assert.AreEqual(model.Boundaries.Count, model.BoundaryNodeIds.Count);
            var expectedNumberOfBoundaryNodeIds = new[] { 105, 4, 3, 24, 1, 1 };
            for (int i = 0; i < model.Boundaries.Count; i++)
            {
                var ids = model.BoundaryNodeIds[model.Boundaries[i]];
                Assert.AreEqual(expectedNumberOfBoundaryNodeIds[i], ids.Length);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ImportBulkDataTest()
        {
            WaterQualityModel model = new WaterQualityModel();

            string hydPath = TestHelper.GetTestFilePath(@"IO\real\uni3d.hyd");

            new HydFileImporter().ImportItem(hydPath, model);

            Assert.AreEqual("uni3d.are", model.AreasRelativeFilePath);
            Assert.AreEqual("uni3d.vol", model.VolumesRelativeFilePath);
            Assert.AreEqual("uni3d.flo", model.FlowsRelativeFilePath);
            Assert.AreEqual("uni3d.poi", model.PointersRelativeFilePath);
            Assert.AreEqual("uni3d.len", model.LengthsRelativeFilePath);
            Assert.AreEqual("uni3d.sal", model.SalinityRelativeFilePath);
            Assert.AreEqual(string.Empty, model.TemperatureRelativeFilePath);
            Assert.AreEqual("uni3d.vdf", model.VerticalDiffusionRelativeFilePath);
            Assert.AreEqual("uni3d.srf", model.SurfacesRelativeFilePath);
            Assert.AreEqual("uni3d.tau", model.ShearStressesRelativeFilePath);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ImportMetaDataTest()
        {
            WaterQualityModel model = new WaterQualityModel();

            string hydPath = TestHelper.GetTestFilePath(@"IO\real\uni3d.hyd");

            new HydFileImporter().ImportItem(hydPath, model);

            Assert.IsTrue(model.HasHydroDataImported);
            Assert.AreEqual(HydroDynamicModelType.Unstructured, model.ModelType);
            Assert.AreEqual(0, model.ZTop);
            Assert.AreEqual(1, model.ZBot);
            Assert.AreEqual(788900, model.NumberOfHorizontalExchanges);
            Assert.AreEqual(382884, model.NumberOfVerticalExchanges);
            Assert.AreEqual(7, model.NumberOfHydrodynamicLayers);
            Assert.AreEqual(63814, model.NumberOfDelwaqSegmentsPerHydrodynamicLayer);
            Assert.AreEqual(7, model.NumberOfWaqSegmentLayers);
            Assert.AreEqual(7, model.HydrodynamicLayerThicknesses.Length);
            CollectionAssert.AreEqual(new[] { 0.142857, 0.142857, 0.142857, 0.142857, 0.142857, 0.142857, 0.142857 }, model.HydrodynamicLayerThicknesses);
            Assert.AreEqual(7, model.NumberOfHydrodynamicLayersPerWaqLayer.Length);
            CollectionAssert.AreEqual(new[] { 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0 }, model.NumberOfHydrodynamicLayersPerWaqLayer);
        }

        [Test]
        [Category(TestCategory.Performance)]
        public void PerformImportOfRealisticModelInTheOrderOfSeconds()
        {
            // setup
            WaterQualityModel model = new WaterQualityModel();

            string hydPath = TestHelper.GetTestFilePath(@"IO\real\uni3d.hyd");

            // call & Assert
            TestHelper.AssertIsFasterThan(5000, () =>
            {
                new HydFileImporter().ImportItem(hydPath, model);
            });
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void ImportFromSquareModelAndThenFromRealModelShouldNotUpdateAllData()
        {
            // setup
            var commonFilePath = Path.Combine(TestHelper.GetDataDir(), "IO");

            string hydPath = Path.Combine(commonFilePath, "square", "square.hyd");

            WaterQualityModel model = new WaterQualityModel();

            // 1st import:
            new HydFileImporter().ImportItem(hydPath, model);

            const HydroDynamicModelType expectedModelType = HydroDynamicModelType.Unstructured;
            Assert.AreEqual(expectedModelType, model.ModelType,
                "Test precondition: Checking that the model type between two imports does not change.");

            const double observationHeight = 1.1;
            model.ObservationPoints.AddRange(new[]
            {
                new WaterQualityObservationPoint
                {
                    Z = observationHeight
                },
                new WaterQualityObservationPoint
                {
                    Z = observationHeight
                },
                new WaterQualityObservationPoint
                {
                    Z = observationHeight
                }
            });
            const double loadHeight = 2.2;
            model.Loads.AddRange(new[]
            {
                new WaterQualityLoad
                {
                    Z = loadHeight
                },
                new WaterQualityLoad
                {
                    Z = loadHeight
                },
            });

            new SubFileImporter().Import(model.SubstanceProcessLibrary, Path.Combine(commonFilePath, "03d_Tewor2003.sub"));
            Assert.AreEqual(5, model.InitialConditions.Count,
                "Precondition: read sub-file ");
            var oldSubstances = model.SubstanceProcessLibrary.Substances.ToArray();
            var oldProcesses = model.SubstanceProcessLibrary.Processes.ToArray();
            var oldParameters = model.SubstanceProcessLibrary.Parameters.ToArray();
            var oldOutputParameters = model.SubstanceProcessLibrary.OutputParameters.ToArray();
            var oldInactiveSubstances = model.SubstanceProcessLibrary.InActiveSubstances.ToArray();

            const double constantDefaultValue = 9d;
            ChangeFirstInitialConditionToGridCoverage(model, constantDefaultValue);

            var oldStartTime = model.StartTime;
            var oldTimeStep = model.TimeStep;
            var oldStopTime = model.StopTime;
            var oldGrid = model.Grid;
            var oldBoundaries = model.Boundaries.Select(b => b.Name).ToArray();

            // call, 2nd import:
            hydPath = Path.Combine(commonFilePath, "real", "uni3d.hyd");

            new HydFileImporter().ImportItem(hydPath, model);

            Assert.AreEqual(expectedModelType, model.ModelType,
                "Test precondition: Checking that the model type between two imports does not change.");

            // assert
            // Properties that should be updated:
            Assert.IsTrue(model.HasHydroDataImported);
            Assert.AreEqual(hydPath, model.HydroData.FilePath);
            //Assert.AreEqual(new DateTime(2015,2,5, 12,14,22), hydroDataImporter.GetTimeStamp()); // TODO: Hoe kan een Timestamp te verklaren zijn op een IHydroData interface..?
            Assert.AreNotSame(oldGrid, model.Grid);
            Assert.AreSame(model.Grid, ((UnstructuredGridCellCoverage)model.InitialConditions[0]).Grid);
            foreach (var oldBoundary in oldBoundaries)
            {
                CollectionAssert.DoesNotContain(model.Boundaries.Select(b => b.Name), oldBoundary,
                    "There are not boundaries shared between these two hyd-files; Therefore the old ones should not be kept.");
            }
            foreach (var observationPoint in model.ObservationPoints)
            {
                Assert.AreEqual(observationHeight, observationPoint.Z);
            }
            foreach (var load in model.Loads)
            {
                Assert.AreEqual(loadHeight, load.Z);
            }

            // Properties that should not be changed / updated:
            Assert.AreEqual(oldStartTime, model.StartTime);
            Assert.AreEqual(oldStopTime, model.StopTime);
            Assert.AreEqual(oldTimeStep, model.TimeStep);
            AssertFirstInitialConditionHasSetValueSpatialOperation(model, constantDefaultValue);
            CollectionAssert.AreEqual(oldSubstances, model.SubstanceProcessLibrary.Substances);
            CollectionAssert.AreEqual(oldProcesses, model.SubstanceProcessLibrary.Processes);
            CollectionAssert.AreEqual(oldParameters, model.SubstanceProcessLibrary.Parameters);
            CollectionAssert.AreEqual(oldOutputParameters, model.SubstanceProcessLibrary.OutputParameters);
            CollectionAssert.AreEqual(oldInactiveSubstances, model.SubstanceProcessLibrary.InActiveSubstances);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ImportWithVerticalDiffusionSetsModelUseAdditionalHydrodynamicVerticalDiffusionTrue()
        {
            // setup
            WaterQualityModel model = new WaterQualityModel();

            string hydPath = TestHelper.GetTestFilePath(@"IO\real\uni3d.hyd");

            model.UseAdditionalHydrodynamicVerticalDiffusion = false;

            // call
            new HydFileImporter().ImportItem(hydPath, model);

            // assert
            Assert.IsTrue(model.UseAdditionalHydrodynamicVerticalDiffusion);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ImportWithoutVerticalDiffusionSetsModelUseAdditionalHydrodynamicVerticalDiffusionFalse()
        {
            // setup
            WaterQualityModel model = new WaterQualityModel();

            string hydPath = TestHelper.GetTestFilePath(@"IO\FMHyd2D\waqtest.hyd");

            model.UseAdditionalHydrodynamicVerticalDiffusion = true;

            // call
            new HydFileImporter().ImportItem(hydPath, model);

            // assert
            Assert.IsFalse(model.UseAdditionalHydrodynamicVerticalDiffusion);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void Import_ChangeVerticalDiffusion_Import_ShouldntChange()
        {
            // setup
            var commonFilePath = Path.Combine(TestHelper.GetDataDir(), "IO");

            string hydPath = Path.Combine(commonFilePath, "real", "uni3d.hyd");

            // 1st import:
            WaterQualityModel model = new WaterQualityModel();
            new HydFileImporter().ImportItem(hydPath, model);

            Assert.IsTrue(model.UseAdditionalHydrodynamicVerticalDiffusion);

            model.UseAdditionalHydrodynamicVerticalDiffusion = false;
            Assert.IsFalse(model.UseAdditionalHydrodynamicVerticalDiffusion);

            // 2nd import:
            new HydFileImporter().ImportItem(hydPath, model);

            Assert.IsFalse(model.UseAdditionalHydrodynamicVerticalDiffusion);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void Import_ImportAgainWithoutVerticalDiffusion_ShouldntChange()
        {
            // setup
            var commonFilePath = TestHelper.GetTestFilePath("IO");

            string hydPath = Path.Combine(commonFilePath, "real", "uni3d.hyd");

            WaterQualityModel model = new WaterQualityModel();

            // 1st import:
            new HydFileImporter().ImportItem(hydPath, model);

            Assert.IsNotEmpty(model.HydroData.VerticalDiffusionRelativePath);
            Assert.IsTrue(model.UseAdditionalHydrodynamicVerticalDiffusion);

            // 2nd import:
            new HydFileImporter().ImportItem(Path.Combine(commonFilePath, "square", "square.hyd"), model);

            Assert.IsEmpty(model.HydroData.VerticalDiffusionRelativePath);
            // the vertical diffusion should not be changed, because there was already a value on it
            Assert.IsTrue(model.UseAdditionalHydrodynamicVerticalDiffusion);
        }

        // TODO TOOLS-21848: Create test to verify some boundaries with data are retained, but have their ID's updated

        private static void AssertFirstInitialConditionHasSetValueSpatialOperation(WaterQualityModel model, double defaultValueForCoverage)
        {
            var coverage = (UnstructuredGridCellCoverage)model.InitialConditions[0];
            var dataItem = model.AllDataItems.FirstOrDefault(
                di => Equals(di.Value, coverage));

            Assert.IsNotNull(dataItem, "dataitem should exist.");
            Assert.IsNotNull(dataItem.ValueConverter, "data item should have value converter.");
            Assert.IsInstanceOf<SpatialOperationSetValueConverter>(dataItem.ValueConverter,
                "ValueConverter should be a SpatialOperationSetValueConverter.");

            var vc = (SpatialOperationSetValueConverter)dataItem.ValueConverter;

            Assert.AreEqual(1, vc.SpatialOperationSet.Operations.Count,
                "One spatial operation should be applied by syncing.");
            Assert.IsInstanceOf<SetValueOperation>(vc.SpatialOperationSet.Operations[0],
                "Applied spatial operation should be a set-value operation.");
            var operation = (SetValueOperation)vc.SpatialOperationSet.Operations[0];
            Assert.AreEqual(defaultValueForCoverage, operation.Value,
                "Applied set-value operation should be the default value of the original function.");

            Assert.AreEqual(1, operation.Mask.Provider.GetFeatureCount(),
                "One polygon feature expected.");

            var firstFeature = operation.Mask.Provider.GetFeature(0);
            Assert.IsInstanceOf<IPolygon>(firstFeature.Geometry);

            var polygonMaskGeometry = (IPolygon)firstFeature.Geometry;
            Assert.AreEqual(5, polygonMaskGeometry.NumPoints,
                "Expected square loop, which needs 5 points to close.");

            var extend = coverage.Grid.GetExtents();
            var expectedCoordinates = new []
            {
                new Coordinate(extend.MinX, extend.MinY),
                new Coordinate(extend.MaxX, extend.MinY),
                new Coordinate(extend.MaxX, extend.MaxY),
                new Coordinate(extend.MinX, extend.MaxY),
                new Coordinate(extend.MinX, extend.MinY) // To close the loop of the mask!
            };
            var coordinates = polygonMaskGeometry.Coordinates;

            CollectionAssert.AreEqual(expectedCoordinates, coordinates);
        }

        private static void ChangeFirstInitialConditionToGridCoverage(WaterQualityModel model, double defaultValueForCoverage)
        {
            model.InitialConditions[0].Components[0].DefaultValue = defaultValueForCoverage;

            // This call triggers WaterQualityModelSyncExtensions to create a spatial operation.
            FunctionTypeCreator.ReplaceFunctionUsingCreator(model.InitialConditions,
                model.InitialConditions[0],
                FunctionTypeCreatorFactory.CreateUnstructuredGridCoverageCreator(),
                model);

            // Checking that everything indeed is in the expected state:
            AssertFirstInitialConditionHasSetValueSpatialOperation(model, defaultValueForCoverage);
        }
    }
}
