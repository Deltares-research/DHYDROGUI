using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Editing;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.Model;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.SubstanceProcessLibrary;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Extensions;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Extentions;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.FunctionListView;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.ObservationAreas;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Utils;
using DeltaShell.Plugins.SharpMapGis.SpatialOperations;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using SharpMap.SpatialOperations;
using SharpMapTestUtils;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests
{
    [TestFixture]
    public class WaterQualityModelSyncTest
    {
        [Test]
        [Category(TestCategory.Integration)]
        public void TestModelSyncAfterImportOfSubstanceProcessLibrary()
        {
            var waterQualityModel = new WaterQualityModel();
            // Perform import on empty substance process library
            string commonFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), "IO");
            new SubFileImporter().Import(waterQualityModel.SubstanceProcessLibrary, TestHelper.GetTestFilePath(@"ValidWaqModels\\Eutrof_simple_sobek.sub"));

            // Initial conditions and process coefficients should be created
            Assert.AreEqual(12, waterQualityModel.InitialConditions.Count);
            Assert.AreEqual(58, waterQualityModel.ProcessCoefficients.Count);
            Assert.AreEqual(23, waterQualityModel.GetOutputCoverages().Count());

            IFunction firstInitialCondition = waterQualityModel.InitialConditions.First();
            IFunction firstProcessCoefficient = waterQualityModel.ProcessCoefficients.First();

            // Perform import on non empty substance process library: afterwards the first initial condition and the first first process coefficient should still exist
            new SubFileImporter().Import(waterQualityModel.SubstanceProcessLibrary, Path.Combine(commonFilePath, "Eutrof_simple_custom1.sub"));

            // Initial conditions and process coefficients should be created
            Assert.AreEqual(3, waterQualityModel.InitialConditions.Count);
            Assert.AreEqual(3, waterQualityModel.ProcessCoefficients.Count);
            Assert.AreEqual(6, waterQualityModel.GetOutputCoverages().Count());

            Assert.AreSame(firstInitialCondition, waterQualityModel.InitialConditions.ElementAt(0));
            Assert.AreSame(firstProcessCoefficient, waterQualityModel.ProcessCoefficients.ElementAt(0));
            Assert.AreSame(firstInitialCondition, waterQualityModel.InitialConditions.ElementAt(0));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void TestDatFromHydroDataSyncedOnImportingNewSubstanceProcessLibrary()
        {
            // setup
            using (var waterQualityModel = new WaterQualityModel())
            {
                LoadRealModelWithGridFromFile(waterQualityModel);

                const string functionName = "Salinity";
                Assert.IsTrue(waterQualityModel.HasDataInHydroDynamics(functionName), "Precondition: hyd-file should have file to salinity data.");

                // Perform import on empty substance process library
                string commonFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), "IO");

                // call (import a library with "Salinity"
                new SubFileImporter().Import(waterQualityModel.SubstanceProcessLibrary, Path.Combine(commonFilePath, "03d_Tewor2003.sub"));

                // assert
                IFunction salinityFunction = waterQualityModel.ProcessCoefficients.First(pc => pc.Name == functionName);
                Assert.IsTrue(salinityFunction.IsFromHydroDynamics());
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void TestDatFromHydroDataSyncedOnImportingNewSubstanceProcessLibraryThatNoLongerHasSalinity()
        {
            // setup
            using (var waterQualityModel = new WaterQualityModel())
            {
                LoadRealModelWithGridFromFile(waterQualityModel);

                const string functionName = "Salinity";
                Assert.IsTrue(waterQualityModel.HasDataInHydroDynamics(functionName), "Precondition: hyd-file should have file to salinity data.");

                // Perform import on empty substance process library
                string commonFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), "IO");

                new SubFileImporter().Import(waterQualityModel.SubstanceProcessLibrary, Path.Combine(commonFilePath, "03d_Tewor2003.sub"));

                // assert
                IFunction salinityFunction = waterQualityModel.ProcessCoefficients.First(pc => pc.Name == functionName);
                Assert.IsTrue(salinityFunction.IsFromHydroDynamics());

                // call (import a library with "Salinity"
                new SubFileImporter().Import(waterQualityModel.SubstanceProcessLibrary, Path.Combine(commonFilePath, "Eutrof_simple_custom1.sub"));

                // assert
                salinityFunction = waterQualityModel.ProcessCoefficients.FirstOrDefault(pc => pc.Name == functionName);
                Assert.IsNull(salinityFunction);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void TestDataFromHydroDataSyncedOnImportingNewHydFile()
        {
            // setup
            using (var waterQualityModel = new WaterQualityModel())
            {
                // Perform import on empty substance process library
                string commonFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), "IO");

                new SubFileImporter().Import(waterQualityModel.SubstanceProcessLibrary, Path.Combine(commonFilePath, "03d_Tewor2003.sub"));

                const string functionName = "Salinity";
                Assert.IsFalse(waterQualityModel.HasDataInHydroDynamics(functionName), "Precondition: hyd-file should NOT have file to salinity data.");
                IFunction salinityFunction = waterQualityModel.ProcessCoefficients.First(pc => pc.Name == functionName);
                Assert.IsTrue(salinityFunction.IsConst());

                // call
                LoadRealModelWithGridFromFile(waterQualityModel);

                // assert
                Assert.IsTrue(waterQualityModel.HasDataInHydroDynamics(functionName));

                salinityFunction = waterQualityModel.ProcessCoefficients.First(pc => pc.Name == functionName);
                Assert.IsTrue(salinityFunction.IsFromHydroDynamics());
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void TestDataFromHydroDataSyncedOnImportingNewHydFileThatNoLongerHadSalinityAvailable()
        {
            // setup
            using (var waterQualityModel = new WaterQualityModel())
            {
                string commonFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), "IO");

                new SubFileImporter().Import(waterQualityModel.SubstanceProcessLibrary, Path.Combine(commonFilePath, "03d_Tewor2003.sub"));

                LoadRealModelWithGridFromFile(waterQualityModel);

                const string functionName = "Salinity";
                Assert.IsTrue(waterQualityModel.HasDataInHydroDynamics(functionName), "Precondition: Salinity data should be available in hyd file.");

                IFunction salinityFunction = waterQualityModel.ProcessCoefficients.First(pc => pc.Name == functionName);
                Assert.IsTrue(salinityFunction.IsFromHydroDynamics(), "Precondition: Salinity process coefficient should be linked to hyd-file data.");

                // call (new hyd-file being imported)
                LoadSquareModelWithGridFromFile(waterQualityModel);

                // assert
                Assert.IsFalse(waterQualityModel.HasDataInHydroDynamics(functionName));

                salinityFunction = waterQualityModel.ProcessCoefficients.First(pc => pc.Name == functionName);
                Assert.IsTrue(salinityFunction.IsConst(), "Importing hyd-file that has no longer data available should set the function back to constant.");
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void TestAddInitialConditionCoverage_FunctionHasSpatialOperationWithDefaultValue()
        {
            // setup
            using (var waterQualityModel = new WaterQualityModel())
            {
                LoadRealModelWithGridFromFile(waterQualityModel);

                string commonFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), "IO");

                new SubFileImporter().Import(waterQualityModel.SubstanceProcessLibrary, Path.Combine(commonFilePath, "03d_Tewor2003.sub"));

                Assert.AreEqual(5, waterQualityModel.InitialConditions.Count);

                const double constantDefaultValue = 9d;
                waterQualityModel.InitialConditions[0].Components[0].DefaultValue = constantDefaultValue;

                IDataItem dataItem = waterQualityModel.AllDataItems.FirstOrDefault(di => Equals(di.Value, waterQualityModel.InitialConditions[0]));

                Assert.IsNotNull(dataItem);
                Assert.IsNull(dataItem.ValueConverter);

                // Send events to the sync. A constant function is replaced with a coverage
                IFunction newCoverage = FunctionTypeCreator.ReplaceFunctionUsingCreator(
                    waterQualityModel.InitialConditions,
                    waterQualityModel.InitialConditions[0],
                    FunctionTypeCreatorFactory.CreateUnstructuredGridCoverageCreator(),
                    waterQualityModel);

                Assert.IsTrue(newCoverage.IsUnstructuredGridCellCoverage());
                Assert.AreSame(newCoverage, waterQualityModel.InitialConditions[0]);
                // the new coverage contains all default values over all cells. This is due to the fact that it is regulated via a value converter (spatial operations).
                CollectionAssert.AreEqual(Enumerable.Repeat(constantDefaultValue, newCoverage.Components[0].Values.Count).ToList(), newCoverage.GetValues<double>().ToList());

                dataItem = waterQualityModel.AllDataItems.FirstOrDefault(di => Equals(di.Value, waterQualityModel.InitialConditions[0]));

                Assert.IsNotNull(dataItem);
                Assert.IsNotNull(dataItem.ValueConverter);
                Assert.IsInstanceOf<SpatialOperationSetValueConverter>(dataItem.ValueConverter);

                var vc = (SpatialOperationSetValueConverter) dataItem.ValueConverter;

                Assert.AreEqual(1, vc.SpatialOperationSet.Operations.Count);
                Assert.IsInstanceOf<SetValueOperation>(vc.SpatialOperationSet.Operations[0]);

                // the original value may only contain no data value
                CollectionAssert.AreEqual(Enumerable.Repeat((double) newCoverage.Components[0].NoDataValue,
                                                            newCoverage.Components[0].Values.Count).ToList(),
                                          ((UnstructuredGridCoverage) vc.OriginalValue).GetValues<double>().ToList());
                // the final value contains the default value
                CollectionAssert.AreEqual(Enumerable.Repeat(constantDefaultValue, newCoverage.Components[0].Values.Count).ToList(),
                                          ((UnstructuredGridCoverage) vc.ConvertedValue).GetValues<double>().ToList());
                Assert.AreEqual(dataItem.Value, newCoverage);

                var operation = (SetValueOperation) vc.SpatialOperationSet.Operations[0];
                Assert.AreEqual(constantDefaultValue, operation.Value);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void TestModelOutputSyncAfterAddingAndRemovingSubstanceForSubstanceCalculation()
        {
            var waterQualityModel = new WaterQualityModel();

            // Add a substance
            const string substanceName = "Substance";
            waterQualityModel.SubstanceProcessLibrary.Substances.Add(new WaterQualitySubstance {Name = substanceName});

            UnstructuredGridCellCoverage[] outputCoverages = waterQualityModel.GetOutputCoverages().ToArray();
            Assert.AreEqual(1, outputCoverages.Length);
            Assert.AreEqual(substanceName, outputCoverages[0].Name);

            // Clear all substances
            waterQualityModel.SubstanceProcessLibrary.Substances.Clear();

            outputCoverages = waterQualityModel.GetOutputCoverages().ToArray();
            Assert.IsEmpty(outputCoverages);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void TestInitialConditionsConvertsDefaultCoverage()
        {
            using (var model = new WaterQualityModel())
            {
                LoadSquareModelWithGridFromFile(model);

                // add a substance. Initial condition functions will be created.
                double five = 5;
                model.SubstanceProcessLibrary.Substances.Add(new WaterQualitySubstance
                {
                    Name = "O2",
                    Active = true,
                    InitialValue = five
                });

                Assert.AreEqual(1, model.InitialConditions.Count);
                Assert.AreEqual(five, model.InitialConditions[0].Components[0].DefaultValue);
                Assert.IsTrue(model.InitialConditions[0].IsConst());

                FunctionWrapper wrapper = CreateFunctionWrapper(model, model.InitialConditions);

                // change the type, a lot will happen. Sync will be called
                wrapper.FunctionType = "Coverage";

                AssertConstantChangedToCoverage(model.InitialConditions, five);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void TestDispersionConvertsDefaultCoverage()
        {
            using (var model = new WaterQualityModel())
            {
                LoadSquareModelWithGridFromFile(model);

                Assert.AreEqual(1, model.Dispersion.Count);
                double five = 5;
                model.Dispersion[0].Components[0].DefaultValue = five;

                Assert.IsTrue(model.Dispersion[0].IsConst());

                FunctionWrapper wrapper = CreateFunctionWrapper(model, model.Dispersion);

                // change the type, a lot will happen. Sync will be called
                wrapper.FunctionType = "Coverage";

                AssertConstantChangedToCoverage(model.Dispersion, five);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void TestUnstructuredGridCoveragesGetModelGridInjectedAfterBeingAdded()
        {
            var modelGrid = new UnstructuredGrid();
            var waterQualityModel = new WaterQualityModel();

            SetNewGrid(waterQualityModel, modelGrid);

            var newGrid = new UnstructuredGrid();
            var initialConditionCoverage = new UnstructuredGridCellCoverage(newGrid, false);
            var processCoefficientCoverage = new UnstructuredGridCellCoverage(newGrid, false);
            var dispersionCoverage = new UnstructuredGridCellCoverage(newGrid, false);

            Assert.AreEqual(newGrid, initialConditionCoverage.Grid);
            Assert.AreEqual(newGrid, processCoefficientCoverage.Grid);
            Assert.AreEqual(newGrid, dispersionCoverage.Grid);

            waterQualityModel.InitialConditions.Add(initialConditionCoverage);
            waterQualityModel.ProcessCoefficients.Add(processCoefficientCoverage);
            waterQualityModel.Dispersion.Add(dispersionCoverage);

            Assert.AreEqual(modelGrid, initialConditionCoverage.Grid);
            Assert.AreEqual(modelGrid, processCoefficientCoverage.Grid);
            Assert.AreEqual(modelGrid, dispersionCoverage.Grid);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void TestModelUnstructuredGridCoveragesGetModelGridInjectedAfterSettingGrid()
        {
            UnstructuredGrid modelGrid = UnstructuredGridTestHelper.GenerateRegularGrid(2, 2, 10, 10);
            var waterQualityModel = new WaterQualityModel();

            SetNewGrid(waterQualityModel, modelGrid);

            UnstructuredGrid newGrid = UnstructuredGridTestHelper.GenerateRegularGrid(5, 5, 10, 10);

            UnstructuredGridCellCoverage initialConditionCoverage = CreateUnstructuredGridCellCoverage(newGrid, 1.1);
            UnstructuredGridCellCoverage processCoefficientCoverage = CreateUnstructuredGridCellCoverage(newGrid, 2.2);
            UnstructuredGridCellCoverage dispersionCoverage = CreateUnstructuredGridCellCoverage(newGrid, 3.3);

            Assert.AreEqual(newGrid, initialConditionCoverage.Grid);
            Assert.AreEqual(newGrid, processCoefficientCoverage.Grid);
            Assert.AreEqual(newGrid, dispersionCoverage.Grid);

            waterQualityModel.InitialConditions.Add(initialConditionCoverage);
            waterQualityModel.ProcessCoefficients.Add(processCoefficientCoverage);
            waterQualityModel.Dispersion.Add(dispersionCoverage);

            Assert.AreEqual(modelGrid, initialConditionCoverage.Grid);
            Assert.AreEqual(modelGrid, processCoefficientCoverage.Grid);
            Assert.AreEqual(modelGrid, dispersionCoverage.Grid);

            SetNewGrid(waterQualityModel, newGrid);

            Assert.AreEqual(newGrid, initialConditionCoverage.Grid);
            Assert.AreEqual(newGrid, processCoefficientCoverage.Grid);
            Assert.AreEqual(newGrid, dispersionCoverage.Grid);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void SettingsNewGrid_ObservationAreaCoverage_UpdateTheCoveragesGrid()
        {
            // setup
            UnstructuredGrid grid = UnstructuredGridTestHelper.GenerateRegularGrid(5, 5, 1, 1);
            var model = new WaterQualityModel();

            // call
            SetNewGrid(model, grid);

            // assert
            CollectionAssert.AreEqual(Enumerable.Range(0, 25).ToArray(), model.ObservationAreas.Arguments[0].GetValues<int>().ToArray());
        }

        private static void SetNewGrid(WaterQualityModel waterQualityModel, UnstructuredGrid modelGrid)
        {
            TypeUtils.CallPrivateMethod(waterQualityModel, "SetNewGrid", modelGrid, false);
        }

        private static UnstructuredGridCellCoverage CreateUnstructuredGridCellCoverage(UnstructuredGrid newGrid, double defaultValue)
        {
            var initialConditionCoverage = new UnstructuredGridCellCoverage(newGrid, false);
            initialConditionCoverage.Components[0].DefaultValue = defaultValue;
            initialConditionCoverage.Components[0].NoDataValue = -990.0;
            return initialConditionCoverage;
        }

        /// <summary>
        /// Load a square grid model from file.
        /// The grid is a square of 25x25 cells.
        /// </summary>
        private static void LoadSquareModelWithGridFromFile(WaterQualityModel waterQualityModel)
        {
            new HydFileImporter().ImportItem(TestHelper.GetTestFilePath(@"IO\square\square.hyd"), waterQualityModel);
        }

        /// <summary>
        /// Load a real San Francisco model model from file.
        /// </summary>
        private static void LoadRealModelWithGridFromFile(WaterQualityModel model)
        {
            new HydFileImporter().ImportItem(TestHelper.GetTestFilePath(@"IO\real\uni3d.hyd"), model);
        }

        /// <summary>
        /// Creates a function wrapper with a constant as default.
        /// </summary>
        private static FunctionWrapper CreateFunctionWrapper(WaterQualityModel dataOwner, IEventedList<IFunction> functionList)
        {
            var wrapper = new FunctionWrapper(functionList[0], functionList, dataOwner,
                                              new[]
                                              {
                                                  FunctionTypeCreatorFactory.CreateConstantCreator(),
                                                  FunctionTypeCreatorFactory.CreateUnstructuredGridCoverageCreator()
                                              });

            Assert.AreEqual("Constant", wrapper.FunctionType);

            return wrapper;
        }

        private static void AssertConstantChangedToCoverage(IList<IFunction> functionList, double expectedDefaultValue)
        {
            Assert.AreEqual(1, functionList.Count);
            Assert.AreEqual(expectedDefaultValue, functionList[0].Components[0].DefaultValue);
            Assert.AreEqual(-999d, functionList[0].Components[0].NoDataValue);
            Assert.IsFalse(functionList[0].IsConst());

            // it is expected that the default value is set on all values after clearing the coverage.
            double[] allDefaultValue = Enumerable.Repeat(expectedDefaultValue, functionList[0].Components[0].Values.Count).ToArray();
            CollectionAssert.AreEqual(allDefaultValue, functionList[0].Components[0].GetValues<double>().ToArray());
        }

        # region Monitoring output data item syncing

        [Test]
        public void TestModelMonitoringOutputSyncAfterChangeOfMonitoringOutputLevelForSubstanceCalculation()
        {
            var waterQualityModel = new WaterQualityModel {ModelSettings = {MonitoringOutputLevel = MonitoringOutputLevel.None}};

            // Add an observation point
            var observationPoint = new WaterQualityObservationPoint
            {
                Name = "O1",
                Geometry = new Point(new Coordinate(0, 5))
            };

            waterQualityModel.ObservationPoints.Add(observationPoint);

            // Add an observation area
            waterQualityModel.ObservationAreas.AddLabel("abc");

            // Add a substance to the substance process library
            waterQualityModel.SubstanceProcessLibrary.Substances.Add(new WaterQualitySubstance
            {
                Name = "Substance",
                ConcentrationUnit = ""
            });

            // Add a dummy text output data item
            waterQualityModel.DataItems.Add(new DataItem(new TextDocument {Name = "Run report"}) {Role = DataItemRole.Output});

            Assert.IsNull(waterQualityModel.MonitoringOutputDataItemSet); // The monitoring output data item set should not be present

            Assert.AreEqual("Substance", waterQualityModel.OutputSubstancesDataItemSet.DataItems.ElementAt(0).Name);
            Assert.AreEqual("Output parameters", waterQualityModel.DataItems.Where(di => di.Role.HasFlag(DataItemRole.Output)).ElementAt(1).Name);
            Assert.AreEqual("Run report", waterQualityModel.DataItems.Where(di => di.Role.HasFlag(DataItemRole.Output)).ElementAt(2).Name);

            // Set the monitoring output level to "Points"
            waterQualityModel.ModelSettings.MonitoringOutputLevel = MonitoringOutputLevel.Points;
            Assert.IsNotNull(waterQualityModel.MonitoringOutputDataItemSet); // The monitoring output data item set should be present and should contain monitoring point specific items
            Assert.AreEqual(1, waterQualityModel.ObservationVariableOutputs.Count);
            Assert.AreEqual("O1", waterQualityModel.ObservationVariableOutputs[0].Name);
            Assert.AreEqual(1, waterQualityModel.ObservationVariableOutputs[0].TimeSeriesList.Count());
            Assert.AreEqual("Substance", waterQualityModel.ObservationVariableOutputs[0].TimeSeriesList.ElementAt(0).Name);

            IEnumerable<IDataItem> outputDataItems = waterQualityModel.DataItems.Where(di => di.Role.HasFlag(DataItemRole.Output) && !di.Hidden);
            Assert.AreEqual(4, outputDataItems.Count());
            Assert.AreEqual("Substances", outputDataItems.ElementAt(0).Name);
            IEnumerable<IDataItem> substanceDataItems = waterQualityModel.OutputSubstancesDataItemSet.DataItems.Where(di => di.Role.HasFlag(DataItemRole.Output) && !di.Hidden);
            Assert.AreEqual(1, substanceDataItems.Count());
            Assert.AreEqual("Substance", substanceDataItems.ElementAt(0).Name);

            Assert.AreEqual("Monitoring locations", outputDataItems.ElementAt(1).Name);
            Assert.AreEqual("Output parameters", outputDataItems.ElementAt(2).Name);
            Assert.AreEqual("Run report", outputDataItems.ElementAt(3).Name);

            // Set the monitoring output level to "Areas"
            waterQualityModel.ModelSettings.MonitoringOutputLevel = MonitoringOutputLevel.Areas;
            Assert.IsNotNull(waterQualityModel.MonitoringOutputDataItemSet); // The monitoring output data item set should be present and should contain monitoring area specific items

            Assert.AreEqual(1, waterQualityModel.ObservationVariableOutputs.Count);
            Assert.AreEqual("abc", waterQualityModel.ObservationVariableOutputs[0].Name);
            Assert.AreEqual(1, waterQualityModel.ObservationVariableOutputs[0].TimeSeriesList.Count());
            Assert.AreEqual("Substance", waterQualityModel.ObservationVariableOutputs[0].TimeSeriesList.ElementAt(0).Name);

            outputDataItems = waterQualityModel.DataItems.Where(di => di.Role.HasFlag(DataItemRole.Output) && !di.Hidden);
            Assert.AreEqual(4, outputDataItems.Count());
            Assert.AreEqual("Substances", outputDataItems.ElementAt(0).Name);
            substanceDataItems = waterQualityModel.OutputSubstancesDataItemSet.DataItems.Where(di => di.Role.HasFlag(DataItemRole.Output) && !di.Hidden);
            Assert.AreEqual(1, substanceDataItems.Count());
            Assert.AreEqual("Substance", substanceDataItems.ElementAt(0).Name);

            Assert.AreEqual("Monitoring locations", outputDataItems.ElementAt(1).Name);
            Assert.AreEqual("Output parameters", outputDataItems.ElementAt(2).Name);
            Assert.AreEqual("Run report", outputDataItems.ElementAt(3).Name);

            // Set the monitoring output level to "Points and areas"
            waterQualityModel.ModelSettings.MonitoringOutputLevel = MonitoringOutputLevel.PointsAndAreas;
            Assert.IsNotNull(waterQualityModel.MonitoringOutputDataItemSet); // The monitoring output data item set should be present and should contain both monitoring area and monitoring point items
            Assert.AreEqual(2, waterQualityModel.ObservationVariableOutputs.Count);
            Assert.AreEqual("O1", waterQualityModel.ObservationVariableOutputs[0].Name);
            Assert.AreEqual("abc", waterQualityModel.ObservationVariableOutputs[1].Name);
            Assert.AreEqual(1, waterQualityModel.ObservationVariableOutputs[0].TimeSeriesList.Count());
            Assert.AreEqual("Substance", waterQualityModel.ObservationVariableOutputs[0].TimeSeriesList.ElementAt(0).Name);

            outputDataItems = waterQualityModel.DataItems.Where(di => di.Role.HasFlag(DataItemRole.Output) && !di.Hidden);
            Assert.AreEqual(4, outputDataItems.Count());
            Assert.AreEqual("Substances", outputDataItems.ElementAt(0).Name);
            substanceDataItems = waterQualityModel.OutputSubstancesDataItemSet.DataItems.Where(di => di.Role.HasFlag(DataItemRole.Output) && !di.Hidden);
            Assert.AreEqual(1, substanceDataItems.Count());
            Assert.AreEqual("Substance", substanceDataItems.ElementAt(0).Name);

            Assert.AreEqual("Monitoring locations", outputDataItems.ElementAt(1).Name);
            Assert.AreEqual("Output parameters", outputDataItems.ElementAt(2).Name);
            Assert.AreEqual("Run report", outputDataItems.ElementAt(3).Name);

            // Set the monitoring output level to "None" again
            waterQualityModel.ModelSettings.MonitoringOutputLevel = MonitoringOutputLevel.None;
            Assert.IsNull(waterQualityModel.MonitoringOutputDataItemSet); // The monitoring output data item set should not be present

            outputDataItems = waterQualityModel.DataItems.Where(di => di.Role.HasFlag(DataItemRole.Output) && !di.Hidden);
            Assert.AreEqual(3, outputDataItems.Count());
            Assert.AreEqual("Substances", outputDataItems.ElementAt(0).Name);
            substanceDataItems = waterQualityModel.OutputSubstancesDataItemSet.DataItems.Where(di => di.Role.HasFlag(DataItemRole.Output) && !di.Hidden);
            Assert.AreEqual(1, substanceDataItems.Count());
            Assert.AreEqual("Substance", substanceDataItems.ElementAt(0).Name);

            Assert.AreEqual("Output parameters", outputDataItems.ElementAt(1).Name);
            Assert.AreEqual("Run report", outputDataItems.ElementAt(2).Name);
        }

        [Test]
        public void TestModelMonitoringOutputSyncAfterAddingAndRemovingSubstanceForSubstanceCalculation()
        {
            var waterQualityModel = new WaterQualityModel {ModelSettings = {MonitoringOutputLevel = MonitoringOutputLevel.Points}};

            var waterQualitySubstance1 = new WaterQualitySubstance {Name = "Substance 1"};
            var waterQualitySubstance2 = new WaterQualitySubstance {Name = "Substance 2"};

            // In advance, add an output parameter that should be shown in his
            waterQualityModel.SubstanceProcessLibrary.OutputParameters.Add(new WaterQualityOutputParameter
            {
                Name = "Output parameter",
                ShowInHis = true
            });

            // Manually add two observation variable outputs with a dummy output parameter time series
            waterQualityModel.ObservationVariableOutputs.Add(new WaterQualityObservationVariableOutput(new[]
            {
                new Tuple<string, string>("Output parameter", "")
            }) {Name = "O1"});
            waterQualityModel.ObservationVariableOutputs.Add(new WaterQualityObservationVariableOutput(new[]
            {
                new Tuple<string, string>("Output parameter", "")
            }) {Name = "O2"});
            Assert.IsTrue(waterQualityModel.ObservationVariableOutputs.All(ovo => ovo.TimeSeriesList.Count() == 1));

            // Add a substance
            waterQualityModel.SubstanceProcessLibrary.Substances.Add(waterQualitySubstance1);

            Assert.IsTrue(waterQualityModel.ObservationVariableOutputs.All(ovo => ovo.TimeSeriesList.Count() == 2)); // A time series should be added to all observation variable outputs
            Assert.AreEqual("Substance 1", waterQualityModel.ObservationVariableOutputs[0].TimeSeriesList.ElementAt(0).Name);
            Assert.AreEqual("Output parameter", waterQualityModel.ObservationVariableOutputs[0].TimeSeriesList.ElementAt(1).Name);

            // Add another substance
            waterQualityModel.SubstanceProcessLibrary.Substances.Add(waterQualitySubstance2);

            Assert.IsTrue(waterQualityModel.ObservationVariableOutputs.All(ovo => ovo.TimeSeriesList.Count() == 3)); // A time series should be added to all observation variable outputs
            Assert.AreEqual("Substance 1", waterQualityModel.ObservationVariableOutputs[0].TimeSeriesList.ElementAt(0).Name);
            Assert.AreEqual("Substance 2", waterQualityModel.ObservationVariableOutputs[0].TimeSeriesList.ElementAt(1).Name);
            Assert.AreEqual("Output parameter", waterQualityModel.ObservationVariableOutputs[0].TimeSeriesList.ElementAt(2).Name);

            // Remove the second substance
            waterQualityModel.SubstanceProcessLibrary.Substances.Remove(waterQualitySubstance2);

            Assert.IsTrue(waterQualityModel.ObservationVariableOutputs.All(ovo => ovo.TimeSeriesList.Count() == 2)); // The time series should be removed from all observation variable outputs
            Assert.AreEqual("Substance 1", waterQualityModel.ObservationVariableOutputs[0].TimeSeriesList.ElementAt(0).Name);
            Assert.AreEqual("Output parameter", waterQualityModel.ObservationVariableOutputs[0].TimeSeriesList.ElementAt(1).Name);
        }

        [Test]
        public void GivenModelThenAddASubstanceThenSubstanceIsPlacedInOutputSubstancesDataItemSet()
        {
            var waterQualityModel = new WaterQualityModel();

            var waterQualitySubstance1 = new WaterQualitySubstance {Name = "Substance 1"};

            // Add a substance
            waterQualityModel.SubstanceProcessLibrary.Substances.Add(waterQualitySubstance1);

            Assert.AreEqual("Substance 1", waterQualityModel.OutputSubstancesDataItemSet.AsEventedList<UnstructuredGridCellCoverage>()[0].Name);
        }

        [Test]
        public void TestModelMonitoringOutputSyncAfterAddingAndRemovingOutputParameterForSubstanceCalculation()
        {
            var waterQualityModel = new WaterQualityModel {ModelSettings = {MonitoringOutputLevel = MonitoringOutputLevel.Points}};

            var waterQualityOutputParameter1 = new WaterQualityOutputParameter
            {
                Name = "Output parameter 1",
                ShowInHis = false
            };
            var waterQualityOutputParameter2 = new WaterQualityOutputParameter
            {
                Name = "Output parameter 2",
                ShowInHis = true
            };
            var waterQualityOutputParameter3 = new WaterQualityOutputParameter
            {
                Name = "Output parameter 3",
                ShowInHis = true
            };

            // In advance, add a substance
            waterQualityModel.SubstanceProcessLibrary.Substances.Add(new WaterQualitySubstance {Name = "Substance"});

            // Manually add two observation variable outputs with a substance time series
            waterQualityModel.ObservationVariableOutputs.Add(new WaterQualityObservationVariableOutput(new[]
            {
                new Tuple<string, string>("Substance", "")
            }) {Name = "O1"});
            waterQualityModel.ObservationVariableOutputs.Add(new WaterQualityObservationVariableOutput(new[]
            {
                new Tuple<string, string>("Substance", "")
            }) {Name = "O2"});
            Assert.IsTrue(waterQualityModel.ObservationVariableOutputs.All(ovo => ovo.TimeSeriesList.Count() == 1));

            // Add an output parameter that should not be shown in his
            waterQualityModel.SubstanceProcessLibrary.OutputParameters.Add(waterQualityOutputParameter1);

            Assert.IsTrue(waterQualityModel.ObservationVariableOutputs.All(ovo => ovo.TimeSeriesList.Count() == 1)); // No time series should be added to the observation variable outputs => output parameter should not be shown in his
            Assert.AreEqual("Substance", waterQualityModel.ObservationVariableOutputs[0].TimeSeriesList.ElementAt(0).Name);

            // Add an output parameter that should be shown in his
            waterQualityModel.SubstanceProcessLibrary.OutputParameters.Add(waterQualityOutputParameter2);

            Assert.IsTrue(waterQualityModel.ObservationVariableOutputs.All(ovo => ovo.TimeSeriesList.Count() == 2)); // A time series should be added to all observation variable outputs
            Assert.AreEqual("Substance", waterQualityModel.ObservationVariableOutputs[0].TimeSeriesList.ElementAt(0).Name);
            Assert.AreEqual("Output parameter 2", waterQualityModel.ObservationVariableOutputs[0].TimeSeriesList.ElementAt(1).Name);

            // Add another output parameter that should be shown in his
            waterQualityModel.SubstanceProcessLibrary.OutputParameters.Add(waterQualityOutputParameter3);

            Assert.IsTrue(waterQualityModel.ObservationVariableOutputs.All(ovo => ovo.TimeSeriesList.Count() == 3)); // A time series should be added to all observation variable outputs
            Assert.AreEqual("Substance", waterQualityModel.ObservationVariableOutputs[0].TimeSeriesList.ElementAt(0).Name);
            Assert.AreEqual("Output parameter 2", waterQualityModel.ObservationVariableOutputs[0].TimeSeriesList.ElementAt(1).Name);
            Assert.AreEqual("Output parameter 3", waterQualityModel.ObservationVariableOutputs[0].TimeSeriesList.ElementAt(2).Name);

            // Remove the first output parameter that should be shown in his
            waterQualityModel.SubstanceProcessLibrary.OutputParameters.Remove(waterQualityOutputParameter2);

            Assert.IsTrue(waterQualityModel.ObservationVariableOutputs.All(ovo => ovo.TimeSeriesList.Count() == 2)); // The time series should be removed from all observation variable outputs
            Assert.AreEqual("Substance", waterQualityModel.ObservationVariableOutputs[0].TimeSeriesList.ElementAt(0).Name);
            Assert.AreEqual("Output parameter 3", waterQualityModel.ObservationVariableOutputs[0].TimeSeriesList.ElementAt(1).Name);

            // Remove the output parameter that should not be shown in his
            waterQualityModel.SubstanceProcessLibrary.OutputParameters.Remove(waterQualityOutputParameter1);

            Assert.IsTrue(waterQualityModel.ObservationVariableOutputs.All(ovo => ovo.TimeSeriesList.Count() == 2)); // Nothing should happen + no exceptions should be thrown
            Assert.AreEqual("Substance", waterQualityModel.ObservationVariableOutputs[0].TimeSeriesList.ElementAt(0).Name);
            Assert.AreEqual("Output parameter 3", waterQualityModel.ObservationVariableOutputs[0].TimeSeriesList.ElementAt(1).Name);
        }

        [Test]
        public void TestModelMonitoringOutputSyncAfterChangeOfOutputParameterShowInHisForSubstanceCalculation()
        {
            var waterQualityModel = new WaterQualityModel {ModelSettings = {MonitoringOutputLevel = MonitoringOutputLevel.Points}};

            var waterQualityOutputParameter = new WaterQualityOutputParameter
            {
                Name = "Output parameter",
                ShowInHis = false
            };

            // Add an output parameter that should not be shown in his
            waterQualityModel.SubstanceProcessLibrary.OutputParameters.Add(waterQualityOutputParameter);

            // Manually add two observation variable outputs
            waterQualityModel.ObservationVariableOutputs.Add(new WaterQualityObservationVariableOutput(new Tuple<string, string>[]
                                                                                                           {}) {Name = "O1"});
            waterQualityModel.ObservationVariableOutputs.Add(new WaterQualityObservationVariableOutput(new Tuple<string, string>[]
                                                                                                           {}) {Name = "O2"});
            Assert.IsTrue(waterQualityModel.ObservationVariableOutputs.All(ovo => !ovo.TimeSeriesList.Any()));

            // Change the output parameter so that it should be shown in his
            waterQualityOutputParameter.ShowInHis = true;
            Assert.IsTrue(waterQualityModel.ObservationVariableOutputs.All(ovo => ovo.TimeSeriesList.Count() == 1)); // A time series should be added to all observation variable outputs

            // Change the output parameter so that it should no longer be shown in his
            waterQualityOutputParameter.ShowInHis = false;
            Assert.IsTrue(waterQualityModel.ObservationVariableOutputs.All(ovo => !ovo.TimeSeriesList.Any())); // The time series should be removed from all observation variable outputs
        }

        [Test]
        public void TestModelMonitoringOutputSyncAfterAddingAndRemovingObservationPointsForSubstanceCalculation()
        {
            var waterQualityModel = new WaterQualityModel {ModelSettings = {MonitoringOutputLevel = MonitoringOutputLevel.Points}};

            // Add a substance and two output parameters (of which one should be shown in his)
            waterQualityModel.SubstanceProcessLibrary.Substances.Add(new WaterQualitySubstance
            {
                Name = "Substance",
                ConcentrationUnit = ""
            });
            waterQualityModel.SubstanceProcessLibrary.OutputParameters.Add(new WaterQualityOutputParameter
            {
                Name = "Output parameter 1",
                ShowInHis = false
            });
            waterQualityModel.SubstanceProcessLibrary.OutputParameters.Add(new WaterQualityOutputParameter
            {
                Name = "Output parameter 2",
                ShowInHis = true
            });

            // Manually add a dummy observation variable output
            waterQualityModel.ObservationVariableOutputs.Add(new WaterQualityObservationVariableOutput(new Tuple<string, string>[]
                                                                                                           {}) {Name = "Dummy"});

            Assert.AreEqual(1, waterQualityModel.ObservationVariableOutputs.Count);

            // Add an observation point
            var observationPoint = new WaterQualityObservationPoint
            {
                Name = "O1",
                Geometry = new Point(new Coordinate(0, 5))
            };

            waterQualityModel.ObservationPoints.Add(observationPoint);

            Assert.AreEqual(2, waterQualityModel.ObservationVariableOutputs.Count); // An observation variable output item should be added which contains two time series
            Assert.AreEqual("O1", waterQualityModel.ObservationVariableOutputs[0].Name);
            Assert.IsTrue(waterQualityModel.ObservationVariableOutputs[0].TimeSeriesList.Count() == 2);
            Assert.AreEqual("Substance", waterQualityModel.ObservationVariableOutputs[0].TimeSeriesList.ElementAt(0).Name);
            Assert.AreEqual("Output parameter 2", waterQualityModel.ObservationVariableOutputs[0].TimeSeriesList.ElementAt(1).Name);

            // Remove the observation point
            waterQualityModel.ObservationPoints.Remove(observationPoint);

            Assert.AreEqual(1, waterQualityModel.ObservationVariableOutputs.Count); // The corresponding observation variable output item should be removed
        }

        [Test]
        public void TestModelMonitoringOutputSyncAfterRenamingObservationPointsForSubstanceCalculation()
        {
            var waterQualityModel = new WaterQualityModel {ModelSettings = {MonitoringOutputLevel = MonitoringOutputLevel.Points}};

            Assert.AreEqual(0, waterQualityModel.ObservationVariableOutputs.Count);

            // Add an observation point
            var observationPoint = new WaterQualityObservationPoint
            {
                Name = "O1",
                Geometry = new Point(new Coordinate(0, 5))
            };

            waterQualityModel.ObservationPoints.Add(observationPoint);

            Assert.AreEqual(1, waterQualityModel.ObservationVariableOutputs.Count);
            Assert.AreEqual("O1", waterQualityModel.ObservationVariableOutputs[0].Name);

            // Rename the observation point
            observationPoint.Name = "O2";

            Assert.AreEqual(1, waterQualityModel.ObservationVariableOutputs.Count);
            Assert.AreEqual("O2", waterQualityModel.ObservationVariableOutputs[0].Name); // The name of the observation variable output item should be updated
        }

        [Test]
        public void TestUpdateSurfaceWaterTypeMonitoringOutputDataItemsForMonitoringOutputLevelAreas()
        {
            var waterQualityModel = new WaterQualityModel {ModelSettings = {MonitoringOutputLevel = MonitoringOutputLevel.Areas}};

            waterQualityModel.ObservationAreas.AddLabel("abc");

            // Set the monitoring output level to "Areas"
            waterQualityModel.ModelSettings.MonitoringOutputLevel = MonitoringOutputLevel.Areas;

            // Add a dummy surface water type observation variable output
            waterQualityModel.ObservationVariableOutputs.Add(new WaterQualityObservationVariableOutput(new Tuple<string, string>[]
                                                                                                           {}) {Name = "Surface water type 2"});

            Assert.AreEqual(2, waterQualityModel.ObservationVariableOutputs.Count);
            Assert.AreEqual("abc", waterQualityModel.ObservationVariableOutputs[0].Name);
            Assert.AreEqual("Surface water type 2", waterQualityModel.ObservationVariableOutputs[1].Name);

            // rename area
            waterQualityModel.ObservationAreas.BeginEdit("");
            waterQualityModel.ObservationAreas.Components[0].Attributes.Remove("abc");
            waterQualityModel.ObservationAreas.Components[0].Attributes.Add("abcd", "0");
            waterQualityModel.ObservationAreas.EndEdit();

            Assert.AreEqual(1, waterQualityModel.ObservationVariableOutputs.Count);
            Assert.AreEqual("abcd", waterQualityModel.ObservationVariableOutputs[0].Name);
        }

        [Test]
        public void TestUpdateSurfaceWaterTypeMonitoringOutputDataItemsForMonitoringOutputLevelPointsAndAreas()
        {
            var waterQualityModel = new WaterQualityModel {ModelSettings = {MonitoringOutputLevel = MonitoringOutputLevel.Points}};

            // Add an observation point
            var observationPoint = new WaterQualityObservationPoint
            {
                Name = "O1",
                Geometry = new Point(new Coordinate(0, 5))
            };

            waterQualityModel.ObservationPoints.Add(observationPoint);

            // Set the surface water type attribute of the branch
            waterQualityModel.ObservationAreas.AddLabel("Surface water type 1");

            // Set the monitoring output level to "PointsAndAreas"
            waterQualityModel.ModelSettings.MonitoringOutputLevel = MonitoringOutputLevel.PointsAndAreas;

            // Add a dummy surface water type observation variable output
            waterQualityModel.ObservationVariableOutputs.Add(new WaterQualityObservationVariableOutput(new Tuple<string, string>[]
                                                                                                           {}) {Name = "surface water type 2"});

            Assert.AreEqual(3, waterQualityModel.ObservationVariableOutputs.Count);
            Assert.AreEqual("O1", waterQualityModel.ObservationVariableOutputs[0].Name);
            Assert.AreEqual("surface water type 1", waterQualityModel.ObservationVariableOutputs[1].Name);
            Assert.AreEqual("surface water type 2", waterQualityModel.ObservationVariableOutputs[2].Name);

            // rename area
            waterQualityModel.ObservationAreas.BeginEdit("");
            waterQualityModel.ObservationAreas.Components[0].Attributes.Remove("surface water type 1");
            waterQualityModel.ObservationAreas.Components[0].Attributes.Add("surface water type 3", "0");
            waterQualityModel.ObservationAreas.EndEdit();

            Assert.AreEqual(2, waterQualityModel.ObservationVariableOutputs.Count);
            Assert.AreEqual("O1", waterQualityModel.ObservationVariableOutputs[0].Name);
            Assert.AreEqual("surface water type 3", waterQualityModel.ObservationVariableOutputs[1].Name);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GivenAWaterQualityModel_WhenASubstanceIsRemoved_ThenCoverageIsRemovedFromFunctionStoreAndGridIsNotSetToNull()
        {
            // Given
            var model = new WaterQualityModel();
            LazyMapFileFunctionStore functionStore = model.MapFileFunctionStore;
            var substance = new WaterQualitySubstance {Name = "Substance"};
            functionStore.Path = "not_empty";

            model.SubstanceProcessLibrary.Substances.Add(substance);

            // Pre-conditions
            IEventedList<IFunction> functions = functionStore.Functions;
            UnstructuredGridCellCoverage coverage = functions.OfType<UnstructuredGridCellCoverage>()
                                                             .Single();
            Assert.That(functions, Is.Not.Empty);

            // When
            model.SubstanceProcessLibrary.Substances.Remove(substance);

            // Then
            Assert.That(functionStore.Functions, Is.Empty,
                        "Function store should be empty after removing substance.");
            Assert.That(coverage.Grid, Is.Not.Null,
                        "Grid of the coverage should not be set null");
        }

        # endregion
    }
}