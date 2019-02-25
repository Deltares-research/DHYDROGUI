using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.FileWriters.Boundary;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.PhysicalParameters;
using GeoAPI.Extensions.Networks;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.Boundary
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class WaterFlowModel1DBoundaryFileWriterTest
    {
        /// <summary>
        /// GIVEN a wind function with a specified approximation scheme
        /// WHEN WriteFile is called
        /// THEN the correct wind function is written to file
        /// </summary>
        [TestCase(Flow1DInterpolationType.BlockTo,   Flow1DExtrapolationType.Constant, false, BoundaryRegion.TimeInterpolationStrings.BlockTo)]
        [TestCase(Flow1DInterpolationType.BlockTo,   Flow1DExtrapolationType.Constant, true,  BoundaryRegion.TimeInterpolationStrings.BlockTo)]
        [TestCase(Flow1DInterpolationType.BlockFrom, Flow1DExtrapolationType.Constant, false, BoundaryRegion.TimeInterpolationStrings.BlockFrom)]
        [TestCase(Flow1DInterpolationType.BlockFrom, Flow1DExtrapolationType.Constant, true,  BoundaryRegion.TimeInterpolationStrings.BlockFrom)]
        [TestCase(Flow1DInterpolationType.Linear,    Flow1DExtrapolationType.Constant, false, BoundaryRegion.TimeInterpolationStrings.Linear)]
        [TestCase(Flow1DInterpolationType.Linear,    Flow1DExtrapolationType.Constant, true,  BoundaryRegion.TimeInterpolationStrings.Linear)]
        [TestCase(Flow1DInterpolationType.Linear,    Flow1DExtrapolationType.Linear,   false, BoundaryRegion.TimeInterpolationStrings.LinearAndExtrapolate)]
        [TestCase(Flow1DInterpolationType.Linear,    Flow1DExtrapolationType.Linear,   true,  BoundaryRegion.TimeInterpolationStrings.LinearAndExtrapolate)]
        public void GivenAWindFunctionWithASpecifiedApproximationScheme_WhenWriteFileIsCalled_ThenTheCorrectWindFunctionIsWrittenToFile(Flow1DInterpolationType interpolationType,
                                                                                                                                        Flow1DExtrapolationType extrapolationType,
                                                                                                                                        bool isPeriodic,
                                                                                                                                        string expectedInterpolationValue)
        {
            // Given
            var windFunction = new WindFunction();
            windFunction.SetInterpolationType(interpolationType);
            windFunction.SetExtrapolationType(extrapolationType);
            windFunction.SetPeriodicity(isPeriodic);

            var model = GetModel(windFunction: windFunction);

            // When
            var categories = WhenWriteFileIsCalled(model);

            // Then
            // Verify the categories
            Assert.That(categories, Is.Not.Null, 
                        "Expected the read categories not to be null.");
            var windBoundaryCategories = categories.Where(e => e.Name == BoundaryRegion.BcBoundaryHeader).ToList();
            Assert.That(windBoundaryCategories.Count, Is.EqualTo(2),
                        "Expected two categories when writing away just a wind function.");

            // Verify the wind function components
            foreach (var cat in windBoundaryCategories)
            {
                AssertTimeSeriesFunction(cat, 
                                         FunctionAttributes.StandardFeatureNames.ModelWide, 
                                         BoundaryRegion.FunctionStrings.TimeSeries,
                                         expectedInterpolationValue,
                                         isPeriodic);
            }
        }

        /// <summary>
        /// GIVEN a meteo function with a specified approximation scheme
        /// WHEN WriteFile is called
        /// THEN the correct meteo function is written to file
        /// </summary>
        [TestCase(Flow1DInterpolationType.BlockTo,   Flow1DExtrapolationType.Constant, false, BoundaryRegion.TimeInterpolationStrings.BlockTo)]
        [TestCase(Flow1DInterpolationType.BlockTo,   Flow1DExtrapolationType.Constant, true,  BoundaryRegion.TimeInterpolationStrings.BlockTo)]
        [TestCase(Flow1DInterpolationType.BlockFrom, Flow1DExtrapolationType.Constant, false, BoundaryRegion.TimeInterpolationStrings.BlockFrom)]
        [TestCase(Flow1DInterpolationType.BlockFrom, Flow1DExtrapolationType.Constant, true,  BoundaryRegion.TimeInterpolationStrings.BlockFrom)]
        [TestCase(Flow1DInterpolationType.Linear,    Flow1DExtrapolationType.Constant, false, BoundaryRegion.TimeInterpolationStrings.Linear)]
        [TestCase(Flow1DInterpolationType.Linear,    Flow1DExtrapolationType.Constant, true,  BoundaryRegion.TimeInterpolationStrings.Linear)]
        [TestCase(Flow1DInterpolationType.Linear,    Flow1DExtrapolationType.Linear,   false, BoundaryRegion.TimeInterpolationStrings.LinearAndExtrapolate)]
        [TestCase(Flow1DInterpolationType.Linear,    Flow1DExtrapolationType.Linear,   true,  BoundaryRegion.TimeInterpolationStrings.LinearAndExtrapolate)]
        public void GivenAMeteoFunctionWithASpecifiedApproximationScheme_WhenWriteFileIsCalled_ThenTheCorrectMeteoFunctionIsWrittenToFile(Flow1DInterpolationType interpolationType,
                                                                                                                                          Flow1DExtrapolationType extrapolationType,
                                                                                                                                          bool isPeriodic,
                                                                                                                                          string expectedInterpolationValue)
        {
            // Given
            var meteoFunction = new MeteoFunction();
            meteoFunction.SetInterpolationType(interpolationType);
            meteoFunction.SetExtrapolationType(extrapolationType);
            meteoFunction.SetPeriodicity(isPeriodic);

            var model = GetModel(meteoFunction: meteoFunction);

            // When
            var categories = WhenWriteFileIsCalled(model);

            // Then
            // Verify categories
            Assert.That(categories, Is.Not.Null,
                        "Expected the read categories not to be null.");
            var meteoBoundaryCategories = categories.Where(e => e.Name == BoundaryRegion.BcBoundaryHeader).ToList();
            Assert.That(meteoBoundaryCategories.Count, Is.EqualTo(3), 
                        "Expected three categories when writing away just a meteo-function.");

            // Verify meteo function components
            foreach (var cat in meteoBoundaryCategories)
            {
                AssertTimeSeriesFunction(cat, FunctionAttributes.StandardFeatureNames.ModelWide, BoundaryRegion.FunctionStrings.TimeSeries, expectedInterpolationValue, isPeriodic);
            }
        }

        /// <summary>
        /// GIVEN a BoundaryCondition with a specified approximation scheme
        /// WHEN WriteFile is called
        /// THEN the correct boundary condition is written to file
        /// </summary>
        [TestCase(WaterFlowModel1DBoundaryNodeDataType.FlowWaterLevelTable, Flow1DInterpolationType.Linear,  Flow1DExtrapolationType.Constant, false, BoundaryRegion.FunctionStrings.QhTable, BoundaryRegion.TimeInterpolationStrings.Linear)]

        [TestCase(WaterFlowModel1DBoundaryNodeDataType.FlowTimeSeries, Flow1DInterpolationType.BlockTo,   Flow1DExtrapolationType.Constant, false, BoundaryRegion.FunctionStrings.TimeSeries, BoundaryRegion.TimeInterpolationStrings.BlockTo)]
        [TestCase(WaterFlowModel1DBoundaryNodeDataType.FlowTimeSeries, Flow1DInterpolationType.BlockTo,   Flow1DExtrapolationType.Constant, true,  BoundaryRegion.FunctionStrings.TimeSeries, BoundaryRegion.TimeInterpolationStrings.BlockTo)]
        [TestCase(WaterFlowModel1DBoundaryNodeDataType.FlowTimeSeries, Flow1DInterpolationType.BlockFrom, Flow1DExtrapolationType.Constant, false, BoundaryRegion.FunctionStrings.TimeSeries, BoundaryRegion.TimeInterpolationStrings.BlockFrom)]
        [TestCase(WaterFlowModel1DBoundaryNodeDataType.FlowTimeSeries, Flow1DInterpolationType.BlockFrom, Flow1DExtrapolationType.Constant, true,  BoundaryRegion.FunctionStrings.TimeSeries, BoundaryRegion.TimeInterpolationStrings.BlockFrom)]
        [TestCase(WaterFlowModel1DBoundaryNodeDataType.FlowTimeSeries, Flow1DInterpolationType.Linear,    Flow1DExtrapolationType.Constant, false, BoundaryRegion.FunctionStrings.TimeSeries, BoundaryRegion.TimeInterpolationStrings.Linear)]
        [TestCase(WaterFlowModel1DBoundaryNodeDataType.FlowTimeSeries, Flow1DInterpolationType.Linear,    Flow1DExtrapolationType.Constant, true,  BoundaryRegion.FunctionStrings.TimeSeries, BoundaryRegion.TimeInterpolationStrings.Linear)]
        [TestCase(WaterFlowModel1DBoundaryNodeDataType.FlowTimeSeries, Flow1DInterpolationType.Linear,    Flow1DExtrapolationType.Linear,   false, BoundaryRegion.FunctionStrings.TimeSeries, BoundaryRegion.TimeInterpolationStrings.LinearAndExtrapolate)]
        [TestCase(WaterFlowModel1DBoundaryNodeDataType.FlowTimeSeries, Flow1DInterpolationType.Linear,    Flow1DExtrapolationType.Linear,   true,  BoundaryRegion.FunctionStrings.TimeSeries, BoundaryRegion.TimeInterpolationStrings.LinearAndExtrapolate)]

        [TestCase(WaterFlowModel1DBoundaryNodeDataType.WaterLevelTimeSeries, Flow1DInterpolationType.BlockTo,   Flow1DExtrapolationType.Constant, false, BoundaryRegion.FunctionStrings.TimeSeries, BoundaryRegion.TimeInterpolationStrings.BlockTo)]
        [TestCase(WaterFlowModel1DBoundaryNodeDataType.WaterLevelTimeSeries, Flow1DInterpolationType.BlockTo,   Flow1DExtrapolationType.Constant, true,  BoundaryRegion.FunctionStrings.TimeSeries, BoundaryRegion.TimeInterpolationStrings.BlockTo)]
        [TestCase(WaterFlowModel1DBoundaryNodeDataType.WaterLevelTimeSeries, Flow1DInterpolationType.BlockFrom, Flow1DExtrapolationType.Constant, false, BoundaryRegion.FunctionStrings.TimeSeries, BoundaryRegion.TimeInterpolationStrings.BlockFrom)]
        [TestCase(WaterFlowModel1DBoundaryNodeDataType.WaterLevelTimeSeries, Flow1DInterpolationType.BlockFrom, Flow1DExtrapolationType.Constant, true,  BoundaryRegion.FunctionStrings.TimeSeries, BoundaryRegion.TimeInterpolationStrings.BlockFrom)]
        [TestCase(WaterFlowModel1DBoundaryNodeDataType.WaterLevelTimeSeries, Flow1DInterpolationType.Linear,    Flow1DExtrapolationType.Constant, false, BoundaryRegion.FunctionStrings.TimeSeries, BoundaryRegion.TimeInterpolationStrings.Linear)]
        [TestCase(WaterFlowModel1DBoundaryNodeDataType.WaterLevelTimeSeries, Flow1DInterpolationType.Linear,    Flow1DExtrapolationType.Constant, true,  BoundaryRegion.FunctionStrings.TimeSeries, BoundaryRegion.TimeInterpolationStrings.Linear)]
        [TestCase(WaterFlowModel1DBoundaryNodeDataType.WaterLevelTimeSeries, Flow1DInterpolationType.Linear,    Flow1DExtrapolationType.Linear,   false, BoundaryRegion.FunctionStrings.TimeSeries, BoundaryRegion.TimeInterpolationStrings.LinearAndExtrapolate)]
        [TestCase(WaterFlowModel1DBoundaryNodeDataType.WaterLevelTimeSeries, Flow1DInterpolationType.Linear,    Flow1DExtrapolationType.Linear,   true,  BoundaryRegion.FunctionStrings.TimeSeries, BoundaryRegion.TimeInterpolationStrings.LinearAndExtrapolate)]
        public void GivenABoundaryConditionWithASpecifiedApproximationScheme_WhenWriteFileIsCalled_ThenTheCorrectBoundaryConditionIsWrittenToFile(WaterFlowModel1DBoundaryNodeDataType nodeType,
                                                                                                                                                  Flow1DInterpolationType interpolationType,
                                                                                                                                                  Flow1DExtrapolationType extrapolationType,
                                                                                                                                                  bool isPeriodic,
                                                                                                                                                  string expectedFunctionString,
                                                                                                                                                  string expectedInterpolationValue)
        {
            // Given
            const string nodeName = "Node_Smode";

            var relevantBoundary = GetBoundaryNodeData(nodeName);
            relevantBoundary.DataType = nodeType; // In order to trigger the creation of the function, type is set first.
            
            relevantBoundary.Data.SetInterpolationType(interpolationType);
            relevantBoundary.Data.SetExtrapolationType(extrapolationType);
            relevantBoundary.Data.SetPeriodicity(isPeriodic);

            var model = GetModel(boundaryNode: relevantBoundary);

            // When
            var categories = WhenWriteFileIsCalled(model);

            // Then
            // Verify the categories
            Assert.That(categories, Is.Not.Null, 
                        "Expected the read categories not to be null.");
            var boundaryCategories = categories.Where(e => e.Name == BoundaryRegion.BcBoundaryHeader).ToList();
            Assert.That(boundaryCategories.Count, Is.EqualTo(1), 
                        "Expected one category when writing away just a boundary condition.");

            // Verify the BoundaryCondition
            var cat = boundaryCategories.First();
            AssertTimeSeriesFunction(cat, nodeName, expectedFunctionString, expectedInterpolationValue, isPeriodic);
        }

        /// <summary>
        /// GIVEN a none BoundaryCondition
        /// WHEN WriteFile is called
        /// THEN no data is written to file
        /// </summary>
        [Test]
        public void GivenANoneBoundaryCondition_WhenWriteFileIsCalled_ThenNoDataIsWrittenToFile()
        {
            // Given
            var relevantBoundary = new WaterFlowModel1DBoundaryNodeData();
            relevantBoundary.DataType = WaterFlowModel1DBoundaryNodeDataType.None;

            var model = GetModel(boundaryNode: relevantBoundary);

            // When
            var categories = WhenWriteFileIsCalled(model);

            // Then
            Assert.That(categories, Is.Not.Null, 
                        "Expected the read categories not to be null.");
            var boundaryCategories = categories.Where(e => e.Name == BoundaryRegion.BcBoundaryHeader).ToList();
            Assert.That(boundaryCategories.Count, Is.EqualTo(0), 
                        "Expected no categories when writing away a none boundary condition.");
        }

        /// <summary>
        /// GIVEN a constant BoundaryCondition
        /// WHEN WriteFile is called
        /// THEN the correct boundary condition is written to file
        /// </summary>
        [TestCase(WaterFlowModel1DBoundaryNodeDataType.FlowConstant)]
        [TestCase(WaterFlowModel1DBoundaryNodeDataType.WaterLevelConstant)]
        public void GivenAConstantBoundaryCondition_WhenWriteFileIsCalled_ThenTheCorrectBoundaryConditionIsWrittenToFile(WaterFlowModel1DBoundaryNodeDataType nodeType)
        {
            // Given
            const string nodeName = "Node_Smode";
            var relevantBoundary = GetBoundaryNodeData(nodeName);

            relevantBoundary.DataType = nodeType;

            var model = GetModel(boundaryNode: relevantBoundary);

            // When
            var categories = WhenWriteFileIsCalled(model);

            // Then
            // Verify the categories
            Assert.That(categories, Is.Not.Null, "Expected the read categories not to be null.");
            var boundaryCategories = categories.Where(e => e.Name == BoundaryRegion.BcBoundaryHeader).ToList();
            Assert.That(boundaryCategories.Count, Is.EqualTo(1), 
                        "Expected 1 category when writing a constant boundary condition.");

            // Verify the boundary conditions
            var cat = boundaryCategories.First();
            Assert.That(cat.GetPropertyValue(BoundaryRegion.Name.Key), Is.EqualTo(nodeName),
                        "Expected a different name.");
            Assert.That(cat.GetPropertyValue(BoundaryRegion.Function.Key), Is.EqualTo(BoundaryRegion.FunctionStrings.Constant),
                        "Expected a different function type.");
            Assert.That(cat.GetPropertyValue(BoundaryRegion.Interpolation.Key),
                Is.EqualTo(BoundaryRegion.TimeInterpolationStrings.LinearAndExtrapolate),
                "Expected a different time-interpolation");
        }

        /// <summary>
        /// GIVEN a LateralDischarge with a specified approximation scheme
        /// WHEN WriteFile is called
        /// THEN the correct LateralDischarge is written to file
        /// </summary>
        [TestCase(WaterFlowModel1DLateralDataType.FlowWaterLevelTable, Flow1DInterpolationType.Linear, Flow1DExtrapolationType.Constant, false, BoundaryRegion.FunctionStrings.QhTable, BoundaryRegion.TimeInterpolationStrings.Linear)]

        [TestCase(WaterFlowModel1DLateralDataType.FlowTimeSeries, Flow1DInterpolationType.BlockTo,   Flow1DExtrapolationType.Constant, false, BoundaryRegion.FunctionStrings.TimeSeries, BoundaryRegion.TimeInterpolationStrings.BlockTo)]
        [TestCase(WaterFlowModel1DLateralDataType.FlowTimeSeries, Flow1DInterpolationType.BlockTo,   Flow1DExtrapolationType.Constant, true,  BoundaryRegion.FunctionStrings.TimeSeries, BoundaryRegion.TimeInterpolationStrings.BlockTo)]
        [TestCase(WaterFlowModel1DLateralDataType.FlowTimeSeries, Flow1DInterpolationType.BlockFrom, Flow1DExtrapolationType.Constant, false, BoundaryRegion.FunctionStrings.TimeSeries, BoundaryRegion.TimeInterpolationStrings.BlockFrom)]
        [TestCase(WaterFlowModel1DLateralDataType.FlowTimeSeries, Flow1DInterpolationType.BlockFrom, Flow1DExtrapolationType.Constant, true,  BoundaryRegion.FunctionStrings.TimeSeries, BoundaryRegion.TimeInterpolationStrings.BlockFrom)]
        [TestCase(WaterFlowModel1DLateralDataType.FlowTimeSeries, Flow1DInterpolationType.Linear,    Flow1DExtrapolationType.Constant, false, BoundaryRegion.FunctionStrings.TimeSeries, BoundaryRegion.TimeInterpolationStrings.Linear)]
        [TestCase(WaterFlowModel1DLateralDataType.FlowTimeSeries, Flow1DInterpolationType.Linear,    Flow1DExtrapolationType.Constant, true,  BoundaryRegion.FunctionStrings.TimeSeries, BoundaryRegion.TimeInterpolationStrings.Linear)]
        [TestCase(WaterFlowModel1DLateralDataType.FlowTimeSeries, Flow1DInterpolationType.Linear,    Flow1DExtrapolationType.Linear,   false, BoundaryRegion.FunctionStrings.TimeSeries, BoundaryRegion.TimeInterpolationStrings.LinearAndExtrapolate)]
        [TestCase(WaterFlowModel1DLateralDataType.FlowTimeSeries, Flow1DInterpolationType.Linear,    Flow1DExtrapolationType.Linear,   true,  BoundaryRegion.FunctionStrings.TimeSeries, BoundaryRegion.TimeInterpolationStrings.LinearAndExtrapolate)]

        public void GivenALateralDischargeWithASpecifiedApproximationScheme_WhenWriteFileIsCalled_ThenTheCorrectLateralDischargeIsWrittenToFile(WaterFlowModel1DLateralDataType lateralType,
                                                                                                                                                Flow1DInterpolationType interpolationType,
                                                                                                                                                Flow1DExtrapolationType extrapolationType,
                                                                                                                                                bool isPeriodic,
                                                                                                                                                string expectedFunctionString,
                                                                                                                                                string expectedInterpolationValue)
        {
            // Given
            const string nodeName = "Literally_a_lateral";

            var relevantLateral = GetLateralSourceData(nodeName);
            relevantLateral.DataType = lateralType;

            relevantLateral.Data.SetInterpolationType(interpolationType);
            relevantLateral.Data.SetExtrapolationType(extrapolationType);
            relevantLateral.Data.SetPeriodicity(isPeriodic);

            var model = GetModel(lateralNode: relevantLateral);

            // When
            var categories = WhenWriteFileIsCalled(model);

            // Then
            // Verify the categories
            Assert.That(categories, Is.Not.Null, 
                        "Expected the read categories not to be null.");
            var lateralCategories = categories.Where(e => e.Name == BoundaryRegion.BcLateralHeader).ToList();
            Assert.That(lateralCategories.Count, Is.EqualTo(1), 
                        "Expected 1 category when writing a constant boundary condition.");

            // Verify the LateralDischarge
            var cat = lateralCategories.First();
            AssertTimeSeriesFunction(cat, nodeName, expectedFunctionString, expectedInterpolationValue, isPeriodic);
        }

        /// <summary>
        /// GIVEN a constant LateralDischarge
        /// WHEN WriteFile is called
        /// THEN the correct LateralDischarge is written to file
        /// </summary>
        [Test]
        public void GivenAConstantLateralDischarge_WhenWriteFileIsCalled_ThenTheCorrectLateralDischargeIsWrittenToFile()
        {
            // Given
            // Set up basic elements
            const string nodeName = "Literally_a_lateral";

            var relevantLateral = GetLateralSourceData(nodeName);
            relevantLateral.DataType = WaterFlowModel1DLateralDataType.FlowConstant;

            var model = GetModel(lateralNode:relevantLateral);

            // When
            var categories = WhenWriteFileIsCalled(model);

            // Then
            // Verify the categories
            Assert.That(categories, Is.Not.Null,
                        "Expected the read categories not to be null.");
            var lateralCategories = categories.Where(e => e.Name == BoundaryRegion.BcLateralHeader).ToList();
            Assert.That(lateralCategories.Count, Is.EqualTo(1), 
                        "Expected 1 category when writing a constant boundary condition.");

            // Verify the constant LateralDischarge
            var cat = lateralCategories.First();
            Assert.That(cat.GetPropertyValue(BoundaryRegion.Name.Key), Is.EqualTo(nodeName), 
                        "Expected a different name.");
            Assert.That(cat.GetPropertyValue(BoundaryRegion.Function.Key), Is.EqualTo(BoundaryRegion.FunctionStrings.Constant), 
                        "Expected a different function type.");
            Assert.That(cat.GetPropertyValue(BoundaryRegion.Interpolation.Key),
                Is.EqualTo(BoundaryRegion.TimeInterpolationStrings.LinearAndExtrapolate),
                "Expected a different time-interpolation");
        }

        /// <summary>
        /// Get a basic lateral source data with a mocked node with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>A basic <see cref="WaterFlowModel1DLateralSourceData"/>.</returns>
        private static WaterFlowModel1DLateralSourceData GetLateralSourceData(string name)
        {
            var node = MockRepository.GenerateMock<LateralSource>();
            node.Expect(n => n.Name).Return(name);

            return new WaterFlowModel1DLateralSourceData()
            {
                Feature = node
            };
        }

        /// <summary>
        /// Get a basic boundary node data with a mocked node with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>A basic <see cref="WaterFlowModel1DBoundaryNodeData"/>.</returns>
        private static WaterFlowModel1DBoundaryNodeData GetBoundaryNodeData(string name)
        {
            var branchList = MockRepository.GenerateStrictMock<IEventedList<IBranch>>();
            branchList.Expect(l => l.CollectionChanged += null).IgnoreArguments().Repeat.Any();
            var linksList = MockRepository.GenerateStrictMock<IEventedList<HydroLink>>();
            linksList.Expect(l => l.CollectionChanged += null).IgnoreArguments().Repeat.Any();

            var node = MockRepository.GenerateMock<IHydroNode, INotifyPropertyChange>();
            node.Expect(n => n.Name).Return(name);
            node.Expect(n => n.IncomingBranches).Return(branchList);
            node.Expect(n => n.OutgoingBranches).Return(branchList);
            node.Expect(n => n.Links).Return(linksList);

            return new WaterFlowModel1DBoundaryNodeData()
            {
                Feature = node
            };
        }

        /// <summary>
        /// Get a partially mocked WaterFlowModel1D with the specified optional parameters.
        /// </summary>
        /// <param name="windFunction">Optional wind function.</param>
        /// <param name="meteoFunction">Optional meteo function.</param>
        /// <param name="lateralNode">Optional lateral node.</param>
        /// <param name="boundaryNode">Optional boundary node.</param>
        /// <returns>Partially mocked WaterFlowModel1D. </returns>
        /// <remarks>
        /// If a parameter is not null this value will be returned with the
        /// corresponding parameter, else a default value will be used.
        /// </remarks>
        private static WaterFlowModel1D GetModel(WindFunction windFunction = null,
                                                 MeteoFunction meteoFunction = null,
                                                 WaterFlowModel1DLateralSourceData lateralNode = null,
                                                 WaterFlowModel1DBoundaryNodeData boundaryNode = null)
        {
            var boundaryConditions = new EventedList<WaterFlowModel1DBoundaryNodeData>();

            if (boundaryNode != null)
                boundaryConditions.Add(boundaryNode);

            var laterals = new EventedList<WaterFlowModel1DLateralSourceData>();

            if (lateralNode != null)
                laterals.Add(lateralNode);

            if (windFunction == null)
            {
                windFunction = new WindFunction();
                windFunction.Arguments.Clear(); // WindFunctions without arguments are skipped
            }

            if (meteoFunction == null)
            {
                meteoFunction = new MeteoFunction();
                meteoFunction.Arguments.Clear(); // MeteoFunctions without arguments are skipped
            }

            var model = MockRepository.GeneratePartialMock<WaterFlowModel1D>(); // PartialMock because of eventing
            model.Expect(m => m.UseSalt)
                 .Return(false);
            model.Expect(m => m.UseTemperature)
                 .Return(false);
            model.Expect(m => m.BoundaryConditions)
                 .Return(boundaryConditions);
            model.Expect(m => m.LateralSourceData)
                 .Return(laterals);
            model.Expect(m => m.StartTime)
                 .Return(DateTime.Today);
            model.Expect(m => m.Wind)
                 .Return(windFunction);
            model.Expect(m => m.MeteoData)
                 .Return(meteoFunction);

            return model;
        }

        /// <summary>
        /// When-operation for the preceding tests.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>
        /// A set of read <see cref="IDelftIniCategory"/> which were written to file.
        /// </returns>
        private static IList<IDelftBcCategory> WhenWriteFileIsCalled(WaterFlowModel1D model)
        {
            IList<IDelftBcCategory> categories = null;
            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                var path = Path.Combine(tempDir, "outputFile.bc");

                WaterFlowModel1DBoundaryFileWriter.WriteFile(path, model);

                var delftBcReader = new DelftBcReader();
                categories = delftBcReader.ReadDelftBcFile(path);
            });

            return categories;
        }

        /// <summary>
        /// Assert that the provided time series function corresponds with the provided parameters.
        /// </summary>
        /// <param name="cat">The <see cref="IDelftIniCategory"/> describing the function.</param>
        /// <param name="name">The name of the function.</param>
        /// <param name="expectedFunctionString">The expected function value within the category.</param>
        /// <param name="expectedInterpolationValue">The expected interpolation value within the category.</param>
        /// <param name="isPeriodic">Whether it is expected that the category describes a periodic function.</param>
        private static void AssertTimeSeriesFunction(IDelftIniCategory cat,
                                                     string name, 
                                                     string expectedFunctionString,
                                                     string expectedInterpolationValue,
                                                     bool isPeriodic)
        {
            Assert.That(cat.GetPropertyValue(BoundaryRegion.Name.Key),
                Is.EqualTo(name), "Expected a different name.");
            Assert.That(cat.GetPropertyValue(BoundaryRegion.Function.Key), 
                Is.EqualTo(expectedFunctionString), "Expected a different function string.");
            Assert.That(cat.GetPropertyValue(BoundaryRegion.Interpolation.Key), 
                Is.EqualTo(expectedInterpolationValue), "Expected a different interpolation value.");

            if (isPeriodic)
                Assert.That(cat.GetPropertyValue(BoundaryRegion.Periodic.Key), 
                    Is.EqualTo("1"), "Expected a '1' (true) for periodic.");
            else
                Assert.That(cat.GetPropertyValue(BoundaryRegion.Periodic.Key), 
                    Is.Null, "Expected no periodic key in written file.");
        }

        [Test]
        public void TestBoundaryConditionFileWriterGivesExpectedResults_MeteoData()
        {
            var model = new WaterFlowModel1D();
            var startTime = DateTime.Now;
            model.StartTime = startTime;
            
            var meteoDataArguments = new[] { startTime + TimeSpan.FromMinutes(BoundaryFileWriterTestHelper.MeteoDataTimeSeriesArgument1), startTime + TimeSpan.FromMinutes(BoundaryFileWriterTestHelper.MeteoDataTimeSeriesArgument2) };
            var airTemperatureComponents = new[] { BoundaryFileWriterTestHelper.MeteoDataAirTemperatureTimeSeriesComponent1, BoundaryFileWriterTestHelper.MeteoDataAirTemperatureTimeSeriesComponent2 };
            var humidityComponents = new[] { BoundaryFileWriterTestHelper.MeteoDataHumidityTimeSeriesComponent1, BoundaryFileWriterTestHelper.MeteoDataHumidityTimeSeriesComponent2 };
            var cloudinessComponents = new[] { BoundaryFileWriterTestHelper.MeteoDataCloudinessTimeSeriesComponent1, BoundaryFileWriterTestHelper.MeteoDataCloudinessTimeSeriesComponent2 };

            model.MeteoData.Clear();
            model.MeteoData.Arguments[0].SetValues(meteoDataArguments);
            model.MeteoData.AirTemperature.SetValues(airTemperatureComponents);
            model.MeteoData.RelativeHumidity.SetValues(humidityComponents);
            model.MeteoData.Cloudiness.SetValues(cloudinessComponents);

            WaterFlowModel1DBoundaryFileWriter.WriteFile(FileWriterTestHelper.ModelFileNames.BoundaryConditions, model);

            var delftBcReader = new DelftBcReader();
            var categories = delftBcReader.ReadDelftBcFile(FileWriterTestHelper.ModelFileNames.BoundaryConditions);
            Assert.AreEqual(1, categories.Count(g => g.Name == GeneralRegion.IniHeader));

            // Test MeteoData
            var boundaryCategories = categories.Where(c => c.Name == BoundaryRegion.BcBoundaryHeader).ToList();
            Assert.AreEqual(5, boundaryCategories.Count); // wind & meteodata

            // model_wide : meteo data (air temperature)
            Assert.AreEqual(3, boundaryCategories[2].Properties.Count);
            Assert.AreEqual(FunctionAttributes.StandardFeatureNames.ModelWide, boundaryCategories[0].Properties[0].Value);
            Assert.AreEqual(BoundaryRegion.FunctionStrings.TimeSeries, boundaryCategories[0].Properties[1].Value);
            Assert.AreEqual(BoundaryRegion.TimeInterpolationStrings.LinearAndExtrapolate, boundaryCategories[0].Properties[2].Value);

            // model_wide : meteo data (humidity)
            Assert.AreEqual(3, boundaryCategories[3].Properties.Count);
            Assert.AreEqual(FunctionAttributes.StandardFeatureNames.ModelWide, boundaryCategories[0].Properties[0].Value);
            Assert.AreEqual(BoundaryRegion.FunctionStrings.TimeSeries, boundaryCategories[0].Properties[1].Value);
            Assert.AreEqual(BoundaryRegion.TimeInterpolationStrings.LinearAndExtrapolate, boundaryCategories[0].Properties[2].Value);

            // model_wide : meteo data (cloudiness)
            Assert.AreEqual(3, boundaryCategories[4].Properties.Count);
            Assert.AreEqual(FunctionAttributes.StandardFeatureNames.ModelWide, boundaryCategories[0].Properties[0].Value);
            Assert.AreEqual(BoundaryRegion.FunctionStrings.TimeSeries, boundaryCategories[0].Properties[1].Value);
            Assert.AreEqual(BoundaryRegion.TimeInterpolationStrings.LinearAndExtrapolate, boundaryCategories[0].Properties[2].Value);
            
            // check number of quantities
            Assert.AreEqual(2, boundaryCategories[2].Table.Count); // Time, AirTemperature
            Assert.AreEqual(2, boundaryCategories[3].Table.Count); // Time, Humidity
            Assert.AreEqual(2, boundaryCategories[4].Table.Count); // Time, Cloudiness

            // Check Time values are written correctly
            Assert.AreEqual(BoundaryRegion.QuantityStrings.Time, boundaryCategories[2].Table[0].Quantity.Value);
            Assert.AreEqual(BoundaryRegion.UnitStrings.TimeMinutes + " " + startTime.ToString(BoundaryRegion.UnitStrings.TimeFormat), boundaryCategories[2].Table[0].Unit.Value);
            Assert.AreEqual(2, boundaryCategories[2].Table[0].Values.Count);
            Assert.AreEqual(BoundaryFileWriterTestHelper.MeteoDataTimeSeriesArgument1.ToString(CultureInfo.InvariantCulture), boundaryCategories[2].Table[0].Values[0]);
            Assert.AreEqual(BoundaryFileWriterTestHelper.MeteoDataTimeSeriesArgument2.ToString(CultureInfo.InvariantCulture), boundaryCategories[2].Table[0].Values[1]);

            // Check AirTemperature values are written correctly
            Assert.AreEqual(BoundaryRegion.QuantityStrings.MeteoDataAirTemperature, boundaryCategories[2].Table[1].Quantity.Value);
            Assert.AreEqual(BoundaryRegion.UnitStrings.MeteoDataAirTemperature, boundaryCategories[2].Table[1].Unit.Value);
            Assert.AreEqual(BoundaryFileWriterTestHelper.MeteoDataAirTemperatureTimeSeriesComponent1.ToString(CultureInfo.InvariantCulture), boundaryCategories[2].Table[1].Values[0]);
            Assert.AreEqual(BoundaryFileWriterTestHelper.MeteoDataAirTemperatureTimeSeriesComponent2.ToString(CultureInfo.InvariantCulture), boundaryCategories[2].Table[1].Values[1]);
            
            // Check Humidity values are written correctly
            Assert.AreEqual(BoundaryRegion.QuantityStrings.MeteoDataHumidity, boundaryCategories[3].Table[1].Quantity.Value);
            Assert.AreEqual(BoundaryRegion.UnitStrings.MeteoDataHumidity, boundaryCategories[3].Table[1].Unit.Value);
            Assert.AreEqual(BoundaryFileWriterTestHelper.MeteoDataHumidityTimeSeriesComponent1.ToString(CultureInfo.InvariantCulture), boundaryCategories[3].Table[1].Values[0]);
            Assert.AreEqual(BoundaryFileWriterTestHelper.MeteoDataHumidityTimeSeriesComponent2.ToString(CultureInfo.InvariantCulture), boundaryCategories[3].Table[1].Values[1]);

            // Check Cloudiness values are written correctly
            Assert.AreEqual(BoundaryRegion.QuantityStrings.MeteoDataCloudiness, boundaryCategories[4].Table[1].Quantity.Value);
            Assert.AreEqual(BoundaryRegion.UnitStrings.MeteoDataCloudiness, boundaryCategories[4].Table[1].Unit.Value);
            Assert.AreEqual(BoundaryFileWriterTestHelper.MeteoDataCloudinessTimeSeriesComponent1.ToString(CultureInfo.InvariantCulture), boundaryCategories[4].Table[1].Values[0]);
            Assert.AreEqual(BoundaryFileWriterTestHelper.MeteoDataCloudinessTimeSeriesComponent2.ToString(CultureInfo.InvariantCulture), boundaryCategories[4].Table[1].Values[1]);
        }

        [Test]
        public void TestBoundaryConditionFileWriterGivesExpectedResults_BoundaryNodes()
        {
            WaterFlowModel1D model = new WaterFlowModel1D();
            var boundaryNodeData = model.BoundaryConditions;
           
            var startTime = DateTime.Now;
            model.StartTime = startTime;

            var node3Arguments = new[] { startTime + TimeSpan.FromMinutes(BoundaryFileWriterTestHelper.NodeFlowTimeSeriesArgument1), startTime + TimeSpan.FromMinutes(BoundaryFileWriterTestHelper.NodeFlowTimeSeriesArgument2) };
            var node3Components = new[] { BoundaryFileWriterTestHelper.NodeFlowTimeSeriesComponent1, BoundaryFileWriterTestHelper.NodeFlowTimeSeriesComponent2 };
            var node4Arguments = new[] { startTime + TimeSpan.FromMinutes(BoundaryFileWriterTestHelper.NodeWaterLevelTimeSeriesArgument1), startTime + TimeSpan.FromMinutes(BoundaryFileWriterTestHelper.NodeWaterLevelTimeSeriesArgument2) };
            var node4Components = new[] { BoundaryFileWriterTestHelper.NodeWaterLevelTimeSeriesComponent1, BoundaryFileWriterTestHelper.NodeWaterLevelTimeSeriesComponent2 };
            var node5Arguments = new[] { BoundaryFileWriterTestHelper.NodeFlowWaterLevelArgument1, BoundaryFileWriterTestHelper.NodeFlowWaterLevelArgument2 };
            var node5Components = new[] { BoundaryFileWriterTestHelper.NodeFlowWaterLevelComponent1, BoundaryFileWriterTestHelper.NodeFlowWaterLevelComponent2 };

            var windArguments = new[] { startTime + TimeSpan.FromMinutes(BoundaryFileWriterTestHelper.WindTimeSeriesArgument1), startTime + TimeSpan.FromMinutes(BoundaryFileWriterTestHelper.WindTimeSeriesArgument2) };
            var windSpeedComponents = new[] { BoundaryFileWriterTestHelper.WindVelocityTimeSeriesComponent1, BoundaryFileWriterTestHelper.WindVelocityTimeSeriesComponent2 };
            var windDirectionComponents = new[] { BoundaryFileWriterTestHelper.WindDirectionTimeSeriesComponent1, BoundaryFileWriterTestHelper.WindDirectionTimeSeriesComponent2 };
            
            model.Wind.Clear();
            model.Wind.Arguments[0].SetValues(windArguments);
            model.Wind.Velocity.SetValues(windSpeedComponents);
            model.Wind.Direction.SetValues(windDirectionComponents);
            
            boundaryNodeData.Add(BoundaryFileWriterTestHelper.GetBoundaryNodeDataWithConstantType(BoundaryFileWriterTestHelper.NodeConstantFlowName, BoundaryFileWriterTestHelper.NodeConstantFlowType, BoundaryFileWriterTestHelper.NodeConstantFlowValue));
            boundaryNodeData.Add(BoundaryFileWriterTestHelper.GetBoundaryNodeDataWithConstantType(BoundaryFileWriterTestHelper.NodeConstantWaterLevelName, BoundaryFileWriterTestHelper.NodeConstantWaterLevelType, BoundaryFileWriterTestHelper.NodeConstantWaterLevelValue));
            boundaryNodeData.Add(BoundaryFileWriterTestHelper.GetBoundaryNodeDataWithTimeSeriesType(BoundaryFileWriterTestHelper.NodeFlowTimeSeriesName, BoundaryFileWriterTestHelper.NodeFlowTimeSeriesType, node3Arguments, node3Components));
            boundaryNodeData.Add(BoundaryFileWriterTestHelper.GetBoundaryNodeDataWithTimeSeriesType(BoundaryFileWriterTestHelper.NodeWaterLevelTimeSeriesName, BoundaryFileWriterTestHelper.NodeWaterLevelTimeSeriesType, node4Arguments, node4Components));
            boundaryNodeData.Add(BoundaryFileWriterTestHelper.GetBoundaryNodeDataWithFlowWaterLevelData(BoundaryFileWriterTestHelper.NodeFlowWaterLevelName, BoundaryFileWriterTestHelper.NodeFlowWaterLevelType, node5Arguments, node5Components));

            WaterFlowModel1DBoundaryFileWriter.WriteFile(FileWriterTestHelper.ModelFileNames.BoundaryConditions, model);

            var delftBcReader = new DelftBcReader();
            var categories = delftBcReader.ReadDelftBcFile(FileWriterTestHelper.ModelFileNames.BoundaryConditions);
            Assert.AreEqual(1, categories.Count(g => g.Name == GeneralRegion.IniHeader));

            // Test BoundaryNode Data
            var boundaryNodeCategories = categories.Where(c => c.Name == BoundaryRegion.BcBoundaryHeader).ToList();
            Assert.AreEqual(10, boundaryNodeCategories.Count); // 5 nodes + 2 wind + 3 meteodata

            // Node1: Constant Flow
            Assert.AreEqual(3, boundaryNodeCategories[0].Properties.Count);
            Assert.AreEqual(BoundaryFileWriterTestHelper.NodeConstantFlowName, boundaryNodeCategories[0].Properties[0].Value);
            Assert.AreEqual(BoundaryRegion.FunctionStrings.Constant, boundaryNodeCategories[0].Properties[1].Value);
            Assert.AreEqual(BoundaryRegion.TimeInterpolationStrings.LinearAndExtrapolate, boundaryNodeCategories[0].Properties[2].Value);
            Assert.AreEqual(1, boundaryNodeCategories[0].Table.Count);
            Assert.AreEqual(BoundaryRegion.QuantityStrings.WaterDischarge, boundaryNodeCategories[0].Table[0].Quantity.Value);
            Assert.AreEqual(BoundaryRegion.UnitStrings.WaterDischarge, boundaryNodeCategories[0].Table[0].Unit.Value);
            Assert.AreEqual(1, boundaryNodeCategories[0].Table[0].Values.Count);
            Assert.AreEqual(BoundaryFileWriterTestHelper.NodeConstantFlowValue.ToString(CultureInfo.InvariantCulture), boundaryNodeCategories[0].Table[0].Values[0]);

            // Node2: Constant WaterLevel
            Assert.AreEqual(3, boundaryNodeCategories[1].Properties.Count);
            Assert.AreEqual(BoundaryFileWriterTestHelper.NodeConstantWaterLevelName, boundaryNodeCategories[1].Properties[0].Value);
            Assert.AreEqual(BoundaryRegion.FunctionStrings.Constant, boundaryNodeCategories[1].Properties[1].Value);
            Assert.AreEqual(BoundaryRegion.TimeInterpolationStrings.LinearAndExtrapolate, boundaryNodeCategories[1].Properties[2].Value);
            Assert.AreEqual(1, boundaryNodeCategories[1].Table.Count);
            Assert.AreEqual(BoundaryRegion.QuantityStrings.WaterLevel, boundaryNodeCategories[1].Table[0].Quantity.Value);
            Assert.AreEqual(BoundaryRegion.UnitStrings.WaterLevel, boundaryNodeCategories[1].Table[0].Unit.Value);
            Assert.AreEqual(1, boundaryNodeCategories[1].Table[0].Values.Count);
            Assert.AreEqual(BoundaryFileWriterTestHelper.NodeConstantWaterLevelValue.ToString(CultureInfo.InvariantCulture), boundaryNodeCategories[1].Table[0].Values[0]);

            // Node3: Flow TimeSeries
            Assert.AreEqual(3, boundaryNodeCategories[2].Properties.Count);
            Assert.AreEqual(BoundaryFileWriterTestHelper.NodeFlowTimeSeriesName, boundaryNodeCategories[2].Properties[0].Value);
            Assert.AreEqual(BoundaryRegion.FunctionStrings.TimeSeries, boundaryNodeCategories[2].Properties[1].Value);
            Assert.AreEqual(BoundaryRegion.TimeInterpolationStrings.LinearAndExtrapolate, boundaryNodeCategories[2].Properties[2].Value);
            Assert.AreEqual(2, boundaryNodeCategories[2].Table.Count);
            Assert.AreEqual(BoundaryRegion.QuantityStrings.Time, boundaryNodeCategories[2].Table[0].Quantity.Value);
            Assert.AreEqual(BoundaryRegion.UnitStrings.TimeMinutes + " " + startTime.ToString(BoundaryRegion.UnitStrings.TimeFormat), boundaryNodeCategories[2].Table[0].Unit.Value);
            Assert.AreEqual(2, boundaryNodeCategories[2].Table[0].Values.Count);
            Assert.AreEqual(BoundaryFileWriterTestHelper.NodeFlowTimeSeriesArgument1.ToString(), boundaryNodeCategories[2].Table[0].Values[0]);
            Assert.AreEqual(BoundaryFileWriterTestHelper.NodeFlowTimeSeriesArgument2.ToString(), boundaryNodeCategories[2].Table[0].Values[1]);
            Assert.AreEqual(BoundaryRegion.QuantityStrings.WaterDischarge, boundaryNodeCategories[2].Table[1].Quantity.Value);
            Assert.AreEqual(BoundaryRegion.UnitStrings.WaterDischarge, boundaryNodeCategories[2].Table[1].Unit.Value);
            Assert.AreEqual(2, boundaryNodeCategories[2].Table[1].Values.Count);
            Assert.AreEqual(BoundaryFileWriterTestHelper.NodeFlowTimeSeriesComponent1.ToString(CultureInfo.InvariantCulture), boundaryNodeCategories[2].Table[1].Values[0]);
            Assert.AreEqual(BoundaryFileWriterTestHelper.NodeFlowTimeSeriesComponent2.ToString(CultureInfo.InvariantCulture), boundaryNodeCategories[2].Table[1].Values[1]);

            // Node4: WaterLevel TimeSeries
            Assert.AreEqual(3, boundaryNodeCategories[3].Properties.Count);
            Assert.AreEqual(BoundaryFileWriterTestHelper.NodeWaterLevelTimeSeriesName, boundaryNodeCategories[3].Properties[0].Value);
            Assert.AreEqual(BoundaryRegion.FunctionStrings.TimeSeries, boundaryNodeCategories[3].Properties[1].Value);
            Assert.AreEqual(BoundaryRegion.TimeInterpolationStrings.LinearAndExtrapolate, boundaryNodeCategories[3].Properties[2].Value);
            Assert.AreEqual(2, boundaryNodeCategories[3].Table.Count);
            Assert.AreEqual(BoundaryRegion.QuantityStrings.Time, boundaryNodeCategories[3].Table[0].Quantity.Value);
            Assert.AreEqual(BoundaryRegion.UnitStrings.TimeMinutes + " " + startTime.ToString(BoundaryRegion.UnitStrings.TimeFormat), boundaryNodeCategories[3].Table[0].Unit.Value);
            Assert.AreEqual(2, boundaryNodeCategories[3].Table[0].Values.Count);
            Assert.AreEqual(BoundaryFileWriterTestHelper.NodeWaterLevelTimeSeriesArgument1.ToString(), boundaryNodeCategories[3].Table[0].Values[0]);
            Assert.AreEqual(BoundaryFileWriterTestHelper.NodeWaterLevelTimeSeriesArgument2.ToString(), boundaryNodeCategories[3].Table[0].Values[1]);
            Assert.AreEqual(BoundaryRegion.QuantityStrings.WaterLevel, boundaryNodeCategories[3].Table[1].Quantity.Value);
            Assert.AreEqual(BoundaryRegion.UnitStrings.WaterLevel, boundaryNodeCategories[3].Table[1].Unit.Value);
            Assert.AreEqual(2, boundaryNodeCategories[3].Table[1].Values.Count);
            Assert.AreEqual(BoundaryFileWriterTestHelper.NodeWaterLevelTimeSeriesComponent1.ToString(CultureInfo.InvariantCulture), boundaryNodeCategories[3].Table[1].Values[0]);
            Assert.AreEqual(BoundaryFileWriterTestHelper.NodeWaterLevelTimeSeriesComponent2.ToString(CultureInfo.InvariantCulture), boundaryNodeCategories[3].Table[1].Values[1]);

            // Node5: Flow WaterLevel
            Assert.AreEqual(3, boundaryNodeCategories[4].Properties.Count);
            Assert.AreEqual(BoundaryFileWriterTestHelper.NodeFlowWaterLevelName, boundaryNodeCategories[4].Properties[0].Value);
            Assert.AreEqual(BoundaryRegion.FunctionStrings.QhTable, boundaryNodeCategories[4].Properties[1].Value);
            Assert.AreEqual(BoundaryRegion.TimeInterpolationStrings.LinearAndExtrapolate, boundaryNodeCategories[4].Properties[2].Value);
            Assert.AreEqual(2, boundaryNodeCategories[4].Table.Count);
            Assert.AreEqual(BoundaryRegion.QuantityStrings.WaterLevel, boundaryNodeCategories[4].Table[0].Quantity.Value);
            Assert.AreEqual(BoundaryRegion.UnitStrings.WaterLevel, boundaryNodeCategories[4].Table[0].Unit.Value);
            Assert.AreEqual(2, boundaryNodeCategories[4].Table[0].Values.Count);
            Assert.AreEqual(BoundaryFileWriterTestHelper.NodeFlowWaterLevelArgument1.ToString(CultureInfo.InvariantCulture), boundaryNodeCategories[4].Table[0].Values[0]);
            Assert.AreEqual(BoundaryFileWriterTestHelper.NodeFlowWaterLevelArgument2.ToString(CultureInfo.InvariantCulture), boundaryNodeCategories[4].Table[0].Values[1]);
            Assert.AreEqual(BoundaryRegion.QuantityStrings.WaterDischarge, boundaryNodeCategories[4].Table[1].Quantity.Value);
            Assert.AreEqual(BoundaryRegion.UnitStrings.WaterDischarge, boundaryNodeCategories[4].Table[1].Unit.Value);
            Assert.AreEqual(2, boundaryNodeCategories[4].Table[1].Values.Count);
            Assert.AreEqual(BoundaryFileWriterTestHelper.NodeFlowWaterLevelComponent1.ToString(CultureInfo.InvariantCulture), boundaryNodeCategories[4].Table[1].Values[0]);
            Assert.AreEqual(BoundaryFileWriterTestHelper.NodeFlowWaterLevelComponent2.ToString(CultureInfo.InvariantCulture), boundaryNodeCategories[4].Table[1].Values[1]);

            // model_wide : wind speed
            Assert.AreEqual(3, boundaryNodeCategories[5].Properties.Count);
            Assert.AreEqual(FunctionAttributes.StandardFeatureNames.ModelWide, boundaryNodeCategories[5].Properties[0].Value);
            Assert.AreEqual(BoundaryRegion.FunctionStrings.TimeSeries, boundaryNodeCategories[5].Properties[1].Value);
            Assert.AreEqual(BoundaryRegion.TimeInterpolationStrings.LinearAndExtrapolate, boundaryNodeCategories[5].Properties[2].Value);
            Assert.AreEqual(2, boundaryNodeCategories[5].Table.Count);
            Assert.AreEqual(BoundaryRegion.QuantityStrings.Time, boundaryNodeCategories[5].Table[0].Quantity.Value);
            Assert.AreEqual(BoundaryRegion.UnitStrings.TimeMinutes + " " + startTime.ToString(BoundaryRegion.UnitStrings.TimeFormat), boundaryNodeCategories[5].Table[0].Unit.Value); 
            Assert.AreEqual(2, boundaryNodeCategories[5].Table[0].Values.Count);
            Assert.AreEqual(BoundaryFileWriterTestHelper.WindTimeSeriesArgument1.ToString(CultureInfo.InvariantCulture), boundaryNodeCategories[5].Table[0].Values[0]);
            Assert.AreEqual(BoundaryFileWriterTestHelper.WindTimeSeriesArgument2.ToString(CultureInfo.InvariantCulture), boundaryNodeCategories[5].Table[0].Values[1]);
            Assert.AreEqual(BoundaryRegion.QuantityStrings.WindSpeed, boundaryNodeCategories[5].Table[1].Quantity.Value);
            Assert.AreEqual(BoundaryRegion.UnitStrings.WindSpeed, boundaryNodeCategories[5].Table[1].Unit.Value);
            Assert.AreEqual(2, boundaryNodeCategories[5].Table[1].Values.Count);
            Assert.AreEqual(BoundaryFileWriterTestHelper.WindVelocityTimeSeriesComponent1.ToString(CultureInfo.InvariantCulture), boundaryNodeCategories[5].Table[1].Values[0]);
            Assert.AreEqual(BoundaryFileWriterTestHelper.WindVelocityTimeSeriesComponent2.ToString(CultureInfo.InvariantCulture), boundaryNodeCategories[5].Table[1].Values[1]);
            
            // model_wide : wind direction
            Assert.AreEqual(3, boundaryNodeCategories[6].Properties.Count);
            Assert.AreEqual(FunctionAttributes.StandardFeatureNames.ModelWide, boundaryNodeCategories[6].Properties[0].Value);
            Assert.AreEqual(BoundaryRegion.FunctionStrings.TimeSeries, boundaryNodeCategories[6].Properties[1].Value);
            Assert.AreEqual(BoundaryRegion.TimeInterpolationStrings.LinearAndExtrapolate, boundaryNodeCategories[6].Properties[2].Value);
            Assert.AreEqual(2, boundaryNodeCategories[6].Table.Count);
            Assert.AreEqual(BoundaryRegion.QuantityStrings.Time, boundaryNodeCategories[6].Table[0].Quantity.Value);
            Assert.AreEqual(BoundaryRegion.UnitStrings.TimeMinutes + " " + startTime.ToString(BoundaryRegion.UnitStrings.TimeFormat), boundaryNodeCategories[6].Table[0].Unit.Value); 
            Assert.AreEqual(2, boundaryNodeCategories[6].Table[0].Values.Count);
            Assert.AreEqual(BoundaryFileWriterTestHelper.WindTimeSeriesArgument1.ToString(CultureInfo.InvariantCulture), boundaryNodeCategories[6].Table[0].Values[0]);
            Assert.AreEqual(BoundaryFileWriterTestHelper.WindTimeSeriesArgument2.ToString(CultureInfo.InvariantCulture), boundaryNodeCategories[6].Table[0].Values[1]);
            Assert.AreEqual(BoundaryRegion.QuantityStrings.WindDirection, boundaryNodeCategories[6].Table[1].Quantity.Value);
            Assert.AreEqual(BoundaryRegion.UnitStrings.WindDirection, boundaryNodeCategories[6].Table[1].Unit.Value);
            Assert.AreEqual(2, boundaryNodeCategories[6].Table[1].Values.Count);
            Assert.AreEqual(BoundaryFileWriterTestHelper.WindDirectionTimeSeriesComponent1.ToString(CultureInfo.InvariantCulture), boundaryNodeCategories[6].Table[1].Values[0]);
            Assert.AreEqual(BoundaryFileWriterTestHelper.WindDirectionTimeSeriesComponent2.ToString(CultureInfo.InvariantCulture), boundaryNodeCategories[6].Table[1].Values[1]);
        }

        [Test]
        public void TestBoundaryConditionFileWriterGivesExpectedResults_LateralDischarges()
        {
            WaterFlowModel1D model = new WaterFlowModel1D();
            var lateralSourceData = model.LateralSourceData;

            var startTime = DateTime.Now;
            model.StartTime = startTime;

            var lateral2Arguments = new[] { BoundaryFileWriterTestHelper.LateralFlowWaterLevelArgument1, BoundaryFileWriterTestHelper.LateralFlowWaterLevelArgument2 };
            var lateral2Components = new[] { BoundaryFileWriterTestHelper.LateralFlowWaterLevelComponent1, BoundaryFileWriterTestHelper.LateralFlowWaterLevelComponent2 };

            var lateral3Arguments = new[] { startTime + TimeSpan.FromMinutes(BoundaryFileWriterTestHelper.LateralFlowTimeSeriesArgument1), startTime + TimeSpan.FromMinutes(BoundaryFileWriterTestHelper.LateralFlowTimeSeriesArgument2) };
            var lateral3Components = new[] { BoundaryFileWriterTestHelper.LateralFlowTimeSeriesComponent1, BoundaryFileWriterTestHelper.LateralFlowTimeSeriesComponent2 };
 
            lateralSourceData.Add(BoundaryFileWriterTestHelper.GetLateralSourceDataWithFlowData(BoundaryFileWriterTestHelper.LateralConstantFlowName, BoundaryFileWriterTestHelper.LateralConstantFlowType, BoundaryFileWriterTestHelper.LateralConstantFlowValue));
            lateralSourceData.Add(BoundaryFileWriterTestHelper.GetLateralSourceDataWithFlowWaterLevelData(BoundaryFileWriterTestHelper.LateralFlowWaterLevelName, BoundaryFileWriterTestHelper.LateralFlowWaterLevelType, lateral2Arguments, lateral2Components));
            lateralSourceData.Add(BoundaryFileWriterTestHelper.GetLateralSourceDataWithFlowTimeSeriesData(BoundaryFileWriterTestHelper.LateralFlowTimeSeriesName, BoundaryFileWriterTestHelper.LateralFlowTimeSeriesType, lateral3Arguments, lateral3Components));

            WaterFlowModel1DBoundaryFileWriter.WriteFile(FileWriterTestHelper.ModelFileNames.BoundaryConditions, model);

            var delftBcReader = new DelftBcReader();
            var categories = delftBcReader.ReadDelftBcFile(FileWriterTestHelper.ModelFileNames.BoundaryConditions);
            Assert.AreEqual(1, categories.Count(g => g.Name == GeneralRegion.IniHeader));

            // Test LateralSource Data
            var lateralSourceCategories = categories.Where(c => c.Name == BoundaryRegion.BcLateralHeader).ToList();
            Assert.AreEqual(3, lateralSourceCategories.Count);

            // Lateral1: Constant Flow
            Assert.AreEqual(3, lateralSourceCategories[0].Properties.Count);
            Assert.AreEqual(BoundaryFileWriterTestHelper.LateralConstantFlowName, lateralSourceCategories[0].Properties[0].Value);
            Assert.AreEqual(BoundaryRegion.FunctionStrings.Constant, lateralSourceCategories[0].Properties[1].Value);
            Assert.AreEqual(BoundaryRegion.TimeInterpolationStrings.LinearAndExtrapolate, lateralSourceCategories[0].Properties[2].Value);
            Assert.AreEqual(1, lateralSourceCategories[0].Table.Count);
            Assert.AreEqual(BoundaryRegion.QuantityStrings.WaterDischarge, lateralSourceCategories[0].Table[0].Quantity.Value);
            Assert.AreEqual(BoundaryRegion.UnitStrings.WaterDischarge, lateralSourceCategories[0].Table[0].Unit.Value);
            Assert.AreEqual(1, lateralSourceCategories[0].Table[0].Values.Count);
            Assert.AreEqual(BoundaryFileWriterTestHelper.LateralConstantFlowValue.ToString(CultureInfo.InvariantCulture), lateralSourceCategories[0].Table[0].Values[0]);

            // Lateral2: Flow WaterLevel
            Assert.AreEqual(3, lateralSourceCategories[1].Properties.Count);
            Assert.AreEqual(BoundaryFileWriterTestHelper.LateralFlowWaterLevelName, lateralSourceCategories[1].Properties[0].Value);
            Assert.AreEqual(BoundaryRegion.FunctionStrings.QhTable, lateralSourceCategories[1].Properties[1].Value);
            Assert.AreEqual(BoundaryRegion.TimeInterpolationStrings.LinearAndExtrapolate, lateralSourceCategories[1].Properties[2].Value);
            Assert.AreEqual(2, lateralSourceCategories[1].Table.Count);
            Assert.AreEqual(BoundaryRegion.QuantityStrings.WaterLevel, lateralSourceCategories[1].Table[0].Quantity.Value);
            Assert.AreEqual(BoundaryRegion.UnitStrings.WaterLevel, lateralSourceCategories[1].Table[0].Unit.Value);
            Assert.AreEqual(2, lateralSourceCategories[1].Table[0].Values.Count);
            Assert.AreEqual(BoundaryFileWriterTestHelper.LateralFlowWaterLevelArgument1.ToString(CultureInfo.InvariantCulture), lateralSourceCategories[1].Table[0].Values[0]);
            Assert.AreEqual(BoundaryFileWriterTestHelper.LateralFlowWaterLevelArgument2.ToString(CultureInfo.InvariantCulture), lateralSourceCategories[1].Table[0].Values[1]);
            Assert.AreEqual(BoundaryRegion.QuantityStrings.WaterDischarge, lateralSourceCategories[1].Table[1].Quantity.Value);
            Assert.AreEqual(BoundaryRegion.UnitStrings.WaterDischarge, lateralSourceCategories[1].Table[1].Unit.Value);
            Assert.AreEqual(2, lateralSourceCategories[1].Table[1].Values.Count);
            Assert.AreEqual(BoundaryFileWriterTestHelper.LateralFlowWaterLevelComponent1.ToString(CultureInfo.InvariantCulture), lateralSourceCategories[1].Table[1].Values[0]);
            Assert.AreEqual(BoundaryFileWriterTestHelper.LateralFlowWaterLevelComponent2.ToString(CultureInfo.InvariantCulture), lateralSourceCategories[1].Table[1].Values[1]);

            // Lateral3: Flow TimeSeries
            Assert.AreEqual(3, lateralSourceCategories[2].Properties.Count);
            Assert.AreEqual(BoundaryFileWriterTestHelper.LateralFlowTimeSeriesName, lateralSourceCategories[2].Properties[0].Value);
            Assert.AreEqual(BoundaryRegion.FunctionStrings.TimeSeries, lateralSourceCategories[2].Properties[1].Value);
            Assert.AreEqual(BoundaryRegion.TimeInterpolationStrings.LinearAndExtrapolate, lateralSourceCategories[2].Properties[2].Value);
            Assert.AreEqual(2, lateralSourceCategories[2].Table.Count);
            Assert.AreEqual(BoundaryRegion.QuantityStrings.Time, lateralSourceCategories[2].Table[0].Quantity.Value);
            Assert.AreEqual(BoundaryRegion.UnitStrings.TimeMinutes + " " + startTime.ToString(BoundaryRegion.UnitStrings.TimeFormat), lateralSourceCategories[2].Table[0].Unit.Value);
            Assert.AreEqual(2, lateralSourceCategories[2].Table[0].Values.Count);
            Assert.AreEqual(BoundaryFileWriterTestHelper.LateralFlowTimeSeriesArgument1.ToString(), lateralSourceCategories[2].Table[0].Values[0]);
            Assert.AreEqual(BoundaryFileWriterTestHelper.LateralFlowTimeSeriesArgument2.ToString(), lateralSourceCategories[2].Table[0].Values[1]);
            Assert.AreEqual(BoundaryRegion.QuantityStrings.WaterDischarge, lateralSourceCategories[2].Table[1].Quantity.Value);
            Assert.AreEqual(BoundaryRegion.UnitStrings.WaterDischarge, lateralSourceCategories[2].Table[1].Unit.Value);
            Assert.AreEqual(2, lateralSourceCategories[2].Table[1].Values.Count);
            Assert.AreEqual(BoundaryFileWriterTestHelper.LateralFlowTimeSeriesComponent1.ToString(CultureInfo.InvariantCulture), lateralSourceCategories[2].Table[1].Values[0]);
            Assert.AreEqual(BoundaryFileWriterTestHelper.LateralFlowTimeSeriesComponent2.ToString(CultureInfo.InvariantCulture), lateralSourceCategories[2].Table[1].Values[1]);
        }

        [Test]
        public void TestBoundaryConditionFileWriterGivesExpectedResults_BoundaryNodesWithSalt()
        {
            WaterFlowModel1D model = new WaterFlowModel1D();
            model.UseSalt = true;
            var boundaryNodeData = model.BoundaryConditions;

            var startTime = DateTime.Now;
            model.StartTime = startTime;

            const double thCoeff = 27.13;
            const double saltConstant = 10.37;
            double[] saltTimeSeries = new double[4] { 11.13, 17.19, 23.27, 37.43 };

            var boundaryNode1 = BoundaryFileWriterTestHelper.GetBoundaryNodeDataWithConstantType("Node001", WaterFlowModel1DBoundaryNodeDataType.FlowConstant, 1.3);
            boundaryNode1.SaltConditionType = SaltBoundaryConditionType.Constant;
            boundaryNode1.SaltConcentrationConstant = saltConstant;
            boundaryNode1.ThatcherHarlemannCoefficient = thCoeff;

            var boundaryNode2 = BoundaryFileWriterTestHelper.GetBoundaryNodeDataWithConstantType("Node002", WaterFlowModel1DBoundaryNodeDataType.FlowConstant, 2.7);
            boundaryNode2.SaltConditionType = SaltBoundaryConditionType.TimeDependent;
            boundaryNode2.SaltConcentrationTimeSeries = new TimeSeries();
            boundaryNode2.ThatcherHarlemannCoefficient = thCoeff;
            
            var argument = new Variable<DateTime>();
            argument.Values.AddRange(new DateTime[]{startTime, startTime.AddHours(1), startTime.AddHours(2), startTime.AddHours(3)});
            boundaryNode2.SaltConcentrationTimeSeries.Arguments.Clear();
            boundaryNode2.SaltConcentrationTimeSeries.Arguments.Add(argument);

            var component = new Variable<double>();
            component.Values.AddRange(saltTimeSeries);
            boundaryNode2.SaltConcentrationTimeSeries.Components.Clear();
            boundaryNode2.SaltConcentrationTimeSeries.Components.Add(component);

            boundaryNodeData.AddRange(new List<WaterFlowModel1DBoundaryNodeData>(){boundaryNode1, boundaryNode2});
            WaterFlowModel1DBoundaryFileWriter.WriteFile(FileWriterTestHelper.ModelFileNames.BoundaryConditions, model);

            var delftBcReader = new DelftBcReader();
            var categories = delftBcReader.ReadDelftBcFile(FileWriterTestHelper.ModelFileNames.BoundaryConditions);
            Assert.AreEqual(1, categories.Count(g => g.Name == GeneralRegion.IniHeader));

            // Test Salt Data
            var boundaryNodeCategories = categories.Where(c => c.Name == BoundaryRegion.BcBoundaryHeader).ToList();
            Assert.AreEqual(9, boundaryNodeCategories.Count); // + model_wide wind & meteo data

            Assert.AreEqual(3, boundaryNodeCategories[2].Properties.Count);
            Assert.AreEqual("Node001", boundaryNodeCategories[2].Properties[0].Value);
            Assert.AreEqual(BoundaryRegion.FunctionStrings.Constant, boundaryNodeCategories[2].Properties[1].Value);
            Assert.AreEqual(BoundaryRegion.TimeInterpolationStrings.LinearAndExtrapolate, boundaryNodeCategories[2].Properties[2].Value);
            Assert.AreEqual(1, boundaryNodeCategories[2].Table.Count);
            Assert.AreEqual(BoundaryRegion.QuantityStrings.WaterSalinity, boundaryNodeCategories[2].Table[0].Quantity.Value);
            Assert.AreEqual(BoundaryRegion.UnitStrings.SaltPpt, boundaryNodeCategories[2].Table[0].Unit.Value);
            Assert.AreEqual(1, boundaryNodeCategories[2].Table[0].Values.Count);
            Assert.AreEqual(saltConstant.ToString(CultureInfo.InvariantCulture), boundaryNodeCategories[2].Table[0].Values[0]);

            Assert.AreEqual(3, boundaryNodeCategories[3].Properties.Count);
            Assert.AreEqual("Node002", boundaryNodeCategories[3].Properties[0].Value);
            Assert.AreEqual(BoundaryRegion.FunctionStrings.TimeSeries, boundaryNodeCategories[3].Properties[1].Value);
            Assert.AreEqual(BoundaryRegion.TimeInterpolationStrings.LinearAndExtrapolate, boundaryNodeCategories[3].Properties[2].Value);
            Assert.AreEqual(2, boundaryNodeCategories[3].Table.Count);
            Assert.AreEqual(BoundaryRegion.QuantityStrings.Time, boundaryNodeCategories[3].Table[0].Quantity.Value);
            Assert.AreEqual(BoundaryRegion.UnitStrings.TimeMinutes + " " + startTime.ToString(BoundaryRegion.UnitStrings.TimeFormat), boundaryNodeCategories[3].Table[0].Unit.Value);
            Assert.AreEqual(4, boundaryNodeCategories[3].Table[0].Values.Count);
            Assert.AreEqual("0", boundaryNodeCategories[3].Table[0].Values[0]);
            Assert.AreEqual("60", boundaryNodeCategories[3].Table[0].Values[1]);
            Assert.AreEqual("120", boundaryNodeCategories[3].Table[0].Values[2]);
            Assert.AreEqual("180", boundaryNodeCategories[3].Table[0].Values[3]);
            Assert.AreEqual(BoundaryRegion.QuantityStrings.WaterSalinity, boundaryNodeCategories[3].Table[1].Quantity.Value);
            Assert.AreEqual(BoundaryRegion.UnitStrings.SaltPpt, boundaryNodeCategories[3].Table[1].Unit.Value);
            Assert.AreEqual(4, boundaryNodeCategories[3].Table[1].Values.Count);
            Assert.AreEqual(saltTimeSeries[0].ToString(CultureInfo.InvariantCulture), boundaryNodeCategories[3].Table[1].Values[0]);
            Assert.AreEqual(saltTimeSeries[1].ToString(CultureInfo.InvariantCulture), boundaryNodeCategories[3].Table[1].Values[1]);
            Assert.AreEqual(saltTimeSeries[2].ToString(CultureInfo.InvariantCulture), boundaryNodeCategories[3].Table[1].Values[2]);
            Assert.AreEqual(saltTimeSeries[3].ToString(CultureInfo.InvariantCulture), boundaryNodeCategories[3].Table[1].Values[3]);
        }

        [Test]
        public void TestBoundaryConditionFileWriterGivesExpectedResults_LateralDischargesWithSalt()
        {
            WaterFlowModel1D model = new WaterFlowModel1D();
            model.UseSalt = true;
            var lateralSourceData = model.LateralSourceData;

            var startTime = DateTime.Now;
            model.StartTime = startTime;

            const double SALT_VALUE_PPT = 10.37;
            const double SALT_VALUE_MASS = 32.17;

            var lateralSource1 = BoundaryFileWriterTestHelper.GetLateralSourceDataWithFlowData("LateralSource001", WaterFlowModel1DLateralDataType.FlowConstant, 1.3);
            lateralSource1.SaltLateralDischargeType = SaltLateralDischargeType.ConcentrationConstant;
            lateralSource1.SaltConcentrationDischargeConstant = SALT_VALUE_PPT;

            var lateralSource2 = BoundaryFileWriterTestHelper.GetLateralSourceDataWithFlowData("LateralSource002", WaterFlowModel1DLateralDataType.FlowConstant, 2.7);
            lateralSource2.SaltLateralDischargeType = SaltLateralDischargeType.MassConstant;
            lateralSource2.SaltMassDischargeConstant = SALT_VALUE_MASS;

            var lateralSource3 = BoundaryFileWriterTestHelper.GetLateralSourceDataWithFlowData("LateralSource003", WaterFlowModel1DLateralDataType.FlowConstant, 3.9);
            lateralSource3.SaltLateralDischargeType = SaltLateralDischargeType.MassConstant;
            lateralSource3.SaltMassDischargeConstant = SALT_VALUE_MASS;
            lateralSource3.SaltLateralDischargeType = SaltLateralDischargeType.Default; // change to default
            
            lateralSourceData.AddRange(new List<WaterFlowModel1DLateralSourceData>() { lateralSource1, lateralSource2, lateralSource3 });
            WaterFlowModel1DBoundaryFileWriter.WriteFile(FileWriterTestHelper.ModelFileNames.BoundaryConditions, model);

            var delftBcReader = new DelftBcReader();
            var categories = delftBcReader.ReadDelftBcFile(FileWriterTestHelper.ModelFileNames.BoundaryConditions);
            Assert.AreEqual(1, categories.Count(g => g.Name == GeneralRegion.IniHeader));

            // Test Salt Data
            var lateralSourceCategories = categories.Where(c => c.Name == BoundaryRegion.BcLateralHeader).ToList();
            Assert.AreEqual(6, lateralSourceCategories.Count);

            Assert.AreEqual(3, lateralSourceCategories[3].Properties.Count);
            Assert.AreEqual("LateralSource001", lateralSourceCategories[3].Properties[0].Value);
            Assert.AreEqual(BoundaryRegion.FunctionStrings.Constant, lateralSourceCategories[3].Properties[1].Value);
            Assert.AreEqual(BoundaryRegion.TimeInterpolationStrings.LinearAndExtrapolate, lateralSourceCategories[3].Properties[2].Value);
            Assert.AreEqual(1, lateralSourceCategories[3].Table.Count);
            Assert.AreEqual(BoundaryRegion.QuantityStrings.WaterSalinity, lateralSourceCategories[3].Table[0].Quantity.Value);
            Assert.AreEqual(BoundaryRegion.UnitStrings.SaltPpt, lateralSourceCategories[3].Table[0].Unit.Value);
            Assert.AreEqual(1, lateralSourceCategories[3].Table[0].Values.Count);
            Assert.AreEqual(SALT_VALUE_PPT.ToString(CultureInfo.InvariantCulture), lateralSourceCategories[3].Table[0].Values[0]);

            Assert.AreEqual(3, lateralSourceCategories[4].Properties.Count);
            Assert.AreEqual("LateralSource002", lateralSourceCategories[4].Properties[0].Value);
            Assert.AreEqual(BoundaryRegion.FunctionStrings.Constant, lateralSourceCategories[4].Properties[1].Value);
            Assert.AreEqual(BoundaryRegion.TimeInterpolationStrings.LinearAndExtrapolate, lateralSourceCategories[4].Properties[2].Value);
            Assert.AreEqual(1, lateralSourceCategories[4].Table.Count);
            Assert.AreEqual(BoundaryRegion.QuantityStrings.WaterSalinity, lateralSourceCategories[4].Table[0].Quantity.Value);
            Assert.AreEqual(BoundaryRegion.UnitStrings.SaltMass, lateralSourceCategories[4].Table[0].Unit.Value);
            Assert.AreEqual(1, lateralSourceCategories[4].Table[0].Values.Count);
            Assert.AreEqual(SALT_VALUE_MASS.ToString(CultureInfo.InvariantCulture), lateralSourceCategories[4].Table[0].Values[0]);

            Assert.AreEqual(3, lateralSourceCategories[5].Properties.Count);
            Assert.AreEqual("LateralSource003", lateralSourceCategories[5].Properties[0].Value);
            Assert.AreEqual(BoundaryRegion.FunctionStrings.Constant, lateralSourceCategories[5].Properties[1].Value);
            Assert.AreEqual(BoundaryRegion.TimeInterpolationStrings.LinearAndExtrapolate, lateralSourceCategories[5].Properties[2].Value);
            Assert.AreEqual(1, lateralSourceCategories[5].Table.Count);
            Assert.AreEqual(BoundaryRegion.QuantityStrings.WaterSalinity, lateralSourceCategories[5].Table[0].Quantity.Value);
            Assert.AreEqual(BoundaryRegion.UnitStrings.SaltPpt, lateralSourceCategories[5].Table[0].Unit.Value);
            Assert.AreEqual(1, lateralSourceCategories[5].Table[0].Values.Count);
            Assert.AreEqual(WaterFlowModel1DLateralSourceData.DefaultSalinity.ToString(CultureInfo.InvariantCulture), lateralSourceCategories[5].Table[0].Values[0]);
        }

        [Test]
        public void TestBoundaryConditionFileWriterGivesExpectedResults_BoundaryNodesWithTemperature()
        {
            var model = new WaterFlowModel1D {UseTemperature = true};
            var boundaryNodeData = model.BoundaryConditions;

            var startTime = DateTime.Now;
            model.StartTime = startTime;

            const double temperatureConstant = 10.37;
            double[] temperatureTimeSeries = new double[4] { 11.13, 17.19, 23.27, 37.43 };

            var boundaryNode1 = BoundaryFileWriterTestHelper.GetBoundaryNodeDataWithConstantType("Node001", WaterFlowModel1DBoundaryNodeDataType.FlowConstant, 1.3);
            boundaryNode1.TemperatureConditionType = TemperatureBoundaryConditionType.Constant;
            boundaryNode1.TemperatureConstant = temperatureConstant;

            var boundaryNode2 = BoundaryFileWriterTestHelper.GetBoundaryNodeDataWithConstantType("Node002", WaterFlowModel1DBoundaryNodeDataType.FlowConstant, 2.7);
            boundaryNode2.TemperatureConditionType = TemperatureBoundaryConditionType.TimeDependent;
            boundaryNode2.TemperatureTimeSeries = new TimeSeries();

            var argument = new Variable<DateTime>();
            argument.Values.AddRange(new DateTime[] { startTime, startTime.AddHours(1), startTime.AddHours(2), startTime.AddHours(3) });
            boundaryNode2.TemperatureTimeSeries.Arguments.Clear();
            boundaryNode2.TemperatureTimeSeries.Arguments.Add(argument);

            var component = new Variable<double>();
            component.Values.AddRange(temperatureTimeSeries);
            boundaryNode2.TemperatureTimeSeries.Components.Clear();
            boundaryNode2.TemperatureTimeSeries.Components.Add(component);

            boundaryNodeData.AddRange(new List<WaterFlowModel1DBoundaryNodeData>() { boundaryNode1, boundaryNode2 });
            WaterFlowModel1DBoundaryFileWriter.WriteFile(FileWriterTestHelper.ModelFileNames.BoundaryConditions, model);
            
            var delftBcReader = new DelftBcReader();
            var categories = delftBcReader.ReadDelftBcFile(FileWriterTestHelper.ModelFileNames.BoundaryConditions);
            Assert.AreEqual(1, categories.Count(g => g.Name == GeneralRegion.IniHeader));

            // Test Temperature Data
            var boundaryNodeCategories = categories.Where(c => c.Name == BoundaryRegion.BcBoundaryHeader).ToList();
            Assert.AreEqual(9, boundaryNodeCategories.Count); // + model_wide wind & meteodata

            Assert.AreEqual(3, boundaryNodeCategories[2].Properties.Count);
            Assert.AreEqual("Node001", boundaryNodeCategories[2].Properties[0].Value);
            Assert.AreEqual(BoundaryRegion.FunctionStrings.Constant, boundaryNodeCategories[2].Properties[1].Value);
            Assert.AreEqual(BoundaryRegion.TimeInterpolationStrings.LinearAndExtrapolate, boundaryNodeCategories[2].Properties[2].Value);
            Assert.AreEqual(1, boundaryNodeCategories[2].Table.Count);
            Assert.AreEqual(BoundaryRegion.QuantityStrings.WaterTemperature, boundaryNodeCategories[2].Table[0].Quantity.Value);
            Assert.AreEqual(BoundaryRegion.UnitStrings.WaterTemperature, boundaryNodeCategories[2].Table[0].Unit.Value);
            Assert.AreEqual(1, boundaryNodeCategories[2].Table[0].Values.Count);
            Assert.AreEqual(temperatureConstant.ToString(CultureInfo.InvariantCulture), boundaryNodeCategories[2].Table[0].Values[0]);

            Assert.AreEqual(3, boundaryNodeCategories[3].Properties.Count);
            Assert.AreEqual("Node002", boundaryNodeCategories[3].Properties[0].Value);
            Assert.AreEqual(BoundaryRegion.FunctionStrings.TimeSeries, boundaryNodeCategories[3].Properties[1].Value);
            Assert.AreEqual(BoundaryRegion.TimeInterpolationStrings.LinearAndExtrapolate, boundaryNodeCategories[3].Properties[2].Value);
            Assert.AreEqual(2, boundaryNodeCategories[3].Table.Count);
            Assert.AreEqual(BoundaryRegion.QuantityStrings.Time, boundaryNodeCategories[3].Table[0].Quantity.Value);
            Assert.AreEqual(BoundaryRegion.UnitStrings.TimeMinutes + " " + startTime.ToString(BoundaryRegion.UnitStrings.TimeFormat), boundaryNodeCategories[3].Table[0].Unit.Value);
            Assert.AreEqual(4, boundaryNodeCategories[3].Table[0].Values.Count);
            Assert.AreEqual("0", boundaryNodeCategories[3].Table[0].Values[0]);
            Assert.AreEqual("60", boundaryNodeCategories[3].Table[0].Values[1]);
            Assert.AreEqual("120", boundaryNodeCategories[3].Table[0].Values[2]);
            Assert.AreEqual("180", boundaryNodeCategories[3].Table[0].Values[3]);
            Assert.AreEqual(BoundaryRegion.QuantityStrings.WaterTemperature, boundaryNodeCategories[3].Table[1].Quantity.Value);
            Assert.AreEqual(BoundaryRegion.UnitStrings.WaterTemperature, boundaryNodeCategories[3].Table[1].Unit.Value);
            Assert.AreEqual(4, boundaryNodeCategories[3].Table[1].Values.Count);
            Assert.AreEqual(temperatureTimeSeries[0].ToString(CultureInfo.InvariantCulture), boundaryNodeCategories[3].Table[1].Values[0]);
            Assert.AreEqual(temperatureTimeSeries[1].ToString(CultureInfo.InvariantCulture), boundaryNodeCategories[3].Table[1].Values[1]);
            Assert.AreEqual(temperatureTimeSeries[2].ToString(CultureInfo.InvariantCulture), boundaryNodeCategories[3].Table[1].Values[2]);
            Assert.AreEqual(temperatureTimeSeries[3].ToString(CultureInfo.InvariantCulture), boundaryNodeCategories[3].Table[1].Values[3]);
        }
        
        [Test]
        public void TestBoundaryConditionFileWriterGivesExpectedResults_LateralDischargesWithTemperature()
        {
            var model = new WaterFlowModel1D {UseTemperature = true};
            var lateralSourceData = model.LateralSourceData;

            var startTime = DateTime.Now;
            model.StartTime = startTime;

            const double temperatureConstant = 10.37;
            double[] temperatureTimeSeries = new double[4] { 11.13, 17.19, 23.27, 37.43 };

            var lateralSource1 = BoundaryFileWriterTestHelper.GetLateralSourceDataWithFlowData("LateralSource001", WaterFlowModel1DLateralDataType.FlowConstant, 1.3);
            lateralSource1.TemperatureLateralDischargeType = TemperatureLateralDischargeType.Constant;
            lateralSource1.TemperatureConstant = temperatureConstant;

            var lateralSource2 = BoundaryFileWriterTestHelper.GetLateralSourceDataWithFlowData("LateralSource002", WaterFlowModel1DLateralDataType.FlowConstant, 2.7);
            lateralSource2.TemperatureLateralDischargeType = TemperatureLateralDischargeType.TimeDependent;
            lateralSource2.TemperatureTimeSeries = new TimeSeries();

            var argument = new Variable<DateTime>();
            argument.Values.AddRange(new DateTime[] { startTime, startTime.AddHours(1), startTime.AddHours(2), startTime.AddHours(3) });
            lateralSource2.TemperatureTimeSeries.Arguments.Clear();
            lateralSource2.TemperatureTimeSeries.Arguments.Add(argument);

            var component = new Variable<double>();
            component.Values.AddRange(temperatureTimeSeries);
            lateralSource2.TemperatureTimeSeries.Components.Clear();
            lateralSource2.TemperatureTimeSeries.Components.Add(component);
            
            lateralSourceData.AddRange(new List<WaterFlowModel1DLateralSourceData>() { lateralSource1, lateralSource2 });
            WaterFlowModel1DBoundaryFileWriter.WriteFile(FileWriterTestHelper.ModelFileNames.BoundaryConditions, model);
            
            var delftBcReader = new DelftBcReader();
            var categories = delftBcReader.ReadDelftBcFile(FileWriterTestHelper.ModelFileNames.BoundaryConditions);
            Assert.AreEqual(1, categories.Count(g => g.Name == GeneralRegion.IniHeader));

            // Test Salt Data
            var lateralSourceCategories = categories.Where(c => c.Name == BoundaryRegion.BcLateralHeader).ToList();
            Assert.AreEqual(4, lateralSourceCategories.Count);

            Assert.AreEqual(3, lateralSourceCategories[2].Properties.Count);
            Assert.AreEqual("LateralSource001", lateralSourceCategories[2].Properties[0].Value);
            Assert.AreEqual(BoundaryRegion.FunctionStrings.Constant, lateralSourceCategories[2].Properties[1].Value);
            Assert.AreEqual(BoundaryRegion.TimeInterpolationStrings.LinearAndExtrapolate, lateralSourceCategories[2].Properties[2].Value);
            Assert.AreEqual(1, lateralSourceCategories[2].Table.Count);
            Assert.AreEqual(BoundaryRegion.QuantityStrings.WaterTemperature, lateralSourceCategories[2].Table[0].Quantity.Value);
            Assert.AreEqual(BoundaryRegion.UnitStrings.WaterTemperature, lateralSourceCategories[2].Table[0].Unit.Value);
            Assert.AreEqual(1, lateralSourceCategories[2].Table[0].Values.Count);
            Assert.AreEqual(temperatureConstant.ToString(CultureInfo.InvariantCulture), lateralSourceCategories[2].Table[0].Values[0]);

            Assert.AreEqual(3, lateralSourceCategories[3].Properties.Count);
            Assert.AreEqual("LateralSource002", lateralSourceCategories[3].Properties[0].Value);
            Assert.AreEqual(BoundaryRegion.FunctionStrings.TimeSeries, lateralSourceCategories[3].Properties[1].Value);
            Assert.AreEqual(BoundaryRegion.TimeInterpolationStrings.LinearAndExtrapolate, lateralSourceCategories[3].Properties[2].Value);
            Assert.AreEqual(2, lateralSourceCategories[3].Table.Count);
            Assert.AreEqual(BoundaryRegion.QuantityStrings.Time, lateralSourceCategories[3].Table[0].Quantity.Value);
            Assert.AreEqual(BoundaryRegion.UnitStrings.TimeMinutes + " " + startTime.ToString(BoundaryRegion.UnitStrings.TimeFormat), lateralSourceCategories[3].Table[0].Unit.Value);
            Assert.AreEqual(4, lateralSourceCategories[3].Table[0].Values.Count);
            Assert.AreEqual("0", lateralSourceCategories[3].Table[0].Values[0]);
            Assert.AreEqual("60", lateralSourceCategories[3].Table[0].Values[1]);
            Assert.AreEqual("120", lateralSourceCategories[3].Table[0].Values[2]);
            Assert.AreEqual("180", lateralSourceCategories[3].Table[0].Values[3]);
            Assert.AreEqual(BoundaryRegion.QuantityStrings.WaterTemperature, lateralSourceCategories[3].Table[1].Quantity.Value);
            Assert.AreEqual(BoundaryRegion.UnitStrings.WaterTemperature, lateralSourceCategories[3].Table[1].Unit.Value);
            Assert.AreEqual(4, lateralSourceCategories[3].Table[1].Values.Count);
            Assert.AreEqual(temperatureTimeSeries[0].ToString(CultureInfo.InvariantCulture), lateralSourceCategories[3].Table[1].Values[0]);
            Assert.AreEqual(temperatureTimeSeries[1].ToString(CultureInfo.InvariantCulture), lateralSourceCategories[3].Table[1].Values[1]);
            Assert.AreEqual(temperatureTimeSeries[2].ToString(CultureInfo.InvariantCulture), lateralSourceCategories[3].Table[1].Values[2]);
            Assert.AreEqual(temperatureTimeSeries[3].ToString(CultureInfo.InvariantCulture), lateralSourceCategories[3].Table[1].Values[3]);
        }
    }
}