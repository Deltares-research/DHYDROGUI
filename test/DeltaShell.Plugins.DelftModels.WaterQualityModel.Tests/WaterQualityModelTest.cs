using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DeltaShell.IntegrationTestUtils.Builders;
using DeltaShell.NGHS.Common.IO;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.BoundaryData;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.Model;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.SubstanceProcessLibrary;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Extensions;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Extentions;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Model;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.ObservationAreas;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Properties;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Utils;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.Scripting;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.Toolbox;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using NUnit.Framework;
using Rhino.Mocks;
using SharpMap.Extensions.CoordinateSystems;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests
{
    [TestFixture]
    public class WaterQualityModelTest
    {
        [Test]
        public void DefaultWaqFolderFileStructureTest()
        {
            // setup

            // call
            var model = new WaterQualityModel();

            // assert

            // Expected folder layout:
            // %temp%
            // `-- <temp folder location determined by Waq Model>
            //    |-- <waq_model_name>_output
            //    |   `-- ... model work directory, with includes and *inp file etc.
            //    |-- <waq_model_name>
            //    |  |-- boundary_data_tables
            //    |  |   `-- ... all boundary *.tbl and *.usefors files.
            //    |  |-- load_data_tables
            //    |  |   `-- ...  all load *.tbl and *.usefors files.
            //    |  |-- model_run_output
            //    |  |   `-- ... all output generated from waq run like *.lst, *.his, *.map files.

            string tempPath = Path.GetTempPath();
            StringAssert.StartsWith(tempPath, model.ModelSettings.OutputDirectory,
                                    "Output directory should be located somewhere in temp folder.");
            StringAssert.StartsWith(tempPath, model.BoundaryDataManager.FolderPath,
                                    "Boundary data directory should be located somewhere in temp folder.");
            StringAssert.StartsWith(tempPath, model.LoadsDataManager.FolderPath,
                                    "Loads data directory should be located somewhere in temp folder.");

            Assert.AreEqual(Path.Combine(tempPath, "DeltaShell_Working_Directory", "Water_Quality"), model.ModelSettings.WorkDirectory,
                            "Expected default working directory of DeltaShell");
            string waqModelFolderName = model.Name.Replace(" ", "_");
            Assert.AreEqual(waqModelFolderName, Path.GetFileName(model.ModelSettings.WorkDirectory),
                            "Expected model working directory name should be based on name of waq model");

            string waqModelDataFolder = Path.GetDirectoryName(model.ModelSettings.OutputDirectory);
            Assert.AreEqual(waqModelFolderName, Path.GetFileName(waqModelDataFolder),
                            "Expected waq data directory name should be based on name of waq model without post-fix.");
            Assert.AreEqual(waqModelDataFolder, Path.GetDirectoryName(model.BoundaryDataManager.FolderPath),
                            "Parent folder of boundary data manager should be the waq data directory.");
            Assert.AreEqual(waqModelDataFolder, Path.GetDirectoryName(model.LoadsDataManager.FolderPath),
                            "Parent folder of load data manager should be the waq data directory.");
        }

        [Test]
        public void DefaultConstructorExpectedValuesTest()
        {
            // setup

            // call
            var model = new WaterQualityModel();

            // assert
            Assert.AreEqual("Water Quality", model.Name);
            Assert.IsNotNull(model.ModelSettings);
            Assert.IsNotNull(model.SubstanceProcessLibrary);
            Assert.IsTrue(model.Grid.IsEmpty);
            CollectionAssert.IsEmpty(model.Loads);
            CollectionAssert.IsEmpty(model.InitialConditions);
            CollectionAssert.IsEmpty(model.ProcessCoefficients);
            Assert.IsNotNull(model.ObservationAreas);
            Assert.IsNull(model.HydrodynamicLayerThicknesses);
            Assert.IsNull(model.NumberOfHydrodynamicLayersPerWaqLayer);
            Assert.AreEqual(HydroDynamicModelType.Undefined, model.ModelType);
            Assert.AreEqual(LayerType.Undefined, model.LayerType);
            Assert.IsFalse(model.HasHydroDataImported);

            Assert.AreEqual("Boundary Data", model.BoundaryDataManager.Name);
            Assert.AreEqual("Loads Data", model.LoadsDataManager.Name);

            Assert.AreNotEqual(string.Empty, model.InputFileCommandLine.Content,
                               "Input file must have template set.");
            Assert.AreNotEqual(string.Empty, model.InputFileHybrid.Content,
                               "Input file must have template set.");

            const double expectedVerticalDispersion = 1.0;
            Assert.AreEqual(expectedVerticalDispersion, model.HorizontalDispersion);
            Assert.AreEqual(1e-7, model.VerticalDispersion);
            Assert.IsFalse(model.UseAdditionalHydrodynamicVerticalDiffusion);

            Assert.That(model.UseRestart, Is.False);
            Assert.That(model.WriteRestart, Is.False);
            Assert.That(model.UseSaveStateTimeRange, Is.False);

            #region Default Dispersion Function

            Assert.AreEqual(1, model.Dispersion.Count);
            IFunction dispersionFunction = model.Dispersion[0];
            Assert.AreEqual("Dispersion", dispersionFunction.Name);
            Assert.AreEqual(1, dispersionFunction.Components.Count);
            IVariable dispersionVariable = dispersionFunction.Components[0];
            Assert.AreEqual("Dispersion", dispersionVariable.Name);
            Assert.AreEqual(expectedVerticalDispersion, dispersionVariable.DefaultValue);
            Assert.AreEqual("m2/s", dispersionVariable.Unit.Name);
            Assert.AreEqual("m2/s", dispersionVariable.Unit.Symbol);
            Assert.IsTrue(dispersionFunction.IsConst());

            #endregion
        }

        [Test]
        public void GetKernelVersionsTest()
        {
            // setup
            var model = new WaterQualityModel();

            // call
            string[] kernalVersionsText = model.KernelVersions.Split();

            // assert
            StringAssert.Contains(@"Kernel", kernalVersionsText[0]);
            StringAssert.Contains(@"delwaq.exe", kernalVersionsText[1]);
        }

        [Test]
        public void DefaultWaterQualityModelSettingsTest()
        {
            // setup
            var model = new WaterQualityModel();

            // call
            WaterQualityModelSettings settings = model.ModelSettings;

            // assert
            Assert.AreEqual(new TimeSpan(1, 0, 0, 0), settings.HisStopTime - settings.HisStartTime);
            Assert.AreEqual(new TimeSpan(1, 0, 0, 0), settings.MapStopTime - settings.MapStartTime);
            Assert.AreEqual(new TimeSpan(1, 0, 0, 0), settings.BalanceStopTime - settings.BalanceStartTime);

            Assert.AreEqual(new TimeSpan(0, 1, 0, 0), settings.HisTimeStep);
            Assert.AreEqual(new TimeSpan(0, 1, 0, 0), settings.MapTimeStep);
            Assert.AreEqual(new TimeSpan(0, 1, 0, 0), settings.BalanceTimeStep);

            Assert.AreEqual(BalanceUnit.Gram, settings.BalanceUnit);
            Assert.AreEqual(NumericalScheme.Scheme15, settings.NumericalScheme);

            Assert.IsTrue(settings.NoDispersionIfFlowIsZero);
            Assert.IsTrue(settings.NoDispersionOverOpenBoundaries);

            Assert.IsFalse(settings.UseFirstOrder);

            Assert.IsTrue(settings.LumpProcesses);
            Assert.IsTrue(settings.LumpTransport);
            Assert.IsTrue(settings.LumpLoads);

            Assert.IsTrue(settings.SuppressSpace);
            Assert.IsTrue(settings.SuppressTime);

            Assert.IsTrue(settings.NoBalanceMonitoringPoints);
            Assert.IsTrue(settings.NoBalanceMonitoringAreas);
            Assert.IsFalse(settings.NoBalanceMonitoringModelWide);

            Assert.IsTrue(settings.ProcessesActive);
            Assert.AreEqual(MonitoringOutputLevel.PointsAndAreas, settings.MonitoringOutputLevel);
            Assert.IsTrue(settings.CorrectForEvaporation);
        }

        [Test]
        public void InputDataItemsInitializationTest()
        {
            // setup

            // call
            var model = new WaterQualityModel();

            // assert
            IDataItem inputFileDataItem = model.GetDataItemByTag(WaterQualityModel.InputFileCommandLineDataItemMetaData.Tag);
            Assert.AreEqual(DataItemRole.Input, inputFileDataItem.Role);
            Assert.AreEqual(WaterQualityModel.InputFileCommandLineDataItemMetaData.Name, inputFileDataItem.Name);
            Assert.AreEqual(typeof(TextDocument), inputFileDataItem.ValueType);
            Assert.AreSame(model, inputFileDataItem.Owner);
            Assert.AreSame(model.InputFileCommandLine, inputFileDataItem.Value);

            IDataItem inputFileHybridDataItem = model.GetDataItemByTag(WaterQualityModel.InputFileHybridDataItemMetaData.Tag);
            Assert.AreEqual(DataItemRole.Input, inputFileHybridDataItem.Role);
            Assert.AreEqual(WaterQualityModel.InputFileHybridDataItemMetaData.Name, inputFileHybridDataItem.Name);
            Assert.AreEqual(typeof(TextDocument), inputFileHybridDataItem.ValueType);
            Assert.AreSame(model, inputFileHybridDataItem.Owner);
            Assert.AreSame(model.InputFileHybrid, inputFileHybridDataItem.Value);

            IDataItem substanceProcessLibraryDataItem =
                model.GetDataItemByTag(WaterQualityModel.SubstanceProcessLibraryDataItemMetaData.Tag);
            Assert.AreEqual(DataItemRole.Input, substanceProcessLibraryDataItem.Role);
            Assert.AreEqual(WaterQualityModel.SubstanceProcessLibraryDataItemMetaData.Name,
                            substanceProcessLibraryDataItem.Name);
            Assert.AreEqual(typeof(SubstanceProcessLibrary), substanceProcessLibraryDataItem.ValueType);
            Assert.AreSame(model, substanceProcessLibraryDataItem.Owner);
            Assert.AreSame(model.SubstanceProcessLibrary, substanceProcessLibraryDataItem.Value);

            IDataItem gridDataItem = model.GetDataItemByTag(WaterQualityModel.GridDataItemMetaData.Tag);
            Assert.AreEqual(DataItemRole.Input, gridDataItem.Role);
            Assert.AreEqual(WaterQualityModel.GridDataItemMetaData.Name, gridDataItem.Name);
            Assert.AreEqual(typeof(UnstructuredGrid), gridDataItem.ValueType);
            Assert.AreSame(model, gridDataItem.Owner);
            Assert.AreSame(model.Grid, gridDataItem.Value);

            IDataItem bathymetryDataItem = model.GetDataItemByTag(WaterQualityModel.BathymetryDataItemMetaData.Tag);
            Assert.AreEqual(DataItemRole.Input, bathymetryDataItem.Role);
            Assert.AreEqual(WaterQualityModel.BathymetryDataItemMetaData.Name, bathymetryDataItem.Name);
            Assert.AreEqual(typeof(UnstructuredGridVertexCoverage), bathymetryDataItem.ValueType);
            Assert.AreSame(model, bathymetryDataItem.Owner);
            Assert.AreSame(model.Bathymetry, bathymetryDataItem.Value);

            IDataItem boundaryDataDataItem = model.GetDataItemByTag(WaterQualityModel.BoundaryDataDataItemMetaData.Tag);
            Assert.AreEqual(DataItemRole.Input, boundaryDataDataItem.Role);
            Assert.AreEqual(WaterQualityModel.BoundaryDataDataItemMetaData.Name, boundaryDataDataItem.Name);
            Assert.AreEqual(typeof(DataTableManager), boundaryDataDataItem.ValueType);
            Assert.AreSame(model, boundaryDataDataItem.Owner);
            Assert.AreSame(model.BoundaryDataManager, boundaryDataDataItem.Value);

            IDataItem loadsDataDataItem = model.GetDataItemByTag(WaterQualityModel.LoadsDataDataItemMetaData.Tag);
            Assert.AreEqual(DataItemRole.Input, loadsDataDataItem.Role);
            Assert.AreEqual(WaterQualityModel.LoadsDataDataItemMetaData.Name, loadsDataDataItem.Name);
            Assert.AreEqual(typeof(DataTableManager), loadsDataDataItem.ValueType);
            Assert.AreSame(model, loadsDataDataItem.Owner);
            Assert.AreSame(model.LoadsDataManager, loadsDataDataItem.Value);

            IDataItem initialConditionsDataItemSet =
                model.GetDataItemByTag(WaterQualityModel.InitialConditionsDataItemMetaData.Tag);
            Assert.AreEqual(DataItemRole.Input, initialConditionsDataItemSet.Role);
            Assert.AreEqual(WaterQualityModel.InitialConditionsDataItemMetaData.Name,
                            initialConditionsDataItemSet.Name);
            Assert.AreEqual(typeof(IList<IDataItem>), initialConditionsDataItemSet.ValueType);
            Assert.AreSame(model, initialConditionsDataItemSet.Owner);
            var adapter = (IDataItemsEventedListAdapter) initialConditionsDataItemSet.Value;
            Assert.AreEqual(typeof(IFunction), adapter.ItemType);
            CollectionAssert.AreEqual(model.InitialConditions.ToArray(),
                                      adapter.DataItems.Select(di => di.Value).ToArray());

            IDataItem processCoefficientsDataItemSet =
                model.GetDataItemByTag(WaterQualityModel.ProcessCoefficientsDataItemMetaData.Tag);
            Assert.AreEqual(DataItemRole.Input, processCoefficientsDataItemSet.Role);
            Assert.AreEqual(WaterQualityModel.ProcessCoefficientsDataItemMetaData.Name,
                            processCoefficientsDataItemSet.Name);
            Assert.AreEqual(typeof(IList<IDataItem>), processCoefficientsDataItemSet.ValueType);
            Assert.AreSame(model, processCoefficientsDataItemSet.Owner);
            adapter = (IDataItemsEventedListAdapter) processCoefficientsDataItemSet.Value;
            Assert.AreEqual(typeof(IFunction), adapter.ItemType);
            CollectionAssert.AreEqual(model.ProcessCoefficients.ToArray(),
                                      adapter.DataItems.Select(di => di.Value).ToArray());

            IDataItem dispersionDataItemSet = model.GetDataItemByTag(WaterQualityModel.DispersionDataItemMetaData.Tag);
            Assert.AreEqual(DataItemRole.Input, dispersionDataItemSet.Role);
            Assert.AreEqual(WaterQualityModel.DispersionDataItemMetaData.Name, dispersionDataItemSet.Name);
            Assert.AreEqual(typeof(IList<IDataItem>), dispersionDataItemSet.ValueType);
            Assert.AreSame(model, dispersionDataItemSet.Owner);
            adapter = (IDataItemsEventedListAdapter) dispersionDataItemSet.Value;
            Assert.AreEqual(typeof(IFunction), adapter.ItemType);
            CollectionAssert.AreEqual(model.Dispersion.ToArray(), adapter.DataItems.Select(di => di.Value).ToArray());
        }

        [Test]
        public void GetOutputCoveragesOnModelWithoutOutputDataItemsTest()
        {
            // setup
            var model = new WaterQualityModel();

            // the sets (Monitoring locations, Substances and Ouput parameters) have already been instantiated
            Assert.AreEqual(3, model.DataItems.Count(di => di.Role.HasFlag(DataItemRole.Output)));

            // call
            UnstructuredGridCellCoverage[] outputItems = model.GetOutputCoverages().ToArray();

            // assert
            Assert.IsEmpty(outputItems);
        }

        [Test]
        public void GetOutputCoveragesOnModelWithNonUnstructuredGridCellCoverageAsDataItemTest()
        {
            // setup
            var model = new WaterQualityModel();
            model.DataItems.Add(new DataItem(1.1, DataItemRole.Output, "test"));

            Assert.AreEqual(4,
                            model.DataItems.Count(di =>
                                                      di.Role.HasFlag(DataItemRole
                                                                          .Output))); // the sets (Monitoring locations, Substances and Ouput parameters) and the coverage
            Assert.AreEqual(0,
                            model.DataItems.Count(di =>
                                                      di.Role.HasFlag(DataItemRole.Output) && di.Value is UnstructuredGridCellCoverage));

            // call
            UnstructuredGridCellCoverage[] outputItems = model.GetOutputCoverages().ToArray();

            // assert
            Assert.IsEmpty(outputItems);
        }

        [Test]
        public void GetOutputCoveragesOnModelWithUnstructuredGridCellCoverageAsDataItemTest()
        {
            // setup
            var unstructuredGridCellCoverage = new UnstructuredGridCellCoverage(new UnstructuredGrid(), false);

            var model = new WaterQualityModel();
            model.DataItems.Add(new DataItem(unstructuredGridCellCoverage, DataItemRole.Output, "test"));

            Assert.AreEqual(4,
                            model.DataItems.Count(di =>
                                                      di.Role.HasFlag(DataItemRole
                                                                          .Output))); // the sets (Monitoring locations, Substances and Ouput parameters) and the test coverage
            Assert.AreEqual(1,
                            model.DataItems.Count(di =>
                                                      di.Role.HasFlag(DataItemRole.Output) && di.Value is UnstructuredGridCellCoverage));

            // call
            UnstructuredGridCellCoverage[] outputItems = model.GetOutputCoverages().ToArray();

            // assert
            Assert.AreEqual(1, outputItems.Length);
            Assert.AreSame(unstructuredGridCellCoverage, outputItems[0]);
        }

        [Test]
        public void ClearOutput_ThenOutputFolderPathIsSetToNull()
        {
            // Setup
            using (var model = new WaterQualityModel())
            {
                model.OutputFolder = new FileBasedFolder("path");

                // This field should be false when clearing model output.
                TypeUtils.SetField(model, "outputIsEmpty", false);

                // Call
                model.ClearOutput();

                // Assert
                Assert.That(model.OutputFolder.Path, Is.Null);
            }
        }

        [Test]
        public void ClearOutput_ThenOutputShouldBeDisconnected()
        {
            // Setup
            using (var model = new WaterQualityModel())
            {
                model.OutputFolder = new FileBasedFolder("path");
                var dataItem = new DataItem(new TextDocument(), DataItemRole.Output);
                model.DataItems.Add(dataItem);

                // This field should be false when clearing model output.
                TypeUtils.SetField(model, "outputIsEmpty", false);

                // Call
                model.ClearOutput();

                // Assert
                Assert.IsFalse(model.DataItems.Contains(dataItem),
                               "Model output should be disconnected after model clear output");
            }
        }

        [Test]
        public void ChangeInputCollectionSetsOutputOutOfSync()
        {
            // setup
            var model = new WaterQualityModel();
            SetFakeOutputOnModel(model);

            Assert.IsFalse(model.OutputOutOfSync);

            model.ProcessCoefficients.Add(new Function());

            // assert
            Assert.IsTrue(model.OutputOutOfSync);
        }

        [Test]
        public void SetHorizontalDispersionUpdatesHorizontalDispersionFunctionDefaultValue()
        {
            // setup
            var model = new WaterQualityModel();

            // call
            const double horizontalDispersion = 1.7;
            model.HorizontalDispersion = horizontalDispersion;

            // assert
            Assert.AreEqual(horizontalDispersion, model.Dispersion[0].Components[0].DefaultValue);
        }

        [Test]
        public void CallingImportHydroDataWithNullThrowsArgumentNullException()
        {
            var model = new WaterQualityModel();
            TestDelegate call = () => model.ImportHydroData(null);

            var exception = Assert.Throws<ArgumentNullException>(call);
            Assert.AreEqual("No hydrodynamics data was specified." + Environment.NewLine +
                            "Parameter name: data", exception.Message);

            Assert.IsFalse(model.HasHydroDataImported);
        }

        [Test]
        public void Import_HydroData_OverExistingModel_Overwrites_Timers()
        {
            using (var waqModel = new WaterQualityModel {Name = "Model 1"})
            {
                string testFilePath = TestHelper.GetTestFilePath("IO\\attribute files\\random_3x5.atr");
                var fileData = new HydFileData
                {
                    Path = new FileInfo(testFilePath),
                    AttributesRelativePath = "random_3x5.atr",
                    NumberOfDelwaqSegmentsPerHydrodynamicLayer = 3,
                    NumberOfHydrodynamicLayersPerWaqSegmentLayer = new int[]
                    {
                        0,
                        1,
                        2,
                        3,
                        4
                    },
                    HydrodynamicLayerThicknesses = new double[]
                    {
                        0,
                        1,
                        2,
                        3,
                        4,
                        5,
                        6,
                        7,
                        8,
                        9
                    },
                    NumberOfWaqSegmentLayers = 5,
                    ConversionStartTime = waqModel.StartTime.AddYears(10),
                    ConversionStopTime = waqModel.StopTime.AddYears(10),
                    ConversionTimeStep = waqModel.TimeStep.Add(new TimeSpan(1000)),
                    Boundaries = new EventedList<WaterQualityBoundary>()
                };

                Assert.AreNotEqual(waqModel.StartTime, fileData.ConversionStartTime);
                Assert.AreNotEqual(waqModel.StopTime, fileData.ConversionStopTime);
                Assert.AreNotEqual(waqModel.TimeStep, fileData.ConversionTimeStep);

                //Import the second model on top of waqmodel.
                waqModel.ImportHydroData(fileData);
                Assert.AreEqual(waqModel.StartTime, fileData.ConversionStartTime);
                Assert.AreEqual(waqModel.StopTime, fileData.ConversionStopTime);
                Assert.AreEqual(waqModel.TimeStep, fileData.ConversionTimeStep);
            }
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void HasDataInHydroDynamicsReturnsFalseWhenNoHydroDataImporterSet(bool fromFunction)
        {
            // setup
            var model = new WaterQualityModel();
            Assert.IsNull(model.HydroData);

            IFunction function =
                WaterQualityFunctionFactory.CreateConst("Definitely not in hydro data", 1.2, "No, really!", "g",
                                                        "is it?");

            // call
            bool hasData = fromFunction
                               ? model.HasDataInHydroDynamics(function)
                               : model.HasDataInHydroDynamics(function.Name);

            // assert
            Assert.IsFalse(hasData);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void HasDataInHydroDynamicsReturnsFalseWhenFunctionIsNotAvailable(bool fromFunction)
        {
            // setup
            var model = new WaterQualityModel();
            SetUpModelToHaveFunctionInHydroDataWithName(model, "Test", "<some filepath>");

            IFunction function =
                WaterQualityFunctionFactory.CreateConst("Definitely not in hydro data", 1.2, "No, really!", "g",
                                                        "is it?");

            // call
            bool hasData = fromFunction
                               ? model.HasDataInHydroDynamics(function)
                               : model.HasDataInHydroDynamics(function.Name);

            // assert
            Assert.IsFalse(hasData);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void HasDataInHydroDynamicsReturnsTrueWhenFunctionIsAvailable(bool fromFunction)
        {
            // setup
            const string functionName = "Test";

            var model = new WaterQualityModel();
            SetUpModelToHaveFunctionInHydroDataWithName(model, functionName, "<some filepath>");

            IFunction function = WaterQualityFunctionFactory.CreateConst(functionName, 1.2, "No, really!", "g", "is it?");

            // call
            bool hasData = fromFunction
                               ? model.HasDataInHydroDynamics(function)
                               : model.HasDataInHydroDynamics(function.Name);

            // assert
            Assert.IsTrue(hasData);
        }

        [Test]
        public void GetFilePathFromHydroDynamicsThrowsInvalidOperationExceptionWhenFunctionNotAvailable()
        {
            // setup
            var model = new WaterQualityModel();
            SetUpModelToHaveFunctionInHydroDataWithName(model, "Test", "<some filepath>");

            IFunction function =
                WaterQualityFunctionFactory.CreateConst("Definitely not in hydro data", 1.2, "No, really!", "g",
                                                        "is it?");

            // call
            TestDelegate testCall = () => model.GetFilePathFromHydroDynamics(function);

            // assert
            Assert.Throws<InvalidOperationException>(testCall);
        }

        [Test]
        public void GetFilePathFromHydroDynamicsThrowsInvalidOperationExceptionWhenNoHydroDataImporterSet()
        {
            // setup
            var model = new WaterQualityModel();
            Assert.IsNull(model.HydroData);

            IFunction function =
                WaterQualityFunctionFactory.CreateConst("Definitely not in hydro data", 1.2, "No, really!", "g",
                                                        "is it?");

            // call
            TestDelegate testCall = () => model.GetFilePathFromHydroDynamics(function);

            // assert
            Assert.Throws<InvalidOperationException>(testCall);
        }

        [Test]
        public void GetFilePathFromHydroDynamicsReturnsFilePathWhenFunctionIsAvailable()
        {
            // setup
            const string functionName = "Test";

            var model = new WaterQualityModel();
            const string expectedFilePath = "<some valid filepath>";
            SetUpModelToHaveFunctionInHydroDataWithName(model, functionName, expectedFilePath);

            IFunction function = WaterQualityFunctionFactory.CreateConst(functionName, 1.2, "No, really!", "g", "is it?");

            // call
            string filePath = model.GetFilePathFromHydroDynamics(function);

            // assert
            Assert.AreEqual(expectedFilePath, filePath);
        }

        [Test]
        public void GetCellIdForLocationCalledWithoutImportedHydroDataThrowsInvalidOperationException()
        {
            // setup
            var model = new WaterQualityModel();

            // call
            TestDelegate call = () => model.GetSegmentIndexForLocation(null);

            // assert
            var exception = Assert.Throws<InvalidOperationException>(call);
            Assert.AreEqual("Cannot determine grid cell index for location as no hydro dynamic data was imported.",
                            exception.Message);
        }

        [Test]
        public void TestGetCellIdForLocation()
        {
            // setup
            var hydroData = new TestHydroDataStub
            {
                ThirdCellIsInactive = true,
                ModelType = HydroDynamicModelType.Unstructured
            };

            var model = new WaterQualityModel();
            model.ImportHydroData(hydroData);

            // Grid:
            //  (0,20)    (20,20)
            //     O---O---O
            //     | 3 | 4 |
            //     O---O---O
            //     | 1 | 2 |
            //     O---O---O
            //  (0,0)     (20,0)

            // call & assert
            Assert.AreEqual(1, model.GetSegmentIndexForLocation(new Coordinate(5, 5, 0.5)));
            Assert.AreEqual(3, model.GetSegmentIndexForLocation(new Coordinate(5, 15, 0.5)));
            Assert.AreEqual(2, model.GetSegmentIndexForLocation(new Coordinate(15, 5, 0.5)));
            Assert.AreEqual(4, model.GetSegmentIndexForLocation(new Coordinate(15, 15, 0.5)));
        }

        [Test]
        public void IsInsideActiveCellCalledWithoutImportedHydroDataThrowsInvalidOperationException()
        {
            // setup
            var model = new WaterQualityModel();

            // call
            TestDelegate call = () => model.IsInsideActiveCell(null);

            // assert
            var exception = Assert.Throws<InvalidOperationException>(call);
            Assert.AreEqual("Cannot determine if location is inside active cell as no hydro dynamic data was imported.",
                            exception.Message);
        }

        [Test]
        public void TestIsInsideActiveCell()
        {
            // setup
            var hydroData = new TestHydroDataStub
            {
                ThirdCellIsInactive = true,
                ModelType = HydroDynamicModelType.Unstructured
            };

            var model = new WaterQualityModel();
            model.ImportHydroData(hydroData);

            double z = (model.ZTop + model.ZBot) / 2;

            // Grid:
            //  (0,20)    (20,20)
            //     O---O---O
            //     |2 i|3 a|
            //     O---O---O
            //     |0 a|1 a|
            //     O---O---O
            //  (0,0)     (20,0)
            // legend:
            // first: cell index
            // second: a = active, i = inactive

            var expectedActiveLocations = new[]
            {
                new Coordinate(0 + 1e-6, 0 + 1e-6, z),
                new Coordinate(20 - 1e-6, 0 + 1e-6, z),
                new Coordinate(20 - 1e-6, 20 - 1e-6, z),
                new Coordinate(10 + 1e-6, 20 - 1e-6, z),
                new Coordinate(10 + 1e-6, 10 - 1e-6, z),
                new Coordinate(0 + 1e-6, 10 - 1e-6, z),

                new Coordinate(3, 2, z),
                new Coordinate(17, 8, z),
                new Coordinate(15, 15, z)
            };
            var expectedInactiveLocations = new[]
            {
                new Coordinate(0 + 1e-6, 10 + 1e-6, z),
                new Coordinate(10 - 1e-6, 10 + 1e-6, z),
                new Coordinate(10 - 1e-6, 20 - 1e-6, z),
                new Coordinate(0 + 1e-6, 20 - 1e-6, z),

                new Coordinate(6.7, 13.67, z)
            };

            // call & assert
            foreach (Coordinate expectedActiveLocation in expectedActiveLocations)
            {
                Assert.IsTrue(model.IsInsideActiveCell(expectedActiveLocation),
                              string.Format("Expected coordinate {0} to be active, but was not.", expectedActiveLocation));
            }

            foreach (Coordinate expectedInactiveLocation in expectedInactiveLocations)
            {
                Assert.IsFalse(model.IsInsideActiveCell(expectedInactiveLocation),
                               string.Format("Expected coordinate {0} to be inactive, but actually was active.",
                                             expectedInactiveLocation));
            }
        }

        [Test]
        public void TestGetDirectChildren()
        {
            // setup
            var model = new WaterQualityModel();
            model.Loads.Add(new WaterQualityLoad());
            model.ObservationPoints.Add(new WaterQualityObservationPoint());

            // call
            object[] children = model.GetDirectChildren().ToArray();

            // assert
            CollectionAssert.Contains(children, model.InitialConditions);
            CollectionAssert.Contains(children, model.ProcessCoefficients);
            CollectionAssert.Contains(children, model.Dispersion);
            CollectionAssert.Contains(children, model.ObservationPoints);
            CollectionAssert.Contains(children, model.Loads);
            CollectionAssert.Contains(children, model.BoundaryDataManager);
            CollectionAssert.Contains(children, model.LoadsDataManager);
        }

        [Test]
        public void TestDeepCloneThrowsNotSupportedException()
        {
            // setup
            var model = new WaterQualityModel();

            // call
            TestDelegate call = () => model.DeepClone();

            // assert
            var exception = Assert.Throws<NotSupportedException>(call);
            Assert.AreEqual("WaterQualityModel does not support cloning.", exception.Message);
        }

        [Test]
        public void ChangeCoordinateSystemTest()
        {
            var hydroData = new TestHydroDataStub();
            var model = new WaterQualityModel();

            model.ImportHydroData(hydroData);

            string subFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), "IO", "03d_Tewor2003.sub");
            new SubFileImporter().Import(model.SubstanceProcessLibrary, subFilePath);

            FunctionTypeCreator.ReplaceFunctionUsingCreator(
                model.Dispersion, model.Dispersion[0],
                FunctionTypeCreatorFactory.CreateUnstructuredGridCoverageCreator(), model);
            Assert.IsInstanceOf<ICoverage>(model.Dispersion[0]);

            FunctionTypeCreator.ReplaceFunctionUsingCreator(
                model.InitialConditions, model.InitialConditions[0],
                FunctionTypeCreatorFactory.CreateUnstructuredGridCoverageCreator(), model);
            Assert.IsInstanceOf<ICoverage>(model.InitialConditions[0]);

            FunctionTypeCreator.ReplaceFunctionUsingCreator(
                model.ProcessCoefficients, model.ProcessCoefficients[0],
                FunctionTypeCreatorFactory.CreateUnstructuredGridCoverageCreator(), model);
            Assert.IsInstanceOf<ICoverage>(model.ProcessCoefficients[0]);

            ICoordinateSystem system = new OgrCoordinateSystemFactory().CreateFromEPSG(25000);

            model.CoordinateSystem = system;

            Assert.IsNotNull(model.CoordinateSystem);
            Assert.AreEqual(system, model.Grid.CoordinateSystem);
            Assert.AreEqual(system, model.Bathymetry.CoordinateSystem);
            Assert.AreEqual(system, ((ICoverage) model.Dispersion[0]).CoordinateSystem);

            Assert.AreEqual(system, ((ICoverage) model.InitialConditions[0]).CoordinateSystem);

            Assert.AreEqual(system, model.ObservationAreas.CoordinateSystem);

            Assert.AreEqual(system, ((ICoverage) model.ProcessCoefficients[0]).CoordinateSystem);
        }

        [Test] // DELFT3DFM-464: Waq output always loaded as 'OutOfDate'
        public void TestChangeCoordinateSystemToSameAsExistingDoesNotFirePropertyChangedEventOnGrid()
        {
            var hydroData = new TestHydroDataStub();
            var model = new WaterQualityModel();
            model.ImportHydroData(hydroData);

            ICoordinateSystem system = new OgrCoordinateSystemFactory().CreateFromEPSG(25000);

            var eventCounter = 0;
            model.Grid.CoordinateSystemChanged += coordinateSystem => { eventCounter++; };
            model.CoordinateSystem = system;

            Assert.IsNotNull(model.CoordinateSystem);
            Assert.AreEqual(system, model.Grid.CoordinateSystem);
            Assert.AreEqual(1, eventCounter);

            model.CoordinateSystem = system;

            Assert.AreEqual(system, model.Grid.CoordinateSystem);
            Assert.AreEqual(1, eventCounter,
                            "Changing the CoordinateSystem to the same as existing should not fire a PropertyChangedEvent on the Grid");
        }

        [Test]
        public void CoordinateSystem_SettingCoordinateSystemWithoutPROJ4Transformation_CoordinateSystemUnchanged()
        {
            // Setup
            var mocks = new MockRepository();
            var coordinateSystem = mocks.Stub<ICoordinateSystem>();

            // Note that the DotSpatial reference will throw an ApplicationException during runtime. 
            // However, the documentation is unclear whether this will always be the case and therefore
            // this test assumes that a generic exception will be thrown.
            coordinateSystem.Stub(cs => cs.PROJ4).Throw(new Exception());
            mocks.ReplayAll();

            var hydroData = new TestHydroDataStub();
            using (var model = new WaterQualityModel())
            {
                model.ImportHydroData(hydroData);

                UnstructuredGrid modelGrid = model.Grid;
                ICoordinateSystem originalGridCoordinateSystem = modelGrid.CoordinateSystem;

                // Call
                model.CoordinateSystem = coordinateSystem;

                // Assert
                Assert.That(modelGrid.CoordinateSystem, Is.SameAs(originalGridCoordinateSystem),
                            "CoordinateSystem should not change when there's no transformation to PROJ4 possible.");
                mocks.VerifyAll();
            }
        }

        [Test]
        public void CoordinateSystem_SettingCoordinateSystemToNull_CoordinateSystemUnchanged()
        {
            // Setup
            var hydroData = new TestHydroDataStub();
            using (var model = new WaterQualityModel())
            {
                model.ImportHydroData(hydroData);

                UnstructuredGrid modelGrid = model.Grid;
                ICoordinateSystem originalGridCoordinateSystem = modelGrid.CoordinateSystem;

                // Call
                model.CoordinateSystem = null;

                // Assert
                Assert.That(modelGrid.CoordinateSystem, Is.SameAs(originalGridCoordinateSystem),
                            "CoordinateSystem should not change when it is set to NULL.");
            }
        }

        [Test]
        public void GivenWaterQualityModelWithGridCoordinateSystemWithoutPROJ4Transformation_WhenSettingValidCoordinateSystem_ThenCoordinateSystemSet()
        {
            // Given
            var mocks = new MockRepository();

            // Note that the DotSpatial reference will throw an ApplicationException during runtime. 
            // However, the documentation is unclear whether this will always be the case and therefore
            // this test assumes that a generic exception will be thrown.
            var oldCoordinateSystem = mocks.Stub<ICoordinateSystem>();
            oldCoordinateSystem.Stub(cs => cs.PROJ4).Throw(new Exception());

            var newCoordinateSystem = mocks.Stub<ICoordinateSystem>();
            newCoordinateSystem.Stub(cs => cs.PROJ4).Return("StringRepresentation");
            mocks.ReplayAll();

            var hydroData = new TestHydroDataStub();
            using (var model = new WaterQualityModel())
            {
                model.ImportHydroData(hydroData);

                UnstructuredGrid modelGrid = model.Grid;
                modelGrid.CoordinateSystem = oldCoordinateSystem;

                // When
                model.CoordinateSystem = newCoordinateSystem;

                // Then
                Assert.That(modelGrid.CoordinateSystem, Is.SameAs(newCoordinateSystem),
                            "CoordinateSystem should change when there's a transformation to PROJ4 possible from the new value.");
                mocks.VerifyAll();
            }
        }

        [Test]
        public void GetDefaultZ()
        {
            var model = new WaterQualityModel();

            Assert.AreEqual(LayerType.Undefined, model.LayerType);
            Assert.AreEqual(double.NaN, model.GetDefaultZ());

            TypeUtils.SetField(model, "layerType", LayerType.Sigma);
            Assert.AreEqual(LayerType.Sigma, model.LayerType);
            Assert.AreEqual(0, model.GetDefaultZ());

            TypeUtils.SetField(model, "layerType", LayerType.ZLayer);
            Assert.AreEqual(LayerType.ZLayer, model.LayerType);
            Assert.AreEqual(model.ZTop, model.GetDefaultZ());
        }

        [Test]
        public void CallingCancelShouldCancelOnProcessor()
        {
            var processor = MockRepository.GenerateStrictMock<IWaqProcessor>();

            processor.Expect(p => p.TryToCancel).SetPropertyWithArgument(true);

            processor.Replay();

            var model = new WaterQualityModel();

            TypeUtils.SetField(model, "waqProcessor", processor);

            model.Cancel();

            processor.VerifyAllExpectations();
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void Import_Waq_Model_WithSegmentFiles_OverExistingWaqModel_Update_SegmentFileFunctions()
        {
            string filePath = TestHelper.GetTestFilePath(@"ValidWaqModels\\Flow1D\\sobek.hyd");
            Assert.IsTrue(File.Exists(filePath));

            //Import hyd file
            string westernschedlt = TestHelper.GetTestFilePath(@"WaterQualityDataFiles\ImportHydFileForCoordSystem\DefaultCoordSystem\westernscheldt01.hyd");
            Assert.IsTrue(File.Exists(westernschedlt));

            var importer = new HydFileImporter();
            using (var westernModel = importer.ImportItem(westernschedlt) as WaterQualityModel)
            {
                Assert.IsNotNull(westernModel);
                Assert.IsTrue(string.IsNullOrEmpty(westernModel.ChezyCoefficientsFilePath));

                //Import the substances now.
                string subsFilePath = TestHelper.GetTestFilePath(@"ValidWaqModels\\02b_Oxygen_bod_sediment.sub");
                Assert.IsTrue(File.Exists(subsFilePath));
                Assert.IsNotNull(westernModel.SubstanceProcessLibrary);
                new SubFileImporter().Import(westernModel.SubstanceProcessLibrary, subsFilePath);

                //Check the process has been imported as a segmnent file function
                IFunction chezyProcess = westernModel.ProcessCoefficients.FirstOrDefault(pc => pc.Name.ToLower().Equals("chezy"));
                Assert.IsNotNull(chezyProcess);

                //Make sure the chezyprocess IS NOT a segment file function or FromHydroDynamics
                Assert.IsFalse(chezyProcess is SegmentFileFunction);
                Assert.IsFalse(chezyProcess is FunctionFromHydroDynamics);

                using (var differentModel = importer.ImportItem(filePath) as WaterQualityModel)
                {
                    Assert.IsNotNull(differentModel);
                    string expectedLogMessage = string.Format(Resources.WaterQualityModel_HandleNewHydroDynamicsFunctionDataSet_The_process_coefficient__0__has_been_updated_with_the_latest_Hydrodynamic_data_file_, "CHEZY");
                    TestHelper.AssertAtLeastOneLogMessagesContains(() => westernModel.ImportHydroData(differentModel.HydroData), expectedLogMessage);

                    //Check filepaths, it has been updated.
                    Assert.IsFalse(string.IsNullOrEmpty(westernModel.ChezyCoefficientsFilePath));

                    //Check the process has been updated as a 'FunctionFromHydroDynamics' file function
                    chezyProcess = westernModel.ProcessCoefficients.FirstOrDefault(pc => pc.Name.ToLower().Equals("chezy"));
                    Assert.IsNotNull(chezyProcess);

                    Assert.IsTrue(chezyProcess is FunctionFromHydroDynamics);
                }
            }
        }

        [Test]
        public void WaterQualityModel_WaqProcessesRules_IsInitialized()
        {
            var waqModel = new WaterQualityModel();
            Assert.IsNotNull(waqModel.WaqProcessesRules);
            Assert.IsTrue(waqModel.WaqProcessesRules.Any());
        }

        [Test]
        public void SetOutputFolder_WithValueWithSamePath_ThenOutputFolderIsStillSameObject()
        {
            // Setup
            using (var model = new WaterQualityModel())
            {
                const string path = null;
                var outputFolder = new FileBasedFolder(path);
                model.OutputFolder = outputFolder;

                // Call
                model.OutputFolder = new FileBasedFolder(path);

                // Assert
                Assert.That(model.OutputFolder, Is.SameAs(outputFolder));
            }
        }

        [Test]
        public void SetOutputFolder_ToNull_ThenModelIsDisconnected()
        {
            const string outputTextDocumentTag = "OutputTextDocument";

            // Setup
            using (var waqModel = new WaterQualityModel())
            {
                waqModel.OutputFolder = new FileBasedFolder();
                waqModel.DataItems.Add(new DataItem(new TextDocument(), DataItemRole.Output, outputTextDocumentTag));

                // Precondition
                Assert.That(waqModel.GetDataItemByTag(outputTextDocumentTag), Is.Not.Null, "Precondition violated.");

                // Call
                waqModel.OutputFolder = null;

                // Assert
                Assert.That(waqModel.GetDataItemByTag(outputTextDocumentTag), Is.Null,
                            $"Model should be disconnected when setting {nameof(waqModel.OutputFolder)} to Null.");
            }
        }

        [Test]
        public void SetOutputFolder_ToOutputFolderWithExistingPath_ThenModelIsConnected()
        {
            string dataItemTag = WaterQualityModel.ListFileDataItemMetaData.Tag;

            // Setup
            using (var tempDirectory = new TemporaryDirectory())
            {
                string outputDirectoryPath = tempDirectory.Path;
                File.WriteAllText(Path.Combine(outputDirectoryPath, FileConstants.ListFileName), "");

                using (var model = new WaterQualityModel())
                {
                    // Precondition
                    Assert.That(model.GetDataItemByTag(dataItemTag), Is.Null, "Precondition violated.");

                    // Act
                    model.OutputFolder = new FileBasedFolder(outputDirectoryPath);

                    // Assert
                    Assert.That(model.GetDataItemByTag(dataItemTag), Is.Not.Null,
                                $"When setting the {nameof(model.OutputFolder)} then the model is connected.");
                }
            }
        }

        [Test]
        public void CallingSetWorkingDirectoryInModelSettings_ShouldAddTheModelNameAndSetWorkingDirectoryInModelSettings()
        {
            // Arrange
            var model = new WaterQualityModel {Name = "Name"};

            var workingDirectoryPathFuncWithoutModelName = new Func<string>(() => Path.Combine(Path.GetTempPath(), "test"));
            string expectedWorkingDirectoryPathWithModelName = Path.Combine(workingDirectoryPathFuncWithoutModelName(), "Name");

            // Act
            model.SetWorkingDirectoryInModelSettings(workingDirectoryPathFuncWithoutModelName);

            // Assert
            Assert.AreEqual(expectedWorkingDirectoryPathWithModelName, model.ModelSettings.WorkDirectory);
        }

        [Test]
        public void RenameModel_ShouldUpdateTheWorkingDirectoryInModelSettings()
        {
            // Arrange
            var model = new WaterQualityModel {Name = "FirstName"};

            var workingDirectoryPathFuncWithoutModelName = new Func<string>(() => Path.Combine(Path.GetTempPath(), "test"));

            model.SetWorkingDirectoryInModelSettings(workingDirectoryPathFuncWithoutModelName);

            // Act
            model.Name = "SecondName";

            string expectedWorkingDirectoryPathWithModelName = Path.Combine(workingDirectoryPathFuncWithoutModelName(), "SecondName");

            // Assert
            Assert.AreEqual(expectedWorkingDirectoryPathWithModelName, model.ModelSettings.WorkDirectory);
        }

        private void SetUpModelToHaveFunctionInHydroDataWithName(WaterQualityModel model, string functionName,
                                                                 string someValidFilepath)
        {
            var hydroData = new TestHydroDataStub
            {
                HasDataForInjection = name => name == functionName,
                GetFilePathForInjection = name =>
                {
                    if (name == functionName)
                    {
                        return someValidFilepath;
                    }

                    throw new NotImplementedException();
                }
            };
            model.ImportHydroData(hydroData);
        }

        private void SetFakeOutputOnModel(WaterQualityModel model)
        {
            // Fake the output not being empty or out of sync
            TypeUtils.SetPrivatePropertyValue(model, nameof(WaterQualityModel.OutputIsEmpty), false);
        }

        [TestCase(null)]
        [TestCase("does_not_exist")]
        public void SetOutputFolder_ToOutputFolderWithNonExistingPath_ThenModelIsDisconnected(string folderPath)
        {
            const string outputTextDocumentTag = "OutputTextDocument";

            // Setup
            using (var waqModel = new WaterQualityModel())
            {
                waqModel.DataItems.Add(new DataItem(new TextDocument(), DataItemRole.Output, outputTextDocumentTag));

                // Precondition
                Assert.That(waqModel.GetDataItemByTag(outputTextDocumentTag), Is.Not.Null, "Precondition violated.");

                // Call
                waqModel.OutputFolder = new FileBasedFolder {Path = folderPath};

                // Assert
                Assert.That(waqModel.GetDataItemByTag(outputTextDocumentTag), Is.Null,
                            $"Model should be disconnected when setting {nameof(waqModel.OutputFolder)} to an instance with a path that does not exist.");
            }
        }

        #region ImportHydroData Twice

        [Test] // TOOLS-22039
        public void Import_SameHydFileData_HasHydroDataImportedIsTrue()
        {
            // setup
            var hydroData = new TestHydroDataStub();

            var model = new WaterQualityModel();
            model.ImportHydroData(hydroData);

            Assert.IsTrue(model.HasHydroDataImported);
            Assert.IsTrue(model.HasEverImportedHydroData);

            // call
            model.ImportHydroData(hydroData);

            // assert
            Assert.IsTrue(model.HasHydroDataImported);
            Assert.IsTrue(model.HasEverImportedHydroData);
        }

        [Test]
        public void Import_DifferentModelTypeOnSecondImport_ChangeObservationPointAndLoadCoordinateZToNaN()
        {
            // setup
            var hydroData = new TestHydroDataStub {LayerType = LayerType.Sigma};

            var model = new WaterQualityModel();
            model.ImportHydroData(hydroData);

            model.Loads.AddRange(new[]
            {
                new WaterQualityLoad {Z = 1.1},
                new WaterQualityLoad {Z = 2.2}
            });

            model.ObservationPoints.AddRange(new[]
            {
                new WaterQualityObservationPoint {Z = 3.3},
                new WaterQualityObservationPoint {Z = 4.4}
            });

            hydroData = new TestHydroDataStub {LayerType = LayerType.ZLayer};

            // call
            model.ImportHydroData(hydroData);

            // assert
            foreach (WaterQualityLoad load in model.Loads)
            {
                Assert.AreEqual(0, load.Z);
            }

            foreach (WaterQualityObservationPoint observationPoint in model.ObservationPoints)
            {
                Assert.AreEqual(0, observationPoint.Z);
            }
        }

        [Test]
        public void ImportWithHydData_ImportWithoutHydData_IsConstant()
        {
            const string functionName = "Salinity";
            // 1st import:
            TestHydroDataStub hydroData = CreateHydFileStubWithRelativePathOnFunction(functionName, "lalala.data");

            var model = new WaterQualityModel();
            model.ImportHydroData(hydroData);

            string subFilePath = TestHelper.GetTestFilePath(@"IO\03d_Tewor2003.sub");
            new SubFileImporter().Import(model.SubstanceProcessLibrary, subFilePath);

            IFunction firstFunction = FindFunctionInModel(functionName, model);
            Assert.IsNotNull(firstFunction);
            Assert.IsTrue(firstFunction.IsFromHydroDynamics());

            TestHydroDataStub hydroDataImporter2 = CreateHydFileStubWithRelativePathOnFunction(functionName, string.Empty);

            // 2nd import:
            model.ImportHydroData(hydroDataImporter2);
            IFunction replacedFunction = FindFunctionInModel(functionName, model);

            // the salinity function is now changed to a constant, because there was no data left
            Assert.IsNotNull(replacedFunction);
            Assert.AreNotEqual(firstFunction, replacedFunction); // check if the function has been replaced
            Assert.IsTrue(replacedFunction.IsConst());
        }

        [Test]
        public void ImportWithHydData_SetToConstant_ImportWithHydData_IsConstant()
        {
            const string functionName = "Salinity";

            // 1st import:
            TestHydroDataStub hydroData = CreateHydFileStubWithRelativePathOnFunction(functionName, "lalala.data");

            var model = new WaterQualityModel();
            model.ImportHydroData(hydroData);

            string subFilePath = TestHelper.GetTestFilePath(@"IO\03d_Tewor2003.sub");
            new SubFileImporter().Import(model.SubstanceProcessLibrary, subFilePath);

            IFunction firstFunction = FindFunctionInModel(functionName, model);
            Assert.IsNotNull(firstFunction);
            Assert.IsTrue(firstFunction.IsFromHydroDynamics());

            // set to constant
            IFunctionTypeCreator creator = FunctionTypeCreatorFactory.CreateConstantCreator();
            FunctionTypeCreator.ReplaceFunctionUsingCreator(model.ProcessCoefficients, firstFunction, creator, model);

            IFunction constantFunction = FindFunctionInModel(functionName, model);
            Assert.IsNotNull(constantFunction);
            Assert.AreNotEqual(firstFunction, constantFunction);
            Assert.IsTrue(constantFunction.IsConst());

            // 2nd import:
            model.ImportHydroData(hydroData);

            // the salinity function is still a constant, because the user specified as such
            IFunction replacedFunction = FindFunctionInModel(functionName, model);
            Assert.IsNotNull(replacedFunction);
            //https://issuetracker.deltares.nl/browse/DELFT3DFM-1505 Hyd file leads, so it will be overwritten.
            Assert.AreNotEqual(constantFunction,
                               replacedFunction); // the function should not be replaced, because it is the same
            Assert.IsFalse(replacedFunction.IsConst());
            Assert.IsTrue(replacedFunction.IsFromHydroDynamics());
        }

        private static IFunction FindFunctionInModel(string functionName, WaterQualityModel model)
        {
            return model.ProcessCoefficients.FirstOrDefault(f => f.Name == functionName);
        }

        [Test]
        public void ImportWithoutHydData_ImportWithHydData_IsConstant()
        {
            const string functionName = "Salinity";

            // 1st import:
            TestHydroDataStub hydroData = CreateHydFileStubWithRelativePathOnFunction(functionName, string.Empty);

            var model = new WaterQualityModel();
            model.ImportHydroData(hydroData);

            string subFilePath = TestHelper.GetTestFilePath(@"IO\03d_Tewor2003.sub");
            new SubFileImporter().Import(model.SubstanceProcessLibrary, subFilePath);

            IFunction firstFunction = FindFunctionInModel(functionName, model);
            Assert.IsNotNull(firstFunction);
            Assert.IsTrue(firstFunction.IsConst());
            Assert.IsFalse(firstFunction.IsFromHydroDynamics());

            TestHydroDataStub hydroData2 = CreateHydFileStubWithRelativePathOnFunction(functionName, "lalala.data");

            // 2nd import:
            model.ImportHydroData(hydroData2);

            // the salinity function is now changed to a constant, because there was no data left
            IFunction replacedFunction = FindFunctionInModel(functionName, model);
            Assert.IsNotNull(replacedFunction);
            //https://issuetracker.deltares.nl/browse/DELFT3DFM-1505 Hyd file leads, so it will be overwritten.
            Assert.AreNotEqual(firstFunction, replacedFunction);
            Assert.IsFalse(replacedFunction.IsConst());
            Assert.IsTrue(replacedFunction.IsFromHydroDynamics());
        }

        /// <summary>
        /// This function is used in the tests to create a hyd stub that sets a data path to a specific process function.
        /// This is Salinity, Tau or Temp.
        /// </summary>
        /// <param name="functionName">Salinity, Tau or Temp</param>
        /// <param name="dataFile">The string to specify. lalala.data or maybe just <see cref="string.Empty"/>.</param>
        /// <returns>A new hyd file importer stub</returns>
        private static TestHydroDataStub CreateHydFileStubWithRelativePathOnFunction(string functionName,
                                                                                     string dataFile)
        {
            TestHydroDataStub hydroData;

            switch (functionName)
            {
                case "Salinity":
                    hydroData = new TestHydroDataStub(new HydFileData() {SalinityRelativePath = dataFile});
                    break;
                case "Tau":
                    hydroData = new TestHydroDataStub(new HydFileData() {ShearStressesRelativePath = dataFile});
                    break;
                case "Temp":
                    hydroData = new TestHydroDataStub(new HydFileData() {TemperatureRelativePath = dataFile});
                    break;
                default:
                    throw new InvalidOperationException("The test case is not supported by the model. " + functionName);
            }

            return hydroData;
        }

        #endregion Import Twice

        #region Timers synchronization

        [Test]
        public void GivenWAQModelWhenSetEventedListObservationPointsOnLoadThenCollectionChangedEventShouldStillFire()
        {
            var model = new WaterQualityModel();
            TypeUtils.SetPrivatePropertyValue(model, "ObservationPoints",
                                              new EventedList<WaterQualityObservationPoint>());

            Assert.That(model.ObservationPoints.Count, Is.EqualTo(0));
            Assert.That(model.ObservationVariableOutputs.Count, Is.EqualTo(0));
            model.ObservationPoints.Add(new WaterQualityObservationPoint());
            Assert.That(model.ObservationPoints.Count, Is.EqualTo(1));
            Assert.That(model.ObservationVariableOutputs.Count, Is.EqualTo(1));
        }

        [Test]
        public void GivenWAQModelWhenSetEventedListLoadsOnLoadThenCollectionChangedEventShouldStillFire()
        {
            var model = new WaterQualityModel();
            TypeUtils.SetPrivatePropertyValue(model, "Loads", new EventedList<WaterQualityLoad>());

            Assert.That(model.Loads.Count, Is.EqualTo(0));
            Assert.That(model.Loads.Count, Is.EqualTo(0));
            model.Loads.Add(new WaterQualityLoad());
            Assert.That(model.Loads.Count, Is.EqualTo(1));
        }

        [Test]
        [TestCase(false, false)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        [TestCase(true, true)]
        [TestCase(false, false)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        [TestCase(true, true)]
        public void Given_WAQModel_When_SimulationTimeChanges_OutputTimers_AreSynchronized(bool syncStart,
                                                                                           bool syncStop)
        {
            var model = new WaterQualityModel();
            WaterQualityModelSettings settings = model.ModelSettings;

            CheckTimersAreEqual(model, settings);

            model.StartTime = model.StartTime.AddYears(syncStart ? 1 : 0);
            model.StopTime = model.StopTime.AddYears(syncStop ? 1 : 0);

            CheckTimersAreEqual(model, settings);
        }

        [Test]
        public void Given_WAQModel_When_SimulationTimeStepChanges_OutputTimers_AreNotSynchronized()
        {
            var model = new WaterQualityModel();
            WaterQualityModelSettings settings = model.ModelSettings;

            CheckTimersAreEqual(model, settings);

            model.TimeStep = model.TimeStep.Add(new TimeSpan(1000));

            CheckTimersAreEqual(model, settings);

            Assert.AreNotEqual(model.TimeStep, settings.BalanceTimeStep);
            Assert.AreNotEqual(model.TimeStep, settings.HisStartTime);
            Assert.AreNotEqual(model.TimeStep, settings.MapTimeStep);
        }

        private static void CheckTimersAreEqual(WaterQualityModel model, WaterQualityModelSettings settings)
        {
            Assert.AreEqual(model.StartTime, settings.BalanceStartTime);
            Assert.AreEqual(model.StopTime, settings.BalanceStopTime);

            Assert.AreEqual(model.StartTime, settings.HisStartTime);
            Assert.AreEqual(model.StopTime, settings.HisStopTime);

            Assert.AreEqual(model.StartTime, settings.MapStartTime);
            Assert.AreEqual(model.StopTime, settings.MapStopTime);
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void Given_WAQMode_When_SimulationStartTimeChanges_And_OutputTimerSynchronizes_LogMessageIsGiven(
            bool synchronize)
        {
            var model = new WaterQualityModel();

            DateTime newDate = model.StartTime.AddYears(synchronize ? 1 : 0);
            string expectedLogMessage = string.Format(
                Resources
                    .WaterQualityModel_LogSynchronizedTimer_Output_timers___0___have_been_synchronized_to_match_the_Simulation__0____1___,
                "Start Time",
                newDate);

            Action action = () => model.StartTime = newDate;
            CheckLogMessageIfSync(synchronize, action, expectedLogMessage);
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void Given_WAQMode_When_SimulationStopTimeChanges_And_OutputTimerSynchronizes_LogMessageIsGiven(
            bool synchronize)
        {
            var model = new WaterQualityModel();

            DateTime newDate = model.StopTime.AddYears(synchronize ? 1 : 0);
            string expectedLogMessage = string.Format(
                Resources
                    .WaterQualityModel_LogSynchronizedTimer_Output_timers___0___have_been_synchronized_to_match_the_Simulation__0____1___,
                "Stop Time",
                newDate);

            Action action = () => model.StopTime = newDate;
            CheckLogMessageIfSync(synchronize, action, expectedLogMessage);
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void Given_WAQMode_When_SimulationTimeStepChanges_And_OutputTimerSynchronizes_LogMessageIsNotGiven(
            bool synchronize)
        {
            var model = new WaterQualityModel();
            TimeSpan newDate = model.TimeStep.Add(new TimeSpan(synchronize ? 1000 : 0));

            Action action = () => model.TimeStep = newDate;
            TestHelper.AssertLogMessagesCount(action, 0);
        }

        private static void CheckLogMessageIfSync(bool synchronize, Action action, string expectedLogMessage)
        {
            if (synchronize)
            {
                TestHelper.AssertLogMessageIsGenerated(action, expectedLogMessage, 1);
            }
            else
            {
                TestHelper.AssertLogMessagesCount(action, 0);
            }
        }

        [Test]
        public void Given_WAQModel_When_BalanceTimer_Changes_SimulationTimeDoesNot()
        {
            var model = new WaterQualityModel();
            WaterQualityModelSettings settings = model.ModelSettings;

            Assert.AreEqual(model.StartTime, settings.BalanceStartTime);
            Assert.AreEqual(model.StopTime, settings.BalanceStopTime);
            Assert.AreEqual(model.TimeStep, settings.BalanceTimeStep);

            settings.BalanceStartTime = model.StartTime.AddYears(1);
            settings.BalanceStopTime = model.StopTime.AddYears(1);
            settings.BalanceTimeStep = model.TimeStep.Add(new TimeSpan(0, 5, 0, 0));

            Assert.AreNotEqual(model.StartTime, settings.BalanceStartTime);
            Assert.AreNotEqual(model.StopTime, settings.BalanceStopTime);
            Assert.AreNotEqual(model.TimeStep, settings.BalanceTimeStep);
        }

        [Test]
        public void Given_WAQModel_When_CellsTimer_Changes_SimulationTimeDoesNot()
        {
            var model = new WaterQualityModel();
            WaterQualityModelSettings settings = model.ModelSettings;

            Assert.AreEqual(model.StartTime, settings.MapStartTime);
            Assert.AreEqual(model.StopTime, settings.MapStopTime);
            Assert.AreEqual(model.TimeStep, settings.MapTimeStep);

            settings.MapStartTime = model.StartTime.AddYears(1);
            settings.MapStopTime = model.StopTime.AddYears(1);
            settings.MapTimeStep = model.TimeStep.Add(new TimeSpan(0, 5, 0, 0));

            Assert.AreNotEqual(model.StartTime, settings.MapStartTime);
            Assert.AreNotEqual(model.StopTime, settings.MapStopTime);
            Assert.AreNotEqual(model.TimeStep, settings.MapTimeStep);
        }

        [Test]
        public void Given_WAQModel_When_MonitoringLocationsTimer_Changes_SimulationTimeDoesNot()
        {
            var model = new WaterQualityModel();
            WaterQualityModelSettings settings = model.ModelSettings;

            Assert.AreEqual(model.StartTime, settings.HisStartTime);
            Assert.AreEqual(model.StopTime, settings.HisStopTime);
            Assert.AreEqual(model.TimeStep, settings.HisTimeStep);

            settings.HisStartTime = model.StartTime.AddYears(1);
            settings.HisStopTime = model.StopTime.AddYears(1);
            settings.HisTimeStep = model.TimeStep.Add(new TimeSpan(0, 5, 0, 0));

            Assert.AreNotEqual(model.StartTime, settings.HisStartTime);
            Assert.AreNotEqual(model.StopTime, settings.HisStopTime);
            Assert.AreNotEqual(model.TimeStep, settings.HisTimeStep);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Test_When_HydFile_IsImported_InWaqModel_SimulationTimers_AreUpdated()
        {
            string hydPath = TestHelper.GetTestFilePath(@"WaterQualityDataFiles\ImportHydFile\westernscheldt01.hyd");
            hydPath = TestHelper.CreateLocalCopy(hydPath);
            Assert.IsFalse(string.IsNullOrEmpty(hydPath));
            Assert.IsTrue(File.Exists(hydPath));

            const string expectedStartTime = "2014-01-01 00:00:00";
            const string expectedEndTime = "2014-01-08 00:00:00";

            using (var model = new WaterQualityModel
            {
                StartTime = DateTime.Now,
                StopTime = DateTime.Now.AddDays(3)
            })
            {
                //Check correct initial test state
                Assert.AreNotEqual(expectedStartTime, model.StartTime.ToString("yyyy-MM-dd HH:mm:ss"));
                Assert.AreNotEqual(expectedEndTime, model.StopTime.ToString("yyyy-MM-dd HH:mm:ss"));

                //Import hyd file
                var importer = new HydFileImporter();
                importer.ImportItem(hydPath, model);

                Assert.That(model.StartTime.ToString("yyyy-MM-dd HH:mm:ss"), Is.EqualTo(expectedStartTime));
                Assert.That(model.StopTime.ToString("yyyy-MM-dd HH:mm:ss"), Is.EqualTo(expectedEndTime));
            }
        }
        
        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void Test_When_HydFile_IsImported_OverExistingHydFile_SimulationTimers_AreUpdated_ToNewHydFile()
        {
            string hydPath = TestHelper.GetTestFilePath(@"WaterQualityDataFiles\ImportHydFile\westernscheldt01.hyd");
            hydPath = TestHelper.CreateLocalCopy(hydPath);
            Assert.IsFalse(string.IsNullOrEmpty(hydPath));
            Assert.IsTrue(File.Exists(hydPath));

            var expectedStartTime = "2014-01-01 00:00:00";
            var expectedEndTime = "2014-01-08 00:00:00";

            using (var app = CreateApplication())
            {
                app.Run();
                IProjectService projectService = app.ProjectService;
                Project project = projectService.CreateProject();

                // Initialize Project by saving it.
                string tempDirectory = FileUtils.CreateTempDirectory();
                projectService.SaveProjectAs(Path.Combine(tempDirectory, "WAQ_proj"));

                //Initialize WAQ Model and add it to the project.
                var model = new WaterQualityModel();
                project.RootFolder.Items.Add(model);

                //Change Simulation Timers
                model.StartTime = DateTime.Now;
                model.StopTime = DateTime.Now.AddDays(3);

                //Check the timers are different from the expected
                Assert.AreNotEqual(expectedStartTime, model.StartTime.ToString("yyyy-MM-dd HH:mm:ss"));
                Assert.AreNotEqual(expectedEndTime, model.StopTime.ToString("yyyy-MM-dd HH:mm:ss"));

                //Import hyd file
                var importer = new HydFileImporter();
                importer.ImportItem(hydPath, model);

                Assert.AreEqual(expectedStartTime, model.StartTime.ToString("yyyy-MM-dd HH:mm:ss"));
                Assert.AreEqual(expectedEndTime, model.StopTime.ToString("yyyy-MM-dd HH:mm:ss"));

                //Modify values again, this time to save them into the .dsproj.
                model.StartTime = DateTime.Now;
                model.StopTime = DateTime.Now.AddDays(3);

                //Check the timers are different from the expected
                Assert.AreNotEqual(expectedStartTime, model.StartTime.ToString("yyyy-MM-dd HH:mm:ss"));
                Assert.AreNotEqual(expectedEndTime, model.StopTime.ToString("yyyy-MM-dd HH:mm:ss"));

                //Save project
                projectService.SaveProject();

                //Import hyd file
                importer.ImportItem(hydPath, model);

                //Assert timers of hyd file and of project are equal
                Assert.AreEqual(expectedStartTime, model.StartTime.ToString("yyyy-MM-dd HH:mm:ss"));
                Assert.AreEqual(expectedEndTime, model.StopTime.ToString("yyyy-MM-dd HH:mm:ss"));
            }
        }

        [Test]
        public void Test_When_Importing_HydFile_ChangeTimers_ImportAgainSameHydFile_TimersAreInSync()
        {
            string hydPath = TestHelper.GetTestFilePath(@"WaterQualityDataFiles\ImportHydFile2\westernscheldt01.hyd");
            hydPath = TestHelper.CreateLocalCopy(hydPath);
            Assert.IsFalse(string.IsNullOrEmpty(hydPath));
            Assert.IsTrue(File.Exists(hydPath));

            var expectedStartTime = "2014-01-01 00:00:00";
            var expectedEndTime = "2014-01-08 00:00:00";

            //Initialize WAQ Model and add it to the project.
            using (var model = new WaterQualityModel())
            {
                //Import hyd file
                var importer = new HydFileImporter();
                importer.ImportItem(hydPath, model);

                //Change timers of the model
                var customHydroData = model.HydroData as HydFileData;
                Assert.IsNotNull(customHydroData);
                customHydroData.ConversionStartTime = DateTime.Now.AddDays(1);
                customHydroData.ConversionStopTime = DateTime.Now.AddDays(3);

                Assert.AreEqual(expectedStartTime, model.StartTime.ToString("yyyy-MM-dd HH:mm:ss"));
                Assert.AreEqual(expectedEndTime, model.StopTime.ToString("yyyy-MM-dd HH:mm:ss"));

                //Import the same hyd file
                importer.ImportItem(hydPath, model);

                //Assert timers of hyd file and of project are equal
                Assert.AreEqual(expectedStartTime, model.StartTime.ToString("yyyy-MM-dd HH:mm:ss"));
                Assert.AreEqual(expectedEndTime, model.StopTime.ToString("yyyy-MM-dd HH:mm:ss"));
            }
        }

        [Test]
        public void Test_When_HydFile_IsImported_OverExistingHydFile_CoordinateSystem_IsUpdated_ToNewHydFile()
        {
            ICoordinateSystem epsgAmersfoort = new OgrCoordinateSystemFactory().CreateFromEPSG(28992);

            //Initialize WAQ Model
            using (var model = new WaterQualityModel())
            {
                Assert.AreNotEqual(model.CoordinateSystem, epsgAmersfoort);

                //Import hyd file
                string hydPath = TestHelper.GetTestFilePath(@"WaterQualityDataFiles\ImportHydFileForCoordSystem\DefaultCoordSystem\westernscheldt01.hyd");
                Assert.IsTrue(File.Exists(hydPath));
                var importer = new HydFileImporter();
                importer.ImportItem(hydPath, model);

                //Assert that Coordinate System is now set to Amersfoort/RD
                Assert.AreEqual(model.CoordinateSystem, epsgAmersfoort);

                //Change Coordinate System to something random and assert it is set to something different
                ICoordinateSystem newCoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(25000);
                model.CoordinateSystem = newCoordinateSystem;
                Assert.AreNotEqual(model.CoordinateSystem, epsgAmersfoort);

                //Import hyd file
                importer.ImportItem(hydPath, model);

                //Assert that Coordinate System is now again set to Amersfoort/RD
                Assert.AreEqual(epsgAmersfoort, model.CoordinateSystem);
            }
        }

        [Test]
        public void
            Test_When_HydFile_IsImported_OverExisting_ButDifferent_HydFile_CoordinateSystem_IsUpdated_ToNewHydFile()
        {
            ICoordinateSystem epsgAmersfoort = new OgrCoordinateSystemFactory().CreateFromEPSG(28992);
            ICoordinateSystem epsgZ20Par = new OgrCoordinateSystemFactory().CreateFromEPSG(32210);

            //Initialize WAQ Model
            using (var model = new WaterQualityModel())
            {
                Assert.AreNotEqual(model.CoordinateSystem, epsgAmersfoort);

                //Import hyd file
                string hydPath = TestHelper.GetTestFilePath(@"WaterQualityDataFiles\ImportHydFileForCoordSystem\DefaultCoordSystem\westernscheldt01.hyd");
                Assert.IsTrue(File.Exists(hydPath));
                var importer = new HydFileImporter();
                importer.ImportItem(hydPath, model);

                //Assert that Coordinate System is now set to Amersfoort/RD
                Assert.AreEqual(model.CoordinateSystem, epsgAmersfoort);

                //Import a hyd file with a different coordinate system
                string differentHydPath = TestHelper.GetTestFilePath(@"WaterQualityDataFiles\ImportHydFileForCoordSystem\DifferentCoordSystem\z20_par.hyd");
                importer.ImportItem(differentHydPath, model);

                //Assert that Coordinate System is set to te new coordinate system
                Assert.AreEqual(model.CoordinateSystem, epsgZ20Par);
            }
        }

        [Test]
        public void
            Test_When_HydFile_IsImported_OverExisting_HydFile_ButWithDifferent_CoordinateSystem_InfoMessageIsThrown()
        {
            ICoordinateSystem epsgAmersfoort = new OgrCoordinateSystemFactory().CreateFromEPSG(28992);
            ICoordinateSystem epsgZ20Par = new OgrCoordinateSystemFactory().CreateFromEPSG(32210);

            //Initialize WAQ Model
            using (var model = new WaterQualityModel())
            {
                Assert.AreNotEqual(model.CoordinateSystem, epsgAmersfoort);

                //Import hyd file
                string hydPath = TestHelper.GetTestFilePath(@"WaterQualityDataFiles\ImportHydFileForCoordSystem\DefaultCoordSystem\westernscheldt01.hyd");
                Assert.IsTrue(File.Exists(hydPath));
                var importer = new HydFileImporter();
                importer.ImportItem(hydPath, model);

                //Assert that Coordinate System is now set to Amersfoort/RD
                Assert.AreEqual(model.CoordinateSystem, epsgAmersfoort);

                //Import a hyd file with a different coordinate system
                string differentHydPath = TestHelper.GetTestFilePath(@"WaterQualityDataFiles\ImportHydFileForCoordSystem\DifferentCoordSystem\z20_par.hyd");

                TestHelper.AssertAtLeastOneLogMessagesContains(() => importer.ImportItem(differentHydPath, model),
                                                               string.Format(Resources.WaterQualityModel_ImportHydroData_The_coordinate_system_of_the_model___0__has_been_set_to__1_,
                                                                             model.Name,
                                                                             epsgZ20Par));
            }
        }

        [Test]
        public void
            Test_When_HydFile_IsImported_OverExisting_HydFile_ButWithEmpty_CoordinateSystem_InfoMessageIsThrown()
        {
            ICoordinateSystem epsgAmersfoort = new OgrCoordinateSystemFactory().CreateFromEPSG(28992);

            //Initialize WAQ Model
            using (var model = new WaterQualityModel())
            {
                Assert.AreNotEqual(model.CoordinateSystem, epsgAmersfoort);

                //Import hyd file
                string hydPath = TestHelper.GetTestFilePath(@"WaterQualityDataFiles\ImportHydFileForCoordSystem\ImportHydFileEmptyCS\westernscheldt01.hyd");
                Assert.IsTrue(File.Exists(hydPath));
                var importer = new HydFileImporter();
                importer.ImportItem(hydPath, model);

                //Assert that Coordinate System is now set to Amersfoort/RD
                Assert.AreEqual(model.CoordinateSystem, epsgAmersfoort);

                //Import a hyd file with an empty coordinate system
                string hydPathEmptyCs = TestHelper.GetTestFilePath(@"WaterQualityDataFiles\ImportHydFileForCoordSystem\ImportHydFileEmptyCS\FlowFM.hyd");
                TestHelper.AssertAtLeastOneLogMessagesContains(() => importer.ImportItem(hydPathEmptyCs, model),
                                                               string.Format(Resources.WaterQualityModel_ImportHydroData_The_coordinate_system_of_the_model___0__has_been_set_to__1_,
                                                                             model.Name, "<empty>"));
            }
        }

        [Test]
        public void Test_When_HydFile_IsImported_OverExisting_HydFile_WithSame_CoordinateSystem_NoInfoMessageIsThrown()
        {
            ICoordinateSystem epsgAmersfoort = new OgrCoordinateSystemFactory().CreateFromEPSG(28992);

            //Initialize WAQ Model
            using (var model = new WaterQualityModel())
            {
                Assert.AreNotEqual(model.CoordinateSystem, epsgAmersfoort);

                //Import hyd file
                string hydPath = TestHelper.GetTestFilePath(@"WaterQualityDataFiles\ImportHydFileForCoordSystem\DefaultCoordSystem\westernscheldt01.hyd");
                Assert.IsTrue(File.Exists(hydPath));

                var importer = new HydFileImporter();
                importer.ImportItem(hydPath, model);

                //Assert that Coordinate System is now set to Amersfoort/RD
                Assert.AreEqual(model.CoordinateSystem, epsgAmersfoort);

                //Import a hyd file with the same coordinate system, assert that there is only 1 message thrown which is from the output timers.
                Action call = () => importer.ImportItem(hydPath, model);

                string[] messages = TestHelper.GetAllRenderedMessages(call).ToArray();
                IEnumerable<string> outputTimerMessages = messages.Where(m => m.Contains("Output timers"));
                Assert.That(outputTimerMessages.Count(), Is.EqualTo(1));
            }
        }

        [Test]
        [TestCase("chezy", true)]
        [TestCase("velocity", true)]
        [TestCase("width", true)]
        [TestCase("surf", true)]
        [TestCase("salinity", false)]
        public void Test_IsSegmentFunction_ReturnsExpected(string functionName, bool expected)
        {
            using (var model = new WaterQualityModel())
            {
                //Import hyd file
                string hydPath = TestHelper.GetTestFilePath(@"ValidWaqModels\Flow1D\sobek.hyd");
                Assert.IsTrue(File.Exists(hydPath));
                var importer = new HydFileImporter();
                importer.ImportItem(hydPath, model);

                Assert.IsNotNull(model.HydroData);
                Assert.AreEqual(expected, model.HasDataInHydroDynamics(functionName));
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void Import_Waq_Model_WithSegmentFiles_Creates_FunctionFromHydroDynamics()
        {
            string testFilePath = TestHelper.GetTestFilePath(@"ValidWaqModels\Flow1D\sobek.hyd");
            Assert.IsTrue(File.Exists(testFilePath));

            //Import the second model on top of waqmodel.
            var importer = new HydFileImporter();
            using (var waqModel = importer.ImportItem(testFilePath) as WaterQualityModel)
            {
                Assert.IsNotNull(waqModel);

                //Check filepaths
                Assert.IsFalse(string.IsNullOrEmpty(waqModel.SurfacesRelativeFilePath));
                Assert.IsFalse(string.IsNullOrEmpty(waqModel.VelocitiesFilePath));
                Assert.IsFalse(string.IsNullOrEmpty(waqModel.WidthsFilePath));
                Assert.IsFalse(string.IsNullOrEmpty(waqModel.ChezyCoefficientsFilePath));

                //Import the substances now.
                string subsFilePath = TestHelper.GetTestFilePath(@"ValidWaqModels\\02b_Oxygen_bod_sediment.sub");
                subsFilePath = TestHelper.CreateLocalCopy(subsFilePath);

                Assert.IsNotNull(waqModel.SubstanceProcessLibrary);
                new SubFileImporter().Import(waqModel.SubstanceProcessLibrary, subsFilePath);

                //Check the process has been imported as a segmnent file function
                IFunction chezyProcess = waqModel.ProcessCoefficients.FirstOrDefault(pc => pc.Name.ToLower().Equals("chezy"));
                Assert.IsNotNull(chezyProcess);

                Assert.IsTrue(chezyProcess is FunctionFromHydroDynamics);
            }
        }

        #endregion
        
        private static IApplication CreateApplication()
        {
            var pluginsToAdd = new List<IPlugin>
            {
                new WaterQualityModelApplicationPlugin(),
                new CommonToolsApplicationPlugin(),
                new NHibernateDaoApplicationPlugin(),
                new NetworkEditorApplicationPlugin(),
                new SharpMapGisApplicationPlugin(),
                new NetCdfApplicationPlugin(),
                new ScriptingApplicationPlugin(),
                new ToolboxApplicationPlugin(),

            };
            return new DeltaShellApplicationBuilder().WithPlugins(pluginsToAdd).Build();
        }
    }
}